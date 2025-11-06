using System.Collections.Generic;

// Helper class to store potential match data
public class PotentialMatch
{
    public List<Matchable> matchPieces; // All pieces that would be part of the match
    public int matchLength; // Total number of pieces in the match

    public PotentialMatch(List<Matchable> pieces)
    {
        matchPieces = pieces;
        matchLength = pieces.Count;
    }
}
