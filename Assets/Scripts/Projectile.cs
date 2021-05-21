using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Range(3f,10f)]
    public float projectileSpeed;
    private Transform followTarget;

    private void FixedUpdate() {
        if(!followTarget) {
            transform.Translate(Vector2.up * projectileSpeed * Time.fixedDeltaTime);
            return;
        }
    }

    public void SetTarget(Transform target) {
        followTarget = target;
    }

    private void LateUpdate() {
        if(!followTarget) return;
        transform.position = Vector2.MoveTowards(transform.position,followTarget.position,projectileSpeed * Time.deltaTime);
        Vector3 vectorToTarget = followTarget.position - transform.position;
        float angle = Mathf.Atan2(vectorToTarget.y,vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle-90,Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation,q,Time.deltaTime * projectileSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if(collision.transform.CompareTag("Ground")) {
            if(projectileSpeed > 0) {
                gameObject.SetActive(false);
                return;
            }
            Destroy(gameObject);
            return;
        }
        if(collision.gameObject.CompareTag("Enemy")) {
            Destroy(collision.gameObject);
            gameObject.SetActive(false);
        }
    }

    private void OnDisable() {
        if(projectileSpeed > 0) {
            transform.rotation = Quaternion.identity;
            followTarget = null;
            GameController.instance.ProjectileHitSomething();
        }
    }
}
