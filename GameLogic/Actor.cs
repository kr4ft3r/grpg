using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GRPG.GameLogic
{
    public class CharacterStats
    {
        public List<Action> Actions = new List<Action> { Action.Move, Action.Punch };
        public CounterDict<Resource> PerBattleResources = new CounterDict<Resource>();
        public CounterDict<Resource> PerTurnResources = new CounterDict<Resource> {
            {Resource.PrimaryAction, 1},
            {Resource.MoveAction, 1},
        };
    }

    public class Actor
    {
        protected int _location = -1;
        public string Name;
        public CharacterStats Stats;
        public Mission Mission;
        public Team Team;
        public ActorStatus Status = ActorStatus.Active;
        public int Location {
            get { return _location; }
            set {
                int old = _location;
                _location = value;
                if (Mission.IsActive) Mission.AfterActorMoves(this, old, value);
            }
        }

        public CounterDict<Resource> Resources = new CounterDict<Resource>();
        public CounterDict<Effect> Effects = new CounterDict<Effect>();

        public Actor(Mission mission, string name, CharacterStats stats, Team team, int location)
        {
            Mission = mission;
            Name = name;
            Stats = stats;
            Team = team;
            Location = location;
            Resources.SetAll(Stats.PerBattleResources);
        }

        public List<Action> GetAllActions()
        {
            return new List<Action>(Stats.Actions);
        }

        public List<Action> GetAvailableActions()
        {
            return GetAllActions().Where(action => action.GetActionValidity(this) == ActionValidity.Valid).ToList();
        }

        public Action GetAction(string name) => GetAllActions().First(action => action.Name == name);

        public List<ActionTarget> GetAvailableTargets(Action action)
        {
            List<ActionTarget> targets = Mission.Actors.Select(a => new ActionTarget(a)).Where(
                at => action.GetActionValidity(this, at) == ActionValidity.Valid
            ).ToList();
            for (int i = 0; i < Mission.NumLocations; i++)
            {
                ActionTarget target = new ActionTarget(i);
                if (action.GetActionValidity(this, target) == ActionValidity.Valid) targets.Add(target);
            }
            return targets;
        }

        internal void ApplyEffects()
        {
            if (Effects.Contains(Effect.Hasted)) Resources.Add(Resource.PrimaryAction, 1);
            if (Effects.Contains(Effect.Stunned)) Resources.Add(Resource.PrimaryAction, -1);
        }

        internal void NewTurn()
        {
            Resources.SetAll(Stats.PerTurnResources);
            ApplyEffects();
            Effects.DecrementAll();
        }
    }

}