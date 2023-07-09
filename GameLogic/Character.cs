using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GRPG.GameLogic
{
    public class Character
    {
        public string Name;
        public CharacterStats BaseStats = new CharacterStats();
        public CharacterStats Stats = new CharacterStats();
        public HashSet<Feat> Feats = new HashSet<Feat>();

        public Character(string name)
        {
            Name = name;
        }

        public void RecalculateStats()
        {
            Stats = new CharacterStats(BaseStats);
            foreach (var feat in Feats) feat.ModifyStats(Stats);
        }
    }

    public class CharacterStats
    {
        public int SkillMelee;
        public int SkillRanged;
        public int DefenseMelee;
        public int DefenseRanged;

        public List<Action> Actions = new List<Action> { Action.Move, Action.Punch };
        public CounterDict<Resource> PerBattleResources = new CounterDict<Resource>();
        public CounterDict<Resource> PerTurnResources = new CounterDict<Resource>
        {
            {Resource.PrimaryAction, 1},
            {Resource.MoveAction, 1},
        };

        public CharacterStats() {}

        public CharacterStats(CharacterStats stats)
        {
            SkillMelee = stats.SkillMelee;
            SkillRanged = stats.SkillRanged;
            DefenseMelee = stats.DefenseMelee;
            DefenseRanged = stats.DefenseRanged;
            Actions = stats.Actions.ToList();
            PerBattleResources = new CounterDict<Resource>(stats.PerBattleResources);
            PerTurnResources = new CounterDict<Resource>(stats.PerTurnResources);
        }
    }

    public abstract class Feat
    {
        public string Name { get; protected set; }
        public bool IsUpgraded { get; protected set; }
        
        internal abstract void ModifyStats(CharacterStats stats);
    }
}