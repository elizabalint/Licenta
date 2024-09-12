using DIALOGUE;
using History;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

namespace PROJECT
{
    [System.Serializable]
    public class ProjectGameSave
    {
        public static ProjectGameSave activeFile = null;
        /// <summary>
        /// ps = project save (fol o extensie proprie)
        /// </summary>
        public const string FILE_TYPE = ".myp";
        public const string SCREENSHOT_FILE_TYPE = ".jpg";
        public const bool ENCRYPT = true;
        public const float SCREENSHOT_DOWNSCALE_AMOUNT = 0.25f;
        public string filePath => $"{FilePaths.gameSaves}{slotNumber}{FILE_TYPE}";
        public string screenshotPath => $"{FilePaths.gameSaves}{slotNumber}{SCREENSHOT_FILE_TYPE}";

        public string playerName;
        public int slotNumber = 1;

        public bool newGame = true;

        public string[] activeConversations;
        public HistoryState activeState;
        public HistoryState[] historyLogs;
        public Project_VariableData[] variables;
        public string timestamp;


        public void Save()
        {
            newGame = false;
            activeState = HistoryState.Capture();
            historyLogs = HistoryManager.instance.history.ToArray();
            activeConversations = GetConversationData();
            variables = GetVariableData();
            Debug.Log(variables.Length + variables.ToString());
            timestamp = DateTime.Now.ToString("dd-MM-yy HH:mm:ss");
            ScreenshotMaster.CaptureScreenshot(ProjectManager.instance.mainCamera, Screen.width, Screen.height, SCREENSHOT_DOWNSCALE_AMOUNT, screenshotPath);


            string saveJSON = JsonUtility.ToJson(this);

            FileManager.Save(filePath, saveJSON, ENCRYPT);

        }
        private string[] GetConversationData()
        {
            List<string> retData = new List<string>();
            var conversations = DialogueSystem.instance.conversationManager.GetConversationQueue();
            for(int i = 0;i < conversations.Length; i++)
            {
                var conversation = conversations[i];
                string data = "";
                if(conversation.file != string.Empty) 
                {
                    var compressedData = new Project_ConversationDataCompressed();
                    compressedData.fileName =conversation.file;
                    compressedData.progress =conversation.GetProgress();
                    compressedData.startIndex = conversation.fileStartIndex;
                    compressedData.endIndex = conversation.fileEndIndex;
                    data = JsonUtility.ToJson(compressedData);
                }
                else
                {
                    var fullData = new Project_ConversationData();
                    fullData.conversation =conversation.GetLines();
                    fullData.progress =conversation.GetProgress();
                    data = JsonUtility.ToJson(fullData);

                }
                retData.Add(data);
            }
            
            return retData.ToArray();

        }
        public void Activate()
        {
            if(activeState!=null)
                activeState.Load();
            HistoryManager.instance.history = historyLogs.ToList();
            HistoryManager.instance.logManager.Clear();
            HistoryManager.instance.logManager.Rebuild();
            SetVariableData();
            SetConversationData();
            DialogueSystem.instance.prompt.Hide();
        }

        public static ProjectGameSave Load(string filePath, bool activateOnLoad=false)
        {
            ProjectGameSave save = FileManager.Load<ProjectGameSave>(filePath, ENCRYPT);

            activeFile = save;
            if(activateOnLoad)
                save.Activate();
            return save;
        }

        private void SetConversationData()
        {
            for(int i=0; i<activeConversations.Length; i++)
            {
                try
                {
                    string data = activeConversations[i];
                    Conversation conversation = null;

                    var fullData = JsonUtility.FromJson<Project_ConversationData>(data);
                    if (fullData != null && fullData.conversation !=null && fullData.conversation.Count>0)
                    {
                        conversation = new Conversation(fullData.conversation, fullData.progress);
                    }
                    else
                    {
                        var compressedData = JsonUtility.FromJson<Project_ConversationDataCompressed>(data);
                        if (compressedData != null && compressedData.fileName != string.Empty)
                        {
                            TextAsset file = Resources.Load<TextAsset>(compressedData.fileName);
                            int count = compressedData.endIndex - compressedData.startIndex;

                            List<string> lines = FileManager.ReadTextAsset(file).Skip(compressedData.startIndex).Take(count+1).ToList();

                           
                            conversation = new Conversation(lines, compressedData.progress, compressedData.fileName, compressedData.startIndex, compressedData.endIndex);

                        }
                        else
                        {
                            Debug.LogError($"unknown conv format. unable to reload conv from ProjectGameSave using data '{data}'");
                        }
                    }

                    if(conversation != null && conversation.GetLines().Count>0)
                    {
                        if(i==0)
                            DialogueSystem.instance.conversationManager.StartConversation(conversation);
                        else
                            DialogueSystem.instance.conversationManager.Enqueue(conversation);

                    }


                }
                catch(System.Exception e)
                {
                    Debug.LogError($"encountered error while extracting saved conversation data {e}");
                    continue;
                }
                
            }
        }


        private Project_VariableData[] GetVariableData()
        {
            List<Project_VariableData> retData = new List<Project_VariableData>();
            foreach (var database in VariableStore.databases.Values)
            {
                foreach(var variable in database.variables)
                {
                    Project_VariableData variableData = new Project_VariableData();
                    variableData.name = $"{database.name}.{variable.Key}";
                    string val = $"{variable.Value.Get()}";
                    variableData.value =val;
                    variableData.type = val == string.Empty ? "System.String" : variable.Value.Get().GetType().ToString();

                    retData.Add(variableData);
                }
            }

            return retData.ToArray();
        }
        private void SetVariableData()
        {
            foreach(var variable in variables)
            {      
                string val = variable.value;

                switch (variable.type)
                {
                    case "System.Boolean":
                        if (bool.TryParse(val, out bool b_value))
                        {
                            VariableStore.TrySetValue(variable.name, b_value);
                            continue;
                        }
                        break;
                    case "System.Int32":
                        if (int.TryParse(val, out int i_value))
                        {
                            VariableStore.TrySetValue(variable.name, i_value);
                            continue;
                        }
                        break;
                    case "System.Single":
                        if (float.TryParse(val, out float f_value))
                        { VariableStore.TrySetValue(variable.name, f_value);
                            continue;
                        }
                        break;
                    case "System.String":
                        VariableStore.TrySetValue(variable.name, val);
                        continue;

                }
                Debug.LogError($"could not interpret variable type.{variable.name} = {variable.type} ");

            }


        }


    }
}