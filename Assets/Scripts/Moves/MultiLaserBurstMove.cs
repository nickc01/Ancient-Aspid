using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Utilities;

public class MultiLaserBurstMove : AncientAspidMove
{
    [SerializeField]
    Vector2Int amountOfTimesRange = new Vector2Int(3, 5);

    [SerializeField]
    float initialDelay = 0.6f;

    [SerializeField]
    float preFireDelay = 0.3f;

    [SerializeField]
    float fireDuration = 0.25f;

    [SerializeField]
    float angleSpread = 15f;

    [SerializeField]
    AudioClip prepareLaserSound;

    [SerializeField]
    AudioClip fireLaserSound;

    [SerializeField]
    Vector2 fireLaserSoundPitchRange = new Vector2(0.95f, 1.05f);

    [SerializeField]
    Vector2 angleOffsetRandomizationRange = new Vector2(-5f, 5f);

    [SerializeField]
    float centerLaserRandomness = 0f;

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

        var times = amountOfTimesRange.RandomInRange();

        for (int i = 0; i < times; i++)
        {
            yield return DoLaserBurst(i == 0 ? initialDelay : preFireDelay);

            yield return Boss.Head.QuickFlipDirection(Boss.PlayerRightOfBoss);

            if (Cancelled)
            {
                break;
            }
        }

        var direction = Boss.Head.ShotgunLasers.GetCurrentHeadAngle();
        Boss.Head.UnlockHead(direction < 0f ? AspidOrientation.Left : AspidOrientation.Right);
        //Boss.Head.UnlockHead();

        yield break;
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

    IEnumerator DoLaserBurst(float delay)
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
            /*var angleOffset = ((i - 2) * angleSpread) + angleOffsetRandomizationRange.RandomInRange();

            var toPlayer = (Player.Player1.transform.position - centerLaserOrigin.transform.position);

            var rotation = Quaternion.Euler(0f, 0f, MathUtilities.CartesianToPolar(toPlayer).x + angleOffset);*/

            //SetLaserRotation(lasers[i], rotation);
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
                break;
            }
            yield return null;
        }

        if (!Cancelled)
        {
            yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic Quick");

            //WeaverLog.Log("ANGLE TO PLAYER = " + MathUtilities.CartesianToPolar(headToPlayer).x);

            var oldFlipX = Boss.Head.MainRenderer.flipX;
            Boss.Head.ShotgunLasers.SetHeadSpriteToRotation(MathUtilities.CartesianToPolar(headToPlayer).x);
            var newFlipX = Boss.Head.MainRenderer.flipX;

            if (oldFlipX != newFlipX)
            {
                var temp = laserTargets[0];
                laserTargets[0] = laserTargets[4];
                laserTargets[4] = temp;

                temp = laserTargets[1];
                laserTargets[1] = laserTargets[3];
                laserTargets[3] = temp;
            }
            /*for (float t = 0; t < fireDuration; t += Time.deltaTime)
            {
                //UpdateLaserRotations(controller);
                yield return null;
            }*/

            //RemoveTransparencies();

            for (int i = 0; i < lasers.Count; i++)
            {
                lasers[i].FireLaser_P2();
            }

            //Boss.Head.Animator.SpriteRenderer.sprite = fireSprite;

            if (fireLaserSound != null)
            {
                var instance = WeaverAudio.PlayAtPoint(fireLaserSound, transform.position);
                instance.AudioSource.pitch = fireLaserSoundPitchRange.RandomInRange();
            }
            //controller.ChangeMode(ShotgunController.LaserMode.Firing);

            /*for (float t = 0; t < attackTime; t += Time.deltaTime)
            {
                UpdateLaserRotations(controller);

                if (Cancelled || CancelLaserAttack)
                {
                    break;
                }
                yield return null;
            }*/

            for (float t = 0; t < fireDuration; t += Time.deltaTime)
            {
                if (Cancelled)
                {
                    break;
                }
                yield return null;
            }

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


        //Boss.Head.UnlockHead(direction < 0f ? AspidOrientation.Left : AspidOrientation.Right);
        //Boss.Head.UnlockHead(Boss.Head.MainRenderer.flipX ? 60f : -60f);

        yield break;
    }

    public override void OnStun()
    {
        Boss.Head.ShotgunLasers.StopContinouslyUpdating();
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }
    }
}
