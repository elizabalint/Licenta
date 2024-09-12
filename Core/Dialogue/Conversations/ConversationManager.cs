using CHARACTERS;
using COMMANDS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DIALOGUE.LogicalLines;
namespace DIALOGUE
{
    public class ConversationManager 
    {
        private ConversationQueue conversationQueue;
        public Conversation conversation => (conversationQueue.IsEmpty()? null:conversationQueue.top);
        public int conversationProgress => (conversationQueue.IsEmpty() ? -1 : conversationQueue.top.GetProgress());
        private DialogueSystem dialogueSystem => DialogueSystem.instance;

        private Coroutine process = null;
        public bool isRunning => process !=null;
        public bool isOnLogicalLine { get; private set; } = false;



        public TextArchitect architect = null;
        private bool userPrompt = false;
        public bool allowUserPrompts = true;
        private LogicalLineManager logicalLineManager;
        public ConversationManager(TextArchitect architect)
        {
            this.architect = architect;
            dialogueSystem.onUserPrompt_Next += OnUserPrompt_Next;
            logicalLineManager = new LogicalLineManager();  
            conversationQueue = new ConversationQueue();
        }
        public Conversation[] GetConversationQueue() => conversationQueue.GetReadOnly();

        public void Enqueue(Conversation conversation)=> conversationQueue.Enqueue(conversation);
        public void EnqueuePriority(Conversation conversation)=>conversationQueue.EnqueuePriority(conversation);

        private void OnUserPrompt_Next()
        {
            if(allowUserPrompts)
                userPrompt = true;
        }
        public Coroutine StartConversation(Conversation conversation)
        {
            StopConversation();
            conversationQueue.Clear();
            Enqueue(conversation);
            process = dialogueSystem.StartCoroutine(RunningConversation());
            return process;
        }

        public void StopConversation()
        {
            if(!isRunning)
            {
                return;
            }
            dialogueSystem.StopCoroutine(process);
            process = null;
        }
        IEnumerator RunningConversation()
        {
            while(!conversationQueue.IsEmpty()) 
            {
                Conversation currentCnversation = conversation;
                if(currentCnversation.HasReachedEnd())
                {
                    conversationQueue.Dequeue();
                    continue;
                }
                string rawLine = currentCnversation.CurrentLine();
                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    TryAdvanceConversation(currentCnversation);
                    continue;
                }
                DIALOGUE_LINE line = DialogueParser.Parse(rawLine);

                if (logicalLineManager.TryGetLogic(line, out Coroutine logic))
                {
                    isOnLogicalLine = true;
                    yield return logic;
                }
                else
                {
                    // show dialogue
                    if (line.hasDialogue)
                    {
                        yield return Line_RunDialogue(line);
                    }
                    // run any commands
                    if (line.hasCommands)
                    {
                        yield return Line_RunCommands(line);
                    }
                    //wait for user input if dialogue was in this line
                    if (line.hasDialogue)
                    {
                        // wait for user input
                        yield return WaitForUserInput();
                        CommandManager.instance.StopAllProcesses();
                        dialogueSystem.OnSystemPrompt_Clear();
                    }
                }

               TryAdvanceConversation(currentCnversation);
                isOnLogicalLine = false;
            }

            process = null;


        }

        private void TryAdvanceConversation(Conversation conversation)
        {
            conversation.IncrementProgress();

            if (conversation != conversationQueue.top)
                return;

            if (conversation.HasReachedEnd())
                conversationQueue.Dequeue();
        }


        IEnumerator Line_RunDialogue(DIALOGUE_LINE line)
        {
            // arata sau ascunde numele speaker ului daca nu exista
            if (line.hasSpeaker)
                HandleSpeakerLogic(line.speakerData);

            // dupa ce am dat hide la dialogue box, sa nu mai trebuiasca sa folosesc comanda sa-l "reactivez"
            if (!dialogueSystem.dialogueContainer.isVisible)
                dialogueSystem.dialogueContainer.Show();

            // build dialogue
            yield return BuildLineSegments(line.dialogueData);


        }
        public void HandleSpeakerLogic(DL_SPEAKER_DATA speakerData)
        {

            bool characterMustBeCreated = (speakerData.makeCharacterEnter || speakerData.isCastingPosition || speakerData.isCastingExpression);
            Character character = CharacterManager.instance.GetCharacter(speakerData.name, createIfNotExist: characterMustBeCreated);
            if (speakerData.makeCharacterEnter && (!character.isVisible && !character.isRevealing))
                character.Show();
            // add char name to the ui
            dialogueSystem.ShowSpeakerName(TagManager.Inject(speakerData.displayName));
            // customize the dialogue for this character - if applicable
            DialogueSystem.instance.ApplySpeakerDataToDialogueContainer(speakerData.name);
        
            if(speakerData.isCastingPosition)
                character.MoveToPosition(speakerData.castPosition);

            //cast expressiono
            if(speakerData.isCastingExpression)
            {
                foreach(var ce in speakerData.CastExpression)
                    character.OnReceiveCastingExpression(ce.layer, ce.expression);
            }
        
        
        
        }
        IEnumerator Line_RunCommands(DIALOGUE_LINE line)
        {
            List<DL_COMMAND_DATA.Command> commands = line.commandData.commands;

            foreach (DL_COMMAND_DATA.Command command in commands)
            {
                if (command.waitForCompletion || command.name == "wait")
                {
                    CoroutineWrapper cw = CommandManager.instance.Execute(command.name, command.arguments);
                    while(!cw.isDone)
                    {
                        if (userPrompt)
                        {
                            CommandManager.instance.StopCurrentProcess();
                            userPrompt = false;
                        }
                            yield return null;
                    }
                }
                else
                    CommandManager.instance.Execute(command.name, command.arguments);
            }
            yield return null;
        }
        IEnumerator BuildLineSegments(DL_DIALOGUE_DATA line)
        {
            for(int i=0; i<line.segments.Count(); i++)
            {
                DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment = line.segments[i];
                yield return WaitForDialogueSegmentToBeTriggered(segment);
                yield return BuildDialogue(segment.dialogue, segment.startSignal == DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.A || segment.startSignal == DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WA);
            }
        }

        public bool isWaitingOnAutoTimer { get; private set; } = false;


        IEnumerator WaitForDialogueSegmentToBeTriggered(DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment)
        {
            switch(segment.startSignal)
            {
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.C:
                    yield return WaitForUserInput();
                    dialogueSystem.OnSystemPrompt_Clear(); 
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.A:
                    yield return WaitForUserInput();
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WC:
                    isWaitingOnAutoTimer = true; 
                    yield return new WaitForSeconds(segment.signalDelay);
                    isWaitingOnAutoTimer = false;
                    dialogueSystem.OnSystemPrompt_Clear(); 
                    break;

                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WA:
                    isWaitingOnAutoTimer = true;
                    yield return new WaitForSeconds(segment.signalDelay);
                    isWaitingOnAutoTimer = false;
                    break;
                default:
                    break;



            }
        }
        IEnumerator BuildDialogue(string dialogue, bool append = false)
        {
            dialogue = TagManager.Inject(dialogue);

            // build dialogul
            if (!append)
                architect.Build(dialogue);
            else
                architect.Append(dialogue);

            // asteapta ca dialogul sa se termine
            while (architect.isBuilding)
            {
                if (userPrompt)
                {
                    if (!architect.hurryUp)
                        architect.hurryUp = true;
                    else
                        architect.ForceComplete();
                    userPrompt = false;
                }

                yield return null;
            }
        }
        IEnumerator WaitForUserInput()
        {
            dialogueSystem.prompt.Show();
            while(!userPrompt)
                yield return null;
            dialogueSystem.prompt.Hide();
            userPrompt = false;
        }
    }
}
