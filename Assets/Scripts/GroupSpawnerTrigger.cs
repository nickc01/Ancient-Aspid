using UnityEngine;
using WeaverCore.Utilities;

public class GroupSpawnerTrigger : MonoBehaviour
{
    [field: SerializeField]
    public GroupSpawner Spawner { get; private set; }

    [field: SerializeField]
    public int SpawnTriggerAmounts { get; private set; } = 1;

    [field: SerializeField]
    public Vector2 RetriggerDelayRange = new Vector2(15, 20f);

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Hero Detector");
    }

    private void Reset()
    {
        Spawner = GetComponentInChildren<GroupSpawner>();
    }

    float lastSpawnTime = 0;
    float currentRetryDelay = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (SpawnTriggerAmounts > 0 && Time.time >= lastSpawnTime + currentRetryDelay)
        {
            SpawnTriggerAmounts--;
            lastSpawnTime = Time.time;
            currentRetryDelay = RetriggerDelayRange.RandomInRange();
            Spawner.StartSpawning();
        }
    }
}
