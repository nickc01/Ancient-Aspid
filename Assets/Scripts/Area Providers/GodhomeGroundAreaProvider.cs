﻿using UnityEngine;
using WeaverCore;
using WeaverCore.Features;

public class GodhomeGroundAreaProvider : MonoBehaviour, IModeAreaProvider
{
    [SerializeField]
    private float healthThreshold = 0.45f;

    [SerializeField]
    AncientAspid boss;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Enemy Detector");
    }

    private void Start()
    {
        if (boss == null)
        {
            boss = GameObject.FindObjectOfType<AncientAspid>(true);
        }

        boss.GroundMode.GroundAreaProvider = this;
        //GameObject.FindObjectOfType<AncientAspid>(true).GroundMode.GroundAreaProvider = this;
    }

    public Vector2 GetModeTarget(AncientAspid boss)
    {
        return boss.Head.LookingDirection >= 0f
            ? (Vector2)(Player.Player1.transform.position + boss.GroundMode.lungeTargetOffset)
            : (Vector2)(Player.Player1.transform.position + new Vector3(-boss.GroundMode.lungeTargetOffset.x, boss.GroundMode.lungeTargetOffset.y, boss.GroundMode.lungeTargetOffset.z));
    }

    public bool IsTargetActive(AncientAspid boss)
    {
        return Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= 30f && (boss.HealthManager.Health / (float)boss.StartingHealth) <= healthThreshold;
    }

    public Vector2 GetLockAreaOverride(Vector2 oldPos, out bool clampWithinArea)
    {
        clampWithinArea = true;
        return oldPos;
    }
}
