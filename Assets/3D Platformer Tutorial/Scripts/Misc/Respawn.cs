using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class Respawn : MonoBehaviour
{
    /*
Respawn: Allows players to respawn to this point in the level, effectively saving their progress.

The Respawn object has three main states and one interim state: Inactive, Active and Respawn, plus Triggered.

- Inactive: Player hasn't reached this point and the player will not respawn here.

- Active: Player has touched this respawn point, so the player will respawn here.

- Respawn: Player is respawning at this respawn point.

Each state has its own visual effect(s).

Respawn objects also require a simple collider, so the player can activate them. The collider is set as a trigger.

*/
    public Respawn initialRespawn; // set this to the initial respawn point for the level.
    public int RespawnState;
    // Sound effects:
    public AudioClip SFXPlayerRespawn;
    public AudioClip SFXRespawnActivate;
    public AudioClip SFXRespawnActiveLoop;
    public float SFXVolume; // volume for one-shot sounds.
    // references for the various particle emitters...
    private ParticleEmitter emitterActive;
    private ParticleEmitter emitterInactive;
    private ParticleEmitter emitterRespawn1;
    private ParticleEmitter emitterRespawn2;
    private ParticleEmitter emitterRespawn3;
    // ...and for the light:
    private Light respawnLight;
    // The currently active respawn point. Static, so all instances of this script will share this variable.
    public static Respawn currentRespawn;
    public virtual void Start()
    {
         // Get some of the objects we need later.
         // This is often done in a script's Start function. That way, we've got all our initialization code in one place, 
         // And can simply count on the code being fine.
        this.emitterActive = (ParticleEmitter) this.transform.Find("RSParticlesActive").GetComponent(typeof(ParticleEmitter));
        this.emitterInactive = (ParticleEmitter) this.transform.Find("RSParticlesInactive").GetComponent(typeof(ParticleEmitter));
        this.emitterRespawn1 = (ParticleEmitter) this.transform.Find("RSParticlesRespawn1").GetComponent(typeof(ParticleEmitter));
        this.emitterRespawn2 = (ParticleEmitter) this.transform.Find("RSParticlesRespawn2").GetComponent(typeof(ParticleEmitter));
        this.emitterRespawn3 = (ParticleEmitter) this.transform.Find("RSParticlesRespawn3").GetComponent(typeof(ParticleEmitter));
        this.respawnLight = (Light) this.transform.Find("RSSpotlight").GetComponent(typeof(Light));
        this.RespawnState = 0;
        // set up the looping "RespawnActive" sound, but leave it switched off for now:
        if (this.SFXRespawnActiveLoop)
        {
            this.GetComponent<AudioSource>().clip = this.SFXRespawnActiveLoop;
            this.GetComponent<AudioSource>().loop = true;
            this.GetComponent<AudioSource>().playOnAwake = false;
        }
        // Assign the respawn point to be this one - Since the player is positioned on top of a respawn point, it will come in and overwrite it.
        // This is just to make sure that we always have a respawn point.
        Respawn.currentRespawn = this.initialRespawn;
        if (Respawn.currentRespawn == this)
        {
            this.SetActive();
        }
    }

    public virtual void OnTriggerEnter()
    {
        if (Respawn.currentRespawn != this) // make sure we're not respawning or re-activating an already active pad!
        {
             // turn the old respawn point off
            Respawn.currentRespawn.SetInactive();
            // play the "Activated" one-shot sound effect if one has been supplied:
            if (this.SFXRespawnActivate)
            {
                AudioSource.PlayClipAtPoint(this.SFXRespawnActivate, this.transform.position, this.SFXVolume);
            }
            // Set the current respawn point to be us and make it visible.
            Respawn.currentRespawn = this;
            this.SetActive();
        }
    }

    public virtual void SetActive()
    {
        this.emitterActive.emit = true;
        this.emitterInactive.emit = false;
        this.respawnLight.intensity = 1.5f;
        this.GetComponent<AudioSource>().Play(); // start playing the sound clip assigned in the inspector
    }

    public virtual void SetInactive()
    {
        this.emitterActive.emit = false;
        this.emitterInactive.emit = true;
        this.respawnLight.intensity = 1.5f;
        this.GetComponent<AudioSource>().Stop(); // stop playing the active sound clip.			
    }

    public virtual IEnumerator FireEffect()
    {
         // Launch all 3 sets of particle systems.
        this.emitterRespawn1.Emit();
        this.emitterRespawn2.Emit();
        this.emitterRespawn3.Emit();
        this.respawnLight.intensity = 3.5f;
        if (this.SFXPlayerRespawn)
        {
             // if we have a 'player is respawning' sound effect, play it now.
            AudioSource.PlayClipAtPoint(this.SFXPlayerRespawn, this.transform.position, this.SFXVolume);
        }
        yield return new WaitForSeconds(2);
        this.respawnLight.intensity = 2f;
    }

}