using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Owns the gameplay loop: start / play / death / retry, score, and coin payout into
/// SaveData (so playing funds the shop). Lives on an always-active object so UIManager
/// and the obstacles can reach it via Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private enum State { Idle, Playing, Dead }

    [Header("Gameplay refs")]
    [SerializeField] private GameObject gameplayRoot;
    [SerializeField] private DartController player;
    [SerializeField] private ObstacleSpawner spawner;
    [SerializeField] private Vector2 playerStart = new Vector2(-1.2f, 0f);

    [Header("HUD")]
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private GameObject gameOverGroup;
    [SerializeField] private TMP_Text gameOverLabel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;

    [Header("Economy")]
    [SerializeField] private int coinsPerScore = 1;

    private State state = State.Idle;
    private int score;

    public bool IsPlaying => state == State.Playing;
    public float PlayerX => player != null ? player.transform.position.x : -1.2f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (retryButton != null) { retryButton.onClick.RemoveAllListeners(); retryButton.onClick.AddListener(StartGame); }
        if (menuButton != null)  { menuButton.onClick.RemoveAllListeners();  menuButton.onClick.AddListener(ReturnToMenu); }
        if (gameplayRoot != null) gameplayRoot.SetActive(false);
        if (scoreLabel != null) scoreLabel.gameObject.SetActive(false);
        if (gameOverGroup != null) gameOverGroup.SetActive(false);
    }

    /// <summary>Begin (or restart) a run. Called by UIManager on Start and by the Retry button.</summary>
    public void StartGame()
    {
        if (gameplayRoot != null) gameplayRoot.SetActive(true);
        if (gameOverGroup != null) gameOverGroup.SetActive(false);
        score = 0;
        UpdateScoreLabel();
        if (scoreLabel != null) scoreLabel.gameObject.SetActive(true);
        if (spawner != null) spawner.ResetSpawner();
        if (player != null) player.Begin(playerStart);
        state = State.Playing;
    }

    public void AddScore()
    {
        score++;
        UpdateScoreLabel();
    }

    public void OnPlayerDied()
    {
        if (state != State.Playing) return;
        state = State.Dead;
        if (player != null) player.StopControl();

        int reward = score * coinsPerScore;
        SaveData.Coins += reward;
        if (score > SaveData.BestScore) SaveData.BestScore = score;

        if (scoreLabel != null) scoreLabel.gameObject.SetActive(false);
        if (gameOverGroup != null) gameOverGroup.SetActive(true);
        if (gameOverLabel != null)
            gameOverLabel.text = "GAME OVER\nScore  " + score + "\nBest  " + SaveData.BestScore + "\n+" + reward + " coins";
    }

    public void ReturnToMenu()
    {
        state = State.Idle;
        if (gameplayRoot != null) gameplayRoot.SetActive(false);
        if (gameOverGroup != null) gameOverGroup.SetActive(false);
        if (scoreLabel != null) scoreLabel.gameObject.SetActive(false);
        if (UIManager.Instance != null) UIManager.Instance.OpenMainMenu();
    }

    private void UpdateScoreLabel()
    {
        if (scoreLabel != null) scoreLabel.text = score.ToString();
    }
}
