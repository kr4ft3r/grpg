using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GRPG.GameLogic;

public class HumanPlayer {
    private static HumanPlayer instance;
    public static HumanPlayer Instance { get { return instance; } }

    private Mission _mission;
    private Team _team;
    private SceneObjects _sceneObjects;
    private MissionUI _missionUI;
    public List<Actor> SkipTurnActors { get; private set; }

    public HumanPlayer(Mission mission, Team team, SceneObjects sceneObjects, MissionUI ui)
    {
        _mission = mission;
        _team = team;
        _sceneObjects = sceneObjects;
        _missionUI = ui;
        SkipTurnActors = new List<Actor>();

        instance = this;
    }

    public void InitMove()
    {
        SkipTurnActors = new List<Actor>();
        _missionUI.SetActiveTeam(_team);

        Actor defaultActor = _mission.GetTeamMembers(_team).First(); // Will throw "sequence contains no elements" on all players dead
        Debug.Log(defaultActor.Name);
        Action defaultAction = defaultActor.GetAllActions()[0];

        _missionUI.SetActiveActor(defaultActor);
        _missionUI.SetPlayerSelectedActionUIState(defaultAction);
    }

    public void SkipTurn()
    {
        SkipTurnActors.Add(_missionUI.GetActiveActor());
        _missionUI.SetNextActorOrEndTurn();
    }

    public void SequencesDone()
    {
        _missionUI.SetNextActionOrContinue();
    }

    /*public void SceneSequenceDone()
    {
        _missionUI.Skip();
    }*/
}
