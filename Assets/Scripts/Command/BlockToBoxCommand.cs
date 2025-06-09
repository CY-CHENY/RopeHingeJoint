using System.Collections;
using DG.Tweening;
using QFramework;
using UnityEngine;

public class BlockToBoxCommand : AbstractCommand
{
    private BoxData boxData;
    private BlockData blockData;
    private int boxCount;
    public BlockToBoxCommand(BlockData blockData, BoxData boxData)
    {
        this.blockData = blockData;
        this.boxData = boxData;
    }
    protected override void OnExecute()
    {
        Debug.Log($"box:{boxData.Color} current:{boxData.CurrentCount} blockData:{blockData.Item.Color}");
        boxCount = ++boxData.CurrentCount;

        CoroutineController.Instance.StartCoroutine(BlockToBox());
    }

    IEnumerator BlockToBox()
    {
        Box box = boxData.BoxTransform.GetComponent<Box>();
        var targetVec = box.GetEmptySlot();

        //备用区的毛线
        Transform wood = blockData.Item.ItemTransform;

        wood.DOMove(new Vector3(targetVec.x, targetVec.y, targetVec.z), 0.3f);
        box.SetLego(wood.gameObject);
        yield return new WaitForSeconds(0.3f);

        //毛线圈底座消失
        var torus = wood.transform.GetChild(0).GetChild(0).Find("pTorus1");
        if (torus != null)
            torus.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.05f);
        
        //盒子缩放
        box.transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.15f);
        yield return new WaitForSeconds(0.15f);
        box.transform.DOScale(Vector3.one, 0.15f);
        yield return new WaitForSeconds(0.15f);

        blockData.Item = null;
        if (boxCount >= 3)
        {
            this.SendCommand(new ProcessFullBoxCommand(boxData));
        }
    }
}