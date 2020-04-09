using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class LevelStatus : MonoBehaviour
{
    // LevelStatus: Master level state machine script.
    public GameObject exitGateway;
    public GameObject levelGoal;
    public AudioClip unlockedSound;
    public AudioClip levelCompleteSound;
    public GameObject mainCamera;
    public GameObject unlockedCamera;
    public GameObject levelCompletedCamera;
    // This is where info like the number of items the player must collect in order to complete the level lives.
    public int itemsNeeded; // This is how many fuel canisters the player must collect.
    private GameObject playerLink;
    // Awake(): Called by Unity when the script has loaded.
    // We use this function to initialise our link to the Lerpz GameObject.
    public virtual void Awake()
    {
        ((MeshCollider) this.levelGoal.GetComponent(typeof(MeshCollider))).isTrigger = false;
        this.playerLink = GameObject.Find("Player");
        if (!this.playerLink)
        {
            Debug.Log("Could not get link to Lerpz");
        }
        ((MeshCollider) this.levelGoal.GetComponent(typeof(MeshCollider))).isTrigger = false; // make very sure of this!
    }

    public virtual IEnumerator UnlockLevelExit()
    {
        ((AudioListener) this.mainCamera.GetComponent(typeof(AudioListener))).enabled = false;
        this.unlockedCamera.SetActive(true);
        ((AudioListener) this.unlockedCamera.GetComponent(typeof(AudioListener))).enabled = true;
        ((AudioSource) this.exitGateway.GetComponent(typeof(AudioSource))).Stop();
        if (this.unlockedSound)
        {
            AudioSource.PlayClipAtPoint(this.unlockedSound, ((Transform) this.unlockedCamera.GetComponent(typeof(Transform))).position, 2f);
        }
        yield return new WaitForSeconds(1);
        this.exitGateway.SetActive(false); // ... the fence goes down briefly...
        yield return new WaitForSeconds(0.2f); //... pause for a fraction of a second...
        this.exitGateway.SetActive(true); //... now the fence flashes back on again...
        yield return new WaitForSeconds(0.2f); //... another brief pause before...
        this.exitGateway.SetActive(false); //... the fence finally goes down forever!
        ((MeshCollider) this.levelGoal.GetComponent(typeof(MeshCollider))).isTrigger = true;
        yield return new WaitForSeconds(4);
        // swap the cameras back. // give the player time to see the result.
        this.unlockedCamera.SetActive(false); // this lets the NearCamera get the screen all to itself.
        ((AudioListener) this.unlockedCamera.GetComponent(typeof(AudioListener))).enabled = false;
        ((AudioListener) this.mainCamera.GetComponent(typeof(AudioListener))).enabled = true;
    }

    public virtual IEnumerator LevelCompleted()
    {
        ((AudioListener) this.mainCamera.GetComponent(typeof(AudioListener))).enabled = false;
        this.levelCompletedCamera.SetActive(true);
        ((AudioListener) this.levelCompletedCamera.GetComponent(typeof(AudioListener))).enabled = true;
        ((ThirdPersonController) this.playerLink.GetComponent(typeof(ThirdPersonController))).SendMessage("HidePlayer");
        this.playerLink.transform.position = this.playerLink.transform.position + (Vector3.up * 500f); // just move him 500 units
        if (this.levelCompleteSound)
        {
            AudioSource.PlayClipAtPoint(this.levelCompleteSound, this.levelGoal.transform.position, 2f);
        }
        this.levelGoal.GetComponent<Animation>().Play();
        yield return new WaitForSeconds(this.levelGoal.GetComponent<Animation>().clip.length);
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver"); //...just show the Game Over sequence.
    }

    public LevelStatus()
    {
        this.itemsNeeded = 20;
    }

}