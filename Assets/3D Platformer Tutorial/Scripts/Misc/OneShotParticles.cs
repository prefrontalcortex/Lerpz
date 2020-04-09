using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class OneShotParticles : MonoBehaviour
{
    public virtual IEnumerator Start()
    {
        yield return new WaitForSeconds(this.GetComponent<ParticleEmitter>().minEnergy / 2);
        this.GetComponent<ParticleEmitter>().emit = false;
    }

}