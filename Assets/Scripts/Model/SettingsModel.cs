using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using QFramework;
using UnityEngine;

[Serializable]
public class SystemSettingsInfo : ISerializable
{

   // public bool isOnMusic;
    public bool isOnSound;
    public bool isOnVibration;
    private static string fileName = "settings.stf";

    public SystemSettingsInfo() { }

    private SystemSettingsInfo(SerializationInfo info, StreamingContext ctxt)
    {
       // isOnMusic = info.GetBoolean("isOnMusic");
        isOnSound = info.GetBoolean("isOnSound");
        isOnVibration = info.GetBoolean("isOnVibration");
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
       // info.AddValue("isOnMusic", isOnMusic);
        info.AddValue("isOnSound", isOnSound);
        info.AddValue("isOnVibration", isOnVibration);
    }

    public static void SaveSystemInfo(SystemSettingsInfo info)
    {
        Stream stream = File.Open(Path.Combine(Application.persistentDataPath, fileName), FileMode.Create);
        var bf = new BinaryFormatter();
        bf.Serialize(stream, info);
        stream.Close();
    }

    public static void DelFile()
    {
        File.Delete(Path.Combine(Application.persistentDataPath, fileName));
    }

    public static SystemSettingsInfo ParseSystemInfo()
    {
        Stream stream = File.Open(Path.Combine(Application.persistentDataPath, fileName), FileMode.OpenOrCreate);
        BinaryFormatter bf = new BinaryFormatter();
        SystemSettingsInfo info = null;
        try
        {
            info = (SystemSettingsInfo)bf.Deserialize(stream);
        }
        catch
        {

        }
        stream.Close();
        return info;
    }

    public SystemSettingsInfo Clone()
    {
        SystemSettingsInfo info = new SystemSettingsInfo();
       // info.isOnMusic = isOnMusic;
        info.isOnSound = isOnSound;
        info.isOnVibration = isOnVibration;
        return info;
    }

    public bool EqualTo(SystemSettingsInfo info)
    {
        return /*info.isOnMusic == isOnMusic &&*/
        info.isOnSound == isOnSound &&
        info.isOnVibration == isOnVibration;
    }
}

public interface ISettingsModel : IModel
{
   // public BindableProperty<bool> IsOnMusic { get; }
    public BindableProperty<bool> IsOnSound { get; }
    public BindableProperty<bool> IsOnVibration { get; }

    public void SaveSystemInfo(SystemSettingsInfo info);
    public void DelFile();
}

public class SettingModel : AbstractModel, ISettingsModel
{
   // public BindableProperty<bool> IsOnMusic { get; } = new BindableProperty<bool>();

    public BindableProperty<bool> IsOnSound { get; } = new BindableProperty<bool>();

    public BindableProperty<bool> IsOnVibration { get; } = new BindableProperty<bool>();

    private void SetContent(SystemSettingsInfo info)
    {
       // IsOnMusic.Value = info.isOnMusic;
        IsOnSound.Value = info.isOnSound;
        IsOnVibration.Value = info.isOnVibration;
    }

    public void DelFile()
    {
        SystemSettingsInfo.DelFile();
    }

    public void SaveSystemInfo(SystemSettingsInfo info)
    {
        SetContent(info);
        SystemSettingsInfo.SaveSystemInfo(info);
    }

    protected override void OnInit()
    {
        SystemSettingsInfo settingsInfo = SystemSettingsInfo.ParseSystemInfo();
        if (settingsInfo == null)
        {
            settingsInfo = new SystemSettingsInfo();
            // settingsInfo.isOnMusic = true;
            settingsInfo.isOnSound = true;
            settingsInfo.isOnVibration = true;
            SystemSettingsInfo.SaveSystemInfo(settingsInfo);
        }

        SetContent(settingsInfo);
    }
}