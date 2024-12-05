using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }
    public float enemySpawnDelay = 3;
    public float enemySpawnDelayDecrees = 0.1f;
    public float minEnemySpawnDelay = 0.5f;

    [SerializeField] private Enemy prefabEnemy;
    [SerializeField] private int poolSize = 10; // Number of enemies in the pool

    private GameController _gameController;

    // Object Pool Variables
    private Queue<Enemy> enemyPool = new Queue<Enemy>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _gameController = GameController.Instance;
        InitializeEnemyPool();
        SpawnEnemy();
        StartCoroutine(SpawnEnemyDelay());
    }

    public IEnumerator SpawnEnemyDelay()
    {
        while (_gameController._isRunning)
        {
            yield return new WaitForSeconds(enemySpawnDelay);
            SpawnEnemy();
        }
    }

    private void Update()
    {
        DecreaseSpawnDelay();
        
    }

    private void DecreaseSpawnDelay()
    {
        if (enemySpawnDelay > minEnemySpawnDelay)
        {
            enemySpawnDelay -= enemySpawnDelayDecrees * Time.deltaTime;
            enemySpawnDelay = Mathf.Max(enemySpawnDelay, minEnemySpawnDelay);
        }
    }

    public void SpawnEnemy()
    {
        var pos = GetRandomPositionWithinBounds(gameObject);

        Enemy enemy = GetEnemyFromPool();
        enemy.transform.position = pos;
        enemy.gameObject.SetActive(true); 
    }

    private Enemy GetEnemyFromPool()
    {
        if (enemyPool.Count > 0)
        {
            
            Enemy enemy = enemyPool.Dequeue();
            return enemy;
        }
        else
        {
            
            Enemy enemy = Instantiate(prefabEnemy);
            return enemy;
        }
    }

    public void ReturnEnemyToPool(Enemy enemy)
    {
        enemy.gameObject.SetActive(false); 
        enemyPool.Enqueue(enemy);  
    }

    private void InitializeEnemyPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            Enemy enemy = Instantiate(prefabEnemy);
            enemy.gameObject.SetActive(false);  
            enemyPool.Enqueue(enemy);
        }
    }

    public static Vector3 GetRandomPositionWithinBounds(GameObject obj)
    {
        BoxCollider boxCollider = obj.GetComponent<BoxCollider>();

        if (boxCollider == null)
        {
            Debug.Log("No BoxCollider found on the provided GameObject.");
            return obj.transform.position;
        }

       
        Bounds bounds = boxCollider.bounds;
        
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(randomX, randomY, randomZ);
    }
}