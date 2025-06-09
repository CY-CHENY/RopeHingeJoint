using DG.Tweening;
using UnityEngine;

public class RopeRotate : MonoBehaviour
{
    public Transform rope;
    // Start is called before the first frame update

    private float speed = 200f;
    private float rotationy = -180;

    void Start()
    {
        //rope.DOLocalRotate(new Vector3(0, 360, 0), 3f, RotateMode.FastBeyond360).SetLoops(-1);
    }

    // Update is called once per frame
    void Update()
    {
        // var rotation = rope.localRotation;
        rotationy += speed * Time.deltaTime;
        if (rotationy >= 180f)
            rotationy = -180f;
        rope.localRotation = Quaternion.Euler(0, rotationy, 0);
    }
}
