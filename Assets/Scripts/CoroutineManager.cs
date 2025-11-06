using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : Singleton<CoroutineManager>
{
    private readonly Dictionary<object, List<Coroutine>> _trackedCoroutines = new Dictionary<object, List<Coroutine>>();

    public Coroutine StartTrackedCoroutine(IEnumerator routine, object key)
    {
        Coroutine coroutine = StartCoroutine(routine);
        if (!_trackedCoroutines.ContainsKey(key))
        {
            _trackedCoroutines[key] = new List<Coroutine>();
        }
        _trackedCoroutines[key].Add(coroutine);
        StartCoroutine(CleanupCoroutine(coroutine, key));
        return coroutine;
    }

    private IEnumerator CleanupCoroutine(Coroutine coroutine, object key)
    {
        yield return coroutine;
        if (_trackedCoroutines.ContainsKey(key))
        {
            _trackedCoroutines[key].Remove(coroutine);
            if (_trackedCoroutines[key].Count == 0)
            {
                _trackedCoroutines.Remove(key);
            }
        }
    }

    public bool IsTracking(object key)
    {
        return _trackedCoroutines.ContainsKey(key) && _trackedCoroutines[key].Count > 0;
    }

    public Coroutine WaitForAll(object key)
    {
        return StartCoroutine(WaitAll(key));
    }

    private IEnumerator WaitAll(object key)
    {
        while (IsTracking(key))
        {
            yield return null;
        }
    }

    public void StopAllTracked()
    {
        StopAllCoroutines();
        _trackedCoroutines.Clear();
    }
}
