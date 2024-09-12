using System.Linq;
using UnityEngine;

public class ProjectMenuManager : MonoBehaviour
{
    public static ProjectMenuManager instance;
    private MenuPage activePage=null;
    private bool isOpen = false; 
    [SerializeField] private CanvasGroup root;
    [SerializeField] private MenuPage[] pages;
    private CanvasGroupController rootCG;
    //private UIConfirmation uiChoiceMenu => UIConfirmation.instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        rootCG = new CanvasGroupController(this, root);
    }
    private MenuPage GetPage(MenuPage.PageType pageType)
    {
        return pages.FirstOrDefault(page => page.pageType == pageType);

    }
    public void OpenSavePage()
    {
        var page = GetPage(MenuPage.PageType.SaveAndLoad);
        var slm = page.anim.GetComponentInParent<SaveAndLoadMenu>();
        slm.menuFunction = SaveAndLoadMenu.MenuFunction.save;
        OpenPage(page);
    }
    public void OpenLoadPage()
    {
        var page = GetPage(MenuPage.PageType.SaveAndLoad);
        var slm = page.anim.GetComponentInParent<SaveAndLoadMenu>();
        slm.menuFunction = SaveAndLoadMenu.MenuFunction.load;
        OpenPage(page); 

    }
    public void OpenConfigPage()
    {
        var page = GetPage(MenuPage.PageType.Config);
        OpenPage(page);

    }
    public void OpenHelpPage()
    {

        var page = GetPage(MenuPage.PageType.Help);
        OpenPage(page);

    }
    private void OpenPage(MenuPage page)
    {
        if (page == null)
        {
            return;
        }
        if (activePage != null & activePage != page)
        {
            activePage.Close();
        }
        page.Open();
        activePage = page;
        if(!isOpen)
            OpenRoot();
    }

    public void OpenRoot()
    {
        rootCG.Show();
        rootCG.SetInteractableState(true);
        isOpen = true;
    }
    public void CloseRoot()
    {
        rootCG.Hide();
        rootCG.SetInteractableState(false);

        isOpen = false;
    }
    public void Click_Home()
    {
        Project_Configuration.activeConfig.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
    }
    public void Click_Quit()
    {
        //uiChoiceMenu.Show("Quit in desktop?", new UIConfirmation.ConfirmationButton("Yes",()=> Application.Quit()), new UIConfirmation.ConfirmationButton("No", null));
        Application.Quit();
    }

}
