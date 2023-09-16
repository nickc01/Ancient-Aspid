using System;
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class FarAwayLaser : AncientAspidMove
{
    [SerializeField]
    float minDistance = 35f;

    [SerializeField]
    float prepareDuration = 0.75f;

    [SerializeField]
    float fireDelay = 0.25f;

    [SerializeField]
    LaserEmitter farLaserEmitter;

    [SerializeField]
    Transform laserRotationOrigin;

    [SerializeField]
    float predictionAmount = 2f;

    [SerializeField]
    float fireDuration = 0.5f;

    float oldRotationX;

    [SerializeField]
    Vector2Int fireTimesRange = new Vector2Int(1, 3);

    int fireTimeTarget;
    int fireTimeCounter;

    [SerializeField]
    AudioClip preFireSound;

    [SerializeField]
    AudioClip fireSound;

    [field: SerializeField]
    public bool moveEnabled { get; set; } = true;


    public override bool MoveEnabled => moveEnabled && !Boss.FlyingAway && Vector3.Distance(Player.Player1.transform.position,transform.position) > minDistance;

    private void Awake()
    {
        oldRotationX = laserRotationOrigin.localPosition.x;
        fireTimeTarget = UnityEngine.Random.Range(fireTimesRange.x,fireTimesRange.y);
        laserRotationOrigin.SetParent(null);
    }

    private void Update()
    {
        laserRotationOrigin.position = Boss.transform.position;
    }

    public override float PostDelay => 0.5f;


    void PointLaserTowards(Vector3 position)
    {
        var direction = (position - laserRotationOrigin.transform.position);

        laserRotationOrigin.SetZLocalRotation((float)((Math.Atan2(direction.y,direction.x) / Mathf.PI) * 180f));
    }


    TargetOverride targetter;
    uint aimRoutine;

    Func<Vector3> aimTargetGetter;

    IEnumerator AimTowardsPlayerRoutine()
    {
        while (true)
        {
            if (aimTargetGetter != null)
            {
                PointLaserTowards(aimTargetGetter());
            }
            yield return null;
        }
    }

    public override IEnumerator DoMove()
    {
        yield return new WaitForSeconds(0.1f);
        if (++fireTimeCounter < fireTimeTarget)
        {
            yield break;
        }

        fireTimeCounter = 0;
        fireTimeTarget = UnityEngine.Random.Range(fireTimesRange.x, fireTimesRange.y);

        yield return Boss.Head.LockHead();

        targetter = Boss.AddTargetOverride();

        targetter.SetTarget(transform.position);

        //laserRotationOrigin.SetXLocalPosition(Boss.Head.CurrentOrientation == AspidOrientation.Right ? -oldRotationX : oldRotationX);

        if (preFireSound != null)
        {
            WeaverAudio.PlayAtPoint(preFireSound, Player.Player1.transform.position);
        }

        farLaserEmitter.ChargeUpLaser_P1();

        aimTargetGetter = () => Player.Player1.transform.position + new Vector3(0f, 0.5f);
        aimRoutine = Boss.StartBoundRoutine(AimTowardsPlayerRoutine());

        for (float t = 0; t < prepareDuration; t += Time.deltaTime)
        {
            //PointLaserTowards(Player.Player1.transform.position + new Vector3(0f, 0.5f));
            //laserRotationOrigin.LookAt(Player.Player1.transform.position);
            yield return null;
        }

        yield return new WaitForSeconds(farLaserEmitter.EndLaser_P3());

        yield return new WaitForSeconds(fireDelay);

        var playerOld = Player.Player1.transform.position;

        yield return new WaitForSeconds(1f / 30f);

        var playerNew = Player.Player1.transform.position;

        var playerVelocity = (playerNew - playerOld) / (1f / 30f);

        //laserRotationOrigin.LookAt(Player.Player1.transform.position + (playerVelocity * predictionAmount));

        var lastPlayerPos = Player.Player1.transform.position + new Vector3(0f, 0.5f) + (playerVelocity * predictionAmount);

        aimTargetGetter = () => lastPlayerPos;

        //PointLaserTowards(lastPlayerPos);

        yield return new WaitForSeconds(farLaserEmitter.ChargeUpLaser_P1());

        if (fireSound != null)
        {
            WeaverAudio.PlayAtPoint(fireSound, Player.Player1.transform.position);
        }

        farLaserEmitter.FireLaser_P2();

        //yield return new WaitForSeconds(fireDuration);

        for (float t = 0; t < fireDuration; t += Time.deltaTime)
        {
            //PointLaserTowards(lastPlayerPos);
            yield return null;
        }


        yield return new WaitForSeconds(farLaserEmitter.EndLaser_P3());

        //laserRotationOrigin.SetXLocalPosition(oldRotationX);

        Boss.StopBoundRoutine(aimRoutine);
        aimRoutine = 0;

        Boss.Head.UnlockHead();

        Boss.RemoveTargetOverride(targetter);
        targetter = null;

        yield break;
    }

    public override void OnStun()
    {
        farLaserEmitter.StopLaser();
        //laserRotationOrigin.SetXLocalPosition(oldRotationX);

        if (Boss.Head.HeadLocked && !Boss.Head.HeadBeingUnlocked)
        {
            Boss.Head.UnlockHead();
        }

        if (targetter != null)
        {
            Boss.RemoveTargetOverride(targetter);
            targetter = null;
        }
    }
}
