using DIALOGUE;
using TMPro;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace CHARACTERS
{
    [System.Serializable]
    public class CharacterConfigData
    {
        public string name;
        public string alias;
        public Character.CharacterType characterType;

        public float nameFontSize;
        public float dialogueFontSize;
        public Color nameColor;
        public Color dialogueColor;

        public TMP_FontAsset nameFont;
        public TMP_FontAsset dialogueFont;

        [SerializedDictionary("Path / ID", "Sprite")]
        public SerializedDictionary <string, Sprite> sprites = new SerializedDictionary <string, Sprite> ();
        public CharacterConfigData Copy()
        {
            CharacterConfigData result = new CharacterConfigData();

            result.name = name;
            result.alias = alias;
            result.characterType = characterType;
            result.nameColor = new Color(nameColor.r, nameColor.g, nameColor.b, nameColor.a);
            result.dialogueColor = new Color(dialogueColor.r, dialogueColor.g, dialogueColor.b, dialogueColor.a);
            result.nameFont = nameFont;
            result.dialogueFont = dialogueFont;
            result.dialogueFontSize = dialogueFontSize;
            result.nameFontSize = nameFontSize;
            return result;
        }

        private static Color defaultColor => DialogueSystem.instance.config.defaultTextColor;
        private static TMP_FontAsset defaultFont => DialogueSystem.instance.config.defaultFont;
        public static CharacterConfigData Default
        {
            get
            {
                CharacterConfigData result = new CharacterConfigData();

                result.name = "";
                result.alias = "";
                result.characterType = Character.CharacterType.Sprite;

                result.nameColor = new Color(defaultColor.r, defaultColor.g, defaultColor.b, defaultColor.a);
                result.dialogueColor = new Color(defaultColor.r, defaultColor.g, defaultColor.b, defaultColor.a);
                
                result.nameFont = defaultFont;
                result.dialogueFont = defaultFont;
                result.dialogueFontSize=DialogueSystem.instance.config.defaultDialogueFontSize;
                result.nameFontSize = DialogueSystem.instance.config.defaultNameFontSize;
                return result;
            }
        }
    }
}