using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GRPG.GameLogic;

public class SceneLocation : MonoBehaviour
{
    public int Index;
    public string[] Connections;
    public Action LocationAction;
    public ActionAvailability ActionAvailability;
    public bool ActionDisabled;

    public List<Actor> LocationActionPerformerHistory { get; private set; }

    private void Start()
    {
        LocationActionPerformerHistory = new List<Actor>();
    }
}
