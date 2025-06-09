using UnityEngine;
using System;
using System.Collections.Generic;

// 单个关节配置
[Serializable]
public class JointConfig
{
    public string jointName;
    public Vector3 anchor = Vector3.zero;
    public Vector3 axis = Vector3.up;
    public float minLimit = -45f;
    public float maxLimit = 45f;
    public bool useSpring = false;
    public float spring = 0f;
    public float damper = 0f;
    public ConnectedBodyType connectedBodyType = ConnectedBodyType.Parent;
    public string customConnectedBodyName = "";
}

// 连接体类型枚举
public enum ConnectedBodyType
{
    Parent,
    None,
    Custom
}

// 整个模型的配置
[CreateAssetMenu(fileName = "ModelJointConfig", menuName = "配置/关节配置")]
public class ModelJointConfig : ScriptableObject
{
    public string modelName;
    public List<JointConfig> joints = new List<JointConfig>();
}