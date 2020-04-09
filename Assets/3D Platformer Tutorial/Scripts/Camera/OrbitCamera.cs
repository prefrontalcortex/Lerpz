using UnityEngine;
using System.Collections;

[System.Serializable]
// Make the rigid body not change rotation
[UnityEngine.AddComponentMenu("Third Person Camera/Mouse Orbit")]
public partial class OrbitCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 targetOffset;
    public float distance;
    public LayerMask lineOfSightMask;
    public float closerRadius;
    public float closerSnapLag;
    public float xSpeed;
    public float ySpeed;
    public int yMinLimit;
    public int yMaxLimit;
    private float currentDistance;
    private float x;
    private float y;
    private float distanceVelocity;
    public virtual void Start()
    {
        Vector3 angles = this.transform.eulerAngles;
        this.x = angles.y;
        this.y = angles.x;
        this.currentDistance = this.distance;
        if (this.GetComponent<Rigidbody>())
        {
            this.GetComponent<Rigidbody>().freezeRotation = true;
        }
    }

    public virtual void LateUpdate()
    {
        if (this.target)
        {
            this.x = this.x + ((Input.GetAxis("Mouse X") * this.xSpeed) * 0.02f);
            this.y = this.y - ((Input.GetAxis("Mouse Y") * this.ySpeed) * 0.02f);
            this.y = OrbitCamera.ClampAngle(this.y, this.yMinLimit, this.yMaxLimit);
            Quaternion rotation = Quaternion.Euler(this.y, this.x, 0);
            Vector3 targetPos = this.target.position + this.targetOffset;
            Vector3 direction = rotation * -Vector3.forward;
            float targetDistance = this.AdjustLineOfSight(targetPos, direction);
            this.currentDistance = Mathf.SmoothDamp(this.currentDistance, targetDistance, ref this.distanceVelocity, this.closerSnapLag * 0.3f);
            this.transform.rotation = rotation;
            this.transform.position = targetPos + (direction * this.currentDistance);
        }
    }

    public virtual float AdjustLineOfSight(Vector3 target, Vector3 direction)
    {
        RaycastHit hit = default(RaycastHit);
        if (Physics.Raycast(target, direction, out hit, this.distance, this.lineOfSightMask.value))
        {
            return hit.distance - this.closerRadius;
        }
        else
        {
            return this.distance;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle = angle + 360;
        }
        if (angle > 360)
        {
            angle = angle - 360;
        }
        return Mathf.Clamp(angle, min, max);
    }

    public OrbitCamera()
    {
        this.targetOffset = Vector3.zero;
        this.distance = 4f;
        this.closerRadius = 0.2f;
        this.closerSnapLag = 0.2f;
        this.xSpeed = 200f;
        this.ySpeed = 80f;
        this.yMinLimit = -20;
        this.yMaxLimit = 80;
        this.currentDistance = 10f;
    }

}