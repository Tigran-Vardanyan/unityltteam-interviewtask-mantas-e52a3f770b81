using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    public GameObject _prefabPowerUp;

    [SerializeField] private GameObject _prefabExplosion;
    [SerializeField] private Projectile _prefabProjectile;
    

    private float _powerUpSpawnChance = 0.1f;
    public int _health = 2;
    private float _speed = 2.0f;
    private Rigidbody _body;

    private bool canFire = false;
    private float _fireInterval = 2.5f;
    private float _fireTimer = 0.0f;
    private float elapsedGameTime;


    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        canFire = Random.value < 0.4f;
        CalculateHealth();
        elapsedGameTime = Time.time;
    }

    void Update()
    {

        if (canFire)
        {
            _fireTimer += Time.deltaTime;
            if (_fireTimer >= _fireInterval)
            {
                var go = Instantiate(_prefabProjectile);
                go.transform.position = transform.position;
                _fireTimer -= _fireInterval;
            }
        }

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0)
        {
            // Return the enemy to the pool if it goes out of the viewport
            EnemySpawner.Instance.ReturnEnemyToPool(this);
        }
    }

    private void FixedUpdate()
    {
        var p = _body.position;
        p += Vector3.down * (_speed * Time.deltaTime);
        _body.MovePosition(p);
    }

    public void Hit(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            var fx = Instantiate(_prefabExplosion);
            fx.transform.position = transform.position;

            if (Random.value < _powerUpSpawnChance)
            {
                PowerUp powerup = PowerUp.GetFromPool(_prefabPowerUp);
                if (powerup != null)
                {
                    var types = Enum.GetValues(typeof(PowerUp.PowerUpType)).Cast<PowerUp.PowerUpType>().ToList();
                    powerup.SetType(types[Random.Range(0, types.Count)]);
                    powerup.transform.position = transform.position;
                }
            }

            EnemySpawner.Instance.ReturnEnemyToPool(this);
            Object.FindObjectOfType<GameplayUi>(true).AddScore(1);
        }
    }
    private void CalculateHealth()
    {
        _health = 2 + Mathf.Min(Mathf.FloorToInt( (Time.time - elapsedGameTime) / 15f), 5);
    }
}


