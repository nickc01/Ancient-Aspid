using System.Linq;
using UnityEngine;
using WeaverCore;
using WeaverCore.Settings;
using WeaverCore.Utilities;

public sealed class AncientAspidGlobalSettings : GlobalSettings
{
    public override string TabName => "Ancient Aspid";

    [HideInInspector]
    [SettingField(EnabledType.Hidden)]
    public bool forcedTitle = false;

    //[SettingField(EnabledType.AlwaysVisible, "Open Color Manager")]
    public void OpenDebugTools()
    {
        SettingsScreen.Instance.Hide();
        GameObject.Instantiate(WeaverAssets.LoadAssetFromBundle<GameObject,AncientAspidMod>("Color Manager"), WeaverDebugCanvas.Content);
    }

    [SettingField(EnabledType.AlwaysVisible, "Reset Aspid Defeat Flag")]
    [SettingDescription("Will reset the aspid defeat flag for the next save file you load")]
    public void ResetAspidDefeatFlag()
    {
        AspidFileResets.ResetAspidDefeatFlagOnNextSave = true;
    }

    [SettingField(EnabledType.AlwaysVisible, "Reset All Save Settings")]
    [SettingDescription("All save settings related to Ancient Aspid will be reset for the next save file you load")]
    public void ResetAllSaveSettings()
    {
        AspidFileResets.ResetAllSettings = true;
    }

    protected override void OnRegister()
    {
        base.OnRegister();
#if !UNITY_EDITOR
        if (!forcedTitle)
        {
            forcedTitle = true;
            SaveSettings();
            var tmpStyle = MenuStyles.Instance.styles.First(x => x.styleObject.name.Contains("ancient_aspid_menu_style"));
            MenuStyles.Instance.SetStyle(MenuStyles.Instance.styles.ToList().IndexOf(tmpStyle), false);
        }
#endif
    }
}
