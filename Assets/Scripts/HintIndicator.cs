using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintIndicator : Singleton<HintIndicator>
{
    [SerializeField] private float delayBeforeAutoHint = 5f;
    Vector2Int hintLocation;

    private Coroutine autoHintCoroutine;
    private List<Matchable> currentHintedMatchables = new List<Matchable>();

    public void IndicateHint(List<Matchable> matchablesToHint)
    {
        CancelHint();
        currentHintedMatchables = new List<Matchable>(matchablesToHint);
        foreach (Matchable matchable in matchablesToHint)
        {
            matchable.StartHintAnimation();
        }
    }

    public void CancelHint()
    {
        if (autoHintCoroutine != null)
        {
            StopCoroutine(autoHintCoroutine);
            autoHintCoroutine = null;
        }

        if (currentHintedMatchables != null && currentHintedMatchables.Count > 0)
        {
            foreach (Matchable matchable in currentHintedMatchables)
            {
                if (matchable != null && matchable.gameObject != null)
                {
                    matchable.StopHintAnimation();
                }
                else
                {
                    Debug.LogWarning($"Matchable or its gameObject is null in CancelHint");
                }
            }
            currentHintedMatchables.Clear();
        }
    }

    public void StartAutoHint(List<Matchable> matchablesToHint)
    {
        // Cancel any existing hint or waiting hint before starting a new one
        CancelHint();
        autoHintCoroutine = StartCoroutine(WaitAndIndicateHint(matchablesToHint));
    }
    
    private IEnumerator WaitAndIndicateHint(List<Matchable> matchablesToHint)
    {
        yield return new WaitForSeconds(delayBeforeAutoHint);
        IndicateHint(matchablesToHint);
    }
}
