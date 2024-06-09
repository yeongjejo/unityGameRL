using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpwner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] enemies;

    [SerializeField]
    private GameObject boss;

    private float[] arrPosX = {-2.2f, -1.1f, 0f, 1.1f, 2.2f};

    [SerializeField]
    private float spawnInterval = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        StartEnemyRoutine();
    }

    void StartEnemyRoutine() {
        StartCoroutine("EnemyRoutine");
    }

    public void StopEnemtRoutine() {
        StopCoroutine("EnemyRoutine");
    }


    IEnumerator EnemyRoutine() {
        yield return new WaitForSeconds(3f);

        float moveSpeed = 5f;
        int spawnCount = 0;
        int enemyIndex = 0;
        while (true) {
            foreach (float posX in arrPosX) {
                SpawnEnemy(posX, enemyIndex, moveSpeed);
            }

            spawnCount += 1;
            if (spawnCount % 10 == 0) {
                enemyIndex += 1;
                moveSpeed += 2f;
                GameManager.instance.moveSpeed = moveSpeed;
            }

            if (enemyIndex >= enemies.Length) {
                spawnBoss();
                enemyIndex = 0;
                moveSpeed = 5f;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
        
    }

    void SpawnEnemy(float posX, int index, float moveSpeed) {
        Vector3 spawnPos = new Vector3(posX, transform.position.y, transform.position.z);

        if (Random.Range(0, 5) == 0) {
            index += 1;
        }

        if (index >= enemies.Length) {
            index = enemies.Length - 1;
        }

        GameObject enemyObject = Instantiate(enemies[index], spawnPos, Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        enemy.SetMoveSpeed(moveSpeed);
    }


    void spawnBoss() {
        Instantiate(boss, transform.position, Quaternion.identity);
        // GameObject enemyObject = Instantiate(enemies[index], spawnPos, Quaternion.identity);
    }
}
