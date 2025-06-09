
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu]
[Serializable]
public class LevelConfigSO : ScriptableObject
{
    public int level;
    public List<int> boxPool = new List<int>();
}