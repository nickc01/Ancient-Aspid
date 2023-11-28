using UnityEngine;
using WeaverCore;
using WeaverCore.Settings;
using WeaverCore.Utilities;

public sealed class AncientAspidGlobalSettings : GlobalSettings
{
    public override string TabName => "Ancient Aspid";


    [SettingField(EnabledType.AlwaysVisible, "Open Color Manager")]
    public void OpenDebugTools()
    {
        SettingsScreen.Instance.Hide();
        GameObject.Instantiate(WeaverAssets.LoadAssetFromBundle<GameObject,AncientAspidMod>("Color Manager"), WeaverDebugCanvas.Content);
    }
}
