using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    [SerializeField] private float _speed = 0.0f;
    [SerializeField] private Vector3 _direction = Vector3.up;
    private int _damage = 1;

    public void Init(int damage) {
        _damage = damage;
    }

    void Update() {

        var p = transform.position;
        p += _direction * (_speed * Time.deltaTime);
        transform.position = p;
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
            Destroy(gameObject);
        }
    }
}
