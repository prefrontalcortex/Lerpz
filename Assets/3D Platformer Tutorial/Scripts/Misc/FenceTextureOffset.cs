using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class FenceTextureOffset : MonoBehaviour
{
    public float scrollSpeed;
    public virtual void FixedUpdate()
    {
        float offset = Time.time * this.scrollSpeed;
        this.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(offset, offset);
    }

    public FenceTextureOffset()
    {
        this.scrollSpeed = 0.25f;
    }

}