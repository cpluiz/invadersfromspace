using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour{

    public string mobRow;
    public bool isFreeToShot = false;
    public int points = 10;
    public bool moveIdependently { get { return _moveIndependently; } }
    protected bool _moveIndependently = false;
    private bool destroyedByPlayer = false;

    private void OnCollisionEnter2D(Collision2D collision) {
        if(collision.gameObject.CompareTag("PlayerProjectile")) {
            collision.gameObject.SetActive(false);
            destroyedByPlayer = true;
            GameController.instance.RemoveMob(this);
            return;
        }
        if(collision.gameObject.CompareTag("Ground")) {
            GameController.instance.GameOver();
        }
    }

    private void OnDestroy() {
        if(moveIdependently) return;
        GameController.instance.AddPoints(this.points);
    }

    public void IsFreeToShot() {
        //If it's free, why to test again?
        if(isFreeToShot) return;
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(transform.position.x, transform.position.y + GetComponent<SpriteRenderer>().bounds.extents.y + 0.1f),Vector2.down,2f);
        bool free = true;
        foreach(RaycastHit2D hit in hits) {
            if(hit.collider != null && hit.collider.gameObject.GetInstanceID() != gameObject.GetInstanceID()) {
                free &= !hit.transform.CompareTag("Enemy");
            }
        }
        isFreeToShot = free;
    }

    public void Shot() {
        Projectile shot = Instantiate(GameController.instance.enemyProjectilePrefab);
        shot.transform.position = new Vector2(transform.position.x,GetComponent<SpriteRenderer>().bounds.max.y - 0.1f);
        shot.projectileSpeed *= -1;
        shot.gameObject.SetActive(true);
    }
}
