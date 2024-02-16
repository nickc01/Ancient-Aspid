using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;

public class PlayerTracker : MonoBehaviour
{
    public const float PointInterval = 0.25f;
    public const float MaxPoints = 100;

    public static PlayerTracker Instance { get; private set; }

    [NonSerialized]
    public LinkedList<Vector2> Points = new LinkedList<Vector2>();

    [OnRuntimeInit]
    static void OnInit()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private static void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (Player.Player1Raw != null && Player.Player1Raw.TryGetComponent<PlayerTracker>(out var tracker))
        {
            tracker.Points.Clear();
        }
    }

    [OnPlayerInit]
    static void OnPlayerInit(Player player)
    {
        if (!player.TryGetComponent<PlayerTracker>(out var tracker))
        {
            tracker = player.gameObject.AddComponent<PlayerTracker>();
            Instance = tracker;
        }

        tracker.Points.Clear();
    }

    float timer = 0f;

    private void LateUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= PointInterval)
        {
            timer = 0f;
            var point = transform.position;

            if (Points.Count == 0 || Vector2.Distance(point, Points.Last.Value) >= 0.02f)
            {
                Points.AddLast(point);
            }

            if (Points.Count > MaxPoints)
            {
                Points.RemoveFirst();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (var point in Points)
        {
            Gizmos.DrawLine(point + Vector2.down, point + Vector2.up);
            Gizmos.DrawLine(point + Vector2.left, point + Vector2.right);
        }
    }
}
