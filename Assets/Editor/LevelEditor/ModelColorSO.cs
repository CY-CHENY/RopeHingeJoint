
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ModelColorSO : ScriptableObject
{
    public List<NameColor> nameColor = new List<NameColor>();
}

[Serializable]
public class NameColor
{
    [field: SerializeField] public string Name { get; set; }
    [field: SerializeField] public int Color { get; set; }
}