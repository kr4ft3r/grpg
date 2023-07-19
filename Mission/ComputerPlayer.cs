using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GRPG.GameLogic;

public class ComputerPlayer //TODO inheritance if there is need
{
    private static ComputerPlayer instance;
    public static ComputerPlayer Instance { get { return instance; } }

    private Mission _mission;
    private Team _team;
    private SceneObjects _sceneObjects;
    private MissionUI _missionUI;

    private Dictionary<Actor, AIPersonality> ais; //TODO not here
    public ComputerPlayer(Mission mission, Team team, SceneObjects sceneObjects, MissionUI missionUI)
    {
        _mission = mission;
        _team = team;
        _sceneObjects = sceneObjects;
        _missionUI = missionUI;

        //TODO move this to root, might apply to player team as well
        // Init AIs
        ais = new Dictionary<Actor, AIPersonality>();
        foreach(Actor monster in _mission.GetTeamMembers(_team))
        {
            ais[monster] = new ChargerAIPersonality();
            ais[monster].Init(monster, _team, Team.Human, _mission);
        }

        instance = this;
    }

    //
    // Temporary:
    //

    public void RunMoves()
    {
        _missionUI.SetActiveTeam(_team);
        _missionUI.SetAITurnUIState();

        int actionsMade = 0; //prevent sequences getting stuck
        IEnumerable<GRPG.GameLogic.Actor> aiMembers = _mission.GetTeamMembers(_team);
        foreach (GRPG.GameLogic.Actor actor in aiMembers)
        {
            //_missionUI.SetActiveActor(actor);

            bool botDone = false;
            while(!botDone)
            {
                (Action action, ActionTarget target) = ais[actor].GetNextAction();
                if (action.IsNull())
                {
                    Debug.Log("----- NULL ACTION");
                    botDone = true;
                    continue;
                }

                action.Perform(actor, target);
                actionsMade++;
            }
            
        }
        _mission.EndTurn();
        if (actionsMade == 0) SequencesDone();
    }

    // Flawed implementation with random outcome and not all resources spent
    int DoRandomShit(GRPG.GameLogic.Actor actor)
    {
        List<Action> actions = actor.GetAvailableActions();
        if (actions.Count == 0)
            return 0;
        Action rndAction = actions[(int)Random.Range(0, actions.Count)];
        Debug.Log(actor.Name + " trying " + rndAction.Name);
        List<ActionTarget> targets = actor.GetAvailableTargets(rndAction);

        if (targets.Count == 0)
        {
            return 0;
        }

        ActionTarget rndTarget = targets[Random.Range(0, targets.Count)];

        rndAction.Perform(actor, rndTarget);
        return 1;
    }

    public void SequencesDone()
    {
        HumanPlayer.Instance.InitMove();
    }

    /*public void SceneSequenceDone()
    {
        Debug.LogError("Not implemented");
    }*/
}
