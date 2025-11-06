using System.Collections;
using UnityEngine;

public class HintIndicator : Singleton<HintIndicator>
{
    [SerializeField] private float delayBeforeAutoHint = 5f;
    Vector2Int hintLocation;

    private Coroutine autoHintCoroutine;
    private Matchable currentHintedMatchable;

    public void IndicateHint(Matchable matchableToHint)
    {
        CancelHint();
        currentHintedMatchable = matchableToHint;
        matchableToHint.StartHintAnimation();
    }

    public void CancelHint()
    {
        if (autoHintCoroutine != null)
        {
            StopCoroutine(autoHintCoroutine);
            autoHintCoroutine = null;
        }

        if (currentHintedMatchable != null)
        {
            currentHintedMatchable.StopHintAnimation();
            currentHintedMatchable = null;
        }
    }

    public void StartAutoHint(Matchable matchableToHint)
    {
        // Cancel any existing hint or waiting hint before starting a new one
        CancelHint();
        autoHintCoroutine = StartCoroutine(WaitAndIndicateHint(matchableToHint));
    }
    
    private IEnumerator WaitAndIndicateHint(Matchable matchableToHint)
    {
        yield return new WaitForSeconds(delayBeforeAutoHint);
        IndicateHint(matchableToHint);
    }
}
