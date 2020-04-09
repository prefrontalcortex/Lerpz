using UnityEngine;
using System.Collections;

[System.Serializable]
/*
	animations played are:
	idle, threaten, turnjump, attackrun
*/
// sounds
 // played during "idle" state.
 // played during the seek and attack modes.
// Cache a reference to the controller
// Cache a link to LevelStatus state machine script:
// Setup animations
// initialize audio clip. Make sure it's set to the "idle" sound.
// Just attack for now
 // Don't do anything when idle. And wait for player to be in range!
 // This is the perfect time for the player to attack us
// Prepare, turn to player and attack him
 // if idling sound isn't already set up, set it and start it playing.
 // play the idle sound.
// Don't do anything when idle
// The perfect time for the player to attack us
// And if the player is really far away.
// We just idle around until he comes back
// unless we're dying, in which case we just keep idling.
// if player is in range again, stop lazyness
// Good Hunting!		
 // Compute relative point and get the angle towards it
// Clamp it with the max rotation speed
// Rotate
// Return the current angle
 // stop the idling audio so we can switch out the audio clip.
 // change the clip, then play
// Already queue up the attack run animation but set it's blend wieght to 0
// it gets blended in later
// it is looping so it will keep playing until we stop it.
// First we wait for a bit so the player can prepare while we turn around
// As we near an angle of 0, we will begin to move
// depending on the angle, start moving
// Run towards player
// The angle of our forward direction and the player position is larger than 50 degrees
// That means he is out of sight
// If we lost sight then we keep running for some more time (extraRunTime). 
// then stop attacking 
// Just move forward at constant speed
// Keep looking if we are hitting our target
// If we are, knock them out of the way dealing damage
 // deal damage
// knock the player back and to the side
// We are not actually moving forward.
// This probably means we ran into a wall or something. Stop attacking the player.
// yield for one frame
// Now we can go back to playing the idle animation
[UnityEngine.RequireComponent(typeof(AudioSource))]
public partial class EnemyPoliceGuy : MonoBehaviour
{
    public float attackTurnTime;
    public float rotateSpeed;
    public float attackDistance;
    public float extraRunTime;
    public int damage;
    public float attackSpeed;
    public float attackRotateSpeed;
    public float idleTime;
    public Vector3 punchPosition;
    public float punchRadius;
    public AudioClip idleSound;
    public AudioClip attackSound;
    private float attackAngle;
    private bool isAttacking;
    private float lastPunchTime;
    public Transform target;
    public Animation anim;
    private CharacterController characterController;
    public LevelStatus levelStateMachine;
    public virtual IEnumerator Start()
    {
        this.characterController = (CharacterController) this.GetComponent(typeof(CharacterController));
        this.levelStateMachine = (LevelStatus) GameObject.Find("/Level").GetComponent(typeof(LevelStatus));
        if (!this.levelStateMachine)
        {
            Debug.Log("EnemyPoliceGuy: ERROR! NO LEVEL STATUS SCRIPT FOUND.");
        }
        if (!this.target)
        {
            this.target = GameObject.FindWithTag("Player").transform;
        }
        this.anim = this.GetComponent<Animation>();
        this.anim.wrapMode = WrapMode.Loop;
        this.anim.Play("idle");
        this.anim["threaten"].wrapMode = WrapMode.Once;
        this.anim["turnjump"].wrapMode = WrapMode.Once;
        this.anim["gothit"].wrapMode = WrapMode.Once;
        this.anim["gothit"].layer = 1;
        this.GetComponent<AudioSource>().clip = this.idleSound;
        yield return new WaitForSeconds(Random.value);
        while (true)
        {
            yield return this.StartCoroutine(this.Idle());
            yield return this.StartCoroutine(this.Attack());
        }
    }

    public virtual IEnumerator Idle()
    {
        if (this.idleSound)
        {
            if (this.GetComponent<AudioSource>().clip != this.idleSound)
            {
                this.GetComponent<AudioSource>().Stop();
                this.GetComponent<AudioSource>().clip = this.idleSound;
                this.GetComponent<AudioSource>().loop = true;
                this.GetComponent<AudioSource>().Play();
            }
        }
        yield return new WaitForSeconds(this.idleTime);
        while (true)
        {
            this.characterController.SimpleMove(Vector3.zero);
            yield return new WaitForSeconds(0.2f);
            Vector3 offset = this.transform.position - this.target.position;
            if (offset.magnitude < this.attackDistance)
            {
                yield break;
            }
        }
    }

    public virtual float RotateTowardsPosition(Vector3 targetPos, float rotateSpeed)
    {
        Vector3 relative = this.transform.InverseTransformPoint(targetPos);
        float angle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;
        float maxRotation = rotateSpeed * Time.deltaTime;
        float clampedAngle = Mathf.Clamp(angle, -maxRotation, maxRotation);
        this.transform.Rotate(0, clampedAngle, 0);
        return angle;
    }

    public virtual IEnumerator Attack()
    {
        float angle = 0.0f;
        float time = 0.0f;
        Vector3 direction = default(Vector3);
        this.isAttacking = true;
        if (this.attackSound)
        {
            if (this.GetComponent<AudioSource>().clip != this.attackSound)
            {
                this.GetComponent<AudioSource>().Stop();
                this.GetComponent<AudioSource>().clip = this.attackSound;
                this.GetComponent<AudioSource>().loop = true;
                this.GetComponent<AudioSource>().Play();
            }
        }
        this.anim.Play("attackrun");
        angle = 180f;
        time = 0f;
        while ((angle > 5) || (time < this.attackTurnTime))
        {
            time = time + Time.deltaTime;
            angle = Mathf.Abs(this.RotateTowardsPosition(this.target.position, this.rotateSpeed));
            var move = Mathf.Clamp01((90 - angle) / 90);
            this.anim["attackrun"].weight = this.anim["attackrun"].speed = move;
            direction = this.transform.TransformDirection((Vector3.forward * this.attackSpeed) * move);
            this.characterController.SimpleMove(direction);
            yield return null;
        }
        float timer = 0f;
        bool lostSight = false;
        while (timer < this.extraRunTime)
        {
            angle = this.RotateTowardsPosition(this.target.position, this.attackRotateSpeed);
            if (Mathf.Abs(angle) > 40)
            {
                lostSight = true;
            }
            if (lostSight)
            {
                timer = timer + Time.deltaTime;
            }
            direction = this.transform.TransformDirection(Vector3.forward * this.attackSpeed);
            this.characterController.SimpleMove(direction);
            Vector3 pos = this.transform.TransformPoint(this.punchPosition);
            if ((Time.time > (this.lastPunchTime + 0.3f)) && ((pos - this.target.position).magnitude < this.punchRadius))
            {
                this.target.SendMessage("ApplyDamage", this.damage);
                Vector3 slamDirection = this.transform.InverseTransformDirection(this.target.position - this.transform.position);
                slamDirection.y = 0;
                slamDirection.z = 1;
                if (slamDirection.x >= 0)
                {
                    slamDirection.x = 1;
                }
                else
                {
                    slamDirection.x = -1;
                }
                this.target.SendMessage("Slam", this.transform.TransformDirection(slamDirection));
                this.lastPunchTime = Time.time;
            }
            if (this.characterController.velocity.magnitude < (this.attackSpeed * 0.3f))
            {
                break;
            }
            yield return null;
        }
        this.isAttacking = false;
        this.anim.CrossFade("idle");
    }

    public virtual void ApplyDamage()
    {
        this.anim.CrossFade("gothit");
    }

    public virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.TransformPoint(this.punchPosition), this.punchRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, this.attackDistance);
    }

    public EnemyPoliceGuy()
    {
        this.attackTurnTime = 0.7f;
        this.rotateSpeed = 120f;
        this.attackDistance = 17f;
        this.extraRunTime = 2f;
        this.damage = 1;
        this.attackSpeed = 5f;
        this.attackRotateSpeed = 20f;
        this.idleTime = 1.6f;
        this.punchPosition = new Vector3(0.4f, 0, 0.7f);
        this.punchRadius = 1.1f;
        this.attackAngle = 10f;
    }

}