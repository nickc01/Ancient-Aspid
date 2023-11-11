using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore;
using System.Collections.Generic;
using WeaverCore.Enums;

public class GroundJumpMove : AncientAspidMove
{
    public const string JUMP_TIMES = nameof(JUMP_TIMES);

    static RaycastHit2D[] rayCache = new RaycastHit2D[4];

    public float jumpTime = 0.5f;

    public float jumpGravity = 1f;

    public List<AudioClip> jumpSounds;

    public AudioClip jumpLandSound;

    public ShakeType jumpLandShakeType;

    public GameObject jumpLaunchEffectPrefab;

    public ShakeType jumpLaunchShakeType;

    public ParticleSystem jumpLandParticles;

    public override bool MoveEnabled => false;

    //public int JumpTimes { get; set; } = 0;

    //public bool Cancelled { get; private set; } = false;

    //public override bool Interruptible => false;

    Vector2 JumpTargeter(int time)
    {
        if (time % 2 == 0)
        {
            var rb = Player.Player1.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                return Player.Player1.transform.position + new Vector3(rb.velocity.x * jumpTime + 0.1f, 0f);
            }
            else
            {
                return Player.Player1.transform.position;
            }
        }
        else
        {
            var minX = Boss.CurrentRoomRect.xMin + 6f;
            var maxX = Boss.CurrentRoomRect.xMax - 6f;

            var randValue = UnityEngine.Random.Range(minX, maxX);

            randValue = Mathf.Clamp(randValue, transform.position.x - 10f, transform.position.x + 10f);

            return new Vector2(randValue, transform.position.y);
        }
    }

    protected override IEnumerator OnExecute()
    {
        if (!Arguments.TryGetValueOfType(JUMP_TIMES, out float jumpTimes) || jumpTimes <= 0)
        {
            throw new System.Exception("The JumpTimes variable was not set before running the Ground Jump Move");
        }

        //var jumpTimes = JumpTimes;
        //JumpTimes = 0;

        //Cancelled = false;

        yield return JumpPrepare();

        for (int i = 0; i < jumpTimes; i++)
        {
            yield return JumpLaunch();

            foreach (var sound in jumpSounds)
            {
                WeaverAudio.PlayAtPoint(sound, transform.position);
            }

            CameraShaker.Instance.Shake(jumpLaunchShakeType);

            //if (jumpLaunchEffectPrefab != null)
            //{
                //Pooling.Instantiate(jumpLaunchEffectPrefab, transform.position + jumpLaunchEffectPrefab.transform.localPosition, jumpLaunchEffectPrefab.transform.localRotation);
            //}

            var target = JumpTargeter(i);

            if (target.y < transform.position.y + 2f && target.y > transform.position.y - 2)
            {
                target.y = transform.position.y;
            }

            var groundHits = Physics2D.RaycastNonAlloc(target, Vector2.down, rayCache, 10, LayerMask.NameToLayer("Terrain"));

            /*if (groundHits == 0)
            {
                if (Boss.CurrentRoomRect.BottomHit.collider != null)
                {
                    var colliderBounds = Boss.CurrentRoomRect.BottomHit.collider.bounds;

                    target.x = Mathf.Clamp(target.x, colliderBounds.min.x + 5, colliderBounds.max.x - 5);
                }
            }*/

            if (groundHits > 0)
            {
                var bottomCollider = rayCache[0].collider;

                if (bottomCollider != null)
                {
                    var colliderBounds = bottomCollider.bounds;

                    target.x = Mathf.Clamp(target.x, colliderBounds.min.x + 5, colliderBounds.max.x - 5);
                }
            }

            var velocity = MathUtilities.CalculateVelocityToReachPoint(transform.position, target, jumpTime, jumpGravity);

            Boss.Rbody.gravityScale = jumpGravity;
            Boss.Rbody.velocity = velocity;

            Boss.Claws.OnGround = false;

            bool switchDirection = false;
            if (Boss.Orientation == AspidOrientation.Right && Player.Player1.transform.position.x < target.x)
            {
                switchDirection = true;
            }

            if (Boss.Orientation == AspidOrientation.Left && Player.Player1.transform.position.x >= target.x)
            {
                switchDirection = true;
            }

            if (switchDirection)
            {
                yield return JumpSwitchDirectionPrepare();
                yield return JumpSwitchDirection();
                if (Boss.Orientation == AspidOrientation.Left)
                {
                    Boss.Orientation = AspidOrientation.Right;
                }
                else
                {
                    Boss.Orientation = AspidOrientation.Left;
                }
            }

            yield return new WaitUntil(() => Boss.Rbody.velocity.y <= 0f);
            var fallingAwaiter = JumpBeginFalling(switchDirection);
            yield return new WaitUntil(() => Boss.Rbody.velocity.y <= -0.5f);
            //yield return new WaitForSeconds(Time.fixedDeltaTime * 2f);
            //yield return new WaitForSeconds(jumpTime / 5f);

            bool cancel = true;

            for (float t = 0; t < 1.5f; t += Time.deltaTime)
            {
                if (Boss.Rbody.velocity.y >= -0.5f)
                {
                    cancel = false;
                    break;
                }
                yield return null;
            }

            if (cancel)
            {
                Cancelled = true;
                //onCancel?.Invoke();
                yield break;
            }
            //yield return new WaitUntil(() => Boss.Rbody.velocity.y >= 0f);

            yield return fallingAwaiter.WaitTillDone();

            Boss.Rbody.velocity = Boss.Rbody.velocity.With(x: 0f);

            Boss.Claws.OnGround = true;

            if (jumpLandSound != null)
            {
                WeaverAudio.PlayAtPoint(jumpLandSound, transform.position);
            }

            CameraShaker.Instance.Shake(jumpLandShakeType);
            jumpLandParticles.Play();

            yield return JumpLand(i == jumpTimes - 1);
        }

        Boss.Rbody.velocity = default;
        Boss.Rbody.gravityScale = 0f;

        /*if (Boss.CanSeeTarget)
        {
            yield return DoGroundJump(2, JumpTargeter, onCancel);
        }*/
    }

    RoutineAwaiter JumpBeginFalling(bool switchedDirection)
    {
        var clawsRoutine = Boss.Claws.GroundJumpBeginFalling(switchedDirection);
        var headRoutine = Boss.Head.GroundJumpBeginFalling(switchedDirection);
        var wingsRoutine = Boss.Wings.GroundJumpBeginFalling(switchedDirection);
        var wingPlatesRoutine = Boss.WingPlates.GroundJumpBeginFalling(switchedDirection);
        var bodyRoutine = Boss.Body.GroundJumpBeginFalling(switchedDirection);
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);
        return awaiter;
    }

    IEnumerator JumpSwitchDirection()
    {
        var oldOrientation = Boss.Orientation;
        AspidOrientation newOrientation;
        if (oldOrientation == AspidOrientation.Left)
        {
            newOrientation = AspidOrientation.Right;
        }
        else
        {
            newOrientation = AspidOrientation.Left;
        }

        var clawsRoutine = Boss.Claws.MidJumpChangeDirection(oldOrientation, newOrientation);
        var headRoutine = Boss.Head.MidJumpChangeDirection(oldOrientation, newOrientation);
        var wingsRoutine = Boss.Wings.MidJumpChangeDirection(oldOrientation, newOrientation);
        var wingPlatesRoutine = Boss.WingPlates.MidJumpChangeDirection(oldOrientation, newOrientation);
        var bodyRoutine = Boss.Body.MidJumpChangeDirection(oldOrientation, newOrientation);
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);
        yield return awaiter.WaitTillDone();
    }

    IEnumerator JumpSwitchDirectionPrepare()
    {
        var clawsRoutine = Boss.Claws.WaitTillChangeDirectionMidJump();
        var headRoutine = Boss.Head.WaitTillChangeDirectionMidJump();
        var wingsRoutine = Boss.Wings.WaitTillChangeDirectionMidJump();
        var wingPlatesRoutine = Boss.WingPlates.WaitTillChangeDirectionMidJump();
        var bodyRoutine = Boss.Body.WaitTillChangeDirectionMidJump();
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);
        yield return awaiter.WaitTillDone();
    }

    IEnumerator JumpPrepare()
    {
        var clawsRoutine = Boss.Claws.GroundPrepareJump();
        var headRoutine = Boss.Head.GroundPrepareJump();
        var wingsRoutine = Boss.Wings.GroundPrepareJump();
        var wingPlatesRoutine = Boss.WingPlates.GroundPrepareJump();
        var bodyRoutine = Boss.Body.GroundPrepareJump();
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        yield break;
    }

    IEnumerator JumpLaunch()
    {
        var clawsRoutine = Boss.Claws.GroundLaunch();
        var headRoutine = Boss.Head.GroundLaunch();
        var wingsRoutine = Boss.Wings.GroundLaunch();
        var wingPlatesRoutine = Boss.WingPlates.GroundLaunch();
        var bodyRoutine = Boss.Body.GroundLaunch();
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        yield break;
    }

    IEnumerator JumpLand(bool finalLanding)
    {
        var clawsRoutine = Boss.Claws.GroundLand(finalLanding);
        var headRoutine = Boss.Head.GroundLand(finalLanding);
        var wingsRoutine = Boss.Wings.GroundLand(finalLanding);
        var wingPlatesRoutine = Boss.WingPlates.GroundLand(finalLanding);
        var bodyRoutine = Boss.Body.GroundLand(finalLanding);
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        yield break;
    }

    public override void OnStun()
    {
        
    }
}