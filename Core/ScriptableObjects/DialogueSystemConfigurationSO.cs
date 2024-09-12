using UnityEngine;
using CHARACTERS;
using TMPro;


namespace DIALOGUE
{
    [CreateAssetMenu(fileName ="Dialogue System Configuration", menuName ="Dialogue System/Dialogue Configuration Asset")]
    public class DialogueSystemConfigurationSO : ScriptableObject
    {
        public CharacterConfigSO characterConfigurationAsset;
        // asta poate fi si in character configuration
        public Color defaultTextColor = Color.white;

        public TMP_FontAsset defaultFont;

        public float defaultDialogueFontSize = 18;
        public float defaultNameFontSize = 22;

    }
}