using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using QFramework;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

public class MoveToSpareCommand : AbstractCommand
{
    private readonly ItemData item;
    private bool up;

    private BlockData blockData;
    // RopeData ropeData;
    CapsuleRope rope;
    GameObject block;
    public MoveToSpareCommand(ItemData item,bool up)
    {
        this.item = item;
        this.up = up;
    }

    protected override async void OnExecute()
    {
        //备用区
        blockData = this.GetModel<RuntimeModel>().SpareBlockItems.FirstOrDefault(v => v.Item == null);
        if (blockData == null)
            return;
        //绳子
        rope = await this.GetSystem<RopeSystem>().CreateRope();
        rope.gameObject.SetActive(false);
        //收集毛线的木桩
        block = await blockData.Block.CreateSlot(item.Color);

        CoroutineController.Instance.StartCoroutine(StartCollecting());
    }

    private IEnumerator StartCollecting()
    {
        var ropeMeshRenderer = rope.GetComponentInChildren<MeshRenderer>();
        Debug.Log($"绳子颜色:{item.Color}");
        ropeMeshRenderer.material.color = Util.ColorMapping[item.Color];

        //大模型中选中的毛线模型
        Transform wool = item.ItemTransform;
        wool.gameObject.SetActive(true);
        yield return null;
        var dissolve = wool.AddComponent<DissolveController>();
        dissolve.InitCtl();
        yield return null;
        blockData.Item = new ItemData() { Color = item.Color, ItemTransform = block.transform };
        
        Camera mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        Camera modelCamera = GameObject.FindWithTag("ModelCamera").GetComponent<Camera>();
        Vector3 screenPos1 = modelCamera.WorldToScreenPoint(wool.transform.position);
        // 将屏幕位置转回世界坐标，但使用renderCamera
        Vector3 worldPos1 = mainCamera.ScreenToWorldPoint(
            new Vector3(screenPos1.x, screenPos1.y, mainCamera.nearClipPlane + 1));
        //从模型到备用区
        var start = worldPos1;
        //Debug.Log($"wool.position = {wool.transform.position} , worldPos1 = {worldPos1} ,screenPos1 = {screenPos1}");
        var position = block.transform.position;
        var end = new Vector3(position.x, position.y, position.z - 0.5f);
        //先从起点附近延长到终点
        Vector3 dir = end - start;
        Vector3 end2 = start + dir.normalized * 0.5f;

        //Debug.Log($"start:{start} end:{end3}");


        dissolve.StartDissolve(0.9f, up, (pos) =>
        {
            if (!rope.gameObject.activeSelf)
            {
                rope.gameObject.SetActive(true);
                rope.UpdatePoint(pos, end2);
                DOTween.To(() => end2, x =>
                    {
                        //Debug.Log(x);
                        end2 = x;
                        rope.UpdatePoint(start, x);
                    }, end, 0.2f);
            }

            rope.UpdateStart(pos);
        });

        this.GetSystem<VibrateSystem>().VibrateShort();
        yield return new WaitForSeconds(0.2f);

        float offset = 0.015f;
        //显示毛线圈2
        var pos = position;
        rope.UpdateEnd(new Vector3(pos.x, pos.y, pos.z - 0.5f));
        var torus = block.transform.GetChild(0).GetChild(0).Find("pTorus2");
        if (torus != null)
        {
            torus.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            torus.gameObject.SetActive(true);
            torus.DOScale(1.0f, 0.1f);
        }
        this.GetSystem<VibrateSystem>().VibrateShort();
        yield return new WaitForSeconds(0.2f);
        //显示毛线圈3
        pos = block.transform.position;
        rope.UpdateEnd(new Vector3(pos.x, pos.y + offset, pos.z - 0.5f));
        torus = block.transform.GetChild(0).GetChild(0).Find("pTorus3");
        if (torus != null)
        {
            torus.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            torus.gameObject.SetActive(true);
            torus.DOScale(1.0f, 0.1f);
        }

        yield return new WaitForSeconds(0.2f);
        //显示毛线圈4
        pos = block.transform.position;
        rope.UpdateEnd(new Vector3(pos.x, pos.y + offset * 2, pos.z - 0.5f));
        torus = block.transform.GetChild(0).GetChild(0).Find("pTorus4");
        if (torus != null)
        {
            torus.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            torus.gameObject.SetActive(true);
            torus.DOScale(1.0f, 0.1f);
        }

        yield return new WaitForSeconds(0.2f);
        //显示毛线圈5
        pos = block.transform.position;
        var targetVec = new Vector3(pos.x, pos.y + offset * 3, pos.z - 0.5f);
        rope.UpdateEnd(targetVec);
        torus = block.transform.GetChild(0).GetChild(0).Find("pTorus5");
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

        var anim = block.transform.GetChild(0).GetComponent<Animator>();
        anim.SetTrigger("rotate");
        yield return new WaitForSeconds(0.2f);

        this.SendEvent<ItemToSparEvent>();
        this.SendCommand<CheckLegoRaiseCommand>();
        Debug.Log("移动到备用区结束");
    }
}