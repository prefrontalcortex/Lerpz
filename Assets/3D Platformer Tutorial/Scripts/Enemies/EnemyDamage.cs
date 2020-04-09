using UnityEngine;
using System.Collections;

[System.Serializable]
// sound clips:
 // we've been hit, so play the 'struck' sound. This should be a metallic 'clang'.
 // Kill ourselves
// Instantiate replacement dead character model
// create an effect to let the player know he beat the enemy
// fall away from the player, and spin like a top
// drop a random number of pickups in a random fashion
 // how many shall we drop?
 // pick a random direction to throw the pickup.
 // make sure the pickup isn't thrown downwards
// initial position of the pickup
// select a pickup type at random
// set the pickup in motion
/* 
deadModel	When we instantiate the death sequence prefab, we ensure all its child 
	elements are the same as those in the original robot, by copying all 
	the transforms over. Hence this function.
*/
 // Match the transform with the same name
[UnityEngine.AddComponentMenu("Third Person Enemies/Enemy Damage")]
public partial class EnemyDamage : MonoBehaviour
{
    public int hitPoints;
    public Transform explosionPrefab;
    public Transform deadModelPrefab;
    public DroppableMover healthPrefab;
    public DroppableMover fuelPrefab;
    public int dropMin;
    public int dropMax;
    public AudioClip struckSound;
    private bool dead;
    public virtual void ApplyDamage(int damage)
    {
        if (this.GetComponent<AudioSource>() && this.struckSound)
        {
            this.GetComponent<AudioSource>().PlayOneShot(this.struckSound);
        }
        if (this.hitPoints <= 0)
        {
            return;
        }
        this.hitPoints = this.hitPoints - damage;
        if (!this.dead && (this.hitPoints <= 0))
        {
            this.Die();
            this.dead = true;
        }
    }

    public virtual void Die()
    {
        UnityEngine.Object.Destroy(this.gameObject);
        Transform deadModel = UnityEngine.Object.Instantiate(this.deadModelPrefab, this.transform.position, this.transform.rotation);
        EnemyDamage.CopyTransformsRecurse(this.transform, deadModel);
        Transform effect = UnityEngine.Object.Instantiate(this.explosionPrefab, this.transform.position, this.transform.rotation);
        effect.parent = deadModel;
        Rigidbody deadModelRigidbody = deadModel.GetComponent<Rigidbody>();
        Vector3 relativePlayerPosition = this.transform.InverseTransformPoint(Camera.main.transform.position);
        deadModelRigidbody.AddTorque(Vector3.up * 7);
        if (relativePlayerPosition.z > 0)
        {
            deadModelRigidbody.AddForceAtPosition(-this.transform.forward * 2, this.transform.position + (this.transform.up * 5), ForceMode.Impulse);
        }
        else
        {
            deadModelRigidbody.AddForceAtPosition(this.transform.forward * 2, this.transform.position + (this.transform.up * 2), ForceMode.Impulse);
        }
        int toDrop = Random.Range(this.dropMin, this.dropMax + 1);
        int i = 0;
        while (i < toDrop)
        {
            var direction = Random.onUnitSphere;
            if (direction.y < 0)
            {
                direction.y = -direction.y;
            }
            Vector3 dropPosition = this.transform.TransformPoint(Vector3.up * 1.5f) + (direction / 2);
            DroppableMover dropped = null;
            if (Random.value > 0.5f)
            {
                dropped = UnityEngine.Object.Instantiate(this.healthPrefab, dropPosition, Quaternion.identity);
            }
            else
            {
                dropped = UnityEngine.Object.Instantiate(this.fuelPrefab, dropPosition, Quaternion.identity);
            }
            dropped.Bounce((direction * 4) * (Random.value + 0.2f));
            i++;
        }
    }

    public static void CopyTransformsRecurse(Transform src, Transform dst)
    {
        dst.position = src.position;
        dst.rotation = src.rotation;
        foreach (Transform child in dst)
        {
            Transform curSrc = src.Find(child.name);
            if (curSrc)
            {
                EnemyDamage.CopyTransformsRecurse(curSrc, child);
            }
        }
    }

    public EnemyDamage()
    {
        this.hitPoints = 3;
    }

}