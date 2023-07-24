using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    private GameState _state;
    private CharacterPresentation _presentation;
    public void Set(GameState state, CharacterPresentation presentation)
    {
        _state = state;
        _presentation = presentation;
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
