using System.Collections;
using UnityEngine;

public class MatchManager : Singleton<MatchManager>
{
    private MatchableGrid grid;

    [SerializeField] private Transform collectionPoint;
    private Matchable[] matchables;

    private void Start()
    {
        grid = (MatchableGrid)MatchableGrid.Instance;
        matchables = new Matchable[2];
    }

    public IEnumerator ResolveMatch(Match toResolve, MatchType matchType = MatchType.None)
    {
        Matchable matchable;
        Matchable powerupFormed = null;

        // Upgrade the matchable if needed
        if (toResolve.Count > 5)
        {
            toResolve.type = MatchType.Five;
        }

        if (toResolve.Type == MatchType.Four || toResolve.Type == MatchType.Five)
        {
            powerupFormed = UpgradeMatchable(toResolve.ToBeUpgraded, toResolve.PowerupType);
            toResolve.RemoveMatchable(powerupFormed);
        }

        // Calculate matches to resolve AFTER removing the powerup
        int matchesToResolve = Mathf.Min(toResolve.Count, 5);

        for (int i = 0; i < matchesToResolve; i++)
        {
            // Get the matchable to resolve
            matchable = toResolve.Matchables[i];

            // Remove the matchables from the grid
            if (!matchable.isObstacle)
            {
                grid.RemoveObjectAtPosition(matchable.gridPosition);
            }

            // Move the to the collection point
            if (i == matchesToResolve - 1)
            {
                yield return StartCoroutine(matchable.Resolve());
            }
            else
            {
                StartCoroutine(matchable.Resolve());
            }
        }
    }
    
    public Matchable UpgradeMatchable(Matchable matchable, PowerupType powerupType)
    {
        Matchable upgraded = null;

        if (powerupType != PowerupType.None)
        {
            upgraded = matchable;
            switch (powerupType)
            {
                case PowerupType.RocketHorizontal:
                    upgraded.Upgrade(powerupType);
                    break;
                case PowerupType.RocketVertical:
                    upgraded.Upgrade(powerupType);
                    break;
                case PowerupType.DiscoBall:
                    upgraded.Upgrade(powerupType);
                    break;
                case PowerupType.Bomb:
                    upgraded.Upgrade(powerupType);
                    break;
                default:
                    break;
            }
        }

        return upgraded;
    }

    public IEnumerator HandleObjectSwap(Vector2Int touchPosition, Vector2Int direction)
    {
        // Cancel any active hint when player makes a move
        HintIndicator.Instance.CancelHint();
        
        Vector2Int targetPosition = touchPosition + direction;
        // Debug.Log("Touch Position: " + touchPosition + " Target Position: " + targetPosition);

        if (grid.IsWithinBounds(touchPosition) && grid.IsWithinBounds(targetPosition))
        {
            Matchable firstMatchable = grid.GetObjectAtPosition(touchPosition);
            Matchable secondMatchable = grid.GetObjectAtPosition(targetPosition);
            matchables[0] = firstMatchable;
            matchables[1] = secondMatchable;

            if (matchables[0] != null && matchables[0].Idle && matchables[1] != null && matchables[1].Idle)
            {
                yield return StartCoroutine(grid.TrySwapping(matchables));
            }

            matchables[0] = null;
            matchables[1] = null;
        }
    }

    public IEnumerator HandleObjectTouch(Vector2Int touchPosition)
    {
        // Cancel any active hint when player makes a move
        HintIndicator.Instance.CancelHint();

        if (grid.isProcessing)
        {
            yield break;
        }

        if (grid.IsWithinBounds(touchPosition))
        {
            Matchable matchable = grid.GetObjectAtPosition(touchPosition);

            if (matchable != null && matchable.IsPowerUp && matchable.Idle)
            {
                // Remove from grid before activating
                grid.RemoveObjectAtPosition(matchable.gridPosition);
                
                // Activate the powerup directly
                yield return StartCoroutine(matchable.ActivatePowerup());
                
                // Return powerup to pool after activation
                MatchablePool.Instance.ReturnToPool(matchable);
            }
        }
    }
}
