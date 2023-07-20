using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GRPG.GameLogic;

public class MissionMain : MonoBehaviour
{
    /// <summary>
    /// Ordered by location Index
    /// </summary>
    public SceneLocation[] Locations { get; private set; }

    private Mission _mission;
    private Dictionary<string, Actor> _actors;

    void Start()
    {
        GameObject.Instantiate(Resources.Load<Canvas>("UI/MissionCanvas"));
        _mission = new Mission(LoadLocations());
        MissionUI missionUI = GameObject.Find("Mission").GetComponent<MissionUI>();
        Sequencer sequencer = GameObject.Find("Mission").GetComponent<Sequencer>();
        sequencer.MissionUI = missionUI;
        GameObject characterPanel = GameObject.Find("ActiveCharacterPanel");
        GameObject dialoguePanel = GameObject.Find("DialoguePanel");
        SceneObjects sceneObjects = new SceneObjects(sequencer, characterPanel, dialoguePanel);
        MissionResources missionResources = new MissionResources(_mission, sceneObjects, Locations);
        LoadResources(sceneObjects);
        _actors = missionResources.LoadActors(sceneObjects);
        ActorManager actorManager = new ActorManager(_actors);
        SceneSequenceData sequencesData = missionResources.LoadSceneSequences();

        new HumanPlayer(_mission, Team.Human, sceneObjects, missionUI);
        new ComputerPlayer(_mission, Team.AI, sceneObjects, missionUI);

        missionUI.Init(_mission, sceneObjects);

        _mission.AfterActionPerformed += missionUI.LogActionResult;
        _mission.AfterActionPerformed += sceneObjects.ApplyTestActionResult;

        _mission.PostActorIsDamaged += missionUI.LogActorDamaged;
        _mission.PostActorIsDamaged += sceneObjects.OnPostActorIsDamaged;

        _mission.AfterActorMoves += HandleLocationTriggers;
        _mission.ActorWasDowned += sceneObjects.OnActorWasKilled;

        actorManager.ActorGainedAction += missionUI.UpdateCharacterAction;
        actorManager.ActorLostAction += missionUI.UpdateCharacterAction;

        _mission.Start();

        ///sceneObjects.FixActorPositions();

        if (_mission.CurrentTeam == Team.Human) //TODO HC 
        {
            //HumanPlayer.Instance.InitMove();
        } else
        {
            ComputerPlayer.Instance.RunMoves();
        }

        MusicManager musicManager = GameObject.Find("Game").GetComponent<MusicManager>();
        musicManager.NextSong = musicManager.SongRaindropGentle;

        //StartCoroutine(TestMoves());
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        DrawDebug();
    }

    //TODO move all the trah from below to a better place

    string[,] LoadLocations()
    {
        GameObject[] scnEditorLocationGOs = GameObject.FindGameObjectsWithTag("Location");
        int numNodes = scnEditorLocationGOs.Length;
        string[,] graph = new string[numNodes, numNodes];

        var query = from loc in scnEditorLocationGOs // Get script components sorted by Index
                    orderby loc.GetComponent<SceneEditorLocation>().Index
                    select loc.GetComponent<SceneEditorLocation>();
        SceneEditorLocation[] editorLocations = query.ToArray<SceneEditorLocation>();
        Locations = editorLocations.Select(el => el.gameObject.AddComponent<SceneLocation>()).ToArray();
        for (int i = 0; i < Locations.Length; i++)
        {
            Locations[i].Index = editorLocations[i].Index;
            Locations[i].Connections = editorLocations[i].Connections;
            if (editorLocations[i].LocationAction != "")
            {
                if (MissionData.Instance.LocationActions.ContainsKey(editorLocations[i].LocationAction))
                {
                    Locations[i].LocationAction = MissionData.Instance.LocationActions[editorLocations[i].LocationAction];
                    Locations[i].ActionAvailability = editorLocations[i].ActionAvailability;
                    Locations[i].ActionDisabled = editorLocations[i].ActionDisabled;
                }
                else
                {
                    Debug.LogWarning("Mission data doesn't contain location action: " + editorLocations[i].LocationAction);
                }
            }
            
            //
            Destroy(Locations[i].GetComponent<SceneEditorLocation>());
        }

        for (int i = 0; i < numNodes; i++)
        {
            for (int y = 0; y < numNodes; y++)
            {
                graph[i,y] = Locations[i].Connections[y];
            }
        }

        return graph;
    }

    public void LoadResources(SceneObjects sceneObjects)
    {
        // Action presentations
        sceneObjects.ActionPresentations.Add(
            Action.Move.Name, 
            new ActionPresentationData(
            "IconMoveMaterial",
            "MoveIcon",
            (Sequencer sequencer, ActionResult result, SceneObjects sceneObjects, float deltaTime) => {

                float lerpDuration = 0.5f;

                if (result.IsSuccess)
                {
                    if ((sequencer.TimeElapsed / lerpDuration) >= 0.1f && !sequencer.Vars.IsStringSet("step1"))
                    {
                        sequencer.ActorPlaySound(result.Actor, sequencer.PerformSoundPath, .4f);
                        sequencer.Vars.SetString("step1", "1");
                    } else if ((sequencer.TimeElapsed / lerpDuration) >= 0.4f && !sequencer.Vars.IsStringSet("step2"))
                    {
                        sequencer.ActorPlaySound(result.Actor, sequencer.PerformSoundPath, .3f);
                        sequencer.Vars.SetString("step2", "2");
                    } else if ((sequencer.TimeElapsed / lerpDuration) >= 0.7f && !sequencer.Vars.IsStringSet("step3"))
                    {
                        sequencer.ActorPlaySound(result.Actor, sequencer.PerformSoundPath, .15f);
                        sequencer.Vars.SetString("step3", "3");
                    }
                } else
                {
                    //playsoundonce some funny fail
                }

                GameObject actorGO = sceneObjects.GetActorGO(result.Actor);
                Vector3 locationPos = sceneObjects.GetLocationSlotPositionByIndex(result.Target.Location);
                // TODO LerpTo function
                Vector3 lerpedPos;
                if (!sequencer.Vars.IsVectorSet("actorStart"))
                {
                    sequencer.Vars.SetVector("actorStart", actorGO.transform.position);
                }
                if (sequencer.TimeElapsed < lerpDuration)
                {
                    actorGO.GetComponent<SceneActor>().IsWalking = true;
                    lerpedPos = Vector3.Lerp(sequencer.Vars.GetVector("actorStart"), locationPos, sequencer.TimeElapsed / lerpDuration);
                    sequencer.TimeElapsed += Time.deltaTime;
                }
                else
                { // Done
                    actorGO.GetComponent<SceneActor>().IsWalking = false;
                    lerpedPos = locationPos;
                    sequencer.FinishedResult = result;
                    sceneObjects.ActorGOArriveToLocation(actorGO, result.Target.Location);
                }
                actorGO.transform.position = lerpedPos;

            },
            "stepstone_1"
            ));

        sceneObjects.ActionPresentations.Add(
            Action.Punch.Name, 
            new ActionPresentationData(
            "IconPunchMaterial",
            "PrimaryIcon",
            (Sequencer sequencer, ActionResult result, SceneObjects sceneObjects, float deltaTime) =>
            {
                if (result.IsSuccess)
                {
                    sequencer.ActorPlaySoundOnce(result.Actor, "bigPunch");
                } else
                {
                    sequencer.ActorPlaySoundOnce(result.Actor, "woosh");
                }

                float animDuration = 0.4f;
                GameObject actorGO = sceneObjects.GetActorGO(result.Actor);
                GameObject targetGO = sceneObjects.GetActorGO(result.Target.Actor);

                if (!sequencer.Vars.IsVectorSet("actorStart"))
                {
                    sequencer.Vars.SetVector("actorStart", actorGO.transform.position);
                    sequencer.ShowResultAction();
                    sequencer.Vars.SetVector("targetPos", targetGO.transform.position);
                }

                (Vector3 attackerPos, Vector3 victimPos, bool knockbackStarted, bool done) 
                = sequencer.AnimAttackWithKnockback(
                    result.IsSuccess, sequencer.TimeElapsed, 
                    animDuration, .5f, 1f, 
                    sequencer.Vars.GetVector("actorStart"), sequencer.Vars.GetVector("targetPos") 
                    );

                actorGO.transform.position = attackerPos;
                targetGO.transform.position = victimPos;
                if (knockbackStarted)
                {
                    if (result.IsSuccess) {
                        if (sequencer.MissionUI.GetActiveTeam() == Team.Human) 
                            targetGO.GetComponentInChildren<SpriteRenderer>().color = Color.yellow;
                        else
                            targetGO.GetComponentInChildren<SpriteRenderer>().color = Color.red;
                    }
                }
                if (done)
                {
                    targetGO.GetComponentInChildren<SpriteRenderer>().color = Color.white;
                    sequencer.FinishedResult = result;
                }

                sequencer.TimeElapsed += Time.deltaTime;

            },
            null,
            "bigPunch",
            "woosh"
            ));

        /*  Flee 1 presentation
         */

        sceneObjects.ActionPresentations.Add(
            RoaringStarActions.GetFleeAction(1).Name,
            new ActionPresentationData(
            "IconFlee1Material",
            ActionPresentationData.IconSlotSecondary,
            (Sequencer sequencer, ActionResult result, SceneObjects sceneObjects, float deltaTime) =>
            {
                float lerpDuration = 0.5f;

                if (result.IsSuccess)
                {
                    //playsoundonce flee
                    sequencer.ActorPlaySoundOnce(result.Actor, "woosh");
                }
                else
                {
                    //playsoundonce some funny fail
                }

                GameObject actorGO = sceneObjects.GetActorGO(result.Actor);
                Vector3 locationPos = sceneObjects.GetLocationSlotPositionByIndex(result.Target.Location);
                // TODO LerpTo function
                Vector3 lerpedPos;
                /*Quaternion*/ float lerpedRotation = 0;
                float lerpedHeight = 0;
                if (!sequencer.Vars.IsVectorSet("actorStart"))
                {
                    sequencer.Vars.SetVector("actorStart", actorGO.transform.position);
                }
                if (sequencer.TimeElapsed < lerpDuration)
                {
                    //actorGO.GetComponent<SceneActor>().IsWalking = true;
                    if (result.IsSuccess)
                    {
                        lerpedPos = Vector3.Lerp(sequencer.Vars.GetVector("actorStart"), locationPos, sequencer.TimeElapsed / lerpDuration);
                        lerpedRotation = Mathf.Lerp(0, 360, sequencer.TimeElapsed / lerpDuration);
                        //startPos.y + Mathf.Sin( Mathf.PI * 2 * counter / 360)
                        lerpedHeight = Mathf.PingPong(sequencer.TimeElapsed / lerpDuration, 10f);  //Mathf.Sin((sequencer.TimeElapsed / lerpDuration) * 100/*Mathf.PI * 2 * (sequencer.TimeElapsed / lerpDuration) * 3600*/);
                    }
                    else
                    {
                        lerpedPos = Vector3.Lerp(sequencer.Vars.GetVector("actorStart"), locationPos,
                        Mathf.PingPong(sequencer.TimeElapsed, 100f));
                    }
                    //Salto rotation
                    //TODO sprite instead
                    /*actorGO.transform.Find("Collider").transform.localPosition = new Vector3(
                        0, 
                        3f + Mathf.Sin(sequencer.TimeElapsed / lerpDuration)*3, 
                        0
                        );*/
                    //actorGO.transform.Find("Collider").transform.localRotation = Quaternion.Euler(sequencer.TimeElapsed*600f, 0, 0);
                    //actorGO.GetComponent<SceneActor>().SpriteRotation = lerpedRotation;
                    actorGO.GetComponent<SceneActor>().SpriteElevation = Mathf.Sin(lerpedHeight * Mathf.PI);

                    sequencer.TimeElapsed += Time.deltaTime;
                }
                else
                { // Done
                    //actorGO.GetComponent<SceneActor>().IsWalking = false;
                    if (result.IsSuccess)
                    {
                        lerpedPos = locationPos;
                        actorGO.GetComponent<SceneActor>().SpriteElevation = 0;
                        sceneObjects.ActorGOArriveToLocation(actorGO, result.Target.Location);
                    } else
                    {
                        lerpedPos = sequencer.Vars.GetVector("actorStart");
                    }

                    sequencer.FinishedResult = result;
                }
                actorGO.transform.position = lerpedPos;

            },
            null,
            "woosh",
            "woosh"
            ));

        sceneObjects.ActionPresentations.Add(
            RoaringStarActions.GetSnipeAction(1).Name,
            new ActionPresentationData(
                "IconShootMaterial",
                ActionPresentationData.IconSlotPrimary,
                (Sequencer sequencer, ActionResult result, SceneObjects sceneObjects, float deltaTime) => {
                    sequencer.ActionPerformerPlayDefaultSound();
                    if (sequencer.TimeElapsed >= 0.2f)
                    {
                        if (result.IsSuccess)
                        {
                            sequencer.ActionPerformerPlaySuccessSound();
                        }
                        else
                        {
                            sequencer.ActionPerformerPlayFailSound();
                        }
                    }

                    float animDuration = 0.4f;

                    GameObject actorGO = sceneObjects.GetActorGO(result.Actor);
                    GameObject targetGO = sceneObjects.GetActorGO(result.Target.Actor);

                    if (!sequencer.Vars.IsStringSet("_smoke"))
                    {
                        sequencer.Vars.SetString("_smoke", "");
                        Vector3 smokeDir = (targetGO.transform.position - actorGO.transform.position).normalized;
                        Vector3 smokePos = actorGO.transform.position + (smokeDir * 4f) + (Vector3.up * 4);
                        GameObject smoke = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ParticleSprite"), smokePos, Quaternion.identity);
                        smoke.transform.localScale += new Vector3(50f, 50f, 1);
                        ParticleSpriteScript ptrSpr = smoke.GetComponent<ParticleSpriteScript>();
                        ptrSpr.Sprites = Resources.LoadAll<Sprite>("Sprites/Effects/SmokeAndFire");
                        ptrSpr.AnimFrom = 0;
                        ptrSpr.AnimTo = 7;
                        ptrSpr.lifeTime = .7f;
                        ptrSpr.moveDirection = smokeDir * 2f;
                        ptrSpr.moveDirectionLength = .5f;
                        ptrSpr.moveStartPos = smokePos;
                    }

                    if (!sequencer.Vars.IsVectorSet("actorStart"))
                    {
                        sequencer.Vars.SetVector("actorStart", actorGO.transform.position);
                        sequencer.ShowResultAction();
                        sequencer.Vars.SetVector("targetPos", targetGO.transform.position);
                    }

                    (Vector3 victimPos, bool knockbackStarted, bool done)
                    = sequencer.AnimKnockback(
                        result.IsSuccess, sequencer.TimeElapsed,
                        animDuration, .5f, 1f,
                        sequencer.Vars.GetVector("actorStart"), sequencer.Vars.GetVector("targetPos")
                        );

                    targetGO.transform.position = victimPos;
                    if (knockbackStarted)
                    {
                        if (result.IsSuccess)
                        {
                            if (sequencer.MissionUI.GetActiveTeam() == Team.Human)
                                targetGO.GetComponent<SceneActor>().SetSpriteColorWhite();
                            //targetGO.GetComponentInChildren<SpriteRenderer>().color = Color.black;
                            else
                                targetGO.GetComponent<SceneActor>().SetSpriteColor(Color.red);
                                //targetGO.GetComponentInChildren<SpriteRenderer>().color = Color.red;
                        }
                    }
                    if (done)
                    {
                        targetGO.GetComponent<SceneActor>().SetSpriteColor(Color.white);
                        sequencer.FinishedResult = result;
                    }

                    sequencer.TimeElapsed += Time.deltaTime;
                },
                "explosion",
                "sabreHit", //TODO temp
                "woosh", //TODO temp
                1f
                )
            );

        /* Tank1 presentation
         */

        sceneObjects.ActionPresentations.Add(
            RoaringStarActions.GetTankAction(1).Name,
            new ActionPresentationData(
            "IconTank1Material",
            ActionPresentationData.IconSlotSecondary,
            null,
            null,
            "female_tank",
            "woosh"
            ));

        /*  Sabre1 presentation
         */
        sceneObjects.ActionPresentations.Add(RoaringStarActions.GetSabreSwingAction(1).Name,
            new ActionPresentationData(
                "IconSaberMaterial",
                ActionPresentationData.IconSlotPrimary,
                (Sequencer sequencer, ActionResult result, SceneObjects sceneObjects, float deltaTime) => {
                    if (result.IsSuccess)
                    {
                        sequencer.ActorPlaySoundOnce(result.Actor, "sabreHit");
                    }
                    else
                    {
                        sequencer.ActorPlaySoundOnce(result.Actor, "woosh");
                    }

                    float animDuration = 0.4f;
                    GameObject actorGO = sceneObjects.GetActorGO(result.Actor);
                    GameObject targetGO = sceneObjects.GetActorGO(result.Target.Actor);

                    if (!sequencer.Vars.IsVectorSet("actorStart"))
                    {
                        sequencer.Vars.SetVector("actorStart", actorGO.transform.position);
                        sequencer.ShowResultAction();
                        sequencer.Vars.SetVector("targetPos", targetGO.transform.position);
                    }

                    (Vector3 attackerPos, Vector3 victimPos, bool knockbackStarted, bool done)
                    = sequencer.AnimAttackWithKnockback(
                        result.IsSuccess, sequencer.TimeElapsed,
                        animDuration, .5f, 1f,
                        sequencer.Vars.GetVector("actorStart"), sequencer.Vars.GetVector("targetPos")
                        );

                    actorGO.transform.position = attackerPos;
                    targetGO.transform.position = victimPos;
                    if (knockbackStarted)
                    {
                        if (result.IsSuccess)
                        {
                            if (sequencer.MissionUI.GetActiveTeam() == Team.Human)
                                targetGO.GetComponent<SceneActor>().SetSpriteColorWhite();
                            else
                                targetGO.GetComponent<SceneActor>().SetSpriteColor(Color.red);
                        }
                    }
                    if (done)
                    {
                        targetGO.GetComponent<SceneActor>().SetSpriteColor(Color.white);
                        sequencer.FinishedResult = result;
                    }

                    sequencer.TimeElapsed += Time.deltaTime;
                },
                null,
                null,
                "woosh"
                )
            );

        /* Natural Blink presentation
         */

        sceneObjects.ActionPresentations.Add(
            RoaringStarActions.GetNaturalBlink().Name,
            new ActionPresentationData(
            null,
            null,
            (Sequencer sequencer, ActionResult result, SceneObjects sceneObjects, float deltaTime) => {
                sequencer.ActionPerformerPlayDefaultSound();
                /*if (result.IsSuccess)
                {
                    GameObject actorGO = sceneObjects.GetActorGO(result.Actor);
                    actorGO.transform.position = sceneObjects.GetLocationSlotPositionByIndex(result.Target.Location);
                    sceneObjects.ActorGOArriveToLocation(actorGO, result.Target.Location);
                }
                sequencer.FinishedResult = result;*/
                if (!sequencer.Vars.IsBoolSet("_setup_done"))
                {
                    sequencer.Vars.SetInt("_blink_stage", 0);
                    sequencer.Vars.SetBool("_setup_done", true);
                }
                int blinkStage = sequencer.Vars.GetInt("_blink_stage");
                GameObject actorGO = sceneObjects.GetActorGO(result.Actor);

                if (blinkStage == 0 && sequencer.TimeElapsed >= 0.1f)
                {
                    actorGO.GetComponent<SceneActor>().SetSpriteColor(Color.black);
                    sequencer.Vars.SetInt("_blink_stage", 1);
                }
                if (blinkStage == 1 && sequencer.TimeElapsed >= 0.3f)
                {
                    actorGO.GetComponent<SceneActor>().SetSpriteColorWhite();
                    sequencer.Vars.SetInt("_blink_stage", 2);
                }
                if (blinkStage == 2 && sequencer.TimeElapsed >= 0.5f)
                {
                    actorGO.transform.position = sceneObjects.GetLocationSlotPositionByIndex(result.Target.Location);
                    sceneObjects.ActorGOArriveToLocation(actorGO, result.Target.Location);
                    sequencer.Vars.SetInt("_blink_stage", 3);
                }
                if (blinkStage == 3 && sequencer.TimeElapsed >= 0.8f) {
                    actorGO.GetComponent<SceneActor>().SetSpriteColor(Color.white);
                    sequencer.Vars.SetInt("_blink_stage", 4);
                }
                if (blinkStage == 4)
                {
                    sequencer.ShowResultAction();
                    sequencer.FinishedResult = result;
                    if (!sequencer.MissionVars.IsBoolSet("_simone_blinked"))
                    {
                        sequencer.SetSceneSequenceAfterAction("simone_blinked_first_time");
                        sequencer.MissionVars.SetBool("_simone_blinked", true);
                    }
                }

                sequencer.TimeElapsed += deltaTime;

            },
            "shvicc",
            "woosh",
            "woosh"
            ));

        /*
         * Mission Location Actions
         */

        sceneObjects.ActionPresentations.Add(RoaringStarActions.GetSearchSkeletonSceneAction().Name,
            new ActionPresentationData(
                "IconSkullMaterial",
                ActionPresentationData.IconSlotSpecial,
                (Sequencer sequencer, ActionResult result, SceneObjects sceneObjects, float deltaTime) => {
                    sequencer.FinishedResult = result;
                    if (result.Actor.Name == "Red")
                        sequencer.SetSceneSequenceAfterAction("red_search_skeleton");
                    else if (result.Actor.Name == "Simone")
                        sequencer.SetSceneSequenceAfterAction("simone_search_skeleton");
                },
                null,
                null,
                null
                )
            );

        /*
         * Monster actions
         */

        // Smack
        sceneObjects.ActionPresentations.Add(
            RoaringStarActions.GetSmackAction().Name,
            new ActionPresentationData(
            "IconPunchMaterial",
            "PrimaryIcon",
            (Sequencer sequencer, ActionResult result, SceneObjects sceneObjects, float deltaTime) =>
            {
                if (result.IsSuccess)
                {
                    sequencer.ActorPlaySoundOnce(result.Actor, "bigPunch");
                }
                else
                {
                    sequencer.ActorPlaySoundOnce(result.Actor, "woosh");
                }
                if (result.DiceRoll == -1) // Must be Simone
                {
                    sequencer.FinishedResult = result;
                }

                float animDuration = 0.4f;
                GameObject actorGO = sceneObjects.GetActorGO(result.Actor);
                GameObject targetGO = sceneObjects.GetActorGO(result.Target.Actor);

                if (!sequencer.Vars.IsVectorSet("actorStart"))
                {
                    sequencer.Vars.SetVector("actorStart", actorGO.transform.position);
                    sequencer.ShowResultAction();
                    sequencer.Vars.SetVector("targetPos", targetGO.transform.position);
                }

                (Vector3 attackerPos, Vector3 victimPos, bool knockbackStarted, bool done)
                = sequencer.AnimAttackWithKnockback(
                    result.IsSuccess, sequencer.TimeElapsed,
                    animDuration, .5f, 1f,
                    sequencer.Vars.GetVector("actorStart"), sequencer.Vars.GetVector("targetPos")
                    );

                actorGO.transform.position = attackerPos;
                targetGO.transform.position = victimPos;
                if (knockbackStarted)
                {
                    if (result.IsSuccess)
                    {
                        if (sequencer.MissionUI.GetActiveTeam() == Team.Human)
                            targetGO.GetComponent<SceneActor>().SetSpriteColorWhite();
                        else
                            targetGO.GetComponent<SceneActor>().SetSpriteColor(Color.red);
                    }
                }
                if (done)
                {
                    targetGO.GetComponent<SceneActor>().SetSpriteColor(Color.white);
                    sequencer.FinishedResult = result;
                }

                sequencer.TimeElapsed += Time.deltaTime;

            },
            null,
            "bigPunch",
            "woosh"
            ));

        // Acid Lick
        sceneObjects.ActionPresentations.Add(RoaringStarActions.GetAcidLickAction().Name, new ActionPresentationData(
            null,
            ActionPresentationData.IconSlotSecondary,
            (Sequencer sequencer, ActionResult result, SceneObjects sceneObjects, float deltaTime) => {

                if (result.DiceRoll == -1) // Must be Simone
                {
                    sequencer.FinishedResult = result;
                }

                float animDuration = 2.0f;
                GameObject actorGO = sceneObjects.GetActorGO(result.Actor);
                GameObject targetGO = sceneObjects.GetActorGO(result.Target.Actor);
                Vector3 attackerPos;
                Vector3 victimPos;

                if (!sequencer.Vars.IsVectorSet("actorStart"))
                {
                    sequencer.Vars.SetVector("actorStart", actorGO.transform.position);
                    sequencer.ShowResultAction();
                    sequencer.Vars.SetVector("targetPos", targetGO.transform.position);
                    sequencer.Vars.SetVector("cringePos", targetGO.transform.position + Vector3.up);
                }

                victimPos = sequencer.Vars.GetVector("targetPos");
                if (sequencer.TimeElapsed < animDuration)
                {
                    attackerPos = Vector3.Lerp(sequencer.Vars.GetVector("actorStart"), sequencer.Vars.GetVector("targetPos"), 
                        Mathf.PingPong((sequencer.TimeElapsed / animDuration) * 2f, 1.0f));
                    if (sequencer.TimeElapsed >= .8f && result.IsSuccess)
                    {
                        sequencer.ActionPerformerPlayDefaultSound();
                        victimPos = Vector3.Lerp(sequencer.Vars.GetVector("targetPos"), sequencer.Vars.GetVector("cringePos"), 
                            Mathf.PingPong((sequencer.TimeElapsed / animDuration) * 16f, 1.0f + deltaTime*2f));
                        targetGO.GetComponentInChildren<SpriteRenderer>().color = new Color(0, 
                            100 + Mathf.PingPong((sequencer.TimeElapsed / animDuration * 2), 150f), 
                            0);
                    } else if (sequencer.TimeElapsed >= .8f && !result.IsSuccess)
                    {
                        sequencer.ActionPerformerPlayFailSound();
                    }
                    sequencer.TimeElapsed += deltaTime;
                } else
                {
                    attackerPos = sequencer.Vars.GetVector("actorStart");
                    victimPos = sequencer.Vars.GetVector("targetPos");
                    targetGO.GetComponentInChildren<SpriteRenderer>().color = Color.white;
                    sequencer.FinishedResult = result;
                }

                actorGO.transform.position = attackerPos;
                targetGO.transform.position = victimPos;

            },
            "acidlick",
            null,
            "acidlick_fail"
            ));

    }

    private void HandleLocationTriggers(Actor actor, int old, int location)
    {
        
        // Location actions usable by human player:

        // Arrived to location that gives action
        if (Locations[location].LocationAction != null && !Locations[location].ActionDisabled)
        {
            if (!actor.Stats.Actions.Contains(Locations[location].LocationAction))
            {
                bool actionAvailable = true;

                switch (Locations[location].ActionAvailability)
                {
                    case ActionAvailability.Once:
                        actionAvailable = !(Locations[location].LocationActionPerformerHistory.Count > 0);
                        break;
                    case ActionAvailability.OncePerCharacter:
                        actionAvailable = !(Locations[location].LocationActionPerformerHistory.Contains(actor));
                        break;
                }

                if (actionAvailable)
                {
                    ActorManager.Instance.ActorEquipAction(actor, Locations[location].LocationAction);
                }
            }
        }

        // Left a location that gives action
        if (Locations[old].LocationAction != null)
        {
            ActorManager.Instance.ActorUnequipAction(actor, Locations[old].LocationAction);
        }

    }


    void DrawDebug()
    {
        if (!Application.isEditor) return;

        for (int i = 0; i < _mission.NumLocations; i++)
        {
            for (int y = 0; y < _mission.NumLocations; y++)
            {
                Connection connection = _mission.Connections[i, y];
                
                if (connection.CanMove)
                {
                    
                    Color color = Color.yellow;
                    Debug.DrawLine(Locations[i].transform.position, Locations[y].transform.position, color);
                }
            }
        }
    }
}
