using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class Bomb : MonoBehaviour
{
    //public CorruptedKin SourceBoss { get; set; }
    //static WeaverCore.ObjectPool ScuttlerBombPool;

    [SerializeField]
    [Tooltip("The angular velocity in degrees per second")]
    protected float angularVelocity = 45f;
    [SerializeField]
    protected LayerMask collisionMask;

    protected new Rigidbody2D rigidbody;
    protected new Collider2D collider;
    protected new SpriteRenderer renderer;
    protected ParticleSystem particles;
    protected PoolableObject poolComponent;

    [SerializeField]
    float explosionScale = 1.25f;

    /*[SerializeField]
    float waveSizeMultiplier = 0.75f;
    [SerializeField]
    float waveSpacing = -0.25f;

    float airTimeCounter = 0f;*/

    protected virtual void Awake()
    {
        if (rigidbody == null)
        {
            rigidbody = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
            particles = GetComponentInChildren<ParticleSystem>();
            renderer = GetComponentInChildren<SpriteRenderer>();
            poolComponent = GetComponent<PoolableObject>();
        }
        renderer.enabled = true;
        collider.enabled = true;
        rigidbody.isKinematic = false;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        /*if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			WeaverLog.Log("Collided With Player!");
		}*/
        if (!rigidbody.isKinematic && ((1 << collision.gameObject.layer) & collisionMask.value) != 0)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (renderer.enabled)
        {
            OnExplode();
        }
    }

    protected virtual void OnExplode()
    {
        renderer.enabled = false;
        rigidbody.isKinematic = true;
        collider.enabled = false;
        particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        //Debug.Log("Bomb Position = " + transform.position);
        //Debug.Log("Actual Air Time = " + airTimeCounter);
        InfectedExplosion.Spawn(transform.position, explosionScale);
        poolComponent.ReturnToPool(1f);

        /*if (SourceBoss != null && SourceBoss.InfectionWave != null && transform.position.y < SourceBoss.FloorY + 1f)
        {
            SlamWave leftWave, rightWave;
            SourceBoss.WaveSlams.SpawnSlam(SourceBoss.InfectionWave.System, transform.position.x, out leftWave, out rightWave, waveSpacing);
            leftWave.SizeToSpeedRatio *= waveSizeMultiplier;
            rightWave.SizeToSpeedRatio *= waveSizeMultiplier;
        }*/
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Hero Box"))
        {
            //WeaverLog.Log("Triggered With Player!");
            Explode();
        }
    }



    /*protected virtual void Update()
    {
        airTimeCounter += Time.deltaTime;
    }*/


    public static Bomb Spawn(Bomb prefab, Vector3 position, Vector2 velocity, float angularVelocity)
    {
        /*if (ScuttlerBombPool == null)
		{
			ScuttlerBombPool = new WeaverCore.ObjectPool(CorruptedKinGlobals.Instance.ScuttlerBombPrefab);
		}*/

        //var instance = ScuttlerBombPool.Instantiate<ScuttlerBomb>(position, Quaternion.identity);
        var instance = Pooling.Instantiate(prefab, position, Quaternion.identity);
        instance.rigidbody.velocity = velocity;
        instance.rigidbody.angularDrag = 0f;
        instance.rigidbody.angularVelocity = angularVelocity;

        return instance;
    }

    public static Bomb Spawn(Bomb prefab, Vector3 position, Vector2 velocity)
    {
        return Spawn(prefab, position, velocity, prefab.angularVelocity);
    }

    public static Bomb Spawn(Bomb prefab, Vector3 position, Vector3 destination, float time, float angularVelocity)
    {
        var gravityScale = prefab.GetComponent<Rigidbody2D>().gravityScale;
        var velocity = MathUtilities.CalculateVelocityToReachPoint(position, destination, time, gravityScale);

        //Debug.Log("Start = " + position);
        //Debug.Log("End = " + destination);
        //Debug.Log("Velocity = " + velocity);
        //Debug.Log("Time = " + time);

        return Spawn(prefab, position, velocity, angularVelocity);

        /*do
		{
			var velocity = MathUtilties.CalculateVelocityToReachPoint(position, destination, time, gravityScale);

			var peak = MathUtilties.CalculateMaximumOfCurve(position.y, destination.y, time, gravityScale);

			if (time > peak.x)
			{
				time /= 2f;
				continue;
			}
			return Spawn(position, velocity, angularVelocity);
		} while (true);*/
    }
}
