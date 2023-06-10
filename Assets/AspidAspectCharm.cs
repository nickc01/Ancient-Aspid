using System;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class AspidAspectCharm : WeaverCharm
{
    static Player loadedPlayer = null;
    static AspidAspectCharm loadedCharm = null;
    static bool fullyLoaded = false;

    [SerializeField]
    SpitterPet petPrefab;

    public static event Action<AspidAspectCharm, bool> EquippedEvent;

    public override string Name => "Aspid Aspect";

    public override string Description => "Summons an aspid to fight for you";

    public override int NotchCost => 1;

    public override bool Equipped
    {
        get => base.Equipped;
        set
        {
            if (base.Equipped != value)
            {
                base.Equipped = value;
                EquippedEvent?.Invoke(this, value);
            }
        }
    }

    [OnFeatureLoad]
    static void OnCharmLoad(AspidAspectCharm charm)
    {
        loadedCharm = charm;
        CharmRefresh();
        WeaverLog.Log("CHARM LOAD = " + charm);
    }

    [OnFeatureUnload]
    static void OnCharmUnload(AspidAspectCharm charm)
    {
        loadedCharm = null;
        CharmRefresh();
        WeaverLog.Log("CHARM UNLOAD = " + charm);
    }

    [OnPlayerInit]
    static void PlayerLoad(Player player)
    {
        WeaverLog.Log("PLAYER LOADED = " + player);
        loadedPlayer = player;
        CharmRefresh();
    }

    [OnPlayerUninit]
    static void PlayerUnload(Player player)
    {
        WeaverLog.Log("PLAYER UNLOADED = " + player);
        loadedPlayer = null;
        CharmRefresh();
    }

    [OnRuntimeInit]
    static void OnInit()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private static void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
        CharmRefresh();
    }

    static void CharmRefresh()
    {
        var charmsAllowed = Initialization.Environment != RunningState.Game || (Initialization.Environment == RunningState.Game && !PlayMakerUtilities.GetFsmBool(Player.Player1.gameObject, "ProxyFSM", "No Charms"));

        var loaded = (loadedCharm != null) && (loadedPlayer != null) && charmsAllowed;

        if (loaded != fullyLoaded)
        {
            fullyLoaded = loaded;
            WeaverLog.Log("CHARM LOADED = " + fullyLoaded);
            if (loaded)
            {
                EquippedEvent += OnCharmEquipped;

                if (loadedCharm.Equipped)
                {
                    EquippedEvent?.Invoke(loadedCharm, true);
                }
            }
            else
            {
                if (loadedCharm.Equipped)
                {
                    EquippedEvent?.Invoke(loadedCharm, false);
                }
                EquippedEvent -= OnCharmEquipped;
            }
        }

        /*if (loadedCharm != null && loadedPlayer != null)
        {
            MiniAspidPet.ReplaceHatchlingPrefab(loadedPlayer);
        }
        else
        {
            if (loadedPlayer != null)
            {
                MiniAspidPet.RevertHatchlingPrefab(loadedPlayer);
            }
        }*/
    }

    /*[OnPlayerInit]
    static void PlayerInit(Player player)
    {
        WeaverLog.Log("PLAYER INIT");
        var charm = Registry.GetFeature<AspidAspectCharm>();

        EquippedEvent += (charm, equipped) =>
        {
            charm.OnCharmEquipped(charm, equipped);
        };

        if (charm.Equipped)
        {
            EquippedEvent?.Invoke(charm, true);
        }
    }

    [OnPlayerUninit]
    static void PlayerUninit(Player player)
    {
        WeaverLog.Log("PLAYER UNINIT");
        var charm = Registry.GetFeature<AspidAspectCharm>();

        if (charm.Equipped)
        {
            EquippedEvent?.Invoke(charm, false);
        }

        EquippedEvent -= charm.OnCharmEquipped;
    }*/

    static SpitterPet petInstance;

    static void OnCharmEquipped(AspidAspectCharm charm, bool equipped)
    {
        WeaverLog.Log("CHARM EQUIPPED = " + equipped);
        if (equipped && petInstance == null)
        {
            petInstance = GameObject.Instantiate(charm.petPrefab, Player.Player1.transform.position + new Vector3(0f, 4f, 0f), Quaternion.identity);
        }
        else if (!equipped && petInstance != null)
        {
            GameObject.Destroy(petInstance.gameObject);
            petInstance = null;
        }

        if (!equipped)
        {
            foreach (var aspid in GameObject.FindObjectsOfType<MiniAspidPet>())
            {
                GameObject.Destroy(aspid.gameObject);
            }
        }

        if (Player.Player1 != null)
        {
            if (equipped)
            {
                MiniAspidPet.ReplaceHatchlingPrefab(Player.Player1);

                var existingHatchlings = GameObject.FindGameObjectsWithTag("Knight Hatchling");

                foreach (var hatchling in existingHatchlings)
                {
                    var hatchlingComponent = hatchling.GetComponent("KnightHatchling");

                    if (hatchlingComponent != null)
                    {
                        hatchlingComponent.SendMessage("FsmCharmsEnd");
                    }
                }
            }
            else
            {
                MiniAspidPet.RevertHatchlingPrefab(Player.Player1);
            }
        }
    }
}

/*public class TestCharm : IWeaverCharm
{
    void TestPrint(string message)
    {
        WeaverLog.Log($"{GetType().Name} - {message}");
    }



    public string Name
    {
        get
        {
            TestPrint($"Getting Charm Cost = Test Charm");
            return "Test Charm";
        }
    }

    public string Description
    {
        get
        {
            TestPrint($"Getting Charm Cost = This is a test charm");
            return "This is a test charm";
        }
    }

    public int NotchCost
    {
        get
        {
            TestPrint($"Getting Charm Cost = {2}");
            return 2;
        }
    }

    bool _acquired = false;
    public bool Acquired
    {
        get
        {
            TestPrint($"Getting Acquired = {_acquired}");
            return _acquired;
        }
        set
        {
            TestPrint($"Setting Acquired = {value}");
            _acquired = value;
        }
    }


    bool _equipped = false;
    public bool Equipped
    {
        get
        {
            TestPrint($"Getting Equipped = {_equipped}");
            return _equipped;
        }
        set
        {
            TestPrint($"Setting Equipped = {value}");
            _equipped = value;
        }
    }

    bool _newlyCollected = false;
    public bool NewlyCollected
    {
        get
        {
            TestPrint($"Getting Newly Collected = {_newlyCollected}");
            return _newlyCollected;
        }
        set
        {
            TestPrint($"Setting Newly Collected = {value}");
            _newlyCollected = value;
        }
    }
}*/