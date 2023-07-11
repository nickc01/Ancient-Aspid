using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Features;

public class LaserShotgunMove : AncientAspidMove
{
    public List<LaserEmitter> lasers;

    public List<float> laserTransparencies;

    public List<float> laserRotations = new List<float>
    {
        -25f,
        -10f,
        0f,
        10f,
        25f
    };

    public List<int> laserBloodAmounts = new List<int>
    {
        3,3,3,3,3
    };

    [SerializeField]
    Vector3 idlePosition;

    [SerializeField]
    Vector3 idleRotation;

    [SerializeField]
    Vector3 attackPosition;

    [SerializeField]
    Vector3 attackRotation;

    [SerializeField]
    Vector3 targetOffset;

    [SerializeField]
    float bloodSpawnRate = 0.1f;

    [SerializeField]
    float targetLerpSpeed = 7;

    [SerializeField]
    float prepareTimeFirstEncounter = 1f;

    [SerializeField]
    float prepareTime = 1f;

    [SerializeField]
    float attackDelay = 0.1f;

    [SerializeField]
    float attackTime = 1.2f;

    [SerializeField]
    float attackRotationSpeed = 5f;

    [SerializeField]
    Sprite attackSprite;

    [SerializeField]
    AudioClip chargeUpSound;

    [SerializeField]
    AudioClip fireSound;

    [SerializeField]
    AudioClip fireLoop;

    [SerializeField]
    ParticleSystem shotgunMouthParticles;

    [SerializeField]
    float fireSoundStaggerTime = 0.01f;

    Transform oldBossTarget;


    //Vector3 targetPos;
    Coroutine fireLaserRoutine;
    List<Coroutine> bloodParticlesRoutines = new List<Coroutine>();
    LaserRapidFireMove rapidFireMove;
    AudioPlayer loopSound;
    TargetOverride target = null;
    bool firstTime = false;

    public ShotgunController CurrentController { get; private set; }

    public override bool MoveEnabled => false;

    private void Awake()
    {
        rapidFireMove = GetComponent<LaserRapidFireMove>();
    }

    private void Update()
    {
        if (CurrentController != null)
        {
            CurrentController.Update();
        }
    }

    IEnumerator PlaySoundsStaggered(AudioClip clip, float volume = 1f)
    {
        WeaverAudio.PlayAtPoint(clip, Boss.Head.transform.position, volume);

        if (fireSoundStaggerTime > 0)
        {
            yield return new WaitForSeconds(fireSoundStaggerTime);
        }

        WeaverAudio.PlayAtPoint(clip, Boss.Head.transform.position, volume);
    }

    AudioPlayer PlayFireSound()
    {
        StartCoroutine(PlaySoundsStaggered(fireSound));
        return WeaverAudio.PlayAtPointLooped(fireLoop, rapidFireMove.GetMiddleBeamContact(lasers[2]));
    }

    public IEnumerator DoShotgunLaser(ShotgunController controller, float prepareTime, float attackTime)
    {
        CurrentController = controller;

        controller.Init(lasers);

        if (!Boss.Head.HeadLocked)
        {
            yield return Boss.Head.LockHead();
        }

        shotgunMouthParticles.Play();
        //targetPos = Player.Player1.transform.position;

        Vector3 bossOffset;

        if (Boss.Head.LookingDirection >= 0f)
        {
            bossOffset = targetOffset;
        }
        else
        {
            bossOffset = new Vector3(-targetOffset.x, targetOffset.y, targetOffset.z);
        }

        if (Boss.AspidMode == AncientAspid.Mode.Tactical)
        {
            if (target == null)
            {
                target = Boss.AddTargetOverride();
            }

            target.SetTarget(bossOffset + Player.Player1.transform.position);
        }

        for (int i = 0; i < lasers.Count; i++)
        {
            lasers[i].ChargeUpLaser_P1();
        }
        ApplyTransparencies();

        //var center = lasers[2];

        SetAttackMode(false, Boss.Head.LookingDirection >= 0);

        StartCoroutine(PlaySoundsStaggered(chargeUpSound));

        controller.ChangeMode(ShotgunController.LaserMode.Preparing);

        float prepTime = 0f;

        if (firstTime)
        {
            firstTime = false;
            prepTime = prepareTimeFirstEncounter;
        }
        else
        {
            prepTime = prepareTime;
        }

        for (float i = 0; i < prepTime; i += Time.deltaTime)
        {
            //targetPos = Vector3.Lerp(targetPos, Player.Player1.transform.position, Time.deltaTime * targetLerpSpeed);
            //AimLasersAtTarget(targetPos);

            UpdateLaserRotations(controller);
            yield return null;
        }

        for (int i = 0; i < lasers.Count; i++)
        {
            lasers[i].EndLaser_P3();
        }

        shotgunMouthParticles.Stop();

        //Boss.SetTarget(Boss.transform.position);

        var oldFlip = Boss.Head.MainRenderer.flipX;

        if (Vector3.Distance(transform.position, Player.Player1.transform.position) <= 30f)
        {
            var totalDelay = attackDelay + Boss.Head.Animator.AnimationData.GetClipDuration("Fire Laser Antic Quick") - lasers[0].MinChargeUpDuration;

            FireLasers(totalDelay, controller, attackTime);

            controller.ChangeMode(ShotgunController.LaserMode.PostPrepare);

            for (float t = 0; t < attackDelay; t += Time.deltaTime)
            {
                UpdateLaserRotations(controller);
                yield return null;
            }
            //yield return new WaitForSeconds(attackDelay);

            Boss.Head.MainRenderer.flipX = Boss.Head.LookingDirection >= 0f;

            yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic Quick");

            CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.RumblingMed);

            loopSound = PlayFireSound();

            for (int i = 0; i < lasers.Count; i++)
            {
                bloodParticlesRoutines.Add(StartCoroutine(EmitParticlesRoutine(bloodSpawnRate, i)));
            }

            Boss.Head.Animator.SpriteRenderer.sprite = attackSprite;



            yield return new WaitUntil(() => fireLaserRoutine == null);
            foreach (var routine in bloodParticlesRoutines)
            {
                StopCoroutine(routine);
            }
            bloodParticlesRoutines.Clear();

            loopSound.Delete();
            loopSound = null;

            CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.None);
        }
        //Boss.SetTarget(oldBossTarget);
        //Boss.UnfreezeTarget();
        if (target != null)
        {
            Boss.RemoveTargetOverride(target);
            target = null;
        }

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser End Quick");

        Boss.Head.MainRenderer.flipX = oldFlip;

        SetAttackMode(false, Boss.Head.LookingDirection >= 0);

        controller.OnDone();

        if (CurrentController == controller)
        {
            CurrentController = null;
        }

        lasers[0].transform.parent.localScale = Vector3.one;
    }

    public IEnumerator DoShotgunLaser()
    {
        return DoShotgunLaser(new DefaultShotgunController(targetLerpSpeed, attackRotationSpeed, laserRotations), prepareTime, attackTime);
    }

    public override IEnumerator DoMove()
    {
        return DoShotgunLaser();
        /*if (!Boss.Head.HeadLocked)
        {
            yield return Boss.Head.LockHead();
        }

        shotgunMouthParticles.Play();
        targetPos = Player.Player1.transform.position;

        Vector3 bossOffset;

        if (Boss.Head.LookingDirection >= 0f)
        {
            //Boss.SetTarget(Player.Player1.transform.position + targetOffset);
            bossOffset = targetOffset;
        }
        else
        {
            bossOffset = new Vector3(-targetOffset.x, targetOffset.y, targetOffset.z);
            //Boss.SetTarget(Player.Player1.transform.position + new Vector3(-targetOffset.x,targetOffset.y,targetOffset.z));
        }

        //var freezePos = bossOffset + Player.Player1.transform.position;

        //Boss.FreezeTarget(() => freezePos);

        if (target == null)
        {
            target = Boss.AddTargetOverride();
        }

        target.SetTarget(bossOffset + Player.Player1.transform.position);

        for (int i = 0; i < lasers.Count; i++)
        {
            lasers[i].ChargeUpLaser_P1();
        }
        ApplyTransparencies();

        var center = lasers[2];

        SetAttackMode(false,Boss.Head.LookingDirection >= 0);

        StartCoroutine(PlaySoundsStaggered(chargeUpSound));

        for (float i = 0; i < prepareTime; i += Time.deltaTime)
        {
            targetPos = Vector3.Lerp(targetPos, Player.Player1.transform.position, Time.deltaTime * targetLerpSpeed);
            AimLasersAtTarget(targetPos);
            yield return null;
        }

        for (int i = 0; i < lasers.Count; i++)
        {
            lasers[i].EndLaser_P3();
        }

        shotgunMouthParticles.Stop();

        //Boss.SetTarget(Boss.transform.position);

        var totalDelay = attackDelay + Boss.Head.Animator.AnimationData.GetClipDuration("Fire Laser Antic Quick") - lasers[0].MinChargeUpDuration;

        FireLasers(totalDelay);

        yield return new WaitForSeconds(attackDelay);

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic Quick");

        CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.RumblingMed);

        loopSound = PlayFireSound();

        for (int i = 0; i < lasers.Count; i++)
        {
            bloodParticlesRoutines.Add(StartCoroutine(EmitParticlesRoutine(bloodSpawnRate, i)));
        }

        Boss.Head.Animator.SpriteRenderer.sprite = attackSprite;



        yield return new WaitUntil(() => fireLaserRoutine == null);
        foreach (var routine in bloodParticlesRoutines)
        {
            StopCoroutine(routine);
        }
        bloodParticlesRoutines.Clear();

        loopSound.Delete();
        loopSound = null;

        CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.None);

        //Boss.SetTarget(oldBossTarget);
        //Boss.UnfreezeTarget();
        Boss.RemoveTargetOverride(target);
        target = null;

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser End Quick");

        SetAttackMode(false, Boss.Head.LookingDirection >= 0);*/
    }

    public override void OnStun()
    {
        if (CurrentController != null)
        {
            CurrentController.OnStun();
            CurrentController = null;
        }
        CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.None);
        StopLasers();
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }
        Boss.Head.Animator.StopCurrentAnimation();
        SetAttackMode(false, Boss.Head.LookingDirection >= 0);
        //Boss.SetTarget(oldBossTarget);
        if (target != null)
        {
            Boss.RemoveTargetOverride(target);
            target = null;
        }
        //Boss.UnfreezeTarget();

        if (bloodParticlesRoutines.Count > 0)
        {
            foreach (var routine in bloodParticlesRoutines)
            {
                StopCoroutine(routine);
            }
            bloodParticlesRoutines.Clear();
        }

        if (loopSound != null)
        {
            loopSound.Delete();
            loopSound = null;
        }

        shotgunMouthParticles.Stop();

        lasers[0].transform.parent.localScale = Vector3.one;
    }

    void UpdateLaserRotations(ShotgunController controller)
    {
        var ninety = Quaternion.Euler(0, 0, 90f);
        for (int i = 0; i < lasers.Count; i++)
        {
            var rotation = ninety * controller.GetLaserRotation(i);

            var direction = rotation * Vector3.right;

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            GetOrigin(lasers[i]).SetRotationZ(angle);
        }
    }

    /*float LaserLookAt(LaserEmitter emitter, Vector3 target)
    {
        var origin = GetOrigin(emitter);

        var difference = new Vector2(target.x, target.y) - new Vector2(origin.position.x,origin.position.y);

        difference = origin.transform.parent.InverseTransformVector(difference);

        var angle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        origin.transform.SetRotationZ(angle + 90f);

        return angle + 90f;
    }*/

    /*void SetLaserRotation(LaserEmitter emitter, float zDegrees)
    {
        var origin = GetOrigin(emitter);
        origin.transform.SetRotationZ(zDegrees);
    }*/

    IEnumerator EmitParticlesRoutine(float spawnRate, int laserIndex)
    {
        float timer = UnityEngine.Random.Range(0,spawnRate);
        while (true)
        {
            timer += Time.deltaTime;
            if (timer >= spawnRate)
            {
                timer -= spawnRate;
                rapidFireMove.PlayBloodEffects(laserBloodAmounts[laserIndex], lasers[laserIndex]);
            }
            yield return null;
        }
    }

    void FireLasers(float delay, ShotgunController controller, float attackTime)
    {
        IEnumerator FireRoutine()
        {
            yield return new WaitForSeconds(delay);

            float duration = 0;

            SetAttackMode(true, Boss.Head.LookingDirection >= 0);

            foreach (var laser in lasers)
            {
                duration = laser.ChargeUpLaser_P1();
            }

            controller.ChangeMode(ShotgunController.LaserMode.Prefire);

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                //AimLasersAtTarget(targetPos);
                UpdateLaserRotations(controller);
                yield return null;
            }

            RemoveTransparencies();

            foreach (var laser in lasers)
            {
                laser.FireLaser_P2();
            }

            controller.ChangeMode(ShotgunController.LaserMode.Firing);

            for (float t = 0; t < attackTime; t += Time.deltaTime)
            {
                UpdateLaserRotations(controller);
                /*targetPos = Vector3.MoveTowards(targetPos, Player.Player1.transform.position, attackRotationSpeed * Time.deltaTime);
                AimLasersAtTarget(targetPos);*/
                yield return null;
            }

            float endTime = 0;

            foreach (var laser in lasers)
            {
                endTime = laser.EndLaser_P3();
            }

            controller.ChangeMode(ShotgunController.LaserMode.Ending);

            for (float t = 0; t < endTime; t += Time.deltaTime)
            {
                UpdateLaserRotations(controller);
                //AimLasersAtTarget(targetPos);
                yield return null;
            }

            controller.ChangeMode(ShotgunController.LaserMode.None);

            fireLaserRoutine = null;
        }

        fireLaserRoutine = StartCoroutine(FireRoutine());
    }

    void StopLasers()
    {
        if (fireLaserRoutine != null)
        {
            fireLaserRoutine = null;
            StopCoroutine(fireLaserRoutine);
            foreach (var laser in lasers)
            {
                laser.StopLaser();
            }
        }
    }

    /*void AimLasersAtTarget(Vector3 target)
    {
        var center = lasers[2];

        var rotation = LaserLookAt(center, target);

        for (int i = 0; i < lasers.Count; i++)
        {
            lasers[i].Laser.transform.parent.SetRotationZ(rotation + laserRotations[i]);
        }
    }*/
    

    Transform GetOrigin(LaserEmitter emitter)
    {
        return emitter.Laser.transform.parent;
    }

    public void ApplyTransparencies()
    {
        for (int i = 0; i < lasers.Count; i++)
        {
            var color = lasers[i].Laser.Color;
            color.a = laserTransparencies[i];
            lasers[i].Laser.Color = color;
        }
    }

    public void RemoveTransparencies()
    {
        for (int i = 0; i < lasers.Count; i++)
        {
            var color = lasers[i].Laser.Color;
            color.a = 1;
            lasers[i].Laser.Color = color;
        }
    }

    public void SetAttackMode(bool attackMode, bool facingRight)
    {
        var parent = lasers[0].transform.parent;
        if (attackMode)
        {
            parent.localPosition = FlipXIfOnRight(attackPosition, facingRight);
            parent.localEulerAngles = FlipZIfOnRight(attackRotation, facingRight);
        }
        else
        {
            parent.localPosition = FlipXIfOnRight(idlePosition, facingRight);
            parent.localEulerAngles = FlipZIfOnRight(idleRotation, facingRight);
        }

        parent.localScale = new Vector3(facingRight ? -1 : 1, 1f, 1f);
    }

    Vector3 FlipXIfOnRight(Vector3 position, bool facingRight)
    {
        if (facingRight)
        {
            position.x = -position.x;
        }
        return position;
    }

    Vector3 FlipZIfOnRight(Vector3 rotation, bool facingRight)
    {
        if (facingRight)
        {
            rotation.z = -rotation.z;
        }
        return rotation;
    }
}
