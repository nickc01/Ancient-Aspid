using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore;

public class AncientAspidMusicController : MonoBehaviour
{
    public enum MusicPhase
    {
        AR1,
        UPWARDING,
        AR2,
        AR3
    }

    public MusicPhase CurrentlyPlayingPhase { get; private set; }
    //MusicPhase? previousPhase = null;

    [SerializeField]
    StudioEventEmitter emitter;

    [SerializeField]
    float phaseTransitionTime = 1f;

    float phaseValue = 0f;

    FMOD.Studio.Bus bus;

    Transform playerTransform;

    public bool Playing { get; private set; } = false;

    bool hooked = false;

    private void Start()
    {
        if (!hooked)
        {
            hooked = true;
            bus = FMODUnity.RuntimeManager.GetBus("bus:/");
            FModMusicManager.OnLevelsUpdated += FModMusicManager_OnLevelsUpdated;
            playerTransform = Player.Player1.transform;
        }
    }

    private void LateUpdate()
    {
        transform.position = playerTransform.position;
    }

    private void FModMusicManager_OnLevelsUpdated(Music.SnapshotVolumeLevels levels)
    {
        bus.setVolume(levels.Main);
    }

    private void OnDestroy()
    {
        if (hooked)
        {
            hooked = false;
            FModMusicManager.OnLevelsUpdated -= FModMusicManager_OnLevelsUpdated;
        }

        //FModMusicManager.OnLevelsUpdated -= FModMusicManager_OnLevelsUpdated;
        /*foreach (var emitter in musicEmitters)
        {
            if (emitter.IsPlaying())
            {
                emitter.Stop();
            }
        }*/
        emitter.Stop();
    }

    public void Play(MusicPhase startPhase)
    {
        if (Playing)
        {
            return;
        }

        if (!hooked)
        {
            hooked = true;
            bus = FMODUnity.RuntimeManager.GetBus("bus:/");
            FModMusicManager.OnLevelsUpdated += FModMusicManager_OnLevelsUpdated;
            playerTransform = Player.Player1.transform;
        }

        Playing = true;

        CurrentlyPlayingPhase = startPhase;

        //var enabledEmitter = musicEmitters[emitterPhases.IndexOf(startPhase)];

        /*for (int i = 0; i < musicEmitters.Count; i++)
        {
            var emitter = musicEmitters[i];
            emitter.Play();
            emitter.EventInstance.setTimelinePosition(0);
            emitter.EventInstance.setVolume(emitterPhases[i] == startPhase ? 1f : 0f);
        }*/
        emitter.Play();

        bus.setVolume(FModMusicManager.Levels.Main);
    }

    public void TransitionToPhase(MusicPhase phase)
    {
        CurrentlyPlayingPhase = phase;
        StopAllCoroutines();
        /*if (previousPhase != null)
        {
            var previousEmitter = musicEmitters[emitterPhases.IndexOf(previousPhase.Value)];
            previousEmitter.EventInstance.setVolume(0f);
            previousPhase = null;
        }*/
        StartCoroutine(TransitionRoutine(phaseValue, (int)phase * 25f, phaseTransitionTime));
    }

    IEnumerator TransitionRoutine(float from, float to, float time)
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            phaseValue = Mathf.Lerp(from, to, t / time);
            emitter.SetParameter("PHASE", phaseValue);
            yield return null;
        }

        phaseValue = to;
        emitter.SetParameter("PHASE", phaseValue);

        /*float[] oldVolumes = new float[emitterPhases.Count];

        for (int i = 0; i < musicEmitters.Count; i++)
        {
            musicEmitters[i].EventInstance.getVolume(out oldVolumes[i]);
        }

        var newEmitter = musicEmitters[emitterPhases.IndexOf(to)];

        for (float t = 0; t < time; t += Time.deltaTime)
        {
            for (int i = 0; i < musicEmitters.Count; i++)
            {
                musicEmitters[i].EventInstance.setVolume(Mathf.Lerp(oldVolumes[i], musicEmitters[i] == newEmitter ? 1f : 0f, t / time));
            }
            yield return null;
        }

        for (int i = 0; i < musicEmitters.Count; i++)
        {
            musicEmitters[i].EventInstance.setVolume(musicEmitters[i] == newEmitter ? 1f : 0f);
        }*/
        /*var oldEmitter = musicEmitters[emitterPhases.IndexOf(from)];
        var newEmitter = musicEmitters[emitterPhases.IndexOf(to)];

        oldEmitter.EventInstance.getVolume(out var oldEmitterVolume);
        newEmitter.EventInstance.getVolume(out var newEmitterVolume);
        //previousPhase = from;

        for (float t = 0; t < time; t += Time.deltaTime)
        {
            oldEmitter.EventInstance.setVolume(Mathf.Lerp(oldEmitterVolume, 0f, t / time));
            newEmitter.EventInstance.setVolume(Mathf.Lerp(newEmitterVolume, 1f, t / time));
            yield return null;
        }*/

        //oldEmitter.EventInstance.setVolume(0f);
        //newEmitter.EventInstance.setVolume(1f);

        //previousPhase = null;
    }

    public void Stop(float duration = 0.5f)
    {
        if (!Playing)
        {
            return;
        }

        Playing = false;

        StopAllCoroutines();
        //previousPhase = null;
        StartCoroutine(StopRoutine(duration));
    }

    IEnumerator StopRoutine(float duration)
    {
        yield return TransitionRoutine(phaseValue, 0f, duration);
        /*float[] oldVolumes = new float[emitterPhases.Count];

        for (int i = 0; i < musicEmitters.Count; i++)
        {
            musicEmitters[i].EventInstance.getVolume(out oldVolumes[i]);
        }

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            for (int i = 0; i < musicEmitters.Count; i++)
            {
                musicEmitters[i].EventInstance.setVolume(Mathf.Lerp(oldVolumes[i], 0f, t / duration));
            }
            //currentEmitter.EventInstance.setVolume(Mathf.Lerp(newEmitterVolume, 0f, t / duration));
            yield return null;
        }*/

        yield return new WaitForSeconds(0.5f);
        /*var currentEmitter = musicEmitters[emitterPhases.IndexOf(CurrentlyPlayingPhase)];

        */

        emitter.Stop();
    }
}
