using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    private List<MissionData> _missionData;
    private int _currentMissionIndex;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        GameMissions gameMissions = new GameMissions();
        _missionData = gameMissions.Missions;

        MissionData.SetInstance(_missionData[_currentMissionIndex]);//TODO move
    }

    //TODO load mission

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
