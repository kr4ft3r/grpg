using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GRPG.GameLogic;

public class PlayerSelectedActionUIState : UIState
{
    private MissionUI _missionUI;
    private SceneObjects _sceneObjects;
    private Actor _activeActor;
    private Action _action;
    public PlayerSelectedActionUIState(MissionUI ui, SceneObjects objects, Actor activeActor, Action action)
    {
        Debug.Log(activeActor.Name + " selected " + action.Name);
        _missionUI = ui;
        _sceneObjects = objects;
        _activeActor = activeActor;
        _action = action;
    }

    public override void ContinueButtonClicked()
    {
        //throw new System.NotImplementedException();
    }

    public override void Enter()
    {
        _missionUI.Log(_activeActor.Name + " choosing target for " + _action);
        _missionUI.SetShowCharacterPanel(true);
        _missionUI.ActionCursorText = _action.Name;

        List<ActionTarget> targets = _activeActor.GetAvailableTargets(_action);
        List<int> targetLocations = targets.ConvertAll<int>(t => t.Location);
        List<Actor> targetActors = targets.ConvertAll<Actor>(t => t.Actor);

        Debug.Log(targets.Count + " possible target");

        _sceneObjects.HideAllLocations();
        _sceneObjects.DisableLocationColliders(targetLocations);
        _sceneObjects.DisableActorColliders(targetActors);
        _sceneObjects.ShowLocations(targetLocations);

        _uiTagFilter = "CharacterUI";
        //rly need better way to tell action's target type
        if (targetLocations.Count > 0 && targetLocations[0] >= 0)
            _aimingTagFilter = "Location";
        else if (targetActors.Count > 0 && targetActors[0] != null)
            _aimingTagFilter = "Actor";

        // Animate active action button
        _sceneObjects.CharacterPanel.transform
            .Find(_sceneObjects.ActionPresentations[_action.Name].iconSlot)
            .GetComponent<ActionButtonScript>().SetButtonActive(true);
        /*activeActionButton.GetComponent<Animator>()
            .SetBool("isActive", true);
        activeActionButton.GetComponent<ActionButtonScript>().Active = true;*/
    }

    public override void Exit()
    {
        _missionUI.ActionCursorText = "";
        _missionUI.HoverCursorText = "";
        _sceneObjects.HideAllLocations();
        GameObject activeActionButton = _sceneObjects.CharacterPanel.transform
            .Find(_sceneObjects.ActionPresentations[_action.Name].iconSlot).gameObject;
        activeActionButton.GetComponent<Animator>()
            .SetBool("isActive", false);
        activeActionButton.GetComponent<ActionButtonScript>().Active = false;
    }

    public override void LeftButtonClicked()
    {
        if (_aimedGO == null) return;

        Actor actorTarget = null;
        int locationTarget = -1;
        _activeActor.GetAvailableTargets(_action).ForEach(t => {
            if (t.Actor != null && _sceneObjects.GetActorGO(t.Actor) == _aimedGO)
                actorTarget = t.Actor;
            else if (t.Location >= 0 && _aimedGO.tag == "Location" && t.Location == _aimedGO.GetComponent<SceneLocation>().Index)
                locationTarget = t.Location;
        });

        ActionResult result = null;

        if (actorTarget != null)
        {
            result = _action.Perform(_activeActor, new ActionTarget(actorTarget));
        } else if (locationTarget >= 0)
        {
            result = _action.Perform(_activeActor, new ActionTarget(locationTarget));
        } else
        {
            Debug.Log("Something is fucked");
        }

        ////_missionUI.SetNextActionOrContinue();
        _missionUI.SetAnimationSequenceUIState();
    }

    public override void OnMouseEnter()
    {
        Debug.Log("AimedGO: " + _aimedGO.name);

        Actor actorTarget = null;
        int locationTarget = -1;
        _activeActor.GetAvailableTargets(_action).ForEach(t => {
            if (t.Actor != null && _sceneObjects.GetActorGO(t.Actor) == _aimedGO)
                actorTarget = t.Actor;
            else if (t.Location >= 0 && _aimedGO.tag == "Location" && t.Location == _aimedGO.GetComponent<SceneLocation>().Index)
                locationTarget = t.Location;
        });

        if (actorTarget != null)
        {
            _missionUI.HoverCursorText = _action.Name + " " + actorTarget.Name + "("+actorTarget.Resources[Resource.HitPoints]+"HP) (" +
                _action.GetSuccessChance(_activeActor, new ActionTarget(actorTarget)) + "%)";
        }
        else if (locationTarget >= 0)
        {
            _missionUI.HoverCursorText = _action.Name + " " + "here" + " (" +
                _action.GetSuccessChance(_activeActor, new ActionTarget(locationTarget)) + "%)";
        }
        else
        {
            _missionUI.HoverCursorText = "Something is fucked";
        }

        //_action.GetSuccessChance(_activeActor, _aimedGO.GetComponent<SceneActor>())
        //throw new System.NotImplementedException();
    }

    public override void OnMouseExit()
    {
        _missionUI.HoverCursorText = _action.Name;
        //throw new System.NotImplementedException();
    }

    public override void RightButtonClicked()
    {
        //throw new System.NotImplementedException();
    }

    public override void SkipButtonClicked()
    {
        //TODO next team member.. shouldn't be in UI class?
        _missionUI.SetNextActorOrEndTurn();
    }
}
