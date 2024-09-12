using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GraphicObject 
{
    public RawImage renderer;
    private const string NAME_FORMAT = "Graphic - [{0}]";
    private const string DEFAULT_UI_MATERIAL = "Default UI Material";
    private const string MATERIAL_PATH = "Materials/layerTransitionMaterial";
    private const string MATERIAL_FIELD_COLOR = "_Color";
    private const string MATERIAL_FIELD_MAINTEX = "_MainTex";
    private const string MATERIAL_FIELD_BLENDTEX = "_BlendTex";
    private const string MATERIAL_FIELD_BLEND = "_Blend";
    private const string MATERIAL_FIELD_ALPHA = "_Alpha";
    private Coroutine co_fadingIn = null;
    private Coroutine co_fadingOut = null;
    private GraphicLayer layer;


    public bool isVideo { get { return video!=null; } }
    public VideoPlayer video = null;
    public AudioSource audio = null;

    public string graphicPath = "";
    public string graphicName{ get; private set; }
    public GraphicObject(GraphicLayer layer,string graphicPath, Texture tex, bool immediate)
    {
        this.graphicPath = graphicPath;
        this.layer = layer;
        GameObject ob = new GameObject();
        ob.transform.SetParent(layer.panel);
        renderer = ob.AddComponent<RawImage>();
        graphicName = tex.name;

        InitGraphic(immediate);
        renderer.name = string.Format(NAME_FORMAT, graphicName);
        renderer.material.SetTexture(MATERIAL_FIELD_MAINTEX, tex);

    }
    private void InitGraphic(bool immediate)
    {
        renderer.transform.localPosition= Vector3.zero;
        renderer.transform.localScale= Vector3.one;

        RectTransform rect = renderer.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.one;


        renderer.material = GetTransitionMaterial();
        float startOpacity = immediate? 1.0f : 0.0f;
        renderer.material.SetFloat(MATERIAL_FIELD_BLEND, startOpacity);
        renderer.material.SetFloat(MATERIAL_FIELD_ALPHA, startOpacity);

    }
    private Material GetTransitionMaterial()
    {
        Material mat = Resources.Load<Material>(MATERIAL_PATH);

        if(mat!= null)
            return new Material(mat);
        return null;
    }
    GraphicPanelManager panelManeger => GraphicPanelManager.instance;
    public Coroutine FadeIn(float speed=1f, Texture blend = null)
    {
        if(co_fadingOut!=null)
        {
            panelManeger.StopCoroutine(co_fadingOut);
        }
        if (co_fadingIn != null)
            return co_fadingIn;
        co_fadingIn = panelManeger.StartCoroutine(Fading(1f,speed,blend));
        return co_fadingIn;
    }
    public Coroutine FadeOut(float speed=1f, Texture blend = null)
    {
        if (co_fadingIn != null)
        {
            panelManeger.StopCoroutine(co_fadingIn);
        }
        if (co_fadingOut != null)
            return co_fadingOut;
        co_fadingOut = panelManeger.StartCoroutine(Fading(0f, speed, blend));
        return co_fadingOut;
    }
    private IEnumerator Fading(float target, float speed, Texture blend)
    {
        bool isBlending = blend != null;
        bool fadingIn = target > 0;

        if (renderer.material.name ==DEFAULT_UI_MATERIAL)
        {
            Texture tex = renderer.material.GetTexture(MATERIAL_FIELD_MAINTEX);
            renderer.material = GetTransitionMaterial();
            renderer.material.SetTexture(MATERIAL_FIELD_MAINTEX, tex);
        }

        renderer.material.SetTexture(MATERIAL_FIELD_BLENDTEX, blend);
        renderer.material.SetFloat(MATERIAL_FIELD_ALPHA, isBlending ? 1 : fadingIn ? 0 : 1);
        renderer.material.SetFloat(MATERIAL_FIELD_BLEND, isBlending ? fadingIn ? 0 : 1 : 1);

        string opacityParam = isBlending ? MATERIAL_FIELD_BLEND : MATERIAL_FIELD_ALPHA;

        while(renderer.material.GetFloat(opacityParam) != target)
        {
            float opacity = Mathf.MoveTowards(renderer.material.GetFloat(opacityParam), target, speed * Time.deltaTime);
            renderer.material.SetFloat(opacityParam, opacity);
            yield return null;
        }
        co_fadingIn = null;
        co_fadingOut = null;

        if (target == 0)
        {
            Destroy();
        }
        else
        {
            DestrouBackgroundGraphicsOnLayer();

            // Ensure renderer is not null before accessing its properties
            if (renderer != null)
            {
                var material = renderer.material;

                // Ensure material is not null before accessing its texture
                if (material != null)
                {
                    renderer.texture = material.GetTexture(MATERIAL_FIELD_MAINTEX);

                    // Safely set material to null
                    renderer.material = null;
                }
                
            }
           
        }


    }
    public void Destroy()
    {
        if(layer.currentGraphic!=null && layer.currentGraphic.renderer==renderer)
           layer.currentGraphic = null;
            
        if(layer.oldGraphics.Contains(this))
            layer.oldGraphics.Remove(this);
            
         Object.Destroy(renderer.gameObject);

    }
    private void DestrouBackgroundGraphicsOnLayer()
    {
        layer.DestroyOldGraphics();
    }
}
