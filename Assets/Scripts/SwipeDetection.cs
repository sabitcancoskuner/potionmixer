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
        Vector2Int swipeDirection = GetSwipeDirection(direction2D);
        HandleSwipe(swipeDirection);
    }

    private Vector2Int GetSwipeDirection(Vector2 direction)
    {
        if (Vector2.Dot(Vector2.up, direction) > directionThreshold)
        {
            // Debug.Log("Swipe Up");
            return Vector2Int.up;
        }
        else if (Vector2.Dot(Vector2.down, direction) > directionThreshold)
        {
            // Debug.Log("Swipe Down");
            return Vector2Int.down;
        }
        else if (Vector2.Dot(Vector2.left, direction) > directionThreshold)
        {
            // Debug.Log("Swipe Left");
            return Vector2Int.left;
        }
        else if (Vector2.Dot(Vector2.right, direction) > directionThreshold)
        {
            // Debug.Log("Swipe Right");
            return Vector2Int.right;
        }

        return Vector2Int.zero; // no swipe detected

    }

    private void HandleSwipe(Vector2Int direction)
    {
        if (Vector3.Distance(startPosition, endPosition) >= minSwipeDistance && (endTime - startTime) <= maxSwipeTime)
        {
            // Swipe detected in the 'direction'
            // Implement your logic here based on the swipe direction
            Vector2Int touchPosition = new Vector2Int(4, 4);
            touchPosition.x += CustomRound(startPosition.x);
            touchPosition.y += CustomRound(startPosition.y);
            StartCoroutine(MatchManager.Instance.HandleObjectSwap(touchPosition, direction));
        }
        // Not swiped but touched at that position
        else
        {
            Vector2Int touchPosition = new Vector2Int(4, 4);
            touchPosition.x += CustomRound(startPosition.x);
            touchPosition.y += CustomRound(startPosition.y);
            StartCoroutine(MatchManager.Instance.HandleObjectTouch(touchPosition));
        }
    }

    private int CustomRound(float value)
    {
        if (value >= 0)
        {
            // For positive numbers: round down if decimal < 0.5, round up if >= 0.5
            return Mathf.FloorToInt(value + 0.5f);
        }
        else
        {
            // For negative numbers: round up (toward zero) if decimal < 0.5, round down (away from zero) if >= 0.5
            return Mathf.CeilToInt(value - 0.5f);
        }
    }


}
