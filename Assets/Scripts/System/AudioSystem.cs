using System.Threading.Tasks;
using QFramework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AudioSystem : AbstractSystem
{
    /// <summary>
    /// 背景音乐优先级
    /// </summary>
    private int BackgroundPriority = 0;

    /// <summary>
    /// 单通道音效优先级
    /// </summary>
    private int SinglePriority = 10;

    private float backgroundVolume = 0.6f;

    /// <summary>
    /// 背景音乐音量
    /// </summary>
    private float BackgroundVolume
    {
        get => backgroundVolume;
        set
        {
            backgroundVolume = value;
            _backgroundAudio.volume = value;
        }
    }

    /// <summary>
    /// 音效音量
    /// </summary>
    public float SoundEffectVolume = 1;

    private AudioSource _backgroundAudio;
    private AudioSource _singleAudio;
    private Transform root;
    private bool isOnSound;

    protected override void OnInit()
    {
        root = new GameObject(nameof(AudioSystem)).transform;
        _backgroundAudio = CreateAudioSource("BackgroundAudio", BackgroundPriority, backgroundVolume);
        _singleAudio = CreateAudioSource("SingleAudio", SinglePriority, SoundEffectVolume);
        UnityEngine.GameObject.DontDestroyOnLoad(root);

        this.RegisterEvent<ChangeSettingEvent>((e) =>
        {
            SetSound(this.GetModel<ISettingsModel>().IsOnSound.Value);
            SetBackgroundMusic(this.GetModel<ISettingsModel>().IsOnSound.Value);
            //SetBackgroundMusic(this.GetModel<ISettingsModel>().IsOnMusic.Value);
        });

        SetSound(this.GetModel<ISettingsModel>().IsOnSound.Value);
        SetBackgroundMusic(this.GetModel<ISettingsModel>().IsOnSound.Value);
    }

    public void SetSound(bool isOn)
    {
        isOnSound = isOn;
        _singleAudio.mute = !isOn;
    }

    public void SetBackgroundMusic(bool isOn)
    {
        if (_backgroundAudio.mute == !isOn)
            return;
        _backgroundAudio.mute = !isOn;
        if (isOn)
            PlayBackgroundMusic();
    }

    /// <summary>
    /// 创建一个音源
    /// </summary>
    private AudioSource CreateAudioSource(string name, int priority, float volume)
    {
        GameObject audioObj = new GameObject(name);
        audioObj.transform.SetParent(root);
        audioObj.transform.localPosition = Vector3.zero;
        audioObj.transform.localRotation = Quaternion.identity;
        audioObj.transform.localScale = Vector3.one;
        AudioSource audio = audioObj.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.priority = priority;
        audio.volume = volume;
        audio.mute = !isOnSound;
        return audio;
    }

    public void PlayBackgroundMusic()
    {
        _backgroundAudio.Play();
    }

    public async void PlayBackgroundMusic(string clipName)
    {
        //var clip = this.GetSystem<IYooAssetsSystem>().LoadAssetSync<AudioClip>($"Assets/GameResources/Audio/{clipName}");
        var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<AudioClip>($"Assets/GameResources/Audio/{clipName}.mp3");
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            if (_backgroundAudio.isPlaying)
            {
                _backgroundAudio.Stop();
            }

            _backgroundAudio.clip = obj.Result;
            _backgroundAudio.loop = true;
            _backgroundAudio.pitch = 1.0f;
            _backgroundAudio.spatialBlend = 0;
            _backgroundAudio.Play();
        }

    }

    public async void PlaySingleSound(string clipName, bool isLoop = false, float speed = 1)
    {
        if (!isOnSound) return;

        //var clip = this.GetSystem<IYooAssetsSystem>().LoadAssetSync<AudioClip>($"Assets/GameResources/Audio/{clipName}");
        var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<AudioClip>($"Assets/GameResources/Audio/{clipName}.mp3");
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            if (_singleAudio.isPlaying)
            {
                _singleAudio.Stop();
            }

            _singleAudio.clip = obj.Result;
            _singleAudio.loop = isLoop;
            _singleAudio.pitch = speed;
            _singleAudio.spatialBlend = 0;
            _singleAudio.Play();
        }

    }
}