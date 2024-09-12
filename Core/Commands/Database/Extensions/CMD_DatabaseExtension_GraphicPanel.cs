using System;
using System.Collections;
using UnityEngine;

namespace COMMANDS
{
    public class CMD_DatabaseExtension_GraphicPanel : CMD_DatabaseExtension
    {
        private static string[] PARAM_PANEL = new string[] { "-p", "-panel" };
        private static string[] PARAM_LAYER = new string[] { "-l", "-layer" };
        private static string[] PARAM_MEDIA = new string[] { "-m", "-media" };
        private static string[] PARAM_SPEED = new string[] { "-spd", "-speed" };
        private static string[] PARAM_IMMEDIATE = new string[] { "-i", "-immediate" };
        private static string[] PARAM_BLENDTEX = new string[] { "-b", "-bled" };
        private const string HOMEDIRECTORY_SYMBOL = "-/";
        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("setlayermedia", new Func<string[], IEnumerator>(SetLayerMedia));
            database.AddCommand("clearlayermedia", new Func<string[], IEnumerator>(ClearLayerMedia));

        }


        private static IEnumerator SetLayerMedia(string[] data)
        {
            string panelName = "";
            int layer = 0;
            string mediaName = "";
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTexName = "";
            string pathToGraphic = "";
            UnityEngine.Object graphic = null;
            Texture blendTex = null;


            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_PANEL, out panelName);
            GraphicPanel panel = GraphicPanelManager.instance.GetPanel(panelName);
            if(panel == null )
            {
                Debug.LogError($"unable to grab panel '{panelName}' because it is not a valid panel. please check the panel name and adjust the command");
                yield break;
            }

            // try to get the layer to apply this graphic to
            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue: 0);
            parameters.TryGetValue(PARAM_MEDIA, out mediaName);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            if(!immediate)
                parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1);

            parameters.TryGetValue(PARAM_BLENDTEX, out blendTexName);

            // load the image
            pathToGraphic = GetPathToGraphic(FilePaths.resources_backgroundImages, mediaName);
            graphic = Resources.Load<Texture>(pathToGraphic);
            if(graphic == null)
            {
                Debug.LogError($"Could not find media file called '{mediaName}' in the resources directories. specify the full path within resources and make sure that the file exists");
                yield break ;
            }

            if(!immediate && blendTexName!=string.Empty)
                blendTex = Resources.Load<Texture>(FilePaths.resources_blendTextures + blendTexName);

            //get layer to apply the media to
            GraphicLayer graphicLayer = panel.GetLayer(layer, createIfDoesNotExist: true);
            if(graphic is Texture)
            {
                if(!immediate)
                    CommandManager.instance.AddTerminationActionToCurrentProcess(() => { graphicLayer?.SetTexture(graphic as Texture, filePath:pathToGraphic, immediate:true); });
                yield return graphicLayer.SetTexture(graphic as Texture, transitionSpeed, blendTex, pathToGraphic, immediate);
            }
            


        }
        private static IEnumerator ClearLayerMedia(string[] data)
        {
            string panelName = "";
            int layer = 0;
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTextName = "";
            Texture blendTex = null;


            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_PANEL, out panelName);
            GraphicPanel panel = GraphicPanelManager.instance.GetPanel(panelName);
            if (panel == null)
            {
                Debug.LogError($"unable to grab panel '{panelName}' because it is not a valid panel. please check the panel name and adjust the command");
                yield break;
            }
            // try to get the layer to apply this graphic to
            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue: -1);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            if (!immediate)
                parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1);

            parameters.TryGetValue(PARAM_BLENDTEX, out blendTextName);


            if (!immediate && blendTextName != string.Empty)
                blendTex = Resources.Load<Texture>(FilePaths.resources_blendTextures + blendTextName);

            if (layer == -1)
                panel.Clear(transitionSpeed, blendTex, immediate);
            else
            {
                GraphicLayer graphicLayer = panel.GetLayer(layer);
                if(graphicLayer == null)
                {
                    Debug.LogError($"could not clear layer [{layer}] on panel '{panel.panelName}'");
                    yield break;
                }
                graphicLayer.Clear(transitionSpeed,blendTex, immediate);
            }


        }

        private static string GetPathToGraphic(string defaultPath, string graphicName)
        {
            if(graphicName.StartsWith(HOMEDIRECTORY_SYMBOL))
                return graphicName.Substring(HOMEDIRECTORY_SYMBOL.Length);

            return defaultPath + graphicName;
        }

    }
}