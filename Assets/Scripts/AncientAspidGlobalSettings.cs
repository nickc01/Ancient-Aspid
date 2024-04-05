using System.Linq;
using UnityEngine;
using WeaverCore;
using WeaverCore.Settings;
using WeaverCore.Utilities;

public sealed class AncientAspidGlobalSettings : GlobalSettings
{
    public override string TabName => "Ancient Aspid";


    [HideInInspector]
    public bool forcedTitle = false;

    [SettingField(EnabledType.AlwaysVisible, "Open Color Manager")]
    public void OpenDebugTools()
    {
        SettingsScreen.Instance.Hide();
        GameObject.Instantiate(WeaverAssets.LoadAssetFromBundle<GameObject,AncientAspidMod>("Color Manager"), WeaverDebugCanvas.Content);
    }

    protected override void OnRegister()
    {
        base.OnRegister();
        if (!forcedTitle)
        {
            forcedTitle = true;
            SaveSettings();
            var tmpStyle = MenuStyles.Instance.styles.First(x => x.styleObject.name.Contains("ancient_aspid_menu_style"));
            MenuStyles.Instance.SetStyle(MenuStyles.Instance.styles.ToList().IndexOf(tmpStyle), false);
        }
    }
}
