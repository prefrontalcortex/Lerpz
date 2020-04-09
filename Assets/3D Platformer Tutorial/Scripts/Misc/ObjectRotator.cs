using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class ObjectRotator : MonoBehaviour
{
    // objectRotater: Rotates the object to which it is attached.
    // Useful for collectable items, etc.
    public virtual void Update()
    {
        this.transform.Rotate(0, 45 * Time.deltaTime, 0);
    }

    public virtual void OnBecameVisible()
    {
        this.enabled = true;
    }

    public virtual void OnBecameInvisible()
    {
        this.enabled = false;
    }

}