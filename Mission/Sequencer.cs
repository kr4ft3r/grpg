using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GRPG.GameLogic;

/// <summary>
/// Do not try to understand or maintain this class. And show some respect, it has more responsibilities than you ever will.
/// </summary>
public class Sequencer : MonoBehaviour
{
    public SceneObjects SceneObjects;
    public MissionUI MissionUI;

    /// <summary>
    /// Sequence variables that persist only during single sequence
    /// </summary>
    public Vars Vars { get; private set; }
    /// <summary>
    /// Seqence variables that persist throughout mission
    /// </summary>
    public Vars MissionVars { get; private set; }

    private List<ActionResult> _results;
    private Dictionary<string, Vector3> _positionVars;
    private Dictionary<string, string> _stringVars;
    public ActionResult FinishedResult = null;
    public float TimeElapsed;
    public ActionResult RunningResult;

    public string SuccessSoundPath;
    public string FailSoundPath;
    public string PerformSoundPath;

    private void Awake()
    {
        _results = new List<ActionResult>();
        _positionVars = new Dictionary<string, Vector3>();
        _stringVars = new Dictionary<string, string>();
        Vars = new Vars();
        MissionVars = new Vars();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (RunningSceneSequence != null)
        {
            if (SceneSequenceFinished)
            {
                RunningSceneSequence = null;
                MissionUI.Skip();
                return;
            }
            RunningSceneSequence(MissionUI, this, SceneObjects, Time.deltaTime);

            return;
        }

        HandleResultsSequence();
    }

    void HandleResultsSequence()
    {
        if (_results.Count == 0) return;

        if (FinishedResult == null) // Result still running
        {
            if (RunningResult == null) // Init new result handling
            {
                RunningResult = _results[0];
                if (RunningResult.Actor.Team == Team.AI) MissionUI.SetActiveActor(RunningResult.Actor); // run active actor info & cosmetics for AI
                TimeElapsed = 0;
                Vars = new Vars();
            }

            HandleResultFrame(_results[0]);
            if (FinishedResult == RunningResult && _results.Count == 1) // That was last one
            {
                if (_sceneSequenceAfterAction != null) // override behavior, this result will run scene sequence before returning control
                {
                    MissionUI.SetSceneSequenceUIStateWithReturnedControl(_sceneSequenceAfterAction);
                    _sceneSequenceAfterAction = null;
                }
                //Dispatch
                else if (RunningResult.Actor.Team == Team.AI)
                    ComputerPlayer.Instance.SequencesDone();
                else if (RunningResult.Actor.Team == Team.Human)
                    HumanPlayer.Instance.SequencesDone(); // TODO Good candidate for polymorph
            }
        }
        else // Switch to new result next frame
        {
            _results.RemoveAt(0);
            FinishedResult = null;
            RunningResult = null;
        }
    }

    void HandleResultFrame(ActionResult result)
    {
        ActionPresentationData presentation = GameActions.Presentations[result.Action.Name]; //TODO cache this
        PerformSoundPath = presentation.performSound;
        SuccessSoundPath = presentation.successSound;
        FailSoundPath = presentation.failSound;

        // Run update callback if presentation has it
        if (presentation.ResultSequenceUpdate != null)
        {
            presentation.ResultSequenceUpdate(this, result, SceneObjects, Time.deltaTime);
            return;
        }

        // Handling of actions without update callback in presentation:
        if (result.IsSuccess && presentation.successSound != null)
            ActorPlaySound(result.Actor, presentation.successSound);
        else if (!result.IsSuccess && presentation.failSound != null)
            ActorPlaySound(result.Actor, presentation.successSound);

        switch (result.Action.Name)
        {
            default: // Skip
                ShowResultAction(); //default
                FinishedResult = result;

                break;
        }
    }

    public System.Action<MissionUI, Sequencer, SceneObjects, float> RunningSceneSequence;
    public bool SceneSequenceFinished;
    private string _sceneSequenceAfterAction;

    public void SetSceneSequenceAfterAction(string sequenceName)
    {
        _sceneSequenceAfterAction = sequenceName;
    }

    public void RunSceneSequence(System.Action<MissionUI, Sequencer, SceneObjects, float> sequence)
    {
        TimeElapsed = 0;
        Vars = new Vars();
        RunningSceneSequence = sequence;
        SceneSequenceFinished = false;
    }

    private int _dialogueIndex = 0;

    public void HandleDialogue(List<(string,string)> dialogue, string nextSceneSequence = null)
    {
        
        if (dialogue.Count == 0 || _dialogueIndex >= dialogue.Count)
        {
            _dialogueIndex = 0;
            Vars.SetString("_dialogue_" + "end", "");
            MissionUI.HideDialoguePanel();
            if (nextSceneSequence == null) // No next sequence, but end this one
            {
                RunningSceneSequence = null;
                SceneSequenceFinished = false;
                MissionUI.Skip();
                return;
            } else if (nextSceneSequence == "continue") { // No next sequence, but don't end this one
                //TODO
            } else { // Start next sequence
                RunningSceneSequence = null;
                SceneSequenceFinished = false;
                SceneObjects.StartSceneSequence(nextSceneSequence);
                return;
            }
        }

        if (!Vars.IsStringSet("_dialogue_" + _dialogueIndex))
        {
            Vars.SetString("_dialogue_" + _dialogueIndex, "");
            MissionUI.ShowDialogueWindow(
                SceneObjects.CharacterPresentations[dialogue.ElementAt(_dialogueIndex).Item1],
                dialogue.ElementAt(_dialogueIndex).Item2);
            //no _dialogueIndex++;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            _dialogueIndex++;
        }

    }

    public void ContinueDialogue()//TODO LINK this up
    {
        _dialogueIndex++;
    }

    public void SkipDialogue()
    {
        _dialogueIndex = 1000;
    }

    public void AddResult(ActionResult result)
    {
        _results.Add(result);
    }

    // INFO

    public void ShowResultAction()
    {
        if (Vars.IsStringSet("_show_action")) return;
        if (RunningResult == null) return;
        SceneObjects.GetActorGO(RunningResult.Actor).GetComponent<SceneActor>().AddInfoText(
            RunningResult.Action.Name
            );
        Vars.SetString("_show_action", "");
    }

    // SFX: sounds

    public void ActorPlaySoundOnce(Actor actor, string sound, float volumeScale = 1.0f)
    {//TODO better do init callback
        if (Vars.IsStringSet("_play_sound_" + sound)) return;

        Vars.SetString("_play_sound_" + sound, sound);
        ActorPlaySound(actor, sound, volumeScale);
    }

    public void ActorPlaySound(Actor actor, string sound, float volumeScale = 1.0f)
    {
        GameObjectPlaySound(SceneObjects.GetActorGO(actor), sound, volumeScale);
    }

    /// <summary>
    /// Can be run only once
    /// </summary>
    public void ActionPerformerPlayDefaultSound()
    {
        if (PerformSoundPath != null)
            ActorPlaySoundOnce(RunningResult.Actor, PerformSoundPath);
    }

    /// <summary>
    /// Can be run only once
    /// </summary>
    public void ActionPerformerPlaySuccessSound()
    {
        if (SuccessSoundPath != null)
            ActorPlaySoundOnce(RunningResult.Actor, SuccessSoundPath);
    }

    /// <summary>
    /// Can be run only once
    /// </summary>
    public void ActionPerformerPlayFailSound()
    {
        if (FailSoundPath != null)
            ActorPlaySoundOnce(RunningResult.Actor, FailSoundPath);
    }

    public void GameObjectPlaySound(GameObject go, string sound, float volumeScale = 1.0f)
    {
        AudioClip audio = Resources.Load<AudioClip>("Sounds/" + sound);
        try
        {
            go.GetComponent<AudioSource>().PlayOneShot(audio, volumeScale);
        } catch (System.Exception e)
        {
            Debug.LogWarning(e.Message + " " + e.StackTrace);
        }
    }

    // SFX: anims

    /// <summary>
    /// Handles attacker's pingpong and victim's knockback
    /// </summary>
    /// <param name="success"></param>
    /// <param name="timeElapsed"></param>
    /// <param name="attackDuration"></param>
    /// <param name="knockbackStartPercent">Multiplier of attackDuraction (ie 0.5f)</param>
    /// <param name="knockbackStrength"></param>
    /// <param name="attackerStart"></param>
    /// <param name="victimStart"></param>
    /// <returns></returns>
    public (Vector3 attackerPos, Vector3 victimPos, bool knockbackStarted, bool done) 
        AnimAttackWithKnockback(bool success,
        float timeElapsed, float attackDuration, float knockbackStartPercent, float knockbackStrength,
        Vector3 attackerStart, Vector3 victimStart)
    {
        Vector3 attackerPos, victimPos;
        bool knockbackStarted = false;
        bool done = false;
        Vector3 knockbackPos = victimStart +
                        ((victimStart - attackerStart).normalized * knockbackStrength);

        if (timeElapsed < attackDuration)
        {
            float attackPingPong = Mathf.PingPong((timeElapsed / attackDuration) * 2f, 1.0f);
            attackerPos = Vector3.Lerp(attackerStart, victimStart, attackPingPong);
            victimPos = victimStart;
            // Knockback
            if (success && timeElapsed >= attackDuration * knockbackStartPercent)
            {
                knockbackStarted = true;
                float knockbackPingPong = Mathf.PingPong((timeElapsed / attackDuration) * (2*attackDuration*knockbackStartPercent*10) /*4f*/, 1.0f);
                victimPos = Vector3.Lerp(victimStart, knockbackPos, knockbackPingPong);
            }
        } else
        {
            done = true;
            attackerPos = attackerStart;
            victimPos = victimStart;
        }

        return (attackerPos, victimPos, knockbackStarted, done);
    }

    /// <summary>
    /// Just victim knockback
    /// </summary>
    /// <param name="success"></param>
    /// <param name="timeElapsed"></param>
    /// <param name="attackDuration"></param>
    /// <param name="knockbackStartPercent">Multiplier of attackDuraction (ie 0.5f)</param>
    /// <param name="knockbackStrength"></param>
    /// <param name="attackerStart"></param>
    /// <param name="victimStart"></param>
    /// <returns></returns>
    public (Vector3 victimPos, bool knockbackStarted, bool done)
        AnimKnockback(bool success,
        float timeElapsed, float attackDuration, float knockbackStartPercent, float knockbackStrength,
        Vector3 attackerStart, Vector3 victimStart)
    {
        Vector3 attackerPos, victimPos;
        bool knockbackStarted = false;
        bool done = false;
        Vector3 knockbackPos = victimStart +
                        ((victimStart - attackerStart).normalized * knockbackStrength);

        if (timeElapsed < attackDuration)
        {
            victimPos = victimStart;
            // Knockback
            if (success && timeElapsed >= attackDuration * knockbackStartPercent)
            {
                knockbackStarted = true;
                float knockbackPingPong = Mathf.PingPong((timeElapsed / attackDuration) * (2*attackDuration*knockbackStartPercent*10)/*4f*/, 1.0f);
                victimPos = Vector3.Lerp(victimStart, knockbackPos, knockbackPingPong);
            }
        }
        else
        {
            done = true;
            attackerPos = attackerStart;
            victimPos = victimStart;
        }

        return (victimPos, knockbackStarted, done);
    }


}
