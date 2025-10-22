using UnityEngine;

public class Matchable : MovableObject
{
    private int type;

    public int Type
    {
        get { return type; }
    }

    private SpriteRenderer spriteRenderer;
    public Vector2Int gridPosition;

    private MatchablePool matchablePool;
    private MatchableGrid matchableGrid;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        matchablePool = (MatchablePool)MatchablePool.Instance;
        matchableGrid = (MatchableGrid)MatchableGrid.Instance;
    }

    public void SetType(int newType, Sprite newSprite)
    {
        type = newType;
        spriteRenderer.sprite = newSprite;
    }
}
