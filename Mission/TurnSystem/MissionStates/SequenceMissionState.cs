#nullable enable
using System.Collections;
using System.Collections.Generic;

public class SequenceMissionState : IMissionState
{
    private MissionGameplay _gameplay;

    public SequenceMissionState(MissionGameplay gameplay)
    {
        _gameplay = gameplay;
    }

    public string GetName()
    {
        return "sequence";
    }

    public void Enter()
    {
        _gameplay.missionUI.Log("(story sequence starts)");
    }

    public void Exit()
    {
        _gameplay.missionUI.Log("(story sequence ends)");
        //TODO hide story UI
    }

    public IMissionState? HandleCommand(IAction action)
    {
        //TODO progress story, and/or skip intro commands
        if(action.GetName() == "continue_sequence" && Sequence.Current != null)
        {
            if (Sequence.Current.Done)
            {
                return _gameplay.previousMissionState;
            }
            action.Execute();
            EndSegment();
        }

        return null;
    }

    private void EndSegment()
    {
        _gameplay.missionUI.Log("\n     ..press Space to continue\n");
    }
}
