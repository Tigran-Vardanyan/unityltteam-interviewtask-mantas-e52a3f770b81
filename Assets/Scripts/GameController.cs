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
        _player = Player.Instance;
        _player.OnDie += OnPlayerDie;
        _player.fireInterval = fireInterval;
        _enemySpawner = EnemySpawner.Instance;
        _enemySpawner.enemySpawnDelay = enemySpawnDelay;
        _enemySpawner.enemySpawnDelayDecrees = enemySpawnDelayDecrees;
        _enemySpawner.minEnemySpawnDelay = minEnemySpawnDelay;
    }
    private void OnPlayerDie() {
        _isRunning = false;
    }
}
