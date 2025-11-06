using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private MatchablePool matchablePool;
    private MatchableGrid matchableGrid;

    [SerializeField] private Vector2Int gridDimensions;

    private void Start()
    {
        matchablePool = (MatchablePool)MatchablePool.Instance;
        matchableGrid = (MatchableGrid)MatchableGrid.Instance;

        StartCoroutine(Setup());
    }
    
    private IEnumerator Setup()
    {
        matchablePool.CreatePool(gridDimensions.x * gridDimensions.y * 2);
        matchableGrid.InitializeGrid(gridDimensions);

        yield return null;

        yield return StartCoroutine(matchableGrid.PopulateObstacles());
        StartCoroutine(matchableGrid.PopulateGrid(initialPopulation: true));
    }
}
