var scrollSpeed = 0.25;

function FixedUpdate() 
{
    var offset = Time.time * scrollSpeed;
    GetComponent.<Renderer>().material.mainTextureOffset = Vector2(offset,offset);
}