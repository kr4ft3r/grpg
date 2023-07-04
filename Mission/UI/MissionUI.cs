using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionUI : MonoBehaviour
{
    public Team HumanTeam { get; set; }
    public Team AITeam { get; set; }

    private List<string> _log = new List<string>() { "Loading..." };
    private IMissionState _activeMissionState;

    public void Log(string line)
    {
        if (_log.Count == 10)
        {
            _log.RemoveAt(0);
        }
        _log.Add(line);
    }

    private void OnGUI()
    {
        DrawLog();
        DrawTeams();
    }

    //
    // Drawing functions
    //

    private void DrawLog()
    {
        string log = string.Join("\n", _log);
        GUI.Label(new Rect(10, 10, 400, 600), log);
    }

    private Rect humanTeamRect = new Rect(450, 10, 600, 300);
    private Rect aiTeamRect = new Rect(450, 350, 600, 300);
    private static int humanTeamWindowID = 0;
    private static int aiTeamWindowID = 1;

    private void DrawTeams()
    {
        if (_activeMissionState == null) return;
        if (HumanTeam == null || AITeam == null) return;

        humanTeamRect = GUI.Window(humanTeamWindowID, humanTeamRect, TeamWindowFunction, HumanTeam.name);
        aiTeamRect = GUI.Window(aiTeamWindowID, aiTeamRect, TeamWindowFunction, AITeam.name);
    }

    void TeamWindowFunction(int windowID)
    {
        Team team;
        bool isHumanPlayer = false;
        if (windowID == humanTeamWindowID)
        {
            team = HumanTeam;
            isHumanPlayer = true;
        } else
        {
            team = AITeam;
        }

        int actorIndex = 0;
        int actorOffsetY = 50;
        int actionButtonOffsetX = 175;
        foreach (Actor actor in team.members)
        {
            // Header
            string actorHeader = actor.name + "  MP:" + actor.MovementPoints + "/" + actor.baseStats.MP;
            GUI.Label(new Rect(10, 20 + actorIndex*actorOffsetY, 200, 50), actorHeader);

            // Action buttons
            if (isHumanPlayer && _activeMissionState.GetName() == "human_turn")
            {
                List<string> actions = new List<string>() { "Simple Action (5)", "Advanced Action (10)", "Skip turn" };
                int actionIndex = 0;
                foreach (string action in actions)
                {
                    GUI.Button(new Rect(10 + actionIndex * actionButtonOffsetX, 45 + actorIndex * actorOffsetY, 175, 20), action);
                    actionIndex++;
                }
            }

            actorIndex++;
        }
    }

    //
    // Event handlers
    //

    public void HandleMissionStateExit(IMissionState exitedState)
    {
        Debug.Log("BING!");
    }

    public void HandleMissionStateEnter(IMissionState state)
    {
        _activeMissionState = state;
    }

}
