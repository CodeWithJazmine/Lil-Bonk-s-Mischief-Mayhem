using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Variables")]
    private bool gameStarted = false;
    private bool gameIsOver = false;
    [SerializeField] private float gameTimer = 120.0f; // Game timer in seconds
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("UI")]
    public GameObject activeMenu;
    public GameObject activeCanvas, previousCanvas;
    public GameObject startMenuCanvas, pauseMenuCanvas, optionsMenuCanvas, gameOverCanvas, chaosMeterObj;

    [Header("Score System")]
    public int currentScore = 0;
    public ScoreManager scoreManager;
    public ChaosMeter chaosMeter;
    [SerializeField] private float scoreAnimationDuration = 1.0f;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Audio")]
    public AudioSource bonkSound;
    public AudioSource explosionSound;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }

    private void Start()
    {
        InitializeGame();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reinitialize the game every time the scene is loaded
        InitializeGame();
    }

    private void Update()
    {
        HandleGameTimer();
        HandleMenuInput();
    }

    private void InitializeGame()
    {
        if (startMenuCanvas == null || pauseMenuCanvas == null || optionsMenuCanvas == null || gameOverCanvas == null)
        {
            Debug.Log("Some or all menu canvases not assigned in the GameManager.");
            return;
        }

        if (scoreManager == null)
        {
            Debug.Log("ScoreManager not assigned.");
            return;
        }

        if (chaosMeter == null)
        {
            Debug.Log("ChaosMeter not assigned.");
            return;
        }

        gameIsOver = false;
        currentScore = 0;

        startMenuCanvas.SetActive(true);
        pauseMenuCanvas.SetActive(false);
        optionsMenuCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        scoreText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);


        chaosMeter.InitializeBonkChain(scoreManager.bonkChainTimeout); // Set the timeout for thechain
        chaosMeter.OnChaosMaxed.AddListener(OnChaosMeterMaxed);
        chaosMeter.OnChaosReset.AddListener(OnChaosMeterReset);

        Pause(); // Pause the game until the player starts it
    }

    #region Score System and Chaos Meter

    // HandleBonk: call this when the player successfully bonks a BONKABLE object.
    public void HandleBonk(int points)
    {
        if (scoreManager != null)
        {
            scoreManager.HandleBonk(points);
        }

        if (chaosMeter != null)
        {
            chaosMeter.AddChaos(chaosMeter.chaosIncreasePerBonk);
        }
    }

    // UpdateScore: can be used to update the player's score.
    // Do not call ths function if related to bonks or bonk chain. (Use HandleBonk instead)
    public void UpdateScore(int points)
    {
        int previousScore = currentScore;
        currentScore += points;
        StopCoroutine("AnimateScore");
        StartCoroutine(AnimateScore(previousScore, currentScore, scoreAnimationDuration)); // Adjust duration as needed
    }

    // AnimateScore: can be used to animate the score UI when the player's score changes so it looks more dynamic.
    private IEnumerator AnimateScore(int startValue, int endValue, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float lerpValue = Mathf.Lerp(startValue, endValue, time / duration);
            scoreText.text = Mathf.RoundToInt(lerpValue).ToString("D5"); // with leading zeros for 5 digits
            yield return null;
        }
        scoreText.text = endValue.ToString("D5"); // with leading zeros for 5 digits
    }

    // OnChaosMeterMaxed and OnChaosMeterReset are called by ChaosMeter events.
    private void OnChaosMeterMaxed()
    {
        if (scoreManager != null)
        {
            scoreManager.StartBonkChain();
        }
    }

    private void OnChaosMeterReset()
    {
        if (scoreManager != null)
        {
            scoreManager.ResetBonkChain();
        }
    }

    #endregion // End of Score System

    #region Menu System

    private void HandleMenuInput()
    {
        // Start the game when the player presses Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            startMenuCanvas.SetActive(false);
            gameStarted = true;
            Unpause();
        }

        // Restart game on game over
        if (gameIsOver && Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }

        // Open pause menu
        if ((Input.GetButtonDown("Cancel") || (Input.GetKeyDown(KeyCode.P))) && gameStarted && activeMenu == null && activeCanvas == null)
        {
            pauseMenuCanvas.SetActive(true);
            activeCanvas = pauseMenuCanvas;
            previousCanvas = activeCanvas;
            Pause();
        }
        // Close pause menu
        else if ((Input.GetButtonDown("Cancel") || (Input.GetKeyDown(KeyCode.P))) && activeCanvas == pauseMenuCanvas)
        {
            pauseMenuCanvas.SetActive(false);
            activeCanvas = null;
            Unpause();
        }
        // Close options menu
        else if ((Input.GetButtonDown("Cancel") || (Input.GetKeyDown(KeyCode.P))) && activeCanvas == optionsMenuCanvas)
        {
            optionsMenuCanvas.SetActive(false);
            activeCanvas = previousCanvas;
            activeCanvas.SetActive(true);
        }
    }
    private void Pause()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        chaosMeterObj.SetActive(false);
        scoreText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
    }

    public void Unpause()
    {
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (activeMenu != null)
        {
            activeMenu.SetActive(false);
        }

        chaosMeterObj.SetActive(true);
        scoreText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);

        activeMenu = null;
        activeCanvas = null;
        previousCanvas = null;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Unpause();
    }

    public void OptionsMenu()
    {
        previousCanvas = activeCanvas;

        optionsMenuCanvas.SetActive(true);
        activeCanvas = optionsMenuCanvas;

        if (previousCanvas != null)
        {
            previousCanvas.SetActive(false);
        }
    }
    #endregion

    #region Game Over and Timer
    public void GameOver()
    {
        gameIsOver = true;

        if (gameOverCanvas != null)
        { 
            gameOverCanvas.SetActive(true);
        }
        activeCanvas = gameOverCanvas;

        Pause();
       
        Debug.Log("Game Over! Time's up!");
    }

    private void HandleGameTimer()
    {
        if (gameStarted && !gameIsOver)
        {
            if (gameTimer > 0)
            {
                gameTimer -= Time.deltaTime;

                // convert the timer to minutes, seconds, and milliseconds
                int minutes = Mathf.FloorToInt(gameTimer / 60);
                int seconds = Mathf.FloorToInt(gameTimer % 60);
                int milliseconds = Mathf.FloorToInt((gameTimer * 100) % 100);

                // update the timer text
                timerText.text = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
            }
            else
            {
                gameTimer = 0;
                timerText.text = "00:00:00"; // make sure the timer displays 00:00:00 
                GameOver();
            }
        }
    }
    #endregion

    public void PlayBonkSound()
    {
        bonkSound.Play();
    }

    public void PlayExplosionSound()
    {
        explosionSound.Play();
    }
}
