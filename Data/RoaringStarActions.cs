using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using GRPG.GameLogic;

public class RoaringStarActions
{
    public static Action GetTankAction(int level)
    {
        if (level == 1) return new TankLevel1Action();

        return Action.Null;
    }

    public static Action GetSabreSwingAction(int level)
    {
        if (level == 1) return new SabreLevel1Action();

        return Action.Null;
    }

    public static Action GetFleeAction(int level)
    {
        if (level == 1) return new FleeLevel1Action();

        return Action.Null;
    }

    public static Action GetSnipeAction(int level)
    {
        if (level == 1) return new SnipeLevel1Action();

        return Action.Null;
    }

    public static Action GetNaturalBlink()
    {
        return new NaturalBlink();
    }

    //

    public static Action GetSearchSkeletonSceneAction()
    {
        return new SearchSkeleton();
    }

    // Monster

    public static Action GetSmackAction()
    {
        return new ActionSmack();
    }

    public static Action GetAcidLickAction()
    {
        return new AcidLick();
    }
}

/*
 * Warrior
 */

public class TankLevel1Action : Action
{
    private Actor _performer;
    public TankLevel1Action()
    {
        Name = "Warrior's Stance";
        Constraint = TargetConstraint.Actor | TargetConstraint.Self;
        Cost = new CounterDict<Resource>(Resource.PrimaryAction, 1);
    }

    protected override ActionResult DoPerform(Actor actor, ActionTarget target)
    {
        _performer = actor;
        actor.Effects.Add(Effect.Rooted, 1);

        actor.Mission.ActorIsDamaged += ReduceDamage;
        actor.PostActorNewTurn += WearOff;

        return new ActionResult(actor, this, target, true);
    }

    protected void ReduceDamage(Damage damage)
    {
        if (damage.Victim == _performer)
            damage.ScaleDamage(.4f);
    }

    protected void WearOff(Actor actor)
    {
        if (actor == _performer)
            actor.Mission.ActorIsDamaged -= ReduceDamage;
    }
}

public class SabreLevel1Action : Action
{
    public SabreLevel1Action()
    {
        Name = "Sabre Swing";
        Constraint = TargetConstraint.Actor | TargetConstraint.Enemy | TargetConstraint.OwnLocationOnly;
        Cost = new CounterDict<Resource>(Resource.PrimaryAction, 1);
    }

    protected override int CalcSuccessChance(Actor actor, ActionTarget target)
    {
        if (actor.Effects.Contains(Effect.SureHit)) return 100;
        return 75;
    }

    protected override ActionResult DoPerform(Actor actor, ActionTarget target)
    {
        var chance = GetSuccessChance(actor, target);
        var roll = Dice.Roll(100);
        var succ = roll <= chance;
        if (succ)
            target.Actor.TakeDamage(actor, this, Dice.Roll(8) + 4);

        if (actor.Mission.AfterActorAttacks != null) actor.Mission.AfterActorAttacks(actor, target.Actor, succ);
        return new ActionResult(actor, this, target, roll, chance, succ);
    }
}

/*
 * Rogue
 */

public class FleeLevel1Action : Action
{
    public FleeLevel1Action()
    {
        Name = "Flee";
        Constraint = TargetConstraint.Location | TargetConstraint.NeighbourOnly;
        Cost = new CounterDict<Resource>(Resource.PrimaryAction, 1);
    }
    protected override ActionResult DoPerform(Actor actor, ActionTarget target)
    {
        var chance = GetSuccessChance(actor, target);
        var roll = Dice.Roll(100);
        var succ = roll <= chance;
        if (succ)
        {
            actor.Location = target.Location;
        }

        return new ActionResult(actor, this, target, succ);
    }

    protected override int CalcSuccessChance(Actor actor, ActionTarget target)
    {
        Team enemyTeam = (actor.Team == Team.Human ? Team.AI : Team.Human);
        int enemiesHereCount = actor.Mission.GetTeamMembers(enemyTeam)
            .Where(a => a.Location == actor.Location)
            .Count();

        return System.Math.Clamp(100 - (enemiesHereCount * 10), 0, 100);
    }

    public override ActionValidity GetActionValidity(Actor actor)
    {
        if (actor.Effects[Effect.Rooted] > 0) return ActionValidity.PreventedByEffect;

        Team enemyTeam = (actor.Team == Team.Human ? Team.AI : Team.Human);
        IEnumerable<Actor> enemiesHere = actor.Mission.GetTeamMembers(enemyTeam)
            .Where(a => a.Location == actor.Location)
            .DefaultIfEmpty(null);
        if (enemiesHere.Count() == 0) return ActionValidity.NoEnemies;
        //Debug.Log("...... VALIDITYYYYY " + enemiesHere);

        return base.GetActionValidity(actor);
    }
}

public class SnipeLevel1Action : Action
{
    public SnipeLevel1Action()
    {
        Name = "Snipe";
        Constraint = TargetConstraint.Actor | TargetConstraint.Enemy;
        Cost = new CounterDict<Resource>(Resource.PrimaryAction, 1);
    }

    protected override int CalcSuccessChance(Actor actor, ActionTarget target)
    {
        if (actor.Effects.Contains(Effect.SureHit)) return 100;
        int chance = 60;
        if (actor.Location == target.Actor.Location) // Not good at close range
        {
            chance /= 2;
        }
        return chance;
    }

    protected override ActionResult DoPerform(Actor actor, ActionTarget target)
    {
        var chance = GetSuccessChance(actor, target);
        var roll = Dice.Roll(100);
        var succ = roll <= chance;
        if (succ)
            target.Actor.TakeDamage(actor, this, Dice.Roll(4) + 2);

        if (actor.Mission.AfterActorAttacks != null) actor.Mission.AfterActorAttacks(actor, target.Actor, succ);
        return new ActionResult(actor, this, target, roll, chance, succ);
    }
}

/*
 * Magician
 */

// Mission 1 scripted spell, teleport to location without enemies
public class NaturalBlink : Action
{
    public NaturalBlink()
    {
        Name = "Natural Blink";
        Constraint = TargetConstraint.Location | TargetConstraint.Passive;
        Cost = new CounterDict<Resource>(Resource.PrimaryAction, 1);
    }
    protected override int CalcSuccessChance(Actor actor, ActionTarget target)
    {
        return 100;
    }
    protected override ActionResult DoPerform(Actor actor, ActionTarget target)
    {
        Team enemyTeam = Team.AI;
        if (actor.Team == Team.AI) enemyTeam = Team.Human;
        List<int> validLocations = Enumerable.Range(0, actor.Mission.NumLocations).ToList();
        List<int> invalidLocations = actor.Mission.GetTeamMembers(enemyTeam).Select(e => e.Location).ToList();
        invalidLocations.Add(actor.Location);// not my location either
        validLocations = validLocations.Except(invalidLocations).ToList(); // diff
        int location = Random.Range(0, validLocations.Count);
        target.Location = location;
        actor.Location = target.Location;

        return new ActionResult(actor, this, target, true);
    }
}


/*
 * Mission Actions
 */

// Mission 1
public class SearchSkeleton : Action
{
    public SearchSkeleton()
    {
        Name = "Search Skeleton";
        Constraint = TargetConstraint.Location | TargetConstraint.OwnLocationOnly;
        Cost = new CounterDict<Resource>(Resource.PrimaryAction, 1);
    }

    protected override ActionResult DoPerform(Actor actor, ActionTarget target)
    {
        return new ActionResult(actor, this, new ActionTarget(actor.Location), true);
        //throw new System.NotImplementedException();
    }
}



/*
 * Monster actions
 */

public class ActionSmack : Action
{
    public ActionSmack()
    {
        Name = "Smack";
        Constraint = TargetConstraint.Actor | TargetConstraint.Enemy | TargetConstraint.OwnLocationOnly;
        Cost = new CounterDict<Resource>(Resource.PrimaryAction, 1);
    }

    protected override int CalcSuccessChance(Actor actor, ActionTarget target)
    {
        if (actor.Effects.Contains(Effect.SureHit)) return 100;
        return 65;
    }

    protected override ActionResult DoPerform(Actor actor, ActionTarget target)
    {
        //Hacky
        if (MissionData.Instance.MissionName == "Cave of Devourers" && target.Actor.Name == "Simone")
        {
            RoaringStarActions.GetNaturalBlink().Perform(target.Actor, new ActionTarget(0));
            return new ActionResult(actor, this, target, -1, GetSuccessChance(actor, target), false);
        }

        var chance = GetSuccessChance(actor, target);
        var roll = Dice.Roll(100);
        var succ = roll <= chance;
        if (succ)
        {
            target.Actor.TakeDamage(actor, this, Dice.Roll(4) + 2);
        }
        if (actor.Mission.AfterActorAttacks != null) actor.Mission.AfterActorAttacks(actor, target.Actor, succ);
        return new ActionResult(actor, this, target, roll, chance, succ);
    }
}

public class AcidLick : Action
{
    public AcidLick()
    {
        Name = "Acid Lick";
        Constraint = TargetConstraint.Actor | TargetConstraint.Enemy | TargetConstraint.OwnLocationOnly;
        Cost = new CounterDict<Resource>(Resource.PrimaryAction, 1);
    }

    protected override int CalcSuccessChance(Actor actor, ActionTarget target)
    {
        if (actor.Effects.Contains(Effect.SureHit)) return 100;
        return 80;
    }

    protected override ActionResult DoPerform(Actor actor, ActionTarget target)
    {
        //Hacky
        if (MissionData.Instance.MissionName == "Cave of Devourers" && target.Actor.Name == "Simone")
        {
            RoaringStarActions.GetNaturalBlink().Perform(target.Actor, new ActionTarget(0));
            return new ActionResult(actor, this, target, -1, GetSuccessChance(actor, target), false);
        }

        var chance = GetSuccessChance(actor, target);
        var roll = Dice.Roll(100);
        var succ = roll <= chance;
        if (succ)
        {
            target.Actor.Effects.Add(Effect.Stunned, 1);
            target.Actor.TakeDamage(actor, this, Dice.Roll(10));
        }

        actor.Stats.Actions.Remove(this); //the only way i can think of atm to force 1 action per turn without making it secondary

        if (actor.Mission.AfterActorAttacks != null) actor.Mission.AfterActorAttacks(actor, target.Actor, succ);
        return new ActionResult(actor, this, target, roll, chance, succ);
    }
}