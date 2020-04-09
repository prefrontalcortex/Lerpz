using UnityEngine;
using System.Collections;

[System.Serializable]
// This camera is similar to the one used in Jak & Dexter
// When jumping don't move camera upwards but only down!
 // We'd be moving the camera upwards, do that only if it's really high
// When walking always update the target height
// We start snapping when user pressed Fire2!
// We are close to the target, so we can stop snapping now!
 // We try to maintain a constant distance on the x-z plane with a spring.
 // Y position is handled with a seperate spring
 // Now it's getting hairy. The devil is in the details here, the big issue is jumping of course.
 // * When jumping up and down don't center the guy in screen space. This is important to give a feel for how high you jump.
 //   When keeping him centered, it is hard to see the jump.
 // * At the same time we dont want him to ever go out of screen and we want all rotations to be totally smooth
 //
 // So here is what we will do:
 //
 // 1. We first find the rotation around the y axis. Thus he is always centered on the y-axis
 // 2. When grounded we make him be cented
 // 3. When jumping we keep the camera rotation but rotate the camera to get him back into view if his head is above some threshold
 // 4. When landing we must smoothly interpolate towards centering him on screen
// Generate base rotation only around y-axis
// Calculate the projected center position and top position in world space
[UnityEngine.AddComponentMenu("Third Person Camera/Spring Follow Camera")]
public partial class SpringFollowCamera : MonoBehaviour
{
    public Transform target;
    public float distance;
    public float height;
    public float smoothLag;
    public float maxSpeed;
    public float snapLag;
    public float clampHeadPositionScreenSpace;
    public LayerMask lineOfSightMask;
    private bool isSnapping;
    private Vector3 headOffset;
    private Vector3 centerOffset;
    private ThirdPersonController controller;
    private Vector3 velocity;
    private float targetHeight;
    public virtual void Awake()
    {
        CharacterController characterController = this.target.GetComponent<Collider>().GetComponent<CharacterController>();
        if (characterController)
        {
            this.centerOffset = characterController.bounds.center - this.target.position;
            this.headOffset = this.centerOffset;
            this.headOffset.y = characterController.bounds.max.y - this.target.position.y;
        }
        if (this.target)
        {
            this.controller = (ThirdPersonController) this.target.GetComponent(typeof(ThirdPersonController));
        }
        if (!this.controller)
        {
            Debug.Log("Please assign a target to the camera that has a Third Person Controller script component.");
        }
    }

    public virtual void LateUpdate()
    {
        Vector3 targetCenter = this.target.position + this.centerOffset;
        Vector3 targetHead = this.target.position + this.headOffset;
        if (this.controller.IsJumping())
        {
            float newTargetHeight = targetCenter.y + this.height;
            if ((newTargetHeight < this.targetHeight) || ((newTargetHeight - this.targetHeight) > 5))
            {
                this.targetHeight = targetCenter.y + this.height;
            }
        }
        else
        {
            this.targetHeight = targetCenter.y + this.height;
        }
        if (Input.GetButton("Fire2") && !this.isSnapping)
        {
            this.velocity = Vector3.zero;
            this.isSnapping = true;
        }
        if (this.isSnapping)
        {
            this.ApplySnapping(targetCenter);
        }
        else
        {
            this.ApplyPositionDamping(new Vector3(targetCenter.x, this.targetHeight, targetCenter.z));
        }
        this.SetUpRotation(targetCenter, targetHead);
    }

    public virtual void ApplySnapping(Vector3 targetCenter)
    {
        Vector3 position = this.transform.position;
        Vector3 offset = position - targetCenter;
        offset.y = 0;
        float currentDistance = offset.magnitude;
        float targetAngle = this.target.eulerAngles.y;
        float currentAngle = this.transform.eulerAngles.y;
        currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref this.velocity.x, this.snapLag);
        currentDistance = Mathf.SmoothDamp(currentDistance, this.distance, ref this.velocity.z, this.snapLag);
        Vector3 newPosition = targetCenter;
        newPosition = newPosition + ((Quaternion.Euler(0, currentAngle, 0) * Vector3.back) * currentDistance);
        newPosition.y = Mathf.SmoothDamp(position.y, targetCenter.y + this.height, ref this.velocity.y, this.smoothLag, this.maxSpeed);
        newPosition = this.AdjustLineOfSight(newPosition, targetCenter);
        this.transform.position = newPosition;
        if (this.AngleDistance(currentAngle, targetAngle) < 3f)
        {
            this.isSnapping = false;
            this.velocity = Vector3.zero;
        }
    }

    public virtual Vector3 AdjustLineOfSight(Vector3 newPosition, Vector3 target)
    {
        RaycastHit hit = default(RaycastHit);
        if (Physics.Linecast(target, newPosition, out hit, this.lineOfSightMask.value))
        {
            this.velocity = Vector3.zero;
            return hit.point;
        }
        return newPosition;
    }

    public virtual void ApplyPositionDamping(Vector3 targetCenter)
    {
        Vector3 newPosition = default(Vector3);
        Vector3 position = this.transform.position;
        Vector3 offset = position - targetCenter;
        offset.y = 0;
        Vector3 newTargetPos = (offset.normalized * this.distance) + targetCenter;
        newPosition.x = Mathf.SmoothDamp(position.x, newTargetPos.x, ref this.velocity.x, this.smoothLag, this.maxSpeed);
        newPosition.z = Mathf.SmoothDamp(position.z, newTargetPos.z, ref this.velocity.z, this.smoothLag, this.maxSpeed);
        newPosition.y = Mathf.SmoothDamp(position.y, targetCenter.y, ref this.velocity.y, this.smoothLag, this.maxSpeed);
        newPosition = this.AdjustLineOfSight(newPosition, targetCenter);
        this.transform.position = newPosition;
    }

    public virtual void SetUpRotation(Vector3 centerPos, Vector3 headPos)
    {
        Vector3 cameraPos = this.transform.position;
        Vector3 offsetToCenter = centerPos - cameraPos;
        Quaternion yRotation = Quaternion.LookRotation(new Vector3(offsetToCenter.x, 0, offsetToCenter.z));
        Vector3 relativeOffset = (Vector3.forward * this.distance) + (Vector3.down * this.height);
        this.transform.rotation = yRotation * Quaternion.LookRotation(relativeOffset);
        Ray centerRay = this.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 1));
        Ray topRay = this.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, this.clampHeadPositionScreenSpace, 1));
        Vector3 centerRayPos = centerRay.GetPoint(this.distance);
        Vector3 topRayPos = topRay.GetPoint(this.distance);
        float centerToTopAngle = Vector3.Angle(centerRay.direction, topRay.direction);
        float heightToAngle = centerToTopAngle / (centerRayPos.y - topRayPos.y);
        float extraLookAngle = heightToAngle * (centerRayPos.y - centerPos.y);
        if (extraLookAngle < centerToTopAngle)
        {
            extraLookAngle = 0;
        }
        else
        {
            extraLookAngle = extraLookAngle - centerToTopAngle;
            this.transform.rotation = this.transform.rotation * Quaternion.Euler(-extraLookAngle, 0, 0);
        }
    }

    public virtual float AngleDistance(float a, float b)
    {
        a = Mathf.Repeat(a, 360);
        b = Mathf.Repeat(b, 360);
        return Mathf.Abs(b - a);
    }

    public SpringFollowCamera()
    {
        this.distance = 4f;
        this.height = 1f;
        this.smoothLag = 0.2f;
        this.maxSpeed = 10f;
        this.snapLag = 0.3f;
        this.clampHeadPositionScreenSpace = 0.75f;
        this.headOffset = Vector3.zero;
        this.centerOffset = Vector3.zero;
        this.velocity = Vector3.zero;
        this.targetHeight = 100000f;
    }

}