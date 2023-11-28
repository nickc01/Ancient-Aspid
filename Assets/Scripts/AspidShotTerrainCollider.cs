using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Components;

public class AspidShotTerrainCollider : MonoBehaviour
{
    EnemyProjectile projectile;

    private void Awake()
    {
        projectile = GetComponentInParent<EnemyProjectile>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!projectile.Rigidbody.isKinematic)
        {
            projectile.ForceHit(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!projectile.Rigidbody.isKinematic)
        {
            projectile.ForceHit(collision.gameObject);
        }
    }

}
