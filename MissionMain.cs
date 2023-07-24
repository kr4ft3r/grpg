using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GRPG.GameLogic;

/// <summary>
/// Main loader for missions.
/// 
/// Mission scene needs:
/// - Main Camera with components MissionCamera and AudioListener
/// - Object called "Mission" with components MissionMain, MissionUI, and Sequencer, with all fields default for all components
/// - Scene lighting must be baked in Unity settings in order to work when loading scene
/// </summary>
public class MissionMain : MonoBehaviour
{
    /// <summary>
    /// Ordered by location Index
    /// </summary>
    public SceneLocation[] Locations { get; private set; }

    private Mission _mission;
    private Dictionary<string, Actor> _actors;

    void Start()
    {
        GameObject.Instantiate(Resources.Load<Canvas>("UI/MissionCanvas"));
        _mission = new Mission(LoadLocations());
        MissionUI missionUI = GameObject.Find("Mission").GetComponent<MissionUI>();
        Sequencer sequencer = GameObject.Find("Mission").GetComponent<Sequencer>();
        sequencer.MissionUI = missionUI;
        GameObject characterPanel = GameObject.Find("ActiveCharacterPanel");
        GameObject dialoguePanel = GameObject.Find("DialoguePanel");
        SceneObjects sceneObjects = new SceneObjects(sequencer, characterPanel, dialoguePanel);
        MissionResources missionResources = new MissionResources(_mission, sceneObjects, Locations);
        //GameActions.Init();
        _actors = missionResources.LoadActors(sceneObjects);
        ActorManager actorManager = new ActorManager(_actors);
        SceneSequenceData sequencesData = missionResources.LoadSceneSequences();

        new HumanPlayer(_mission, Team.Human, sceneObjects, missionUI);
        new ComputerPlayer(_mission, Team.AI, sceneObjects, missionUI);

        missionUI.Init(_mission, sceneObjects);

        _mission.AfterActionPerformed += missionUI.LogActionResult;
        _mission.AfterActionPerformed += sceneObjects.ApplyTestActionResult;

        _mission.PostActorIsDamaged += missionUI.LogActorDamaged;
        _mission.PostActorIsDamaged += sceneObjects.OnPostActorIsDamaged;

        _mission.AfterActorMoves += HandleLocationTriggers;
        _mission.ActorWasDowned += sceneObjects.OnActorWasKilled;

        actorManager.ActorGainedAction += missionUI.UpdateCharacterAction;
        actorManager.ActorLostAction += missionUI.UpdateCharacterAction;

        _mission.Start();

        ///sceneObjects.FixActorPositions();

        if (_mission.CurrentTeam == Team.Human) //TODO HC 
        {
            //HumanPlayer.Instance.InitMove();
        } else
        {
            ComputerPlayer.Instance.RunMoves();
        }

        MusicManager musicManager = GameObject.Find("Game").GetComponent<MusicManager>();
        musicManager.NextSong = musicManager.SongRaindropGentle;

        //StartCoroutine(TestMoves());
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        DrawDebug();
    }

    //TODO move all the trah from below to a better place

    string[,] LoadLocations()
    {
        GameObject[] scnEditorLocationGOs = GameObject.FindGameObjectsWithTag("Location");
        int numNodes = scnEditorLocationGOs.Length;
        string[,] graph = new string[numNodes, numNodes];

        var query = from loc in scnEditorLocationGOs // Get script components sorted by Index
                    orderby loc.GetComponent<SceneEditorLocation>().Index
                    select loc.GetComponent<SceneEditorLocation>();
        SceneEditorLocation[] editorLocations = query.ToArray<SceneEditorLocation>();
        Locations = editorLocations.Select(el => el.gameObject.AddComponent<SceneLocation>()).ToArray();
        for (int i = 0; i < Locations.Length; i++)
        {
            Locations[i].Index = editorLocations[i].Index;
            Locations[i].Connections = editorLocations[i].Connections;
            if (editorLocations[i].LocationAction != "")
            {
                if (MissionData.Instance.LocationActions.ContainsKey(editorLocations[i].LocationAction))
                {
                    Locations[i].LocationAction = MissionData.Instance.LocationActions[editorLocations[i].LocationAction];
                    Locations[i].ActionAvailability = editorLocations[i].ActionAvailability;
                    Locations[i].ActionDisabled = editorLocations[i].ActionDisabled;
                }
                else
                {
                    Debug.LogWarning("Mission data doesn't contain location action: " + editorLocations[i].LocationAction);
                }
            }
            
            //
            Destroy(Locations[i].GetComponent<SceneEditorLocation>());
        }

        for (int i = 0; i < numNodes; i++)
        {
            for (int y = 0; y < numNodes; y++)
            {
                graph[i,y] = Locations[i].Connections[y];
            }
        }

        return graph;
    }

    private void HandleLocationTriggers(Actor actor, int old, int location)
    {
        
        // Location actions usable by human player:

        // Arrived to location that gives action
        if (Locations[location].LocationAction != null && !Locations[location].ActionDisabled)
        {
            if (!actor.Stats.Actions.Contains(Locations[location].LocationAction))
            {
                bool actionAvailable = true;

                switch (Locations[location].ActionAvailability)
                {
                    case ActionAvailability.Once:
                        actionAvailable = !(Locations[location].LocationActionPerformerHistory.Count > 0);
                        break;
                    case ActionAvailability.OncePerCharacter:
                        actionAvailable = !(Locations[location].LocationActionPerformerHistory.Contains(actor));
                        break;
                }

                if (actionAvailable)
                {
                    ActorManager.Instance.ActorEquipAction(actor, Locations[location].LocationAction);
                }
            }
        }

        // Left a location that gives action
        if (Locations[old].LocationAction != null)
        {
            ActorManager.Instance.ActorUnequipAction(actor, Locations[old].LocationAction);
        }

    }


    void DrawDebug()
    {
        if (!Application.isEditor) return;

        for (int i = 0; i < _mission.NumLocations; i++)
        {
            for (int y = 0; y < _mission.NumLocations; y++)
            {
                Connection connection = _mission.Connections[i, y];
                
                if (connection.CanMove)
                {
                    
                    Color color = Color.yellow;
                    Debug.DrawLine(Locations[i].transform.position, Locations[y].transform.position, color);
                }
            }
        }
    }
}
