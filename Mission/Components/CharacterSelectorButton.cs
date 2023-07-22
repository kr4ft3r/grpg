using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using GRPG.GameLogic;

public class CharacterSelectorButton : MonoBehaviour
{
    public int SlotIndex;
    public string CharacterName;
    public string ActorName;
    public Sprite Portrait;
    public int HP;
    public int StartingHP;

    private Actor _actor;
    private MissionUI _missionUI;
    private bool _clickable = false;

    public void Set(Actor actor, CharacterPresentation presentation, GameObject parent, MissionUI ui)
    {
        _actor = actor;
        ActorName = actor.Name;
        CharacterName = presentation.Name;
        Portrait = Resources.Load<Sprite>("Images/Portraits/" + presentation.Thumb);
        HP = actor.Resources[Resource.HitPoints];
        StartingHP = ActorManager.Instance.ActorsStartingHP[actor.Name];

        GetComponent<Image>().sprite = Portrait;
        parent.transform.Find("CharacterSelectorHP" + SlotIndex).localScale
                = new Vector3((float)HP / ActorManager.Instance.ActorsStartingHP[ActorName], 1, 1);

        if (actor.GetAvailableActions().Count == 0)
        {
            GetComponent<Image>().color = new Color(1, 1, 1, .5f);
            _clickable = false;
        } else
        {
            GetComponent<Image>().color = new Color(1, 1, 1, 1);
            _clickable = true;
        }

        _missionUI = ui;
    }

    public void OnClicked()
    {
        if (!_clickable) return;

        _missionUI.SetActiveActor(_actor);
        _missionUI.SetPlayerSelectedActionUIState(_actor.GetAvailableActions()[0]);
    }

    public void OnPointerEnter()
    {
        Debug.Log("ENTER");
        string text = CharacterName + " " + "(" + HP + "/" + StartingHP + "HP)";
        _missionUI.HoverCursorText = text + " no more actions";
        if (!_clickable) return;
        _missionUI.HoverCursorText = text + " click to select";
    }

    public void OnPointerExit()
    {
        Debug.Log("EXIT");
        //_missionUI.HoverCursorText = "";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
