using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.RequireComponent(typeof(ThirdPersonController))]
public partial class ThirdPersonPushBodies : MonoBehaviour
{
    public float pushPower;
    public LayerMask pushLayers;
    private ThirdPersonController controller;
    public virtual void Start()
    {
        this.controller = (ThirdPersonController) this.GetComponent(typeof(ThirdPersonController));
    }

    public virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        // no rigidbody
        if ((body == null) || body.isKinematic)
        {
            return;
        }
        // Ignore pushing those rigidbodies
        int bodyLayerMask = 1 << body.gameObject.layer;
        if ((bodyLayerMask & this.pushLayers.value) == 0)
        {
            return;
        }
        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3f)
        {
            return;
        }
        // Calculate push direction from move direction, we only push objects to the sides
        // never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        // push with move speed but never more than walkspeed
        body.velocity = (pushDir * this.pushPower) * Mathf.Min(this.controller.GetSpeed(), this.controller.walkSpeed);
    }

    public ThirdPersonPushBodies()
    {
        this.pushPower = 0.5f;
        this.pushLayers = (LayerMask) (-1);
    }

}