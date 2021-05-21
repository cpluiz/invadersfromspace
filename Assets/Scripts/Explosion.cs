using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Explosion : MonoBehaviour{
    Animator explosionAnimator;
    private void Awake() {
        explosionAnimator = GetComponent<Animator>();
    }

    public void AnimationEnded() {
        Destroy(gameObject);
    }
}
