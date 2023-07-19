using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using GRPG.GameLogic;

public class ActionButtonScript : MonoBehaviour
{
    public bool Active = false;
    public bool Available = false;
    public MissionUI MissionUI;
    public Action Action;
    public string ActionAvailabilityInfo;
    public string SpecialFunction;

    public void Click()
    {
        if (SpecialFunction == "SkipTurn")
        {
            HumanPlayer.Instance.SkipTurn();
            return;
        }
        if (Active || !Available || MissionUI == null || Action == null) { Debug.Log("NOPE:"+Active+Available+MissionUI+Action); return; }

        MissionUI.SetPlayerSelectedActionUIState(Action);
    }

    public void OnHover()
    {
        if (SpecialFunction == "SkipTurn")
        {
            MissionUI.HoverCursorText = "Skip Turn";
            GetComponent<Animator>().SetBool("isHovered", true);
            return;
        }

        //TODO info
        MissionUI.HoverCursorText = Action.Name + (Active ? " (selected)" : "") + " (" + ActionAvailabilityInfo + ")";

        if (Active) return;
        
        if (Available)
        {
            GetComponent<Animator>().SetBool("isHovered", true);
        }
    }

    public void OnHoverOut()
    {
        //TODO hide info

        //if (Active) return;
        MissionUI.HoverCursorText = "";
        
        GetComponent<Animator>().SetBool("isHovered", false);
    }

    public void SetButtonExists(bool exists, MissionUI missionUI)
    {
        this.MissionUI = missionUI;
        gameObject.SetActive(exists);
    }

    public void SetButtonAction(Action action, ActionPresentationData presentation)
    {
        Action = action;
        GetComponent<Image>().material = Resources.Load<Material>("Icons/" + presentation.iconMaterial);
    }

    public void SetButtonActionAvailable(bool available, string availabilityInfo)
    {
        Available = available;
        ActionAvailabilityInfo = availabilityInfo;
        GetComponent<Image>().material.SetColor("_Color", new Color(1, 1, 1, (available ? 1 : .5f)));
    }

    public void SetButtonActive(bool active)
    {
        Active = active;
        GetComponent<Animator>().SetBool("isActive", active);
        GetComponent<Animator>().SetBool("isHovered", false);
    }
}
