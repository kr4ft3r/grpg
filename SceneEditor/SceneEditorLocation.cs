using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionAvailability
{
    Unlimited, Once, OncePerCharacter
}

public class SceneEditorLocation : MonoBehaviour
{
    public int Index;
    public string[] Connections;
    public string LocationAction = "";
    public ActionAvailability ActionAvailability;
    public bool ActionDisabled;

    // Start is called before the first frame update
    void Start()
    {
        // Hide sphere
        GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
