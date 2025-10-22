using UnityEngine;

public class MatchablePool : ObjectPool<Matchable>
{
    [SerializeField] private int numberOfTypes;
    [SerializeField] private Sprite[] sprites;

    public void RandomizeType(Matchable matchable)
    {
        int randomType = Random.Range(0, numberOfTypes);
        matchable.SetType(randomType, sprites[randomType]);
    }

    public Matchable GetRandomMatchable()
    {
        Matchable matchable = GetPooledObject();
        RandomizeType(matchable);
        
        return matchable;
    }

    public int NextType(Matchable matchable)
    {
        int currentType = matchable.Type;
        int newType = (currentType + 1) % numberOfTypes;
        matchable.SetType(newType, sprites[newType]);

        return newType;
    }
}
