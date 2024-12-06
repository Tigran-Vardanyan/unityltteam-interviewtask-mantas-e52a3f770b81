using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using URPGlitch.Runtime.AnalogGlitch;
using URPGlitch.Runtime.DigitalGlitch;
using Object = UnityEngine.Object;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public event System.Action OnDie;

    [SerializeField] private GameObject _prefabExplosion;
    [SerializeField] private Projectile _prefabProjectile;
    [SerializeField] private Transform _projectileSpawnLocation;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private GameObject shield;
    [SerializeField] private PowerUp _prefabPowerUp;

    private VolumeProfile volumeProfile;
    private int _health;
    private Vector3 targetPosition;

    private Rigidbody _body;
    private Vector2 _lastInput;
    private bool _hasInput = false;

    public float fireInterval = 0.4f;
    private float _fireTimer = 0.0f;
    public float moveSpeed = 5f;
    private Camera _camera;
    private bool _isDead = false;
    private bool _hasShield = false;
    private Queue<Projectile> projectilePool = new Queue<Projectile>();
    
    private GameplayUi _gameplayUi;
    private GameOverUi _gameOverUi;

    [SerializeField] private int poolSize = 10;

    private void Awake()
    {
        _camera = Camera.main;
        _body = GetComponent<Rigidbody>();
        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeProjectilePool();
        PowerUp.InitializePowerUpPool(10, _prefabPowerUp);
    }

    void Start()
    {
        _gameplayUi = FindObjectOfType<GameplayUi>();
        _gameOverUi = FindObjectOfType<GameOverUi>(true);
        _health = maxHealth;
        _gameplayUi.UpdateHealth(_health);
        _gameOverUi.Close();

        if (globalVolume != null)
        {
            volumeProfile = globalVolume.profile;
        }
    }

    private void FixedUpdate()
    {
        if (!_isDead)
        {
            HandleInput();
            FireProjectile();
        }

        shield.SetActive(_hasShield);
    }

    private void HandleInput()
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
                Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                transform.position = newPosition;

                float normalizedDistance = Mathf.Clamp01(distance / moveSpeed);
                float targetRotationY = Mathf.Lerp(0, targetPosition.x > transform.position.x ? -20 : 20, normalizedDistance);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, targetRotationY, 0), Time.deltaTime * 5f);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    private void FireProjectile()
    {
        _fireTimer += Time.deltaTime;
        if (_fireTimer >= fireInterval)
        {
            Projectile projectile = GetProjectileFromPool();
            projectile.transform.position = _projectileSpawnLocation.position;
            _fireTimer -= fireInterval;
        }
    }

    private Projectile GetProjectileFromPool()
    {
        if (projectilePool.Count > 0)
        {
            Projectile projectile = projectilePool.Dequeue();
            projectile.gameObject.SetActive(true);
            return projectile;
        }
        else
        {
            return Instantiate(_prefabProjectile);
        }
    }

    public void ReturnProjectileToPool(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
        projectilePool.Enqueue(projectile);
    }

    private void InitializeProjectilePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            Projectile projectile = Instantiate(_prefabProjectile);
            projectile.gameObject.SetActive(false);
            projectilePool.Enqueue(projectile);
        }
    }

    public void Hit()
    {
        if (_isDead) return;

        if (_hasShield)
        {
            _hasShield = false;
        }
        else
        {
            _health--;
            if (volumeProfile.TryGet<AnalogGlitchVolume>(out var analogGlitch))
            {
                StartCoroutine(AnimateGlitchParameters(analogGlitch, 0.6f));
            }

            if (volumeProfile.TryGet<DigitalGlitchVolume>(out var digitalGlitch))
            {
                StartCoroutine(AnimateGlitchParameters(digitalGlitch, 0.4f));
            }

            _gameplayUi.UpdateHealth(_health);

            if (_health <= 0)
            {
                _gameplayUi.gameObject.SetActive(false);
                _gameOverUi.Open(_gameplayUi._score);
                var fx = Instantiate(_prefabExplosion);
                fx.transform.position = transform.position;
                Destroy(gameObject);
                OnDie?.Invoke();
            }
        }
    }

    public void DisableInput()
    {
        _isDead = true;
    }

    public void AddPowerUp(PowerUp.PowerUpType type)
    {
        if (_isDead) return;

        switch (type)
        {
            case PowerUp.PowerUpType.FIRE_RATE:
                fireInterval *= 0.5f;
                break;

            case PowerUp.PowerUpType.ADD_HEALTH:
                if (_health < maxHealth)
                {
                    _health++;
                    Object.FindObjectOfType<GameplayUi>(true).UpdateHealth(_health);
                }
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
            enemy.Hit(enemy._health);
        }
    }
}