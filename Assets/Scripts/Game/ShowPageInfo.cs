public class ShowPageInfo
{
    public UIPageType pageType;
    public bool closeOther;
    public UILevelType levelType;
    public object data;
    public bool isLocal;

    public ShowPageInfo(UIPageType pageType, bool closeOther = false, UILevelType levelType = UILevelType.UIPage,
        bool isLocal = false)
    {
        this.pageType = pageType;
        this.closeOther = closeOther;
        this.levelType = levelType;
        this.data = null;
        this.isLocal = isLocal;
    }

    public ShowPageInfo(UIPageType pageType, UILevelType levelType, object data, bool isLocal = false)
    {
        this.pageType = pageType;
        this.closeOther = false;
        this.levelType = levelType;
        this.data = data;
        this.isLocal = isLocal;
    }

    public ShowPageInfo(UIPageType pageType, UILevelType levelType, bool isLocal = false)
    {
        this.pageType = pageType;
        this.closeOther = false;
        this.levelType = levelType;
        this.data = null;
        this.isLocal = isLocal;
    }

    public ShowPageInfo(UIPageType pageType, bool closeOther, UILevelType levelType, object data, bool isLocal)
    {
        this.pageType = pageType;
        this.closeOther = closeOther;
        this.levelType = levelType;
        this.data = data;
        this.isLocal = isLocal;
    }
}