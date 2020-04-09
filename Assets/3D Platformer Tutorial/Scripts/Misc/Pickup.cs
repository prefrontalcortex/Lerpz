using UnityEngine;
using System.Collections;

public enum PickupType
{
    Health = 0,
    FuelCell = 1
}

[System.Serializable]
 // do we exist in the level or are we instantiated by an enemy dying?
 // A switch...case statement may seem overkill for this, but it makes adding new pickup types trivial.
//* Make sure we are running into a player
//* prevent picking up the trigger twice, because destruction
//  might be delayed until the animation has finished
// Play sound
// If there is an animation attached.
// Play it.
// Auto setup the pickup
[UnityEngine.RequireComponent(typeof(SphereCollider))]
[UnityEngine.AddComponentMenu("Third Person Props/Pickup")]
public partial class Pickup : MonoBehaviour
{
    public PickupType pickupType;
    public int amount;
    public AudioClip sound;
    public float soundVolume;
    private bool used;
    private DroppableMover mover;
    public virtual void Start()
    {
        this.mover = (DroppableMover) this.GetComponent(typeof(DroppableMover));
    }

    public virtual bool ApplyPickup(ThirdPersonStatus playerStatus)
    {
        switch (this.pickupType)
        {
            case PickupType.Health:
                playerStatus.AddHealth(this.amount);
                break;
            case PickupType.FuelCell:
                playerStatus.FoundItem(this.amount);
                break;
        }
        return true;
    }

    public virtual void OnTriggerEnter(Collider col)
    {
        if (this.mover && this.mover.enabled)
        {
            return;
        }
        ThirdPersonStatus playerStatus = (ThirdPersonStatus) col.GetComponent(typeof(ThirdPersonStatus));
        if (this.used || (playerStatus == null))
        {
            return;
        }
        if (!this.ApplyPickup(playerStatus))
        {
            return;
        }
        this.used = true;
        if (this.sound)
        {
            AudioSource.PlayClipAtPoint(this.sound, this.transform.position, this.soundVolume);
        }
        if (this.GetComponent<Animation>() && this.GetComponent<Animation>().clip)
        {
            this.GetComponent<Animation>().Play();
            UnityEngine.Object.Destroy(this.gameObject, this.GetComponent<Animation>().clip.length);
        }
        else
        {
            UnityEngine.Object.Destroy(this.gameObject);
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

    public Pickup()
    {
        this.pickupType = PickupType.FuelCell;
        this.amount = 1;
        this.soundVolume = 2f;
    }

}