using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
// Attach this to a GUIText to make a frames/second indicator.
//
// It calculates frames/second over each updateInterval,
// so the display does not keep changing wildly.
//
// It is also fairly accurate at very low FPS counts (<10).
// We do this not by simply counting frames per interval, but
// by accumulating FPS for each frame. This way we end up with
// correct overall FPS even if the interval renders something like
// 5.5 frames.
 // FPS accumulated over the interval
 // Frames drawn over the interval
 // Left time for current interval
// Interval ended - update GUI text and start new interval
 // display two fractional digits (f2 format)
[UnityEngine.RequireComponent(typeof(Text))]
public partial class FPS : MonoBehaviour
{
    public float updateInterval;
    private float accum;
    private int frames;
    private float timeleft;
    public virtual void Start()
    {
        if (!this.GetComponent<Text>())
        {
            MonoBehaviour.print("FramesPerSecond needs a Text component!");
            this.enabled = false;
            return;
        }
        this.timeleft = this.updateInterval;
    }

    public virtual void Update()
    {
        this.timeleft = this.timeleft - Time.deltaTime;
        this.accum = this.accum + (Time.timeScale / Time.deltaTime);
        ++this.frames;
        if (this.timeleft <= 0f)
        {
            this.GetComponent<Text>().text = "" + (this.accum / this.frames).ToString("f2");
            this.timeleft = this.updateInterval;
            this.accum = 0f;
            this.frames = 0;
        }
    }

    public FPS()
    {
        this.updateInterval = 0.5f;
    }

}