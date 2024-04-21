using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        StartCoroutine(PreloaderRoutine());
    }

    //[NonSerialized]
    //List<AudioPlayer> preloads = new List<AudioPlayer>();

    IEnumerator PreloaderRoutine()
    {
        List<MusicChannels> channels = new List<MusicChannels>
        {
            MusicChannels.Main,
            MusicChannels.MainAlt,
            MusicChannels.Action,
            MusicChannels.Sub,
            MusicChannels.Tension,
            MusicChannels.Extra
        };

        float duration = introDuration * 0.75f;

        foreach (var channel in channels.Select(alternativeMusicCue.GetChannelInfo))
        {
            if (channel != null && channel.Clip != null)
            {
                //preloads.Add(WeaverAudio.PlayAtPoint(channel.Clip, Player.Player1.transform.position, 0.002f, WeaverCore.Enums.AudioChannel.Music));
                channel.Clip.LoadAudioData();
                yield return new WaitForSecondsRealtime(duration / channels.Count);
            }
        }
    }

    IEnumerator PlayRoutine(MusicPhase startPhase)
    {
        yield return new WaitForSecondsRealtime(introDuration);
        Music.PlayMusicCue(alternativeMusicCue, 0f, 0f, false);
        var index = phases.IndexOf(startPhase);
        Music.ApplyMusicSnapshot(phaseSnapshots[index], 0f, 0);

        /*foreach (var preload in preloads)
        {
            preload.Delete();
        }*/
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
