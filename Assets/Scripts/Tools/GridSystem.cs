using System.Collections.Generic;
using UnityEngine;

public class GridSystem<T> : Singleton<GridSystem<T>>
{
    private T[,] gridData;

    private Vector2Int gridDimensions;

    public Vector2Int Dimensions
    {
        get { return gridDimensions; }
    }

    private bool isReady = false;

    public bool IsReady
    {
        get { return isReady; }
    }

    // Initialize the grid with specified dimensions
    public void InitializeGrid(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            Debug.LogError("Grid dimensions must be greater than zero.");
            return;
        }

        gridDimensions = new Vector2Int(width, height);
        gridData = new T[width, height];
        isReady = true;
    }

    public void InitializeGrid(Vector2Int dimensions)
    {
        InitializeGrid(dimensions.x, dimensions.y);
    }

    // Clear the entire grid
    public void ClearGrid()
    {
        if (!isReady)
        {
            Debug.LogWarning("Grid is not initialized. Cannot clear.");
            return;
        }

        for (int x = 0; x < gridDimensions.x; x++)
        {
            for (int y = 0; y < gridDimensions.y; y++)
            {
                gridData[x, y] = default(T);
            }
        }
    }

    // Bound check for grid coordinates
    public bool IsWithinBounds(int x, int y)
    {
        if (!isReady)
        {
            Debug.LogWarning("Grid is not initialized. Cannot check bounds.");
            return false;
        }

        return x >= 0 && x < gridDimensions.x && y >= 0 && y < gridDimensions.y;
    }

    public bool IsWithinBounds(Vector2Int position)
    {
        return IsWithinBounds(position.x, position.y);
    }

    // Check if the grid position is empty
    public bool IsPositionEmpty(int x, int y)
    {
        if (!isReady)
        {
            Debug.LogWarning("Grid is not initialized. Cannot check position.");
            return false;
        }

        if (!IsWithinBounds(x, y))
        {
            Debug.LogWarning("Position is out of bounds.");
            return false;
        }

        return EqualityComparer<T>.Default.Equals(gridData[x, y], default(T));
    }

    public bool IsPositionEmpty(Vector2Int position)
    {
        return IsPositionEmpty(position.x, position.y);
    }

    // Set data at a specific grid position
    public void SetObjectAtPosition(T data, int x, int y)
    {
        if (!isReady)
        {
            Debug.LogWarning("Grid is not initialized. Cannot set data.");
            return;
        }

        if (!IsWithinBounds(x, y))
        {
            Debug.LogWarning("Position is out of bounds.");
            return;
        }

        gridData[x, y] = data;
    }

    public void SetObjectAtPosition(T data, Vector2Int position)
    {
        SetObjectAtPosition(data, position.x, position.y);
    }

    // Get data from a specific grid position
    public T GetObjecAtPosition(int x, int y)
    {
        if (!isReady)
        {
            Debug.LogWarning("Grid is not initialized. Cannot get data.");
            return default(T);
        }

        if (!IsWithinBounds(x, y))
        {
            Debug.LogWarning("Position is out of bounds.");
            return default(T);
        }

        return gridData[x, y];
    }

    public T GetObjecAtPosition(Vector2Int position)
    {
        return GetObjecAtPosition(position.x, position.y);
    }

    // Remove data from a position (set to default)
    public T RemoveObjectAtPosition(int x, int y)
    {
        if (!isReady)
        {
            Debug.LogWarning("Grid is not initialized. Cannot remove data.");
        }

        if (!IsWithinBounds(x, y))
        {
            Debug.LogWarning("Position is out of bounds.");
        }

        T removedData = gridData[x, y];
        gridData[x, y] = default(T);
        return removedData;
    }

    public T RemoveObjectAtPosition(Vector2Int position)
    {
        return RemoveObjectAtPosition(position.x, position.y);
    }

    // Swap the objects at two grid positions
    public void SwapObjectsAtPositions(int x1, int y1, int x2, int y2)
    {
        if (!isReady)
        {
            Debug.LogWarning("Grid is not initialized. Cannot swap data.");
            return;
        }

        if (!IsWithinBounds(x1, y1) || !IsWithinBounds(x2, y2))
        {
            Debug.LogWarning("One or both positions are out of bounds.");
            return;
        }

        T temp = gridData[x1, y1];
        gridData[x1, y1] = gridData[x2, y2];
        gridData[x2, y2] = temp;
    }

    public void SwapObjectsAtPositions(Vector2Int pos1, Vector2Int pos2)
    {
        SwapObjectsAtPositions(pos1.x, pos1.y, pos2.x, pos2.y);
    }

    // Move an object to a new position
    public bool MoveObjectToPosition(int fromX, int fromY, int toX, int toY)
    {
        if (!isReady)
        {
            Debug.LogWarning("Grid is not initialized. Cannot move data.");
            return false;
        }

        if (!IsWithinBounds(fromX, fromY) || !IsWithinBounds(toX, toY))
        {
            Debug.LogWarning("One or both positions are out of bounds.");
            return false;
        }

        T dataToMove = gridData[fromX, fromY];
        gridData[toX, toY] = dataToMove;

        return true;
    }

    public bool MoveObjectToPosition(Vector2Int fromPos, Vector2Int toPos)
    {
        return MoveObjectToPosition(fromPos.x, fromPos.y, toPos.x, toPos.y);
    }


}
