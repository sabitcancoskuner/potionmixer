using System.Collections.Generic;
using UnityEngine;

public enum Orientation
{
    None,
    Horizontal,
    Vertical,
    Both
}

public enum MatchType
{
    None,
    Three,
    Four,
    Five,
    LShape,
    TShape,
    Special
}

public class Match
{
    private int unlisted = 0;

    public Orientation orientation;

    private List<Matchable> matchables;
    private Matchable toBeUpgraded= null;
    public MatchType type;

    public List<Matchable> Matchables
    {
        get { return matchables; }
    }

    public int Count
    {
        get { return matchables.Count + unlisted; }
    }

    public bool Contains(Matchable toCompare)
    {
        return matchables.Contains(toCompare);
    }

    public Match()
    {
        matchables = new List<Matchable>(5); // it will resize as needed
    }

    public Match(Matchable original) : this()
    {
        AddMatchable(original);
        toBeUpgraded = original;
    }

    // Get the matchable to be upgrade into a powerup
    public Matchable ToBeUpgraded
    {
        get { return toBeUpgraded; }
    }

    public void AddMatchable(Matchable matchable)
    {
        matchables.Add(matchable);
    }

    public void RemoveMatchable(Matchable matchable)
    {
        matchables.Remove(matchable);
    }

    public void AddUnlisted()
    {
        unlisted++;
    }

    public void Merge(Match toMerge)
    {
        matchables.AddRange(toMerge.Matchables);

        // Update the matchable orientation
        if
        (
            orientation == Orientation.Both ||
            toMerge.orientation == Orientation.Both ||
            (orientation == Orientation.Horizontal && toMerge.orientation == Orientation.Vertical) ||
            (orientation == Orientation.Vertical && toMerge.orientation == Orientation.Horizontal)

        )
        {
            orientation = Orientation.Both;
        }

        else if (toMerge.orientation != Orientation.Horizontal)
        {
            orientation = Orientation.Horizontal;
        }
        
        else if (toMerge.orientation != Orientation.Vertical)
        {
            orientation = Orientation.Vertical;
        }
    }
}
