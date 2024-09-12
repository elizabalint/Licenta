using PROJECT;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadSlot : MonoBehaviour
{
    public GameObject root;
    public RawImage previewImage;
    public TextMeshProUGUI titleText;
    public Button deleteButton;
    public Button saveButton;
    public Button loadButton;

    [HideInInspector]public int fileNumber = 0;
    [HideInInspector]public string filePath = "";
    private UIConfirmation uiChoice => UIConfirmation.instance;

    public void PopulateDetails(SaveAndLoadMenu.MenuFunction function)
    {
        if(File.Exists(filePath))
        {
            ProjectGameSave file = ProjectGameSave.Load(filePath);
            PopulateDetailsFromFile(function, file);
        } 
        else
        {
            PopulateDetailsFromFile(function, null);
        }
    }


    private void PopulateDetailsFromFile(SaveAndLoadMenu.MenuFunction function, ProjectGameSave file)
    {
        if(file == null)
        {
            titleText.text = $"{fileNumber}. Empty File";
            deleteButton.gameObject.SetActive(false);
            loadButton.gameObject.SetActive(false);
            saveButton.gameObject.SetActive(function == SaveAndLoadMenu.MenuFunction.save);
            previewImage.texture = SaveAndLoadMenu.instance.emptyFileImage;
        }
        else
        {
            titleText.text = $"{fileNumber}. {file.timestamp}";
            deleteButton.gameObject.SetActive(true);
            loadButton.gameObject.SetActive(function == SaveAndLoadMenu.MenuFunction.load);
            saveButton.gameObject.SetActive(function == SaveAndLoadMenu.MenuFunction.save);
            
            byte[] data = File.ReadAllBytes(file.screenshotPath);
            Texture2D screenshotPreview = new Texture2D(1, 1);
            ImageConversion.LoadImage(screenshotPreview, data);
            
            previewImage.texture = screenshotPreview;

        }
    }

    public void Delete()
    {
        File.Delete(filePath);
        PopulateDetails(SaveAndLoadMenu.instance.menuFunction);
    }
    private void OnConformDelete()
    {
        uiChoice.Show("Delete this file? (<i> This cannot be undone!<i>)", new UIConfirmation.ConfirmationButton("Yes", OnConformDelete), new UIConfirmation.ConfirmationButton("No", null));

    }
    public void Load()
    {
        ProjectGameSave file = ProjectGameSave.Load(filePath, false);
        SaveAndLoadMenu.instance.Close(closedAllMenus: true);


        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main Menu")
        {
            MainMenu.instance.LoadGame(file);
        }
        else
        {
            file.Activate();

        }

    }
    public void Save()
    {
       /* if (HistoryManager.instance.isViewingHistory)
            UIConfirmation.instance.Show("You cannot save while viewing history", new UIConfirmation.ConfirmationButton("Okay", null));
            return;
       */
        var activeSave = ProjectGameSave.activeFile;
        activeSave.slotNumber = fileNumber;
        activeSave.Save();

        PopulateDetailsFromFile(SaveAndLoadMenu.instance.menuFunction, activeSave);
    }

    


}
