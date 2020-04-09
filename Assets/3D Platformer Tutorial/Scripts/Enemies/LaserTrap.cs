using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.RequireComponent(typeof(LineRenderer))]
public partial class LaserTrap : MonoBehaviour
{
    public float height;
    public float speed;
    public float timingOffset;
    public float laserWidth;
    public int damage;
    public GameObject hitEffect;
    private Vector3 originalPosition;
    private RaycastHit hit;
    private float lastHitTime;
    public virtual void Start()
    {
        this.originalPosition = this.transform.position;
        ((LineRenderer) this.GetComponent(typeof(LineRenderer))).SetPosition(1, Vector3.forward * this.laserWidth);
    }

    public virtual void Update()
    {
        float offset = ((1 + Mathf.Sin((Time.time * this.speed) + this.timingOffset)) * this.height) / 2;
        this.transform.position = this.originalPosition + new Vector3(0, offset, 0);
        if ((Time.time > (this.lastHitTime + 0.25f)) && Physics.Raycast(this.transform.position, this.transform.forward, out this.hit, this.laserWidth))
        {
            if ((this.hit.collider.tag == "Player") || (this.hit.collider.tag == "Enemy"))
            {
                UnityEngine.Object.Instantiate(this.hitEffect, this.hit.point, Quaternion.identity);
                this.hit.collider.SendMessage("ApplyDamage", this.damage, SendMessageOptions.DontRequireReceiver);
                this.lastHitTime = Time.time;
            }
        }
    }

    public LaserTrap()
    {
        this.height = 3.2f;
        this.speed = 2f;
        this.laserWidth = 12f;
        this.damage = 1;
    }

}