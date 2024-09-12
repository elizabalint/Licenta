using System.Text.RegularExpressions;
namespace DIALOGUE
{
    public class DialogueParser
    {
        private const string commandRegexPattern = @"[\w\[\]]*[^\s]\(";
        public static DIALOGUE_LINE Parse(string rawLine)
        {
            //Debug.Log($"Parsing line - '{rawLine}'");

            (string speaker, string dialogue, string commands) = RipContent(rawLine);

            //Debug.Log($"Speaker = '{speaker}'\nDialogue = '{dialogue}'\nCommands = '{commands}'");
            
            commands = TagManager.Inject(commands);

            return new DIALOGUE_LINE(rawLine, speaker, dialogue, commands);
        }
        private static(string,string,string) RipContent(string rawLine)
        {
            string speaker = "", dialogue = "", commands = "";

            int dialogueStart = -1;
            int dialogueEnd = -1;
            bool isEscaped = false;


            for(int i = 0;i<rawLine.Length;i++)
            {
                char current = rawLine[i];
                if (current == '\\')
                    isEscaped = !isEscaped;
                else if (current == '"' && !isEscaped)
                {
                    if (dialogueStart == -1)
                        dialogueStart = i;
                    else if (dialogueEnd == -1)
                        dialogueEnd = i;
                }
                else
                    isEscaped = false;
                
            }
            //Debug.Log(rawLine.Substring(dialogueStart + 1, (dialogueEnd - dialogueStart)-1));
            //pattern pentru textul citit
            Regex commandRegex = new Regex(commandRegexPattern);
            MatchCollection matches = commandRegex.Matches(rawLine);
            int commandStrat = -1;
            foreach (Match match in matches)
            {
                if (match.Index<dialogueStart || match.Index>dialogueEnd)
                {
                    commandStrat = match.Index;
                    break;
                }
               
            }
            if (commandStrat != -1 && (dialogueStart == -1 && dialogueEnd == -1))
                return ("", "", rawLine.Trim());

            // daca am ajuns aici avem ori dialog, ori comanda cu mai multe argumente si verif daca e dialog sau nu
            if (dialogueStart != -1 && dialogueEnd != -1 && (commandStrat == -1 || commandStrat > dialogueEnd))
            {
                // avem dialog
                speaker = rawLine.Substring(0,dialogueStart).Trim();
                dialogue = rawLine.Substring(dialogueStart + 1, dialogueEnd-dialogueStart-1).Replace("\\\"","\"");
                if (commandStrat != -1)
                    commands = rawLine.Substring(commandStrat).Trim() ;
            }
            else if (commandStrat != -1 && dialogueStart > commandStrat)
                commands = rawLine;
            else 
                dialogue = rawLine;
            return (speaker, dialogue, commands);
        }
    }

}