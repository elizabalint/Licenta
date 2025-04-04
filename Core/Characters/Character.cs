﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;
using TMPro;
namespace CHARACTERS
{
    public abstract class Character
    {
        public const bool ENABLE_ON_START = false; 
        private const float UNHIGHLIGHTED_DARKEN_STRENGTH = 0.65f;
        public const bool DEFAULT_ORIENTATION_IS_FACING_LEFT = true;



        public string name = "";
        public string displayName = "";
        public string castingName = "";
        public RectTransform root;
        public bool enabled { get { return root.gameObject.activeInHierarchy; } set { root.gameObject.SetActive(value); } }

        public CharacterConfigData config;
        public Animator animator;
        public Color color { get; protected set; } = Color.white;
        protected Color diaplayColor => highlighted ? highlightedColor : unhighlightedColor;
        protected Color highlightedColor => color;
        protected Color unhighlightedColor => new Color(color.a * UNHIGHLIGHTED_DARKEN_STRENGTH, color.g * UNHIGHLIGHTED_DARKEN_STRENGTH, color.b * UNHIGHLIGHTED_DARKEN_STRENGTH, color.a);
        public bool highlighted { get; protected set; } = true;
        public bool isChangingColor => co_changingColor != null;
        protected bool facingLeft = DEFAULT_ORIENTATION_IS_FACING_LEFT;
        public int priority { get; protected set; }

        public Vector2 targetPosition { get; private set; }

        protected CharacterManager characterManager => CharacterManager.instance;
        public DialogueSystem dialogueSystem => DialogueSystem.instance;

        //Coroutines co = coroutime
        protected Coroutine co_revealing, co_hiding, co_moving, co_highlighting, co_changingColor, co_flipping;

        public bool isRevealing => co_revealing != null;
        public bool isHiding => co_hiding != null;
        public bool isMooving => co_moving != null;
        public bool isHighlighting => (highlighted && co_highlighting != null);
        public bool isUnHighlighting => (!highlighted && co_highlighting != null);

        public virtual bool isVisible { get; set; }
        public bool isFacingLeft => facingLeft;
        public bool isFacingRight => !facingLeft;
        public bool isFlipping => co_flipping != null;



        public Character(string name, CharacterConfigData config, GameObject prefab)
        {
            this.name = name;
            displayName = name;
            this.config = config;
            if (prefab != null)
            {
                GameObject ob = Object.Instantiate(prefab, characterManager.characterPanel);

                ob.name = characterManager.FormatCharacterPath(characterManager.characterPrefabNameFormat, name);

                ob.SetActive(true);
                root = ob.GetComponent<RectTransform>();
                //Debug.Log($"root:{root} {root.anchorMax} {root.anchorMin}");
                targetPosition = root.anchoredPosition;
                animator = root.GetComponentInChildren<Animator>();
            }
        }
        public Coroutine Say(string dialogue) => Say(new List<string> { dialogue });
        public Coroutine Say(List<string> dialogue)
        {
            dialogueSystem.ShowSpeakerName(displayName);
            UpdateTextCustomizationOnScreen();

            return dialogueSystem.Say(dialogue);
        }

        public void SetNameColor(Color color) => config.nameColor = color;
        public void SetDialogueColor(Color color) => config.dialogueColor = color;
        public void SetNameFont(TMP_FontAsset font) => config.nameFont = font;
        public void SetDialogueFont(TMP_FontAsset font) => config.dialogueFont = font;

        public void ResetConfigurationData() => config = CharacterManager.instance.GetCharacterConfig(name);
        public void UpdateTextCustomizationOnScreen() => dialogueSystem.ApplySpeakerDataToDialogueContainer(config);

        public virtual Coroutine Show()
        {
            if (isRevealing)
                characterManager.StopCoroutine(co_revealing);
            if (isHiding)
                characterManager.StopCoroutine(co_hiding);
            co_revealing = characterManager.StartCoroutine(ShowingOrHiding(true));
            return co_revealing;
        }
        public virtual Coroutine Hide()
        {
            if (isHiding)
                characterManager.StopCoroutine(co_hiding);
            if (isRevealing)
                characterManager.StopCoroutine(co_revealing);
            co_hiding = characterManager.StartCoroutine(ShowingOrHiding(false));
            return co_hiding;
        }
        public virtual IEnumerator ShowingOrHiding(bool show)
        {
            Debug.Log("Show/Hide cannot be called from a base character type.");
            yield return null;

        }

        public virtual void SetPosition(Vector2 position)
        {
            targetPosition = position;
        }
        public virtual Coroutine MoveToPosition(Vector2 position, float speed = 2f, bool smooth = false)
        {
            if (root == null)
                return null;
            if (isMooving)
                characterManager.StopCoroutine(co_moving);
            co_moving = characterManager.StartCoroutine(MovingToPosition(position, speed, smooth));
            //Debug.Log($"pozitie move: {position}");

            targetPosition = position;

            return co_moving;
        }

        private IEnumerator MovingToPosition(Vector2 position, float speed, bool smooth)
        {
            (Vector2 minAnchorTarget, Vector2 maxAnchorTarget) = ConvertUITargetPositionToRelativeCharacterAnchorTargets(position);
            Vector2 padding = root.anchorMax - root.anchorMin;

            while (root.anchorMin != minAnchorTarget || root.anchorMax != maxAnchorTarget)
            {
                root.anchorMin = smooth ?
                    Vector2.Lerp(root.anchorMin, minAnchorTarget, speed * Time.deltaTime)
                    : Vector2.MoveTowards(root.anchorMin, minAnchorTarget, speed * Time.deltaTime * 0.35f);
                root.anchorMax = root.anchorMin + padding;

                if (smooth && Vector2.Distance(root.anchorMin, minAnchorTarget) <= 0.001f)
                {
                    root.anchorMin = minAnchorTarget;
                    root.anchorMax = maxAnchorTarget;
                    break;
                }
                yield return null;
            }
            Debug.Log("Done Moving");
            co_moving = null;
        }

        protected (Vector2, Vector2) ConvertUITargetPositionToRelativeCharacterAnchorTargets(Vector2 position)
        {
            Vector2 padding = root.anchorMax - root.anchorMin;
            Debug.Log($"padding: {padding}");
            float maxX = 1f - padding.x;
            float maxY = 1f - padding.y;
            Debug.Log($"maxx: {maxX}");
            Debug.Log($"maxy: {maxY}");

            Vector2 minAnchorTarget = new Vector2(maxX * position.x, maxY * position.y);
            Vector2 maxAnchorTarget = minAnchorTarget + padding;
            Debug.Log($"minmax: {minAnchorTarget}, {maxAnchorTarget}");

            return (minAnchorTarget, maxAnchorTarget);
        }
        public Coroutine Highlight(float speed = 1f, bool immediate = false)
        {
            if (isHighlighting || isUnHighlighting)
                characterManager.StopCoroutine(co_highlighting);
            highlighted = true;
            co_highlighting = characterManager.StartCoroutine(Highlighting(speed, immediate));
            return co_highlighting;
        }

        public Coroutine UnHighlight(float speed = 1f, bool immediate = false)
        {
            if (isUnHighlighting || isUnHighlighting)
                characterManager.StopCoroutine(co_highlighting);
            highlighted = false;
            co_highlighting = characterManager.StartCoroutine(Highlighting(speed, immediate));
            return co_highlighting;
        }
        public virtual IEnumerator Highlighting(float speedMultiplier, bool immediate = false)
        {
            Debug.Log("Highlighting is not avaible n this character type!!!");
            yield return null;
        }

        public Coroutine TransitionColor(Color color, float speed = 1f)
        {
            this.color = color;
            if (isChangingColor)
                characterManager.StopCoroutine(co_changingColor);

            co_changingColor = characterManager.StartCoroutine(ChangingColor(diaplayColor, speed));
            return co_changingColor;
        }
        public virtual void SetColor(Color color)
        {
            this.color = color;
        }
        public virtual IEnumerator ChangingColor(Color color, float speed)
        {
            Debug.Log("Color changing is not possible on this character type");
            yield return null;
        }

        public Coroutine Flip(float speed = 1, bool immediate = false)
        {
            if (isFacingLeft)
                return FaceRight();
            else
                return FaceLeft();
        }
        public Coroutine FaceLeft(float speed = 1, bool immediate = false)
        {
            if (isFlipping)
                characterManager.StopCoroutine(co_flipping);
            facingLeft = true;
            co_flipping = characterManager.StartCoroutine(FaceDirection(facingLeft, speed, immediate));
            return co_flipping;
        }
        public Coroutine FaceRight(float speed = 1, bool immediate = false)
        {

            if (isFlipping)
                characterManager.StopCoroutine(co_flipping);
            facingLeft = false;
            co_flipping = characterManager.StartCoroutine(FaceDirection(facingLeft, speed, immediate));
            return co_flipping;
        }
        public virtual IEnumerator FaceDirection(bool faceLeft, float speedMultiplier, bool immediate)
        {
            Debug.Log("cannot flip a character of this type");
            yield return null;
        }

        // pentru a nu suprapune caracterele
        public void SetPriority(int priority, bool autoSortCharacterOnUI = true)
        {
            this.priority = priority;
            if (autoSortCharacterOnUI)
                characterManager.SortCharacters();

        }
        public virtual void OnReceiveCastingExpression(int layer, string expression)
        {
            return;
        }

        public enum CharacterType
        {
            Text,
            Sprite,
            SpriteSheet
        }
    }
}