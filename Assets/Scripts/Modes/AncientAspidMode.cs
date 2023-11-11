using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public abstract class AncientAspidMode : MonoBehaviour
{
    Dictionary<string, object> _defaultArgs;

    public Dictionary<string, object> DefaultArgs
    {
        get
        {
            if (_defaultArgs == null)
            {
                _defaultArgs = new Dictionary<string, object>();
            }
            return _defaultArgs;
        }
        set
        {
            _defaultArgs = value;
        }
    }


    AncientAspid _boss;
    public AncientAspid Boss => _boss ??= GetComponent<AncientAspid>();

    public AncientAspidMove CurrentMove { get; private set; } = null;

    public bool IsModeEnabled(Dictionary<string, object> args = null)
    {
        return ModeEnabled(GetArgs(args));
    }

    protected abstract bool ModeEnabled(Dictionary<string, object> args);

    public IEnumerator Execute()
    {
        return OnExecute(DefaultArgs);
    }

    public IEnumerator Execute(Dictionary<string, object> customArgs)
    {
        /*Debug.Log("_-_Mode222_-_ = " + GetType().Name);
        Debug.Log("Args222");

        foreach (var arg in DefaultArgs)
        {
            Debug.Log($"{arg.Key} = {arg.Value?.GetType().Name ?? "null"}-{arg.Value?.ToString() ?? ""}");
        }

        var args = GetArgs(customArgs);

        Debug.Log("_-_Mode_-_ = " + GetType().Name);
        Debug.Log("Args");

        foreach (var arg in args)
        {
            Debug.Log($"{arg.Key} = {arg.Value?.GetType().Name ?? "null"}-{arg.Value?.ToString() ?? ""}");
        }*/

        var args = GetArgs(customArgs);

        return OnExecute(args);
    }

    protected abstract IEnumerator OnExecute(Dictionary<string, object> customArgs);

    Dictionary<string, object> GetArgs(Dictionary<string, object> customArgs)
    {
        if (customArgs == null)
        {
            return new Dictionary<string, object>(DefaultArgs);
        }
        else
        {
            var args = new Dictionary<string, object>(DefaultArgs);
            foreach (var pair in customArgs)
            {
                args[pair.Key] = pair.Value;
            }

            return args;
        }
    }

    void GetArgs(Dictionary<string, object> customArgs, Dictionary<string, object> dest)
    {
        dest.Clear();
        dest.AddRange(DefaultArgs);
        if (customArgs != null)
        {
            foreach (var pair in customArgs)
            {
                dest[pair.Key] = pair.Value;
            }
        }
    }

    public IEnumerator RunAspidMove(AncientAspidMove move, Dictionary<string, object> args)
    {
        //move.Arguments = GetArgs(customArgs);
        GetArgs(args, move.Arguments);
        CurrentMove = move;

        WeaverLog.Log("Running Move = " + StringUtilities.Prettify(move.GetType().Name));

        foreach (var arg in move.Arguments)
        {
            WeaverLog.Log($"MOVE ARG {arg.Key} = {arg.Value}");
        }

        yield return Boss.RunMove(move);
        CurrentMove = null;
    }

    public abstract void Stop();

    public void StopCurrentMove()
    {
        if (CurrentMove != null)
        {
            CurrentMove.StopMove();
        }
    }

    /*protected IEnumerator RunMoveWhile(AncientAspidMove move, Func<bool> predicate)
    {
        if (move.Interruptible)
        {
            yield return Boss.RunMoveWhile(move, predicate);
            if (move.Cancelling)
            {
                yield return new WaitUntil(() => !move.Cancelling);
            }
        }
        else
        {
            yield return Boss.RunMove(move);
        }
    }*/

    public virtual void OnDeath()
    {

    }
}