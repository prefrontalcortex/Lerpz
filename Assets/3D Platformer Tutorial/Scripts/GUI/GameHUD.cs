using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class GameHUD : MonoBehaviour
{
    // GameHUD: Platformer Tutorial Master GUI script.
    // This script handles the in-game HUD, showing the lives, number of fuel cells remaining, etc.
    public GUISkin guiSkin;
    public float nativeVerticalResolution;
    // main decoration textures:
    public Texture2D healthImage;
    public Vector2 healthImageOffset;
    // the health 'pie chart' assets consist of six textures with alpha channels. Only one is ever shown:
    public Texture2D[] healthPieImages;
    public Vector2 healthPieImageOffset;
    // the lives count is displayed in the health image as a text counter
    public Vector2 livesCountOffset;
    // The fuel cell decoration image on the right side
    public Texture2D fuelCellsImage;
    public Vector2 fuelCellOffset;
    // The counter text inside the fuel cell image
    public Vector2 fuelCellCountOffset;
    private ThirdPersonStatus playerInfo;
    // Cache link to player's state management script for later use.
    public virtual void Awake()
    {
        this.playerInfo = (ThirdPersonStatus) UnityEngine.Object.FindObjectOfType(typeof(ThirdPersonStatus));
        if (!this.playerInfo)
        {
            Debug.Log("No link to player's state manager.");
        }
    }

    public virtual void OnGUI()
    {
        int itemsLeft = this.playerInfo.GetRemainingItems(); // fetch items remaining -- the fuel cans. This can be a negative number!
        // Similarly, health needs to be clamped to the number of pie segments we can show.
        // We also need to check it's not negative, so we'll use the Mathf Clamp() function:
        int healthPieIndex = Mathf.Clamp(this.playerInfo.health, 0, this.healthPieImages.Length);
        // Displays fuel cans remaining as a number.	
        // As we don't want to display negative numbers, we clamp the value to zero if it drops below this:
        if (itemsLeft < 0)
        {
            itemsLeft = 0;
        }
        // Set up gui skin
        GUI.skin = this.guiSkin;
        // Our GUI is laid out for a 1920 x 1200 pixel display (16:10 aspect). The next line makes sure it rescales nicely to other resolutions.
        GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(Screen.height / this.nativeVerticalResolution, Screen.height / this.nativeVerticalResolution, 1));
        // Health & lives info.
        this.DrawImageBottomAligned(this.healthImageOffset, this.healthImage); // main image.
        // now for the pie chart. This is where a decent graphics package comes in handy to check relative sizes and offsets.
        Texture2D pieImage = this.healthPieImages[healthPieIndex - 1];
        this.DrawImageBottomAligned(this.healthPieImageOffset, pieImage);
        // Displays lives left as a number.	
        this.DrawLabelBottomAligned(this.livesCountOffset, this.playerInfo.lives.ToString());
        // Now it's the fuel cans' turn. We want this aligned to the lower-right corner of the screen:
        this.DrawImageBottomRightAligned(this.fuelCellOffset, this.fuelCellsImage);
        this.DrawLabelBottomRightAligned(this.fuelCellCountOffset, itemsLeft.ToString());
    }

    public virtual void DrawImageBottomAligned(Vector2 pos, Texture2D image)
    {
        GUI.Label(new Rect(pos.x, (this.nativeVerticalResolution - image.height) - pos.y, image.width, image.height), image);
    }

    public virtual void DrawLabelBottomAligned(Vector2 pos, string text)
    {
        GUI.Label(new Rect(pos.x, this.nativeVerticalResolution - pos.y, 100, 100), text);
    }

    public virtual void DrawImageBottomRightAligned(Vector2 pos, Texture2D image)
    {
        float scaledResolutionWidth = (this.nativeVerticalResolution / Screen.height) * Screen.width;
        GUI.Label(new Rect((scaledResolutionWidth - pos.x) - image.width, (this.nativeVerticalResolution - image.height) - pos.y, image.width, image.height), image);
    }

    public virtual void DrawLabelBottomRightAligned(Vector2 pos, string text)
    {
        float scaledResolutionWidth = (this.nativeVerticalResolution / Screen.height) * Screen.width;
        GUI.Label(new Rect(scaledResolutionWidth - pos.x, this.nativeVerticalResolution - pos.y, 100, 100), text);
    }

    public GameHUD()
    {
        this.nativeVerticalResolution = 1200f;
        this.healthImageOffset = new Vector2(0, 0);
        this.healthPieImageOffset = new Vector2(10, 147);
        this.livesCountOffset = new Vector2(425, 160);
        this.fuelCellOffset = new Vector2(0, 0);
        this.fuelCellCountOffset = new Vector2(391, 161);
    }

}