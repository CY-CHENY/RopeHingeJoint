using UnityEngine;

public struct ResetModelCameraScaleEvent
{

}

public struct ChangeSettingEvent
{

}

public struct AssetsInitEndEvent
{
    public bool isFinish;
}
public struct InitializeFailedEvent { }
public struct PatchStepChangeEvent { public string Tips; }
public struct PackageVersionRequestFailedEvent { public string PackageName; }
public struct PackageManifestUpdateFailedEvent
{
    public string PackageName;
    public string PackageVersion;
}
public struct FoundUpdateFilesEvent
{
    public int TotalCount;
    public long TotalSizeBytes;
}
public struct WebFileDownloadFailedEvent
{
    public string FileName;
    public string Error;
}
public struct DownloadUpdateEvent
{
    public int TotalDownloadCount;
    public int CurrentDownloadCount;
    public long TotalDownloadSizeBytes;
    public long CurrentDownloadSizeBytes;
}
public struct TryInitializeEvent { }
public struct BeginDownloadWebFilesEvent { }
public struct TryUpdatePackageVersionEvent { }
public struct TryUpdatePatchManifestEvent { }
public struct TryDownloadWebFilesEvent { }

public struct FinishDownloadResEvent
{
    public bool isFinish;
}

public struct BrickObjectSpawnedEvent
{
    public GameObject Instnace;
}

public struct CollectLegoEvent
{
    public Material colorMat;
    public int boxIndex;
}

public struct MoveFinishedEvent { }

public struct ItemToSparEvent
{
    public ItemData item;
}

public struct ProcessFullBoxFinishEvent { }

public struct SignInFailedEvent{}

public struct SignInSuccessEvent { }

public struct GetRankDataSuccessEvent{}
public struct GetRankDataFailEvent{}

public struct ClickModelEvent
{
    public Transform transform;
}

public struct LegoRaiseEvent
{
    
}

public struct RopeCreatedEvent
{
    public Vector3 StartPoint;
    public Vector3 EndPoint;
    public GameObject RopeObject;
}

public struct UpdateBoxLayoutEvent{}

public struct CreateStarEvent
{
    public Vector3 pos;
}