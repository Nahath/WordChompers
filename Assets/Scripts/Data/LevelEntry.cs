using System;

[Serializable]
public class LevelEntry
{
    public int difficulty;
    public int start;
    public int end;
}

[Serializable]
public class LevelEntryList
{
    public LevelEntry[] levels;
}
