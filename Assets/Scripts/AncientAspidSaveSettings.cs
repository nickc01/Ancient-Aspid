using WeaverCore;
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

    public int aspidKillCount;
    public bool aspidDiscovered;
    public bool aspidIsNewEntry;

    protected override void OnSaveLoaded(int saveFileNumber)
    {
        if (AspidFileResets.ResetAspidDefeatFlagOnNextSave)
        {
            WeaverLog.Log("Ancient Aspid Defeat Flag has been reset!");
            AspidFileResets.ResetAspidDefeatFlagOnNextSave = false;
            aspidBossDefeated = false;
        }

        if (AspidFileResets.ResetAllSettings)
        {
            WeaverLog.Log("All Ancient Aspid Save Settings have been reset!");
            AspidFileResets.ResetAllSettings = false;
            BottomAreaRevealed = false;
            charmAcquired = false;
            charmEquipped = false;
            charmNewlyCollected = false;
            visitedAmmoniteAery = false;
            aspidBossDefeated = false;
            AncientAspidBossCompletion = Completion.None;
            aspidKillCount = 0;
            aspidDiscovered = false;
            aspidIsNewEntry = false;
        }
    }
}
