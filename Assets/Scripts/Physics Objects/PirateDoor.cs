using UnityEngine;

public class PirateDoor : MonoBehaviour
{
    [SerializeField] Transform soul;
    HingeJoint joint;
    JointMotor motor;
    [SerializeField] int currentNumber = 0;
    int numberToWin = 1;
    void Start()
    {
        joint = GetComponent<HingeJoint>();
        motor = joint.motor;
    }

    void Update()
    {
        if (soul != null)
        {
            if (Vector3.Distance(soul.position, transform.position) > 20.0f)
            {
                soul.gameObject.GetComponent<FollowPlayer>().tagged = false;
                soul = null;
                currentNumber--;
            }
        }

        if (currentNumber == numberToWin)
        {
            if (motor.targetVelocity > 0f)
            {
                motor.targetVelocity *= -1;
                joint.motor = motor;
            }

            if (!joint.useMotor && joint.angle > joint.limits.min) joint.useMotor = true;
            else if (joint.useMotor && joint.angle <= joint.limits.min) joint.useMotor = false;
        }
        else
        {
            if (motor.targetVelocity < 0f)
            {
                motor.targetVelocity *= -1;
                joint.motor = motor;
            }

            if (!joint.useMotor && joint.angle < joint.limits.max) joint.useMotor = true;
            else if (joint.useMotor && joint.angle >= joint.limits.max) joint.useMotor = false;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Soul" && !col.isTrigger && !col.GetComponent<FollowPlayer>().tagged)
        {
            col.GetComponent<FollowPlayer>().tagged = true;
            currentNumber++;
            soul = col.transform;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Soul" && !col.isTrigger)
        {
            //currentNumber--;
        }
    }
}
