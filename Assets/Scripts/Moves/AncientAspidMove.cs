﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Interfaces;

public abstract class AncientAspidMove : MonoBehaviour, IBossMove
{
    AncientAspid _boss;
    public AncientAspid Boss => _boss ??= GetComponent<AncientAspid>();

    public abstract bool MoveEnabled { get; }

    public abstract IEnumerator DoMove();

    public virtual void OnCancel()
    {
        OnStun();
    }

    public virtual void OnDeath()
    {
        OnDeath();
    }

    public abstract void OnStun();
}

