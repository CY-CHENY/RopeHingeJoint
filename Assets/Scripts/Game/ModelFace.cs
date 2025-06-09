using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ModelFace : BaseController
{
    
    public void Init()
    {
        var hinge = gameObject.GetComponents<HingeJoint>();
        for (int i = 0; i < hinge.Length; i++)
        {
            ModelRope modelRope = hinge[i].connectedBody.gameObject.GetComponent<ModelRope>();
            if(modelRope!=null) modelRope.AddToModelFace(this);
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Finish"))
        {
            Destroy(gameObject);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Finish"))
        {
            Destroy(gameObject);
        }
    }
    
}
