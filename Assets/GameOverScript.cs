using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class GameOverScript : MonoBehaviour
{
    public virtual void LateUpdate()
    {
        if (!this.GetComponent<AudioSource>().isPlaying || Input.anyKeyDown)
        {
            Application.LoadLevel("StartMenu");
        }
    }

}