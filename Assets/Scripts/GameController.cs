using UnityEngine;

public class GameController : MonoBehaviour {

    [SerializeField] private Enemy _prefabEnemy;
    [SerializeField] private Vector3 _spawnPosition;
    [SerializeField] private Vector3 _spawnOffsets;
    [SerializeField] private float _enemySpawnInterval = 0.5f;
    private float _enemySpawnTimer = 0.0f;
    bool _running = true;

    private Player _player;
    
    void Awake() {
        Application.targetFrameRate = 60;
    }

    void Start() {
        _player = Object.FindObjectOfType<Player>(true);
        _player.OnDie += OnPlayerDie;

        _running = true;
    }

    void Update() {
        if (!_running) return;
        _enemySpawnTimer += Time.deltaTime;
        if ( _enemySpawnTimer >= _enemySpawnInterval ) {
            var e = Instantiate(_prefabEnemy);

            var p = _spawnPosition + new Vector3(
                Random.Range(-_spawnOffsets.x, _spawnOffsets.x),
                Random.Range(-_spawnOffsets.y, _spawnOffsets.y),
                0.0f
            );
            e.transform.position = p;

            _enemySpawnTimer -= _enemySpawnInterval;
        }
    }

    void OnPlayerDie() {
        _running = false;

    }


}
