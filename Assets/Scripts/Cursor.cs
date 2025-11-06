using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    private MatchableGrid matchableGrid;
    private MatchablePool matchablePool;
    
    private Vector3 worldPosition;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        matchableGrid = (MatchableGrid)MatchableGrid.Instance;
        matchablePool = (MatchablePool)MatchablePool.Instance;
    }

    void Update()
    {
        // Always update cursor world position
        Vector3 screenPosition = Input.mousePosition;
        worldPosition = Utils.ScreenToWorldPoint(mainCamera, screenPosition);
        
        // Update cursor transform position
        transform.position = worldPosition;

        // Check if pressing 1
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            ChangeObjectAtCursor();
        }
    }

    private void ChangeObjectAtCursor()
    {
        // Convert world position to grid position
        Vector2Int gridPosition = WorldToGridPosition(worldPosition);

        // Check if position is valid in grid
        if (matchableGrid.IsWithinBounds(gridPosition) && !matchableGrid.IsPositionEmpty(gridPosition))
        {
            // Get the current matchable at this position
            Matchable currentMatchable = matchableGrid.GetObjectAtPosition(gridPosition);
            
            if (currentMatchable != null && currentMatchable.Idle)
            {
                // Change to next type
                matchablePool.NextType(currentMatchable);
            }
        }
    }

    private Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        // Assuming grid origin is at matchableGrid.transform.position
        Vector3 gridOrigin = matchableGrid.transform.position;
        Vector3 localPos = worldPos - gridOrigin;
        
        // Round to nearest grid cell
        int x = Mathf.RoundToInt(localPos.x);
        int y = Mathf.RoundToInt(localPos.y);
        
        return new Vector2Int(x, y);
    }
}
