using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;

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


    Vector3 targetPos;
    Coroutine fireLaserRoutine;
    List<Coroutine> bloodParticlesRoutines = new List<Coroutine>();
    LaserRapidFireMove rapidFireMove;
    AudioPlayer loopSound;

    public override bool MoveEnabled => false;

    private void Awake()
    {
        rapidFireMove = GetComponent<LaserRapidFireMove>();
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

    public override IEnumerator DoMove()
    {
        shotgunMouthParticles.Play();
        oldBossTarget = Boss.TargetTransform;
        targetPos = Player.Player1.transform.position;

        if (Boss.Head.LookingDirection >= 0f)
        {
            Boss.SetTarget(Player.Player1.transform.position + targetOffset);
        }
        else
        {
            Boss.SetTarget(Player.Player1.transform.position + new Vector3(-targetOffset.x,targetOffset.y,targetOffset.z));
        }

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

        Boss.SetTarget(Boss.transform.position);

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

        Boss.SetTarget(oldBossTarget);

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser End Quick");

        SetAttackMode(false, Boss.Head.LookingDirection >= 0);
    }

    public override void OnStun()
    {
        CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.None);
        StopLasers();
        SetAttackMode(false, Boss.Head.LookingDirection >= 0);
        Boss.SetTarget(oldBossTarget);

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
    }

    float LaserLookAt(LaserEmitter emitter, Vector3 target)
    {
        var origin = GetOrigin(emitter);

        var difference = new Vector2(target.x, target.y) - new Vector2(origin.position.x,origin.position.y);

        difference = origin.transform.parent.InverseTransformVector(difference);

        var angle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        origin.transform.SetRotationZ(angle + 90f);

        return angle + 90f;
    }

    void SetLaserRotation(LaserEmitter emitter, float zDegrees)
    {
        var origin = GetOrigin(emitter);
        origin.transform.SetRotationZ(zDegrees);
    }

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

    void FireLasers(float delay)
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

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                AimLasersAtTarget(targetPos);
                yield return null;
            }

            RemoveTransparencies();

            foreach (var laser in lasers)
            {
                laser.FireLaser_P2();
            }

            for (float t = 0; t < attackTime; t += Time.deltaTime)
            {
                targetPos = Vector3.MoveTowards(targetPos, Player.Player1.transform.position, attackRotationSpeed * Time.deltaTime);
                AimLasersAtTarget(targetPos);
                yield return null;
            }

            float endTime = 0;

            foreach (var laser in lasers)
            {
                endTime = laser.EndLaser_P3();
            }

            for (float t = 0; t < endTime; t += Time.deltaTime)
            {
                AimLasersAtTarget(targetPos);
                yield return null;
            }

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

    void AimLasersAtTarget(Vector3 target)
    {
        var center = lasers[2];

        var rotation = LaserLookAt(center, target);

        for (int i = 0; i < lasers.Count; i++)
        {
            lasers[i].Laser.transform.parent.SetRotationZ(rotation + laserRotations[i]);
        }
    }
    

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
