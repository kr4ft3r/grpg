using System.Collections;
using System.Collections.Generic;

using GRPG.Data;

/// <summary>
/// Character's logical representation within the mission system
/// </summary>
public class Actor
{
    /// <summary>
    /// Character's name
    /// </summary>
    public readonly string name;
    /// <summary>
    /// Starting stats for this character
    /// </summary>
    public readonly CharacterStats baseStats;

    private int _movementPointsSpent = 0;

    public Actor(string name, CharacterStats stats) //TODO pass Character identity instead
    {
        this.name = name;
        baseStats = stats;
    }

    /// <summary>
    /// Called at start of new round, before character's turn
    /// </summary>
    public void SetStartOfRound()
    {
        _movementPointsSpent = 0;
    }

    /// <summary>
    /// Current available movement points
    /// </summary>
    public int MovementPoints
    {
        get
        {
            return baseStats.MP - _movementPointsSpent; //TODO temporary modifiers
        }
    }

    public void ReduceMovementPoints(int amount)
    {
        if (amount > MovementPoints)
        {
            _movementPointsSpent = baseStats.MP; // Set to zero
            return;
        }
        _movementPointsSpent += amount;
    }
}
