using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace CHARACTERS
{
    public class Character_Sprite : Character
    {
        // pt layer la charactere!!!!!
        private const string SPRITE_RENDERER_PARENT_NAME = "Renderers";
        private const string SPRITESHEET_DEFAULT_SHEETNAME = "Default";
        private const char SPRITESHEET_TEX_SPRITE_DELIMITTER = '-';
        public List<CharacterSpriteLayer> layers = new List<CharacterSpriteLayer>();
        private string artAssetsDirectory = "";
        //--------------------------------------
        public override bool isVisible => isRevealing || rootCG.alpha ==1;   
        private CanvasGroup rootCG => root.GetComponent<CanvasGroup>();

        public Character_Sprite(string name, CharacterConfigData config, GameObject prefab, string rootAssetsFolder) : base(name, config, prefab)
        {
            //pt ca at cand introducem un caracter sa apara "treptat"
            rootCG.alpha = ENABLE_ON_START ? 1: 0;

            //Debug.Log($"created sprite character: '{name}'");


            // pt layer la charactere!!!!!
            artAssetsDirectory = rootAssetsFolder + "/Images";

            GetLayers();
        }
        // pt layer la charactere!!!!!
        private void GetLayers()
        {
            Transform rendererRoot = animator.transform.Find(SPRITE_RENDERER_PARENT_NAME);
            if (rendererRoot == null)
                return;
                 
            for( int i = 0; i < rendererRoot.transform.childCount; i++ )
            {
                Transform child = rendererRoot.transform.GetChild(i);
                Image rendererImage = child.GetComponent<Image>();

                if(rendererImage != null )
                {
                    CharacterSpriteLayer layer = new CharacterSpriteLayer(rendererImage, i);

                    layers.Add(layer);
                    child.name = $"Layer: {i}";
                }
            }
        }
        public void SetSprite(Sprite sprite, int layer =0)
        {
            layers[layer].SetSprite(sprite);
        }
        public Sprite GetSprite(string spriteName)
        {
            // pentru serialized dictionary
            if(config.sprites.Count>0)
            {
                if(config.sprites.TryGetValue(spriteName, out Sprite sprite))
                    return sprite;
            }
            // teoretic nu mai am nev de asta da o s o las
            if(config.characterType==CharacterType.Sprite)
            {
                string[] data = spriteName.Split(SPRITESHEET_TEX_SPRITE_DELIMITTER);
                Sprite[] spriteArray = new Sprite[0];
                if(data.Length == 2 )
                {
                    string textureName = data[0];
                    spriteName = data[1];
                    Debug.Log($"aici: {textureName}");

                    spriteArray = Resources.LoadAll<Sprite>($"{artAssetsDirectory}/{textureName}");

                }
                else
                {
                    spriteArray = Resources.LoadAll<Sprite>($"{artAssetsDirectory}/{SPRITESHEET_DEFAULT_SHEETNAME}");
                }
                if (spriteArray.Length == 0)
                    Debug.LogWarning($"Character '{name}' does not have a default art asset called '{SPRITESHEET_DEFAULT_SHEETNAME}'");
                return Array.Find(spriteArray, sprite => sprite.name == spriteName);


            }
            else
            {
                return Resources.Load<Sprite>($"{artAssetsDirectory}/{spriteName}");
            }
        }
        public Coroutine TransitionSprite(Sprite sprite, int layer =0, float speed =1)
        {
            CharacterSpriteLayer spriteLayer = layers[layer];
            return spriteLayer.TransitionSprite(sprite, speed);
        }
        //-------------------------------------------
        public override IEnumerator ShowingOrHiding(bool show)
        {
            float targetAlpha = show ? 1f : 0;
            CanvasGroup self = rootCG;
            while(self.alpha!=targetAlpha)
            {
                self.alpha = Mathf.MoveTowards(self.alpha, targetAlpha, 3f*Time.deltaTime);
                yield return null;
            }
            co_revealing = null;
            co_hiding = null;
        }
        public override void SetColor(Color color)
        {
            base.SetColor(color);
            color = diaplayColor;

            foreach (CharacterSpriteLayer layer in layers)
            {
                layer.StopChangingColor();
                layer.SetColor(color);
            }
        }
        public override IEnumerator ChangingColor(Color color, float speed)
        {
            foreach(CharacterSpriteLayer layer in layers)
                layer.TransitionColor(color, speed);
            yield return null;
            while (layers.Any(l => l.isChangingColor))
                yield return null;
            co_changingColor = null;
        }
        public override IEnumerator Highlighting(float speedMultiplier, bool immediate =false)
        {
            Color targetColor = diaplayColor;

            foreach(CharacterSpriteLayer layer in layers)
            {
                if(immediate)
                    layer.SetColor(diaplayColor);
                else
                    layer.TransitionColor(targetColor, speedMultiplier);
            }
            yield return null;

            while(layers.Any(l=>l.isChangingColor))
                yield return null;
            co_highlighting=null;
        }


        public override IEnumerator FaceDirection(bool faceLeft, float speedMultiplier, bool immediate)
        {
            foreach(CharacterSpriteLayer layer in layers)
            {
                if(faceLeft)
                    layer.FaceLeft(speedMultiplier, immediate);
                else
                    layer.FaceRight(speedMultiplier, immediate);
            }
            yield return null;
            while(layers.Any(l=>l.isFlipping)) yield return null;
            co_flipping=null;
        }
        public override void OnReceiveCastingExpression(int layer, string expression)
        {
            Sprite sprite = GetSprite(expression);
            if(sprite == null)
            {
                Debug.LogWarning($"Sprite '{expression}' could not be found for character '{name}'");
                return;
            }
            TransitionSprite(sprite, layer);
        }
    }
}