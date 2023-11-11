using UnityEngine;

public class ScaredGroupSpawner : GroupSpawner
{
    public Rect SpawnArea;

    public override void GetSpawnPosAndRot(out Vector3 pos, out Quaternion rot)
    {
        base.GetSpawnPosAndRot(out _, out rot);

        pos = transform.position +  new Vector3(UnityEngine.Random.Range(SpawnArea.xMin, SpawnArea.xMax), UnityEngine.Random.Range(SpawnArea.yMin, SpawnArea.yMax), transform.position.z);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = new Color(1,1,0,0.5f);
        Gizmos.DrawCube(transform.position + (Vector3)SpawnArea.center, SpawnArea.size);
    }
}
