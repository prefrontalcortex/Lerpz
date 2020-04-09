using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Fades out any objects between the player and this transform.
   The renderers shader is first changed to be an Alpha/Diffuse, then alpha is faded out to fadedOutAlpha.
   Only objects 
   
   In order to catch all occluders, 5 rays are casted. occlusionRadius is the distance between them.
*/
[System.Serializable]
public class FadeoutLOSInfo : object
{
    public Renderer renderer;
    public Material[] originalMaterials;
    public Material[] alphaMaterials;
    public bool needFadeOut;
    public FadeoutLOSInfo()
    {
        this.needFadeOut = true;
    }

}
[System.Serializable]
// Mark all objects as not needing fade out
// We cast 5 rays to really make sure even occluders that are partly occluding the player are faded out
// Find all blocking objects which we want to hide
 // Make sure we have a renderer
// We are not fading this renderer already, so insert into faded objects map
// Just mark the renderer as needing fade out
// Now go over all renderers and do the actual fading!
// Fade out up to minimum alpha value
// Fade back in
// All alpha materials are faded back to 100%
// Thus we can switch back to the original materials
[UnityEngine.AddComponentMenu("Third Person Camera/Fadeout Line of Sight")]
public partial class FadeoutLineOfSight : MonoBehaviour
{
    public LayerMask layerMask;
    public Transform target;
    public float fadeSpeed;
    public float occlusionRadius;
    public float fadedOutAlpha;
    private List<FadeoutLOSInfo> fadedOutObjects;
    public virtual FadeoutLOSInfo FindLosInfo(Renderer r)
    {
        foreach (FadeoutLOSInfo fade in this.fadedOutObjects)
        {
            if (r == fade.renderer)
            {
                return fade;
            }
        }
        return null;
    }

    public virtual void LateUpdate()
    {
        int i = 0;
        Vector3 from = this.transform.position;
        Vector3 to = this.target.position;
        float castDistance = Vector3.Distance(to, from);
        foreach (FadeoutLOSInfo fade in this.fadedOutObjects)
        {
            fade.needFadeOut = false;
        }
        Vector3[] offsets = new Vector3[] {new Vector3(0, 0, 0), new Vector3(0, this.occlusionRadius, 0), new Vector3(0, -this.occlusionRadius, 0), new Vector3(this.occlusionRadius, 0, 0), new Vector3(-this.occlusionRadius, 0, 0)};
        foreach (Vector3 offset in offsets)
        {
            Vector3 relativeOffset = this.transform.TransformDirection(offset);
            RaycastHit[] hits = Physics.RaycastAll(from + relativeOffset, to - from, castDistance, this.layerMask.value);
            foreach (RaycastHit hit in hits)
            {
                Renderer hitRenderer = hit.collider.GetComponent<Renderer>();
                if ((hitRenderer == null) || !hitRenderer.enabled)
                {
                    continue;
                }
                FadeoutLOSInfo info = this.FindLosInfo(hitRenderer);
                if (info == null)
                {
                    info = new FadeoutLOSInfo();
                    info.originalMaterials = hitRenderer.sharedMaterials;
                    info.alphaMaterials = new Material[info.originalMaterials.Length];
                    info.renderer = hitRenderer;
                    i = 0;
                    while (i < info.originalMaterials.Length)
                    {
                        Material newMaterial = new Material(Shader.Find("Alpha/Diffuse"));
                        newMaterial.mainTexture = info.originalMaterials[i].mainTexture;
                        newMaterial.color = info.originalMaterials[i].color;

                        {
                            float _8 = 1f;
                            Color _9 = newMaterial.color;
                            _9.a = _8;
                            newMaterial.color = _9;
                        }
                        info.alphaMaterials[i] = newMaterial;
                        i++;
                    }
                    hitRenderer.sharedMaterials = info.alphaMaterials;
                    this.fadedOutObjects.Add(info);
                }
                else
                {
                    info.needFadeOut = true;
                }
            }
        }
        float fadeDelta = this.fadeSpeed * Time.deltaTime;
        i = 0;
        while (i < this.fadedOutObjects.Count)
        {
            var fade = this.fadedOutObjects[i];
            if (fade.needFadeOut)
            {
                foreach (Material alphaMaterial in fade.alphaMaterials)
                {
                    float alpha = alphaMaterial.color.a;
                    alpha = alpha - fadeDelta;
                    alpha = Mathf.Max(alpha, this.fadedOutAlpha);

                    {
                        float _10 = alpha;
                        Color _11 = alphaMaterial.color;
                        _11.a = _10;
                        alphaMaterial.color = _11;
                    }
                }
            }
            else
            {
                int totallyFadedIn = 0;
                foreach (Material alphaMaterial in fade.alphaMaterials)
                {
                    var alpha = alphaMaterial.color.a;
                    alpha = alpha + fadeDelta;
                    alpha = Mathf.Min(alpha, 1f);

                    {
                        float _12 = alpha;
                        Color _13 = alphaMaterial.color;
                        _13.a = _12;
                        alphaMaterial.color = _13;
                    }
                    if (alpha >= 0.99f)
                    {
                        totallyFadedIn++;
                    }
                }
                if (totallyFadedIn == fade.alphaMaterials.Length)
                {
                    if (fade.renderer != null)
                    {

                        {
                            var _6 = fade.originalMaterials;
                            Renderer _7 = fade.renderer;
                            _7.sharedMaterials = _6;
                        }
                    }
                    foreach (object newerMaterial in fade.alphaMaterials)
                    {
                        UnityEngine.Object.Destroy((UnityEngine.Object) newerMaterial);
                    }
                    this.fadedOutObjects.RemoveAt(i);
                    i--;
                }
            }
            i++;
        }
    }

    public FadeoutLineOfSight()
    {
        this.layerMask = (LayerMask) 2;
        this.fadeSpeed = 1f;
        this.occlusionRadius = 0.3f;
        this.fadedOutAlpha = 0.3f;
        this.fadedOutObjects = new List<FadeoutLOSInfo>();
    }

}