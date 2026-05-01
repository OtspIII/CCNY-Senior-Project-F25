using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Buoyant Objects Require a 'RigidBody' Component Attached, Otherwise It Will Not Work:
[RequireComponent(typeof(Rigidbody))]
public class BuoyantObject : MonoBehaviour
{
    [Header("Float Points:")]
    public Transform[] floater;
    [Space]

    [Header("Under Water Settings:")]
    public float underWaterDrag = 3.0f;
    public float underWaterAngularDrag = 1.0f;
    [Space]

    [Header("Above Water Settings:")]
    public float airDrag = 0.0f;
    public float airAngularDrag = 0.05f;
    [Space]

    [Header("Buoyancy Force:")]
    public float floatingPower = 135.0f;
    [Space]


    [Header("Object Info:")]
    Rigidbody m_RigidBody;
    bool isUnderWater;
    int floatersUnderWater;


    [Header("External Info:")]
    OceanWaves oceanWaves;
    private float waterHeight;



    void Start()
    {
        //Gets Refrence To The RigidBody Component Attached:
        m_RigidBody = GetComponent<Rigidbody>();

        //Gets Refrence To An Object With the 'OceanWaves' Script Component Attached:
        oceanWaves = FindAnyObjectByType<OceanWaves>();
    }

    void FixedUpdate()
    {
        floatersUnderWater = 0;

        for (int i = 0; i < floater.Length; i++)
        {
            //Given the Vector3 Position The Floater is Located -> Uses The Y Position of the OceanWave on the Same Vector3 Position:
            waterHeight = oceanWaves.transform.position.y + oceanWaves.GetWaveHeightAtPosition(floater[i].position);

            float displacement = floater[i].position.y - waterHeight;

            //Below The Water:
            if (displacement < 0)
            {
                //Adding Force: [Upwards at a Constant Force] * [Displacement -> Further From Equilibirum == More Force]
                m_RigidBody.AddForceAtPosition(Vector3.up * floatingPower * Mathf.Abs(displacement), floater[i].position);

                floatersUnderWater += 1;

                //If The Bool is Not Yet Declared To Be UnderWater:
                if (!isUnderWater)
                {
                    //Declare it To Be UnderWater:
                    isUnderWater = true;

                    //Adjust The RigidBody's [Drag] & [AngularDrag] To Match It's Environment:
                    SwitchState(true);
                }
            }
        }

        if (isUnderWater && floatersUnderWater == 0)
        {
            isUnderWater = false;
            SwitchState(false);
        }
    }


    void SwitchState(bool underEquilibrium)
    {
        if (underEquilibrium)
        {
            m_RigidBody.linearDamping = underWaterDrag;
            m_RigidBody.angularDamping = underWaterAngularDrag;
        }
        else
        {
            m_RigidBody.linearDamping = airDrag;
            m_RigidBody.angularDamping = airAngularDrag;
        }
    }
}
