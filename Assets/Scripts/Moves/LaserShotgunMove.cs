using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Features;
using WeaverCore.Utilities;

public class LaserShotgunMove : AncientAspidMove
{
    //public List<LaserEmitter> lasers;

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
    List<AudioClip> chargeUpSounds;

    [SerializeField]
    Vector2 chargeUpSoundPitchRange = new Vector2(0.95f, 1.05f);

    [SerializeField]
    List<AudioClip> fireSounds;

    [SerializeField]
    Vector2 fireSoundPitchRange = new Vector2(0.95f, 1.05f);

    [SerializeField]
    AudioClip fireLoop;

    [SerializeField]
    ParticleSystem shotgunMouthParticles;

    [SerializeField]
    float fireSoundStaggerTime = 0.01f;

    [SerializeField]
    Transform shotgunLaserOrigin;

    Transform oldBossTarget;


    Coroutine fireLaserRoutine;
    List<Coroutine> bloodParticlesRoutines = new List<Coroutine>();
    LaserRapidFireMove rapidFireMove;
    AudioPlayer loopSound;
    TargetOverride target = null;
    bool firstTime = false;

    public ShotgunController CurrentController { get; private set; }

    public bool CancelLaserAttack { get; set; }


    public override bool MoveEnabled => false;

    private void Awake()
    {
        rapidFireMove = GetComponent<LaserRapidFireMove>();
        prevLaserOffset = shotgunLaserOrigin.transform.localPosition;
    }

    private void Update()
    {
        if (CurrentController != null)
        {
            CurrentController.Update();
        }
    }

    IEnumerator PlaySoundsStaggered(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        var instance = WeaverAudio.PlayAtPoint(clip, Boss.Head.transform.position, volume);
        instance.AudioSource.pitch = pitch;

        if (fireSoundStaggerTime > 0)
        {
            yield return new WaitForSeconds(fireSoundStaggerTime);
        }

        instance = WeaverAudio.PlayAtPoint(clip, Boss.Head.transform.position, volume);
        instance.AudioSource.pitch = pitch;
    }

    AudioPlayer PlayFireSound()
    {
        StartCoroutine(PlaySoundsStaggered(fireSounds.GetRandomElement(),1f, fireSoundPitchRange.RandomInRange()));
        return WeaverAudio.PlayAtPointLooped(fireLoop, rapidFireMove.GetMiddleBeamContact(Boss.Head.ShotgunLasers.LaserEmitters[2]));
    }

    Vector2 prevLaserOffset;

    public IEnumerator DoShotgunLaser(ShotgunController controller, float prepareTime, float attackTime, bool immediateEnding = false, Sprite fireSprite = null, Vector2 lasersOffset = default)
    {
        var lasers = Boss.Head.ShotgunLasers.LaserEmitters;

        CancelLaserAttack = false;
        lasers[0].transform.parent.localScale = Vector3.one;

        var currentPhase = Boss.CurrentPhase;
        CurrentController = controller;

        controller.Init(lasers);

        if (!Boss.Head.HeadLocked)
        {
            yield return Boss.Head.LockHead();
        }

        shotgunMouthParticles.Play();
        Vector3 bossOffset;

        if (Boss.Head.LookingDirection >= 0f)
        {
            bossOffset = targetOffset;
        }
        else
        {
            bossOffset = new Vector3(-targetOffset.x, targetOffset.y, targetOffset.z);
        }

        if (Boss.CurrentRunningMode == Boss.TacticalMode)
        {
            if (target == null)
            {
                target = Boss.AddTargetOverride();
            }

            target.SetTarget(bossOffset + Player.Player1.transform.position);
        }



        if (prepareTime > 0f)
        {
            for (int i = 0; i < lasers.Count; i++)
            {
                if (controller.LaserEnabled(i))
                {
                    lasers[i].ChargeUpLaser_P1();
                }
            }
        }
        ApplyTransparencies();

        //SetAttackMode(false, Boss.Head.LookingDirection >= 0, controller.DoScaleFlip());

        if (prepareTime > 0f)
        {
            StartCoroutine(PlaySoundsStaggered(chargeUpSounds.GetRandomElement(), 1f, chargeUpSoundPitchRange.RandomInRange()));
        }

        controller.ChangeMode(ShotgunController.LaserMode.Preparing);

        //List<Quaternion> laserAngles = new List<Quaternion>();
        Quaternion[] laserAngles = new Quaternion[5];

        Boss.Head.ShotgunLasers.ContinouslyUpdateLasers(laserAngles);

        void UpdateLaserRotations(ShotgunController controller)
        {
            for (int i = 0; i < lasers.Count; i++)
            {
                if (controller.LaserEnabled(i))
                {
                    laserAngles[i] = controller.GetLaserRotation(i);
                }
            }
        }

        if (prepareTime > 0f)
        {
            for (float i = 0; i < prepareTime; i += Time.deltaTime)
            {
                UpdateLaserRotations(controller);
                yield return null;
            }
        }

        if (prepareTime > 0f)
        {
            for (int i = 0; i < lasers.Count; i++)
            {
                if (controller.LaserEnabled(i))
                {
                    lasers[i].EndLaser_P3();
                }
            }
        }

        shotgunMouthParticles.Stop();

        var oldFlip = Boss.Head.MainRenderer.flipX;

        if (!Cancelled && Boss.CurrentPhase == currentPhase && Vector3.Distance(transform.position, Player.Player1.transform.position) <= 28f)
        {
            var totalDelay = attackDelay + Boss.Head.Animator.AnimationData.GetClipDuration("Fire Laser Antic Quick") - lasers[0].MinChargeUpDuration;

            FireLasers(totalDelay, controller, attackTime, laserAngles);

            controller.ChangeMode(ShotgunController.LaserMode.PostPrepare);

            for (float t = 0; t < attackDelay; t += Time.deltaTime)
            {
                UpdateLaserRotations(controller);
                yield return null;
            }
            Boss.Head.MainRenderer.flipX = Boss.Head.LookingDirection >= 0f;

            yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic Quick");

            CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.RumblingMed);

            loopSound = PlayFireSound();

            for (int i = 0; i < lasers.Count; i++)
            {
                if (controller.LaserEnabled(i))
                {
                    bloodParticlesRoutines.Add(StartCoroutine(EmitParticlesRoutine(bloodSpawnRate, i)));
                }
            }

            Boss.Head.Animator.SpriteRenderer.sprite = fireSprite ?? attackSprite;

            shotgunLaserOrigin.transform.localPosition = lasersOffset;



            yield return new WaitUntil(() => fireLaserRoutine == null || controller.CurrentMode == ShotgunController.LaserMode.Ending);
            foreach (var routine in bloodParticlesRoutines)
            {
                StopCoroutine(routine);
            }
            bloodParticlesRoutines.Clear();

            loopSound.Delete();
            loopSound = null;

            CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.None);
        }
        if (target != null)
        {
            Boss.RemoveTargetOverride(target);
            target = null;
        }

        if (!immediateEnding)
        {
            yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser End Quick");
        }

        Boss.Head.MainRenderer.flipX = oldFlip;

        controller.OnDone();

        if (CurrentController == controller)
        {
            CurrentController = null;
        }

        Boss.Head.ShotgunLasers.StopContinouslyUpdating();
    }

    public IEnumerator DoShotgunLaser()
    {
        return DoShotgunLaser(new DefaultShotgunController(targetLerpSpeed, attackRotationSpeed, laserRotations), prepareTime, attackTime);
    }

    protected override IEnumerator OnExecute()
    {
        return DoShotgunLaser();
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
        //SetAttackMode(false, Boss.Head.LookingDirection >= 0, false);
        if (target != null)
        {
            Boss.RemoveTargetOverride(target);
            target = null;
        }
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

        //lasers[0].transform.parent.localScale = Vector3.one;
        shotgunLaserOrigin.transform.localPosition = prevLaserOffset;
    }

    /*void UpdateLaserRotations(ShotgunController controller)
    {
        var ninety = Quaternion.Euler(0, 0, 90f);
        for (int i = 0; i < lasers.Count; i++)
        {
            if (controller.LaserEnabled(i))
            {
                var rotation = ninety * controller.GetLaserRotation(i);

                var direction = rotation * Vector3.right;

                var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                GetOrigin(lasers[i]).SetRotationZ(angle);
            }
        }
    }

    public float GetAngleToTargetFromLaser(int laserIndex, Vector3 target)
    {
        return MathUtilities.CartesianToPolar(target - GetOrigin(lasers[laserIndex]).position).x;
    }

    public Quaternion GetLaserAngle(int laserIndex)
    {
        return GetOrigin(lasers[laserIndex]).rotation * Quaternion.Euler(0, 0, -90f);
    }

    public Vector3 GetFarAwayLaserTarget(int laserIndex)
    {
        return GetOrigin(lasers[laserIndex]).position + (Vector3)MathUtilities.PolarToCartesian(GetLaserAngle(laserIndex).eulerAngles.z, 1000f);
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
                rapidFireMove.PlayBloodEffects(laserBloodAmounts[laserIndex], Boss.Head.ShotgunLasers.LaserEmitters[laserIndex]);
            }
            yield return null;
        }
    }

    void FireLasers(float delay, ShotgunController controller, float attackTime, Quaternion[] laserAngles)
    {
        var lasers = Boss.Head.ShotgunLasers.LaserEmitters;

        void UpdateLaserRotations(ShotgunController controller)
        {
            for (int i = 0; i < lasers.Count; i++)
            {
                if (controller.LaserEnabled(i))
                {
                    laserAngles[i] = controller.GetLaserRotation(i);
                }
            }
        }

        IEnumerator FireRoutine()
        {
            yield return new WaitForSeconds(delay);

            float duration = 0;

            //SetAttackMode(true, Boss.Head.LookingDirection >= 0, controller.DoScaleFlip());

            for (int i = 0; i < lasers.Count; i++)
            {
                if (controller.LaserEnabled(i))
                {
                    duration = lasers[i].ChargeUpLaser_P1();
                }
            }
            controller.ChangeMode(ShotgunController.LaserMode.Prefire);

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                UpdateLaserRotations(controller);
                yield return null;
            }

            RemoveTransparencies();

            for (int i = 0; i < lasers.Count; i++)
            {
                if (controller.LaserEnabled(i))
                {
                    lasers[i].FireLaser_P2();
                }
            }
            controller.ChangeMode(ShotgunController.LaserMode.Firing);

            for (float t = 0; t < attackTime; t += Time.deltaTime)
            {
                UpdateLaserRotations(controller);

                if (Cancelled || CancelLaserAttack)
                {
                    break;
                }
                yield return null;
            }

            float endTime = 0;

            for (int i = 0; i < lasers.Count; i++)
            {
                if (controller.LaserEnabled(i))
                {
                    endTime = lasers[i].EndLaser_P3();
                }
            }
            controller.ChangeMode(ShotgunController.LaserMode.Ending);

            for (float t = 0; t < endTime; t += Time.deltaTime)
            {
                UpdateLaserRotations(controller);
                yield return null;
            }

            controller.ChangeMode(ShotgunController.LaserMode.None);

            lasers[0].transform.parent.localScale = Vector3.one;

            fireLaserRoutine = null;

            shotgunLaserOrigin.transform.localPosition = prevLaserOffset;
        }

        fireLaserRoutine = StartCoroutine(FireRoutine());
    }

    void StopLasers()
    {
        if (fireLaserRoutine != null)
        {
            fireLaserRoutine = null;
            StopCoroutine(fireLaserRoutine);
            foreach (var laser in Boss.Head.ShotgunLasers.LaserEmitters)
            {
                laser.StopLaser();
            }
        }
    }


    /*Transform GetOrigin(LaserEmitter emitter)
    {
        return emitter.Laser.transform.parent;
    }*/

    public void ApplyTransparencies()
    {
        var lasers = Boss.Head.ShotgunLasers.LaserEmitters;
        for (int i = 0; i < lasers.Count; i++)
        {
            var color = lasers[i].Laser.Color;
            color.a = laserTransparencies[i];
            lasers[i].Laser.Color = color;
        }
    }

    public void RemoveTransparencies()
    {
        var lasers = Boss.Head.ShotgunLasers.LaserEmitters;
        for (int i = 0; i < lasers.Count; i++)
        {
            var color = lasers[i].Laser.Color;
            color.a = 1;
            lasers[i].Laser.Color = color;
        }
    }

    /*public void SetAttackMode(bool attackMode, bool facingRight, bool doScaleFlip)
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

        if (doScaleFlip)
        {
            parent.localScale = new Vector3(facingRight ? -1 : 1, 1f, 1f);
        }
    }*/

    /*Vector3 FlipXIfOnRight(Vector3 position, bool facingRight)
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
    }*/
}
