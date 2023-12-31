using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GRPG.GameLogic;

/// <summary>
/// Hardcode data and behavior for all the missions here.
/// - Mission name
/// - Actor blueprints
/// - Scene sequences
/// </summary>
public class GameMissions
{
    public List<MissionData> Missions { get; private set; }
    public GameMissions()
    {
        Missions = new List<MissionData>() {
            new MissionData(
                "Cave of Devourers",
                Mission1_Actors(),
                Mission1_SceneSequences(),
                new Dictionary<string, Action>()
                {
                    {"SearchSkeleton", RoaringStarActions.GetSearchSkeletonSceneAction() }
                }
                ),
            new MissionData(
                "Hideaway Planet",
                Mission2_Actors(),
                Mission2_SceneSequences(),
                new Dictionary<string, Action>(){ 
                })
        };
    }

    /*public MissionData LoadMission()
    {
    Ne.
    }*/

    /*
     * Mission 1: Cave of Devourers
     */

    private Dictionary<string, MissionActorBlueprint> Mission1_Actors()
    {
        Dictionary<string, MissionActorBlueprint> actors = new Dictionary<string, MissionActorBlueprint>();
        Dictionary<string, Dictionary<string, int>> skills = CharacterSkills.GetSkillsWithUpgrades(GameMain.Instance.GameState);

        // Simona
        CharacterStats simonaStats = new CharacterStats();
        simonaStats.Actions = CharacterSkills.SimoneMissionActions(skills["Simone"]); //new List<Action>() { Action.Move };
        simonaStats.PerBattleResources = new CounterDict<Resource>() { };
        simonaStats.PerTurnResources = new CounterDict<Resource>() { { Resource.MoveAction, 1 }, { Resource.PrimaryAction, 1 } };
        actors.Add("Simone", new MissionActorBlueprint(
            "Simone", 18,
            PersistentCharacters.SimonePresentation,
            simonaStats,
            Team.Human,
            0));

        // Andrea
        CharacterStats andreaStats = new CharacterStats();
        andreaStats.Actions = CharacterSkills.AndreaMissionActions(skills["Andrea"]);//new List<Action>() { Action.Move, RoaringStarActions.GetTankAction(1) };
        andreaStats.PerBattleResources = new CounterDict<Resource>() { };
        andreaStats.PerTurnResources = new CounterDict<Resource>() { { Resource.MoveAction, 1 }, { Resource.PrimaryAction, 1 } };
        actors.Add("Andrea", new MissionActorBlueprint(
            "Andrea", 54,
            PersistentCharacters.AndreaPresentation,
            andreaStats,
            Team.Human,
            0));

        // Red
        CharacterStats redStats = new CharacterStats();
        redStats.Actions = CharacterSkills.RedMissionActions(skills["Red"]); //new List<Action>() { Action.Move, RoaringStarActions.GetFleeAction(1) };
        redStats.PerBattleResources = new CounterDict<Resource>() { };
        redStats.PerTurnResources = new CounterDict<Resource>() { { Resource.MoveAction, 1 }, { Resource.PrimaryAction, 1 } };
        actors.Add("Red", new MissionActorBlueprint(
            "Red", 30,
            PersistentCharacters.RedPresentation,
            redStats,
            Team.Human,
            0));

        //Monsters

        CharacterStats devourerStats = new CharacterStats();
        devourerStats.Actions = new List<Action>() { Action.Move, RoaringStarActions.GetAcidLickAction(), RoaringStarActions.GetSmackAction() };
        devourerStats.PerBattleResources = new CounterDict<Resource>() { };
        devourerStats.PerTurnResources = new CounterDict<Resource>() { { Resource.MoveAction, 1 }, { Resource.PrimaryAction, 1 } };

        actors.Add("Devourer", new MissionActorBlueprint(
            "Devourer", 10,
            new CharacterPresentation(
                "Devourer",
                null, null, null,
                1.5f, 3,
                walkSprites: new List<string>() { "Demon/walk1", "Demon/walk2", "Demon/walk3", "Demon/walk4" },
                null,
                deadSprites: new List<string>() { "Demon/ded1", "Demon/ded2", "Demon/ded3", "Demon/ded4", "Demon/ded5" }),
            devourerStats,
            Team.AI,
            new int[6] { 3, 11, 5, 7, 8, 10 }
        ));

        return actors;
    }

    private SceneSequenceData Mission1_SceneSequences()
    {
        return new SceneSequenceData(
            new Dictionary<string, System.Action<MissionUI, Sequencer, SceneObjects, float>>() {
                {"player_character_died", (MissionUI missionUI, Sequencer sequencer, SceneObjects sceneObjects, float deltaTime) =>{
                    string deadCharacter = sequencer.MissionVars.GetString("_dead_character");
                    sequencer.HandleDialogue(new List<(string,string)>() {
                        ("Simone", "No, "+deadCharacter+"! This can't be happpening... This did NOT happen!!!"),
                        ("Simone", "* TODO: insert some short but epic screen-wide blast, as Simone alters reality *")
                    }, "mission_failed");
                }},
                {"mission_failed", (MissionUI missionUI, Sequencer sequencer, SceneObjects sceneObjects, float deltaTime) => {
                    sceneObjects.RestartMission();
                } },
                {"intro", (MissionUI missionUI, Sequencer sequencer, SceneObjects sceneObjects, float deltaTime) => {
                    if (!sequencer.Vars.IsStringSet("_dialogue_" + 0)) //TODO better to have isfirstframe func
                    { // Move all monster up so they can jump ye
                        foreach(KeyValuePair<Actor,GameObject> kv in sceneObjects.ActorObjects)
                            if (kv.Value.GetComponent<SceneActor>().Presentation.Name == "Devourer")
                                kv.Value.GetComponentInChildren<SpriteRenderer>().enabled = false;
                    }
                    sequencer.HandleDialogue(new List<(string,string)>(){
                        ("Andrea", "Well, this is it I guess. Simona, stay back sis! I still got some strength in me to hold those creatures back� I am so sorry for getting you into this mess."),
                        ("Simone", "Oh Andy, I am the one who is sorry, for being so useless. *sob* It is ok, I am ready, at least we will perish together."),
                        ("Andrea", "Quit such talk! I will not let you die here, you will find a way out, you hear me?!"),
                        ("Red", "Umm, never mind me, you guys. Hey, that�s fine, I am no one's sister but I will show you how a real pirate dies - by fleeing until my legs run out!"),
                        ("Simone", "Oh Red, but you are like a sister to us. Come, let�s all have a hug." ),
                        ("Red", "Get away from me, ye bugger. If you weren�t so damn useless, we might at least have some chance."),
                        ("Andrea", "Quiet� they are coming. I love you, sis. Now get lost, find a place to hide."),
                        ("Simone", "*sob* No, no, I am staying with you." )
                    }, "monsters_appear");
                }},
                {"monsters_appear", (MissionUI missionUI, Sequencer sequencer, SceneObjects sceneObjects, float deltaTime) => {
                    //TODO implement bool values
                    float fallDuration = 0.4f;
                    bool monster1Appeared = sequencer.Vars.IsStringSet("monster1_appeared");
                    bool monster2Appeared = sequencer.Vars.IsStringSet("monster2_appeared");
                    bool monster3Appeared = sequencer.Vars.IsStringSet("monster3_appeared");
                    bool monster4Appeared = sequencer.Vars.IsStringSet("monster4_appeared");
                    bool monster5Appeared = sequencer.Vars.IsStringSet("monster5_appeared");
                    bool monster6Appeared = sequencer.Vars.IsStringSet("monster6_appeared");
                    GameObject devourer1GO = sceneObjects.GetActorGOByUniqueName("Devourer 1");
                    GameObject devourer2GO = sceneObjects.GetActorGOByUniqueName("Devourer 2");
                    GameObject devourer3GO = sceneObjects.GetActorGOByUniqueName("Devourer 3");
                    GameObject devourer4GO = sceneObjects.GetActorGOByUniqueName("Devourer 4");
                    GameObject devourer5GO = sceneObjects.GetActorGOByUniqueName("Devourer 5");
                    GameObject devourer6GO = sceneObjects.GetActorGOByUniqueName("Devourer 6");
                    //TODO GameObject[] monsters = new GameObject[3] {devourer1GO, devourer2GO, devourer3GO};
                    // TODO int vals
                    if (sequencer.TimeElapsed >= 0.2f && !monster1Appeared)
                    {
                        sequencer.Vars.SetString("monster1_appeared", (sequencer.TimeElapsed).ToString());
                        sequencer.Vars.SetVector("monster1_goal", devourer1GO.transform.position);
                        devourer1GO.transform.Translate(Vector3.up * 10);
                        sequencer.Vars.SetVector("monster1_position", devourer1GO.transform.position);
                        devourer1GO.GetComponentInChildren<SpriteRenderer>().enabled = true;
                    }
                    if (sequencer.TimeElapsed >= 0.4f && !monster2Appeared)
                    {
                        sequencer.Vars.SetString("monster2_appeared", (sequencer.TimeElapsed).ToString());
                        sequencer.Vars.SetVector("monster2_goal", devourer2GO.transform.position);
                        sceneObjects.GetActorGOByUniqueName("Devourer 2").transform.Translate(Vector3.up * 10);
                        sequencer.Vars.SetVector("monster2_position", devourer2GO.transform.position);
                        devourer2GO.GetComponentInChildren<SpriteRenderer>().enabled = true;
                    }
                    if (sequencer.TimeElapsed >= 0.6f && !monster3Appeared)
                    {
                        sequencer.Vars.SetString("monster3_appeared", (sequencer.TimeElapsed).ToString());
                        sequencer.Vars.SetVector("monster3_goal", devourer3GO.transform.position);
                        sceneObjects.GetActorGOByUniqueName("Devourer 3").transform.Translate(Vector3.up * 10);
                        sequencer.Vars.SetVector("monster3_position", devourer3GO.transform.position);
                        devourer3GO.GetComponentInChildren<SpriteRenderer>().enabled = true;
                    }
                    if (sequencer.TimeElapsed >= 0.7f && !monster4Appeared)
                    {
                        sequencer.Vars.SetString("monster4_appeared", (sequencer.TimeElapsed).ToString());
                        sequencer.Vars.SetVector("monster4_goal", devourer4GO.transform.position);
                        sceneObjects.GetActorGOByUniqueName("Devourer 4").transform.Translate(Vector3.up * 10);
                        sequencer.Vars.SetVector("monster4_position", devourer4GO.transform.position);
                        devourer4GO.GetComponentInChildren<SpriteRenderer>().enabled = true;
                    }
                    if (sequencer.TimeElapsed >= 0.8f && !monster5Appeared)
                    {
                        sequencer.Vars.SetString("monster5_appeared", (sequencer.TimeElapsed).ToString());
                        sequencer.Vars.SetVector("monster5_goal", devourer5GO.transform.position);
                        sceneObjects.GetActorGOByUniqueName("Devourer 5").transform.Translate(Vector3.up * 10);
                        sequencer.Vars.SetVector("monster5_position", devourer5GO.transform.position);
                        devourer5GO.GetComponentInChildren<SpriteRenderer>().enabled = true;
                    }
                    if (sequencer.TimeElapsed >= 0.9f && !monster6Appeared)
                    {
                        sequencer.Vars.SetString("monster6_appeared", (sequencer.TimeElapsed).ToString());
                        sequencer.Vars.SetVector("monster6_goal", devourer6GO.transform.position);
                        sceneObjects.GetActorGOByUniqueName("Devourer 6").transform.Translate(Vector3.up * 10);
                        sequencer.Vars.SetVector("monster6_position", devourer6GO.transform.position);
                        devourer6GO.GetComponentInChildren<SpriteRenderer>().enabled = true;
                    }

                    float timePassed;

                    if (monster1Appeared)
                    {
                        timePassed = sequencer.TimeElapsed - float.Parse(sequencer.Vars.GetString("monster1_appeared"));
                        if (timePassed < fallDuration)
                        {
                            devourer1GO.transform.position = Vector3.Lerp(sequencer.Vars.GetVector("monster1_position"), sequencer.Vars.GetVector("monster1_goal"), timePassed / fallDuration);
                        } else
                        {
                            devourer1GO.transform.position = sequencer.Vars.GetVector("monster1_goal");
                            if(!sequencer.Vars.IsStringSet("monster1_playedsound"))
                            {
                                devourer1GO.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Sounds/gnjec"));
                                sequencer.Vars.SetString("monster1_playedsound", "");
                            }
                        }
                    }
                    if (monster2Appeared)
                    {
                        timePassed = sequencer.TimeElapsed - float.Parse(sequencer.Vars.GetString("monster2_appeared"));
                        if (timePassed < fallDuration)
                        {
                            devourer2GO.transform.position = Vector3.Lerp(sequencer.Vars.GetVector("monster2_position"), sequencer.Vars.GetVector("monster2_goal"), timePassed / fallDuration);
                        } else
                        {
                            devourer2GO.transform.position = sequencer.Vars.GetVector("monster2_goal");
                            if(!sequencer.Vars.IsStringSet("monster2_playedsound"))
                            {
                                devourer2GO.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Sounds/gnjec"));
                                sequencer.Vars.SetString("monster2_playedsound", "");
                            }
                        }
                    }
                    if (monster3Appeared)
                    {
                        timePassed = sequencer.TimeElapsed - float.Parse(sequencer.Vars.GetString("monster3_appeared"));
                        if (timePassed < fallDuration)
                        {
                            devourer3GO.transform.position = Vector3.Lerp(sequencer.Vars.GetVector("monster3_position"), sequencer.Vars.GetVector("monster3_goal"), timePassed / fallDuration);
                        } else
                        {
                            devourer3GO.transform.position = sequencer.Vars.GetVector("monster3_goal");
                            if(!sequencer.Vars.IsStringSet("monster3_playedsound"))
                            {
                                devourer3GO.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Sounds/gnjec"));
                                sequencer.Vars.SetString("monster3_playedsound", "");
                            }
                        }
                    }
                    if (monster4Appeared)
                    {
                        timePassed = sequencer.TimeElapsed - float.Parse(sequencer.Vars.GetString("monster4_appeared"));
                        if (timePassed < fallDuration)
                        {
                            devourer4GO.transform.position = Vector3.Lerp(sequencer.Vars.GetVector("monster4_position"), sequencer.Vars.GetVector("monster4_goal"), timePassed / fallDuration);
                        } else
                        {
                            devourer4GO.transform.position = sequencer.Vars.GetVector("monster4_goal");
                            if(!sequencer.Vars.IsStringSet("monster4_playedsound"))
                            {
                                devourer4GO.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Sounds/gnjec"));
                                sequencer.Vars.SetString("monster4_playedsound", "");
                            }
                        }
                    }
                    if (monster5Appeared)
                    {
                        timePassed = sequencer.TimeElapsed - float.Parse(sequencer.Vars.GetString("monster5_appeared"));
                        if (timePassed < fallDuration)
                        {
                            devourer5GO.transform.position = Vector3.Lerp(sequencer.Vars.GetVector("monster5_position"), sequencer.Vars.GetVector("monster5_goal"), timePassed / fallDuration);
                        } else
                        {
                            devourer5GO.transform.position = sequencer.Vars.GetVector("monster5_goal");
                            if(!sequencer.Vars.IsStringSet("monster5_playedsound"))
                            {
                                devourer5GO.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Sounds/gnjec"));
                                sequencer.Vars.SetString("monster5_playedsound", "");
                            }
                        }
                    }
                    if (monster6Appeared)
                    {
                        timePassed = sequencer.TimeElapsed - float.Parse(sequencer.Vars.GetString("monster6_appeared"));
                        if (timePassed < fallDuration)
                        {
                            devourer6GO.transform.position = Vector3.Lerp(sequencer.Vars.GetVector("monster6_position"), sequencer.Vars.GetVector("monster6_goal"), timePassed / fallDuration);
                        } else
                        {
                            devourer6GO.transform.position = sequencer.Vars.GetVector("monster6_goal");
                            if(!sequencer.Vars.IsStringSet("monster6_playedsound"))
                            {
                                devourer6GO.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Sounds/gnjec"));
                                sequencer.Vars.SetString("monster6_playedsound", "");
                            }

                            sequencer.SceneSequenceFinished = true;
                        }
                    }

                    sequencer.TimeElapsed += deltaTime;
                    //
                }},
                {"outro", (MissionUI missionUI, Sequencer sequencer, SceneObjects sceneObjects, float deltaTime) =>{
                    sequencer.HandleDialogue(new List<(string,string)>() {
                        ("Andrea", "Aaand that would be the last one. Hoorah! No pirate has ever defeated the Cave of Devourers!"),
                        ("Simone", "I�m so happy, sis! The spirit of our dear mother must have been watching over us."),
                        ("Andrea", "And you got some tricks on you as well. What was that, with you teleporting about?"),
                        ("Simone", "I honestly don�t know, Andy. I just got so scared of monsters, I wished to be somewhere else and all of a sudden I really was somewhere else!"),
                        ("Red", "If you�re done celebrating the little brat�s demonic powers, I would like to announce that I have found a treasure map on this unlucky individual."),
                        ("Andrea", "A treasure map you say? Now that brings an old itch."),
                        ("Andrea", "A treasure map you say? Now that brings an old itch.")//TODO
                    });
                    if (!sequencer.Vars.IsStringSet("_dialogue_" + 6)) GameMain.Instance.NextMission();//TODO temp.
                }},
                {"simone_search_skeleton", (MissionUI missionUI, Sequencer sequencer, SceneObjects sceneObjects, float deltaTime) => {
                    //if (!sequencer.IsStringSet("_dialogue_" + 0))
                    sequencer.HandleDialogue(new List<(string,string)>(){
                        ("Simone", "Poor soul� Wait, what�s this? A saber! Andy, CATCH!"),
                        ("Andrea", "Good job sis, now we�re talking! I will carve into these creeps an even bigger mouth.")
                    });
                    if (sequencer.Vars.IsStringSet("_dialogue_1") && !sequencer.Vars.IsStringSet("_equpped"))
                    {
                        missionUI.Log("EQUIPPING");
                        ActorManager.Instance.ActorEquipAction("Andrea", RoaringStarActions.GetSabreSwingAction(1));
                        sequencer.Vars.SetString("_equipped", "");
                        sequencer.ActorPlaySoundOnce(ActorManager.Instance.GetActorByName("Andrea"), "female_warrior_cheer");
                    }
                }},
                {"red_search_skeleton", (MissionUI missionUI, Sequencer sequencer, SceneObjects sceneObjects, float deltaTime) => {
                    //if (!sequencer.IsStringSet("_dialogue_" + 0))
                    sequencer.HandleDialogue(new List<(string,string)>(){
                        ("Red", "What a wretch� You, my dead fellow, are the bare bones of a cautionary tale."),
                        ("Red", "Hold on, what do we have here? Flintlock pistol, as good as new. And ammo as well! And I am famous for my aim is what I am.")
                    });
                    if (sequencer.Vars.IsStringSet("_dialogue_1") && !sequencer.Vars.IsStringSet("_equpped"))
                    {
                        ActorManager.Instance.ActorEquipAction("Red", RoaringStarActions.GetSnipeAction(1));
                        sequencer.Vars.SetString("_equipped", "");
                        sequencer.ActorPlaySoundOnce(ActorManager.Instance.GetActorByName("Red"), "female_high_yes");
                    }
                }},
                {"simone_blinked_first_time", (MissionUI missionUI, Sequencer sequencer, SceneObjects sceneObjects, float deltaTime) =>{
                    sequencer.HandleDialogue(new List<(string,string)>() {
                        ("Simone", "What... Why am I here?"),
                        ("Andrea", "Simone! Did you just freaking teleported?"),
                        ("Red", "By the Roaring Star� That little brat is a witch! I always knew it!"),
                        ("Simone", "I didn�t do anything, I swear!")
                    });
                }}
            }
            );
    }

    /**
     * Mission 2: Hideaway Planet
     */

    private Dictionary<string, MissionActorBlueprint> Mission2_Actors()
    {
        Dictionary<string, MissionActorBlueprint> actors = new Dictionary<string, MissionActorBlueprint>();

        // Simona
        CharacterStats simonaStats = new CharacterStats();
        simonaStats.Actions = new List<Action>() { Action.Move };
        simonaStats.PerBattleResources = new CounterDict<Resource>() { };
        simonaStats.PerTurnResources = new CounterDict<Resource>() { { Resource.MoveAction, 1 }, { Resource.PrimaryAction, 1 } };
        actors.Add("Simone", new MissionActorBlueprint(
            "Simone", 18,
            new CharacterPresentation(
                "Simone",
                "testsimone",
                "Simone_thumb",
                "OrbSimone",
                2f, 4f,
                null,
                "Placeholder/graphics-sprites-WOAsprite"),
            simonaStats,
            Team.Human,
            0));

        // Andrea
        CharacterStats andreaStats = new CharacterStats();
        andreaStats.Actions = new List<Action>() { Action.Move, RoaringStarActions.GetTankAction(1) };
        andreaStats.PerBattleResources = new CounterDict<Resource>() { };
        andreaStats.PerTurnResources = new CounterDict<Resource>() { { Resource.MoveAction, 1 }, { Resource.PrimaryAction, 1 } };
        actors.Add("Andrea", new MissionActorBlueprint(
            "Andrea", 48,
            new CharacterPresentation(
                "Andrea",
                "testportrait",
                "Andrea_thumb",
                "OrbAndrea",
                2f, 4f,
                null,
                "Placeholder/graphics-sprites-WOAsprite"),
            andreaStats,
            Team.Human,
            0));

        // Red
        CharacterStats redStats = new CharacterStats();
        redStats.Actions = new List<Action>() { Action.Move, RoaringStarActions.GetFleeAction(1) };
        redStats.PerBattleResources = new CounterDict<Resource>() { };
        redStats.PerTurnResources = new CounterDict<Resource>() { { Resource.MoveAction, 1 }, { Resource.PrimaryAction, 1 } };
        actors.Add("Red", new MissionActorBlueprint(
            "Red", 28,
            new CharacterPresentation(
                "Red",
                "testred",
                "Red_thumb",
                "OrbAndrea",
                2f, 4f,
                null,
                "Placeholder/graphics-sprites-WOAsprite"),
            redStats,
            Team.Human,
            0));

        return actors;
    }

    private SceneSequenceData Mission2_SceneSequences()
    {
        return new SceneSequenceData(
            new Dictionary<string, System.Action<MissionUI, Sequencer, SceneObjects, float>>()
            {
                {"player_character_died", (MissionUI missionUI, Sequencer sequencer, SceneObjects sceneObjects, float deltaTime) =>{
                    string deadCharacter = sequencer.MissionVars.GetString("_dead_character");
                    sequencer.HandleDialogue(new List<(string,string)>() {
                        ("Simone", "No, "+deadCharacter+"! This can't be happpening... This did NOT happen!!!"),
                        ("Simone", "* TODO: insert some short but epic screen-wide blast, as Simone alters reality *")
                    }, "mission_failed");
                }},
                {"mission_failed", (MissionUI missionUI, Sequencer sequencer, SceneObjects sceneObjects, float deltaTime) => {
                    sceneObjects.RestartMission();
                } },
                {"intro", (MissionUI missionUI, Sequencer sequencer, SceneObjects sceneObjects, float deltaTime) => {
                    sequencer.HandleDialogue(new List<(string,string)>(){
                        ("Andrea", "Uh oh...")
                    });
                }},
                {"outro", (MissionUI missionUI, Sequencer sequencer, SceneObjects sceneObjects, float deltaTime) =>{
                    sequencer.HandleDialogue(new List<(string,string)>() {
                        ("Andrea", "We done, ye.")
                    });
                }},
            });
    }
}
