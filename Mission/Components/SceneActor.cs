using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GRPG.GameLogic;

public class SceneActor : MonoBehaviour
{
    /// <summary>
    /// Is currently making a move
    /// </summary>
    public bool IsActive = false;
    /// <summary>
    /// Is currently walking
    /// </summary>
    public bool IsWalking = false;
    public bool IsDying = false;
    public bool HasDied = false;

    public Font LabelFont;

    public bool HasSprite { get; private set; }
    public bool HasWalkSprites { get; private set; }
    public bool HasDeadSprites { get; private set; }
    public CharacterPresentation Presentation { get; private set; }

    private GameObject _sprite;
    private SpriteRenderer _spriteRenderer;
    private Shader _spriteShaderWhite;
    private Shader _spriteShaderDefault;
    private float _animTimeCounter = 0;
    private int _animIndex = 0;

    private List<(string,float)> _infoTexts;
    private List<string> _queuedInfoTexts;
    private float _timeBetweenInfoTexts = 0.5f;

    public float SpriteRotation;
    public float SpriteElevation;

    // Start is called before the first frame update
    void Start()
    {
        _infoTexts = new List<(string, float)>();
        _queuedInfoTexts = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_sprite != null)
        {
            _sprite.transform.localPosition = new Vector3(0, SpriteElevation, 0);
            _sprite.transform.LookAt(Camera.main.transform);
            _sprite.transform.Rotate(Vector3.forward, SpriteRotation); //= Quaternion.Euler(0, 0, SpriteRotation);
        }

        if (IsWalking && _sprite != null && HasWalkSprites)
        {
            if (_animTimeCounter >= 0.2f)
            {
                _animIndex++;
                if (_animIndex >= Presentation.WalkSprites.Count) _animIndex = 0;
                _sprite.GetComponent<SpriteRenderer>().sprite = Presentation.WalkSprites[_animIndex];

                _animTimeCounter = 0;
            } else
            {
                _animTimeCounter += Time.deltaTime;
            }
            
        }

        if (IsDying && _sprite != null && HasDeadSprites)
        {
            if (_animTimeCounter >= 0.2f)
            {
                _animIndex++;
                if (_animIndex >= Presentation.DeadSprites.Count) { 
                    IsDying = false; // it's ok you can stop dying now
                    HasDied = true; // and start being dead
                } else
                {
                    _sprite.GetComponent<SpriteRenderer>().sprite = Presentation.DeadSprites[_animIndex];
                }

                _animTimeCounter = 0;
            }
            else
            {
                _animTimeCounter += Time.deltaTime;
            }
        }

        if (_infoTexts.Count > 0 && _queuedInfoTexts.Count > 0)
        {
            float timeSinceLast = Time.time - _infoTexts[_infoTexts.Count - 1].Item2;
            if (timeSinceLast >= _timeBetweenInfoTexts)
            {
                AddInfoText(_queuedInfoTexts[0]);
                _queuedInfoTexts.RemoveAt(0);
            }
        }
    }

    public void AddInfoText(string text)
    {
        if (_infoTexts.Count > 0)
        {
            float timeSinceLast = Time.time - _infoTexts[_infoTexts.Count - 1].Item2;
            if (timeSinceLast < _timeBetweenInfoTexts)
            {
                _queuedInfoTexts.Add(text);
                return;
            }
        }
        
        _infoTexts.Add((text, Time.time));
    }

    public void TakeDamage(Damage damage, int hpRemaining, float delay = 2f)
    {
        StartCoroutine(ShowDamage(damage.DamageAmount, hpRemaining, delay));

        if (hpRemaining <= 0) Die();
    }

    public IEnumerator ShowDamage(int damage, int hpRemaining, float delay = 2f)
    {
        yield return new WaitForSeconds(delay);
        AddInfoText((damage > 0 ? "-" : (damage < 0 ? "+" : "miss")) + damage);
    }

    public void Die()
    {
        IsDying = true;
        //AddInfoText("Dead");
    }

    public void SetPresentation(CharacterPresentation presentation)
    {
        Presentation = presentation;
        Debug.Log("Setting presentation for " + Presentation.Name);

        if (
            Presentation.WalkSprites != null ||
            Presentation.IdleSpriteSheet != null
            ) ConfigureSprite(); //TODO better
    }

    private void ConfigureSprite()
    {
        Debug.Log("Configuring sprite for " + Presentation.Name);
        HasSprite = true;
        GetComponentInChildren<MeshRenderer>().enabled = false;
        transform.Find("Collider").transform.localScale = new Vector3(Presentation.ActorWidthHeightScale.x, Presentation.ActorWidthHeightScale.y, Presentation.ActorWidthHeightScale.x);
        GameObject sprite = new GameObject("Sprite");
        sprite.transform.parent = transform;

        SpriteRenderer renderer = sprite.AddComponent<SpriteRenderer>();
        renderer.drawMode = SpriteDrawMode.Simple;

        // only height for sprite size
        sprite.transform.localScale = new Vector3(Presentation.ActorWidthHeightScale.y*2.5f, Presentation.ActorWidthHeightScale.y * 2.5f, 0);
        //no need their pivot will be bottom sprite.transform.localPosition = new Vector3(0, 3f, 0);
        sprite.transform.localPosition = new Vector3(0, 0, 0);

        _sprite = sprite;
        _spriteRenderer = sprite.GetComponent<SpriteRenderer>();
        _spriteShaderWhite = Shader.Find("GUI/Text Shader");
        _spriteShaderDefault = _spriteRenderer.material.shader; //Shader.Find("Sprites/Default");

        if (Presentation.WalkSprites != null)
        {
            HasWalkSprites = true;
            renderer.sprite = Presentation.WalkSprites[0];
        }

        if (Presentation.IdleSpriteSheet != null)
        {
            renderer.sprite = Presentation.IdleSpriteSheet;
        }

        if (Presentation.DeadSprites != null)
        {
            HasDeadSprites = true;
        }
    }

    public void SetSpriteColor(Color color)
    {
        if (_spriteRenderer.material.shader.name != _spriteShaderDefault.name)
        {
            Debug.Log(" ~S h a d e r (default) " + _spriteShaderDefault.name + " " + color.ToString());
            _spriteRenderer.material.shader = _spriteShaderDefault;
        }
        _spriteRenderer.material.color = color;
    }

    public void SetSpriteColorWhite()
    {
        if (_spriteRenderer.material.shader.name == _spriteShaderWhite.name) return;
        Debug.Log(" ~S h a d e r (white) " + _spriteShaderWhite.name);
        _spriteRenderer.material.shader = _spriteShaderWhite;
        _spriteRenderer.material.color = Color.white;
    }

    private void OnGUI()
    {
        //GUI.skin.font = LabelFont;
        GUI.skin.label.font = LabelFont;
        GUI.skin.label.fontStyle = FontStyle.Normal;

        if (_infoTexts.Count > 0)
        {
            List<int> remove = new List<int>();
            int index = -1;
            Vector3 rootTextPos = new Vector3(transform.position.x, transform.position.y + 8, transform.position.z);
            foreach((string text, float start) in _infoTexts)
            {
                index++;
                float yOffset = (Time.time - start) * 4;
                /*if (yOffset > 10)
                {
                    yOffset += yOffset * Time.deltaTime;
                }*/
                Vector3 labelPos = new Vector3(rootTextPos.x, rootTextPos.y + yOffset, rootTextPos.z);
                labelPos = Camera.main.WorldToScreenPoint(labelPos);
                if (labelPos.y >= Screen.height)
                {
                    remove.Add(index);
                    continue;
                }
                GUI.Label(new Rect(labelPos.x, Screen.height - labelPos.y, 250, 20), text);
            }
            for (int i = remove.Count - 1; i >= 0; i--)
            {
                _infoTexts.RemoveAt(i);
            }
        }
    }
}
