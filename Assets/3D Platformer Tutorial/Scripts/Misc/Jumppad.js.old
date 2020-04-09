var jumpHeight = 5.0;

function OnTriggerEnter (col : Collider) 
{
	var controller : ThirdPersonController = col.GetComponent(ThirdPersonController);
	if (controller != null)
	{
		if (GetComponent.<AudioSource>()) 
		{
			GetComponent.<AudioSource>().Play();
		}

		controller.SuperJump(jumpHeight);
	}
	
}


// Auto setup the script and associated trigger.
function Reset ()
{
	if (GetComponent.<Collider>() == null)	
		gameObject.AddComponent(BoxCollider);
	GetComponent.<Collider>().isTrigger = true;
}

@script RequireComponent(BoxCollider)
@script AddComponentMenu("Third Person Props/Jump pad")