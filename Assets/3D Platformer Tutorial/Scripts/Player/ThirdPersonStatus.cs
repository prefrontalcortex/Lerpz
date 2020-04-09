using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class ThirdPersonStatus : MonoBehaviour
{
    // ThirdPersonStatus: Handles the player's state machine.
    // Keeps track of inventory, health, lives, etc.
    public int health;
    public int maxHealth;
    public int lives;
    // sound effects.
    public AudioClip struckSound;
    public AudioClip deathSound;
    private LevelStatus levelStateMachine; // link to script that handles the levelcomplete sequence.
    private int remainingItems; // total number to pick up on this level. Grabbed from LevelStatus.
    public virtual void Awake()
    {
        this.levelStateMachine = (LevelStatus) UnityEngine.Object.FindObjectOfType(typeof(LevelStatus));
        if (!this.levelStateMachine)
        {
            Debug.Log("No link to Level Status");
        }
        this.remainingItems = this.levelStateMachine.itemsNeeded;
    }

    // Utility function used by HUD script:
    public virtual int GetRemainingItems()
    {
        return this.remainingItems;
    }

    public virtual void ApplyDamage(int damage)
    {
        if (this.struckSound)
        {
            AudioSource.PlayClipAtPoint(this.struckSound, this.transform.position); // play the 'player was struck' sound.
        }
        this.health = this.health - damage;
        if (this.health <= 0)
        {
            this.SendMessage("Die");
        }
    }

    public virtual void AddLife(int powerUp)
    {
        this.lives = this.lives + powerUp;
        this.health = this.maxHealth;
    }

    public virtual void AddHealth(int powerUp)
    {
        this.health = this.health + powerUp;
        if (this.health > this.maxHealth) // We can only show six segments in our HUD.
        {
            this.health = this.maxHealth;
        }
    }

    public virtual void FoundItem(int numFound)
    {
        this.remainingItems = this.remainingItems - numFound;
        if (this.remainingItems == 0)
        {
            this.StartCoroutine(this.levelStateMachine.UnlockLevelExit()); // ...and let our player out of the level.
        }
    }

    public virtual void FalloutDeath()
    {
        this.StartCoroutine(this.Die());
        return;
    }

    public virtual IEnumerator Die()
    {
        // play the death sound if available.
        if (this.deathSound)
        {
            AudioSource.PlayClipAtPoint(this.deathSound, this.transform.position);
        }
        this.lives--;
        this.health = this.maxHealth;
        if (this.lives < 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
        }
        // If we've reached here, the player still has lives remaining, so respawn.
        var respawnPosition = Respawn.currentRespawn.transform.position;
        Camera.main.transform.position = (respawnPosition - (this.transform.forward * 4)) + Vector3.up; // reset camera too
        // Hide the player briefly to give the death sound time to finish...
        this.SendMessage("HidePlayer");
        // Relocate the player. We need to do this or the camera will keep trying to focus on  the (invisible) player where he's standing on top of the FalloutDeath box collider.
        this.transform.position = respawnPosition + Vector3.up;
        yield return new WaitForSeconds(1.6f);
        // (NOTE: "HidePlayer" also disables the player controls.) // give the sound time to complete. 
        this.SendMessage("ShowPlayer"); // Show the player again, ready for... 
        // ... the respawn point to play it's particle effect
        this.StartCoroutine(Respawn.currentRespawn.FireEffect());
    }

    public virtual void LevelCompleted()
    {
        this.StartCoroutine(this.levelStateMachine.LevelCompleted());
    }

    public ThirdPersonStatus()
    {
        this.health = 6;
        this.maxHealth = 6;
        this.lives = 4;
    }

}