using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore;
using System.Collections;

public class BossTrailerLungeTrigger : MonoBehaviour 
{
    [SerializeField]
    float trailerModeDestY = 0f;

    [SerializeField]
    AncientAspid boss;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<HeroController>() != null)
        {
            if (boss == null)
            {
                boss = GameObject.FindObjectOfType<AncientAspid>();
            }
            StartCoroutine(TriggerTrailerLandingRoutine());
        }
    }

    public IEnumerator TriggerTrailerLandingRoutine()
    {
        boss.Body.PlayDefaultAnimation = false;

        IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        boss.ApplyFlightVariance = false;

        var headRoutine = boss.Head.LockHead(boss.Head.LookingDirection >= 0f ? 60f : -60f, 1f);
        var bodyRoutine = boss.Body.RaiseTail();
        var minWaitTimeRoutine = Wait(0.5f);

        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(boss, headRoutine, bodyRoutine, minWaitTimeRoutine);

        yield return awaiter.WaitTillDone();

        boss.FlightEnabled = false;
        boss.Recoil.SetRecoilSpeed(0f);

        var bigBlobMove = GetComponent<EnterGround_BigVomitShotMove>();

        yield return new WaitUntil(() => !boss.Claws.DoingBasicAttack);

        foreach (var claw in boss.Claws.claws)
        {
            yield return claw.LockClaw();
        }


        boss.Wings.PrepareForLunge();
        boss.Claws.PrepareForLunge();

        boss.Wings.DoLunge();
        boss.Claws.DoLunge();
        boss.WingPlates.DoLunge();
        boss.Head.DoLunge();

        var groundMode = boss.GetComponent<GroundMode>();

        if (groundMode.lungeSound != null)
        {
            WeaverAudio.PlayAtPoint(groundMode.lungeSound, boss.transform.position);
        }

        Vector3 destination = boss.transform.position.With(y: boss.transform.position.y - 100f);

        var angleToDestination = VectorUtilities.VectorToDegrees((destination - boss.transform.position).normalized);

        var downwardAngle = Vector3.Dot(Vector3.right, (destination - boss.transform.position).normalized) * 90f;

        groundMode.lungeDashEffect.SetActive(true);
        groundMode.lungeDashRotationOrigin.SetZLocalRotation(angleToDestination);


        bool steepAngle = true;

        boss.Rbody.velocity = (destination - boss.transform.position).normalized * groundMode.lungeSpeed;

        yield return null;

        var yVelocity = boss.Rbody.velocity.y;
        var xVelocity = boss.Rbody.velocity.x;
        var halfVelocity = Mathf.Abs(boss.Rbody.velocity.y) / 2f;



        yield return new WaitUntil(() => boss.transform.position.y <= trailerModeDestY);

        transform.SetPositionY(trailerModeDestY);

        boss.Rbody.velocity = default;

        var headLandRoutine = boss.Head.PlayLanding(steepAngle);
        var bodyLandRoutine = boss.Body.PlayLanding(steepAngle);
        var wingPlateLandRoutine = boss.WingPlates.PlayLanding(steepAngle);
        var wingsLandRoutine = boss.Wings.PlayLanding(steepAngle);
        var clawsLandRoutine = boss.Claws.PlayLanding(steepAngle);
        var shiftPartsRoutine = boss.ShiftBodyParts(steepAngle, boss.Body, boss.WingPlates, boss.Wings);

        List<uint> landingRoutines = new List<uint>();

        var landingAwaiter = RoutineAwaiter.AwaitBoundRoutines(boss, landingRoutines, headLandRoutine, bodyLandRoutine, wingPlateLandRoutine, wingsLandRoutine, clawsLandRoutine, shiftPartsRoutine);

        bool slammed = false;

        uint switchDirectionRoutine = 0;
        AspidOrientation oldOrientation = boss.Orientation;

        groundMode.lungeRockParticles.Play();

        if (groundMode.lungeLandSoundHeavy != null)
        {
            WeaverAudio.PlayAtPoint(groundMode.lungeLandSoundHeavy, boss.transform.position);
        }
        CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.EnemyKillShake);

        boss.Rbody.velocity = boss.Rbody.velocity.With(x: 0f);

        var headFinishRoutine = boss.Head.FinishLanding(slammed);
        var bodyFinishRoutine = boss.Body.FinishLanding(slammed);
        var wingPlateFinishRoutine = boss.WingPlates.FinishLanding(slammed);
        var wingsFinishRoutine = boss.Wings.FinishLanding(slammed);
        var clawsFinishRoutine = boss.Claws.FinishLanding(slammed);

        var finishAwaiter = RoutineAwaiter.AwaitBoundRoutines(boss, headFinishRoutine, bodyFinishRoutine, wingPlateFinishRoutine, wingsFinishRoutine, clawsFinishRoutine);

        yield return finishAwaiter.WaitTillDone();

        if (switchDirectionRoutine != 0 && boss.Orientation == oldOrientation)
        {
            yield return new WaitUntil(() => boss.Orientation != oldOrientation);
        }

        yield break;
    }
}
