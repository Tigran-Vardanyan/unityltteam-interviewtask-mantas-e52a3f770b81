using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using URPGlitch.Runtime.AnalogGlitch;
using URPGlitch.Runtime.DigitalGlitch;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private Projectile prefabProjectile;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private GameObject shield;
    [SerializeField] private int playerDamage;
    
    [SerializeField] private Volume globalVolume;

    public int currentHealth;
    private float fireCooldown;
    private bool _hasShield = false;
    private VolumeProfile volumeProfile;
    private Camera _camera;
    private Vector3 targetPosition;

    private void Awake()
    {
        currentHealth = maxHealth;
        if (globalVolume != null)
        {
            volumeProfile = globalVolume.profile;
        }
        _camera = Camera.main;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleShooting();
        shield.SetActive(_hasShield);
    }

    private void HandleMovement()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Mathf.Abs(_camera.transform.position.z);

            targetPosition = _camera.ScreenToWorldPoint(mousePosition);

            Vector3 screenMin = _camera.ScreenToWorldPoint(new Vector3(3f, 0.5f, mousePosition.z));
            Vector3 screenMax = _camera.ScreenToWorldPoint(new Vector3(Screen.width - 3f, Screen.height - 0.5f, mousePosition.z));

            targetPosition = new Vector3(
                Mathf.Clamp(targetPosition.x, screenMin.x, screenMax.x),
                Mathf.Clamp(targetPosition.y, screenMin.y, screenMax.y),
                transform.position.z);

            float distance = Vector3.Distance(transform.position, targetPosition);

            if (distance > 0.01f)
            {
                Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
                transform.position = newPosition;

                float normalizedDistance = Mathf.Clamp01(distance / speed);
                float targetRotationY = Mathf.Lerp(0, targetPosition.x > transform.position.x ? -20 : 20, normalizedDistance);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, targetRotationY, 0), Time.deltaTime * 5f);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    private void HandleShooting()
    {
        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0)
        {
            var projectile = Projectile.GetFromPool(prefabProjectile.gameObject,true);
            projectile.Init( true,playerDamage); // Player projectile
            projectile.transform.position = projectileSpawnPoint.position;
            fireCooldown = fireRate;
        }
    }

    public void Hit()
    {
        if (_hasShield)
        {
            _hasShield = false;
        }
        else
        {
            currentHealth--;
            GameController.Instance.UpdateHeath(currentHealth);
            if (volumeProfile.TryGet<AnalogGlitchVolume>(out var analogGlitch))
            {
                StartCoroutine(AnimateGlitchParameters(analogGlitch, 0.6f));
            }

            if (volumeProfile.TryGet<DigitalGlitchVolume>(out var digitalGlitch))
            {
                StartCoroutine(AnimateGlitchParameters(digitalGlitch, 0.4f));
            }

            if (currentHealth <= 0)
            {
                GameController.Instance.PlayerDied();
            }
        }

    }

    public void AddPowerUp(PowerUp.PowerUpType type)
    {
        switch (type)
        {
            case PowerUp.PowerUpType.FIRE_RATE:
                fireRate = Mathf.Max(0.2f, fireRate - 0.1f);
                break;
            case PowerUp.PowerUpType.ADD_HEALTH:
                currentHealth = Mathf.Min(maxHealth, currentHealth + 1);
                GameController.Instance.UpdateHeath(currentHealth);
                break;
            case PowerUp.PowerUpType.ADD_SHIELD:
                _hasShield = true;
                StartCoroutine(ShieldEffect());
                break;
        }
    }
    
    private IEnumerator ShieldEffect()
    {
        yield return new WaitForSeconds(5f);
        _hasShield = false;
    }
      private IEnumerator AnimateGlitchParameters(AnalogGlitchVolume analogGlitch, float duration)
    {
        float elapsedTime = 0f;
        float halfDuration = duration / 2f;
        float maxIntensity = 0.8f;

        while (elapsedTime < halfDuration)
        {
            float currentIntensity = Mathf.Lerp(0f, maxIntensity, elapsedTime / halfDuration);
            analogGlitch.scanLineJitter.Override(currentIntensity);
            analogGlitch.horizontalShake.Override(currentIntensity);
            analogGlitch.colorDrift.Override(currentIntensity);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            float currentIntensity = Mathf.Lerp(maxIntensity, 0f, elapsedTime / halfDuration);
            analogGlitch.scanLineJitter.Override(currentIntensity);
            analogGlitch.horizontalShake.Override(currentIntensity);
            analogGlitch.colorDrift.Override(currentIntensity);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        analogGlitch.scanLineJitter.Override(0f);
        analogGlitch.horizontalShake.Override(0f);
        analogGlitch.colorDrift.Override(0f);
    }

    private IEnumerator AnimateGlitchParameters(DigitalGlitchVolume digitalGlitch, float duration)
    {
        float elapsedTime = 0f;
        float halfDuration = duration / 2f;
        float maxIntensity = 0.4f;

        while (elapsedTime < halfDuration)
        {
            float currentIntensity = Mathf.Lerp(0f, maxIntensity, elapsedTime / halfDuration);
            digitalGlitch.intensity.Override(currentIntensity);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            float currentIntensity = Mathf.Lerp(maxIntensity, 0f, elapsedTime / halfDuration);
            digitalGlitch.intensity.Override(currentIntensity);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        digitalGlitch.intensity.Override(0f);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Hit();
            Enemy enemy = collision.transform.GetComponent<Enemy>();
            enemy.Hit(enemy.health);
        }
    }
}