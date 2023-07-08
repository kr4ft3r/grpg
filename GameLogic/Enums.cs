using System;
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

    [Flags]
    public enum TargetConstraint
    {
        None = 0,
        Actor = 1,              // Must specify actor target if set
        Location = 2,           // Must specify location target if set
        Self = 4,               // Self targeting allowed
        Enemy = 8,              // Enemy targeting allowed
        Ally = 16,              // Ally (non-self) targeting allowed
        NeighbourOnly = 32,     // Location must be neighbouring own location (CanMove)
        OwnLocationOnly = 64,   // actor.Location has to be target.Location or target.Actor.Location
    }

    public enum ActionValidity
    {
        Valid,
        NotMyTurn,
        NotEnoughResources,
        WrongTargetType,
        LocationNotAccessible,
        OwnLocationOnly,
        OnlyTargetSelf,
        CannotTargetSelf,
        TargetOnWrongTeam,
        PreventedByEffect
    }
}