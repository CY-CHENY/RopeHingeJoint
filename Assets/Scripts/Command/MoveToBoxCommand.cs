using System.Collections;
using DG.Tweening;
using QFramework;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

public class MoveToBoxCommand : AbstractCommand
{
    private readonly ItemData itemData;
    private readonly BoxData boxData;
    private readonly bool up;
    private int count;
    CapsuleRope rope;
    Box box;
    GameObject wood;
    public MoveToBoxCommand(ItemData item, BoxData boxData, bool up)
    {
        itemData = item;
        this.boxData = boxData;
        this.up = up;
    }

    protected override async void OnExecute()
    {
        Debug.Log("MoveToBoxCommand");
        count = ++boxData.CurrentCount;
        this.SendEvent(new ClickModelEvent { transform = itemData.ItemTransform });
        //绳子
        rope = await this.GetSystem<RopeSystem>().CreateRope();
        rope.gameObject.SetActive(false);
        //盒子
        box = boxData.BoxTransform.GetComponent<Box>();
        //收集毛线的木桩
        wood = await box.CreateSlot(itemData.Color);
        CoroutineController.Instance.StartCoroutine(StartCollecting());
    }

    
    IEnumerator StartCollecting()
    {
        var ropeMeshRenderer = rope.GetComponentInChildren<MeshRenderer>();
        Debug.Log($"木桩收集绳子颜色:{itemData.Color}");
        ropeMeshRenderer.material.color = Util.ColorMapping[itemData.Color];

        //目标模型
        Transform wool = itemData.ItemTransform;
        wool.gameObject.SetActive(true);
        yield return null;
        var dissolve = wool.AddComponent<DissolveController>();
        dissolve.InitCtl();
        yield return null;
        
        Camera mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        Camera modelCamera = GameObject.FindWithTag("ModelCamera").GetComponent<Camera>();
        Debug.Log($"mainCamera = {mainCamera.depth} , modelCamera = {modelCamera.depth}");
        Vector3 screenPos1 = modelCamera.WorldToScreenPoint(wool.transform.position);
        // 将屏幕位置转回世界坐标，但使用renderCamera
        Vector3 worldPos1 = mainCamera.ScreenToWorldPoint(
            new Vector3(screenPos1.x, screenPos1.y, mainCamera.nearClipPlane + 1));
        //从模型到盒子
        var start = worldPos1;//模型位置
        var position = wood.transform.position;//木桩位置
        var end = new Vector3(position.x, position.y, position.z - 0.5f);
        //先从起点附近延长到终点
        Vector3 dir = end - start;
        Vector3 end2 = start + dir.normalized * 0.5f;

        dissolve.StartDissolve(0.9f, up, (vector3) =>
        {
            if (!rope.gameObject.activeSelf)
            {
                rope.gameObject.SetActive(true);
                rope.UpdatePoint(vector3, end2);
                DOTween.To(() => end2, x =>
                            {
                                //Debug.Log(x);
                                end2 = x;
                                rope.UpdateEnd(x);
                            }, end, 0.2f);
            }
            rope.UpdateStart(vector3);
        });

        this.GetSystem<VibrateSystem>().VibrateShort();//手机抖动
        yield return new WaitForSeconds(0.2f);
        float offset = 0.015f;
        //显示毛线圈2
        var pos = position;
        rope.UpdateEnd(new Vector3(pos.x, pos.y, pos.z - 0.5f));
        var torus = wood.transform.GetChild(0).GetChild(0).Find("pTorus2");
        if (torus != null)
        {
            torus.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            torus.gameObject.SetActive(true);
            torus.DOScale(1.0f, 0.1f);
        }
        this.GetSystem<VibrateSystem>().VibrateShort();
        yield return new WaitForSeconds(0.2f);
        //显示毛线圈3
        pos = wood.transform.position;
        rope.UpdateEnd(new Vector3(pos.x, pos.y + offset, pos.z - 0.5f));
        torus = wood.transform.GetChild(0).GetChild(0).Find("pTorus3");
        if (torus != null)
        {
            torus.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            torus.gameObject.SetActive(true);
            torus.DOScale(1.0f, 0.1f);
        }

        yield return new WaitForSeconds(0.2f);
        //显示毛线圈4
        pos = wood.transform.position;
        rope.UpdateEnd(new Vector3(pos.x, pos.y + offset * 2, pos.z - 0.5f));
        torus = wood.transform.GetChild(0).GetChild(0).Find("pTorus4");
        if (torus != null)
        {
            torus.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            torus.gameObject.SetActive(true);
            torus.DOScale(1.0f, 0.1f);
        }

        yield return new WaitForSeconds(0.2f);
        //显示毛线圈5
        pos = wood.transform.position;
        var targetVec = new Vector3(pos.x, pos.y + offset * 3, pos.z - 0.5f);
        rope.UpdateEnd(targetVec);
        torus = wood.transform.GetChild(0).GetChild(0).Find("pTorus5");
        if (torus != null)
        {
            torus.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            torus.gameObject.SetActive(true);
            torus.DOScale(1.0f, 0.1f);
        }

        yield return new WaitForSeconds(0.1f);

        //收绳子
        DOTween.To(() => start, x =>
        {
            rope.UpdateStart(x);
        },
            new Vector3(targetVec.x, targetVec.y - 0.5f, targetVec.z), 0.2f
        ).OnComplete(() =>
        {
            rope.gameObject.SetActive(false);
        });

        yield return new WaitForSeconds(0.2f);

        //毛线圈整体下移
        wood.transform.DOBlendableLocalMoveBy(new Vector3(0, -0.2f, 0), 0.3f);
        var anim = wood.transform.GetChild(0).GetComponent<Animator>();
        anim.SetTrigger("rotate");

        yield return new WaitForSeconds(0.25f);
        //毛线圈底座消失
        torus = wood.transform.GetChild(0).GetChild(0).Find("pTorus1");
        if (torus != null)
            torus.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.05f);
        //盒子缩放
        // DOTween.IsTweening(box.transform)
        box.transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.15f);
        yield return new WaitForSeconds(0.15f);
        box.transform.DOScale(Vector3.one, 0.15f);
        yield return new WaitForSeconds(0.15f);
        Debug.Log("移动到盒子:" + count);

        if (count >= 3)
        {
            this.SendCommand(new ProcessFullBoxCommand(boxData));
        }

        this.SendCommand(new CheckLegoRaiseCommand());
        yield return null;
    }
}