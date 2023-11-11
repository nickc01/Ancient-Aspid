using System.Collections;
using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Utilities;

public class GroundModeVomitVariation : GroundModeVariationBase
{
    EnterGround_BigVomitShotMove bigBlobMove;

    public GroundModeVomitVariation(GroundMode mode) : base(mode) { }

    IEnumerator ShootGiantBlob(EnterGround_BigVomitShotMove move, float preDelay = 0.25f)
    {
        yield return new WaitForSeconds(preDelay);
        yield return Boss.RunMove(move);


        for (float t = move.SpawnedGlobEstLandTime; t >= 0.5f; t -= Time.deltaTime)
        {
            yield return null;
        }
    }

    public override IEnumerator OnBegin()
    {
        bigBlobMove = Boss.GetComponent<EnterGround_BigVomitShotMove>();
        yield return ShootGiantBlob(bigBlobMove);
    }

    public override Vector3 GetLungeTarget()
    {
        return bigBlobMove.SpawnedGlob.transform.position + new Vector3(0, 2, 0);
    }

    public override bool DoSlide(Vector3 lungeTarget)
    {
        return false;
    }

    public override void LungeCancel()
    {
        bigBlobMove.SpawnedGlob.ForceDisappear();
    }

    public override void LungeLand(bool sliding)
    {
        if (Vector3.Distance(transform.position, bigBlobMove.SpawnedGlob.transform.position) <= 5)
        {
            bigBlobMove.SpawnedGlob.ForceDisappear();
            Mode.stompSplash.Play();
            Mode.stompPillar.Play();

            var spawnCount = UnityEngine.Random.Range(Mode.groundSplashBlobCount.x, Mode.groundSplashBlobCount.y);

            for (int i = 0; i < spawnCount; i++)
            {
                float angle = Mode.groundSplashAngleRange.RandomInRange();
                float magnitude = Mode.groundSplashVelocityRange.RandomInRange();

                var velocity = MathUtilities.PolarToCartesian(angle, magnitude);

                VomitGlob.Spawn(Boss.GlobPrefab, transform.position + Mode.groundSplashSpawnOffset, velocity);
            }
        }
    }
}

