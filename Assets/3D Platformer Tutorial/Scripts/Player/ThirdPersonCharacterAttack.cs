using UnityEngine;
using System.Collections;

[System.Serializable]
// Play sound.
[UnityEngine.RequireComponent(typeof(AudioSource))]
public partial class ThirdPersonCharacterAttack : MonoBehaviour
{
    public int punchSpeed;
    public float punchHitTime;
    public float punchTime;
    public Vector3 punchPosition;
    public float punchRadius;
    public int punchHitPoints;
    public AudioClip punchSound;
    private bool busy;
    private Animation anim;
    public virtual void Start()
    {
        this.anim = this.GetComponent<Animation>();
        this.anim["punch"].speed = this.punchSpeed;
    }

    public virtual void Update()
    {
        ThirdPersonController controller = (ThirdPersonController) this.GetComponent(typeof(ThirdPersonController));
        if (((!this.busy && Input.GetButtonDown("Fire1")) && controller.IsGroundedWithTimeout()) && !controller.IsMoving())
        {
            this.SendMessage("DidPunch");
            this.busy = true;
        }
    }

    public virtual IEnumerator DidPunch()
    {
        this.anim.CrossFadeQueued("punch", 0.1f, QueueMode.PlayNow);
        yield return new WaitForSeconds(this.punchHitTime);
        Vector3 pos = this.transform.TransformPoint(this.punchPosition);
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject go in enemies)
        {
            EnemyDamage enemy = (EnemyDamage) go.GetComponent(typeof(EnemyDamage));
            if (enemy == null)
            {
                continue;
            }
            if (Vector3.Distance(enemy.transform.position, pos) < this.punchRadius)
            {
                enemy.SendMessage("ApplyDamage", this.punchHitPoints);
                if (this.punchSound)
                {
                    this.GetComponent<AudioSource>().PlayOneShot(this.punchSound);
                }
            }
        }
        yield return new WaitForSeconds(this.punchTime - this.punchHitTime);
        this.busy = false;
    }

    public virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.TransformPoint(this.punchPosition), this.punchRadius);
    }

    public ThirdPersonCharacterAttack()
    {
        this.punchSpeed = 1;
        this.punchHitTime = 0.2f;
        this.punchTime = 0.4f;
        this.punchPosition = new Vector3(0, 0, 0.8f);
        this.punchRadius = 1.3f;
        this.punchHitPoints = 1;
    }

}