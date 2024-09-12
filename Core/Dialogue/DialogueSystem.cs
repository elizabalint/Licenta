using System.Collections.Generic;
using UnityEngine;
using CHARACTERS;
namespace DIALOGUE
{
    public class DialogueSystem : MonoBehaviour
    {
        [SerializeField]private DialogueSystemConfigurationSO _config; 
        public DialogueSystemConfigurationSO config=> _config;
        public DialogueContainer dialogueContainer = new DialogueContainer();
        public ConversationManager conversationManager { get; private set; }
        private TextArchitect architect;
        public AutoReader autoReader { get; private set; }
        [SerializeField] private CanvasGroup mainCanvas;
        public static DialogueSystem instance { get; private set; }
        public delegate void DialogueSystemEvent();
        public event DialogueSystemEvent onUserPrompt_Next;
        public event DialogueSystemEvent onClear;

        public bool isRunningConversation => conversationManager.isRunning;
        public DialogueContinuePrompt prompt ;
        private CanvasGroupController cgController;
        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
                Initialize();
            }
            else
                DestroyImmediate(gameObject);

        }
        bool _initialized = false;
        private void Initialize()
        {
            if(_initialized)
                return; 

            architect = new TextArchitect(dialogueContainer.dialogueText);
            conversationManager = new ConversationManager(architect);
            cgController = new CanvasGroupController(this, mainCanvas) ;
            dialogueContainer.Initialize();

            autoReader = GetComponent<AutoReader>();
            if (autoReader !=null)
                autoReader.Initialize(conversationManager);
        }

        public void OnUserPrompt_Next()
        {
            onUserPrompt_Next?.Invoke();
            if (autoReader != null && autoReader.isOn)
                autoReader.Disable();
        }
        public void OnSystemPrompt_Next()
        {
            onUserPrompt_Next?.Invoke();
        }

        public void OnSystemPrompt_Clear()
        {
            onClear?.Invoke();
        }
        public void OnStartViewingHistory()
        {
            prompt.Hide();
            autoReader.allowToggle = false;
            conversationManager.allowUserPrompts = false;
            if (autoReader.isOn)
                autoReader.Disable();
        }

        public void OnStopViewingHistry()
        {
            prompt.Show();
            autoReader.allowToggle = true;
            conversationManager.allowUserPrompts = true;
        }

        public void ApplySpeakerDataToDialogueContainer(string speakerName)
        {
            Character character = CharacterManager.instance.GetCharacter(speakerName);
            CharacterConfigData config = character != null ? character.config : CharacterManager.instance.GetCharacterConfig(speakerName);
            
            ApplySpeakerDataToDialogueContainer(config);
        
        }

        public void ApplySpeakerDataToDialogueContainer(CharacterConfigData config)
        {
            dialogueContainer.SetDialogueColor(config.dialogueColor);
            dialogueContainer.SetDialogueFont(config.dialogueFont);
            dialogueContainer.SetDialogueFontSize(config.dialogueFontSize);
            dialogueContainer.nameContainer.SetNameFont(config.nameFont);
            dialogueContainer.nameContainer.SetNameColor(config.nameColor);
            dialogueContainer.nameContainer.SetNameFontSize(config.nameFontSize);
        }
        public void ShowSpeakerName(string speakerNmae = "")
        {
            if (speakerNmae.ToLower() != "narrator")
                dialogueContainer.nameContainer.Show(speakerNmae);
            else
            {
                HideSpeakerName();
                dialogueContainer.nameContainer.nameText.text = "";
            }
        }
        public void HideSpeakerName() => dialogueContainer.nameContainer.Hide();



        // fct for DialogueSyaytem
        public Coroutine Say(string speaker, string dialogue)
        {
            List<string> conversation = new List<string>() { $"{speaker}\"{dialogue}\"" };
            return Say(conversation);
        }

        public Coroutine Say(List<string> lines, string filePath ="")
        {
            Conversation conversation =new Conversation(lines, file:filePath);
            return conversationManager.StartConversation(conversation);
        }
        public Coroutine Say(Conversation conversation)
        {
            return conversationManager.StartConversation(conversation);
        }
        public bool isVisible => cgController.isVisible;
        public Coroutine Show(float speed=1f, bool immediate =false) => cgController.Show(speed, immediate);
        public Coroutine Hide(float speed = 1f, bool immediate = false) => cgController.Hide(speed, immediate);
    }
}
