using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GRPG.GameLogic;

public struct MissionActorBlueprint
{
    /// <summary>
    /// Constructor for heroes and monsters unique to the scene
    /// </summary>
    /// <param name="name"></param>
    /// <param name="hitPoints"></param>
    /// <param name="presentation"></param>
    /// <param name="stats"></param>
    /// <param name="team"></param>
    /// <param name="location"></param>
    public MissionActorBlueprint(string name, int hitPoints, CharacterPresentation presentation, CharacterStats stats, Team team, int location)
    {
        stats.PerBattleResources.Add(Resource.HitPoints, hitPoints );
        Name = name;
        CharacterPresentation = presentation;
        CharacterStats = stats;
        Team = team;
        StartingLocation = location;
        StartingLocationStack = new int[0];
    }

    /// <summary>
    /// Constructor for generic monsters and such, length of locations will determine amount to spawn
    /// </summary>
    /// <param name="name"></param>
    /// <param name="hitPoints"></param>
    /// <param name="presentation"></param>
    /// <param name="stats"></param>
    /// <param name="team"></param>
    /// <param name="locations"></param>
    public MissionActorBlueprint(string name, int hitPoints, CharacterPresentation presentation, CharacterStats stats, Team team, int[] locations)
    {
        stats.PerBattleResources.Add(Resource.HitPoints, hitPoints);
        Name = name;
        CharacterPresentation = presentation;
        CharacterStats = stats;
        Team = team;
        StartingLocation = -1;
        StartingLocationStack = locations;
    }

    public string Name;
    public CharacterPresentation CharacterPresentation;
    public CharacterStats CharacterStats;
    public Team Team;
    public int StartingLocation;
    public int[] StartingLocationStack;

}
