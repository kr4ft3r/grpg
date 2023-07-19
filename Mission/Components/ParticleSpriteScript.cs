using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpriteScript : MonoBehaviour
{
    public Sprite SingleSprite;
    public Sprite[] Sprites;
    public int AnimFrom;
    public int AnimTo;
    public bool Loop;
    public float lifeTime;
    public int FPS = 10;

    private float _time;
    private float _animTime;
    private int _animIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        _time = 0;
        if (Sprites != null && Sprites.Length > 0)
        {
            GetComponent<SpriteRenderer>().sprite = Sprites[AnimFrom];
        } else
        {
            GetComponent<SpriteRenderer>().sprite = SingleSprite;
        }
        _animIndex = AnimFrom;

    }

    // Update is called once per frame
    void Update()
    {
        if (_time >= lifeTime)
        {
            Destroy(gameObject);
        }
        

        transform.LookAt(Camera.main.transform);

        if (Sprites != null && Sprites.Length > 0)
        {
            if (_animTime >= (float)(1.0f / FPS))
            {
                _animIndex++;
                if (_animIndex > AnimTo && Loop) _animIndex = 0;
                else if (_animIndex > AnimTo) _animIndex = AnimTo;

                GetComponent<SpriteRenderer>().sprite = Sprites[_animIndex];

                _animTime = 0;
            }
            else
            {
                _animTime += Time.deltaTime;
            }
        }


        _time += Time.deltaTime;
    }
}
