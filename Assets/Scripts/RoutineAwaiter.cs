using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.Features;

public class RoutineAwaiter
{
    class AlternateRoutineSource : MonoBehaviour
    {
        static AlternateRoutineSource _instance;
        public static AlternateRoutineSource Instance => _instance ??= Create();

        static AlternateRoutineSource Create()
        {
            var newGM = new GameObject("ALTERNATE ROUTINE SOURCE");
            newGM.hideFlags = HideFlags.HideAndDontSave;
            return newGM.AddComponent<AlternateRoutineSource>();
        }
    }

    bool[] completedTasks;

    public bool Done { get; private set; }

    public event Action OnDone;

    RoutineAwaiter()
    {

    }

    public IEnumerator WaitTillDone()
    {
        yield return new WaitUntil(() => Done);
    }

    public static RoutineAwaiter AwaitRoutine(IEnumerator routine, MonoBehaviour source = null)
    {
        if (source == null)
        {
            source = AlternateRoutineSource.Instance;
        }
        var awaiter = new RoutineAwaiter();
        awaiter.completedTasks = new bool[1];
        source.StartCoroutine(awaiter.RoutineRunner(routine, 0));
        return awaiter;
    }

    public static RoutineAwaiter AwaitRoutines(IEnumerable<IEnumerator> routines, MonoBehaviour source = null)
    {
        return AwaitRoutines(source, routines.ToArray());
        /*int count = routines.Count();
        if (source == null)
        {
            source = AlternateRoutineSource.Instance;
        }
        var awaiter = new RoutineAwaiter();
        awaiter.completedTasks = new bool[count];
        int index = 0;
        foreach (var routine in routines)
        {
            source.StartCoroutine(awaiter.RoutineRunner(routine, index++));
        }
        return awaiter;*/
    }

    public static RoutineAwaiter AwaitRoutines(MonoBehaviour source = null, params IEnumerator[] routines)
    {
        return AwaitRoutines(source, null, routines);
        /*int count = routines.Count();
        if (source == null)
        {
            source = AlternateRoutineSource.Instance;
        }
        var awaiter = new RoutineAwaiter();
        awaiter.completedTasks = new bool[count];
        int index = 0;
        foreach (var routine in routines)
        {
            source.StartCoroutine(awaiter.RoutineRunner(routine, index++));
        }
        return awaiter;*/
    }

    public static RoutineAwaiter AwaitRoutines(MonoBehaviour source = null, List<Coroutine> outputRoutines = null, params IEnumerator[] routines)
    {
        outputRoutines?.Clear();
        int count = routines.Count();
        if (source == null)
        {
            source = AlternateRoutineSource.Instance;
        }
        var awaiter = new RoutineAwaiter();
        awaiter.completedTasks = new bool[count];
        int index = 0;
        foreach (var routine in routines)
        {
            var startedRoutine = source.StartCoroutine(awaiter.RoutineRunner(routine, index++));
            outputRoutines?.Add(startedRoutine);
        }
        return awaiter;
    }

    public static RoutineAwaiter AwaitBoundRoutines(Enemy source, params IEnumerator[] routines)
    {
        return AwaitBoundRoutines(source, null, routines);
    }

    public static RoutineAwaiter AwaitBoundRoutines(Enemy source, List<uint> outputRoutineIDs, params IEnumerator[] routines)
    {
        outputRoutineIDs?.Clear();
        int count = routines.Count();
        var awaiter = new RoutineAwaiter();
        awaiter.completedTasks = new bool[count];
        int index = 0;
        foreach (var routine in routines)
        {
            var id = source.StartBoundRoutine(awaiter.RoutineRunner(routine, index++));
            outputRoutineIDs?.Add(id);
        }
        return awaiter;
    }

    public static RoutineAwaiter AwaitBoundRoutines(IEnumerable<IEnumerator> routines, Enemy source)
    {
        return AwaitBoundRoutines(source, null, routines.ToArray());
        /*int count = routines.Count();
        var awaiter = new RoutineAwaiter();
        awaiter.completedTasks = new bool[count];
        int index = 0;
        foreach (var routine in routines)
        {
            source.StartBoundRoutine(awaiter.RoutineRunner(routine, index++));
        }
        return awaiter;*/
    }

    IEnumerator RoutineRunner(IEnumerator routine, int completionIndex)
    {
        try
        {
            yield return routine;
        }
        finally
        {
            completedTasks[completionIndex] = true;
            CheckCompletion();
        }
    }

    void CheckCompletion()
    {
        if (!Done && completedTasks.All(b => b == true))
        {
            Done = true;
            OnDone?.Invoke();
        }
    }
}
