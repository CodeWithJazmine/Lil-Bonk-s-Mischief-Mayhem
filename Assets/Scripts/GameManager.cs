using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score System")]
    public int currentScore = 0;
    public ScoreManager scoreManager;
    public ChaosMeter chaosMeter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (scoreManager == null)
        {
            Debug.Log("ScoreManager not assigned.");
        }
        if (chaosMeter == null)
        {
            Debug.Log("ChaosMeter not assigned.");
        }
        else
        {
            chaosMeter.InitializeBonkChain(scoreManager.bonkChainTimeout); // Set the timeout for the chain
            chaosMeter.OnChaosMaxed.AddListener(OnChaosMeterMaxed);
            chaosMeter.OnChaosReset.AddListener(OnChaosMeterReset);
        }

        InitializeGame();
    }

    private void InitializeGame()
    {
        currentScore = 0;
    }

#region Score System

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
        currentScore += points;
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
}
