using System.Collections.Generic;
using UnityEngine;

public class GraphicLayer 
{
    public const string LAYER_OBJECT_NAME_FORMAT = "Layer: {0}";
    public int layerDepth = 0;
    public Transform panel;
    public GraphicObject currentGraphic = null;
    public List<GraphicObject> oldGraphics = new List<GraphicObject>();
    public Coroutine SetTexture(string filePath, float transitionSpeed=1f, Texture blendingTexture=null, bool immediate=false)
    {
        Texture tex = Resources.Load<Texture>(filePath);
        if(tex == null)
        {
            Debug.LogError($"could not load graphic texture from '{filePath}' please ensure it exists withic Resources");
            return null;
        }
        
        return SetTexture(tex, transitionSpeed, blendingTexture, filePath, immediate);
    }
    // salvam aici si filePath pentru cand o sa am nev pt a salva parcursul jocului
    public Coroutine SetTexture(Texture tex, float transitionSpeed = 1f, Texture bledingTexture = null, string filePath ="", bool immediate = false)
    {
        return CreateGraphic(tex, transitionSpeed, filePath, blendingTexture: bledingTexture, immediate:immediate);
    }

    private Coroutine CreateGraphic<T>(T graphicData, float transitionSpeed, string filePath, bool useAudioForVideo = false, Texture blendingTexture = null, bool immediate = false)
    {
        GraphicObject newGraphic = null ;
        if(graphicData is Texture ) 
        {
            newGraphic = new GraphicObject(this, filePath, graphicData as Texture, immediate);
        }
        if(currentGraphic!=null && !oldGraphics.Contains(currentGraphic))
            oldGraphics.Add(currentGraphic);

        currentGraphic = newGraphic;
        if(!immediate)
            return currentGraphic.FadeIn(transitionSpeed, blendingTexture);

        //otherwise this is an immediate effect
        DestroyOldGraphics();
        return null;
    }
    public void DestroyOldGraphics()
    {
        foreach (var g in oldGraphics)
        {
            if (g.renderer != null)
            Object.Destroy(g.renderer.gameObject);
        }
        oldGraphics.Clear() ;
    }
    public void Clear(float transitionSpeed = 1, Texture blendTexture =null, bool immediate =false)
    {
        if (currentGraphic != null)
        {
            if (!immediate)
                currentGraphic.FadeOut(transitionSpeed, blendTexture);
            else
                currentGraphic.Destroy();
                }
        foreach(var g in oldGraphics)
        {
            if (!immediate)
                g.FadeOut(transitionSpeed, blendTexture);
            else
                g.Destroy();
        }
    }
}
