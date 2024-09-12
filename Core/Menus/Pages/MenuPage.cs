using UnityEngine;

public class MenuPage : MonoBehaviour
{

    public enum PageType {SaveAndLoad, Config, Help }
    public PageType pageType;

    private const string OPEN = "Open";
    private const string CLOSE = "Close";
    public Animator anim;
    public virtual void Open()
    {
        anim.SetTrigger(OPEN);
    }
    public virtual void Close(bool closedAllMenus=false)
    {
        anim.SetTrigger(CLOSE);
        if (closedAllMenus)
            ProjectMenuManager.instance.CloseRoot();

    }

}

