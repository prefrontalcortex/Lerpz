var runSpeedScale = 1.0;
var walkSpeedScale = 1.0;

var anim : Animation;

function Start ()
{
	anim = GetComponent.<Animation>();

	// By default loop all animations
	anim.wrapMode = WrapMode.Loop;

	anim["run"].layer = -1;
	anim["walk"].layer = -1;
	anim["idle"].layer = -2;
	anim.SyncLayer(-1);

	anim["ledgefall"].layer = 9;	
	anim["ledgefall"].wrapMode = WrapMode.Loop;


	// The jump animation is clamped and overrides all others
	anim["jump"].layer = 10;
	anim["jump"].wrapMode = WrapMode.ClampForever;

	anim["jumpfall"].layer = 10;	
	anim["jumpfall"].wrapMode = WrapMode.ClampForever;

	// This is the jet-pack controlled descent animation.
	anim["jetpackjump"].layer = 10;	
	anim["jetpackjump"].wrapMode = WrapMode.ClampForever;

	anim["jumpland"].layer = 10;	
	anim["jumpland"].wrapMode = WrapMode.Once;

	anim["walljump"].layer = 11;	
	anim["walljump"].wrapMode = WrapMode.Once;

	// we actually use this as a "got hit" animation
	anim["buttstomp"].speed = 0.15;
	anim["buttstomp"].layer = 20;
	anim["buttstomp"].wrapMode = WrapMode.Once;	
	var punch = anim["punch"];
	punch.wrapMode = WrapMode.Once;

	// We are in full control here - don't let any other animations play when we start
	anim.Stop();
	anim.Play("idle");
}

function Update ()
{
	var playerController : ThirdPersonController = GetComponent(ThirdPersonController);
	var currentSpeed = playerController.GetSpeed();

	// Fade in run
	if (currentSpeed > playerController.walkSpeed)
	{
		anim.CrossFade("run");
		// We fade out jumpland quick otherwise we get sliding feet
		anim.Blend("jumpland", 0);
	}
	// Fade in walk
	else if (currentSpeed > 0.1)
	{
		anim.CrossFade("walk");
		// We fade out jumpland realy quick otherwise we get sliding feet
		anim.Blend("jumpland", 0);
	}
	// Fade out walk and run
	else
	{
		anim.Blend("walk", 0.0, 0.3);
		anim.Blend("run", 0.0, 0.3);
		anim.Blend("run", 0.0, 0.3);
	}
	
	anim["run"].normalizedSpeed = runSpeedScale;
	anim["walk"].normalizedSpeed = walkSpeedScale;
	
	if (playerController.IsJumping ())
	{
		if (playerController.IsControlledDescent())
		{
			anim.CrossFade ("jetpackjump", 0.2);
		}
		else if (playerController.HasJumpReachedApex ())
		{
			anim.CrossFade ("jumpfall", 0.2);
		}
		else
		{
			anim.CrossFade ("jump", 0.2);
		}
	}
	// We fell down somewhere
	else if (!playerController.IsGroundedWithTimeout())
	{
		anim.CrossFade ("ledgefall", 0.2);
	}
	// We are not falling down anymore
	else
	{
		anim.Blend ("ledgefall", 0.0, 0.2);
	}
}

function DidLand () {
	anim.Play("jumpland");
}

function DidButtStomp () {
	anim.CrossFade("buttstomp", 0.1);
	anim.CrossFadeQueued("jumpland", 0.2);
}

function Slam () {
	anim.CrossFade("buttstomp", 0.2);
	var playerController : ThirdPersonController = GetComponent(ThirdPersonController);
	while(!playerController.IsGrounded())
	{
		yield;	
	}
	anim.Blend("buttstomp", 0, 0);
}


function DidWallJump ()
{
	// Wall jump animation is played without fade.
	// We are turning the character controller 180 degrees around when doing a wall jump so the animation accounts for that.
	// But we really have to make sure that the animation is in full control so 
	// that we don't do weird blends between 180 degree apart rotations
	anim.Play ("walljump");
}

@script AddComponentMenu ("Third Person Player/Third Person Player Animation")