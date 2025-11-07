using System.Collections;
using PrimeTween;
using UnityEngine;

public class ResolveEffect : MonoBehaviour
{
    public float duration = 1f;

    private void OnEnable()
    {
        // Start the smoke effect
        StartCoroutine(PlayEffect());
    }

    public void SetDuration(float newDuration)
    {
        duration = newDuration;
    }

    private IEnumerator PlayEffect()
    {
        // Play the smoke effect for the specified duration
        yield return new WaitForSeconds(duration);

        // Stop the smoke effect
        gameObject.SetActive(false);
    }
}
