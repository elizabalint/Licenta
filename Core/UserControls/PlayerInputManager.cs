using History;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace DIALOGUE
{
    public class PlayerInputManager : MonoBehaviour
    {
        private PlayerInput input;
        private List<(InputAction action, Action<InputAction.CallbackContext> command)> actions  = new List<(InputAction action, Action<InputAction.CallbackContext> command)> ();
        public AutoReader autoReader; // Reference to the AutoReader component

        // Start is called before the first frame update
        void Awake()
        {
            input = GetComponent<PlayerInput> ();
                InitializeActions ();
        }
        private void InitializeActions()
        {
            actions.Add((input.actions["Next"], OnNext));
            actions.Add((input.actions["HistoryBack"], OnHistoryBack));
            actions.Add((input.actions["HistoryForward"], OnHistoryForward));
            actions.Add((input.actions["HistoryLogs"], OnHistoryToggleLog));
            actions.Add((input.actions["Skip"], OnSkip));


        }
        private void OnEnable()
        {
            foreach(var inputAction in actions)
                inputAction.action.performed += inputAction.command;
        }
        private void OnDisable()
        {
            foreach (var inputAction in actions)
                inputAction.action.performed -= inputAction.command;
        }

        /*
        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                PromptAdvance();
        }*/
        public void OnNext(InputAction.CallbackContext c)
        {
            DialogueSystem.instance.OnUserPrompt_Next();
        }

        public void OnHistoryBack(InputAction.CallbackContext c)
        {
            HistoryManager.instance.GoBack();
        }
        public void OnHistoryForward(InputAction.CallbackContext c)
        {
            HistoryManager.instance.GoForward();
        }
        public void OnHistoryToggleLog(InputAction.CallbackContext c)
        {
            var logs = HistoryManager.instance.logManager;
            if(!logs.isOpen)
                logs.Open();
            else
                logs.Close();
        }
        public void OnSkip(InputAction.CallbackContext c)
        {
            if (autoReader != null)
            {
                autoReader.Toggle_Skip();
            }
            else
            {
                Debug.LogError("AutoReader reference is not set.");
            }
        }
    }
}
