using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    [SerializeField]
    private Sprite full_shield, dammaged_shield;
    [SerializeField]
    private SpriteRenderer left_pice, right_pice;
    [SerializeField]
    private int life = 3;

    private void TakeDammage() {
        life--;
        if(life == 2) left_pice.sprite = dammaged_shield;
        if(life == 1) right_pice.sprite = dammaged_shield;
        if(life <= 0) Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        TakeDammage();
        if(collision.gameObject.name == "Projectile") {
            collision.gameObject.SetActive(false);
            return;
        }
        Destroy(collision.gameObject);
    }
}
