using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int currentScore = 0;

    private int bonkChainCount = 0;
    private float multiplier = 1.0f;
    [SerializeField] private float multiplierScaler = 0.25f;

    private bool isBonkChainActive = false;

    public float bonkChainTimeout = 2.0f; // Max time between bonks

    public void HandleBonk(int basePoints)
    {
        // Add points with multiplier
        // Update score through GameManager
        GameManager.Instance.UpdateScore(Mathf.CeilToInt(basePoints * multiplier));
        Debug.Log($"Score: {GameManager.Instance.currentScore} (Multiplier: {multiplier})");

        // Update multiplier during active chain
        if (isBonkChainActive)
        {
            bonkChainCount++;
            multiplier = 1.0f + (bonkChainCount * multiplierScaler);
            Debug.Log($"Bonk Chain Streak! Multiplier: {multiplier}");
        }
    }

    public void StartBonkChain()
    {
        isBonkChainActive = true;
        bonkChainCount = 0; // Reset bonk chain count
        multiplier = 1.0f; // Reset multiplier
        Debug.Log("Bonk Chain Started!");
    }

    public void ResetBonkChain()
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
        GameManager.Instance.UpdateScore(currentScore);
        Debug.Log($"Bonus points! Score: {currentScore}");
    }
}
