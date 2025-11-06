using UnityEngine;

public class MatchablePool : ObjectPool<Matchable>
{
    [SerializeField] private int numberOfTypes;

    public void RandomizeType(Matchable matchable)
    {
        int randomType = Random.Range(0, numberOfTypes);
        matchable.SetType(randomType);
    }

    public Matchable GetRandomMatchable()
    {
        Matchable matchable = GetPooledObject();
        RandomizeType(matchable);

        // Ensure collider is enabled for new matchables
        // matchable.EnableCollider();
        
        return matchable;
    }

    public int NextType(Matchable matchable)
    {
        int currentType = matchable.Type;
        int newType = (currentType + 1) % numberOfTypes;
        matchable.SetType(newType);

        return newType;
    }

}
