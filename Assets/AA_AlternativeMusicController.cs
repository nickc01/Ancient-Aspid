using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using static AncientAspidMusicController;

public class AA_AlternativeMusicController : MonoBehaviour
{
    [SerializeField]
    List<AncientAspidMusicController.MusicPhase> phases;

    [SerializeField]
    List<Music.SnapshotType> phaseSnapshots;

    [SerializeField]
    MusicCue alternativeMusicCue;

    [SerializeField]
    AudioClip introClip;

    [SerializeField]
    float phaseTransitionTime = 2f;

    [SerializeField]
    float introDuration = 4.875f;

    [NonSerialized]
    bool playing = false;

    public void Play(MusicPhase startPhase)
    {
        if (playing)
        {
            return;
        }
        transform.SetParent(null);
        playing = true;
        Music.ApplyMusicSnapshot(Music.SnapshotType.Normal, 0f, 0f);
        WeaverAudio.PlayAtPoint(introClip, Player.Player1.transform.position, channel: WeaverCore.Enums.AudioChannel.Music);
        StartCoroutine(PlayRoutine(startPhase));
    }

    IEnumerator PlayRoutine(MusicPhase startPhase)
    {
        yield return new WaitForSecondsRealtime(introDuration);
        Music.PlayMusicCue(alternativeMusicCue, 0f, 0f, false);
        var index = phases.IndexOf(startPhase);
        Music.ApplyMusicSnapshot(phaseSnapshots[index], 0f, 0);
    }

    public void TransitionToPhase(MusicPhase phase)
    {
        var index = phases.IndexOf(phase);
        Music.ApplyMusicSnapshot(phaseSnapshots[index], 0, phaseTransitionTime);
    }

    public void Stop(float duration = 0.5f)
    {
        playing = false;
        Music.ApplyMusicSnapshot(Music.SnapshotType.Silent, 0f, duration);
    }
}
