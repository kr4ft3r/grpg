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

        public virtual ActionValidity GetActionValidity(Mission mission, Actor actor)
        {
            // Might change this to return a reason for being unable to perform action.
            if (mission.CurrentTeam != actor.Team) return ActionValidity.NotMyTurn;
            foreach (var item in Cost)
            {
                if (actor.Resources[item.Key] < item.Value) return ActionValidity.NotEnoughResources;
            }
            return ActionValidity.Valid;
        }

        public ActionValidity GetActionValidity(Mission mission, Actor actor, ActionTarget target)
        {
            if (TargetType == TargetType.Actor && target.Actor is null) return ActionValidity.WrongTargetType;
            if (TargetType == TargetType.Location && target.Location < 0) return ActionValidity.WrongTargetType;
            return CheckTarget(mission, actor, target);
        }

        public int GetSuccessChance(Mission mission, Actor actor, ActionTarget target)
        {
            if (GetActionValidity(mission, actor, target) != ActionValidity.Valid) return -1;
            return CalcSuccessChance(mission, actor, target);
        }

        protected virtual ActionValidity CheckTarget(Mission mission, Actor actor, ActionTarget target)
        {
            return ActionValidity.Valid;
        }

        protected virtual int CalcSuccessChance(Mission mission, Actor actor, ActionTarget target)
        {
            return 100;
        }

        public abstract ActionResult Perform(Mission mission, Actor actor, ActionTarget target);
    }

    public class ActionMove : Action
    {
        public ActionMove()
        {
            Name = "Move";
            TargetType = TargetType.Location;
            Cost = new CounterDict<Resource>(Resource.MoveAction, 1);
        }

        public override ActionValidity GetActionValidity(Mission mission, Actor actor)
        {
            if (actor.Effects[Effect.Rooted] > 0) return ActionValidity.PreventedByEffect;
            return base.GetActionValidity(mission, actor);
        }

        protected override ActionValidity CheckTarget(Mission mission, Actor actor, ActionTarget target)
        {
            if (!mission.Connections[actor.Location, target.Location].CanMove) return ActionValidity.LocationNotAccessible;
            return ActionValidity.Valid;
        }

        public override ActionResult Perform(Mission mission, Actor actor, ActionTarget target)
        {
            if (GetSuccessChance(mission, actor, target) <= 0) throw new System.Exception("Invalid action.");
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

        protected override ActionValidity CheckTarget(Mission mission, Actor actor, ActionTarget target)
        {
            return target.Actor.Location == actor.Location ? ActionValidity.Valid : ActionValidity.OutOfRange;
        }

        protected override int CalcSuccessChance(Mission mission, Actor actor, ActionTarget target)
        {
            if (actor.Effects.Contains(Effect.SureHit)) return 100;
            if (target.Actor.Equals(actor)) return 99;
            return 50;
        }

        public override ActionResult Perform(Mission mission, Actor actor, ActionTarget target)
        {
            if (GetSuccessChance(mission, actor, target) <= 0) throw new System.Exception("Invalid action.");
            var chance = GetSuccessChance(mission, actor, target);
            var roll = Dice.Roll(100);
            var succ = roll <= chance;
            if (succ)
            {
                mission.Actors.Remove(actor);
            }
            return new ActionResult(actor, this, target, roll, chance, succ);
        }
    }

}