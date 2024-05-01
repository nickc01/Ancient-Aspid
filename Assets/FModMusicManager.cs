using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

public static class FModMusicManager
{
    /// <summary>
    /// The current audio levels of each mixer. This DOES NOT take into account the volume levels set in the options menu
    /// </summary>
    public static Music.SnapshotVolumeLevels UnfilteredLevels { get; private set; } = new Music.SnapshotVolumeLevels(1, 1, 1, 1, 1, 1, 1);

    /// <summary>
    /// The current audio levels of each mixer. This DOES take into account the volume levels set in the options menu
    /// </summary>
    public static Music.SnapshotVolumeLevels Levels { get; private set; } = new Music.SnapshotVolumeLevels(1, 1, 1, 1, 1, 1, 1);

    public static event Action<Music.SnapshotVolumeLevels> OnLevelsUpdated;

    static UnboundCoroutine levelInterpolatorRoutine;
    static UnboundCoroutine pauseInterpolatorRoutine;

    static float currentPauseState = 1f;

    [OnHarmonyPatch]
    static void OnHarmonyPatch(HarmonyPatcher patcher)
    {
        {
            var orig = typeof(AudioMixer).GetMethod("TransitionToSnapshot", BindingFlags.NonPublic | BindingFlags.Instance);

            var postfix = typeof(FModMusicManager).GetMethod(nameof(TransitionToSnapshotPostfix), BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(orig, null, postfix);
        }

        WeaverAudio.OnMasterVolumeUpdate += WeaverAudio_OnMasterVolumeUpdate;
        WeaverAudio.OnMusicVolumeUpdate += WeaverAudio_OnMusicVolumeUpdate;
        WeaverAudio.OnPauseStateUpdate += WeaverAudio_OnPauseStateUpdate;
    }

    private static void WeaverAudio_OnPauseStateUpdate(bool isPaused)
    {
        TransitionPauseState(isPaused);
    }

    private static void WeaverAudio_OnMusicVolumeUpdate(float musicLevel)
    {
        Levels = UnfilteredLevels * (WeaverAudio.MasterVolume * musicLevel * currentPauseState);
        OnLevelsUpdated?.Invoke(Levels);
    }

    private static void WeaverAudio_OnMasterVolumeUpdate(float masterLevel)
    {
        Levels = UnfilteredLevels * (WeaverAudio.MusicVolume * masterLevel * currentPauseState);
        OnLevelsUpdated?.Invoke(Levels);
    }

    static void TransitionToSnapshotPostfix(AudioMixer __instance, AudioMixerSnapshot snapshot, float timeToReach)
    {
        if (Music.GetLevelsForSnapshot(snapshot, out var newLevels))
        {
            if (levelInterpolatorRoutine != null)
            {
                UnboundCoroutine.Stop(levelInterpolatorRoutine);
                levelInterpolatorRoutine = null;
            }

            levelInterpolatorRoutine = UnboundCoroutine.Start(LevelInterpolator(UnfilteredLevels, newLevels, timeToReach));
        }
    }

    static void UpdateFilteredLevels()
    {
        Levels = UnfilteredLevels * (WeaverAudio.MasterVolume * WeaverAudio.MusicVolume * currentPauseState);
        OnLevelsUpdated?.Invoke(Levels);
    }

    static IEnumerator LevelInterpolator(Music.SnapshotVolumeLevels from, Music.SnapshotVolumeLevels to, float duration)
    {
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            UnfilteredLevels = Music.SnapshotVolumeLevels.Lerp(from, to, t / duration);
            UpdateFilteredLevels();
            yield return null;
        }

        UnfilteredLevels = to;
        UpdateFilteredLevels();
    }

    static void TransitionPauseState(bool newPauseState)
    {
        float newPauseValue = newPauseState ? 0.5f : 1f;

        if (pauseInterpolatorRoutine != null)
        {
            UnboundCoroutine.Stop(pauseInterpolatorRoutine);
            pauseInterpolatorRoutine = null;
        }

        pauseInterpolatorRoutine = UnboundCoroutine.Start(PauseInterpolator(currentPauseState, newPauseValue, 0.5f));
    }

    static IEnumerator PauseInterpolator(float from, float to, float duration)
    {
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            currentPauseState = Mathf.Lerp(from, to, t / duration);
            UpdateFilteredLevels();
            yield return null;
        }

        currentPauseState = to;
        UpdateFilteredLevels();
    }
}
