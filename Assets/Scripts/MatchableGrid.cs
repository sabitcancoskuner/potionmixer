using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;


public class MatchableGrid : GridSystem<Matchable>
{
    private MatchablePool matchablePool;

    private List<PotentialMatch> possibleMoves;
    
    public bool isProcessing = false;
    
    // Define the play area height (bottom 9 rows are playable, top 9 are spawn buffer)
    private const int PLAY_AREA_HEIGHT = 9;

    private void Start()
    {
        matchablePool = (MatchablePool)MatchablePool.Instance;
        possibleMoves = new List<PotentialMatch>();
    }

    public IEnumerator PopulateGrid(bool allowMatches = false, bool initialPopulation = false)
    {
        List<Matchable> newMatchables = new List<Matchable>();
        Matchable newMatchable;
        Vector3 onScreenPosition;

        // Spawn matchables column by column to get proper staggered heights
        for (int x = 0; x < Dimensions.x; x++)
        {
            int spawnHeight = 0; // Track how high above the grid to spawn in this column

            // Populate all empty cells in the entire grid
            for (int y = 0; y < Dimensions.y; y++)
            {
                if (IsPositionEmpty(x, y))
                {
                    newMatchable = matchablePool.GetRandomMatchable();

                    if (initialPopulation)
                    {
                        // Place directly on grid for initial population
                        onScreenPosition = transform.position + new Vector3(x, y, 0);
                        newMatchable.transform.position = onScreenPosition;
                    }
                    else
                    {
                        // Spawn above the entire grid, stacked by spawn height
                        onScreenPosition = transform.position + new Vector3(x, Dimensions.y + spawnHeight, 0);
                        newMatchable.transform.position = onScreenPosition;
                        spawnHeight++; // Next empty spot in this column spawns higher
                    }

                    newMatchable.gameObject.SetActive(true);
                    newMatchable.gridPosition = new Vector2Int(x, y);
                    SetObjectAtPosition(newMatchable, x, y);
                    newMatchables.Add(newMatchable);

                    // Only avoid matches in the play area (bottom PLAY_AREA_HEIGHT rows)
                    if (!allowMatches && y < PLAY_AREA_HEIGHT)
                    {
                        int type = newMatchable.Type;
                        while (HasMatchAtPosition(x, y, type))
                        {
                            if (matchablePool.NextType(newMatchable) == type)
                            {
                                Debug.LogWarning("All matchable types result in a match. Cannot avoid matches.");
                                break;
                            }
                        }
                    }
                }
            }
        }

        // Start all matchables moving to their grid positions
        if (!initialPopulation)
        {
            object fallKey = new object();
            int matchableIndex = 0;
            foreach (Matchable matchable in newMatchables)
            {
                Vector3 targetPosition = transform.position + new Vector3(matchable.gridPosition.x, matchable.gridPosition.y, 0);
                CoroutineManager.Instance.StartTrackedCoroutine(matchable.MoveWithPhysics(targetPosition), fallKey);

                // Add a small stagger delay between each matchable spawning
                // This creates a natural cascading drop effect
                matchableIndex++;
                if (matchableIndex < newMatchables.Count)
                {
                    yield return new WaitForSeconds(0.02f); // Small delay between each matchable
                }
            }

            // Wait for all matchables to finish falling
            yield return CoroutineManager.Instance.WaitForAll(fallKey);
        }

        if (initialPopulation)
        {
            CheckPossibleMoves();
        }

        yield return null;
    }
    
    // Populate obstacles around the grid edges, 6 is jar and 7 is ice block
    public IEnumerator PopulateObstacles(int obstacleType = 7)
    {
        Matchable obstacle;
        int[] xPos = new int[] { 0, Dimensions.x - 1 };

        foreach (int x in xPos)
        {
            // Only populate obstacles in the play area (bottom PLAY_AREA_HEIGHT rows)
            for (int y = 0; y < PLAY_AREA_HEIGHT; y++)
            {
                obstacle = matchablePool.GetPooledObject();
                obstacle.SetType(obstacleType);
                obstacle.gameObject.SetActive(true);
                obstacle.gridPosition = new Vector2Int(x, y);
                SetObjectAtPosition(obstacle, x, y);

                Vector3 onScreenPosition = transform.position + new Vector3(x, y, 0);
                obstacle.transform.position = onScreenPosition;
            }
        }
        
        yield return null;
    }

    private bool HasMatchAtPosition(int x, int y, int type)
    {
        // Check horizontal
        int horizontalMatchCount = 0;
        horizontalMatchCount += CountMatchesInDirection(GetObjectAtPosition(x, y), Vector2Int.left);
        horizontalMatchCount += CountMatchesInDirection(GetObjectAtPosition(x, y), Vector2Int.right);

        if (horizontalMatchCount >= 2)
            return true;

        // Check vertical
        int verticalMatchCount = 0;
        verticalMatchCount += CountMatchesInDirection(GetObjectAtPosition(x, y), Vector2Int.up);
        verticalMatchCount += CountMatchesInDirection(GetObjectAtPosition(x, y), Vector2Int.down);

        if (verticalMatchCount >= 2)
            return true;

        return false;
    }

    private int CountMatchesInDirection(Matchable matchable, Vector2Int direction)
    {
        int matchCount = 0;
        Vector2Int checkPosition = matchable.gridPosition + direction;

        while (IsWithinBounds(checkPosition) && !IsPositionEmpty(checkPosition) && GetObjectAtPosition(checkPosition).Type == matchable.Type)
        {
            matchCount++;
            checkPosition += direction;
        }

        return matchCount;
    }

    public IEnumerator TrySwapping(Matchable[] toBeSwapped)
    {
        if (isProcessing) yield break;
        isProcessing = true;

        try
        {
            // Make a copy of what will be swapped so player can keep swapping while checking for matches
            Matchable[] copies = new Matchable[2];
            copies[0] = toBeSwapped[0];
            copies[1] = toBeSwapped[1];

            if (toBeSwapped[0].Type == 7 || toBeSwapped[1].Type == 7)
            {
                yield break;
            }

            HintIndicator.Instance.CancelHint();

            // yield until matchable animate swapping
            yield return StartCoroutine(SwapMatchables(copies));

            // Check for matches
            Match[] matches = new Match[2];
            matches[0] = GetMatch(copies[0]);
            matches[1] = GetMatch(copies[1]);

            if (matches[0] != null || matches[1] != null)
            {
                object matchKey = new object();

                if (matches[0] != null)
                {
                    CoroutineManager.Instance.StartTrackedCoroutine(MatchManager.Instance.ResolveMatch(matches[0]), matchKey);
                    CoroutineManager.Instance.StartTrackedCoroutine(Damage(matches[0]), matchKey);
                }
                if (matches[1] != null)
                {
                    CoroutineManager.Instance.StartTrackedCoroutine(MatchManager.Instance.ResolveMatch(matches[1]), matchKey);
                    CoroutineManager.Instance.StartTrackedCoroutine(Damage(matches[1]), matchKey);
                }

                // Wait for both matches to complete
                yield return CoroutineManager.Instance.WaitForAll(matchKey);
                yield return StartCoroutine(FillAndScanGrid());
            }
            else
            {
                // If no matches, swap back
                yield return StartCoroutine(SwapMatchables(copies));
            }

            CheckPossibleMoves();
        }
        finally
        {
            isProcessing = false;
        }
    }
    
    private IEnumerator Damage(Match toDamage)
    {
        foreach (Matchable matchable in toDamage.Matchables)
        {
            Vector2Int position = matchable.gridPosition;
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                position + Vector2Int.up,
                position + Vector2Int.down,
                position + Vector2Int.left,
                position + Vector2Int.right
            };

            foreach (Vector2Int adjacent in adjacentPositions)
            {
                if (IsWithinBounds(adjacent) && !IsPositionEmpty(adjacent))
                {
                    Matchable adjacentMatchable = GetObjectAtPosition(adjacent);

                    if(adjacentMatchable.isObstacle)
                    {
                        StartCoroutine(adjacentMatchable.Resolve());
                    }
                }
            }
        }   
        yield return null;
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
        // Don't check for matches if the matchable is an obstacle
        if (matchable.isObstacle)
        {
            return null;
        }
        
        // Don't check for matches in the spawn buffer (only in play area)
        if (matchable.gridPosition.y >= PLAY_AREA_HEIGHT)
        {
            return null;
        }

        Match match = new Match(matchable);
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
        Match match = new Match();
        Vector2Int checkPos = matchable.gridPosition + direction;
        Matchable next;

        while (IsWithinBounds(checkPos) && !IsPositionEmpty(checkPos))
        {
            // Stop if we're checking above the play area (in spawn buffer)
            if (checkPos.y >= PLAY_AREA_HEIGHT)
            {
                break;
            }
            
            next = GetObjectAtPosition(checkPos);

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

            // Allow single-piece branches (Count > 1) to capture L-shapes and T-shapes
            if (branch.Count > 1)
            {
                tree.Merge(branch);
                // Recursively check for more branches
                GetBranches(tree, branch, perpendicular == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal);
            }
        }
    }

    private IEnumerator ScanForMatches(System.Action<bool> callback)
    {
        bool madeMatch = false;
        Matchable toMatch;
        Match match;
        object matchKey = new object();

        // Iterate through the play area looking for matches (only bottom PLAY_AREA_HEIGHT rows)
        for (int y = 0; y < PLAY_AREA_HEIGHT; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                if (!IsPositionEmpty(x, y))
                {
                    toMatch = GetObjectAtPosition(x, y);

                    if (!toMatch.Idle)
                    {
                        continue;
                    }

                    match = GetMatch(toMatch);

                    if (match != null)
                    {
                        CoroutineManager.Instance.StartTrackedCoroutine(MatchManager.Instance.ResolveMatch(match), matchKey);
                        madeMatch = true;
                    }
                }
            }
        }
        
        if(madeMatch)
        {
            yield return CoroutineManager.Instance.WaitForAll(matchKey);
        }
        callback(madeMatch);
    }

    private IEnumerator FillAndScanGrid()
    {
        bool matchesFound;
        do
        {
            yield return StartCoroutine(CollapseGrid());
            yield return StartCoroutine(PopulateGrid());
            
            matchesFound = false;
            yield return StartCoroutine(ScanForMatches(result => matchesFound = result));
        } while (matchesFound);

        CheckPossibleMoves();
    }


    public IEnumerator CollapseGrid()
    {
        object collapseKey = new object();
        // Process each column independently using two-pointer algorithm
        for (int x = 0; x < Dimensions.x; x++)
        {
            int writePointer = 0; // Points to where next matchable should be written

            // Read from bottom to top through the ENTIRE column (including spawn buffer)
            for (int readPointer = 0; readPointer < Dimensions.y; readPointer++)
            {
                if (!IsPositionEmpty(x, readPointer))
                {
                    Matchable matchable = GetObjectAtPosition(x, readPointer);

                    if (matchable.Type == 7)
                    {
                        // Ice block obstacle, cannot move
                        writePointer = readPointer + 1;
                        continue;
                    }

                    // If read and write pointers are different, move the matchable
                    if (readPointer != writePointer)
                    {
                        // Remove from current position
                        RemoveObjectAtPosition(x, readPointer);

                        // Set new grid position
                        matchable.gridPosition = new Vector2Int(x, writePointer);
                        SetObjectAtPosition(matchable, x, writePointer);

                        // Calculate world position and move
                        Vector3 worldPosition = transform.position + new Vector3(x, writePointer, 0);
                        CoroutineManager.Instance.StartTrackedCoroutine(matchable.MoveWithPhysics(worldPosition), collapseKey);

                        // Add a small stagger delay based on how far the matchable is falling
                        // This creates a natural cascading effect
                        yield return new WaitForSeconds(0.001f); // 0.02 seconds per cell fallen
                    }

                    // Move write pointer up for next matchable
                    writePointer++;
                }
            }
        }

        // Wait a bit for movements to complete
        yield return CoroutineManager.Instance.WaitForAll(collapseKey);
        yield return StartCoroutine(CheckForDigonalMovement());
    }

    private IEnumerator CheckForDigonalMovement()
    {
        bool foundDiagonalMove = true;
        
        // Keep checking while diagonal movements are available
        while (foundDiagonalMove)
        {
            foundDiagonalMove = false;

            // Use goto to break out of both loops when we find a diagonal move
            for (int x = 1; x < Dimensions.x - 1; x++)
            {
                for (int y = 1; y < PLAY_AREA_HEIGHT; y++)
                {
                    if (GetObjectAtPosition(x, y).Type != 7 &&!IsPositionEmpty(x, y) && !IsPositionEmpty(x, y - 1))
                    {
                        // Check for diagonal match
                        Matchable current = GetObjectAtPosition(x, y);
                        
                        if (IsPositionEmpty(x + 1, y - 1))
                        {
                            // Found a digonal match down right
                            RemoveObjectAtPosition(x, y);                        
                            current.gridPosition = new Vector2Int(x + 1, y - 1); 
                            SetObjectAtPosition(current, x + 1, y - 1);
                            current.transform.position = transform.position + new Vector3(x + 1, y - 1, 0);
                            foundDiagonalMove = true;
                            goto DiagonalMoveFound; // Break out of both loops
                        }
                        else if (IsPositionEmpty(x - 1, y - 1))
                        {
                            // Found a diagonal match down left
                            RemoveObjectAtPosition(x, y);
                            current.gridPosition = new Vector2Int(x - 1, y - 1);
                            SetObjectAtPosition(current, x - 1, y - 1);
                            current.transform.position = transform.position + new Vector3(x - 1, y - 1, 0);
                            foundDiagonalMove = true;
                            goto DiagonalMoveFound; // Break out of both loops
                        }
                    }
                }
            }
            
            DiagonalMoveFound:
            // If we found any diagonal moves, collapse the grid and check again
            if (foundDiagonalMove)
            {
                yield return StartCoroutine(CollapseGrid());
            }
        }
    }

    public IEnumerator TryActivatingPowerup(Matchable toActivate, Transform collectionPoint)
    {
        if (isProcessing) yield break;
        isProcessing = true;
        
        try
        {
            yield return StartCoroutine(toActivate.ActivatePowerup());
        }
        finally
        {
            isProcessing = false;
        }
    }

    public IEnumerator MatchRow(Matchable matchable)
    {
        if (isProcessing) yield break;
        isProcessing = true;
        
        try
        {
            Matchable toResolve;
            object matchKey = new object();
            for (int x = 0; x < Dimensions.x; x++)
            {
                if (!IsPositionEmpty(x, matchable.gridPosition.y))
                {
                    toResolve = GetObjectAtPosition(x, matchable.gridPosition.y);
                    if (toResolve.Idle)
                    {
                        CoroutineManager.Instance.StartTrackedCoroutine(MatchManager.Instance.ResolveMatch(new Match(toResolve)), matchKey);
                    }
                }
            }
            yield return CoroutineManager.Instance.WaitForAll(matchKey);

            yield return StartCoroutine(FillAndScanGrid());
        }
        finally
        {
            isProcessing = false;
        }
    }

    public IEnumerator MatchColumn(Matchable matchable)
    {
        if (isProcessing) yield break;
        isProcessing = true;
        
        try
        {
            Matchable toResolve;
            object matchKey = new object();
            // Only match within the play area height
            for (int y = 0; y < PLAY_AREA_HEIGHT; y++)
            {
                if (!IsPositionEmpty(matchable.gridPosition.x, y))
                {
                    toResolve = GetObjectAtPosition(matchable.gridPosition.x, y);
                    if (toResolve.Idle)
                    {
                        CoroutineManager.Instance.StartTrackedCoroutine(MatchManager.Instance.ResolveMatch(new Match(toResolve)), matchKey);
                    }
                }
            }
            yield return CoroutineManager.Instance.WaitForAll(matchKey);

            yield return StartCoroutine(FillAndScanGrid());
        }
        finally
        {
            isProcessing = false;
        }
    }

    public IEnumerator MatchAllOfType(Matchable discoBall, int type)
    {
        if (isProcessing) yield break;
        isProcessing = true;
        
        try
        {
            RemoveObjectAtPosition(discoBall.gridPosition);
            object matchKey = new object();
            // Only match within the play area
            for (int y = 0; y < PLAY_AREA_HEIGHT; y++)
            {
                for (int x = 0; x < Dimensions.x; x++)
                {
                    if (!IsPositionEmpty(x, y))
                    {
                        Matchable toResolve = GetObjectAtPosition(x, y);
                        if (toResolve.Type == type && toResolve.Idle)
                        {
                            CoroutineManager.Instance.StartTrackedCoroutine(MatchManager.Instance.ResolveMatch(new Match(toResolve)), matchKey);
                        }
                    }
                }
            }
            yield return CoroutineManager.Instance.WaitForAll(matchKey);

            yield return StartCoroutine(FillAndScanGrid());
        }
        finally
        {
            isProcessing = false;
        }
    }

    public IEnumerator MatchArea(Matchable matchable, int radius = 2)
    {
        if (isProcessing) yield break;
        isProcessing = true;
        
        try
        {
            // Remove the bomb from the grid first to prevent it from being counted as empty
            RemoveObjectAtPosition(matchable.gridPosition);

            // Calculate the bounds of the 5x5 area (radius of 2 from center)
            // Clamp to play area boundaries
            int startX = Mathf.Max(0, matchable.gridPosition.x - radius);
            int endX = Mathf.Min(Dimensions.x - 1, matchable.gridPosition.x + radius);
            int startY = Mathf.Max(0, matchable.gridPosition.y - radius);
            int endY = Mathf.Min(PLAY_AREA_HEIGHT - 1, matchable.gridPosition.y + radius);

            object matchKey = new object();
            // Destroy all matchables in the area
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    if (!IsPositionEmpty(x, y))
                    {
                        Matchable toResolve = GetObjectAtPosition(x, y);
                        if (toResolve.Idle)
                        {
                            CoroutineManager.Instance.StartTrackedCoroutine(MatchManager.Instance.ResolveMatch(new Match(toResolve)), matchKey);
                        }
                    }
                }
            }
            yield return CoroutineManager.Instance.WaitForAll(matchKey);

            yield return StartCoroutine(FillAndScanGrid());
        }
        finally
        {
            isProcessing = false;
        }
    }

    public void CheckPossibleMoves()
    {
        if (ScanForMoves() == 0)
        {
            Debug.Log("No possible moves left!");
            // Handle no possible moves (e.g., reshuffle the grid)
            StartCoroutine(ShuffleGrid());
        }

        else
        {
            ShowHint();
        }
    }

    private IEnumerator ShuffleGrid()
    {
        List<Matchable> allMatchables = new List<Matchable>();

        // Collect all matchables from the play area only (not obstacles)
        for (int y = 0; y < PLAY_AREA_HEIGHT; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                if (!IsPositionEmpty(x, y))
                {
                    Matchable matchable = GetObjectAtPosition(x, y);
                    // Don't shuffle obstacles
                    if (!matchable.isObstacle)
                    {
                        allMatchables.Add(matchable);
                        RemoveObjectAtPosition(new Vector2Int(x, y));
                    }
                }
            }
        }

        int maxAttempts = 10000;
        int attempts = 0;
        bool validShuffleFound = false;

        // Keep shuffling until we find a valid configuration or hit max attempts
        while (!validShuffleFound && attempts < maxAttempts)
        {
            attempts++;

            // Shuffle the list
            for (int i = 0; i < allMatchables.Count; i++)
            {
                Matchable temp = allMatchables[i];
                int randomIndex = Random.Range(i, allMatchables.Count);
                allMatchables[i] = allMatchables[randomIndex];
                allMatchables[randomIndex] = temp;
            }

            // Temporarily assign to grid to check for matches and moves
            int index = 0;
            bool hasImmediateMatches = false;
            
            for (int y = 0; y < Dimensions.y; y++)
            {
                for (int x = 0; x < Dimensions.x; x++)
                {
                    if (index < allMatchables.Count)
                    {
                        Matchable matchable = allMatchables[index];
                        matchable.gridPosition = new Vector2Int(x, y);
                        SetObjectAtPosition(matchable, x, y);
                        
                        // Check if this position creates an immediate match
                        if (HasMatchAtPosition(x, y, matchable.Type))
                        {
                            hasImmediateMatches = true;
                            break;
                        }
                        
                        index++;
                    }
                }
                
                if (hasImmediateMatches)
                    break;
            }

            // Valid shuffle = no immediate matches AND has possible moves
            if (!hasImmediateMatches && ScanForMoves() > 0)
            {
                validShuffleFound = true;
            }
        }

        // If no valid shuffle found after max attempts, log warning
        if (!validShuffleFound)
        {
            Debug.LogWarning($"Could not find valid shuffle after {maxAttempts} attempts. Using last shuffle.");
        }

        // Animate matchables to their new positions
        for (int i = 0; i < allMatchables.Count; i++)
        {
            Matchable matchable = allMatchables[i];
            Vector3 worldPosition = transform.position + new Vector3(matchable.gridPosition.x, matchable.gridPosition.y, 0);
            
            if (i == allMatchables.Count - 1)
            {
                // Wait for the last one to finish
                yield return StartCoroutine(matchable.MoveToPosition(worldPosition));
            }
            else
            {
                StartCoroutine(matchable.MoveToPosition(worldPosition));
            }
        }

        yield return null;
    }

    // Scan for all possible moves
    private int ScanForMoves()
    {
        possibleMoves.Clear();

        // Iterate through the play area looking for possible moves
        for (int y = 0; y < PLAY_AREA_HEIGHT; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                if (IsWithinBounds(x, y) && !IsPositionEmpty(x, y))
                {
                    Matchable matchable = GetObjectAtPosition(x, y);
                    if (matchable.isObstacle)
                    {
                        continue;
                    }
                    List<Matchable> potentialMatch = CanMove(matchable);
                    if (potentialMatch != null && potentialMatch.Count > 0)
                    {
                        possibleMoves.Add(new PotentialMatch(potentialMatch));
                    }
                }
            }
        }

        // Sort by match length descending (longest matches first)
        possibleMoves = possibleMoves.OrderByDescending(m => m.matchLength).ToList();

        return possibleMoves.Count;
    }

    private List<Matchable> CanMove(Matchable matchable)
    {
        List<Matchable> match = null;

        // Check all four directions
        match = CanMoveInDirection(matchable, Vector2Int.up);
        if (match != null) return match;

        match = CanMoveInDirection(matchable, Vector2Int.down);
        if (match != null) return match;

        match = CanMoveInDirection(matchable, Vector2Int.left);
        if (match != null) return match;

        match = CanMoveInDirection(matchable, Vector2Int.right);
        if (match != null) return match;

        return null;
    }
    
    private List<Matchable> CanMoveInDirection(Matchable toCheck, Vector2Int direction)
    {
        Vector2Int firstPosition = toCheck.gridPosition + direction * 2;
        Vector2Int secondPosition = toCheck.gridPosition + direction * 3;

        List<Matchable> match = IsAPotentialMatch(toCheck, firstPosition, secondPosition, direction);
        if (match != null) return match;

        Vector2Int clockWise = new Vector2Int(direction.y, -direction.x);
        Vector2Int counterClockWise = new Vector2Int(-direction.y, direction.x);

        // Look diagonally clockwise
        firstPosition = toCheck.gridPosition + direction + clockWise;
        secondPosition = toCheck.gridPosition + direction + clockWise * 2;

        match = IsAPotentialMatch(toCheck, firstPosition, secondPosition, direction);
        if (match != null) return match;

        // Look diagonally both ways
        secondPosition = toCheck.gridPosition + direction + counterClockWise;

        match = IsAPotentialMatch(toCheck, firstPosition, secondPosition, direction);
        if (match != null) return match;

        // Look diagonally counter-clockwise
        firstPosition = toCheck.gridPosition + direction + counterClockWise * 2;

        match = IsAPotentialMatch(toCheck, firstPosition, secondPosition, direction);
        if (match != null) return match;

        return null;
    }

    private List<Matchable> IsAPotentialMatch(Matchable toCheck, Vector2Int firstPosition, Vector2Int secondPosition, Vector2Int moveDirection)
    {
        // Check that all positions are within play area bounds (not spawn buffer)
        if (firstPosition.y >= PLAY_AREA_HEIGHT || secondPosition.y >= PLAY_AREA_HEIGHT)
            return null;
            
        if (IsWithinBounds(firstPosition) && !IsPositionEmpty(firstPosition)
            && IsWithinBounds(secondPosition) && !IsPositionEmpty(secondPosition)
            && GetObjectAtPosition(firstPosition).Idle && GetObjectAtPosition(secondPosition).Idle
            && GetObjectAtPosition(firstPosition).Type == toCheck.Type && GetObjectAtPosition(secondPosition).Type == toCheck.Type)
        {
            // Build the list of all pieces that would be part of this match
            List<Matchable> matchPieces = new List<Matchable>();
            
            // Add the piece that would move to create the match
            Vector2Int targetPosition = toCheck.gridPosition + moveDirection;
            
            // Make sure target position is in play area
            if (targetPosition.y >= PLAY_AREA_HEIGHT)
                return null;
            
            // Simulate the swap: temporarily place toCheck at the target position
            // Then find all pieces that would match
            
            // Add the two pieces we already know will match
            matchPieces.Add(GetObjectAtPosition(firstPosition));
            matchPieces.Add(GetObjectAtPosition(secondPosition));
            
            // Add the piece that would be moved (toCheck will be at targetPosition)
            matchPieces.Add(toCheck);
            
            // Now check for additional pieces in line with the match
            // Check in both directions from the target position
            Vector2Int directionVector = firstPosition - targetPosition;
            Vector2Int checkDirection = new Vector2Int(
                directionVector.x != 0 ? directionVector.x / Mathf.Abs(directionVector.x) : 0,
                directionVector.y != 0 ? directionVector.y / Mathf.Abs(directionVector.y) : 0
            );
            
            // Check in the direction of the match
            Vector2Int checkPos = targetPosition + checkDirection;
            while (IsWithinBounds(checkPos) && checkPos.y < PLAY_AREA_HEIGHT && !IsPositionEmpty(checkPos))
            {
                Matchable checkMatchable = GetObjectAtPosition(checkPos);
                if (checkMatchable.Type == toCheck.Type && checkMatchable.Idle && !matchPieces.Contains(checkMatchable))
                {
                    matchPieces.Add(checkMatchable);
                    checkPos += checkDirection;
                }
                else break;
            }
            
            // Check in the opposite direction
            checkPos = targetPosition - checkDirection;
            while (IsWithinBounds(checkPos) && checkPos.y < PLAY_AREA_HEIGHT && !IsPositionEmpty(checkPos))
            {
                Matchable checkMatchable = GetObjectAtPosition(checkPos);
                if (checkMatchable.Type == toCheck.Type && checkMatchable.Idle && !matchPieces.Contains(checkMatchable))
                {
                    matchPieces.Add(checkMatchable);
                    checkPos -= checkDirection;
                }
                else break;
            }
            
            return matchPieces;
        }

        return null;
    }

    public void ShowHint()
    {
        if (possibleMoves.Count > 0)
        {
            // Get the longest match (first one since we sorted by length descending)
            PotentialMatch longestMatch = possibleMoves[0];
            HintIndicator.Instance.StartAutoHint(longestMatch.matchPieces);
        }
    }

}
