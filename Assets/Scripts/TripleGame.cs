using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

public class TripleGame : Architecture<TripleGame>
{
    protected override void Init()
    {

        RegisterSystem<IAddressableSystem>(new AddressableSystem());
        RegisterSystem(new ConfigSystem());
        RegisterSystem(new GameLogicSystem());
        RegisterSystem(new BoxSystem());
        RegisterSystem(new RopeSystem());
        RegisterSystem(new LegoSystem());
        RegisterSystem(new AudioSystem());
        RegisterSystem(new SignInSystem());
        RegisterSystem(new AvatarDownloadSystem());
        RegisterSystem(new VibrateSystem());
        RegisterSystem(new ParticlesSystem());
        RegisterSystem(new ColorSystem());

        RegisterModel(new SignInModel());
        RegisterModel(new RotationModel());
        RegisterModel(new ScaleModel());
        RegisterModel(new RuntimeModel());
        RegisterModel(new MainRankingModel());
        RegisterModel<ISettingsModel>(new SettingModel());
        RegisterModel<IGameModel>(new GameModel());
        RegisterModel(new PlayerInfoModel());
        RegisterUtility<IWordFilterService>(new DefaultWordFilterService());
        RegisterUtility(new PlayerPrefsStorage());
        RegisterUtility(new SDKUtility());
        RegisterUtility(new ImageDownloader());

        InitBannedWords();
    }
    
    private void InitBannedWords()
    {
        var textAsset = Resources.Load<TextAsset>("Config/BannedWords");
        if (textAsset != null)
        {
            var words = textAsset.text.Split(
                new[] { "\r\n", "\r", "\n","," }, 
                System.StringSplitOptions.RemoveEmptyEntries);
            
            var filterService = GetUtility<IWordFilterService>();
            filterService.ReloadWords(new List<string>(words));
        }
    }
}
