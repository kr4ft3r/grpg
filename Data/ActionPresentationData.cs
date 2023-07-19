using System.Collections;
using System.Collections.Generic;

using GRPG.GameLogic;

public struct ActionPresentationData
{
    public const string IconSlotMove = "MoveIcon";
    public const string IconSlotPrimary = "PrimaryIcon";
    public const string IconSlotSecondary = "SecondaryIcon";
    public const string IconSlotSpecial = "SpecialIcon";

    public ActionPresentationData(
        string iconMaterial = null, 
        string iconSlot = null,
        System.Action<Sequencer,ActionResult,SceneObjects,float> resultSequenceUpdate = null,
        //TODO Do we need these when we have callbacks? Maybe fallbacks if callbacks are null
        string performSound = null, 
        string successSound = null, 
        string failSound = null,
        float showDamageDelay = 2.0f
        )
    {
        this.iconMaterial = iconMaterial;
        this.iconSlot = iconSlot;
        this.ResultSequenceUpdate = resultSequenceUpdate;
        this.performSound = performSound;
        this.successSound = successSound;
        this.failSound = failSound;
        this.showDamageDelay = showDamageDelay;
    }

    /// <summary>
    /// Must match the material name in Resources/Icons/
    /// </summary>
    public string iconMaterial;
    /// <summary>
    /// Must match the UI element name
    /// </summary>
    public string iconSlot;
    /// <summary>
    /// Code that runs on each frame while action is performed. Includes all animations and FX.
    /// To end the sequence: must assign ActionResult to Sequencer.FinishedResult.
    /// </summary>
    public System.Action<Sequencer, ActionResult, SceneObjects, float> ResultSequenceUpdate { get; set; }
    /// <summary>
    /// Will be played whether the sequence callback exists or is null.
    /// </summary>
    public string performSound;
    /// <summary>
    /// Will be played on result success, only if sequence callback is null.
    /// </summary>
    public string successSound;
    /// <summary>
    /// Will be played on result failure, only if sequence callback is null.
    /// </summary>
    public string failSound;
    /// <summary>
    /// Async time in seconds to wait before displaying damage information, default to 2.0
    /// </summary>
    public float showDamageDelay;
}
