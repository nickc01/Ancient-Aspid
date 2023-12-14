using UnityEngine;

public interface IModeAreaProvider
{
    Vector2 GetModeTarget(AncientAspid boss);
    bool IsTargetActive(AncientAspid boss);

    Vector2 GetLockAreaOverride(Vector2 oldPos, out bool clampWithinArea);
}