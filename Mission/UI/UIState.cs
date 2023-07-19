using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIState
{
    protected string _aimingTagFilter = "none"; //"Actor", "Location"
    protected string _uiTagFilter = "none"; //"CharacterUI"
    protected GameObject _aimedGO;
    public abstract void Enter();
    public abstract void Exit();
    public abstract void LeftButtonClicked();
    public abstract void RightButtonClicked();
    public abstract void ContinueButtonClicked();
    public abstract void SkipButtonClicked();
    public abstract void OnMouseEnter();
    public abstract void OnMouseExit();
    public virtual void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 300) && 
            (hit.transform.parent.tag == _aimingTagFilter || hit.transform.parent.tag == _uiTagFilter))
        {
            GameObject aimed = null;
            if (hit.transform.parent != null) //For now we always assume collider is child of actual game object
            {
                aimed = hit.transform.parent.gameObject;
            }
            //Debug.Log(hit.transform.name);
            if (aimed != _aimedGO)
            {
                _aimedGO = aimed;
                OnMouseEnter();
            } else
            {
                _aimedGO = aimed;
            }
        } else
        {
            if (_aimedGO != null)
            {
                OnMouseExit();
            }
            _aimedGO = null;
        }
    }
}
