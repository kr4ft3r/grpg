using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GRPG.GameLogic;

/// <summary>
/// GameState has all the data required required for saving and loading game state between missions.
/// </summary>
[Serializable]
public class GameState
{
    public string Version;
    public int MissionIndex;
    // Skills are state with which mission upgrade selection starts,
    // Upgrades are state of upgrades for that mission, these can be altered on each retry of the same mission
    // Addition of Skills and Upgrades is the final state when mission play starts,
    // and also the state of Skills for next mission upgrade selection
    // Available points are 3 for each mission upgrade phase, max upgrade of any skill is 3
    public int AvailablePoints;
    public Dictionary<string, int> SimoneSkills;
    public Dictionary<string, int> SimoneUpgrades;
    public Dictionary<string, int> AndreaSkills;
    public Dictionary<string, int> AndreaUpgrades;
    public Dictionary<string, int> RedSkills;
    public Dictionary<string, int> RedUpgrades;

    public GameState(string version)
    {
        Version = version;
    }
}

public class CharacterSkills
{
    public static readonly Dictionary<string, int> SimoneDefault = new Dictionary<string, int>() {
        {SkillLine.SimonePrimary, 0 },
        {SkillLine.SimoneSecondary, 1 }, // This will be overriden to 0 for first mission
    };
    public static readonly Dictionary<string, int> AndreaDefault = new Dictionary<string, int>() {
        {SkillLine.AndreaPrimary, 1 },  // This will be overriden to 0 for first mission
        {SkillLine.AndreaSecondary, 1 },
    };
    public static readonly Dictionary<string, int> RedDefault = new Dictionary<string, int>() {
        {SkillLine.RedPrimary, 1 },  // This will be overriden to 0 for first mission
        {SkillLine.RedSecondary, 1 }
    };

    public static List<GRPG.GameLogic.Action> DefaultActions()
    {
        return new List<GRPG.GameLogic.Action>() { GRPG.GameLogic.Action.Move };
    }

    public static List<GRPG.GameLogic.Action> SimoneMissionActions(Dictionary<string,int> skills)
    {
        List<GRPG.GameLogic.Action> actions = DefaultActions();
        //TODO if (skills[SkillLine.SimonePrimary] > 0)    actions.Add(RoaringStarActions.)
        //TODO if (skills[SkillLine.SimoneSecondary] > 0)    actions.Add(RoaringStarActions.)
        return actions;
    }
    public static List<GRPG.GameLogic.Action> AndreaMissionActions(Dictionary<string, int> skills)
    {
        List<GRPG.GameLogic.Action> actions = DefaultActions();
        if (skills[SkillLine.AndreaPrimary] > 0) actions.Add(RoaringStarActions.GetSabreSwingAction(skills[SkillLine.AndreaPrimary]));
        if (skills[SkillLine.AndreaSecondary] > 0) actions.Add(RoaringStarActions.GetTankAction(skills[SkillLine.AndreaSecondary]));
        return actions;
    }
    public static List<GRPG.GameLogic.Action> RedMissionActions(Dictionary<string, int> skills)
    {
        List<GRPG.GameLogic.Action> actions = DefaultActions();
        if (skills[SkillLine.RedPrimary] > 0) actions.Add(RoaringStarActions.GetSnipeAction(skills[SkillLine.RedPrimary]));
        if (skills[SkillLine.RedSecondary] > 0) actions.Add(RoaringStarActions.GetFleeAction(skills[SkillLine.RedSecondary]));
        return actions;
    }

    public static Dictionary<string,Dictionary<string,int>> GetSkillsWithUpgrades(GameState state)
    {
        Dictionary<string, int> simone = new Dictionary<string, int>();
        foreach (KeyValuePair<string, int> kv in state.SimoneSkills)
            simone[kv.Key] = kv.Value + state.SimoneUpgrades[kv.Key];
        Dictionary<string, int> andrea = new Dictionary<string, int>();
        foreach (KeyValuePair<string, int> kv in state.AndreaSkills)
            andrea[kv.Key] = kv.Value + state.AndreaUpgrades[kv.Key];
        Dictionary<string, int> red = new Dictionary<string, int>();
        foreach (KeyValuePair<string, int> kv in state.RedSkills)
            red[kv.Key] = kv.Value + state.RedUpgrades[kv.Key];

        return new Dictionary<string, Dictionary<string, int>>()
        {
            {"Simone",  simone},
            {"Andrea",  andrea},
            {"Red",  red}
        };
    }
}

public class SkillLine
{
    public static readonly int MaxUpgrade = 3;
    public static readonly string SimonePrimary = "Magic";
    public static readonly string SimoneSecondary = "Teleportation";
    public static readonly string AndreaPrimary = "Slash";
    public static readonly string AndreaSecondary = "Stance";
    public static readonly string RedPrimary = "Shoot";
    public static readonly string RedSecondary = "Leap";
}