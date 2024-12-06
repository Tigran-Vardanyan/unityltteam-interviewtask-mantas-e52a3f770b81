using System;
using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour {
    [SerializeField] private float _speed = 0.0f;
    [SerializeField] private Vector3 _direction = Vector3.up;
    [SerializeField] private float _lifetime = 5.0f;
    private int _damage = 1;

    private Camera _camera;
    private float _screenEdgePadding = 1f;

    // Added a field to identify the type of projectile (Player or Enemy)
    private bool _isPlayerProjectile = false;

    private void Start() {
        _camera = Camera.main;
    }

    // Initialize method now takes a type to distinguish between player/enemy projectiles
    public void Init(int damage, bool isPlayerProjectile) {
        _damage = damage;
        _isPlayerProjectile = isPlayerProjectile;  // Set if this is a player projectile
    }

    void Update() {
        MoveProjectile();
        CheckOutOfBounds();
    }

    private void MoveProjectile() {
        var p = transform.position;
        p += _direction * (_speed * Time.deltaTime);
        transform.position = p;
    }

    private void CheckOutOfBounds() {
        Vector3 viewportPos = _camera.WorldToViewportPoint(transform.position);

        if (viewportPos.x < -_screenEdgePadding || viewportPos.x > 1 + _screenEdgePadding ||
            viewportPos.y < -_screenEdgePadding || viewportPos.y > 1 + _screenEdgePadding) {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other) {
        bool destroy = false;
        
        var enemy = other.GetComponent<Enemy>();
        if (enemy != null) {
            enemy.Hit(_damage);
            destroy = true;
        }
        else {
            var player = other.GetComponent<Player>();
            if (player != null) {
                player.Hit();
                destroy = true;
            }
        }

        if (destroy) {
            ReturnToPool();
        }
    }
    
    private void ReturnToPool() {
        if (_isPlayerProjectile) {
            Player.Instance.ReturnProjectileToPool(this);  // Player pool
        }
    }
}