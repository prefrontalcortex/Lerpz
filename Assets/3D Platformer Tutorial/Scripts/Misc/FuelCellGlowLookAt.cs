using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class FuelCellGlowLookAt : MonoBehaviour
{
    // fuelCellGlowLookAt: Forces the object to always face the camera.
    // (Used for the 'glowing halo' effect behind the collectable items.)
    public virtual void Update()
    {
        this.transform.LookAt(Camera.main.transform);
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