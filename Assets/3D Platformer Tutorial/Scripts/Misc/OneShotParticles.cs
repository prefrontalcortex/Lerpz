using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class OneShotParticles : MonoBehaviour
{
    public virtual IEnumerator Start()
    {
        yield return new WaitForSeconds(this.GetComponent<ParticleSystem>().main.startLifetime.Evaluate(1f) / 2);
        var e = this.GetComponent<ParticleSystem>().emission;
        e.enabled = false;
    }

}