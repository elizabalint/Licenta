using CHARACTERS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace COMMANDS
{
    public class CMD_DatabaseExtension_Characters: CMD_DatabaseExtension
    {
        private static string[] PARAM_IMMEDIATE => new string[] { "-i", "-immediate" };
        private static string[] PARAM_ENABLE => new string[] {"-e", "-enabled"};
        private static string[] PARAM_SPEED => new string[] { "-spd", "-speed" };
        private static string[] PARAM_SMOOTH => new string[] { "-sm", "-smooth" };
        private static string PARAM_XPOS => "-x";
        private static string PARAM_YPOS => "-y";


        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("CreateCharacter", new Action<string[]>(CreateCharacter));
            database.AddCommand("MoveCharacter", new Func<string[], IEnumerator>(MoveCharacter));
            database.AddCommand("showall", new Func<string[], IEnumerator>(ShowAll));
            database.AddCommand("hideall", new Func<string[], IEnumerator>(HideAll));


            CommandDatabase baseCommands = CommandManager.instance.CreateSubDatabase(CommandManager.DATABASE_CHARACTERS_BASE);
            baseCommands.AddCommand("move", new Func<string[], IEnumerator>(MoveCharacter));
            baseCommands.AddCommand("show", new Func<string[], IEnumerator>(Show));
            baseCommands.AddCommand("hide", new Func<string[], IEnumerator>(Hide));
            baseCommands.AddCommand("setPriority", new Action<string[]>(SetPriority));
            baseCommands.AddCommand("highlight", new Func<string[], IEnumerator>(Highlight));
            baseCommands.AddCommand("unhighlight", new Func<string[], IEnumerator>(Unhighlight));
        }
        private static IEnumerator MoveCharacter(string[] data)
        {
            string characterName = data[0];
            Character character = CharacterManager.instance.GetCharacter(characterName);
            if (character == null)
                yield break;
            float x = 0, y = 0;
            float speed = 1;
            bool smooth = false;
            bool immediate = false;
            var parameters = ConvertDataToParameters(data);

            //try to get the x axis position
            parameters.TryGetValue(PARAM_XPOS, out x);
            // try to get the y axis pos
            parameters.TryGetValue(PARAM_YPOS, out y);
            //try to get the speed
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1);
            //try to get the smoothing
            parameters.TryGetValue(PARAM_SMOOTH, out smooth, defaultValue: false);
            //try to get tmmediate setting of position
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            Vector2 position = new Vector2(x, y);
            if (immediate)
                character.SetPosition(position);
            else
            {

                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { character?.SetPosition(position); });

                yield return character.MoveToPosition(position, speed, smooth);
            }
        }

        public static void CreateCharacter(string[] data) 
        {
            string characterName = data[0];
            bool enable = false;
            bool immediate = false;
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_ENABLE, out enable, defaultValue: false);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            Character character = CharacterManager.instance.CreateCharacter(characterName);
            if (!enable)
                return;
            if (immediate)
                character.isVisible = true;
            else
                character.Show();
        }

        public static IEnumerator ShowAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            foreach (string character in data)
            {
                Character character1= CharacterManager.instance.GetCharacter(character, createIfNotExist: false);
                if(character1 != null)
                {
                    characters.Add(character1);
                }
            }

            if(characters.Count == 0)
            {
                yield break;
            }

            // convert the data array to a parameter container
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            
            // call the logic on all the characters
            foreach(Character character in characters) 
            { 
                if(immediate)
                    character.isVisible = true;
                else
                    character.Show();
            }
            if(!immediate)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                        character.isVisible = true;
                });
                while(characters.Any(c=>c.isRevealing))
                    yield return null;
            }
        }
        public static IEnumerator HideAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            foreach (string character in data)
            {
                Character character1 = CharacterManager.instance.GetCharacter(character, createIfNotExist: false);
                if (character1 != null)
                {
                    characters.Add(character1);
                }
            }

            if (characters.Count == 0)
            {
                yield break;
            }

            // convert the data array to a parameter container
            //var parameters = ConvertDataToParameters(data);

            //parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            // call the logic on all the characters
            foreach (Character character in characters)
            {
               
                    character.Hide();
            }
           /* if (!immediate)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                        character.isVisible = false;
                });
                while (characters.Any(c => c.isHiding))
                    yield return null;
            }*/
        }



        private static IEnumerator Show(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0]);
            if (character == null) 
            {
                yield break;
            }
            bool immediate = false;
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);
            if (immediate)
                character.isVisible = true;
            else
            {
                // a long running process should have a stop action to cancel out the coroutine and run logic that sould complete this command
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { if (character !=null) character.isVisible = true; });
                yield return character.Show();
            }
        }
        private static IEnumerator Hide(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0]);
            if (character == null)
            {
                yield break;
            }
            bool immediate = false;
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);
            if (immediate)
                character.isVisible = false;
            else
            {
                // a long running process should have a stop action to cancel out the coroutine and run logic that sould complete this command
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { if (character != null) character.isVisible = true; });
                yield return character.Hide();
            }
        }


        public static void SetPriority(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfNotExist: false);
            int priority;
            if (character == null || data.Length < 2)
                return;
            if (!int.TryParse(data[1], out priority))
                priority = 0;
            character.SetPriority(priority);
        }

        public static IEnumerator Highlight(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfNotExist: false) as Character;
            if(character == null)
                yield break;
            bool immediate = false;
            var parameters = ConvertDataToParameters(data, startingIndex:1);
            parameters.TryGetValue(new string[] {"-i", "-immediate"}, out immediate, defaultValue: false);
            if (immediate)
                character.Highlight(immediate: true);
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { character?.Highlight(immediate: true); });
                yield return character.Highlight();
            }

        }
        public static IEnumerator Unhighlight(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfNotExist: false) as Character;
            if (character == null)
                yield break;
            bool immediate = false;
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);
            if (immediate)
                character.Highlight(immediate: true);
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { character?.UnHighlight(immediate: true); });
                yield return character.UnHighlight();
            }

        }

    }
}
