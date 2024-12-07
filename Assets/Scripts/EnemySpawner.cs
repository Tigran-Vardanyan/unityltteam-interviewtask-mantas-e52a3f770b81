using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Spawner Settings")]
    [SerializeField] private Enemy prefabEnemy;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 3.0f;
    [SerializeField] private int poolSize = 10;

    private List<Enemy> enemyPool;
    private bool isSpawning;
    private Camera mainCamera;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        mainCamera = Camera.main;
        InitializePool();
        
    }

    private void InitializePool()
    {
        enemyPool = new List<Enemy>();

        for (int i = 0; i < poolSize; i++)
        {
            var enemy = Instantiate(prefabEnemy);
            enemy.gameObject.SetActive(false);
            enemyPool.Add(enemy);
        }
    }

    public void StartSpawning()
    {
        isSpawning = true;
        InvokeRepeating(nameof(SpawnEnemy), 0, spawnInterval);
    }

    public void StopSpawning()
    {
        isSpawning = false;
        CancelInvoke(nameof(SpawnEnemy));
    }

    private void SpawnEnemy()
    {
        if (!isSpawning) return;

        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        var enemy = GetEnemyFromPool();

        if (enemy != null)
        {
            //Enemy spawn position depends on field of camera view width
            float distance = Mathf.Abs(spawnPoint.position.z - mainCamera.transform.position.z); 
            float halfVerticalFOV = mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad; 
            float halfHeight = Mathf.Tan(halfVerticalFOV) * distance;
            float halfWidth = halfHeight * mainCamera.aspect;
            enemy.transform.position = new Vector3(
                spawnPoint.position.x + Random.Range(-halfWidth + 2f, halfWidth - 2f),
                spawnPoint.position.y,
                spawnPoint.position.z);
            
            enemy.ResetEnemy();
            enemy.gameObject.SetActive(true);
        }
    }

    private Enemy GetEnemyFromPool()
    {
        foreach (var enemy in enemyPool)
        {
            if (!enemy.gameObject.activeInHierarchy)
            {
                return enemy;
            }
        }

        Debug.LogWarning("All enemies are active! Consider increasing pool size.");
        return null;
    }

    public void ReturnEnemyToPool(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
    }
}