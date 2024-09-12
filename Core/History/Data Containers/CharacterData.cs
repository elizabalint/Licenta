using System.Collections.Generic;
using UnityEngine;
using CHARACTERS;
using static History.CharacterData.AnimationData;
namespace History
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public string castingName;
        public string displayName;
        public bool enabled;
        public Color color;
        public int priority;
        public bool isHighlighted;
        public bool isFacingLeft;
        public Vector2 position;

        public string animationJSON;
        public string dataJSON;
        public CharacterConfigCache characterConfig;

        [System.Serializable]
        public class CharacterConfigCache
        {
            public string name;
            public string alias;

            public Character.CharacterType characterType;
            public Color nameColor;
            public Color dialogueColor;
            public string nameFont;
            public string dialogueFont;

            public float nameFontScale = 1f;
            public float dialogueFontScale = 1f;


            public CharacterConfigCache(CharacterConfigData reference)
            {
                name = reference.name;
                alias = reference.alias;
                characterType = reference.characterType;

                nameColor = reference.nameColor;
                dialogueColor = reference.dialogueColor;
                nameFont = FilePaths.resources_font + reference.nameFont.name;
                dialogueFont = FilePaths.resources_font + reference.dialogueFont.name;

                nameFontScale = reference.nameFontSize;
                dialogueFontScale = reference.dialogueFontSize;
            }

        }
        public static List<CharacterData> Capture()
        {
            List<CharacterData> characters = new List<CharacterData>();

            foreach (var character in CharacterManager.instance.allCharacters)
            {
                if (!character.isVisible) continue;

                CharacterData entry = new CharacterData();
                entry.characterName = character.name;
                entry.castingName = character.castingName;
                entry.displayName = character.displayName;
                entry.enabled = character.isVisible;
                entry.color = character.color;
                entry.priority = character.priority;
                entry.isFacingLeft = character.isFacingLeft;
                entry.isHighlighted = character.highlighted;
                entry.position = character.targetPosition;


                //Debug.Log($"Capture - Character: {character.name}, Position: {entry.position}");
                entry.characterConfig = new CharacterConfigCache(character.config);

                SpriteData sData = new SpriteData();
                sData.layers = new List<SpriteData.LayerData>();
                Character_Sprite sc = character as Character_Sprite;
                foreach (var layer in sc.layers)
                {
                    var layerData = new SpriteData.LayerData();
                    layerData.color = layer.renderer.color;
                    layerData.spriteName = layer.renderer.sprite.name;
                    sData.layers.Add(layerData);
                }

                entry.dataJSON = JsonUtility.ToJson(sData);
                characters.Add(entry);
            }
            return characters;
        }
        public static void Apply(List<CharacterData> data)
        {
            //Debug.Log($"data {data.Count}");
            List<string> cache = new List<string>();
            foreach (CharacterData characterData in data)
            {
                Character character = null;
                if(characterData.castingName ==string.Empty)
                {
                    character = CharacterManager.instance.GetCharacter(characterData.characterName, createIfNotExist: true);

                }
                else
                {
                    character = CharacterManager.instance.GetCharacter(characterData.characterName, createIfNotExist: false);
                    if(character == null)
                    {
                        string castingName = $"{characterData.characterName}{CharacterManager.CHARACTER_CASTING_ID}{characterData.castingName}";
                        character = CharacterManager.instance.CreateCharacter(castingName);
                    }
                }
                character.displayName = characterData.displayName;
                character.SetColor(characterData.color);

                if (characterData.isHighlighted)
                    character.Highlight(immediate: true);
                else
                    character.UnHighlight(immediate: true);

                character.SetPriority(characterData.priority);

                if (characterData.isFacingLeft)
                    character.FaceLeft(immediate: true);
                else
                    character.FaceRight(immediate: true);

                character.SetPosition(characterData.position);
                //Debug.Log($"Apply - Character: {character.name}, Position: {characterData.position}");

                character.isVisible = characterData.enabled;


                //AnimationData animationData =JsonUtility.FromJson<AnimationData>( characterData.animationJSON);
                //ApplyAnimationData(character, animationData);

                SpriteData sData = JsonUtility.FromJson<SpriteData>(characterData.dataJSON);
                Character_Sprite sc = character as Character_Sprite;

                for (int i = 0; i < sData.layers.Count; i++)
                {
                    var layer = sData.layers[i];
                    if (sc.layers[i].renderer.sprite != null && sc.layers[i].renderer.sprite.name != layer.spriteName)
                    {
                        Sprite sprite = sc.GetSprite(layer.spriteName);
                        if (sprite != null)
                            sc.SetSprite(sprite, i);
                        else
                            Debug.LogWarning($"history state could not load sprite '{layer.spriteName}'");
                    }
                }
                cache.Add(character.name);

            }
            //Debug.Log($"cache: {cache.Count}");
            foreach (Character character in CharacterManager.instance.allCharacters)
            {
                character.Show();
                if (!cache.Contains(character.name))
                {
                    character.isVisible = false;
                    character.Hide();
                    //Debug.Log($"visible {character.name}:{character.isVisible}");
                }
            }
        }

        private static string GetAnimationData(Character character)
        {
            Animator animator = character.animator;
            AnimationData data = new AnimationData();
            foreach (var param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                    continue;
                AnimationParameter pData = new AnimationParameter { name = param.name };
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        pData.type = "Bool";
                        pData.value = animator.GetBool(param.name).ToString();
                        break;
                    case AnimatorControllerParameterType.Float:
                        pData.type = "Float";
                        pData.value = animator.GetFloat(param.name).ToString();
                        break;
                    case AnimatorControllerParameterType.Int:
                        pData.type = "Int";
                        pData.value = animator.GetInteger(param.name).ToString();
                        break;
                }
                data.parameters.Add(pData);
            }

            return JsonUtility.ToJson(data);


        }
        private static void ApplyAnimationData(Character character, AnimationData data)
        {
            Animator animator = character.animator;
            foreach (var param in data.parameters)
            {
                switch (param.type)
                {
                    case "Bool":
                        animator.SetBool(param.name, bool.Parse(param.value));
                        break;
                    case "Float":
                        animator.SetFloat(param.name, float.Parse(param.value));
                        break;
                    case "Int":
                        animator.SetInteger(param.name, int.Parse(param.value));
                        break;
                }
            }
        }

        [System.Serializable]
        public class AnimationData
        {
            public List<AnimationParameter> parameters = new List<AnimationParameter>();
            [System.Serializable]
            public class AnimationParameter
            {
                public string name;
                public string type;
                public string value;
            }
        }

        [System.Serializable]
        public class SpriteData
        {
            public List<LayerData> layers;
            [System.Serializable]
            public class LayerData
            {
                public string spriteName;
                public Color color;
            }
        }
    }
}