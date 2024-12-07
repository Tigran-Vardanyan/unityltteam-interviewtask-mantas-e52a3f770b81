using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject prefabPowerUp;
    [SerializeField] private GameObject prefabExplosion;
    [SerializeField] private Projectile prefabProjectile;
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private int baseHealth = 2;
    [SerializeField] private float healthIncreaseInterval = 15f;
    [SerializeField] private float fireChance = 0.4f;
    [SerializeField] private float fireInterval = 2.5f;
    [SerializeField] private float powerUpSpawnChance = 0.1f;
    [SerializeField] private int enemyDamage;

    private Rigidbody body;
    public int health;
    private bool canFire;
    private float fireTimer;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        ResetEnemy();
       
    }

    private void Update()
    {
        HandleFiring();
        CheckOutOfViewport();
    }

    private void FixedUpdate()
    {
        MoveEnemy();
    }

    public void ResetEnemy()
    {
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        health = baseHealth + Mathf.Min(Mathf.FloorToInt(Time.timeSinceLevelLoad/ healthIncreaseInterval), 5);
        canFire = Random.value < fireChance;
        fireTimer = 0;
    }

    private void HandleFiring()
    {
        if (canFire)
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireInterval)
            {
                var projectile = Projectile.GetFromPool(prefabProjectile.gameObject,false);
                projectile.Init(false,enemyDamage); // Enemy projectile
                projectile.transform.position = transform.position;
                fireTimer -= fireInterval;
            }
        }
    }

    private void CheckOutOfViewport()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0)
        {
            EnemySpawner.Instance.ReturnEnemyToPool(this);
        }
    }

    private void MoveEnemy()
    {
        var position = body.position;
        position += Vector3.down * (speed * Time.deltaTime);
        body.MovePosition(position);
    }

    public void Hit(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            GameController.Instance.AddScore(1);
            Explode();
        }
    }

    private void Explode()
    {
        var explosion = Instantiate(prefabExplosion);
        explosion.transform.position = transform.position;

        if (Random.value < powerUpSpawnChance)
        {
            var powerUp = PowerUp.GetFromPool(prefabPowerUp);
            if (powerUp != null)
            {
                var types = Enum.GetValues(typeof(PowerUp.PowerUpType)).Cast<PowerUp.PowerUpType>().ToList();
                powerUp.SetType(types[Random.Range(0, types.Count)]);
                powerUp.transform.position = transform.position;
            }
        }

        EnemySpawner.Instance.ReturnEnemyToPool(this);
        FindObjectOfType<GameplayUi>(true)?.AddScore(1);
    }
}