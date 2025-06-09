using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ColorMaterialSO : ScriptableObject
{
    //public List<ColorMaterialData> materialDatas;
}

[Serializable]
public class ColorMaterialData
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public Material material { get; private set; }
}