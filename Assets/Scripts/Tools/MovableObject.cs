using System.Collections;
using UnityEngine;

public class MovableObject : MonoBehaviour
{
    private Vector3 from, to;
    private float howFar;
    [SerializeField] private float speed = 1f;

    protected bool idle = true;

    public bool Idle
    {
        get { return idle; }
    }

    public IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        if (speed <= 0f)
        {
            Debug.LogError("Speed must be greater than zero.");
            yield break;
        }

        from = transform.position;
        to = targetPosition;
        howFar = 0f;
        idle = false;

        while (howFar < 1f)
        {
            howFar += Time.deltaTime * speed;
            transform.position = Vector3.Lerp(from, to, Easing(howFar));
            yield return null;
        }

        idle = true;
    }

    public IEnumerator MoveToTransform(Transform targetTransform)
    {
        if (speed <= 0f)
        {
            Debug.LogError("Speed must be greater than zero.");
            yield break;
        }

        from = transform.position;
        to = targetTransform.position;
        howFar = 0f;
        idle = false;

        while (howFar < 1f)
        {
            howFar += Time.deltaTime * speed;
            transform.position = Vector3.Lerp(from, to, Easing(howFar));

            to = targetTransform.position;
            yield return null;
        }

        idle = true;
    }

    private float Easing(float x)
    {
        return x * x * (3f - 2f * x);
    }
}
