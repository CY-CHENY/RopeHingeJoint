using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using QFramework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

public class Box : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }

    public Transform[] slots;
    private List<GameObject> legos = new List<GameObject>();

    private BoxType type;
    private ItemColor color;

    private SpriteRenderer spriteRenderer;
    private GameObject lightObj;

    void Awake()
    {
        var cheng = transform.Find("cheng");
        if (cheng != null)
        {
            spriteRenderer = cheng.GetComponent<SpriteRenderer>();
        }

        var light = transform.Find("light");
        if(light != null)
        {
            lightObj = light.gameObject;
            lightObj.SetActive(false);
        }
    }
    public void SetData(BoxData data)
    {
        type = data.Type;
        color = data.Color;

        UpdateDisplay();
    }

    private async void UpdateDisplay()
    {
        if (type == BoxType.Advertisement)
        {
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Sprite>($"Assets/GameResources/Sprites/Box/Unlock.png");
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                spriteRenderer.sprite = Instantiate(obj.Result);
            }

            this.AddComponent<BoxCollider>().isTrigger = true;
        }
        else if (type == BoxType.Normal)
        {
            //spriteRenderer.sprite = this.GetSystem<IYooAssetsSystem>().LoadAssetSync<Sprite>("Box_" + GetFileName(color));
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Sprite>($"Assets/GameResources/Sprites/Box/{GetFileName(color)}.png");
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                spriteRenderer.sprite = Instantiate(obj.Result);
            }
        }
        else if (type == BoxType.Super)
        {
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Sprite>($"Assets/GameResources/Sprites/Box/Super.png");
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                spriteRenderer.sprite = Instantiate(obj.Result);
            }
            lightObj.SetActive(true);
            lightObj.transform.DORotate(new Vector3(0, 0, 360),20f, RotateMode.FastBeyond360).SetLoops(-1);
        }
    }

    public static string GetFileName(ItemColor color)
    {
        switch (color)
        {
            case ItemColor.Red:
                return nameof(ItemColor.Red);
            case ItemColor.Blue:
                return nameof(ItemColor.Blue);
            case ItemColor.Green:
                return nameof(ItemColor.Green);
            case ItemColor.Yellow:
                return nameof(ItemColor.Yellow);
            // case ItemColor.Cyan:
            //     return nameof(ItemColor.Cyan);
            case ItemColor.Purple:
                return nameof(ItemColor.Purple);
            case ItemColor.Orange:
                return nameof(ItemColor.Orange);
            case ItemColor.White:
                return nameof(ItemColor.White);
            case ItemColor.Black:
                return nameof(ItemColor.Black);
            default:
                return "Unlock";
        }
    }

    public Vector3 GetEmptySlot()
    {
        if (legos.Count >= 3)
            return Vector3.zero;

        return slots[legos.Count].position;
    }

    public async UniTask<GameObject> CreateSlot(ItemColor superBoxColor = ItemColor.None)
    {
        Material woolMatPrefab = null;
        Material woodMatPrefab = null;
        GameObject woodPrefab = null;

        var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Material>("Wool");
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            woolMatPrefab = obj.Result;
        }

        var obj1 = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Material>("WoodMat");
        if (obj1.Status == AsyncOperationStatus.Succeeded)
        {
            woodMatPrefab = obj1.Result;
        }

        var obj2 = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<GameObject>("Wood");
        if (obj2.Status == AsyncOperationStatus.Succeeded)
        {
            woodPrefab = obj2.Result;
        }

        // var woolMatPrefab = this.GetSystem<IYooAssetsSystem>().LoadAssetSync<Material>("WoodMat");
        // var woodPrefab = this.GetSystem<IYooAssetsSystem>().LoadAssetSync<GameObject>("DGLXX_wood");
        GameObject slot = Instantiate(woodPrefab);
        slot.transform.localRotation = Quaternion.Euler(new Vector3(-44, 0, 0));
        var position = GetEmptySlot();
        slot.transform.position = new Vector3(position.x, position.y + 0.2f, position.z);
        slot.transform.localScale = new Vector3(0.72f, 0.72f, 0.72f);
        var model = slot.transform.GetChild(0).GetChild(0);
        for (int i = 0; i < model.childCount; i++)
        {
            Transform child = model.GetChild(i);
            if (!child.name.Equals("pCube_a") && !child.name.Equals("pTorus1"))
            {
                child.gameObject.SetActive(false);

                var instanceMat = Instantiate(woolMatPrefab);
                var meshRender = child.GetComponent<MeshRenderer>();
                meshRender.material = instanceMat;
                meshRender.material.SetColor("_BaseColor", Util.ColorMapping[superBoxColor == ItemColor.None ? color : superBoxColor]);
            }
            else
            {
                var instanceMat = Instantiate(woodMatPrefab);
                var meshRender = child.GetComponent<MeshRenderer>();
                meshRender.material = instanceMat;

                child.gameObject.SetActive(true);
            }
        }
        SetLego(slot);
        return slot;
    }

    public void SetLego(GameObject lego)
    {
        legos.Add(lego);
        //Debug.Log($"box scale:{transform.localScale} --> wood scale:{lego.transform.localScale}");
        //盒子有可能在播缩放动画，让木桩的scale正常显示就要乘以盒子的scale(盒子默认scale必须是1)
        lego.transform.localScale = lego.transform.localScale * transform.localScale.x;
        lego.transform.SetParent(transform,true);
    }

    // public void ClearLegos()
    // {
    //     foreach (var lego in legos)
    //     {
    //         Destroy(lego);
    //     }
    //     legos.Clear();
    // }

}