using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedTargetController : MonoBehaviour{
    [SerializeField]
    SpriteRenderer sprite;
    Transform target;

    public void SetTarget(Transform t) {
        gameObject.SetActive(true);
        target = t;
    }

    private void LateUpdate() {
        if(target == null) gameObject.SetActive(false);
        transform.position = target.position;
        sprite.color = new Color(1f,0,0,Mathf.PingPong(Time.time,0.5f));
    }
}
