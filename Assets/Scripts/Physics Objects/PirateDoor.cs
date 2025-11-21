using UnityEngine;

public class PirateDoor : MonoBehaviour
{
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
        if (col.gameObject.tag == "Soul" && !col.isTrigger)
        {
            currentNumber++;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Soul" && !col.isTrigger)
        {
            currentNumber--;
        }
    }
}
