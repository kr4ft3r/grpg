using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using GRPG.GameLogic;

public class SceneObjects
{
    public Dictionary<Actor,GameObject> ActorObjects { get; private set; }
    public Dictionary<GameObject,int> ActorGOLocationMap { get; private set; }
    public Dictionary<GameObject,int> ActorGOLocationSlotMap { get; private set; }
    public Dictionary<string,ActionPresentationData> ActionPresentations { get; private set; }
    public Dictionary<string,CharacterPresentation> CharacterPresentations { get; private set; }
    public GameObject CharacterPanel;
    public GameObject DialoguePanel;
    public readonly Sequencer sequencer;

    private float[] _randomSlotVals;// = new float[100];
    private float[] _slotVals;

    public SceneObjects(Sequencer sequencer, GameObject characterPanel, GameObject dialoguePanel)
    {
        ActorObjects = new Dictionary<Actor, GameObject>();
        ActorGOLocationMap = new Dictionary<GameObject, int>();
        ActorGOLocationSlotMap = new Dictionary<GameObject, int>();
        ActionPresentations = new Dictionary<string, ActionPresentationData>();
        CharacterPresentations = new Dictionary<string, CharacterPresentation>();
        this.sequencer = sequencer;
        sequencer.SceneObjects = this;
        CharacterPanel = characterPanel;
        DialoguePanel = dialoguePanel;

        //_randomSlotVals = _randomSlotVals.Select(s => Random.Range(0.0f, 1.0f)).ToArray();
        _slotVals = new float[10] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };//, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2.0f };
    }

    public void ApplyTestActionResult(ActionResult result)
    {
        // If it was location action record performer history and handle removal if needed
        GameObject[] locations = GameObject.FindGameObjectsWithTag("Location");
        foreach (GameObject loc in locations)
        {
            if (loc.GetComponent<SceneLocation>().LocationAction == null) continue;
            if (result.Actor.Location == loc.GetComponent<SceneLocation>().Index &&
                result.Action.Name == loc.GetComponent<SceneLocation>().LocationAction.Name)
            {
                loc.GetComponent<SceneLocation>().LocationActionPerformerHistory.Add(result.Actor);
                if (loc.GetComponent<SceneLocation>().ActionAvailability == ActionAvailability.OncePerCharacter ||
                    loc.GetComponent<SceneLocation>().ActionAvailability == ActionAvailability.Once)
                {
                    ActorManager.Instance.ActorUnequipAction(result.Actor, result.Action);
                }
            }
        }

        // Run animation
        sequencer.AddResult(result);
    }

    //TODO public void CreateActorGO

    public void AddActorGO(Actor actor, GameObject go)
    {
        ActorObjects.Add(actor, go);
        ActorGOLocationMap.Add(go, actor.Location);
        ActorGOLocationSlotMap.Add(go, GetFreeSlotOnLocation(actor.Location));
    }

    public GameObject GetActorGO(Actor actor)
    {
        return ActorObjects[actor];
    }

    public GameObject GetActorGOByUniqueName(string name)
    {
        return ActorObjects.Where(a => a.Key.Name == name).Select(a => a.Value).ToArray()[0]; //Select(a => { if(a.Key.Name == name) return a.Value}).ToArray()[0];
    }

    public void ActorGOArriveToLocation(GameObject go, int loc)
    {
        ActorGOLocationSlotMap[go] = GetFreeSlotOnLocation(loc); // must be called first
        ActorGOLocationMap[go] = loc;
    }

    public List<SceneLocation> GetAllLocations()
    {
        return GameObject.FindGameObjectsWithTag("Location").Select(l => l.GetComponent<SceneLocation>()).ToList();
    }

    public Vector3 GetLocationPositionByIndex(int index)
    {
        GameObject[] locations = GameObject.FindGameObjectsWithTag("Location");
        foreach(GameObject loc in locations)
        {
            if (loc.GetComponent<SceneLocation>().Index == index)
                return loc.transform.position;
        }
        return new Vector3();
    }

    public Vector3 GetLocationSlotPositionByIndex(int index)//, bool nextAvailable = false)
    {
        //TODO actual location slots
        Vector3 locationPos = GetLocationPositionByIndex(index);
        float rnd = _slotVals[GetFreeSlotOnLocation(index)];//GetAmountOfActorsOnLocation(index) + (nextAvailable ? 1 : 0)];
        //Debug.Log("~~~ LOCATION:" + index + " rnd:" + rnd);
        float R = 5.0f;
        float r = R * Mathf.Sqrt(rnd);
        float theta = rnd * 2 * Mathf.PI;
        return new Vector3(locationPos.x + r * Mathf.Cos(theta), locationPos.y, locationPos.z + r * Mathf.Sin(theta));
    }

    public int GetAmountOfActorsOnLocation(int index)
    {
        int result = 0;
        foreach(KeyValuePair<GameObject,int> kv in ActorGOLocationMap)
        {
            if (kv.Value == index) result++;
        }

        return result;
    }

    public int GetFreeSlotOnLocation(int index)
    { // I know this is shit but i changed many methods and left with this one, can't be arsed to refactor
        List<int> occupied = new List<int>();
        foreach (KeyValuePair<GameObject, int> kv in ActorGOLocationMap)
            if (kv.Value == index && ActorGOLocationSlotMap.ContainsKey(kv.Key)) //keys are missing at mission init
                occupied.Add(ActorGOLocationSlotMap[kv.Key]);
        for (int i = 0; i < _slotVals.Length; i++)
            if (!occupied.Contains(i)) return i;

        return 0;
    }

    public void ShowLocations(List<int> indexes)
    {
        GameObject[] locations = GameObject.FindGameObjectsWithTag("Location");
        foreach (GameObject loc in locations)
        {
            if (indexes.Contains(loc.GetComponent<SceneLocation>().Index))
                loc.GetComponentInChildren<MeshRenderer>().enabled = true;
        }
    }

    public void HideAllLocations()
    {
        GameObject[] locations = GameObject.FindGameObjectsWithTag("Location");
        foreach (GameObject loc in locations)
        {
            loc.GetComponentInChildren<MeshRenderer>().enabled = false;
        }
    }

    public void OnPostActorIsDamaged(Actor actor, Damage damage)
    {
        float delay = 0;
        Debug.Log("  ==DAMAGED Action:" + damage.Action.Name);
        Debug.Log("  ==DAMAGED IsNull:" + damage.Action.IsNull());
        if (!damage.Action.IsNull()) Debug.Log("  ==DAMAGED delay:" + ActionPresentations[damage.Action.Name].showDamageDelay);
        if (!damage.Action.IsNull()) delay = ActionPresentations[damage.Action.Name].showDamageDelay;
        GetActorGO(actor).GetComponent<SceneActor>().TakeDamage(damage, actor.Resources[Resource.HitPoints], delay);
    }

    public void OnActorWasKilled(Actor actor)
    {
        //GetActorGO(actor).transform.RotateAround(GetActorGO(actor).transform.position, Vector3.forward, 90);//TODO
        /*if (actor.Team == Team.Human)
        {
            sequencer.MissionVars.SetString("_dead_character", actor.Name);
            sequencer.MissionUI.SetSceneSequenceUIState("player_character_died");
        }*/
    }

    public bool CheckForLoseCondition()
    {
        List<Actor> playerActors = ActorObjects.Where(a => a.Key.Team == Team.Human).Select(a => a.Key).ToList();
        foreach (Actor actor in playerActors)
            if (GetActorGO(actor).GetComponent<SceneActor>().IsDying) //TODO HasDied
            {
                sequencer.MissionVars.SetString("_dead_character", actor.Name);
                sequencer.MissionUI.SetSceneSequenceUIState("player_character_died");
                return true;
            }
        return false;
    }

    public void RestartMission()//TODO move, temporary solution
    {
        GameMissions gameMissions = new GameMissions();
        MissionData.SetInstance(gameMissions.Missions[0]);//TODO move
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //
    // Rewrite what's below, was in wrong mindset
    //

    public void StartSceneSequence(string sequence)
    {
        StartSceneSequence(MissionData.Instance.SceneSequences.Sequences[sequence]);
    }

    public void StartSceneSequence(System.Action<MissionUI, Sequencer, SceneObjects, float> sequence)
    {
        sequencer.RunSceneSequence(sequence);
    }

    public void DisableLocationColliders(List<int> ignoreIndexes = null)
    {
        if (ignoreIndexes == null) ignoreIndexes = new List<int>();

        GameObject[] locations = GameObject.FindGameObjectsWithTag("Location");
        foreach (GameObject loc in locations)
        {
            if (ignoreIndexes.Contains(loc.GetComponent<SceneLocation>().Index)) loc.transform.GetComponentInChildren<SphereCollider>().enabled = true;
            else loc.transform.GetComponentInChildren<SphereCollider>().enabled = false;
        }
    }

    public void EnableLocationColliders(List<int> ignoreIndexes = null)
    {
        if (ignoreIndexes == null) ignoreIndexes = new List<int>();

        GameObject[] locations = GameObject.FindGameObjectsWithTag("Location");
        foreach (GameObject loc in locations)
        {
            if (ignoreIndexes.Contains(loc.GetComponent<SceneLocation>().Index)) 
                loc.transform.GetComponentInChildren<SphereCollider>().enabled = false;
            else loc.transform.GetComponentInChildren<SphereCollider>().enabled = true;
        }
    }

    public void DisableActorColliders(List<Actor> ignoreActors = null)
    {
        if (ignoreActors == null) ignoreActors = new List<Actor>();

        foreach (KeyValuePair<Actor,GameObject> kv in ActorObjects)
        {
            if (ignoreActors.Contains(kv.Key)) kv.Value.transform.GetComponentInChildren<BoxCollider>().enabled = true;
            else kv.Value.transform.GetComponentInChildren<BoxCollider>().enabled = false;
        }
    }

    public void EnableActorColliders(List<Actor> ignoreActors = null)
    {
        if (ignoreActors == null) ignoreActors = new List<Actor>();

        foreach (KeyValuePair<Actor, GameObject> kv in ActorObjects)
        {
            if (ignoreActors.Contains(kv.Key)) kv.Value.transform.GetComponentInChildren<BoxCollider>().enabled = false;
            else kv.Value.transform.GetComponentInChildren<BoxCollider>().enabled = true;
        }
    }
}
