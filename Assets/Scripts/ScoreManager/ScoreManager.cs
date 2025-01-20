using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int currentScore;
    private int chainBonkCount;
    private float multiplier;
    private bool isChainBonkActive;
    public float chainBonkTimeout = 2.0f;
    private float chainBonkTimer;

    [ContextMenu("Add Debug Score")]
    public void DebugAddScore()
    {
        AddScore(100); // Example value for testing
    }
    public void AddScore(int basePoints)
    {
        // === TODO: Remove this multipler line and implement a proper multiplier system ===
        multiplier = 1.0f;
        // === This multiplier is just for testing purposes ===

        currentScore += Mathf.CeilToInt(basePoints * multiplier);
        Debug.Log($"Score: {currentScore} (Multipler: {multiplier})");
    }
}
