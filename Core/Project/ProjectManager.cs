using DIALOGUE;
using System.Collections.Generic;
using UnityEngine;

namespace PROJECT
{
    public class ProjectManager : MonoBehaviour
    {
        [SerializeField] private ProjectSO config;
        public static ProjectManager instance { get; private set; }
        public Camera mainCamera;

        private void Awake()
        {
            instance = this;
            ProjectDatabaseLinkSetup linkSetup = GetComponent<ProjectDatabaseLinkSetup>();
            linkSetup.SetupExtrenalLinks();

            if(ProjectGameSave.activeFile ==null)
                ProjectGameSave.activeFile = new ProjectGameSave();

        }

        private void Start()
        {
            LoadGame();
        }
        private void LoadGame()
        {
            if(ProjectGameSave.activeFile.newGame)
            {
                List<string> lines = FileManager.ReadTextAsset(config.startingFile);
                Conversation start = new Conversation(lines);
                DialogueSystem.instance.Say(start);
            }
            else
            {
                ProjectGameSave.activeFile.Activate();
            }
        }
    }
}