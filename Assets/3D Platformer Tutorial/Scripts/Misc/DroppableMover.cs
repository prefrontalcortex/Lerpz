using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class DroppableMover : MonoBehaviour
{
    public float gravity;
    public LayerMask collisionMask;
    private Vector3 velocity;
    private Vector3 position;
    public virtual void Bounce(Vector3 force)
    {
        this.position = this.transform.position;
        this.velocity = force;
    }

    public virtual void Update()
    {
        this.velocity.y = this.velocity.y - (this.gravity * Time.deltaTime);
        var moveThisFrame = this.velocity * Time.deltaTime;
        var distanceThisFrame = moveThisFrame.magnitude;
        if (Physics.Raycast(this.position, moveThisFrame, distanceThisFrame, (int) this.collisionMask))
        {
            this.enabled = false;
        }
        else
        {
            this.position = this.position + moveThisFrame;
            this.transform.position = new Vector3(this.position.x, this.position.y + 0.75f, this.position.z);
        }
    }

    public DroppableMover()
    {
        this.gravity = 10f;
        this.velocity = Vector3.zero;
    }

}