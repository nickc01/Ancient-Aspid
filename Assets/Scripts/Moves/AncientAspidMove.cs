using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Interfaces;

public abstract class AncientAspidMove : MonoBehaviour, IBossMove
{
    public Dictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();

    [NonSerialized]
    private AncientAspid _boss;
    public AncientAspid Boss => _boss ??= GetComponent<AncientAspid>();

    public bool Cancelled { get; protected set; } = false;

    public abstract bool MoveEnabled { get; }

    public IEnumerator DoMove()
    {
        Cancelled = false;
        yield return OnExecute();
        Arguments.Clear();
        yield break;
    }

    public virtual float PreDelay => 0f;
    public virtual float GetPostDelay(int prevHealth)
    {
        return 0f;
    }

    protected abstract IEnumerator OnExecute();

    void IEnemyMove.OnCancel()
    {

    }

    public virtual void OnDeath()
    {
        OnStun();
    }

    public abstract void OnStun();

    public virtual void StopMove()
    {
        Cancelled = true;
    }
}

