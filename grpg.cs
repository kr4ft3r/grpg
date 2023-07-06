using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GRPG.GameLogic
{

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
        public static int Roll(int numSides) => _rnd.Next(numSides) + 1;
    }

    public class Connection
    {
        public bool CanSee;
        public bool CanMove;
        public bool HasCover;
    };

    public class Mission
    {
        public Connection[,] Connections { get; private set; }
        public HashSet<Actor> Actors = new HashSet<Actor>();
        public List<Team> Teams = new List<Team>();
        public int TurnNumber { get; private set; }
        public int CurrentTeamIndex { get; private set; }

        public Team CurrentTeam { get { return Teams[CurrentTeamIndex]; } }
        public IEnumerable<Actor> GetTeamMembers(Team team) => Actors.Where(a => a.Team == team);

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
        public HashSet<Action> Actions = new HashSet<Action> { Action.Move, Action.Disintegrate };
        public IDictionary<Resource, int> PerBattleResources = new DefaultDict<Resource, int>();
        public IDictionary<Resource, int> PerTurnResources = new DefaultDict<Resource, int>();
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

    public enum Team
    {
        Human,
        AI
    }

    public class Actor
    {
        public string Name;
        public CharacterStats Stats;
        public Mission Mission;
        public Team Team;
        public int Location;

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

        public IEnumerable<Action> GetAllActions()
        {
            return new HashSet<Action>(Stats.Actions);
        }

        public IEnumerable<Action> GetAvailableActions()
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
            if (Effects.Keys.Contains(Effect.Hasted)) Resources[Resource.PrimaryAction] += 1;
            if (Effects.Keys.Contains(Effect.Stunned)) Resources[Resource.PrimaryAction] -= 1;
        }
    }

    public class ActionTarget
    {
        public Actor Actor;
        public int Location = -1;

        public ActionTarget(Actor actor, int location)
        {
            Actor = actor;
            Location = location;
        }
    }

    public enum TargetType
    {
        Actor, Location
    }

    public struct ActionResult
    {
        public Actor Actor;
        public ActionTarget Target;
        public Action Action;
        public int DiceRoll;
        public int Difficulty;
        public bool IsSuccess;
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

        internal virtual bool CheckTarget(Mission mission, Actor actor, ActionTarget target)
        {
            return true;
        }

        public abstract int GetSuccessChance(Mission battle, Actor actor, ActionTarget target);
        public abstract void Perform(Mission battle, Actor actor, ActionTarget target);
    }

    public class ActionMove : Action
    {
        public new readonly string Name = "Move";
        public new readonly TargetType TargetType = TargetType.Location;
        public new readonly IDictionary<Resource, int> Cost = new Dictionary<Resource, int>
    {
        { Resource.PrimaryAction, 1}
    };

        public override bool IsAvailable(Mission mission, Actor actor)
        {
            if (actor.Effects[Effect.Rooted] > 0) return false;
            return base.IsAvailable(mission, actor);
        }

        internal override bool CheckTarget(Mission mission, Actor actor, ActionTarget target)
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
        public new readonly string Name = "Disintegrating Punch";
        public new readonly TargetType TargetType = TargetType.Actor;
        public new readonly IDictionary<Resource, int> Cost = new Dictionary<Resource, int>
    {
        { Resource.PrimaryAction, 1}
    };

        internal override bool CheckTarget(Mission mission, Actor actor, ActionTarget target)
        {
            return (target.Location == actor.Location);
        }

        public override int GetSuccessChance(Mission mission, Actor actor, ActionTarget target)
        {
            if (target.Actor.Equals(actor)) return 99;
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


    public static class Test
    {
        public static void Main(string[] args)
        {
            System.Console.WriteLine("Hi.");
        }
    }
}