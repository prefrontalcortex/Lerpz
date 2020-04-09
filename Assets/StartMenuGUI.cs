using UnityEngine;
using System.Collections;

[System.Serializable]
// Make the script also execute in edit mode
[UnityEngine.ExecuteInEditMode]
public partial class StartMenuGUI : MonoBehaviour
{
    public GUISkin gSkin; // link to the LerpzTutorialSkinasset
    public Texture2D backdrop; // our backdrop image goes in here.
    private bool isLoading; // if true, we'll display the "Loading..." message.
    public virtual void OnGUI()
    {
        if (this.gSkin)
        {
            GUI.skin = this.gSkin;
        }
        else
        {
            Debug.Log("StartMenuGUI: GUI Skin object missing!");
        }
        GUIStyle backgroundStyle = new GUIStyle();
        backgroundStyle.normal.background = this.backdrop;
        GUI.Label(new Rect((Screen.width - (Screen.height * 2)) * 0.75f, 0, Screen.height * 2, Screen.height), "", backgroundStyle);
        GUI.Label(new Rect((Screen.width / 2) - 197, 50, 400, 100), "Lerpz Escapes", "mainMenuTitle");
        if (GUI.Button(new Rect((Screen.width / 2) - 70, Screen.height - 160, 140, 70), "Play"))
        {
            this.isLoading = true;
            UnityEngine.SceneManagement.SceneManager.LoadScene("TheGame"); // load the game level. 
        }
        bool isWebPlayer = false;
        if (!isWebPlayer)
        {
            if (GUI.Button(new Rect((Screen.width / 2) - 70, Screen.height - 80, 140, 70), "Quit"))
            {
                Application.Quit();
            }
        }
        if (this.isLoading)
        {
            GUI.Label(new Rect((Screen.width / 2) - 110, (Screen.height / 2) - 60, 400, 70), "Loading...", "mainMenuTitle");
        }
    }

}