using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour {
    private float _speed = 3.0f;

    public enum PowerUpType {
        FIRE_RATE = 0,
    }

    [SerializeField] private PowerUpType _type;


    public void SetType(PowerUpType type) {
        _type = type;
    }

    private void Update() {
        var p = transform.position;
        p += Vector3.down * (_speed * Time.deltaTime);
        transform.position = p;
    }

    private void OnTriggerEnter(Collider other) {
        
        var player = other.GetComponent<Player>();
        if (player == null) return;

        player.AddPowerUp(_type);
        Destroy(gameObject);

    }
}
