using UnityEngine;

public class FilePaths 
{
    private const string HOME_DIRECTORY_SYMBOL = "~/";
    public static readonly string root = $"{Application.dataPath}/gameData/";

    //runtime Paths
    public static readonly string gameSaves = $"{runtimePath}Save Files/";

    // resources path
    public static readonly string resources_font = "Fonts/";
    public static readonly string resources_graphics = "Graphics/";
    public static readonly string resources_backgroundImages = $"{resources_graphics}BG Images/";
    public static readonly string resources_blendTextures = $"{resources_graphics}Transition Effects/";


    public static readonly string resources_dialogueFiles = $"Dialogue Files/";


    public static string GetPathToResources(string defaultPath, string resourceName)
    {
        if(resourceName.StartsWith(HOME_DIRECTORY_SYMBOL))
            return resourceName.Substring(HOME_DIRECTORY_SYMBOL.Length);
        return defaultPath + resourceName;
    }
    public static string runtimePath
    {
        get
        {
            #if UNITY_EDITOR
                return "Assets/appdata";
            #else
                return Application.persistentDataPath + "/appdata/";
            #endif
        }
    }
}

