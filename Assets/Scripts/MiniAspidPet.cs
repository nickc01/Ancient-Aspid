using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Attributes;

public class MiniAspidPet : SpitterPet
{
    public static bool MiniAspidPetEnabled { get; private set; } = false;

    static GameObject OriginalPrefab;

    [OnRuntimeInit]
    static void OnRuntimeInit()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private static void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
        if (Player.Player1Raw != null)
        {
            UnboundCoroutine.Start(OnSceneLoad(Player.Player1Raw));
        }
    }

    static IEnumerator OnSceneLoad(Player player)
    {
        yield return new WaitForSeconds(0.1f);

        var hatchlings = GameObject.FindGameObjectsWithTag("Knight Hatchling");

        var max = GetHatchlingMax(player);

        if (hatchlings.Length > max)
        {
            for (int i = 0; i < hatchlings.Length - max; i++)
            {
                GameObject.Destroy(hatchlings[i]);
            }
        }
    }

    public static int GetHatchlingMax(Player player)
    {
        if (player == null)
        {
            player = Player.Player1Raw;
        }

        if (player != null && player.transform != null)
        {
            var charmEffects = player.transform.Find("Charm Effects");

            if (charmEffects != null)
            {
                return PlayMakerUtilities.GetFsmInt(charmEffects.gameObject, "Hatchling Spawn", "Hatchling Max");
            }
        }

        return 4;
    }

    public static void ReplaceHatchlingPrefab(Player player)
    {
        var charmEffects = player.transform.Find("Charm Effects");
        if (charmEffects == null)
        {
            return;
        }


        var hatchlingFSM = PlayMakerUtilities.GetPlaymakerFSMOnObject(charmEffects.gameObject, "Hatchling Spawn");

        var fsm = PlayMakerUtilities.GetFSMOnPlayMakerComponent(hatchlingFSM);

        GameObject hatchlingAspid = WeaverAssets.LoadAssetFromBundle<GameObject, AncientAspidMod>("Knight Hatchling Aspid");

        foreach (var state in PlayMakerUtilities.GetStatesOnFSM(fsm))
        {
            var actionData = PlayMakerUtilities.GetActionData(state);

            if (actionData != null)
            {
                var gameObjects = (IEnumerable)actionData.ReflectGetField("fsmGameObjectParams");

                if (gameObjects != null)
                {
                    foreach (var fsmGM in gameObjects)
                    {
                        var gm = (GameObject)fsmGM.ReflectGetProperty("Value");

                        if (gm != null && gm.name == "Knight Hatchling")
                        {
                            OriginalPrefab = gm;
                            fsmGM.ReflectSetProperty("Value", hatchlingAspid);
                            MiniAspidPetEnabled = true;
                        }
                    }
                }
            }
        }
    }

    public static void RevertHatchlingPrefab(Player player)
    {
        var charmEffects = player.transform.Find("Charm Effects");
        if (charmEffects == null)
        {
            return;
        }

        var hatchlingFSM = PlayMakerUtilities.GetPlaymakerFSMOnObject(charmEffects.gameObject, "Hatchling Spawn");

        var fsm = PlayMakerUtilities.GetFSMOnPlayMakerComponent(hatchlingFSM);

        foreach (var state in PlayMakerUtilities.GetStatesOnFSM(fsm))
        {
            var actionData = PlayMakerUtilities.GetActionData(state);

            if (actionData != null)
            {
                var gameObjects = (IEnumerable)actionData.ReflectGetField("fsmGameObjectParams");

                if (gameObjects != null)
                {
                    foreach (var fsmGM in gameObjects)
                    {
                        var gm = (GameObject)fsmGM.ReflectGetProperty("Value");

                        if (gm != null && gm.name == "Knight Hatchling Aspid")
                        {
                            fsmGM.ReflectSetProperty("Value", OriginalPrefab);
                            MiniAspidPetEnabled = false;
                        }
                    }
                }
            }
        }
    }
}
