using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSequenceUIState : UIState
{

    private List<(CharacterPresentation, string)> _dialogueLines;

    private MissionUI _missionUI;
    private SceneObjects _sceneObjects;
    private System.Action<MissionUI, Sequencer, SceneObjects, float> _sequence;
    public UIState NextState { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="objects"></param>
    /// <param name="sequenceData"></param>
    /// <param name="nextState">Use for previous state to return to, or for end mission state</param>
    public SceneSequenceUIState(MissionUI ui, SceneObjects objects, System.Action<MissionUI, Sequencer, SceneObjects, float> sequence, UIState nextState)
    {
        _missionUI = ui;
        _sceneObjects = objects;
        _sequence = sequence;
        NextState = nextState;
    }

    public override void ContinueButtonClicked()
    {
        //throw new System.NotImplementedException();
    }

    public override void Enter()
    {
        _missionUI.SetShowCharacterPanel(false);
        _sceneObjects.HideAllLocations();
        _sceneObjects.DisableLocationColliders();
        _sceneObjects.DisableActorColliders();

        _sceneObjects.StartSceneSequence(_sequence);
    }

    public override void Exit()
    {
        
        //throw new System.NotImplementedException();
    }

    public override void LeftButtonClicked()
    {
        //throw new System.NotImplementedException();
    }

    public override void OnMouseEnter()
    {
        //throw new System.NotImplementedException();
    }

    public override void OnMouseExit()
    {
        //throw new System.NotImplementedException();
    }

    public override void RightButtonClicked()
    {
        //throw new System.NotImplementedException();
    }

    public override void SkipButtonClicked()
    {
        if (NextState == null)
        {
            HumanPlayer.Instance.SequencesDone();
            return;
        }
        _missionUI.UnsetSceneSequenceUIState(NextState);
        //throw new System.NotImplementedException();
    }
}
