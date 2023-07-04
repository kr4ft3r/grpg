#nullable enable
using System.Collections;
using System.Collections.Generic;

public class ContinueSequenceAction : IAction
{
    private MissionGameplay _mission;
    public ContinueSequenceAction(MissionGameplay mission)
    {
        _mission = mission;
    }

    public string GetName()
    {
        return "continue_sequence";
    }

    public void Execute()
    {
        if (Sequence.Current == null)
        {
            return;
        }
        string? text = Sequence.Current.Next();
        if (text == null)
        {
            return;
        }

        _mission.missionUI.Log(
            text
            );
    }
}
