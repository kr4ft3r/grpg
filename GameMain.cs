using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMain : MonoBehaviour
{
    public static GameMain Instance { get; private set; }
    public GameState GameState { get; private set; }
    public int GetCurrentMissionIndex() { return GameState.MissionIndex; }

    private List<MissionData> _missionData;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;

        GameState = new GameState("0.1");
        SetStartingSkills();
        SetEmptyUpgrades();
        GameMissions gameMissions = new GameMissions();
        _missionData = gameMissions.Missions;
        //_currentMissionIndex = 1;///////TEST
        MissionData.SetInstance(_missionData[GameState.MissionIndex]);//TODO move
    }

    //TODO load mission

    public void NextMission()
    {
        SetEmptyUpgrades();
        GameState.MissionIndex++;
        MissionData.SetInstance(_missionData[GameState.MissionIndex]);
        SceneManager.LoadScene("mission" + (GameState.MissionIndex + 1));
    }

    private void SetStartingSkills()
    {
        GameState.SimoneSkills = CharacterSkills.SimoneDefault;
        GameState.AndreaSkills = CharacterSkills.AndreaDefault;
        GameState.RedSkills = CharacterSkills.RedDefault;
        if (GameState.MissionIndex == 0)
        {
            // Strip for first mission
            GameState.SimoneSkills[SkillLine.SimonePrimary] = 0;
            GameState.SimoneSkills[SkillLine.SimoneSecondary] = 0;
            GameState.AndreaSkills[SkillLine.AndreaPrimary] = 0;
            GameState.RedSkills[SkillLine.RedPrimary] = 0;
            SetEmptyUpgrades();
        }
    }

    private void SetEmptyUpgrades()
    {
        GameState.SimoneUpgrades = new Dictionary<string, int>()
        {
            {SkillLine.SimonePrimary, 0 },
            {SkillLine.SimoneSecondary, 0 }
        };
        GameState.AndreaUpgrades = new Dictionary<string, int>()
        {
            {SkillLine.AndreaPrimary, 0 },
            {SkillLine.AndreaSecondary, 0 }
        };
        GameState.RedUpgrades = new Dictionary<string, int>()
        {
            {SkillLine.RedPrimary, 0 },
            {SkillLine.RedSecondary, 0 }
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
