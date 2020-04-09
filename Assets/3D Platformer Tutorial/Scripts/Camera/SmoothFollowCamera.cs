using UnityEngine;
using System.Collections;

[System.Serializable]
/// This camera is similar to the one used in Super Mario 64
/*

"Fire2" snaps the camera

*/
// The target we are following
// The distance in the x-z plane to the target
// the height we want the camera to be above the target
 // Early out if we don't have a target
//	DebugDrawStuff();
// Calculate the current & target rotation angles
// Adjust real target angle when camera is locked
// When pressing Fire2 (alt) the camera will snap to the target direction real quick.
// It will stop snapping when it reaches the target
 // We are close to the target, so we can stop snapping now!
// Normal camera motion
// Lock the camera when moving backwards!
// * It is really confusing to do 180 degree spins when turning around.
// When jumping don't move camera upwards but only down!
 // We'd be moving the camera upwards, do that only if it's really high
// When walking always update the target height
// Damp the height
// Convert the angle into a rotation, by which we then reposition the camera
// Set the position of the camera on the x-z plane to:
// distance meters behind the target
// Set the height of the camera
// Always look at the target	
 // Now it's getting hairy. The devil is in the details here, the big issue is jumping of course.
 // * When jumping up and down we don't want to center the guy in screen space.
 //  This is important to give a feel for how high you jump and avoiding large camera movements.
 //   
 // * At the same time we dont want him to ever go out of screen and we want all rotations to be totally smooth.
 //
 // So here is what we will do:
 //
 // 1. We first find the rotation around the y axis. Thus he is always centered on the y-axis
 // 2. When grounded we make him be centered
 // 3. When jumping we keep the camera rotation but rotate the camera to get him back into view if his head is above some threshold
 // 4. When landing we smoothly interpolate towards centering him on screen
// Generate base rotation only around y-axis
// Calculate the projected center position and top position in world space
[UnityEngine.AddComponentMenu("Third Person Camera/Smooth Follow Camera")]
[UnityEngine.RequireComponent(typeof(Camera))]
public partial class SmoothFollowCamera : MonoBehaviour
{
    public Transform target;
    public float distance;
    public float height;
    public float angularSmoothLag;
    public float angularMaxSpeed;
    public float heightSmoothLag;
    public float snapSmoothLag;
    public float snapMaxSpeed;
    public float clampHeadPositionScreenSpace;
    public float lockCameraTimeout;
    private Vector3 headOffset;
    private Vector3 centerOffset;
    private float heightVelocity;
    private float angleVelocity;
    private bool snap;
    private ThirdPersonController controller;
    private float targetHeight;
    public virtual void Awake()
    {
        if (this.target)
        {
            this.controller = (ThirdPersonController) this.target.GetComponent(typeof(ThirdPersonController));
        }
        if (this.controller)
        {
            CharacterController characterController = this.target.GetComponent<Collider>().GetComponent<CharacterController>();
            this.centerOffset = characterController.bounds.center - this.target.position;
            this.headOffset = this.centerOffset;
            this.headOffset.y = characterController.bounds.max.y - this.target.position.y;
        }
        else
        {
            Debug.Log("Please assign a target to the camera that has a ThirdPersonController script attached.");
        }
        this.Cut(this.target, this.centerOffset);
    }

    public virtual void DebugDrawStuff()
    {
        Debug.DrawLine(this.target.position, this.target.position + this.headOffset);
    }

    public virtual float AngleDistance(float a, float b)
    {
        a = Mathf.Repeat(a, 360);
        b = Mathf.Repeat(b, 360);
        return Mathf.Abs(b - a);
    }

    public virtual void Apply(Transform dummyTarget, Vector3 dummyCenter)
    {
        if (!this.controller)
        {
            return;
        }
        Vector3 targetCenter = this.target.position + this.centerOffset;
        Vector3 targetHead = this.target.position + this.headOffset;
        float originalTargetAngle = this.target.eulerAngles.y;
        float currentAngle = this.transform.eulerAngles.y;
        float targetAngle = originalTargetAngle;
        if (Input.GetButton("Fire2"))
        {
            this.snap = true;
        }
        if (this.snap)
        {
            if (this.AngleDistance(currentAngle, originalTargetAngle) < 3f)
            {
                this.snap = false;
            }
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref this.angleVelocity, this.snapSmoothLag, this.snapMaxSpeed);
        }
        else
        {
            if (this.controller.GetLockCameraTimer() < this.lockCameraTimeout)
            {
                targetAngle = currentAngle;
            }
            if ((this.AngleDistance(currentAngle, targetAngle) > 160) && this.controller.IsMovingBackwards())
            {
                targetAngle = targetAngle + 180;
            }
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref this.angleVelocity, this.angularSmoothLag, this.angularMaxSpeed);
        }
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
        var currentHeight = this.transform.position.y;
        currentHeight = Mathf.SmoothDamp(currentHeight, this.targetHeight, ref this.heightVelocity, this.heightSmoothLag);
        var currentRotation = Quaternion.Euler(0, currentAngle, 0);
        this.transform.position = targetCenter;
        this.transform.position = this.transform.position + ((currentRotation * Vector3.back) * this.distance);

        {
            float _14 = currentHeight;
            Vector3 _15 = this.transform.position;
            _15.y = _14;
            this.transform.position = _15;
        }
        this.SetUpRotation(targetCenter, targetHead);
    }

    public virtual void LateUpdate()
    {
        this.Apply(this.transform, Vector3.zero);
    }

    public virtual void Cut(Transform dummyTarget, Vector3 dummyCenter)
    {
        float oldHeightSmooth = this.heightSmoothLag;
        float oldSnapMaxSpeed = this.snapMaxSpeed;
        float oldSnapSmooth = this.snapSmoothLag;
        this.snapMaxSpeed = 10000;
        this.snapSmoothLag = 0.001f;
        this.heightSmoothLag = 0.001f;
        this.snap = true;
        this.Apply(this.transform, Vector3.zero);
        this.heightSmoothLag = oldHeightSmooth;
        this.snapMaxSpeed = oldSnapMaxSpeed;
        this.snapSmoothLag = oldSnapSmooth;
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

    public virtual Vector3 GetCenterOffset()
    {
        return this.centerOffset;
    }

    public SmoothFollowCamera()
    {
        this.distance = 7f;
        this.height = 3f;
        this.angularSmoothLag = 0.3f;
        this.angularMaxSpeed = 15f;
        this.heightSmoothLag = 0.3f;
        this.snapSmoothLag = 0.2f;
        this.snapMaxSpeed = 720f;
        this.clampHeadPositionScreenSpace = 0.75f;
        this.lockCameraTimeout = 0.2f;
        this.headOffset = Vector3.zero;
        this.centerOffset = Vector3.zero;
        this.targetHeight = 100000f;
    }

}