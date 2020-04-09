function LateUpdate () 
{
 if (!GetComponent.<AudioSource>().isPlaying || Input.anyKeyDown)
  Application.LoadLevel("StartMenu");
}
