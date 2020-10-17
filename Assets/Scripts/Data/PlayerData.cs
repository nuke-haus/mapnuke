public class PlayerData
{
    public NationData NationData
    {
        get;
        private set;
    }

    public int TeamNum
    {
        get;
        private set;
    }

    public PlayerData(NationData nation, int team = 1)
    {
        NationData = nation;
        TeamNum = team;
    }
}
