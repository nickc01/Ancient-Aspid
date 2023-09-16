using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class BombMove : AncientAspidMove
{
    [SerializeField]
    float headSpeed = 1.5f;

    [SerializeField]
    [Range(1, 2)]
    int attackVariant = 1;

    [SerializeField]
    Bomb bombPrefab;

    [SerializeField]
    float shotSpeed = 25f;

    [SerializeField]
    int shotAmount = 1;

    [SerializeField]
    float shotAngleSeparation = 15f;

    [SerializeField]
    List<AudioClip> fireSounds;

    [SerializeField]
    ColorFader fader;

    [SerializeField]
    ParticleSystem bombParticles;

    [SerializeField]
    float postDelay = 0.25f;

    [SerializeField]
    Vector2 angularVelocityRange = new Vector2(-50f,50f);

    public override bool MoveEnabled => Boss.CanSeeTarget &&
        Boss.AspidMode == AncientAspid.Mode.Tactical &&
        Vector3.Distance(Player.Player1.transform.position, transform.position) <= 30f;

    Vector3 baseFaderPos;

    List<Bomb> _lastFiredBombs = new List<Bomb>();

    public IEnumerable<Bomb> LastFiredBombs => _lastFiredBombs;

    /// <summary>
    /// If specified, this bomb controller will get executed when the move gets executed. Otherwise, the default bomb controller is used
    /// </summary>
    public IBombController CustomController { get; set; } = null;

    float _bombGravityScale = float.NaN;
    public float BombGravityScale
    {
        get
        {
            if (float.IsNaN(_bombGravityScale))
            {
                _bombGravityScale = 1f;
                if (bombPrefab.TryGetComponent(out Rigidbody2D rb))
                {
                    _bombGravityScale = rb.gravityScale;
                }
            }
            return _bombGravityScale;
        }
    }

    public override IEnumerator DoMove()
    {
        bool headPreLocked = Boss.Head.HeadLocked;

        if (!headPreLocked)
        {
            yield return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left, headSpeed);
        }

        float gravityScale = 1;
        if (bombPrefab.TryGetComponent(out Rigidbody2D rb))
        {
            gravityScale = rb.gravityScale;
        }

        yield return FireBombs(CustomController ?? new DefaultBombController(shotSpeed, gravityScale));

        CustomController = null;
        /*yield return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left, headSpeed);

        if (baseFaderPos == default)
        {
            baseFaderPos = fader.transform.localPosition;
        }

        if (Boss.Head.LookingDirection >= 0f)
        {
            fader.transform.localPosition = baseFaderPos.With(x: -baseFaderPos.x);
            fader.transform.SetScaleX(-1f);
        }
        else
        {
            fader.transform.localPosition = baseFaderPos;
            fader.transform.SetScaleX(1f);
        }

        fader.Fade(true);
        bombParticles.Play();

        var oldState = Boss.Head.MainRenderer.TakeSnapshot();

        Boss.Head.MainRenderer.flipX = Boss.Head.LookingDirection >= 0f;

        yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Prepare");

        float gravityScale = 1;
        if (bombPrefab.TryGetComponent(out Rigidbody2D rb))
        {
            gravityScale = rb.gravityScale;
        }

        Fire(new DefaultBombController(shotSpeed, gravityScale));

        fader.Fade(false);
        bombParticles.Stop();

        yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Attack");

        Boss.Head.MainRenderer.Restore(oldState);

        Boss.Head.UnlockHead();*/
        if (!headPreLocked)
        {
            Boss.Head.UnlockHead();
        }
    }

    /// <summary>
    /// Fires bombs using the passed in controller. Assumes the boss head is already locked
    /// </summary>
    public IEnumerator FireBombs(IBombController bombController, bool doPrepare = true)
    {
        if (baseFaderPos == default)
        {
            baseFaderPos = fader.transform.localPosition;
        }

        if (Boss.Head.LookingDirection >= 0f)
        {
            fader.transform.localPosition = baseFaderPos.With(x: -baseFaderPos.x);
            fader.transform.SetScaleX(-1f);
        }
        else
        {
            fader.transform.localPosition = baseFaderPos;
            fader.transform.SetScaleX(1f);
        }

        fader.Fade(true);
        bombParticles.Play();

        var oldState = Boss.Head.MainRenderer.TakeSnapshot();

        Boss.Head.MainRenderer.flipX = Boss.Head.LookingDirection >= 0f;

        if (doPrepare)
        {
            yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Prepare");
        }

        /*float gravityScale = 1;
        if (bombPrefab.TryGetComponent(out Rigidbody2D rb))
        {
            gravityScale = rb.gravityScale;
        }*/

        Fire(bombController);
        //Fire(Boss.Head.LookingDirection, 2, 0.5f);

        fader.Fade(false);
        bombParticles.Stop();

        yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Attack");

        Boss.Head.MainRenderer.Restore(oldState);

        //Boss.Head.UnlockHead();
    }

    void Fire(IBombController bombController /*float angle, int shots, float velocityMultplier = 1f*/)
    {
        _lastFiredBombs.Clear();
        if (shotSpeed <= 0)
        {
            shotSpeed = 0.01f;
        }

        var sourcePos = Boss.Head.GetFireSource(Boss.Head.LookingDirection);

        Blood.SpawnBlood(sourcePos, new Blood.BloodSpawnInfo(3*2, 7*2, 10f, 25f, Boss.Head.LookingDirection - 90f - 40f, Boss.Head.LookingDirection - 90f + 40f, null));
        //Blood.SpawnBlood(sourcePos, new Blood.BloodSpawnInfo(3, 4, 10f, 15f, 120f, 150f, null));

        //var velocityToPlayer = MathUtilities.CalculateVelocityToReachPoint(sourcePos, Player.Player1.transform.position, Vector3.Distance(sourcePos, Player.Player1.transform.position) / shotSpeed, gravityScale);

        int shots = bombController.BombsToShoot;

        shots -= 1;

        float lowerShots = -shots / 2;
        int bombIndex = 0;

        for (float i = lowerShots; i <= lowerShots + shots; i++)
        {
            bombController.GetBombInfo(bombIndex,sourcePos,out var velocity,out var size);

            var polarVelocity = MathUtilities.CartesianToPolar(velocity);

            var bomb = FireShot(polarVelocity.x, shotAngleSeparation * i, sourcePos, polarVelocity.y, size);

            var z = bombController.GetBombZAxis(bombIndex);

            if (!float.IsNaN(z))
            {
                bomb.transform.SetZLocalPosition(z);
            }

            if (bomb != null)
            {
                _lastFiredBombs.Add(bomb);
            }
            bombIndex++;
        }

        foreach (var fireSound in fireSounds)
        {
            if (fireSound != null)
            {
                WeaverAudio.PlayAtPoint(fireSound, sourcePos);
            }
        }
    }

    Bomb FireShot(float playerAngle, float angle, Vector3 sourcePos, float velocity, float size)
    {
        if (!Boss.RiseFromCenterPlatform)
        {
            //var instance = Pooling.Instantiate(ShotPrefab, sourcePos, Quaternion.identity);
            var bomb = Bomb.Spawn(bombPrefab, sourcePos, MathUtilities.PolarToCartesian(playerAngle + angle, velocity), angularVelocityRange.RandomInRange());

            if (!float.IsNaN(size))
            {
                bomb.transform.SetLocalScaleXY(size, size);
            }
            return bomb;
            /*if (instance.TryGetComponent(out Rigidbody2D rb))
            {
                rb.velocity = MathUtilities.PolarToCartesian(playerAngle + angle, velocity);
            }*/
            /*if (instance.TryGetComponent(out AspidShotBase aspidShot))
            {
                aspidShot.ScaleFactor = shotScale;
            }
            else
            {
                instance.transform.SetLocalScaleXY(shotScale, shotScale);
            }*/
        }
        return null;
    }

    public override float PostDelay => postDelay;

    public override void OnStun()
    {
        fader.Fade(false);
        bombParticles.Stop();
        Boss.Head.Animator.StopCurrentAnimation();
        CustomController = null;
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }
    }
}
