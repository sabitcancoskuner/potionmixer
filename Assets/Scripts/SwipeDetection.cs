using UnityEngine;

public class SwipeDetection : MonoBehaviour
{
    private TouchManager touchManager;

    [SerializeField] private float minSwipeDistance = 0.5f;
    [SerializeField] private float maxSwipeTime = 1.0f;
    [Range(0f, 1f)]
    [SerializeField] private float directionThreshold = 0.7f;

    private Vector2 startPosition;
    private float startTime;
    private Vector2 endPosition;
    private float endTime;

    private void Awake()
    {
        touchManager = TouchManager.Instance;
    }

    private void OnEnable()
    {
        touchManager.OnStartTouch += SwipeStart;
        touchManager.OnEndTouch += SwipeEnd;
    }

    private void OnDisable()
    {
        touchManager.OnStartTouch -= SwipeStart;
        touchManager.OnEndTouch -= SwipeEnd;
    }
    
    private void SwipeStart(Vector2 position, float time)
    {
        startPosition = position;
        startTime = time;
    }

    private void SwipeEnd(Vector2 position, float time)
    {
        endPosition = position;
        endTime = time;
        Vector3 direction = endPosition - startPosition;
        Vector3 direction2D = new Vector2(direction.x, direction.y).normalized;
        SwipeDirection(direction2D);
    }

    private void SwipeDirection(Vector2 direction)
    {
        if (Vector2.Dot(Vector2.up, direction) > directionThreshold)
        {
            Debug.Log("Swipe Up");
        }
        else if (Vector2.Dot(Vector2.down, direction) > directionThreshold)
        {
            Debug.Log("Swipe Down");
        }
        else if (Vector2.Dot(Vector2.left, direction) > directionThreshold)
        {
            Debug.Log("Swipe Left");
        }
        else if (Vector2.Dot(Vector2.right, direction) > directionThreshold)
        {
            Debug.Log("Swipe Right");
        }

        Debug.DrawLine(startPosition, endPosition, Color.red, 3.0f);
    }


}
