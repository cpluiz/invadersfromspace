using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotherShipController : EnemyController{
    private int direction = 0;
    [SerializeField]
    [Range(1f,10f)]
    private float speed = 2;

    public void SetMotion(int dir) {
        direction = dir;
    }

    void Update() {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    void LateUpdate() {
        float cameraHalfSize = Camera.main.orthographicSize;
        float shipSize = GetComponent<SpriteRenderer>().bounds.size.x * transform.localScale.x;
        if(transform.position.x > cameraHalfSize + (shipSize * 1.5f) || transform.position.x < -cameraHalfSize - (shipSize * 1.5f))
            GameController.instance.MotherShipDestroyed();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if(collision.gameObject.CompareTag("PlayerProjectile")) {
            collision.gameObject.SetActive(false);
            GameController.instance.MotherShipDestroyed(true);
            GameController.instance.AddPoints(this.points);
            return;
        }
    }
}
