using UnityEngine;

public class CollectibleLetter : MonoBehaviour
{
    public string letter; // The specific letter (e.g., "B", "O", "N", "K")

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure only the player can collect
        {
            Collect();
        }
    }

    private void Collect()
    {
        Debug.Log($"Collected Letter: {letter}");

        // Notify a game manager or score system
        GameManager.Instance.scoreManager.CollectLetter(letter);

        // Destroy the letter object
        Destroy(gameObject);
    }
}