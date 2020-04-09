using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
public partial class GameOverGUI : MonoBehaviour
{
    public GUIStyle background;
    public GUIStyle gameOverText;
    public GUIStyle gameOverShadow;
    public float gameOverScale;
    public float gameOverShadowScale;
    public virtual void OnGUI()
    {
        GUI.Label(new Rect((Screen.width - (Screen.height * 2)) * 0.75f, 0, Screen.height * 2, Screen.height), "", this.background);
        GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, Vector3.one * this.gameOverShadowScale);
        GUI.Label(new Rect((Screen.width / (2 * this.gameOverShadowScale)) - 150, (Screen.height / (2 * this.gameOverShadowScale)) - 40, 300, 100), "Game Over", this.gameOverShadow);
        GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, Vector3.one * this.gameOverScale);
        GUI.Label(new Rect((Screen.width / (2 * this.gameOverScale)) - 150, (Screen.height / (2 * this.gameOverScale)) - 40, 300, 100), "Game Over", this.gameOverText);
    }

    public GameOverGUI()
    {
        this.gameOverScale = 1.5f;
        this.gameOverShadowScale = 1.5f;
    }

}