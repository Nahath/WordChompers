using System;

[Serializable]
public class LevelEntry
{
    public int level;
    public int difficulty;
}

[Serializable]
public class LevelEntryList
{
    public LevelEntry[] levels;
}
