using UnityEngine;
using System.Collections;

[System.Serializable]
// By default loop all animations
// The jump animation is clamped and overrides all others
// This is the jet-pack controlled descent animation.
// we actually use this as a "got hit" animation
// We are in full control here - don't let any other animations play when we start
// Fade in run
// We fade out jumpland quick otherwise we get sliding feet
// Fade in walk
// We fade out jumpland realy quick otherwise we get sliding feet
// Fade out walk and run
// We fell down somewhere
// We are not falling down anymore
 // Wall jump animation is played without fade.
 // We are turning the character controller 180 degrees around when doing a wall jump so the animation accounts for that.
 // But we really have to make sure that the animation is in full control so 
 // that we don't do weird blends between 180 degree apart rotations
[UnityEngine.AddComponentMenu("Third Person Player/Third Person Player Animation")]
public partial class ThirdPersonPlayerAnimation : MonoBehaviour
{
    public float runSpeedScale;
    public float walkSpeedScale;
    public Animation anim;
    public virtual void Start()
    {
        this.anim = this.GetComponent<Animation>();
        this.anim.wrapMode = WrapMode.Loop;
        this.anim["run"].layer = -1;
        this.anim["walk"].layer = -1;
        this.anim["idle"].layer = -2;
        this.anim.SyncLayer(-1);
        this.anim["ledgefall"].layer = 9;
        this.anim["ledgefall"].wrapMode = WrapMode.Loop;
        this.anim["jump"].layer = 10;
        this.anim["jump"].wrapMode = WrapMode.ClampForever;
        this.anim["jumpfall"].layer = 10;
        this.anim["jumpfall"].wrapMode = WrapMode.ClampForever;
        this.anim["jetpackjump"].layer = 10;
        this.anim["jetpackjump"].wrapMode = WrapMode.ClampForever;
        this.anim["jumpland"].layer = 10;
        this.anim["jumpland"].wrapMode = WrapMode.Once;
        this.anim["walljump"].layer = 11;
        this.anim["walljump"].wrapMode = WrapMode.Once;
        this.anim["buttstomp"].speed = 0.15f;
        this.anim["buttstomp"].layer = 20;
        this.anim["buttstomp"].wrapMode = WrapMode.Once;
        AnimationState punch = this.anim["punch"];
        punch.wrapMode = WrapMode.Once;
        this.anim.Stop();
        this.anim.Play("idle");
    }

    public virtual void Update()
    {
        ThirdPersonController playerController = (ThirdPersonController) this.GetComponent(typeof(ThirdPersonController));
        float currentSpeed = playerController.GetSpeed();
        if (currentSpeed > playerController.walkSpeed)
        {
            this.anim.CrossFade("run");
            this.anim.Blend("jumpland", 0);
        }
        else
        {
            if (currentSpeed > 0.1f)
            {
                this.anim.CrossFade("walk");
                this.anim.Blend("jumpland", 0);
            }
            else
            {
                this.anim.Blend("walk", 0f, 0.3f);
                this.anim.Blend("run", 0f, 0.3f);
                this.anim.Blend("run", 0f, 0.3f);
            }
        }
        this.anim["run"].normalizedSpeed = this.runSpeedScale;
        this.anim["walk"].normalizedSpeed = this.walkSpeedScale;
        if (playerController.IsJumping())
        {
            if (playerController.IsControlledDescent())
            {
                this.anim.CrossFade("jetpackjump", 0.2f);
            }
            else
            {
                if (playerController.HasJumpReachedApex())
                {
                    this.anim.CrossFade("jumpfall", 0.2f);
                }
                else
                {
                    this.anim.CrossFade("jump", 0.2f);
                }
            }
        }
        else
        {
            if (!playerController.IsGroundedWithTimeout())
            {
                this.anim.CrossFade("ledgefall", 0.2f);
            }
            else
            {
                this.anim.Blend("ledgefall", 0f, 0.2f);
            }
        }
    }

    public virtual void DidLand()
    {
        this.anim.Play("jumpland");
    }

    public virtual void DidButtStomp()
    {
        this.anim.CrossFade("buttstomp", 0.1f);
        this.anim.CrossFadeQueued("jumpland", 0.2f);
    }

    public virtual IEnumerator Slam()
    {
        this.anim.CrossFade("buttstomp", 0.2f);
        ThirdPersonController playerController = (ThirdPersonController) this.GetComponent(typeof(ThirdPersonController));
        while (!playerController.IsGrounded())
        {
            yield return null;
        }
        this.anim.Blend("buttstomp", 0, 0);
    }

    public virtual void DidWallJump()
    {
        this.anim.Play("walljump");
    }

    public ThirdPersonPlayerAnimation()
    {
        this.runSpeedScale = 1f;
        this.walkSpeedScale = 1f;
    }

}