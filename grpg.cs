using System.Collections;
using System.Collections.Generic;
using System.Linq;


// https://stackoverflow.com/questions/15622622/analogue-of-pythons-defaultdict
// TODO: Don't need it, make a dict counter class for resources and effects
public class DefaultDict<TKey, TValue> : Dictionary<TKey, TValue> where TValue : new()
{
    public new TValue this[TKey key]
    {
        get
        {
            TValue val;
            if (!TryGetValue(key, out val))
            {
                val = new TValue();
                Add(key, val);
            }
            return val;
        }
        set { base[key] = value; }
    }
}

public static class Dice
{
    private static System.Random _rnd = new System.Random();
    public static Roll(int numSides) => _rnd.Next(numSides) + 1;
}

[Flags]
public enum Connection : int
{
    CanSee = 1,
    CanMove = 2,
    HasCover = 4
};

public class Mission
{
    public Connection[,] Connections { get; private set; }
    public HashSet<Actor> Actors = new HashSet<Actor>();
    public List<Team> Teams = new List<Team>();
    public uint TurnNumber { get; private set; }
    public uint CurrentTeamIndex { get; private set; }

    public Team CurrentTeam { get { return Teams[CurrentTeamIndex]; } }
    public List<Actor> GetTeamMembers(Team team) => Actors.Where(a => a.Team == team);

    public Mission(uint numNodes)
    {
        Connections = new Connection[numNodes, numNodes];
        Teams.Add(Team.Human);
        Teams.Add(Team.AI);
    }

    public void EndTurn()
    {
        CurrentTeamIndex += 1;
        foreach (var actor in this.GetTeamMembers(CurrentTeam)) actor.NewTurn(); 
        if (CurrentTeamIndex < this.Teams.Count) return;
        CurrentTeamIndex = 0;
        TurnNumber += 1;
    }
}

public class CharacterStats
{
    public HashSet<Action> Actions = new HashSet<Action> {Action.Move, Action.Disintegrate};
    public IDictionary<Resource, uint> PerBattleResources = new DefaultDict<Resource, int>();
    public IDictionary<Resource, uint> PerTurnResources = new DefaultDict<Resource, int>();
}

public enum Resource
{
    PrimaryAction,  // We'll only use these 1st iteration
    SecondaryAction,
    MoveAction,
    Reaction,
    Item,
    Booster
}

public enum Effect
{
    Hasted,
    Stunned,
    Rooted
}

public enum Team { 
    Human, 
    AI 
}

public class Actor
{
    public string Name;
    public CharacterStats Stats;
    public Mission Mission;
    public Team Team;
    public uint Location;

    public IDictionary<Resource, int> Resources = new DefaultDict<Resource, int>();
    public IDictionary<Effect, int> Effects = new DefaultDict<Effect, int>();

    public Actor(Mission mission, string name, CharacterStats stats, Team team, int location)
    {
        Mission = mission;
        Name = name;
        Stats = stats;
        Team = team;
        Location = location;

        foreach (var item in Stats.PerTurnResources) Resources[item.Key] = item.Value;
        foreach (var item in Stats.PerBattleResources) Resources[item.Key] += item.Value;
    }

    public ISet<Action> GetAllActions()
    {
        return new HashSet<Action>(Stats.Actions);
    }

    public ISet<Action> GetAvailableActions()
    {
        return GetAllActions().Where(action => action.IsAvailable(Mission, this));
    }

    internal void NewTurn()
    {
        // Reset per-turn resources
        foreach (var item in Stats.PerTurnResources) Resources[item.Key] = item.Value;

        // Decrement or remove remaining turn count of effects.
        foreach (var effect in Effects.Keys)
        {
            if (Effects[effect] == 1) Effects.Remove(effect);
            else Effects[effect] -= 1;
        }
        
        // Apply remaining effects
        if (Effects.Contains(Effect.Hasted)) Resources[Resource.PrimaryAction] += 1;
        if (Effects.Contains(Effect.Stunned)) Resources[Resource.PrimaryAction] -= 1;
    }
}

public struct ActionTarget
{
    Actor Actor;
    int Location = -1;
}

public enum TargetType
{
    Actor, Location
}

public struct ActionResult
{
    Actor Actor;
    ActionTarget Target;
    Action Action;
    int DiceRoll;
    int Difficulty;
    bool IsSuccess;
}

public abstract class Action
{
    public static Action Move = new ActionMove();
    public static Action Disintegrate = new ActionDisintegrate();

    public readonly string Name;
    public readonly IDictionary<Resource, int> Cost;
    public readonly TargetType TargetType;

    public virtual bool IsAvailable(Mission mission, Actor actor)
    {
        // Might change this to return a reason for being unable to perform action.
        if (mission.CurrentTeam != actor.Team) return false;
        foreach (var item in Cost)
        {
            if (actor.Resources[item.Key] < item.Value) return false;
        }
        return true;
    }

    public bool IsValidTarget(Mission mission, Actor actor, ActionTarget target)
    {
        if (TargetType == TargetType.Actor && target.Actor is null) return false;
        if (TargetType == TargetType.Location && target.Location < 0) return false;
        return CheckTarget(mission, actor, target);
    }

    private virtual bool CheckTarget(Mission mission, Actor actor, ActionTarget target)
    {
        return true;
    }
    
    public abstract int GetSuccessChance(Mission battle, Actor actor, ActionTarget target);
    public abstract void Perform(Mission battle, Actor actor, ActionTarget target);
}

public class ActionMove : Action
{
    public readonly string Name = "Move";
    public readonly TargetType TargetType = TargetType.Location;
    public readonly IDictionary<Resource, int> Cost = new Dictionary<Resource, int> 
    {
        { Resource.PrimaryAction, 1}
    };

    public override bool IsAvailable(Mission mission, Actor actor)
    {
        if (actor.Effects[Effect.Rooted] > 0) return false;
        return base.IsAvailable(mission, actor);
    }

    private override bool CheckTarget(Mission mission, Actor actor, ActionTarget target)
    {
        return mission.Connections[actor.Location, target.Location].CanMove;
    }

    public override int GetSuccessChance(Mission mission, Actor actor, ActionTarget target)
    {
        if (!IsValidTarget(mission, actor, target))
            return -1;
        return 100;
    }

    public override void Perform(Mission mission, Actor actor, ActionTarget target)
    {
        if (GetSuccessChance(mission, actor, target) <= 0) return;
        actor.Location = target.Location;
    }
}

public class ActionDisintegrate : Action
{
    public readonly string Name = "Disintegrating Punch";
    public readonly TargetType TargetType = TargetType.Actor;
    public readonly IDictionary<Resource, int> Cost = new Dictionary<Resource, int> 
    {
        { Resource.PrimaryAction, 1}
    };

    private override bool CheckTarget(Mission mission, Actor actor, ActionTarget target)
    {
        return (target.Location == actor.Location);
    }

    public override int GetSuccessChance(Mission mission, Actor actor, ActionTarget target)
    {
        if (target.Actor is actor) return 99;
        return 50;
    }

    public override void Perform(Mission mission, Actor actor, ActionTarget target)
    {
        var chance = GetSuccessChance(mission, actor, target);
        var roll = Dice.Roll(100);
        if (roll <= chance)
        {
            mission.Actors.Remove(actor);
        }
    }
}


