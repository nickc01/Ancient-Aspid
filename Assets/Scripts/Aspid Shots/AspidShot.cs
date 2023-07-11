using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class AspidShot : AspidShotBase
{

    public static AspidShot Spawn(AspidShot prefab, Vector3 position, float angleDegrees, float velocity)
    {
        return Spawn(prefab, position, new Vector2(Mathf.Cos(angleDegrees * Mathf.Deg2Rad) * velocity, Mathf.Sin(angleDegrees * Mathf.Deg2Rad) * velocity));
    }

    public static AspidShot Spawn(AspidShot prefab, Vector3 position, Vector3 target, float time)
    {
        return Spawn(prefab, position, target, time, AncientAspidPrefabs.Instance.AspidShotPrefab.Rigidbody.gravityScale);
    }

    public static AspidShot Spawn(AspidShot prefab, Vector3 position, Vector3 target, float time, float gravityScale)
    {
        return Spawn(prefab, position, MathUtilities.CalculateVelocityToReachPoint(position, target, time, gravityScale));
    }

    public static AspidShot Spawn(AspidShot prefab, Vector3 position, Vector2 velocity)
	{
		var instance = Pooling.Instantiate(prefab, position, Quaternion.identity);

		instance.Rigidbody.velocity = velocity;

		return instance;
	}

    public static AspidShot Spawn(Vector3 position, Vector2 velocity)
    {
        var instance = Pooling.Instantiate(AncientAspidPrefabs.Instance.AspidShotPrefab, position, Quaternion.identity);

        instance.Rigidbody.velocity = velocity;

        return instance;
    }

    public static AspidShot Spawn(Vector3 position, float angleDegrees, float velocity)
	{
		return Spawn(position, new Vector2(Mathf.Cos(angleDegrees * Mathf.Deg2Rad) * velocity, Mathf.Sin(angleDegrees * Mathf.Deg2Rad) * velocity));
	}

	public static AspidShot Spawn(Vector3 position, Vector3 target, float time)
	{
		return Spawn(position, target, time, AncientAspidPrefabs.Instance.AspidShotPrefab.Rigidbody.gravityScale);
	}

	public static AspidShot Spawn(Vector3 position, Vector3 target, float time, float gravityScale)
	{
		return Spawn(position, MathUtilities.CalculateVelocityToReachPoint(position, target, time, gravityScale));
	}
}
