using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private Player player;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField]private GameplayUi _gameplayUi;
    [SerializeField]private GameOverUi _gameOverUi;

    private int score;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        score = 0;
        enemySpawner.StartSpawning();
        player.gameObject.SetActive(true);
        _gameplayUi.UpdateHealth(player.currentHealth);
        _gameOverUi.Close();
    }

    public void UpdateHeath(int health)
    {
        _gameplayUi.UpdateHealth(health);
    }

    public void PlayerDied()
    {
        enemySpawner.StopSpawning();
        player.gameObject.SetActive(false);
        _gameOverUi.Open(score);
    }

    public void AddScore(int value)
    {
        score += value;
        Debug.Log("Score: " + score);
    }
}