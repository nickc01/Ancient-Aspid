using UnityEngine;

public struct SpriteRendererState
{
    Sprite sprite;
    bool flipX;
    bool flipY;

    public SpriteRendererState(SpriteRenderer renderer)
    {
        sprite = renderer.sprite;
        flipX = renderer.flipX;
        flipY = renderer.flipY;
    }

    public void Restore(SpriteRenderer renderer)
    {
        renderer.sprite = sprite;
        renderer.flipX = flipX;
        renderer.flipY = flipY;
    }
}

namespace UnityEngine
{
    public static class SpriteRendererState_Extensions
    {
        public static SpriteRendererState TakeSnapshot(this SpriteRenderer renderer)
        {
            return new SpriteRendererState(renderer);
        }

        public static void Restore(this SpriteRenderer renderer, SpriteRendererState state)
        {
            state.Restore(renderer);
        }
    }
}
