using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelRope : BaseController
{
    public List<ModelFace> listModelFace;
    
    void Awake()
    {
        listModelFace = new List<ModelFace>();
    }

    public void AddToModelFace(ModelFace modelFace)
    {
        listModelFace.Add(modelFace);
    }
    
    public void Detach()
    {
        foreach (var v in listModelFace)
        {
            v.GetComponent<Rigidbody>().WakeUp();
            v.GetComponent<Rigidbody>().AddTorque(Random.onUnitSphere * Random.Range(1f, 2f), ForceMode.Impulse);
        }
    }
}
