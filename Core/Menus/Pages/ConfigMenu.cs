using DIALOGUE;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfigMenu : MenuPage
{
    public static ConfigMenu instance {  get; private set; }    
    [SerializeField] private GameObject[] panels;
    private GameObject activePanel;
    public UI_ITEMS ui;
    private Project_Configuration config => Project_Configuration.activeConfig;
    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == 0);
        }
        activePanel = panels[0];
        SetAvailableRevolution();
        LoadConfig();
    }

    private void LoadConfig()
    {
        if(File.Exists(Project_Configuration.filePath))
            Project_Configuration.activeConfig = FileManager.Load<Project_Configuration>(Project_Configuration.filePath, encrypt:Project_Configuration.ENCRYPT);
        else
            Project_Configuration.activeConfig = new Project_Configuration();
        Project_Configuration.activeConfig.Load();
    
    }

    private void OnAplicationQuit()
    {
        Project_Configuration.activeConfig.Save();
        Project_Configuration.activeConfig = null;
    }


    public void OpenPanel(string panelName)
    {
        GameObject panel = panels.First(p => p.name.ToLower() == panelName.ToLower());
        if(panel == null )
        {
            Debug.LogWarning($"did not find panel called '{panelName}' in config menu");
            return;
        }

        if(activePanel != null && activePanel!=panel)
        {
            activePanel.SetActive(false);
        }
        panel.SetActive(true);
        activePanel = panel;


    }





    public void SetAvailableRevolution()
    {
        Resolution[] resolutions = Screen.resolutions;
        List<string> options = new List<string>();
        for(int i = resolutions.Length-1; i >= 0;i--)
        {
            options.Add($"{resolutions[i].width}x{resolutions[i].height}");
        }
        ui.resolution.ClearOptions();
        ui.resolution.AddOptions(options);
    }

    [System.Serializable]
    public class UI_ITEMS
    {
        private static Color button_selectedColor = new Color(0.4862745f, 0.3686275f, 0.2705882f, 1);
        private static Color button_unselectedColor = new Color(1f, 1f,1f, 1f);
        private static Color text_selectedColor = new Color(0.2301886f, 0.07911804f, 0, 1);
        private static Color text_unselectedColor = new Color(0.25f, 0.25f, 0.25f, 1);
        [Header("General")]
        public Button fullscreen; 
        public Button windowed;
        public TMP_Dropdown resolution;
        public Button skippiongContinue;
        public Button skippingStop;
        public Slider architectSpeed;
        public Slider autoReadeerSpeed;
        [Header("Audio")]
        public Slider musicVolume;
        public Slider sfxVolume;
        public Slider voiceVolume;
        // in Project_Config si mai jos aici
        public void SetButtonColors(Button A, Button B, bool selectedA)
        {
            A.GetComponent<Image>().color = selectedA ? button_selectedColor : button_unselectedColor;
            B.GetComponent<Image>().color = !selectedA ? button_selectedColor : button_unselectedColor;
            
            A.GetComponentInChildren<TextMeshProUGUI>().color = selectedA ? text_selectedColor : text_unselectedColor;
            B.GetComponentInChildren<TextMeshProUGUI>().color = !selectedA ?text_selectedColor : text_unselectedColor;
        }
    }
    
    //ui callable functions
    public void SetDisplayToFullScreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        ui.SetButtonColors(ui.fullscreen, ui.windowed, fullscreen);
    }
    public void SetDisplayResolution()
    {
        string resolution = ui.resolution.captionText.text;
        //Debug.Log($"resolution: {resolution}");
        string[] values = resolution.Split('x');

        if (int.TryParse(values[0], out int width) && int.TryParse(values[1], out int height))
        {

            Screen.SetResolution(width, height, Screen.fullScreen);
            config.display_resolution = resolution;
        }
        else
        {
            Debug.LogError($"parsing error for screen resolution . [{resolution}] could not be parsed into WIDTGxHEIGHT");
        }
    }


    public void SetContinueSkippingAfterChoice(bool continueSkipping)
    {
        config.continueSkippingAfterChoice = continueSkipping;
        ui.SetButtonColors(ui.skippiongContinue, ui.skippingStop, continueSkipping);

    }
    public void SetTextArchtextSpeed()
    {
        Debug.Log($"config.dialogueTextSpeed: {config.dialogueTextSpeed}");
        config.dialogueTextSpeed = ui.architectSpeed.value;
        
        if(DialogueSystem.instance != null) 
            DialogueSystem.instance.conversationManager.architect.speed = config.dialogueTextSpeed;
    }
    public void SetAutoReaderSpeed()
    {
        config.dialogueAutoReadSpeed = ui.autoReadeerSpeed.value;
        if (DialogueSystem.instance == null)
            return;
        AutoReader autoReader = DialogueSystem.instance.autoReader;
        if(autoReader != null)
            autoReader.speed = config.dialogueAutoReadSpeed;
    }

}
