using System.Collections;
using System.Collections.Generic;

public class HumanTurnMissionState : IMissionState
{
    private MissionGameplay _gameplay;
    public HumanTurnMissionState(MissionGameplay gameplay)
    {
        _gameplay = gameplay;
    }
    
    public string GetName()
    {
        return "human_turn";
    }

    public void Enter()
    {
        _gameplay.missionUI.Log("(your turn starts)");
    }

    public void Exit()
    {
        //
    }

    public IMissionState HandleCommand(IAction action)
    {
        return null;
    }
}
