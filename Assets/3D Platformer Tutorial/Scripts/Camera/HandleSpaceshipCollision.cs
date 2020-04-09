using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class HandleSpaceshipCollision : MonoBehaviour
{
    public virtual void OnTriggerEnter(Collider col)
    {
        var playerLink = (ThirdPersonStatus) col.GetComponent(typeof(ThirdPersonStatus));
        if (!playerLink) // not the player.
        {
            return;
        }
        else
        {
            playerLink.LevelCompleted();
        }
    }

}