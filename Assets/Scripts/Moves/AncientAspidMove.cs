using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Interfaces;

public abstract class AncientAspidMove : MonoBehaviour, IBossMove
{
    /// <summary>
    /// Contains any custom arguments passed to the move to customize it's execution. Gets wiped after the move fully executes
    /// </summary>
    public Dictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();

    AncientAspid _boss;
    public AncientAspid Boss => _boss ??= GetComponent<AncientAspid>();

    /// <summary>
    /// Is the move currently being signalled to stop prematurely?
    /// </summary>
    public bool Cancelled { get; protected set; } = false;

    /// <summary>
    /// Is the move able to be executed when randomly selected?
    /// </summary>
    public abstract bool MoveEnabled { get; }

    public IEnumerator DoMove()
    {
        Cancelled = false;
        yield return OnExecute();
        Arguments.Clear();
        //Cancelling = false;
        yield break;
    }

    public virtual float PreDelay => 0f;
    public virtual float GetPostDelay(int prevHealth) => 0f;

    protected abstract IEnumerator OnExecute();

    /*public void OnCancel()
    {
        if (!Interruptible)
        {
            throw new Exception("This move is not mean't to be cancelled prematurely");
        }
        Cancelling = true;

        IEnumerator RunCancelRoutine()
        {
            yield return OnCancelRoutine();
            Cancelling = false;
        }

        Boss.StartBoundRoutine(RunCancelRoutine(),() => Cancelling = false);
    }*/

    void IEnemyMove.OnCancel()
    {
        
    }

    public virtual void OnDeath()
    {
        OnStun();
    }

    public abstract void OnStun();

    /*protected sealed IEnumerator OnCancelRoutine()
    {
        yield break;
    }*/

    /// <summary>
    /// Signals to the move to stop prematurely
    /// </summary>
    public virtual void StopMove()
    {
        Cancelled = true;
    }
}

