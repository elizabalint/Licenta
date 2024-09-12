using System.Collections;
using UnityEngine;
using TMPro;

public class TextArchitect
{
    private TextMeshProUGUI tmpro_ui; //permite afisarea textului 
    private TextMeshPro tmpro_world; 

    public TMP_Text tmpro => tmpro_ui != null ? tmpro_ui : tmpro_world; 

    public string currentText => tmpro.text; // 
    public string taregetText { get; private set; } = ""; //what are we trying to build or append (daca e append, targetText va fi deasupra gen pre-text) 
    public string preText { get; private set; } = ""; // orice este deja stocat in arhitect va fi stocat in variabila asta inainte sa fie amplasat noul text
    private int preTextLength = 0;

    public string fullTargetText => preText + taregetText; // full string architect ar trebui sa construiasca

    public enum BuildMethod { instant, typewriter, fade }
    public BuildMethod buildMethod = BuildMethod.typewriter;

    public Color textColor { get { return tmpro.color; } set { tmpro.color = value; } }

    public float speed { get { return baseSpeed * speedMultiplier; } set { speedMultiplier = value; } } // atat de repede textul va "veni" pe ecran
    private const float baseSpeed = 1; //gen universaal idk
    private float speedMultiplier = 1;  // poate fi configurat de user sa mearga mai repede

    public int characterPerCycle { get { return speed <= 2f ? characterMultiplier : speed <= 2.5f ? characterMultiplier * 2 : characterMultiplier * 3; } }
    private int characterMultiplier = 1;


    public bool hurryUp = false;
    //constructori
    public TextArchitect(TextMeshProUGUI tmpro_ui)
    {
        this.tmpro_ui = tmpro_ui;
    }
    public TextArchitect(TextMeshPro tmpro_world)
    {
        this.tmpro_world = tmpro_world;
    }

    // bring in text
    public Coroutine Build(string text)
    {
        preText = "";
        taregetText = text;
        Stop();
        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;

    }
    // append text unui text deja existent in text arhitect 
    public Coroutine Append(string text)
    {
        preText = tmpro.text;
        taregetText = text;
        Stop();
        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;

    }
    public void SetText(string text)
    {
        preText = "";
        taregetText = text;
        Stop();
        tmpro.text = taregetText;
        ForceComplete();
    }
  
    // handle the text generation
    private Coroutine buildProcess = null;
    public bool isBuilding => buildProcess != null;
    public void Stop()
    {
        if (!isBuilding)
            return;
        tmpro.StopCoroutine(buildProcess);
        buildProcess = null;
    }
    IEnumerator Building()
    {
        Prepare();
        switch (buildMethod)
        {
            case BuildMethod.typewriter:
                yield return Build_Typewriter();
                break;
            case BuildMethod.fade:
                yield return Build_Fade();
                break;
        }
        OnComplete();
    }
    private void OnComplete()
    {
        buildProcess = null;
        hurryUp = false;
    }
    public void ForceComplete()
    {
        switch(buildMethod)
        {
            case BuildMethod.typewriter:
                tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
                break;
            case BuildMethod.fade:
                tmpro.ForceMeshUpdate();
                break;
        }
        Stop();
        OnComplete();
    }
    private void Prepare()
    {
        switch(buildMethod)
        {
            case BuildMethod.instant:
                Prepare_Instant();
                break;
            case BuildMethod.typewriter:
                Prepare_Typewriter();
                break;
            case BuildMethod.fade:
                Prepare_Fade();
                break;
        }
    }
    private void Prepare_Instant()
    {
        tmpro.color = tmpro.color; //aceeasi culoare
        tmpro.text = fullTargetText;
        tmpro.ForceMeshUpdate();//toate schimbarile pe care le facem vor fi aplicate aici (daca schimbam caracterul va fi aplicat)
        tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount; // make sure every character is visible on screen
    }
    private void Prepare_Typewriter()
    {
        tmpro.color = tmpro.color;
        tmpro.maxVisibleCharacters = 0;
        tmpro.text = preText;

        if(preText != null) 
        {
            tmpro.ForceMeshUpdate();
            tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;

        }
        tmpro.text += taregetText;
        tmpro.ForceMeshUpdate();
    }
    private void Prepare_Fade()
    {
        tmpro.text = preText;
        if (preText != "")
        {
            tmpro.ForceMeshUpdate();
            preTextLength = tmpro.textInfo.characterCount;
        }
        else preTextLength = 0;
         
        tmpro.text += taregetText;
        tmpro.maxVisibleCharacters = int.MaxValue;
        tmpro.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmpro.textInfo;
 
        Color colorVisible = new Color(textColor.r, textColor.g, textColor.b, 1);
        Color colorHidden = new Color(textColor.r, textColor.g, textColor.b, 0);

        Color32[] vectexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;

        for(int i=0; i<textInfo.characterCount;i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if(!charInfo.isVisible) continue;
            if (i<preTextLength)
            {
                for(int v= 0; v<4;v++)
                    vectexColors[charInfo.vertexIndex +v] = colorVisible;
            }
            else
                for (int v = 0; v < 4; v++)
                {
                    vectexColors[charInfo.vertexIndex + v] = colorHidden;
                }
        }
        tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

    }
    private IEnumerator Build_Typewriter()
    {
        while(tmpro.maxVisibleCharacters< tmpro.textInfo.characterCount)
        {
            tmpro.maxVisibleCharacters += hurryUp ? characterPerCycle * 5 : characterPerCycle;

            yield return new WaitForSeconds(0.015f / speed);
        }
    }
    private IEnumerator Build_Fade()
    {
        int minRange = preTextLength;
        int maxRange = minRange + 1;

        byte alphaThreshold = 15;

        TMP_TextInfo textInfo = tmpro.textInfo;
        Color32[] vertexColor = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;
        float[] alphas = new float[textInfo.characterCount];

        while(true)
        {
            float fadeSpeed = ((hurryUp ? characterPerCycle*5 : characterPerCycle) * speed)*4f;
            for(int i = minRange; i < maxRange; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible) continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                alphas[i] = Mathf.MoveTowards(alphas[i], 255, fadeSpeed);
                for (int v = 0; v < 4; v++)
                {
                    vertexColor[charInfo.vertexIndex + v].a = (byte) alphas[i];
                }
                if (alphas[i] >= 255)
                    minRange++;

            } 
            tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            bool lastCharacterIsInvisibel = !textInfo.characterInfo[maxRange-1].isVisible;
            if (alphas[maxRange-1]>alphaThreshold || lastCharacterIsInvisibel)
            {
                if (maxRange < textInfo.characterCount)
                    maxRange++;
                else if (alphas[maxRange - 1] >= 255 || lastCharacterIsInvisibel)
                    break;
            }
            yield return new WaitForEndOfFrame();
        }

    }


}
