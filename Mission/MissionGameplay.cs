using System.Collections;
using System.Collections.Generic;

public class MissionGameplay
{
    public int TurnNumber { get; }
    public readonly MissionUI missionUI;
    public readonly Team humanTeam;
    public readonly Team aiTeam;

    public delegate void OnMissionStateExit(IMissionState exitedState);
    public delegate void OnMissionStateEnter(IMissionState state);

    event OnMissionStateEnter raiseMissionStateEnter;
    event OnMissionStateExit raiseMissionStateExit;

    public readonly SequenceMissionState introMissionState;
    public readonly HumanTurnMissionState humanTurnMissionState;
    //AnimatingMissionState

    public IMissionState missionState { get; private set; }
    public IMissionState previousMissionState { get; private set; }

    public MissionGameplay(
        MissionUI missionUI,
        Sequence introSequence,
        Team humanTeam,
        Team aiTeam
        )
    {
        TurnNumber = 0;
        
        this.missionUI = missionUI;
        Sequence.Current = introSequence;
        this.humanTeam = humanTeam;
        this.aiTeam = aiTeam;

        // Init mission states
        introMissionState = new SequenceMissionState(this);
        humanTurnMissionState = new HumanTurnMissionState(this);

        previousMissionState = humanTurnMissionState; //TODO initiative
        missionState = introMissionState;
    }

    public void Init()
    {
        missionUI.Log("Initializing mission");

        raiseMissionStateEnter += missionUI.HandleMissionStateEnter;
        raiseMissionStateExit += missionUI.HandleMissionStateExit;
        missionUI.HumanTeam = humanTeam;
        missionUI.AITeam = aiTeam;

        missionState.Enter();
    }

    public void HandleCommand(IAction action)
    {
        IMissionState state = missionState.HandleCommand(action);

        if (state == null)
        {
            return;
        }

        missionState.Exit();
        raiseMissionStateExit(missionState);

        previousMissionState = missionState;
        missionState = state;
        
        missionState.Enter();
        raiseMissionStateEnter(missionState);
    }
}
