using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class Player : MonoBehaviour {

    public static Player Instance { get; private set; }
    public event System.Action OnDie;
    
    [SerializeField] private GameObject _prefabExplosion;
    [SerializeField] private Projectile _prefabProjectile;
    [SerializeField] private Transform _projectileSpawnLocation;

    private int _health = 3;
    
    private Rigidbody _body = null;
    
    private Vector2 _lastInput;
    private bool _hasInput = false;
    
    public float fireInterval = 0.4f;
    private float _fireTimer = 0.0f;
    private Vector2 playAreaMin = new Vector2(-3f, -5f); // Minimum bounds
    private Vector2 playAreaMax = new Vector2(3f, 5f); // Maximum bounds
    public float moveSpeed = 5f;
    private void Awake() {
        _body = GetComponent<Rigidbody>();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start() {
        Object.FindObjectOfType<GameplayUi>(true).UpdateHealth(_health);
        Object.FindObjectOfType<GameOverUi>(true).Close();
    }

    private void Update() {

        HandleMouseInput();
        FireProjectile();
    }
    private void HandleMouseInput()
    {
        if (Input.GetMouseButton(0))
        {
            // Get mouse position and convert to world point
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));

            // Set the target position and clamp within bounds
            Vector3 targetPosition = new Vector3(
                Mathf.Clamp(mousePosition.x, playAreaMin.x, playAreaMax.x),
                Mathf.Clamp(mousePosition.y, playAreaMin.y, playAreaMax.y),
                0
            );

            // Smoothly move towards the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }
    private void FireProjectile()
    {
        _fireTimer += Time.deltaTime;
        if (_fireTimer >= fireInterval)
        {
            var go = Instantiate(_prefabProjectile);
            go.transform.position = _projectileSpawnLocation.position;
            _fireTimer -= fireInterval;
        }
    }


    public void Hit() {

        _health--;

        Object.FindObjectOfType<GameplayUi>(true).UpdateHealth(_health);

        if (_health <= 0) {
            Object.FindObjectOfType<GameOverUi>(true).Open();
            var fx = Instantiate(_prefabExplosion);
            fx.transform.position = transform.position;
            Destroy(gameObject);
            OnDie?.Invoke();
            return;
        }
    }

    // private void FixedUpdate() {
    //     if (_hasInput) {
    //         Vector2 pos = _lastInput;
    //         const float playAreaMin = -3f;
    //         const float playAreaMax = 3f;
    //
    //         var p = pos.x / Screen.width;
    //         _body.MovePosition(new Vector3(Mathf.Lerp(playAreaMin, playAreaMax, p), 0.0f, 0.0f));
    //     }
    // }

    public void AddPowerUp(PowerUp.PowerUpType type) {

        if (type == PowerUp.PowerUpType.FIRE_RATE) {
            fireInterval *= 0.5f;
        }
    }
}