using DIALOGUE;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CHARACTERS
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager instance { get; private set; }
        private Dictionary<string, Character> characters = new Dictionary<string, Character>();
        private CharacterConfigSO config => DialogueSystem.instance.config.characterConfigurationAsset;

        public const string CHARACTER_CASTING_ID = " as ";
        public IEnumerable<Character> allCharacters => characters.Values;

        // cu asta avem "Resources.Load("Characters/NAME_CHARACTER")
        public const string CHARACTER_NAME_ID = "<charname>";
        public string characterRootPathFormat => $"Characters/{CHARACTER_NAME_ID}";
        public string characterPrefabNameFormat => $"Character - [{CHARACTER_NAME_ID}]";
        public string characterPrefabPathFormat => $"{characterRootPathFormat}/{characterPrefabNameFormat}";


        [SerializeField] private RectTransform _characterPanel;
        public RectTransform characterPanel => _characterPanel;

        public CharacterConfigData GetCharacterConfig(string characterName)
        {
            return config.GetConfig(characterName);
        }
        private void Awake()
        {
            instance = this;
        }

        public Character GetCharacter(string characterName, bool createIfNotExist = false)
        {
            if (characters.ContainsKey(characterName.ToLower()))
            {
                return characters[characterName.ToLower()];
            }
            else if (createIfNotExist) { return CreateCharacter(characterName); }

            return null;
        }
        public bool HasCharacter(string characterName) => characters.ContainsKey(characterName.ToLower());
        public Character CreateCharacter(string characterName, bool revealAfterCreation = false)
        {
            if (characters.ContainsKey(characterName.ToLower()))
            {
                Debug.LogWarning($"A Character called '{characterName}' already exists. Did not create the character.");
                return null;
            }
            CHARACTER_INFO info = GetCharacterInfo(characterName);
            Character character = CreateCharacterFromInfo(info);
            if (info.castingName != info.name)
                character.castingName = info.castingName;



            //adaugam in dictionar pt a nu putea crea 2 caractere la fel din meniu (adica TestCharacters momentan)
            characters.Add(info.name.ToLower(), character);

            if (revealAfterCreation)
                character.Show();

            //Debug.Log(character.name);
            return character;
        }
        private CHARACTER_INFO GetCharacterInfo(string characterName)
        {
            CHARACTER_INFO result = new CHARACTER_INFO();
            string[] nameData = characterName.Split(CHARACTER_CASTING_ID, System.StringSplitOptions.RemoveEmptyEntries);

            result.name = nameData[0];
            result.castingName = nameData.Length > 1 ? nameData[1] : result.name;

            result.config = config.GetConfig(result.castingName);

            result.prefab = GetPrefabForCharacter(result.castingName);


            // pt layer la charactere!!!!!
            result.roorCharacterFolder = FormatCharacterPath(characterRootPathFormat, result.castingName); // daca nu am sprite sa iau characterNmae in loc de "result.castingName"
            return result;
        }

        private GameObject GetPrefabForCharacter(string characterName)
        {
            string prefabPath = FormatCharacterPath(characterPrefabPathFormat, characterName);
            return Resources.Load<GameObject>(prefabPath);


        }

        public string FormatCharacterPath(string path, string characterName) => path.Replace(CHARACTER_NAME_ID, characterName);

        private Character CreateCharacterFromInfo(CHARACTER_INFO info)
        {
            CharacterConfigData config = info.config;
            switch (config.characterType)
            {
                case Character.CharacterType.Text:
                    return new Character_Text(info.name, config);
                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    return new Character_Sprite(info.name, config, info.prefab, info.roorCharacterFolder);

                default:
                    return null;
            }


        }

        public void SortCharacters()
        {
            List<Character> activeCharacters = characters.Values.Where(c => c.root.gameObject.activeInHierarchy && c.isVisible).ToList();
            List<Character> inactiveCharacters = characters.Values.Except(activeCharacters).ToList();
            activeCharacters.Sort((a, b) => a.priority.CompareTo(b.priority));
            activeCharacters.Concat(inactiveCharacters);
            SortCharacters(activeCharacters);

        }
        public void SetCharacters(string[] characterNames)
        {
            List<Character> sortedCharacter = new List<Character>();
            sortedCharacter = characterNames
                .Select(name => GetCharacter(name))
                .Where(character => character != null)
                .ToList();

            List<Character> remainingCharacters = characters.Values
                .Except(sortedCharacter)
                .OrderBy(character => character.priority)
                .ToList();
            sortedCharacter.Reverse();


            int startingPriority = remainingCharacters.Count > 0 ? remainingCharacters.Max(character => character.priority) : 0;
            for (int i = 0; i < sortedCharacter.Count; i++)
            {
                Character character = sortedCharacter[i];
                character.SetPriority(startingPriority + i + 1, autoSortCharacterOnUI: false);
            }

            List<Character> allCharacters = remainingCharacters.Concat(sortedCharacter).ToList();
            SortCharacters(sortedCharacter);
        }
        public void SortCharacters(List<Character> charactersSortInOrder)
        {
            int i = 0;
            foreach (Character character in charactersSortInOrder)
            {
                //Debug.Log($"{character.name} priority: {character.priority}");
                character.root.SetSiblingIndex(i++);
            }
        }


        private class CHARACTER_INFO
        {
            public string name = "";
            public string castingName = "";
            public CharacterConfigData config = null;
            public GameObject prefab = null;


            // pt layer la charactere!!!!!
            public string roorCharacterFolder = "";
        }
    }
}