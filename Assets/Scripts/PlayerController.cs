using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour{

    [SerializeField]
    public Projectile projectile;
    private Vector3 startPosition;
    public int lifes = 3;

    [Range(1f,5f)]
    public float playerSpeed = 1f;

    private void Awake() {
        projectile.gameObject.SetActive(false);
        startPosition = transform.position;
    }

    public void Update() {
        transform.Translate(Vector2.right * Input.GetAxis("Horizontal") * Time.deltaTime * playerSpeed);
        if(projectile.gameObject.activeSelf) return;
        if(Input.GetButtonDown("Shot")) Shoot();
        if(Input.GetButtonDown("Follow"))
            if(GameController.instance.AutoShotSent())
                Shoot();
    }

    public void Shoot() {
        projectile.transform.position = transform.position + new Vector3(0,0.05f,0);
        projectile.gameObject.SetActive(true);
        GameController.instance.AddShot();
        AudioManager.PlayShotSound();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if(collision.rigidbody.CompareTag("Enemy")) {
            Destroy(collision.gameObject);
            lifes--;
            AudioManager.PlayerDeath();
            GameController.instance.SetLifes(lifes);
            //TODO - change to game over
            if(lifes <= 0) Time.timeScale = 0;
            transform.position = startPosition;
        }
    }

}

