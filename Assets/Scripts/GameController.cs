using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour{

    private static GameController _instance;
    public static GameController instance { get => _instance; }

    [Header("Game Configuration")]
    public Transform shieldContainer;
    public Transform shieldPrefab;
    public LockedTargetController targetSign;
    [Range(3,5)]
    public int totalShields = 4;
    private List<Transform> shields;
    private float shieldWidth, shieldMargin;
    [Range(0.1f,3f)]
    public float shieldBorder = 3;
    [SerializeField]
    [Range(10,900)]
    private int ShotsToAuto = 50;
    public HordeController hordeController;
    public PlayerController playerController;
    public Projectile enemyProjectilePrefab;
    private bool handlingCollision;
    [Header("UI Configuration")]
    [SerializeField]
    private Image[] lifeImage;
    [SerializeField]
    private TMP_Text txt_score, txt_highscore, txt_lifes, txt_guidedShot;
    private int points = 0;
    private int hiScore = 0;
    private int totalShots = 0;
    private bool hasAutoShot, findNearest, showNearest;
    private EnemyController nearestEnemy;
    private float startDistance;

    public void Awake() {
        if(_instance == null) {
            _instance = this;
        } else if(_instance != this) {
            Destroy(gameObject);
        }
    }

    void Start() {
        float vertExtent = (Camera.main.orthographicSize * 2);
        shieldWidth = shieldPrefab.GetComponent<BoxCollider2D>().size.x;
        shieldMargin = (vertExtent - shieldBorder - (shieldWidth * totalShields)) / totalShields;
        SpawnShields();
        hordeController.SpawnMobs();
        hordeController.CountEnemies();
        playerController.lifes = 3;
        lifeImage[0].gameObject.SetActive(true);
        lifeImage[1].gameObject.SetActive(true);
        points = 0;
        hiScore = PlayerPrefs.GetInt("HiSocre",0);
        string score = hiScore < 10 ? "000" + hiScore : (hiScore >= 10 && hiScore < 100 ? "00" + hiScore : (hiScore >= 100 && hiScore < 1000 ? "0" + hiScore : hiScore.ToString()));
        txt_highscore.text = "HI-SCORE: " + score;
        txt_guidedShot.text = ShotsToAuto + " shots to guided missile";
        startDistance = Mathf.Abs(hordeController.transform.position.y - playerController.transform.position.y);
    }

    void SpawnShields() {
        if(shields != null)
            foreach(Transform shield in shields)
                Destroy(shield.gameObject);
        shields = new List<Transform>();
        float posX;
        for(int i = 1; i <= totalShields; i++) {
            Transform shield = Instantiate<Transform>(shieldPrefab,shieldContainer);
            posX = -Camera.main.orthographicSize + (shieldBorder/2) -shieldMargin + ((shieldMargin + shieldWidth)*i);
            shield.localPosition = new Vector2(posX,0);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision) {
        if(handlingCollision) return;
        if(collision.gameObject.GetComponent<EnemyController>()) {
            if(collision.gameObject.GetComponent<MotherShipController>()) return;
            handlingCollision = true;
            hordeController.ChangeDirection();
            hordeController.transform.Translate(Vector2.down * 0.1f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.gameObject.GetComponent<EnemyController>())
            handlingCollision = false;
    }

    private void LateUpdate() {
        if(handlingCollision) handlingCollision = false;
        if(findNearest) GetClosestEnemy();
        if(showNearest) HighlightNearestEnemy();
        HordeDistanceToPlayer();
    }

    public void RemoveMob(EnemyController mob) {
        hordeController.RemoveMob(mob);
    }

    public void AddPoints(int pts) {
        points += pts;
        string score = points < 10 ? "000" + points : (points >= 10 && points < 100 ? "00"+points : (points >= 100 && points < 1000 ? "0"+points : points.ToString()) );
        txt_score.text = "SCORE: " + score;
    }

    public void AddShot() {
        if(hasAutoShot || showNearest) return;
        totalShots++;
        int shotsToAuto = totalShots % ShotsToAuto;
        if(shotsToAuto == 0) {
            hasAutoShot = true;
            findNearest = true;
            showNearest = true;
            txt_guidedShot.text = "Press CTRL to shot guided missile";
            return;
        }
        txt_guidedShot.text = (ShotsToAuto - shotsToAuto) + " shots to guided missile";
    }

    public void GetClosestEnemy() {
        nearestEnemy = hordeController.GetClosestEnemy(playerController.transform);
    }

    public bool AutoShotSent() {
        if(!hasAutoShot) return hasAutoShot;
        playerController.projectile.SetTarget(nearestEnemy.transform);
        findNearest = false;
        hasAutoShot = false;
        return true;
    }

    public void SetLifes(int lifes) {
        txt_lifes.text = lifes.ToString();
        if(lifes > 0)
            lifeImage[lifes-1].gameObject.SetActive(false);
        if(lifes == 0)
            GameOver();
    }

    public void GameOver() {
        if(points > hiScore) {
            PlayerPrefs.SetInt("HiSocre",points);
            string score = hiScore < 10 ? "000" + hiScore : (hiScore > 10 && hiScore < 100 ? "00" + hiScore : (hiScore > 100 && hiScore < 1000 ? "0" + hiScore : hiScore.ToString()));
            txt_highscore.text = "HI-SCORE: " + score;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    void HighlightNearestEnemy() {
        targetSign.SetTarget(nearestEnemy.transform);
    }

    public void ProjectileHitSomething() {
        if(showNearest && !findNearest) {
            showNearest = false;
            targetSign.gameObject.SetActive(false);
            txt_guidedShot.text = ShotsToAuto + " shots to guided missile";
        }
    }

    public void HordeDistanceToPlayer() {
        if(hordeController == null) return;
        float scaledDistance = Mathf.Abs(hordeController.transform.position.y - playerController.transform.position.y) / startDistance;
        AudioManager.ChangeMusicSpeed(1 + ((1 - scaledDistance) * 5));
    }

    public void MotherShipDestroyed(bool destroyedByPlayer = false) {
        hordeController.DestroyMotherShip(destroyedByPlayer);
        if(destroyedByPlayer) {
            SpawnShields();
        }
    }
}
