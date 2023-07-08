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
        public static Action Move = new ActionMove();
        public static Action Disintegrate = new ActionDisintegrate();

        public string Name { get; protected set; }
        public CounterDict<Resource> Cost { get; protected set; }
        public TargetType TargetType { get; protected set; }

        public virtual ActionValidity GetActionValidity(Actor actor)
        {
            var mission = actor.Mission;
            // Might change this to return a reason for being unable to perform action.
            if (mission.CurrentTeam != actor.Team) return ActionValidity.NotMyTurn;
            foreach (var item in Cost)
            {
                if (actor.Resources[item.Key] < item.Value) return ActionValidity.NotEnoughResources;
            }
            return ActionValidity.Valid;
        }

        public ActionValidity GetActionValidity(Actor actor, ActionTarget target)
        {
            if (TargetType == TargetType.Actor && target.Actor is null) return ActionValidity.WrongTargetType;
            if (TargetType == TargetType.Location && target.Location < 0) return ActionValidity.WrongTargetType;
            return CheckTarget(actor, target);
        }

        public int GetSuccessChance(Actor actor, ActionTarget target)
        {
            if (GetActionValidity(actor, target) != ActionValidity.Valid) return -1;
            return CalcSuccessChance(actor, target);
        }

        protected virtual ActionValidity CheckTarget(Actor actor, ActionTarget target)
        {
            return ActionValidity.Valid;
        }

        protected virtual int CalcSuccessChance(Actor actor, ActionTarget target)
        {
            return 100;
        }

        public abstract ActionResult Perform(Actor actor, ActionTarget target);
    }

    public class ActionMove : Action
    {
        public ActionMove()
        {
            Name = "Move";
            TargetType = TargetType.Location;
            Cost = new CounterDict<Resource>(Resource.MoveAction, 1);
        }

        public override ActionValidity GetActionValidity(Actor actor)
        {
            if (actor.Effects[Effect.Rooted] > 0) return ActionValidity.PreventedByEffect;
            return base.GetActionValidity(actor);
        }

        protected override ActionValidity CheckTarget(Actor actor, ActionTarget target)
        {
            var mission = actor.Mission;
            if (!mission.Connections[actor.Location, target.Location].CanMove) return ActionValidity.LocationNotAccessible;
            return ActionValidity.Valid;
        }

        public override ActionResult Perform(Actor actor, ActionTarget target)
        {
            if (GetSuccessChance(actor, target) <= 0) throw new System.Exception("Invalid action.");
            actor.Location = target.Location;
            return new ActionResult(actor, this, target, true);
        }
    }

    public class ActionDisintegrate : Action
    {
        public ActionDisintegrate()
        {
            Name = "Disintegrating Punch";
            TargetType = TargetType.Actor;
            Cost = new CounterDict<Resource>(Resource.PrimaryAction, 1);
        }

        protected override ActionValidity CheckTarget(Actor actor, ActionTarget target)
        {
            return target.Actor.Location == actor.Location ? ActionValidity.Valid : ActionValidity.OutOfRange;
        }

        protected override int CalcSuccessChance(Actor actor, ActionTarget target)
        {
            if (actor.Effects.Contains(Effect.SureHit)) return 100;
            if (target.Actor.Equals(actor)) return 99;
            return 50;
        }

        public override ActionResult Perform(Actor actor, ActionTarget target)
        {
            var chance = GetSuccessChance(actor, target);
            var roll = Dice.Roll(100);
            var succ = roll <= chance;
            if (succ)
            {
                actor.Mission.Actors.Remove(target.Actor);
            }
            actor.Mission.AfterActorAttacks(actor, target.Actor, succ);
            return new ActionResult(actor, this, target, roll, chance, succ);
        }
    }

}