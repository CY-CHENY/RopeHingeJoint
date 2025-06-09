using System.Linq;
using QFramework;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GuideUI : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
    public Text Content;
    public Image Circle;
    public Image Finger;
    private int guide = 0;

    private int clickCount = 0;
    private bool isRotating = false;
    private float dragThreshold = 5f;

    private Sequence sequenceCircle;
    private Sequence sequenceFinger;

    private Vector2 uiLocalPos;

#if UNITY_WEBGL
    private Vector3 lastPosition;
#endif

    // // Start is called before the first frame update
    void Start()
    {
        this.GetModel<RuntimeModel>().GuideOne.RegisterWithInitValue(OnGuideOneChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void OnGuideOneChanged(GameObject flower)
    {
        Debug.Log("OnGuideOneChanged");
        RectTransform canvasRect = UIController.Instance.canvasRect;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(flower.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, null, out uiLocalPos
        );

        Content.text = "点击相同颜色毛线，完成消除";
        Content.transform.localPosition = new Vector2(uiLocalPos.x, uiLocalPos.y + 700);
        Circle.transform.localPosition = new Vector2(uiLocalPos.x, uiLocalPos.y + 300);
        Circle.transform.localScale = Vector3.one * 2;
        sequenceCircle = DOTween.Sequence();
        sequenceCircle.Append(Circle.transform.DOBlendableScaleBy(new Vector3(0.3f, 0.3f, 0.3f), 0.5f))
        .Append(Circle.transform.DOBlendableScaleBy(new Vector3(-0.3f, -0.3f, -0.3f), 0.5f)).SetLoops(-1);
        Finger.transform.localPosition = new Vector2(200, uiLocalPos.y - 100);
        Vector3 dir = Circle.transform.localPosition - Finger.transform.localPosition;
        Vector3 target = dir.normalized * 100f;
        sequenceFinger = DOTween.Sequence();
        sequenceFinger.Append(Finger.transform.DOBlendableLocalMoveBy(target, 0.5f)).Append(Finger.transform.DOBlendableLocalMoveBy(-target, 0.5f)).SetLoops(-1);
    }

    void OnDisable()
    {
        if (sequenceFinger != null)
            sequenceFinger.Kill();

        sequenceFinger = null;

        if (sequenceCircle != null)
            sequenceCircle.Kill();

        sequenceCircle = null;
    }
    // Update is called once per frame
    void Update()
    {

#if UNITY_WEBGL && DOUYINMINIGAME

if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isRotating = false;
                    break;
                case TouchPhase.Moved:
                    if (guide == 1)
                    {
                        float distance = touch.deltaPosition.magnitude;
                        if (!isRotating && distance > dragThreshold)
                        {
                            isRotating = true;
                        }
                        if (isRotating)
                        {
                            HandleDrag(touch.deltaPosition);

                        }
                    }

                    break;
                case TouchPhase.Ended:
                    {
                        if (isRotating && guide == 1)
                        {
                            guide++;
                        }

                        isRotating = false;
                        if (guide == 0)
                        {
                            HandleClick(touch.position);
                        }
                        else if (guide == 1)
                        {

                        }
                        else
                        {
                            UIController.Instance.HidePage(UIPageType.GuideUI);
                            this.GetModel<RuntimeModel>().Guide.Value = false;
                        }
                    }
                    break;
            }
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            isRotating = false;
            lastPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            if (guide == 1)
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
                {
                    HandleDrag(deltaPisition);
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isRotating && guide == 1)
            {
                guide++;
            }

            isRotating = false;
            if (guide == 0)
            {
                HandleClick(Input.mousePosition);
            }
            else if (guide == 1)
            {

            }
            else
            {
                UIController.Instance.HidePage(UIPageType.GuideUI);
                this.GetModel<RuntimeModel>().Guide.Value = false;
            }
        }
#endif


    }

    private void HandleDrag(Vector2 deltaPosition)
    {
        // Debug.Log("HandleDrag");
        this.SendCommand(new RotateModelCommand(deltaPosition.x));
    }

    private void HandleClick(Vector2 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            Debug.Log("Hit " + hit.collider.gameObject.name);

            var model = this.GetModel<RuntimeModel>();
            var item = model.AllItems.FirstOrDefault(i => i.ItemTransform == hit.transform);
            if (item != null)
            {
                clickCount++;
                var localPos = hit.transform.InverseTransformPoint(hit.point);
                this.SendCommand(new TryPlaceItemCommand(item, localPos.y > 0));
            }
        }

        if (clickCount >= 3)
        {
            guide++;
            if (sequenceCircle != null)
                sequenceCircle.Kill();
            sequenceFinger = null;

            if (sequenceFinger != null)
                sequenceFinger.Kill();
            sequenceFinger = null;
            Content.text = "旋转物体，继续消除毛线";
            Circle.SetActive(false);
            Finger.transform.localPosition = new Vector3(-100, uiLocalPos.y - 100);
            sequenceFinger = DOTween.Sequence();
            sequenceFinger.Append(Finger.transform.DOLocalMove(new Vector2(300, uiLocalPos.y - 100), 1f))
            .Append(Finger.transform.DOLocalMove(new Vector2(-100, uiLocalPos.y - 100), 1f)).SetLoops(-1);
        }
    }
}
