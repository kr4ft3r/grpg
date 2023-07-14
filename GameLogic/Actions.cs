using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GRPG.GameLogic
{
    public class ActionTarget
    {
        public Actor Actor;
        public int Location = -1;

        public ActionTarget(Actor actor, int location)
        {
            Actor = actor;
            Location = location;
        }

        public ActionTarget(Actor actor)
        {
            Actor = actor;
        }

        public ActionTarget(int loc)
        {
            Location = loc;
        }
    }

    public class ActionResult
    {
        public Actor Actor;
        public Action Action;
        public ActionTarget Target;
        public int DiceRoll = -1;
        public int Difficulty = -1;
        public bool IsSuccess;

        public ActionResult(Actor actor, Action action, ActionTarget target, int roll, int diff, bool succ)
        {
            Actor = actor;
            Action = action;
            Target = target;
            DiceRoll = roll;
            Difficulty = diff;
            IsSuccess = succ;
        }
        public ActionResult(Actor actor, Action action, ActionTarget target, bool succ)
        {
            Actor = actor;
            Action = action;
            Target = target;
            IsSuccess = succ;
        }
    }

    public abstract class Action
    {
        public static TargetConstraint SelfOnly = TargetConstraint.Actor | TargetConstraint.Self;
        public static Action Null = new NullAction();
        public static Action Move = new ActionMove();
        public static Action Punch = new ActionPunch();

        public string Name { get; protected set; }
        public CounterDict<Resource> Cost { get; protected set; }
        public TargetConstraint Constraint { get; protected set; }

        public virtual ActionValidity GetActionValidity(Actor actor)
        {
            var mission = actor.Mission;
            if (mission.CurrentTeam != actor.Team) return ActionValidity.NotMyTurn;
            if (!actor.Resources.Contains(this.Cost)) return ActionValidity.NotEnoughResources;
            return ActionValidity.Valid;
        }

        public ActionValidity GetActionValidity(Actor actor, ActionTarget target)
        {
            var firstCheck = GetActionValidity(actor);
            if (firstCheck != ActionValidity.Valid) return firstCheck;
            if (Constraint.HasFlag(TargetConstraint.Actor))
            {
                if (target.Actor is null) return ActionValidity.WrongTargetType;
                var enemyTeam = actor.Team == Team.Human ? Team.AI : Team.Human;
                var targetingSelf = actor.Equals(target.Actor);
                var conn = actor.Mission.Connections[actor.Location, target.Actor.Location];
                if (!conn.CanSee) return ActionValidity.LocationNotAccessible;
                if (!conn.CanMove && Constraint.HasFlag(TargetConstraint.NeighbourOnly)) return ActionValidity.LocationNotAccessible;
                if (targetingSelf && !Constraint.HasFlag(TargetConstraint.Self)) return ActionValidity.CannotTargetSelf;
                if (!targetingSelf && Constraint == SelfOnly) return ActionValidity.OnlyTargetSelf;
                if (target.Actor.Team == actor.Team && !targetingSelf && !Constraint.HasFlag(TargetConstraint.Ally)) return ActionValidity.TargetOnWrongTeam;
                if (target.Actor.Team == enemyTeam && !Constraint.HasFlag(TargetConstraint.Enemy)) return ActionValidity.TargetOnWrongTeam;
                if (actor.Location != target.Actor.Location && Constraint.HasFlag(TargetConstraint.OwnLocationOnly)) return ActionValidity.OwnLocationOnly;
            } 
            if (Constraint.HasFlag(TargetConstraint.Location))
            {
                if (target.Location < 0) return ActionValidity.WrongTargetType;
                var conn = actor.Mission.Connections[actor.Location, target.Location];
                if (!conn.CanSee) return ActionValidity.LocationNotAccessible;
                if (!conn.CanMove && Constraint.HasFlag(TargetConstraint.NeighbourOnly)) return ActionValidity.LocationNotAccessible;
            }  
            return CheckTarget(actor, target);
        }

        public int GetSuccessChance(Actor actor, ActionTarget target)
        {
            if (GetActionValidity(actor, target) != ActionValidity.Valid) return -1;
            return CalcSuccessChance(actor, target);
        }

        public ActionResult Perform(Actor actor, ActionTarget target)
        {
            if (GetActionValidity(actor, target) != ActionValidity.Valid) throw new System.Exception("Invalid target.");
            var result = DoPerform(actor, target);
            actor.Resources.Substract(this.Cost);
            actor.Mission.AfterActionPerformed(result);
            return result;
        }

        public virtual bool IsNull()
        {
            return false;
        }

        protected virtual ActionValidity CheckTarget(Actor actor, ActionTarget target)
        {
            return ActionValidity.Valid;
        }

        protected virtual int CalcSuccessChance(Actor actor, ActionTarget target)
        {
            return 100;
        }

        protected abstract ActionResult DoPerform(Actor actor, ActionTarget target);
    }

    public class NullAction : Action
    {
        public NullAction()
        {
            Name = "NULL";
        }
        public override bool IsNull()
        {
            return true;
        }
        protected override ActionResult DoPerform(Actor actor, ActionTarget target)
        {
            return new ActionResult(actor, this, target, true);
        }
    }

    public class ActionMove : Action
    {
        public ActionMove()
        {
            Name = "Move";
            Constraint = TargetConstraint.Location | TargetConstraint.NeighbourOnly;
            FFS.Log("Move: {0}", Constraint);
            Cost = new CounterDict<Resource>(Resource.MoveAction, 1);
        }

        public override ActionValidity GetActionValidity(Actor actor)
        {
            if (actor.Effects[Effect.Rooted] > 0) return ActionValidity.PreventedByEffect;
            return base.GetActionValidity(actor);
        }

        protected override ActionResult DoPerform(Actor actor, ActionTarget target)
        {
            actor.Location = target.Location;
            return new ActionResult(actor, this, target, true);
        }
    }

    public class ActionPunch : Action
    {
        public ActionPunch()
        {
            Name = "Punch";
            Constraint = TargetConstraint.Actor | TargetConstraint.Enemy | TargetConstraint.OwnLocationOnly;
            Cost = new CounterDict<Resource>(Resource.PrimaryAction, 1);
        }

        protected override int CalcSuccessChance(Actor actor, ActionTarget target)
        {
            if (actor.Effects.Contains(Effect.SureHit)) return 100;
            return 50;
        }

        protected override ActionResult DoPerform(Actor actor, ActionTarget target)
        {
            var chance = GetSuccessChance(actor, target);
            var roll = Dice.Roll(100);
            var succ = roll <= chance;
            if (succ)
            {
                target.Actor.Effects.Add(Effect.Stunned, 1);
                target.Actor.TakeDamage(Dice.Roll(12));
            }
            actor.Mission.AfterActorAttacks(actor, target.Actor, succ);
            return new ActionResult(actor, this, target, roll, chance, succ);
        }
    }

}