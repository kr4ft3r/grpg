using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct CharacterPresentation
{
    public CharacterPresentation(
        //TODO maybe name here
        string name,
        string portrait = null,
        string thumb = null,
        string backgroundMaterial = null,
        float actorWidth = 2,
        float actorHeight = 4,
        List<string> walkSprites = null,
        string idleSpriteSheet = null,
        List<string> deadSprites = null
        )
    {
        Name = name;
        Portrait = portrait;
        Thumb = thumb;
        BackgroundMaterial = backgroundMaterial;
        WalkSpritesPaths = walkSprites;
        IdleSpriteSheetPath = idleSpriteSheet;
        DeadSpritesPaths = deadSprites;

        //Loading
        ActorWidthHeightScale = new Vector2(actorWidth, actorHeight);

        if (WalkSpritesPaths == null)
        {
            WalkSprites = null;
        } else
        {
            WalkSprites = WalkSpritesPaths.Select(s => Resources.Load<Sprite>("Sprites/" + s)).ToList();
        }

        if (IdleSpriteSheetPath == null)
        {
            IdleSpriteSheet = null;
        } else
        {
            IdleSpriteSheet = Resources.Load<Sprite>("Sprites/" + IdleSpriteSheetPath);
        }

        if (DeadSpritesPaths == null)
        {
            DeadSprites = null;
        } else
        {
            DeadSprites = DeadSpritesPaths.Select(s => Resources.Load<Sprite>("Sprites/" + s)).ToList();
        }
    }
    /// <summary>
    /// Used for display name
    /// </summary>
    public string Name;
    /// <summary>
    /// Sprite at Resources/Images/Portraits/
    /// </summary>
    public string Portrait;
    /// <summary>
    /// Smaller portrait (200x200)
    /// </summary>
    public string Thumb;
    /// <summary>
    /// Sprite at Resources/Images/
    /// </summary>
    public string BackgroundMaterial;

    public Vector2 ActorWidthHeightScale;

    /// <summary>
    /// If using separate sprites for animation. Sprites at Resources/Sprites/
    /// </summary>
    public List<string> WalkSpritesPaths;

    public string IdleSpriteSheetPath;

    public List<Sprite> WalkSprites;

    public Sprite IdleSpriteSheet;

    public List<string> DeadSpritesPaths;

    public List<Sprite> DeadSprites;

}
