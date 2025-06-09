using QFramework;
using Utils;

public interface IGameModel : IModel
{
    public BindableProperty<bool> SceneLoaded { get; }
    public BindableProperty<bool> SceneLoading { get; }
    public BindableProperty<SceneID> LoadingTargetSceneID { get; }
    public BindableProperty<string> Token { get; }
    //public GameType CurGameType{get;set;}

    public BindableProperty<bool> FirstGame {get;}
}

public class GameModel : AbstractModel, IGameModel
{
    protected override void OnInit()
    {
        SceneLoaded.Value = false;
        var storage = this.GetUtility<PlayerPrefsStorage>();
        FirstGame.Value = bool.Parse(storage.LoadString(nameof(FirstGame), "true"));
        FirstGame.Register(guide => storage.SaveString(nameof(FirstGame), guide.ToString()));
    }

    public BindableProperty<bool> SceneLoaded { get; } = new BindableProperty<bool>();
    public BindableProperty<bool> SceneLoading { get; } = new BindableProperty<bool>();
    public BindableProperty<SceneID> LoadingTargetSceneID { get; } = new BindableProperty<SceneID>();
    public BindableProperty<string> Token { get; } = new BindableProperty<string>();
    public BindableProperty<bool> FirstGame{get;} = new BindableProperty<bool>();
    // public GameType CurGameType{get;set;}
}