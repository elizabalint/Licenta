using PROJECT;
using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public CanvasGroup mainPanel;
    private CanvasGroupController mainCG;
    public static MainMenu instance {  get; private set; }
    //[SerializeField] UIConfirmation uiChoiceMenu;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        mainCG = new CanvasGroupController(this, mainPanel);
    }

    public void ClickStartNewGame()
    {
       //uiChoiceMenu.Show("Start a new game?", new UIConfirmation.ConfirmationButton("Yes", StartNewGame), new  UIConfirmation.ConfirmationButton("No", null));
    }
    private IEnumerator StartingGame()
    {
        mainCG.Hide(speed: 0.3f);
        while(mainCG.isVisible)
            yield return null;
        Project_Configuration.activeConfig.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("FirstAttempt");
    }
    public void StartNewGame()
    {
        ProjectGameSave.activeFile = new ProjectGameSave();
        StartCoroutine(StartingGame());
    }
    public void LoadGame(ProjectGameSave file)
    {
        ProjectGameSave.activeFile = file;
        StartCoroutine(StartingGame());
    }

}
