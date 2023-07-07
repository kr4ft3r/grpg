using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GRPG.GameLogic
{
    public class CharacterStats
    {
        public List<Action> Actions = new List<Action> { Action.Move, Action.Disintegrate };
        public CounterDict<Resource> PerBattleResources = new CounterDict<Resource>();
        public CounterDict<Resource> PerTurnResources = new CounterDict<Resource> {
            {Resource.PrimaryAction, 1},
            {Resource.MoveAction, 1},
        };
    }

    public class Actor
    {
        public string Name;
        public CharacterStats Stats;
        public Mission Mission;
        public Team Team;
        public int Location;

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
            return GetAllActions().Where(action => action.GetActionValidity(Mission, this) == ActionValidity.Valid).ToList();
        }

        public Action GetAction(string name) => GetAllActions().First(action => action.Name == name);

        internal void ApplyEffects()
        {
            if (Effects.Contains(Effect.Hasted)) Resources.Add(Resource.PrimaryAction, 1);
            if (Effects.Contains(Effect.Stunned)) Resources.Add(Resource.PrimaryAction, -1);
        }

        internal void NewTurn()
        {
            Resources.SetAll(Stats.PerTurnResources);
            Effects.DecrementAll();
            ApplyEffects();
        }
    }

}