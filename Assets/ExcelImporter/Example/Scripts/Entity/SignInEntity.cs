using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SignInEntity
{
    public int Id;
    public string Type;

    public int GetRewardPropId()
    {
        var data = Type.Split(';');
        return int.Parse(data[0]);
    }
}
