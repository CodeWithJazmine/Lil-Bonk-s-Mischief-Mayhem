using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private HashSet<string> collectedLetters = new HashSet<string>();
    private string[] requiredLetters = { "B", "O", "N", "K" };


    public int currentScore = 0;

    private int bonkChainCount = 0;
    private float multiplier = 1.0f;
    [SerializeField] private float multiplierScaler = 0.1f;

    private bool isBonkChainActive = false;

    [SerializeField] private float bonkChainTimeout = 2.0f; // Max time between bonks
    
    private ChaosMeter chaosMeter;



    private void Awake()
    {
        // Ensure there is only one instance of the GameManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Find the ChaosMeter and subscribe to its events
        chaosMeter = FindFirstObjectByType<ChaosMeter>();
        if (chaosMeter != null)
        {
            chaosMeter.InitializeBonkChain(bonkChainTimeout); // Set the timeout for the chain
            chaosMeter.OnChaosMaxed.AddListener(StartBonkChain);
            chaosMeter.OnChaosReset.AddListener(ResetBonkChain);
        }
    }

    public void HandleBonk(int basePoints)
    {
        // Add points with multiplier
        currentScore += Mathf.CeilToInt(basePoints * multiplier);
        Debug.Log($"Score: {currentScore} (Multiplier: {multiplier})");

        // Add to Chaos Meter
        if (chaosMeter != null)
        {
            chaosMeter.AddChaos(chaosMeter.chaosIncreasePerBonk);
        }

        // Update multiplier during active chain
        if (isBonkChainActive)
        {
            bonkChainCount++;
            multiplier = 1.0f + (bonkChainCount * multiplierScaler);
            Debug.Log($"Bonk Chain Streak! Multiplier: {multiplier}");
        }
    }

    private void StartBonkChain()
    {
        isBonkChainActive = true;
        bonkChainCount = 0; // Reset bonk chain count
        multiplier = 1.0f; // Reset multiplier
        Debug.Log("Bonk Chain Started!");
    }

    private void ResetBonkChain()
    {
        isBonkChainActive = false;
        bonkChainCount = 0;
        multiplier = 1.0f;
        Debug.Log("Bonk Chain Reset!");
    }

    // AddBonus: can be used to add bonus points to the player's score. 
    // For example: Call this function after player finds all hidden BONK letters.
    public void AddBonus(int bonusPoints)
    {
        currentScore += bonusPoints;
        Debug.Log($"Bonus points! Score: {currentScore}");
    }



    public void CollectLetter(string letter)
    {
        if (!collectedLetters.Contains(letter))
        {
            collectedLetters.Add(letter);
            Debug.Log($"You collected: {letter}");

            // Check if all letters are collected
            if (collectedLetters.Count == requiredLetters.Length)
            {
                CompleteObjective();
            }
        }
    }

  


  // Debugging 
[ContextMenu("Add Debug Score")]
    public void DebugOnPlayerBonk()
    {
        HandleBonk(100); 
    }




    private void CompleteObjective()
    {
        Debug.Log("BONK Completed! All letters collected!");
        // Trigger a reward or progression system here
    }

}

