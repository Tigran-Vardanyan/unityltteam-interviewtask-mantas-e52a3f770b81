using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour {
	public static EnemySpawner Instance { get; private set; }
	public float enemySpawnDelay = 3;
	public float enemySpawnDelayDecrees = 0.1f;
	public float minEnemySpawnDelay = 0.5f;
	
	[SerializeField] private Enemy prefabEnemy;

	private GameController _gameController;

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
		var pos =  GetRandomPositionWithinBounds(gameObject);
		Enemy enemy = Instantiate(prefabEnemy, pos, Quaternion.identity);
	}

	public static Vector3 GetRandomPositionWithinBounds(GameObject obj)
	{
		BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
        
		if (boxCollider == null)
		{
			Debug.Log("No BoxCollider found on the provided GameObject.");
			return obj.transform.position;
		}

		//Get the bounds of the BoxCollider
		Bounds bounds = boxCollider.bounds;

		//Generate a random position within the bounds
		float randomX = Random.Range(bounds.min.x, bounds.max.x);
		float randomY = Random.Range(bounds.min.y, bounds.max.y);
		float randomZ = Random.Range(bounds.min.z, bounds.max.z);

		return new Vector3(randomX, randomY, randomZ);
	}
}
