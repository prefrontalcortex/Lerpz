function OnTriggerEnter (col : Collider)
{
 playerLink=col.GetComponent(ThirdPersonStatus);
 if (!playerLink) // not the player.
{
  return;
}
 else
{
  playerLink.LevelCompleted();
}
}