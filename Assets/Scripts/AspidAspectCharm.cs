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
    SpitterPetNew petPrefab;

    public static event Action<AspidAspectCharm, bool> EquippedEvent;

    public override string Name => "Aspid Aspect";

    public override string Description => @"Charm bears the likeness of an Primal Aspid with a intense stare. 
""Look Into My Eyes ,We're Not So Different,You and I.""

Bestows upon the bearer an arbitrary ally.";

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
    }

    [OnFeatureUnload]
    static void OnCharmUnload(AspidAspectCharm charm)
    {
        loadedCharm = null;
        CharmRefresh();
    }

    [OnPlayerInit]
    static void PlayerLoad(Player player)
    {
        loadedPlayer = player;
        CharmRefresh();
    }

    [OnPlayerUninit]
    static void PlayerUnload(Player player)
    {
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
        var charmsAllowed = true;

        if (Player.Player1Raw != null)
        {
            charmsAllowed = Initialization.Environment != RunningState.Game || (Initialization.Environment == RunningState.Game && !PlayMakerUtilities.GetFsmBool(Player.Player1.gameObject, "ProxyFSM", "No Charms"));
        }

        var loaded = (loadedCharm != null) && (loadedPlayer != null) && charmsAllowed;

        if (loaded != fullyLoaded)
        {
            fullyLoaded = loaded;
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

    }

    static SpitterPetNew petInstance;

    static void OnCharmEquipped(AspidAspectCharm charm, bool equipped)
    {
        if (equipped && petInstance == null)
        {
            petInstance = GameObject.Instantiate(charm.petPrefab, Player.Player1.transform.position + new Vector3(0f, 4f, 0f), Quaternion.identity);
        }
        else if (!equipped && petInstance != null)
        {
            var petInst = petInstance;
            petInstance.FadeOut(() =>
            {
                GameObject.Destroy(petInst.gameObject);
            });
            petInstance = null;
        }

        if (!equipped)
        {
            foreach (var aspid in GameObject.FindObjectsOfType<MiniAspidPet>())
            {
                aspid.GetComponent<SpitterPetNew>().FadeOut(() =>
                {
                    GameObject.Destroy(aspid.gameObject);
                });
            }
        }

        if (Player.Player1Raw != null)
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

