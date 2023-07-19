using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GRPG.GameLogic;

//TODO inherit from this
public class PlayerTurnUIState : UIState
{
    public readonly MissionUI missionUI;
    public readonly SceneObjects sceneObjects;

    public PlayerTurnUIState(MissionUI ui, SceneObjects objects)
    {
        missionUI = ui;
        sceneObjects = objects;
    }

    public override void ContinueButtonClicked()
    {
        //throw new System.NotImplementedException();
    }

    public override void Enter()
    {
        sceneObjects.DisableLocationColliders();
        sceneObjects.DisableActorColliders();
        sceneObjects.HideAllLocations();
        missionUI.SetShowCharacterPanel(false);
        missionUI.HideDialoguePanel();

        if (MissionData.Instance.SceneSequences.Sequences.ContainsKey("intro"))
        {
            missionUI.SetSceneSequenceUIState("intro");
            MissionData.Instance.SceneSequences.Sequences.Remove("intro");
        } else
        {
            HumanPlayer.Instance.InitMove();
        }
    }

    public override void Exit()
    {
        // Disable controls
    }

    public override void OnMouseEnter()
    {
        //
    }

    public override void OnMouseExit()
    {
        //
    }

    public override void LeftButtonClicked()
    {
        //
    }

    public override void RightButtonClicked()
    {
        //throw new System.NotImplementedException();
    }

    public override void SkipButtonClicked()
    {
        //throw new System.NotImplementedException();
    }
}
