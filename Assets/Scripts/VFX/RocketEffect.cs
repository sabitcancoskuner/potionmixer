using System.Collections;
using UnityEngine;

public class RocketEffect : MonoBehaviour
{
    private float duration = 3f;
    private Vector2 direction;

    private void OnEnable()
    {
        // Start the smoke effect
        StartCoroutine(PlayEffect());
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;
    }

    public void SetOrientation(Vector2 orientation)
    {
        // Adjust the rotation based on orientation
        if (orientation.x != 0) // Horizontal
        {
            if (orientation.x > 0)
                transform.rotation = Quaternion.Euler(0, 0, 90);
            else
                transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (orientation.y != 0) // Vertical
        {
            if (orientation.y > 0)
                transform.rotation = Quaternion.Euler(0, 0, 180);
            else
                transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public IEnumerator PlayEffect()
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            transform.position = Vector2.MoveTowards(transform.position, transform.position + (Vector3)direction * 10f, Time.deltaTime * 8f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Stop the smoke effect after 3 seconds
        gameObject.SetActive(false);
    }
}
