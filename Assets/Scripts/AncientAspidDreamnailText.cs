using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Dreamnail;

public class AncientAspidDreamnailText : DreamnailableEnemyTextSimple
{
    bool didHit = false;

    protected override int OnDreamnailHit(Player player)
    {
        if (!CanBeDreamnailed)
        {
            return 0;
        }

        if (!didHit && TryGetComponent<EntityHealth>(out var health))
        {
            didHit = true;
            health.Health -= 1;
        }

        return base.OnDreamnailHit(player);
    }
}
