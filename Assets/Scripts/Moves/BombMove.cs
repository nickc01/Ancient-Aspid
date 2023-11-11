using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class BombMove : AncientAspidMove
{
    public const string CUSTOM_BOMB_CONTROLLER = nameof(CUSTOM_BOMB_CONTROLLER);

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

    public override bool MoveEnabled
    {
        get
        {
            var enabled = Boss.CanSeeTarget &&
        Boss.CurrentRunningMode == Boss.TacticalMode &&
        Vector3.Distance(Player.Player1.transform.position, transform.position) <= 25f;

            enabled = enabled && ((Boss.PlayerRightOfBoss && Boss.Orientation == AspidOrientation.Right) || (!Boss.PlayerRightOfBoss && Boss.Orientation == AspidOrientation.Left) || Boss.Orientation == AspidOrientation.Center);



            return enabled;
        }
    }

    Vector3 baseFaderPos;

    List<Bomb> _lastFiredBombs = new List<Bomb>();

    public IEnumerable<Bomb> LastFiredBombs => _lastFiredBombs;

    /*/// <summary>
    /// If specified, this bomb controller will get executed when the move gets executed. Otherwise, the default bomb controller is used
    /// </summary>
    public IBombController CustomController { get; set; } = null;*/

    //public override bool Interruptible => true;

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

    protected override IEnumerator OnExecute()
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

        Arguments.TryGetValueOfType(CUSTOM_BOMB_CONTROLLER, out IBombController customController);


        yield return FireBombs(customController ?? new DefaultBombController(shotSpeed, gravityScale));

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
        if (!bombController.DoBombs(Boss))
        {
            yield break;
        }
        Cancelled = false;
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
        if (bombController.DoBombs(Boss))
        {
            Fire(bombController);
        }

        fader.Fade(false);
        bombParticles.Stop();

        yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Attack");

        Boss.Head.MainRenderer.Restore(oldState);
    }

    void Fire(IBombController bombController)
    {
        _lastFiredBombs.Clear();
        if (shotSpeed <= 0)
        {
            shotSpeed = 0.01f;
        }

        var sourcePos = Boss.Head.GetFireSource(Boss.Head.LookingDirection);

        Blood.SpawnBlood(sourcePos, new Blood.BloodSpawnInfo(3*2, 7*2, 10f, 25f, Boss.Head.LookingDirection - 90f - 40f, Boss.Head.LookingDirection - 90f + 40f, null));

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
        var bomb = Bomb.Spawn(bombPrefab, sourcePos, MathUtilities.PolarToCartesian(playerAngle + angle, velocity), angularVelocityRange.RandomInRange());

        if (!float.IsNaN(size))
        {
            bomb.transform.SetLocalScaleXY(size, size);
        }
        return bomb;
        return null;
    }

    public override float GetPostDelay(int prevHealth) => postDelay;


    /*protected override IEnumerator OnCancelRoutine()
    {
        OnStun();
        yield break;
    }*/

    public override void OnStun()
    {
        fader.Fade(false);
        bombParticles.Stop();
        Boss.Head.Animator.StopCurrentAnimation();
        //CustomController = null;
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }
    }

    /*public override void GracefullyStop()
    {
        
    }*/
}
