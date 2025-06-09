using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class LegoDataSO : ScriptableObject
{
    public List<LegoData> legoDatas;
}

[Serializable]
public class LegoData
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public int Level { get; private set; }
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public string PrefabName{get;private set;}
    [field: SerializeField] public Sprite IconSource { get; private set; }
    [field: SerializeField] public Sprite IconSourceGray { get; private set; }
    [field: SerializeField] public Sprite CollectIconSource { get; private set; }
    [field: SerializeField] public Sprite CollectIconSourceGray { get; private set; }
}