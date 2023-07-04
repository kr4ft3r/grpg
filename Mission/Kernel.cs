using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GRPG.Data;

/// <summary>
/// Link between Unity crap and our beautiful code
/// </summary>
public class Kernel : MonoBehaviour
{
    private MissionGameplay _missionGameplay;

    // Start is called before the first frame update
    void Start()
    {
        _missionGameplay = new MissionGameplay(
            missionUI: GetComponent<MissionUI>(),
            //TODO intro, teams and other mission-specific stuff better belong to some MissionData object
            introSequence: new Sequence(
                new List<SequenceEvent>() {
                    new SequenceEvent(SequenceEventType.Text,
                    "/========== Welcome to ==========\\\n  THE HARD CODED MISSION INTRO \n\nWritten by the crappy debug window\n"
                    ),
                    new SequenceEvent(SequenceEventType.Text, "The king welcomes you to his castle."),
                    new SequenceEvent(SequenceEventType.Text, "KING: Help! I've got too many fucking dragons hardcoded into my castle."),
                    new SequenceEvent(SequenceEventType.Text, "DRAGON: Har, har! You will never finish this game! The feature creep is beyond your comprehension, mortal!"),
                }
                ),
            humanTeam: new Team("Heroes",
                new List<Actor>()
                {
                    new Actor("Moe", new CharacterStats(MP:14)),
                    new Actor("Larry", new CharacterStats(MP:20)),
                    new Actor("Curly", new CharacterStats(MP:25)),
                },
                Team.Owner.Human
                ),
            aiTeam: new Team("Monsters",
                new List<Actor>()
                {
                    new Actor("HC Dragon 1", new CharacterStats(MP:10)),
                    new Actor("HC Dragon 2", new CharacterStats(MP:14)),
                    new Actor("HC Dragon 3", new CharacterStats(MP:20)),
                },
                Team.Owner.Human
                )
            );
        _missionGameplay.Init();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInputs();
    }

    private void HandleInputs()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _missionGameplay.HandleCommand(
                new ContinueSequenceAction(_missionGameplay)
                );
            return;
        }
    }
}
