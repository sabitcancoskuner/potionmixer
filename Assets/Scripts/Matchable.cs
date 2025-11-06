using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Matchable : MovableObject
{
    private int type;
    private bool isPowerUp = false;
    [SerializeField] private PowerupType powerupType = PowerupType.None;

    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Sprite horizontalRocketSprite;
    [SerializeField] private Sprite verticalRocketSprite;
    [SerializeField] private Sprite discoBallSprite;
    [SerializeField] private Sprite bombSprite;

    [Space]
    [SerializeField] private float gravityForce = 10f;
    private BoxCollider2D boxCollider;

    public int Type
    {
        get { return type; }
    }

    public bool IsPowerUp
    {
        get { return isPowerUp; }
    }

    private SpriteRenderer spriteRenderer;
    public Vector2Int gridPosition;

    private MatchablePool matchablePool;
    private MatchableGrid matchableGrid;
    private Coroutine hintCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        matchablePool = (MatchablePool)MatchablePool.Instance;
        matchableGrid = (MatchableGrid)MatchableGrid.Instance;
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        //rb.bodyType = RigidbodyType2D.Kinematic;

        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void SetType(int newType)
    {
        type = newType;
        spriteRenderer.sprite = sprites[newType];
    }

    public IEnumerator Resolve(Transform collectionPoint)
    {
        if (collectionPoint == null)
        {
            yield break;
        }

        // Disable collider immediately when starting to resolve
        // DisableCollider();

        if (isPowerUp)
        {
            // If powerup is being resolved as part of a match (not activated directly),
            // just activate it without moving to collection point
            yield return StartCoroutine(ActivatePowerup());
            
            // Return object to the pool
            matchablePool.ReturnToPool(this);
            yield break;
        }

        // For normal matchables: Draw above all other objects
        spriteRenderer.sortingOrder = 10;
        DisablePhysics();

        // Move them off the grid
        // yield return StartCoroutine(MoveToTransform(collectionPoint));

        // Reset sorting order
        spriteRenderer.sortingOrder = 1;

        // Return object to the pool
        matchablePool.ReturnToPool(this);

        yield return null;
    }

    public void Upgrade(PowerupType powerupType)
    {
        this.powerupType = powerupType;
        isPowerUp = true;
        // Upgrade the matchable to a powerup
        switch (powerupType)
        {
            case PowerupType.RocketHorizontal:
                type = 100;
                spriteRenderer.sprite = horizontalRocketSprite;
                break;
            case PowerupType.RocketVertical:
                type = 101;
                spriteRenderer.sprite = verticalRocketSprite;
                break;
            case PowerupType.DiscoBall:
                type = 102;
                spriteRenderer.sprite = discoBallSprite;
                break;
            case PowerupType.Bomb:
                type = 103;
                spriteRenderer.sprite = bombSprite;
                break;
        }
    }
    
    public IEnumerator ActivatePowerup()
    {
        if (!isPowerUp)
        {
            yield break;
        }

        // Hide the powerup sprite immediately
        spriteRenderer.enabled = false;

        // Activate powerup effect based on its type
        switch (powerupType)
        {
            case PowerupType.RocketHorizontal:
                // Activate horizontal rocket effect
                yield return StartCoroutine(HandleHorizontalRocket());
                break;
            case PowerupType.RocketVertical:
                // Activate vertical rocket effect
                yield return StartCoroutine(HandleVerticalRocket());
                break;
            case PowerupType.DiscoBall:
                // Activate disco ball effect
                yield return StartCoroutine(HandleDiscoBall());
                break;
            case PowerupType.Bomb:
                // Activate bomb effect
                yield return StartCoroutine(HandleBomb());
                break;
        }
        
        // Re-enable sprite for when it's returned to pool
        spriteRenderer.enabled = true;
    }

    private IEnumerator HandleBomb()
    {
        // Destroy a 5x5 area centered on the bomb position
        yield return StartCoroutine(matchableGrid.MatchArea(this, radius: 2));
    }

    private IEnumerator HandleDiscoBall(bool randomType = false)
    {
        int randomTypeIndex = Random.Range(0, sprites.Length);
        yield return StartCoroutine(matchableGrid.MatchAllOfType(this, randomTypeIndex));
    }

    private IEnumerator HandleVerticalRocket()
    {
        yield return StartCoroutine(matchableGrid.MatchColumn(this));
    }

    private IEnumerator HandleHorizontalRocket()
    {
        yield return StartCoroutine(matchableGrid.MatchRow(this));
    }

    public IEnumerator HintMatchable()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.4f;

        // Continuously loop the scaling animation
        while (true)
        {
            yield return StartCoroutine(Scale(originalScale, targetScale));
            yield return StartCoroutine(Scale(targetScale, originalScale));
        }
    }

    public IEnumerator Scale(Vector2 from, Vector2 to)
    {
        while (Vector3.Distance(transform.localScale, to) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, to, 0.02f);
            yield return null;
        }
        // Ensure we reach the exact target scale
        transform.localScale = to;
        yield return null;
    }

    public void StartHintAnimation()
    {
        spriteRenderer.sortingOrder = 5; // Bring to front for visibility
        StopHintAnimation(); // Stop any existing hint animation first
        hintCoroutine = StartCoroutine(HintMatchable());
    }

    public void StopHintAnimation()
    {
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }
        // Reset to original scale
        transform.localScale = Vector3.one;
        spriteRenderer.sortingOrder = 1; // Reset sorting order
    }

    public void EnablePhysics()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityForce;
    }

    public void DisablePhysics()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
    }

    public void EnableCollider()
    {
        boxCollider.enabled = true;
    }

    public void DisableCollider()
    {
        boxCollider.enabled = false;
    }

    public IEnumerator MoveWithPhysics(Vector3 targetPosition, float timeout = 3f)
    {
        EnablePhysics();
        idle = false;

        float elapsed = 0f;
        float threshold = 0.15f; // How close is "close enough"

        while (Vector3.Distance(transform.position, targetPosition) > threshold && elapsed < timeout)
        {
            if (rb.linearVelocity.magnitude < 15f)
            {
                rb.AddForce(Vector2.down * gravityForce);
            }
            // Apply force towards target

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to final position
        transform.position = targetPosition;
        idle = true;
        DisablePhysics();
    }
    
}

