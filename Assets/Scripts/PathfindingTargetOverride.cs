using System;
using UnityEngine;

public interface IPathfindingOverride
{
    Vector3 GetTarget();
    bool IsTargetEnabled();
    int GetTargetPriority();
    string name { get; }
}

[ExecuteAlways]
public class PathfindingTargetOverride : MonoBehaviour, IPathfindingOverride
{
    [SerializeField]
    int priority = 0;

    [SerializeField]
    bool targetEnabled = true;

    [NonSerialized]
    bool initialized = false;

    [SerializeField]
    bool addOnTrigger = true;

    [NonSerialized]
    Transform target;

    bool added = false;

    public Vector3 GetTarget()
    {
        return target.position;
    }

    public int GetTargetPriority()
    {
        return priority;
    }

    public bool IsTargetEnabled()
    {
        return targetEnabled;
    }

    private void Awake()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;

        gameObject.layer = LayerMask.NameToLayer("Enemy Detector");

        if (transform.childCount == 0)
        {
            target = new GameObject("Target").transform;
            target.SetParent(transform);
            target.localPosition = Vector3.zero;
        }
        else
        {
            target = transform.GetChild(0);
        }
    }

    private void Start()
    {
        Awake();
    }

    private void Update()
    {
        Awake();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (addOnTrigger)
        {
            var boss = collision.gameObject.GetComponentInParent<AncientAspid>();
            if (boss != null)
            {
                boss.AddPathfindingOverride(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (addOnTrigger)
        {
            var boss = collision.gameObject.GetComponentInParent<AncientAspid>();
            if (boss != null)
            {
                boss.RemovePathfindingOverride(this);
            }
        }
    }
}