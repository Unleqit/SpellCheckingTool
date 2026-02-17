public class MatchResult
{
    public string matchString;
    public int distance;

    public MatchResult(string matchString, int distance)
    {
        this.matchString = matchString;
        this.distance = distance;
    }

    public string GetMatchString()
    {
        return matchString;
    }

    public int GetMatchDistance()
    {
        return distance;
    }

    public override string ToString()
    {
        return GetMatchString();
    }
}
