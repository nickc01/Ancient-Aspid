using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Utilities;

[ExecuteAlways]
public class FireLaserMove : AncientAspidMove
{
    [SerializeField]
    bool moveEnabled = true;

    [SerializeField]
    LaserEmitter emitter;

    [Header("Sweep Move")]
    [SerializeField]
    float sweepTime = 5;

    [SerializeField]
    float sweepAnticTime = 0.5f;

    [SerializeField]
    float sweepStartAngle = -90f - 45f;

    [SerializeField]
    float sweepEndAngle = -90f + 45f;

    [Header("Follow Player Move")]
    [SerializeField]
    float followPlayerTime = 5;

    [SerializeField]
    float followPlayerSpeed = 2f;

    [SerializeField]
    float followPlayerAnticTime = 0.5f;

    [SerializeField]
    List<Sprite> head_Sprites;

    [SerializeField]
    List<bool> head_HorizFlip;

    [SerializeField]
    List<float> head_Degrees;

    public override bool MoveEnabled => Boss.Orientation == AspidOrientation.Center && moveEnabled;

    Transform laserRotationOrigin;
    float minEmitterAngle;
    float maxEmitterAngle;

    private void Awake()
    {
        laserRotationOrigin = emitter.transform.GetChild(0);
        minEmitterAngle = head_Degrees[0];
        maxEmitterAngle = head_Degrees[head_Degrees.Count - 1];
    }

    public override IEnumerator DoMove()
    {
        return FireLaser();
    }

    /*private void Update()
    {
        Debug.Log("Laser Angle = " + emitter.Laser.transform.GetZRotation());
    }*/

    IEnumerator FireLaserRoutine(Action<float> updateFunction, float time, float anticTime, float startAngle)
    {
        yield return Boss.Head.DisableFollowPlayer();

        var anticClip = Boss.Head.AnimationPlayer.AnimationData.GetClip("Fire Laser Antic");
        var clipTime = (1f / anticClip.FPS) * anticClip.Frames.Count;

        anticTime = Mathf.Clamp(anticTime, clipTime,anticTime);
        emitter.Laser.transform.rotation = Quaternion.Euler(0f,0f,startAngle);
        emitter.ChargeUpDuration = anticTime;
        emitter.FireDuration = time;
        emitter.FireLaser();

        float timer = 0f;

        IEnumerator LaserUpdateRoutine()
        {
            while (true)
            {
                updateFunction(timer);
                yield return null;
            }
        }

        var updateRoutine = Boss.StartBoundRoutine(LaserUpdateRoutine());

        yield return new WaitForSeconds(anticTime - clipTime);


        Boss.Head.MainRenderer.flipX = Boss.Head.HeadFacingRight;

        yield return Boss.Head.AnimationPlayer.PlayAnimationTillDone("Fire Laser Antic");

        int oldIndex = -1;

        for (timer = 0; timer < time; timer += Time.deltaTime)
        {
            var spriteIndex = GetHeadSpriteIndexForAngle(emitter.Laser.transform.GetZRotation());
            if (spriteIndex != oldIndex)
            {
                oldIndex = spriteIndex;

                Boss.Head.MainRenderer.sprite = head_Sprites[spriteIndex];
                Boss.Head.MainRenderer.flipX = head_HorizFlip[spriteIndex];
            }
            yield return null;
        }

        Boss.StopBoundRoutine(updateRoutine);

        Boss.Head.EnableFollowPlayer();

        //TODO -- ENDING ANIMATION
    }

    void SetLaserRotation(float angle)
    {
        if (angle < minEmitterAngle)
        {
            emitter.transform.rotation = Quaternion.Euler(0f,0f,minEmitterAngle);
            laserRotationOrigin.rotation = Quaternion.Euler(0f,0f,-90 + (angle - minEmitterAngle));
        }
        else if (angle > maxEmitterAngle)
        {
            emitter.transform.rotation = Quaternion.Euler(0f, 0f, maxEmitterAngle);
            laserRotationOrigin.rotation = Quaternion.Euler(0f, 0f, -90 + (angle - maxEmitterAngle));
        }
        else
        {
            emitter.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            laserRotationOrigin.rotation = Quaternion.Euler(0f, 0f, -90f);
        }
    }

    float GetLaserRotation()
    {
        return emitter.transform.eulerAngles.z + 90f + laserRotationOrigin.transform.eulerAngles.z;
    }

    public IEnumerator FireLaser()
    {
        if (UnityEngine.Random.Range(0, 2) == 1)
        {
            return FireLaserAtPlayer(followPlayerTime, followPlayerSpeed);
        }
        else
        {
            return FireLaser(sweepStartAngle, sweepEndAngle, sweepTime);
        }
    }

    float GetLaserAngleToPlayer()
    {
        return MathUtilities.CartesianToPolar(Player.Player1.transform.position - emitter.Laser.transform.position).x;
    }

    public IEnumerator FireLaserAtPlayer(float time, float followSpeed)
    {
        return FireLaserRoutine(currentTime =>
        {
            var from = Quaternion.Euler(0f,0f, GetLaserRotation());
            var to = Quaternion.Euler(0f,0f, GetLaserAngleToPlayer() + 90f);

            SetLaserRotation(Quaternion.Slerp(from,to,followSpeed * Time.deltaTime).eulerAngles.z);
            //emitter.Laser.transform.rotation = Quaternion.Slerp(emitter.Laser.transform.rotation, Quaternion.Euler(0f, 0f, GetLaserAngleToPlayer()), followSpeed * Time.deltaTime);
        }, time, followPlayerAnticTime, GetLaserAngleToPlayer());
    }

    public IEnumerator FireLaser(float fromAngle, float toAngle, float time)
    {
        return FireLaserRoutine(currentTime =>
        {
            var from = Quaternion.Euler(0f, 0f, fromAngle);
            var to = Quaternion.Euler(0f, 0f, toAngle);


            //emitter.Laser.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(fromAngle,toAngle,currentTime / time));
            SetLaserRotation(Quaternion.Slerp(from,to,currentTime / time).eulerAngles.z);
        },time, sweepAnticTime, fromAngle);
    }

    public override void OnStun()
    {
        
    }

    int GetHeadSpriteIndexForAngle(float angle)
    {
        for (int i = head_Degrees.Count - 1; i >= 0; i--)
        {
            if (i == head_Degrees.Count - 1)
            {
                if (angle >= Mathf.Lerp(head_Degrees[i - 1], head_Degrees[i], 0.5f))
                {
                    return i;
                }
            }
            else if (i == 0)
            {
                if (angle < Mathf.Lerp(head_Degrees[i], head_Degrees[i + 1], 0.5f))
                {
                    return i;
                }
            }
            else
            {
                if (angle < Mathf.Lerp(head_Degrees[i], head_Degrees[i + 1], 0.5f) && angle >= Mathf.Lerp(head_Degrees[i - 1], head_Degrees[i], 0.5f))
                {
                    return i;
                }
            }
        }
        return -1;
    }
}
