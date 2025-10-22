using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchableGrid : GridSystem<Matchable>
{
    [SerializeField] private Vector3 offScreenSpawnOffset;

    private MatchablePool matchablePool;

    private void Start()
    {
        matchablePool = (MatchablePool)MatchablePool.Instance;
    }

    public IEnumerator PopulateGrid(bool allowMatches = false, bool initialPopulation = false)
    {
        List<Matchable> newMatchables = new List<Matchable>();
        Matchable newMatchable;
        Vector3 onScreenPosition;

        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                if (IsPositionEmpty(x, y))
                {
                    newMatchable = matchablePool.GetRandomMatchable();
                    onScreenPosition = transform.position + new Vector3(x, y);
                    newMatchable.transform.position = onScreenPosition; 

                    newMatchable.gameObject.SetActive(true);

                    newMatchable.gridPosition = new Vector2Int(x, y);

                    SetObjectAtPosition(newMatchable, x, y);

                    newMatchables.Add(newMatchable);

                    int type = newMatchable.Type;

                    while (!allowMatches && HasMatchAtPosition(x, y, type))
                    {
                        if (matchablePool.NextType(newMatchable) == type)
                        {
                            Debug.LogWarning("All matchable types result in a match. Cannot avoid matches.");
                            Debug.Break();
                            break;
                        }
                    }
                }
            }
        }

        yield return null;
    }

    private bool HasMatchAtPosition(int x, int y, int type)
    {
        // Check horizontal
        int horizontalMatchCount = 0;
        horizontalMatchCount += CountMatchesInDirection(GetObjecAtPosition(x, y), Vector2Int.left);
        horizontalMatchCount += CountMatchesInDirection(GetObjecAtPosition(x, y), Vector2Int.right);

        if (horizontalMatchCount >= 2)
            return true;

        // Check vertical
        int verticalMatchCount = 0;
        verticalMatchCount += CountMatchesInDirection(GetObjecAtPosition(x, y), Vector2Int.up);
        verticalMatchCount += CountMatchesInDirection(GetObjecAtPosition(x, y), Vector2Int.down);

        if (verticalMatchCount >= 2)
            return true;

        return false;
    }

    private int CountMatchesInDirection(Matchable matchable, Vector2Int direction)
    {
        int matchCount = 0;
        Vector2Int checkPosition = matchable.gridPosition + direction;

        while (IsWithinBounds(checkPosition) && !IsPositionEmpty(checkPosition) && GetObjecAtPosition(checkPosition).Type == matchable.Type)
        {
            matchCount++;
            checkPosition += direction;
        }

        return matchCount;
    }

    public IEnumerator TrySwapping(Matchable[] toBeSwapped)
    {
        // Make a copy of what will be swapped so player can keep swapping while checking for matches
        Matchable[] copies = new Matchable[2];
        copies[0] = toBeSwapped[0];
        copies[1] = toBeSwapped[1];

        // yield until matchable animate swapping
        yield return StartCoroutine(SwapMatchables(copies));

        // Check for matches
        
    }

    private IEnumerator SwapMatchables(Matchable[] toBeSwapped)
    {
        // Swap items in the grid
        SwapObjectsAtPositions(toBeSwapped[0].gridPosition, toBeSwapped[1].gridPosition);

        // Tell the matchables their new position
        Vector2Int temp = toBeSwapped[0].gridPosition;
        toBeSwapped[0].gridPosition = toBeSwapped[1].gridPosition;
        toBeSwapped[1].gridPosition = temp;

        // Get the world position to move to
        Vector3[] positions = new Vector3[2];
        positions[0] = toBeSwapped[0].transform.position;
        positions[1] = toBeSwapped[1].transform.position;

        // Move them to their new position on screen
        StartCoroutine(toBeSwapped[0].MoveToPosition(positions[1]));
        yield return StartCoroutine(toBeSwapped[1].MoveToPosition(positions[0])); // wait till second one is done
    }

    private Match GetMatch(Matchable matchable)
    {
        Match match = new Match();
        Match horizontalMatch, verticalMatch;

        horizontalMatch = GetMatchesInDirection(match, matchable, Vector2Int.left);
        horizontalMatch.Merge(GetMatchesInDirection(match, matchable, Vector2Int.right));

        horizontalMatch.orientation = Orientation.Horizontal;

        if (horizontalMatch.Count > 1)
        {
            match.Merge(horizontalMatch);
            // Scan for vertical branches
            GetBranches(match, horizontalMatch, Orientation.Vertical);
        }

        verticalMatch = GetMatchesInDirection(match, matchable, Vector2Int.up);
        verticalMatch.Merge(GetMatchesInDirection(match,matchable, Vector2Int.down));

        verticalMatch.orientation = Orientation.Vertical;

        if (verticalMatch.Count > 1)
        {
            match.Merge(verticalMatch);
            // Scan for horizontal branches
            GetBranches(match, verticalMatch, Orientation.Horizontal);
        }

        if (match.Count == 1)
        {
            return null; // no match found
        }

        return match;
    }

    private Match GetMatchesInDirection(Match tree, Matchable matchable, Vector2Int direction)
    {
        Match match = new Match(matchable);
        Vector2Int checkPos = matchable.gridPosition + direction;
        Matchable next;

        while (IsWithinBounds(checkPos) && !IsPositionEmpty(checkPos))
        {
            next = GetObjecAtPosition(checkPos);

            if (next.Type == matchable.Type && next.Idle)
            {
                if (!tree.Contains(next))
                {
                    match.AddMatchable(next);
                }
                else
                {
                    match.AddUnlisted();
                }
                checkPos += direction;
            }
            else
            {
                break;
            }
        }

        return match;
    }

    private void GetBranches(Match tree, Match branchToSearch, Orientation perpendicular)
    {
        Match branch;

        foreach (Matchable matchable in branchToSearch.Matchables)
        {
            branch = GetMatchesInDirection(tree, matchable, perpendicular == Orientation.Horizontal ? Vector2Int.left : Vector2Int.down);
            branch.Merge(GetMatchesInDirection(tree, matchable, perpendicular == Orientation.Horizontal ? Vector2Int.right : Vector2Int.up));

            branch.orientation = perpendicular;

            if (branch.Count > 1)
            {
                tree.Merge(branch);
                // Recursively check for more branches
                GetBranches(tree, branch, perpendicular == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal);
            }
        }
    }
}
