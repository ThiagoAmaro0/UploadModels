using System.Collections.Generic;

[System.Serializable]
public class ModelAsset
{
    public string name;
    public string description;

    public ModelAsset(string _name, string _description)
    {
        name = _name;
        description = _description;
    }
    
}
public class ModelData
{
    public List<ModelAsset> modelAssets;
}
