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
            Rigidbody rigidbody = v.GetComponent<Rigidbody>();
            rigidbody.WakeUp();
            rigidbody.drag = 1;
            rigidbody.GetComponent<BoxCollider>().isTrigger = false;
            rigidbody.AddTorque(Random.onUnitSphere * Random.Range(1f, 2f), ForceMode.Impulse);
        }
    }
}
