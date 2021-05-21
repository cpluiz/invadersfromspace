using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeController : MonoBehaviour{
    [Header("Horde Configurations")]
    /// <summary>
    /// Define the horizontal empty space around the enemy hord
    /// </summary>
    [Range(0.1f, 3f)]
    public float hordeBorder = 3;
    /// <summary>
    /// Define the internal margin between spawned monsters
    /// </summary>
    [Range(0.1f,1f)]
    public float hordeMargin;
    public int squidRows = 1;
    public int crabRows = 2;
    public int octopusRows = 2;
    [SerializeField]
    private float enemySpeed;
    [Range(1,5)]
    public int maxEnemyShots = 3;
    [SerializeField]
    private float enemyDificultyChanger = 0.2f;
    [SerializeField]
    private Transform squidPrefab, crabPrefab, octopusPrefab, motherShipPrefab, explosionPrefab;
    private float startPosition;
    private float vertExtent;
    private float rowPosition;
    public int direction { get; private set; } = 1;
    private Dictionary<string, List<EnemyController>> mobs;
    private string[] rowShotOrder;
    private Dictionary<string,int> mobCount;
    private int currentRow;
    private Vector3 initialPosition = new Vector3(0,3.72f,0);
    private bool isShooting, isRemovingEnemy;
    private int totalEnemies;
    private MotherShipController motherShip = null;

    public void ChangeDirection() {
        direction *= -1;
    }

    public void SpawnMobs(float speed = 0.3f) {
        Time.timeScale = 0;
        enemySpeed = speed;
        if(mobs != null && mobs.Count > 0)
            foreach(string mobName in mobs.Keys)
                foreach(EnemyController mob in mobs[mobName])
                    Destroy(mob.gameObject);
        StopAllCoroutines();
        rowPosition = 0;
        currentRow = 0;
        mobs = new Dictionary<string,List<EnemyController>>();
        mobCount = new Dictionary<string,int>();
        transform.position = initialPosition;
        vertExtent = (Camera.main.orthographicSize*2);
        startPosition = -(vertExtent/2) + hordeBorder/2;
        SpawnMobRow(squidPrefab,squidRows);
        SpawnMobRow(crabPrefab,crabRows);
        SpawnMobRow(octopusPrefab,octopusRows);
        rowShotOrder = mobs.Keys.ToArray<string>();
        System.Array.Reverse(rowShotOrder);
        Time.timeScale = 1;
        StartCoroutine(nameof(StartShoting));
        StartCoroutine(nameof(SpawnMotherShip));
    }

    IEnumerator StartShoting() {
        while(true) {
            yield return new WaitForSeconds(Random.Range(0.6f,2f));
            EnemyShot();
            if(motherShip != null && (Random.Range(0,2) * 2 - 1) > 0)
                motherShip.Shot();
        }
    }

    IEnumerator SpawnMotherShip() {
        if(motherShip != null) Destroy(motherShip.gameObject); 
        yield return new WaitForSeconds(Random.Range(5f,30f));
        motherShip = Instantiate(motherShipPrefab).GetComponent<MotherShipController>();
        int screenSide = Random.Range(0,2) * 2 - 1;
        Vector3 motherPosition = new Vector3((Camera.main.orthographicSize + motherShip.GetComponent<SpriteRenderer>().bounds.max.x * motherShip.transform.localScale.x) * screenSide,initialPosition.y,0);
        motherShip.transform.position = motherPosition;
        motherShip.SetMotion(-screenSide);
    }

    public void DestroyMotherShip(bool destroyedByPlayer = false) {
        if(destroyedByPlayer) {
            Transform explosion = Instantiate(explosionPrefab);
            explosion.position = motherShip.transform.position;
            AudioManager.EnemyExploding();
        }
        StartCoroutine(nameof(SpawnMotherShip));
    }

    void SpawnMobRow(Transform mobPrefab, int rows) {
        float prefabWidth = mobPrefab.GetComponent<SpriteRenderer>().bounds.size.x * mobPrefab.localScale.x;
        for(int i = 0;i < rows;i++) {
            string rowName = i == 0 ? mobPrefab.gameObject.name : mobPrefab.gameObject.name + "_" + i;
            rowPosition -= (mobPrefab.GetComponent<SpriteRenderer>().bounds.size.y + (hordeMargin / 4));
            int mobTotal = Mathf.FloorToInt((vertExtent - hordeBorder) / (prefabWidth + (hordeMargin / 2)));
            mobs.Add(rowName, SpawnMobsInRow(rowPosition,mobPrefab,prefabWidth,mobTotal, rowName));
            mobCount.Add(rowName,mobs[rowName].Count);
        }
    }

    List<EnemyController> SpawnMobsInRow(float y, Transform prefab, float prefabWidth, int total, string rowName) {
        float posX;
        float marg = (vertExtent - hordeBorder - (prefabWidth * total)) / total;
        List<EnemyController> mobRow = new List<EnemyController>();
        for(int i=1; i<=total;i++) {
            posX = startPosition - marg + ((marg + prefabWidth) * i);
            Transform mob = Instantiate<Transform>(prefab, gameObject.transform);
            mob.localPosition = new Vector2(posX, y);
            EnemyController mobController = mob.GetComponent<EnemyController>();
            mobController.mobRow = rowName;
            mobRow.Add(mobController);
        }
        return mobRow;
    }

    private void LateUpdate() {
        if(mobs == null || mobs.Count <= 0) return;
        foreach(string mobName in mobs.Keys)
            foreach(EnemyController mob in mobs[mobName]) {
                if(mob == null || mobs.Count == 0) continue;
                mob.transform.Translate(Vector2.right * direction * Time.deltaTime * enemySpeed);
            }
    }

    void EnemyShot() {
        if(isShooting) return;
        isShooting = true;
        int shotsInThisRound = Random.Range(1,Mathf.Clamp(maxEnemyShots + 1, 1, totalEnemies<3 ? totalEnemies : 3));
        EnemyController[] hasShoted = new EnemyController[shotsInThisRound];
        if(mobs[rowShotOrder[currentRow]].Count == 0) currentRow++;
        while(shotsInThisRound > 0) {
            EnemyController toShot = GetEnemyFromRow(rowShotOrder[currentRow], currentRow);
            if(toShot != null) { 
                if(hasShoted.Contains(toShot)) continue;
                hasShoted[shotsInThisRound-1] = toShot;
                shotsInThisRound--;
            }
        }
        foreach(EnemyController enemy in hasShoted) {
            enemy.Shot();
        }
        isShooting = false;
    }

    public EnemyController GetEnemyFromRow(string rowName, int rowNumber) {
        if(isRemovingEnemy) {
            isRemovingEnemy = false;
            return null;
        }
        if(mobs[rowShotOrder[rowNumber]].Count < mobCount[rowShotOrder[rowNumber]] && mobs[rowShotOrder[rowNumber]].Count > 0)
            if(Random.Range(0f,1f) > 0.6f && rowNumber < mobs.Keys.Count-1) {
                int row = Mathf.Clamp(rowNumber + 1,0,mobs.Keys.Count - 1);
                return GetEnemyFromRow(rowShotOrder[row],row);
            }

        if(mobs[rowShotOrder[currentRow]].Count <= 0) {
            currentRow++;
            return GetEnemyFromRow(rowShotOrder[currentRow], currentRow);
        }
        int teste = Random.Range(0,mobs[rowShotOrder[rowNumber]].Count);
        EnemyController toShot = mobs[rowShotOrder[rowNumber]][teste];
        if(rowNumber == 0) return toShot;
        if(toShot.isFreeToShot || currentRow == mobs.Keys.Count - 1) return toShot;

        return GetEnemyFromRow(rowName,rowNumber);
    }


    public void RemoveMob(EnemyController mob) {
        StopCoroutine(nameof(StartShoting));
        mobs[mob.mobRow].Remove(mob);
        Transform explosion = Instantiate(explosionPrefab);
        explosion.position = mob.transform.position;
        AudioManager.EnemyExploding();
        Destroy(mob.gameObject);
        CountEnemies();
        StartCoroutine(nameof(StartShoting));
    }

    public void CountEnemies() {
        if(mobs == null || mobs.Count <= 0) return;
        int count = 0;
        foreach(string mobName in mobs.Keys)
            foreach(EnemyController mob in mobs[mobName]) {
                if(mob == null || mobs.Count == 0) continue;
                mob.IsFreeToShot();
                count++;
            }
        totalEnemies = count;
        if(count == 0) {
            enemySpeed += enemyDificultyChanger;
            SpawnMobs();
        }
    }

    public EnemyController GetClosestEnemy(Transform playerTransform) {
        EnemyController tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach(string mobName in mobs.Keys)
            foreach(EnemyController mob in mobs[mobName]) {
                if(mob == null || mobs.Count == 0) continue;
                float dist = Vector3.Distance(mob.transform.position,playerTransform.position);
                if(dist < minDist) {
                    tMin = mob;
                    minDist = dist;
                }
            }
        return tMin;
    }
}
