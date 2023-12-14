using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class Bomb : MonoBehaviour
{
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
        InfectedExplosion.Spawn(transform.position, explosionScale);
        poolComponent.ReturnToPool(1f);

    }

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Hero Box"))
        {
            Explode();
        }
    }

    public static Bomb Spawn(Bomb prefab, Vector3 position, Vector2 velocity, float angularVelocity)
    {
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

        return Spawn(prefab, position, velocity, angularVelocity);

    }
}
