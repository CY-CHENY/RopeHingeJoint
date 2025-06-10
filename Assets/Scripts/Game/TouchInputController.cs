using UnityEngine;
using QFramework;
using System.Linq;
using UnityEngine.EventSystems;

public class TouchInputController : MonoBehaviour, IController
{
    private bool isRotating = false;

    private float dragThreshold = 5f;
    public Camera legoCamera;
    public Camera mainCamera;

    private RuntimeModel model;

    private bool isBegan = false;

    private Vector3 lastPosition;

    void Start()
    {
        model = this.GetModel<RuntimeModel>();
    }
    void Update()
    {
        if (model.Guide.Value)
            return;

        if (!model.GameStart.Value)
            return;

#if UNITY_WEBGL && DOUYINMINIGAME

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (IsTouchOnUI(touch))
                return;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isBegan = true;
                    isRotating = false;
                    break;
                case TouchPhase.Moved:
                    float distance = touch.deltaPosition.magnitude;
                    if (!isRotating && distance > dragThreshold)
                    {
                        isRotating = true;
                    }
                    if (isRotating)
                        HandleDrag(touch.deltaPosition);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isBegan && !isRotating)
                    {
                        HandleClick(touch.position);
                    }
                    isBegan = false;
                    isRotating = false;
                    break;
            }
        }

#else
        if (IsTouchOnUI())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            isBegan = true;
            isRotating = false;
            lastPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            var currentPosition = Input.mousePosition;
            Vector3 deltaPisition = currentPosition - lastPosition;
            lastPosition = currentPosition;
            float distance = deltaPisition.magnitude;
            if (!isRotating && distance > dragThreshold)
            {
                isRotating = true;
            }
            if (isRotating)
                HandleDrag(deltaPisition);
            lastPosition = currentPosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isBegan && !isRotating)
            {
                HandleClick(Input.mousePosition);
            }
            isBegan = false;
            isRotating = false;
        }
#endif

    }

    bool IsTouchOnUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    bool IsTouchOnUI(Touch touch)
    {
        return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
    }

    private void HandleClick(Vector2 position)
    {
        int layerMask = LayerMask.GetMask("Default");
      
        Ray ray2 = mainCamera.ScreenPointToRay(position);
        RaycastHit hit2;
        if (Physics.Raycast(ray2, out hit2, 100))
        {
            Debug.Log("---mainCamera--Hit----- " + hit2.collider.gameObject.name);
            if (hit2.collider.gameObject.GetComponent<Box>() != null)//解锁盒子
            {
                this.SendCommand(new UnlockBoxCommand(hit2.collider.gameObject));
                return;
            }
        }
        
        Ray ray = legoCamera.ScreenPointToRay(position);
        int layerMask2 = LayerMask.GetMask("Lego");
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100,layerMask2))
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            Debug.Log("---modelCamera--Hit----- " + hit.collider.gameObject.name);
            var model = this.GetModel<RuntimeModel>();
            var item = model.AllItems.FirstOrDefault(i => i.ItemTransform == hit.transform);
            if (item != null)
            {
                // Destroy(hit.collider);
                var localPos = hit.transform.InverseTransformPoint(hit.point);
                this.SendCommand(new TryPlaceItemCommand(item, IsModelVerticalByBounds(hit.collider.gameObject)));
            }
        }
    }
    
    bool IsModelVerticalByBounds(GameObject model)
    {
        Renderer renderer = model.GetComponent<Renderer>();
        if (renderer == null) return false;
    
        Bounds bounds = renderer.bounds;
        float height = bounds.size.y;
        float width = bounds.size.x;
        float depth = bounds.size.z;
        Debug.Log(height > width * 1.5f && height > depth * 1.5f);
        // 如果高度明显大于宽度和深度，则认为是竖直的
        return height > width * 1.5f && height > depth * 1.5f;
    }
    

    private void HandleDrag(Vector2 deltaPosition)
    {
        // Debug.Log("HandleDrag");
        this.SendCommand(new RotateModelCommand(deltaPosition.x));
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}