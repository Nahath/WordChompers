using System;

[Serializable]
public class WordData
{
    public string word;
    public int difficulty;
    public string[] categories;
}

[Serializable]
public class WordDataList
{
    public WordData[] words;
}
