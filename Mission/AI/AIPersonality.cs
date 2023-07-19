using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GRPG.GameLogic;

public abstract class AIPersonality
{
    protected Actor _actor;
    protected Team _myTeam;
    protected Team _enemyTeam;
    protected Mission _mission;
    public void Init(Actor myActor, Team myTeam, Team enemyTeam, Mission mission)
    {
        _myTeam = myTeam;
        _actor = myActor;
        _enemyTeam = enemyTeam;
        _mission = mission;
    }

    public abstract (Action, ActionTarget) GetNextAction();
}
