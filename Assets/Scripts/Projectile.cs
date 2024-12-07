using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private Vector3 direction = Vector3.up;
    [SerializeField] private float lifetime = 5.0f;

    private int damage;
    private bool _isPlayerProjectile;
    private Camera camera;
    private static Queue<Projectile> enemyProjectilePool = new Queue<Projectile>();
    private static Queue<Projectile> playerProjectilePool = new Queue<Projectile>();

    private void Start()
    {
        camera = Camera.main;
    }

    public static Projectile GetFromPool(GameObject prefab, bool isPlayerProjectile)
    {
        if (isPlayerProjectile)
        {
            if (playerProjectilePool.Count > 0)
            {
                var projectile = playerProjectilePool.Dequeue();
                projectile.gameObject.SetActive(true);
                return projectile;
            }
        }
        else
        {
            if (enemyProjectilePool.Count > 0)
            {
                var projectile = enemyProjectilePool.Dequeue();
                projectile.gameObject.SetActive(true);
                return projectile;
            }
        }
        
        var newProjectile = Instantiate(prefab).GetComponent<Projectile>();
        newProjectile._isPlayerProjectile = isPlayerProjectile;
        return newProjectile;
    }

    public void ReturnToPool(bool isPlayerProjectile)
    {
        gameObject.SetActive(false);
        if (isPlayerProjectile)
        {
            playerProjectilePool.Enqueue(this);
        }
        else
        {
            enemyProjectilePool.Enqueue(this);
        }
    }

    public void Init(bool isPlayerProjectile, int damage) {
        _isPlayerProjectile = isPlayerProjectile;
        this.damage = damage;
    }

    private void Update()
    {
        transform.position += direction * (speed * Time.deltaTime);
        CheckOutOfBounds();
    }

    private void CheckOutOfBounds()
    {
        Vector3 viewportPos = camera.WorldToViewportPoint(transform.position);
        if (viewportPos.x < -1 || viewportPos.x > 2 || viewportPos.y < -1 || viewportPos.y > 2)
        {
            ReturnToPool(_isPlayerProjectile);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (_isPlayerProjectile) {
            // Player projectiles
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null) {
                enemy.Hit(damage);
                ReturnToPool(_isPlayerProjectile);
            }
        } else {
            // Enemy projectiles
            var player = other.GetComponent<Player>();
            if (player != null) {
                player.Hit();
                ReturnToPool(_isPlayerProjectile);
            }
        }
    }
}