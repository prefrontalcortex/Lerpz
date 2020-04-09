using UnityEngine;
using System.Collections;

[System.Serializable]
// The script ensures an AudioSource component is always attached.
// First, we make sure the AudioSource component is initialized correctly:
// Init the particles to not emit and switch off the spotlights:
// Once every frame  update particle emission and lights
// handle thruster sound effect
[UnityEngine.RequireComponent(typeof(AudioSource))]
public partial class JetPackParticleController : MonoBehaviour
{
    private float litAmount;
    public virtual IEnumerator Start()
    {
        ThirdPersonController playerController = (ThirdPersonController) this.GetComponent(typeof(ThirdPersonController));
        this.GetComponent<AudioSource>().loop = false;
        this.GetComponent<AudioSource>().Stop();
        var particles = this.GetComponentsInChildren(typeof(ParticleSystem));
        Light childLight = (Light) this.GetComponentInChildren(typeof(Light));
        foreach (ParticleSystem p in particles)
        {
            var e = p.emission;
            e.enabled = false;
        }
        childLight.enabled = false;
        while (true)
        {
            bool isFlying = playerController.IsJumping();
            if (isFlying)
            {
                if (!this.GetComponent<AudioSource>().isPlaying)
                {
                    this.GetComponent<AudioSource>().Play();
                }
            }
            else
            {
                this.GetComponent<AudioSource>().Stop();
            }
            foreach (ParticleSystem p in particles)
            {
                var e = p.emission;
                e.enabled = isFlying;
            }
            if (isFlying)
            {
                this.litAmount = Mathf.Clamp01(this.litAmount + (Time.deltaTime * 2));
            }
            else
            {
                this.litAmount = Mathf.Clamp01(this.litAmount - (Time.deltaTime * 2));
            }
            childLight.enabled = isFlying;
            childLight.intensity = this.litAmount;
            yield return null;
        }
    }

}