using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public Transform spawnPoint;
    public Transform[] waypoints;
    public float spawnTime = 30f;
    float currentSpawnTime;
    
    void Start()
    {
        GameManager.Instance.transform.GetComponent<ChaosMeter>().OnChaosMaxed.AddListener(OnChaosMaxed);
    }

    private void Update()
    {
        currentSpawnTime += Time.deltaTime;
        if (currentSpawnTime > spawnTime)
        {
            Spawn();
            currentSpawnTime = 0;
        }
    }

    void OnChaosMaxed()
    {
        Spawn();
    }

    void Spawn()
    {
        // Spawn a prefab
        GameObject go = Instantiate(EnemyPrefab, spawnPoint.position, spawnPoint.rotation);
        go.GetComponent<EnemyStateMachine>().waypoints = waypoints;
    }
}
