using System;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Components;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class AspidShotExtraDamager : EnemyExtraDamager, IOnPool
{
    static Type SpriteFlashType;
    static MethodInfo flashInfectedMethod;

    EnemyProjectile projectile;

    [field: SerializeField]
    public int AdditionalDamage { get; set; } = 3;

    [SerializeField]
    bool disableWhenPlayerLocked = false;

    int oldPlayerDmg = -1;
    PlayerDamager playerDamager;

    bool didDamage = false;

    private void Awake()
    {
        didDamage = false;
        playerDamager = GetComponent<PlayerDamager>();
        if (playerDamager != null)
        {
            oldPlayerDmg = playerDamager.damageDealt;
        }
        LateUpdate();

        projectile = GetComponent<EnemyProjectile>();
    }

    private void LateUpdate()
    {
        if (disableWhenPlayerLocked && playerDamager != null)
        {
            playerDamager.damageDealt = HeroController.instance.controlReqlinquished ? 0 : oldPlayerDmg;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collider)
    {
        if (!didDamage)
        {
            base.OnTriggerEnter2D(collider);
        }
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (!didDamage)
        {
            base.OnCollisionEnter2D(collision);
        }
    }

    protected override void OnExtraDamage(IExtraDamageable hitEnemy)
    {
        if (hitEnemy is Component component)
        {
            bool validHit = false;

            var healthComponent = HealthUtilities.GetHealthComponentInParent(component.gameObject);

            HealthUtilities.TryGetHealth(healthComponent, out var oldHealth);
            int dmg;
            switch (HealthUtilities.GetHealthComponentType(healthComponent))
            {
                case HealthUtilities.HealthComponentType.EntityHealth:
                    dmg = AdditionalDamage;
                    if (dmg > oldHealth)
                    {
                        dmg = oldHealth;
                    }
                    if (dmg > 0 && !didDamage)
                    {
                        didDamage = true;
                        //WeaverLog.Log("DAMAGE A = " + dmg);
                        (healthComponent as EntityHealth).Health -= dmg - 1;
                        validHit = true;
                    }
                    break;
                case HealthUtilities.HealthComponentType.HealthManager:
                    dmg = AdditionalDamage;

                    if (dmg > oldHealth)
                    {
                        dmg = oldHealth;
                    }
                    if (dmg > 0 && !didDamage)
                    {
                        didDamage = true;
                        healthComponent.SendMessage("ApplyExtraDamage", dmg);
                        validHit = true;
                    }
                    break;
                default:
                    return;
            }
            /*if (component.TryGetComponent<EntityHealth>(out var entityHealth))
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
            }*/

            if (validHit)
            {
                var flashers = healthComponent.GetComponentsInChildren<SpriteFlasher>();

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

                    var otherFlashers = healthComponent.GetComponentsInChildren(SpriteFlashType);

                    foreach (var flasher in otherFlashers)
                    {
                        flashInfectedMethod.Invoke(flasher, null);
                    }
                }

                projectile.ForceHit(component.gameObject);
            }
        }

        base.OnExtraDamage(hitEnemy);
    }

    protected override void OnDamageBackup(Transform obj)
    {
        if (obj.TryGetComponent<SpitterPetNew>(out var _) || obj == transform || obj.GetComponent<AspidShotExtraDamager>() != null)
        {
            return;
        }

        bool validHit = false;

        var healthComponent = HealthUtilities.GetHealthComponentInParent(obj.gameObject);
        var healthType = HealthUtilities.GetHealthComponentType(healthComponent);

        HealthUtilities.TryGetHealth(healthComponent, out var oldHealth);

        //if (obj.TryGetComponent<EntityHealth>(out var entityHealth))
        if (healthType == HealthUtilities.HealthComponentType.EntityHealth)
        {
            int dmg = AdditionalDamage;
            if (dmg > oldHealth)
            {
                dmg = oldHealth;
            }
            if (dmg > 0 && !didDamage)
            {
                didDamage = true;
                //WeaverLog.Log("DAMAGE B = " + dmg);
                (healthComponent as EntityHealth).Health -= dmg;
                validHit = true;
            }
        }
        else if (healthType == HealthUtilities.HealthComponentType.HealthManager)
        {
            int dmg = AdditionalDamage;

            if (dmg > oldHealth)
            {
                dmg = oldHealth;
            }
            if (dmg > 0 && !didDamage)
            {
                didDamage = true;
                healthComponent.SendMessage("ApplyExtraDamage", dmg);
                validHit = true;
            }
        }

        if (validHit)
        {
            var flashers = healthComponent.GetComponentsInChildren<SpriteFlasher>();

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

                var otherFlashers = healthComponent.GetComponentsInChildren(SpriteFlashType);

                foreach (var flasher in otherFlashers)
                {
                    flashInfectedMethod.Invoke(flasher, null);
                }
            }

            projectile.ForceHit(obj.gameObject);
        }
    }

    public void OnPool()
    {
        if (disableWhenPlayerLocked && playerDamager != null)
        {
            playerDamager.damageDealt = oldPlayerDmg;
        }
    }
}
