using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Components;

public class SlashColliderControl : MonoBehaviour
{
    [SerializeField]
    List<PolygonCollider2D> colliders;


    WeaverAnimationPlayer animator;

    int currentFrame;

    private void OnEnable()
    {
        if (animator == null)
        {
            animator = GetComponent<WeaverAnimationPlayer>();
        }
        currentFrame = -1;
    }

    private void LateUpdate()
    {
        if (animator.PlayingFrame != currentFrame)
        {
            currentFrame = animator.PlayingFrame;
            for (int i = colliders.Count - 1; i >= 0; i--)
            {
                colliders[i].enabled = i == currentFrame;
            }
        }
    }

    private void OnDisable()
    {
        for (int i = colliders.Count - 1; i >= 0; i--)
        {
            colliders[i].enabled = false;
        }
    }
}
