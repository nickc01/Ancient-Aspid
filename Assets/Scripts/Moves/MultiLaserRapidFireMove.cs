using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore;
using System;
using System.Collections.Generic;

public class MultiLaserRapidFireMove : AncientAspidMove
{
    [SerializeField]
    float duration = 5f;

    [SerializeField]
    Vector2 angleOffsetRange = new Vector2(-30f, 30f);

    /*[SerializeField]
    float firstBetweenDelay = 0.75f;

    [SerializeField]
    float betweenDelay = 0.5f;*/
    [SerializeField]
    float extraFirstDelay = 0.5f;

    [SerializeField]
    Vector2 betweenDelayMinMax = new Vector2(0.5f, 0.35f);

    [SerializeField]
    float preFireDelay = 0.5f;

    [SerializeField]
    float fireDuration = 0.45f;

    [SerializeField]
    Vector2 firePitchRange = new Vector2(0.95f, 1.05f);

    /*[SerializeField]
    float initialDelay = 0.6f;

    [SerializeField]
    float preFireDelay = 0.3f;

    [SerializeField]
    float fireDuration = 0.25f;

    [SerializeField]
    float angleSpread = 15f;*/

    [SerializeField]
    AudioClip prepareLaserSound;

    [SerializeField]
    AudioClip fireLaserSound;

    //[SerializeField]
    //Vector2 angleOffsetRandomizationRange = new Vector2(-5f, 5f);

    //[SerializeField]
    //float centerLaserRandomness = 0f;

    //[SerializeField]
    //List<LaserEmitter> lasers;

    //[SerializeField]
    //Sprite fireSprite;

    public override bool MoveEnabled
    {
        get
        {
            var enabled = Boss.CanSeeTarget &&
        Boss.CurrentRunningMode == Boss.OffensiveMode &&
        Vector3.Distance(Player.Player1.transform.position, transform.position) <= 30f;

            return enabled;
        }
    }

    protected override IEnumerator OnExecute()
    {
        yield return Boss.Head.LockHead();

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic Quick");

        Boss.Head.ShotgunLasers.SetHeadSpriteToRotation(Quaternion.Euler(0f, 0f, -90f));

        float startTime = Time.time;

        List<uint> laserFireRoutines = new List<uint>();

        List<int> laserIndexes = new List<int>
        {
            0,
            1,
            2,
            3,
            4
        };

        Vector3[] laserTargets = new Vector3[5];

        bool doneFirst = false;

        Boss.Head.ShotgunLasers.ContinouslyUpdateLasers(laserTargets);

        while (Time.time < startTime + duration)
        {
            int selectedLaserIndex;
            float betweenDelay = Mathf.Lerp(betweenDelayMinMax.x, betweenDelayMinMax.y, (Time.time - startTime) / duration);

            if (!doneFirst)
            {
                doneFirst = true;
                selectedLaserIndex = 2;
                betweenDelay += extraFirstDelay;
            }
            else
            {
                selectedLaserIndex = laserIndexes.GetRandomElement();
            }

            laserIndexes.Remove(selectedLaserIndex);

            uint routineID = 0;

            routineID = Boss.StartBoundRoutine(FireLaser(selectedLaserIndex, laserTargets, () =>
            {
                laserIndexes.Add(selectedLaserIndex);
                laserFireRoutines.Remove(routineID);
            }));

            laserFireRoutines.Add(routineID);

            yield return new WaitForSeconds(betweenDelay);
        }

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser End Super Quick");

        yield return new WaitUntil(() => laserFireRoutines.Count == 0);

        /*var times = amountOfTimesRange.RandomInRange();

        for (int i = 0; i < times; i++)
        {
            yield return DoLaserBurst(i == 0 ? initialDelay : preFireDelay);

            yield return Boss.Head.QuickFlipDirection(Boss.PlayerRightOfBoss);

            if (Cancelled)
            {
                break;
            }
        }*/

        Boss.Head.ShotgunLasers.StopContinouslyUpdating();

        Boss.Head.UnlockHead(Boss.Head.ShotgunLasers.GetCurrentHeadAngle());

        yield break;
    }

    IEnumerator FireLaser(int laserIndex, Vector3[] laserTargets, Action onDone = null)
    {
        var laserOrigin = Boss.Head.ShotgunLasers.Lasers[laserIndex];
        var laser = Boss.Head.ShotgunLasers.LaserEmitters[laserIndex];

        var polarToPlayer = MathUtilities.CartesianToPolar(Player.Player1.transform.position - laserOrigin.position);

        laserTargets[laserIndex] = laserOrigin.transform.position + (Vector3)MathUtilities.PolarToCartesian(polarToPlayer.x + angleOffsetRange.RandomInRange(), polarToPlayer.y);

        if (prepareLaserSound != null)
        {
            WeaverAudio.PlayAtPoint(prepareLaserSound, transform.position);
        }

        laser.ChargeUpLaser_P1();

        yield return new WaitForSeconds(preFireDelay);

        if (fireLaserSound != null)
        {
            var instance = WeaverAudio.PlayAtPoint(fireLaserSound, transform.position);
            instance.AudioSource.pitch = firePitchRange.RandomInRange();
        }

        laser.FireLaser_P2();

        yield return new WaitForSeconds(fireDuration);

        yield return new WaitForSeconds(laser.EndLaser_P3());

        onDone?.Invoke();
    }

    /*Transform GetOrigin(LaserEmitter emitter)
    {
        return emitter.Laser.transform.parent;
    }

    void SetLaserRotation(LaserEmitter emitter, Quaternion rotation)
    {
        rotation = Quaternion.Euler(0, 0, 90f) * rotation;

        var direction = rotation * Vector3.right;

        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GetOrigin(emitter).SetRotationZ(angle);
    }*/

    /*IEnumerator DoLaserBurst(float delay)
    {
        var lasers = Boss.Head.ShotgunLasers.LaserEmitters;

        //var centerLaserOrigin = GetOrigin(lasers[2]);
        var centerLaserOrigin = Boss.Head.ShotgunLasers.Lasers[2];

        var toPlayer = (Player.Player1.transform.position - centerLaserOrigin.transform.position);

        var headToPlayer = (Player.Player1.transform.position - Boss.Head.transform.position);

        Vector2 polarToPlayer = MathUtilities.CartesianToPolar(toPlayer);

        Vector3[] laserTargets = new Vector3[5];

        for (int i = 0; i < Boss.Head.ShotgunLasers.Lasers.Count; i++)
        {
            float scale = 1f;

            if (Boss.Head.MainRenderer.flipX)
            {
                scale = -1f;
            }

            var randomness = angleOffsetRandomizationRange.RandomInRange();

            if (i == 2)
            {
                randomness = UnityEngine.Random.Range(-centerLaserRandomness, centerLaserRandomness);
            }

            var angleOffset = ((i - 2) * angleSpread * scale) + randomness;
            laserTargets[i] = centerLaserOrigin.transform.position + (Vector3)MathUtilities.PolarToCartesian(polarToPlayer.x + angleOffset, polarToPlayer.y);
        }


        Boss.Head.ShotgunLasers.ContinouslyUpdateLasers(laserTargets);
        //var animationDuration = Boss.Head.Animator.AnimationData.GetClipDuration("Fire Laser Antic Quick")

        if (delay > 0f)
        {
            for (int i = 0; i < lasers.Count; i++)
            {
                lasers[i].ChargeUpLaser_P1();
            }
        }

        if (prepareLaserSound != null)
        {
            WeaverAudio.PlayAtPoint(prepareLaserSound, transform.position);
        }

        for (float t = 0; t < delay; t += Time.deltaTime)
        {
            if (Cancelled)
            {
                Boss.Head.ShotgunLasers.StopContinouslyUpdating();
                yield break;
            }
            yield return null;
        }

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic Quick");

        //WeaverLog.Log("ANGLE TO PLAYER = " + MathUtilities.CartesianToPolar(headToPlayer).x);

        Boss.Head.ShotgunLasers.SetHeadSpriteToRotation(MathUtilities.CartesianToPolar(headToPlayer).x);

        //RemoveTransparencies();

        for (int i = 0; i < lasers.Count; i++)
        {
            lasers[i].FireLaser_P2();
        }

        //Boss.Head.Animator.SpriteRenderer.sprite = fireSprite;

        if (fireLaserSound != null)
        {
            WeaverAudio.PlayAtPoint(fireLaserSound, transform.position);
        }
        //controller.ChangeMode(ShotgunController.LaserMode.Firing);

        for (float t = 0; t < fireDuration; t += Time.deltaTime)
        {
            if (Cancelled)
            {
                break;
            }
            yield return null;
        }

        float endTime = 0;
        for (int i = 0; i < lasers.Count; i++)
        {
            endTime = lasers[i].EndLaser_P3();
        }

        Boss.Head.Animator.PlayAnimation("Fire Laser End Super Quick");

        if (!Cancelled)
        {
            yield return new WaitForSeconds(endTime);
        }

        Boss.Head.ShotgunLasers.StopContinouslyUpdating();

        yield break;
    }*/

    public override void OnStun()
    {
        Boss.Head.ShotgunLasers.StopContinouslyUpdating();
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }
    }
}
