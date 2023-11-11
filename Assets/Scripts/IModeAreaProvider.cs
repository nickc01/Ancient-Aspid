using UnityEngine;

public interface IModeAreaProvider
{
    Vector2 GetModeTarget(AncientAspid boss);
    bool IsTargetActive(AncientAspid boss);
}