using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class GroupSpawner : MonoBehaviour
{
    [field: SerializeField]
    public Vector2Int SpawnAmount { get; private set; } = new Vector2Int(8, 12);

    [field: SerializeField]
    public Vector2 SpawnDelay { get; private set; } = new Vector2(0.05f, 0.08f);

    [field: SerializeField]
    public BackgroundFlier Prefab { get; private set; }

    [field: SerializeField]
    public Vector2 GeneralSpawnDirection { get; private set; } = new Vector2(0, 0);

    [field: SerializeField]
    public Vector2 GeneralSpawnVelocity { get; private set; } = new Vector2(10f,10f);

    [field: SerializeField]
    public Vector2 SpawnZPosition { get; private set; } = new Vector2(0f, 0f);

    [field: SerializeField]
    public float GeneralSpawnRadius { get; private set; } = 3f;

    [field: SerializeField]
    public bool StartSpawningOnStart { get; private set; } = false;

    public bool Spawning { get; private set; } = false;


    private void Awake()
    {
        if (StartSpawningOnStart)
        {
            StartSpawning();
        }
    }

    public virtual void StartSpawning()
    {
        Spawning = true;
        StartCoroutine(SpawnRoutine(UnityEngine.Random.Range(SpawnAmount.x, SpawnAmount.y + 1),SpawnDelay.RandomInRange()));
    }

    public virtual void StopSpawning()
    {
        Spawning = false;
        StopAllCoroutines();
    }

    IEnumerator SpawnRoutine(int spawnAmount, float spawnDelay)
    {
        for (int t = 0; t < spawnAmount; t++)
        {
            //var spawnPos = transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * GeneralSpawnRadius);

            GetSpawnPosAndRot(out var pos, out var rot);



            //var spawnRot = Quaternion.Euler(0f,0f, GeneralSpawnDirection.RandomInRange());
            var instance = Pooling.Instantiate(Prefab, pos, rot);

            instance.VelocityRange = GeneralSpawnVelocity;
            yield return new WaitForSeconds(spawnDelay);
        }
        Spawning = false;
    }

    public virtual void GetSpawnPosAndRot(out Vector3 pos, out Quaternion rot)
    {
        pos = transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * GeneralSpawnRadius);

        pos.z += SpawnZPosition.RandomInRange();

        rot = Quaternion.Euler(0f, 0f, GeneralSpawnDirection.RandomInRange());
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f,0f,1f,0.5f);
        Gizmos.DrawWireSphere(transform.position, GeneralSpawnRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, MathUtilities.PolarToCartesian(GeneralSpawnDirection.x, GeneralSpawnVelocity.x));

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, MathUtilities.PolarToCartesian(GeneralSpawnDirection.y, GeneralSpawnVelocity.y));
    }
}
