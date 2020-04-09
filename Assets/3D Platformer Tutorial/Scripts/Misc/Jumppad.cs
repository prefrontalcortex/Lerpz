using UnityEngine;
using System.Collections;

[System.Serializable]
// Auto setup the script and associated trigger.
[UnityEngine.RequireComponent(typeof(BoxCollider))]
[UnityEngine.AddComponentMenu("Third Person Props/Jump pad")]
public partial class Jumppad : MonoBehaviour
{
    public float jumpHeight;
    public virtual void OnTriggerEnter(Collider col)
    {
        ThirdPersonController controller = (ThirdPersonController) col.GetComponent(typeof(ThirdPersonController));
        if (controller != null)
        {
            if (this.GetComponent<AudioSource>())
            {
                this.GetComponent<AudioSource>().Play();
            }
            controller.SuperJump(this.jumpHeight);
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

    public Jumppad()
    {
        this.jumpHeight = 5f;
    }

}