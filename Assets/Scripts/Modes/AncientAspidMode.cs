using MonoMod.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public abstract class AncientAspidMode : MonoBehaviour
{
    private Dictionary<string, object> _defaultArgs;

    public Dictionary<string, object> DefaultArgs
    {
        get
        {
            _defaultArgs ??= new Dictionary<string, object>();
            return _defaultArgs;
        }
        set => _defaultArgs = value;
    }

    private AncientAspid _boss;
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
        Dictionary<string, object> args = GetArgs(customArgs);

        return OnExecute(args);
    }

    protected abstract IEnumerator OnExecute(Dictionary<string, object> customArgs);

    private Dictionary<string, object> GetArgs(Dictionary<string, object> customArgs)
    {
        if (customArgs == null)
        {
            return new Dictionary<string, object>(DefaultArgs);
        }
        else
        {
            Dictionary<string, object> args = new Dictionary<string, object>(DefaultArgs);
            foreach (KeyValuePair<string, object> pair in customArgs)
            {
                args[pair.Key] = pair.Value;
            }

            return args;
        }
    }

    private void GetArgs(Dictionary<string, object> customArgs, Dictionary<string, object> dest)
    {
        dest.Clear();
        dest.AddRange(DefaultArgs);
        if (customArgs != null)
        {
            foreach (KeyValuePair<string, object> pair in customArgs)
            {
                dest[pair.Key] = pair.Value;
            }
        }
    }

    public IEnumerator RunAspidMove(AncientAspidMove move, Dictionary<string, object> args)
    {
        GetArgs(args, move.Arguments);
        CurrentMove = move;

        /*WeaverLog.Log("Running Move = " + StringUtilities.Prettify(move.GetType().Name));

        foreach (KeyValuePair<string, object> arg in move.Arguments)
        {
            WeaverLog.Log($"MOVE ARG {arg.Key} = {arg.Value}");
        }*/

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

    public virtual void OnDeath()
    {

    }
}