using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSequenceUIState : UIState
{
    private MissionUI _missionUI;
    private SceneObjects _sceneObjects;

    public AnimationSequenceUIState(MissionUI ui, SceneObjects objects)
    {
        _missionUI = ui;
        _sceneObjects = objects;
    }

    public override void ContinueButtonClicked()
    {
        //throw new System.NotImplementedException();
    }

    public override void Enter()
    {
        _sceneObjects.HideAllLocations();
        _sceneObjects.DisableLocationColliders();
        _sceneObjects.DisableActorColliders();
    }

    public override void Exit()
    {
        //
    }

    public override void LeftButtonClicked()
    {
        //
    }

    public override void OnMouseEnter()
    {
        //
    }

    public override void OnMouseExit()
    {
        //
    }

    public override void RightButtonClicked()
    {
        //
    }

    public override void SkipButtonClicked()
    {
        // 
    }
}
