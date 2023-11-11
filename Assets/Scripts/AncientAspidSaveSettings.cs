using WeaverCore.Settings;
using static BossStatue;

public class AncientAspidSaveSettings : SaveSpecificSettings
{
    public bool BottomAreaRevealed = false;

    public bool charmAcquired = false;
    public bool charmEquipped = false;
    public bool charmNewlyCollected = false;
    public bool visitedAmmoniteAery = false;

    public bool aspidBossDefeated = false;

    public Completion AncientAspidBossCompletion = Completion.None;
}
