using System;
using WeaverCore;
using WeaverCore.Components;

public class AncientAspidHealth : EntityHealth
{
    public event Action<HitInfo> OnHitEvent;

    public override bool Hit(HitInfo hit)
    {
        var actualHit = base.Hit(hit);

        if (actualHit)
        {
            OnHitEvent?.Invoke(hit);
        }
        return actualHit;
    }
}