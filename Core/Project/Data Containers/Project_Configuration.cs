using UnityEngine;

[System.Serializable]
public class Project_Configuration 
{
    public static Project_Configuration activeConfig;
    public static string filePath => $"{FilePaths.root}projectconfig.cfg";
    public const bool ENCRYPT = false;

    //general settings
    public bool display_fullscreen = true;
    public string display_resolution = "1920x1080";
    public bool continueSkippingAfterChoice = false;
    public float dialogueTextSpeed = 1f;
    public float dialogueAutoReadSpeed = 1f;

    //audio settings
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public float voiceVolume = 1f;
    public bool musicMute = false;
    public bool sfxMute = false;
    public bool voiceMute = false;

    public float historyLogScale = 1f;

    public void Load()
    {
        var ui = ConfigMenu.instance.ui;

        //set size window
        ConfigMenu.instance.SetDisplayToFullScreen(display_fullscreen);
        ui.SetButtonColors(ui.fullscreen, ui.windowed, display_fullscreen);

        //screen resolution 
        int res_index = 0;
        for(int i=0; i<ui.resolution.options.Count;i++)
        {
            string resolution = ui.resolution.options[i].text;
            if(resolution ==display_resolution)
            {
                res_index = i;
                break;
            }
        }
        ui.resolution.value =res_index;
       // Debug.Log($"resolution_index: {res_index}");

        //set color button skipping
        ui.SetButtonColors(ui.skippiongContinue, ui.skippingStop, continueSkippingAfterChoice);

        //set value of architect and auto reader speed
        ui.architectSpeed.value = dialogueTextSpeed;
        ui.autoReadeerSpeed.value = dialogueAutoReadSpeed;

    }
    public void Save()
    {
        FileManager.Save(filePath, JsonUtility.ToJson(this), encrypt:ENCRYPT);
    }


}
