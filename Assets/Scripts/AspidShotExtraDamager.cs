using System;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Components;

public class AspidShotExtraDamager : EnemyExtraDamager
{
    static Type SpriteFlashType;
    static MethodInfo flashInfectedMethod;

    EnemyProjectile projectile;

    [field: SerializeField]
    public int AdditionalDamage { get; set; } = 3;

    private void Awake()
    {
        projectile = GetComponent<EnemyProjectile>();
    }

    protected override void OnExtraDamage(IExtraDamageable hitEnemy)
    {
        if (hitEnemy is Component component)
        {
            if (component.TryGetComponent<EntityHealth>(out var entityHealth))
            {
                entityHealth.Health -= AdditionalDamage;
            }
            else
            {
                if (Initialization.Environment == WeaverCore.Enums.RunningState.Game)
                {
                    var healthManager = component.GetComponent("HealthManager");
                    if (healthManager != null)
                    {
                        healthManager.SendMessage("ApplyExtraDamage", AdditionalDamage);
                    }
                }
            }

            var flashers = component.GetComponentsInChildren<SpriteFlasher>();

            foreach (var flasher in flashers)
            {
                flasher.flashInfected();
            }

            if (Initialization.Environment == WeaverCore.Enums.RunningState.Game)
            {
                if (SpriteFlashType == null)
                {
                    SpriteFlashType = typeof(NonBouncer).Assembly.GetType("SpriteFlash");

                    flashInfectedMethod = SpriteFlashType.GetMethod("flashInfected");
                }

                var otherFlashers = component.GetComponentsInChildren(SpriteFlashType);

                foreach (var flasher in otherFlashers)
                {
                    flashInfectedMethod.Invoke(flasher, null);
                }
            }

            projectile.ForceHit(component.gameObject);
        }

        base.OnExtraDamage(hitEnemy);
    }

    protected override void OnDamageBackup(Transform obj)
    {
        WeaverLog.Log("DAMAGE BACKUP = " + obj);
        if (obj.TryGetComponent<SpitterPetNew>(out var _) || obj == transform || obj.GetComponent<AspidShotExtraDamager>() != null)
        {
            return;
        }

        bool validHit = false;

        if (obj.TryGetComponent<EntityHealth>(out var entityHealth))
        {
            entityHealth.Health -= AdditionalDamage;
            validHit = true;
        }
        else
        {
            if (Initialization.Environment == WeaverCore.Enums.RunningState.Game)
            {
                var healthManager = obj.GetComponent("HealthManager");
                if (healthManager != null)
                {
                    healthManager.SendMessage("ApplyExtraDamage", AdditionalDamage);
                    validHit = true;
                }
            }
        }

        var flashers = obj.GetComponentsInChildren<SpriteFlasher>();

        foreach (var flasher in flashers)
        {
            flasher.flashInfected();
        }

        if (Initialization.Environment == WeaverCore.Enums.RunningState.Game)
        {
            if (SpriteFlashType == null)
            {
                SpriteFlashType = typeof(NonBouncer).Assembly.GetType("SpriteFlash");

                flashInfectedMethod = SpriteFlashType.GetMethod("flashInfected");
            }

            var otherFlashers = obj.GetComponentsInChildren(SpriteFlashType);

            foreach (var flasher in otherFlashers)
            {
                flashInfectedMethod.Invoke(flasher, null);
            }
        }

        if (validHit)
        {
            projectile.ForceHit(obj.gameObject);
        }
    }
}
