using UnityEngine;

public class GameController : MonoBehaviour {
    public static GameController Instance { get; private set; }
    [Header("Enemy Settings")]
    public float enemySpawnDelay;
    public float enemySpawnDelayDecrees;
    public float minEnemySpawnDelay;
    [Header("Player Settings")] 
    public float fireInterval;
    public bool _isRunning = true;
    private Player _player;
    private EnemySpawner _enemySpawner;
    private float _gameStartTime;
    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Application.targetFrameRate = 60;
    }

    private void Start() {
        InitializeGame();
    }
    private void InitializeGame()
    {
        _gameStartTime = Time.time; 

        _player = Player.Instance;
        _player.OnDie += OnPlayerDie;
        _player.fireInterval = fireInterval;

        _enemySpawner = EnemySpawner.Instance;
        _enemySpawner.enemySpawnDelay = enemySpawnDelay;
        _enemySpawner.enemySpawnDelayDecrees = enemySpawnDelayDecrees;
        _enemySpawner.minEnemySpawnDelay = minEnemySpawnDelay;
        _isRunning = true; 
    }
    private void OnPlayerDie() {
        _isRunning = false;
        StopCoroutine(_enemySpawner.SpawnEnemyDelay());
        DisablePlayerInput();
        //Optional
        //Time.timeScale = 0f;
    }
    private void DisablePlayerInput() {
       
        _player.DisableInput(); 
    }
}
