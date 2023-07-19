using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GRPG.GameLogic;

public class MissionResources
{
    private Mission _mission;
    private SceneLocation[] _locations;
    private SceneObjects _sceneObjects;
    public MissionResources(Mission mission, SceneObjects sceneObjects, SceneLocation[] locations)
    {
        _mission = mission;
        _locations = locations;
        _sceneObjects = sceneObjects;
    }

    public SceneSequenceData LoadSceneSequences()
    {
        return MissionData.Instance.SceneSequences;
    }

    public Dictionary<string, Actor> LoadActors(SceneObjects sceneObjects)
    {
        Dictionary<string, Actor> actors = new Dictionary<string, Actor>();

        //TODO mission data source, that shit needs to be per mission

        foreach(KeyValuePair<string, MissionActorBlueprint> kv in MissionData.Instance.MissionActorBlueprints)
        {
            string name = kv.Key;
            MissionActorBlueprint actorBlueprint = kv.Value;

            if (actorBlueprint.StartingLocationStack.Length > 0)
            {
                Dictionary<string, Actor> clones = MultiActorFactory(
                    actorBlueprint.StartingLocationStack.Length,
                    actorBlueprint.Name,
                    actorBlueprint.CharacterPresentation,
                    actorBlueprint.CharacterStats,
                    actorBlueprint.Team,
                    actorBlueprint.StartingLocationStack);
                foreach (string key in clones.Keys)
                {
                    actors[key] = clones[key];
                }
            } else
            {
                actors.Add(name, ActorFactory(
                    actorBlueprint.Name,
                    actorBlueprint.CharacterPresentation,
                    actorBlueprint.CharacterStats,
                    actorBlueprint.Team,
                    actorBlueprint.StartingLocation));
            }
        }

        return actors;
    }

    private Actor ActorFactory(string nameId, CharacterPresentation presentation, CharacterStats characterStats, Team team, int startingLocationIndex)
    {
        GRPG.GameLogic.Actor actor = _mission.CreateActor(
            nameId,
            characterStats,
            team,
            startingLocationIndex
            );
        Debug.Log("Loading " + actor.Name + " at " + actor.Location);
        GameObject actorGO = GameObject.Instantiate<GameObject>(
            Resources.Load<GameObject>("Prefabs/Actor"),
            _sceneObjects.GetLocationSlotPositionByIndex(_locations[actor.Location].GetComponent<SceneLocation>().Index), //EditorLocations[simona.Location].transform.position,
            Quaternion.identity);
        _sceneObjects.AddActorGO(actor, actorGO);

        _sceneObjects.CharacterPresentations.Add(actor.Name, presentation);
        actorGO.GetComponent<SceneActor>().SetPresentation(_sceneObjects.CharacterPresentations[actor.Name]);

        return actor;
    }

    private Dictionary<string,Actor> MultiActorFactory(int amount, string nameId, CharacterPresentation presentation, CharacterStats characterStats, Team team, int[] startingLocationIndex)
    {
        Dictionary<string, Actor> actors = new Dictionary<string, Actor>();

        for(int i = 0; i < amount; i++)
        {
            string uniqueName = nameId + " " + (i + 1);
            actors.Add(uniqueName, ActorFactory(uniqueName, presentation, characterStats, team, startingLocationIndex[i]));
        }

        return actors;
    }


}
