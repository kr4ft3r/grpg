using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveActorCircleScript : MonoBehaviour
{
    private float _timePassed;
    private bool _changedTarget;
    private GameObject _target;
    public GameObject Target { get { return _target; } set {
            _timePassed = 0;
            _changedTarget = true;
            _target = value;
        } }
    
    private MeshRenderer _meshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Target == null)
        {
            _meshRenderer.enabled = false;
            return;
        } else
        {
            _meshRenderer.enabled = true;
            transform.position = Target.transform.position;
        }

        if (_changedTarget)
        {
            float animDuration = 1f;
            _timePassed += Time.deltaTime;
            float anim = Tween.easeOutBounce(_timePassed / animDuration);
            transform.localScale = new Vector3(4.0f - anim*3f, 1 ,  4.0f - anim*3f);
            if (_timePassed >= animDuration)
            {
                _changedTarget = false;
            }
        }
    }
}
