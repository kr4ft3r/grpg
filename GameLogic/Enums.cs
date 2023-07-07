using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GRPG.GameLogic
{
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
        Rooted,
        SureHit
    }

    public enum Team
    {
        Human,
        AI
    }

    public enum TargetType
    {
        Actor, 
        Location
    }

    public enum ActionValidity
    {
        Valid,
        NotMyTurn,
        NotEnoughResources,
        WrongTargetType,
        LocationNotAccessible,
        OutOfRange,
        OnCooldown,
        SelfOnly,
        TargetOnWrongTeam,
        PreventedByEffect
    }
}