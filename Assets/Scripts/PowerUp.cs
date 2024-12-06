using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour {
    private float _speed = 3.0f;

    private static Queue<PowerUp> powerUpPool = new Queue<PowerUp>();
    public enum PowerUpType {
        FIRE_RATE = 0,
        ADD_HEALTH = 1,
        ADD_SHIELD = 2,
    }

    [SerializeField] private PowerUpType _type;
    [SerializeField] private Renderer _renderer;

    
    public static void InitializePowerUpPool(int poolSize, PowerUp prefab) {
        for (int i = 0; i < poolSize; i++) {
            PowerUp powerUp = Instantiate(prefab);
            powerUp.gameObject.SetActive(false);  // Deactivate initially
            powerUpPool.Enqueue(powerUp);
        }
    }
    public static PowerUp GetFromPool(GameObject prefab) {
        if (powerUpPool.Count > 0) {
            PowerUp powerUp = powerUpPool.Dequeue();
            powerUp.gameObject.SetActive(true);
            return powerUp;
        } else
        {
            var pref = Instantiate(prefab);
            PowerUp powerUp = pref.GetComponent<PowerUp>();
            return powerUp;
        }
    }
    public void ReturnToPool() {
        gameObject.SetActive(false);
        powerUpPool.Enqueue(this);
    }

    public void SetType(PowerUpType type) {
        _type = type;
        SetPowerUpColor();
    }
    private void Start() {
        SetPowerUpColor();
    }
    private void SetPowerUpColor() {
        switch (_type) {
            case PowerUpType.FIRE_RATE:
                _renderer.material.color = Color.red; 
                break;
            case PowerUpType.ADD_HEALTH:
                _renderer.material.color = Color.green;
                break;
            case PowerUpType.ADD_SHIELD:
                _renderer.material.color = Color.blue;
                break;
        }
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
        ReturnToPool();

    }
}
