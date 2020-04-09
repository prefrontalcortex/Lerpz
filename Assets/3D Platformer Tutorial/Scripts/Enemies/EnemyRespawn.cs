using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class EnemyRespawn : MonoBehaviour
{
    /*
	EnemyRespawn.js
	
	This script checks if the player is in range. If so, it instantiates the enemy prefab specified. When the player moves out of range, the prefab is automatically destroyed.
	
	This prevents repeated calls to the enemy's AI scripts when the AI is nowhere near the player, this improving performance.

*/
    public float spawnRange; // the distance within which the enemy should be active.
    public string gizmoName; // the type of the object. (See OnDrawGizmos() for more.)
    public GameObject enemyPrefab; // link to the Prefab we'll be instantiating / destroying on demand.
    // Cache variables, used to speed up the code.
    private Transform player;
    private GameObject currentEnemy;
    private bool wasOutside;
    // Called on Scene startup. Cache a link to the Player object.
    // (Uses the tagging system to locate him.)
    public virtual void Start()
    {
        this.player = GameObject.FindWithTag("Player").transform;
    }

    // Called at least once every game cycle. This is where the fun stuff happens.
    public virtual void Update()
    {
         // how far away is the player?
        float distanceToPlayer = Vector3.Distance(this.transform.position, this.player.position);
        // is he in range?
        if (distanceToPlayer < this.spawnRange)
        {
             // in range. Do we have an active enemy and the player has just come into range, instantiate the prefab at our location. 
            if (!this.currentEnemy && this.wasOutside)
            {
                this.currentEnemy = UnityEngine.Object.Instantiate(this.enemyPrefab, this.transform.position, this.transform.rotation);
            }
            // player is now inside our range, so set the flag to prevent repeatedly instantiating the prefab.
            this.wasOutside = false;
        }
        else
        {
            // player is out of range.
             // is player leaving the sphere of influence while our prefab is active?
            if (this.currentEnemy && !this.wasOutside)
            {
                UnityEngine.Object.Destroy(this.currentEnemy); // kill the prefab...
            }
            // ...and set our flag so we re-instantiate the prefab if the player returns.
            this.wasOutside = true;
        }
    }

    // Called by the Unity Editor GUI every update cycle.
    // Draws an icon at our transform's location. The icon's filename is derived from the "type" variable, which allows this script to be used for any enemy.
    public virtual void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 1);
        // See the help docs for info on where the icon needs to be stored for this function to work.
        Gizmos.DrawIcon(this.transform.position, this.gizmoName + ".psd");
    }

    // Called by the Unity Editor GUI every update cycle, but only when the object is selected.
    // Draws a sphere showing spawnRange's setting visually.
    public virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1);
        Gizmos.DrawWireSphere(this.transform.position, this.spawnRange);
    }

    public EnemyRespawn()
    {
        this.wasOutside = true;
    }

}