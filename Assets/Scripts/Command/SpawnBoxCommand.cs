using QFramework;
using UnityEngine;

public class SpawnBoxCommand : AbstractCommand<BoxData>
{
    private  BoxData data;
    private readonly int index;
    public SpawnBoxCommand(BoxData data, int index)
    {
        this.data = data;
        this.index = index;
    }
    protected override BoxData OnExecute()
    {
        if (index < 0)
            return null;

        var boxObj = Object.Instantiate(this.GetSystem<BoxSystem>().GetBoxPrefab());
        data.BoxTransform = boxObj.transform;
        Box box = boxObj.GetComponent<Box>();
        box.SetData(data);

        this.GetModel<RuntimeModel>().ActiveBoxes.Insert(index, data);
        return data;
    }
}