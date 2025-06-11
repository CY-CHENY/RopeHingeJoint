using System.Linq;
using QFramework;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using Utils;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

public class ProcessFullBoxCommand : AbstractCommand
{
    private readonly BoxData box;
    private readonly ItemColor targetColor;

    private Sprite coverPrefab;

    public ProcessFullBoxCommand(BoxData box, ItemColor targetColor = ItemColor.None)
    {
        this.box = box;
        this.targetColor = targetColor;
    }

    protected override async void OnExecute()
    {
        Debug.Log("ProcessFullBoxCommand");

        if (box.Type == BoxType.Super)
        {
            CoroutineController.Instance.StartCoroutine(RemoveBox());
            return;
        }

        //盖子
        //var sprite = this.GetSystem<IYooAssetsSystem>().LoadAssetSync<Sprite>("Box_" + Box.GetFileName(box.Color));

        if (box.Type == BoxType.Normal)
        {
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Sprite>
                ($"Assets/GameResources/Sprites/Box/{Box.GetFileName(box.Color)}1.png");
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                coverPrefab = obj.Result;
            }

        }

        CoroutineController.Instance.StartCoroutine(StartCoroutine());
    }

    private IEnumerator RemoveBox()
    {
        box.BoxTransform.SetParent(null);
        box.BoxTransform.DOBlendableMoveBy(new Vector3(0, 2f, 0), 0.3f);

        yield return new WaitForSeconds(0.3f);
        yield return null;
        Debug.Log("移除旧盒子");
        Object.Destroy(box.BoxTransform.gameObject);
        this.GetModel<RuntimeModel>().ActiveBoxes.Remove(box);
        this.SendEvent<UpdateBoxLayoutEvent>();
    }

    private IEnumerator StartCoroutine()
    {
        var model = this.GetModel<RuntimeModel>();

        if (box.Type == BoxType.Normal)
        {
            var cover = new GameObject("cover");
            var spriteRender = cover.AddComponent<SpriteRenderer>();
            spriteRender.sprite = Object.Instantiate(coverPrefab);
            spriteRender.sortingOrder = 1;
            cover.transform.SetParent(box.BoxTransform);
            cover.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            cover.transform.localPosition = new Vector3(0, 5f, -0.76f);
            cover.transform.DOLocalMoveY(0.17f, 0.2f);

            yield return new WaitForSeconds(0.2f);

            box.BoxTransform.DOScaleY(0.9f, 0.1f);
            yield return new WaitForSeconds(0.1f);

            //生成闪光粒子
            GameObject flash = this.GetSystem<ParticlesSystem>().GetBaoFa();
            flash.transform.position = box.BoxTransform.position;
            
            // 生成星星
            this.SendEvent(new CreateStarEvent { pos = box.BoxTransform.position });
        }


        //移除旧盒子
        int index = model.ActiveBoxes.IndexOf(box);
        model.ActiveBoxes.Remove(box);
        box.BoxTransform.SetParent(null);
        box.BoxTransform.DOBlendableMoveBy(new Vector3(0, 2f, 0), 0.2f);
        yield return new WaitForSeconds(0.2f);
        yield return null;
        Debug.Log("移除旧盒子");
        Object.Destroy(box.BoxTransform.gameObject);
        
        //生成新盒子
        var newBoxData = model.GetBoxData();// GetNextBoxData();
        if (newBoxData != null)
        {
            var newBox = this.SendCommand(new SpawnBoxCommand(newBoxData, index));
            //等新盒子动画完成
            yield return new WaitForSeconds(0.3f);
            //检查备用区是否有匹配物品
            var matchedItems = model.SpareBlockItems.Where(i => (i.Item != null && i.Item.Color == newBox.Color)).Take(3 - newBox.CurrentCount).ToList();
            Debug.Log("找到匹配区可用:" + matchedItems.Count);
            foreach (var item in matchedItems)
            {
                this.SendCommand(new BlockToBoxCommand(item, newBox));
            }
        }
        else
        {
            this.SendEvent<UpdateBoxLayoutEvent>();
        }

        this.SendEvent(new ProcessFullBoxFinishEvent { });
    }

    // private BoxData GetNextBoxData()
    // {
    //     var model = this.GetModel<RuntimeModel>();
    //     if (model.BoxQueue.Count <= 0)
    //         return null;

    //     if (targetColor == ItemColor.None)
    //         return model.GetBoxData();

    //     var data = model.BoxQueue.FirstOrDefault(b => b.Color == targetColor);
    //     return data;
    // }
}