using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Power-Up Settings")]
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private Renderer renderer;

    public enum PowerUpType
    {
        FIRE_RATE,
        ADD_HEALTH,
        ADD_SHIELD
    }

    private PowerUpType type;
    private static Queue<PowerUp> powerUpPool = new Queue<PowerUp>();

    public static void InitializePool(int size, GameObject prefab)
    {
        for (int i = 0; i < size; i++)
        {
            var instance = Instantiate(prefab).GetComponent<PowerUp>();
            instance.gameObject.SetActive(false);
            powerUpPool.Enqueue(instance);
        }
    }

    public static PowerUp GetFromPool(GameObject prefab)
    {
        if (powerUpPool.Count > 0)
        {
            var powerUp = powerUpPool.Dequeue();
            powerUp.gameObject.SetActive(true);
            return powerUp;
        }

        return Instantiate(prefab).GetComponent<PowerUp>();
    }

    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        powerUpPool.Enqueue(this);
    }

    public void SetType(PowerUpType powerUpType)
    {
        type = powerUpType;
        renderer.material.color = type switch
        {
            PowerUpType.FIRE_RATE => Color.red,
            PowerUpType.ADD_HEALTH => Color.green,
            PowerUpType.ADD_SHIELD => Color.blue,
            _ => Color.white
        };
    }

    private void Update()
    {
        transform.position += Vector3.down * (speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player>(out var player))
        {
            player.AddPowerUp(type);
            ReturnToPool();
        }
    }
}