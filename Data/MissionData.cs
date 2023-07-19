using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GRPG.GameLogic;

public class MissionData
{
    public static MissionData Instance { get; private set; }

    public string MissionName { get; private set; }
    public Dictionary<string, MissionActorBlueprint> MissionActorBlueprints { get; private set; }
    public SceneSequenceData SceneSequences;
    public Dictionary<string,Action> LocationActions { get; private set; }
    public MissionData(
        string missionName, 
        Dictionary<string, MissionActorBlueprint> missionActorBlueprints, 
        SceneSequenceData sequencesData,
        Dictionary<string, Action> locationActions
        )
    {
        MissionName = missionName;
        MissionActorBlueprints = missionActorBlueprints;
        SceneSequences = sequencesData;
        LocationActions = locationActions;
    }

    public static void SetInstance(MissionData missionData)
    {
        Instance = missionData;
    }
}
