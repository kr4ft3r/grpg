using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMain : MonoBehaviour
{
    public static GameMain Instance { get; private set; }
    public int GetCurrentMissionIndex() { return _currentMissionIndex; }

    private List<MissionData> _missionData;
    private int _currentMissionIndex;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;

        GameMissions gameMissions = new GameMissions();
        _missionData = gameMissions.Missions;
        //_currentMissionIndex = 1;///////TEST
        MissionData.SetInstance(_missionData[_currentMissionIndex]);//TODO move
    }

    //TODO load mission

    public void NextMission()
    {
        _currentMissionIndex++;
        MissionData.SetInstance(_missionData[_currentMissionIndex]);
        SceneManager.LoadScene("mission" + (_currentMissionIndex + 1));
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
