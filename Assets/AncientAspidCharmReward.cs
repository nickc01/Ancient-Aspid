using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class AncientAspidCharmReward : DroppedCustomCharmItem
{
    ItemInspectRegion itemRegion;

    [SerializeField]
    Vector2 spriteScale = Vector2.one;

    protected override void Awake()
    {
        base.Awake();

        itemRegion = (ItemInspectRegion)InspectionRegion;
        itemRegion.OnCollectEvent += ItemRegion_OnCollectEvent;
    }

    private System.Collections.IEnumerator ItemRegion_OnCollectEvent()
    {
        if (CharmUtilities.GiveCharmToPlayer(charm, false))
        {
            var display = UIGetItemMessage.Spawn(charm.CharmSprite, "You acquired", charm.Name, "You can equip the new charm in your inventory", "");
            display.IconRenderer.transform.SetLocalScaleXY(spriteScale);
            yield return new WaitUntil(() => display.IsDone);
        }
    }

    protected override void OnGiveItem() { }
}
