using System;

[Serializable]
public class CategoryData
{
    public string name;
    public int minimumDifficulty;
    public string levelHeader;
}

[Serializable]
public class CategoryDataList
{
    public CategoryData[] categories;
}
