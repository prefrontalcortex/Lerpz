using UnityEngine;
using System.Collections;

[System.Serializable]
 // Player fall out!
// Kill all rigidibodies flying through this area
// (Props that fell off)
// Also kill all character controller passing through
// (enemies)
// Auto setup the pickup
[UnityEngine.AddComponentMenu("Third Person Props/Fallout Death")]
public partial class FalloutDeath : MonoBehaviour
{
    public virtual void OnTriggerEnter(Collider other)
    {
        if ((ThirdPersonStatus) other.GetComponent(typeof(ThirdPersonStatus)))
        {
            ((ThirdPersonStatus) other.GetComponent(typeof(ThirdPersonStatus))).FalloutDeath();
        }
        else
        {
            if (other.attachedRigidbody)
            {
                UnityEngine.Object.Destroy(other.attachedRigidbody.gameObject);
            }
            else
            {
                if (other.GetType() == typeof(CharacterController))
                {
                    UnityEngine.Object.Destroy(other.gameObject);
                }
            }
        }
    }

    public virtual void Reset()
    {
        if (this.GetComponent<Collider>() == null)
        {
            this.gameObject.AddComponent(typeof(BoxCollider));
        }
        this.GetComponent<Collider>().isTrigger = true;
    }

}