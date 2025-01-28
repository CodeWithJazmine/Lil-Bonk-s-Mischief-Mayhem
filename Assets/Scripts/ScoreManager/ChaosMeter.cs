using UnityEngine;
using UnityEngine.Events;

public class ChaosMeter : MonoBehaviour
{
    private float maxChaos = 100.0f; // Maximum chaos value
    public float chaosIncreasePerBonk = 20.0f; // Chaos added per bonk
    private float currentChaos = 0.0f; // Current chaos level

    [SerializeField] private UnityEngine.UI.Slider chaosMeterSlider; // Reference to UI slider

    public UnityEvent OnChaosMaxed; // Event for when chaos is maxed
    public UnityEvent OnChaosReset; // Event for when chaos resets

    private bool isBonkChainActive = false;
    private float bonkChainTimeout; // Time window to keep chain alive
    private float bonkChainTimer; // Remaining time in the chain

    private void Update()
    {
        if (isBonkChainActive)
        {
            // Decrease Chaos Meter relative to remaining bonkChainTimeout
            float decayRate = maxChaos / bonkChainTimeout; // Full decay over the timeout
            currentChaos -= decayRate * Time.deltaTime;
            currentChaos = Mathf.Clamp(currentChaos, 0.0f, maxChaos);

            // End chain if chaos drains completely
            if (currentChaos <= 0.0f)
            {
                ResetChaos();
            }
        }

        UpdateChaosMeterUI();
    }

    public void AddChaos(float amount)
    {
        if (isBonkChainActive)
        {
            ResetBonkChainTimer(); // Refill Chaos Meter if bonk chain is active
        }
        else
        {
            currentChaos += amount;
            currentChaos = Mathf.Clamp(currentChaos, 0.0f, maxChaos);

            if (currentChaos >= maxChaos)
            {
                StartBonkChain();
            }
        }
    }

    private void StartBonkChain()
    {
        isBonkChainActive = true;
        bonkChainTimer = bonkChainTimeout; // Initialize the chain timer
        OnChaosMaxed?.Invoke(); // Notify listeners
        Debug.Log("Chaos Meter Maxed! Bonk Chain Activated!");
    }

    private void ResetBonkChainTimer()
    {
        bonkChainTimer = bonkChainTimeout; // Reset the timeout timer
        currentChaos = maxChaos; // Refill the Chaos Meter
        Debug.Log("Bonk Chain Timer Reset!");
    }

    public void ResetChaos()
    {
        isBonkChainActive = false;
        currentChaos = 0.0f;
        bonkChainTimer = 0.0f;
        OnChaosReset?.Invoke(); // Notify listeners
        Debug.Log("Chaos Meter Reset!");
    }

    private void UpdateChaosMeterUI()
    {
        if (chaosMeterSlider != null)
        {
            chaosMeterSlider.value = currentChaos / maxChaos;
        }
    }

    public void InitializeBonkChain(float timeout)
    {
        bonkChainTimeout = timeout; // Set the timeout duration for the chain
    }
}
