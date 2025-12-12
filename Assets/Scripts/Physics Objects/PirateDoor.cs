using UnityEngine;

public class PirateDoor : MonoBehaviour
{
    [SerializeField] Transform soul;
    HingeJoint joint;
    JointMotor motor;
    [SerializeField] int currentNumber = 0;
    [SerializeField] int numberToWin = 1;
    [SerializeField] GameObject soulHead, soulBody;
    [SerializeField] Material radiantSoul, unlitSoul;
    void Start()
    {
        joint = GetComponent<HingeJoint>();
        motor = joint.motor;
        soulHead.GetComponent<Renderer>().material = unlitSoul;
        soulBody.GetComponent<Renderer>().material = unlitSoul;
    }

    void Update()
    {
        if (soul != null)
        {
            /*if (Vector3.Distance(soul.position, transform.position) > 20.0f)
            {
                soul.gameObject.GetComponent<FollowPlayer>().tagged = false;
                soul = null;
                soulHead.GetComponent<Renderer>().material = unlitSoul;
                soulBody.GetComponent<Renderer>().material = unlitSoul;
                currentNumber--;
            }*/
        }



        if (currentNumber == numberToWin)
        {
            if (soulHead.GetComponent<Renderer>().material != radiantSoul) soulHead.GetComponent<Renderer>().material = radiantSoul;
            if (soulBody.GetComponent<Renderer>().material != radiantSoul) soulBody.GetComponent<Renderer>().material = radiantSoul;


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
            if (soulHead.GetComponent<Renderer>().material != unlitSoul) soulHead.GetComponent<Renderer>().material = unlitSoul;
            if (soulBody.GetComponent<Renderer>().material != unlitSoul) soulBody.GetComponent<Renderer>().material = unlitSoul;

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
        if (col.gameObject.tag == "Soul" && !col.isTrigger && !col.GetComponent<FollowTarget>().tagged)
        {
            col.GetComponent<FollowTarget>().tagged = true;
            soulHead.GetComponent<Renderer>().material = radiantSoul;
            soulBody.GetComponent<Renderer>().material = radiantSoul;
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
