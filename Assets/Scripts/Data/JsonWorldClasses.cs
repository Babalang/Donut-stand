using System;
using System.Collections.Generic;


[System.Serializable]
public class FormeData
{
    public string couleur;
    public string materiau;
    public float poids;
    public int opaque;
    public List<List<float>> position_occupe;
    public string on_Top_of;
    public string under;
    public List<float> orientation;
}

[System.Serializable]
public class RootData
{
    public Dictionary<string, FormeData> forme;
}



[System.Serializable]
public class ArmState
{
    public bool holding;
    public string @object;
}

[System.Serializable]
public class SolutionStep
{
    public int id;
    public int? parent;
    public string action;
    public StateData state;
}

[System.Serializable]
public class StateData
{
    public ArmState arm;
    public Dictionary<string, FormeData> forme;
}
