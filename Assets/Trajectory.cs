using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
    [SerializeField] private Transform TargetObject;

    [Range(1.0f, 6.0f)] public float TargetDistance;
    [Range(20.0f, 70.0f)] public float LaunchAngle;

    private bool bTargetReady;
    private Rigidbody rigid;
    private bool bTouchingGround;

    Vector3 initialPosition;
    Quaternion initialRotation;

    // Use this for initialization
    void Start()
    {   
        bTargetReady = true;
        rigid = GetComponent<Rigidbody>();
        bTouchingGround = true;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    // launches the object towards the TargetObject with a given LaunchAngle
    void Launch()
    {
        // shorthands for the formula
        float R = Vector3.Distance(transform.position, TargetObject.position);
        float G = -Physics.gravity.y;
        float alpha = Mathf.Deg2Rad * LaunchAngle;  // in radians

        // rotate the object to face the target
        transform.LookAt(TargetObject.position);

        // calculate initial speed required to land the cube on target using the formula (9)
        float Vi = Mathf.Sqrt(R * G / Mathf.Sin(2 * alpha));    // initial speed
        float Vy = Vi * Mathf.Sin(alpha); // velocity component in upward  direction of local space
        float Vz = Vi * Mathf.Cos(alpha); // velocity component in forward direction of local space

        // create the velocity vector in local space and get it in global space
        Vector3 localVelocity = new Vector3(0f, Vy, Vz);
        Vector3 globalVelocity = transform.TransformDirection(localVelocity);

        // launch the object by setting its initial velocity and flipping its state
        rigid.velocity = globalVelocity;
        bTargetReady = false;
    }

    // Sets a random target around the object based on the TargetDistance
    void SetNewTarget()
    {
        Transform targetTF = TargetObject.GetComponent<Transform>(); // shorthand

        // Think of randomly acquiring a point TargetDistance away from us as the following:
        // - we start with a vector in the XZ-Plane (ground), let's pick right and scale it w/ TargetDistance.
        //   (or pick left, forward, back, or any perpendicular vector to the rotation axis, which is up)
        // - we also add an offset which makes the starting position at the same height level as the target
        Vector3 startingPoint = Vector3.right * TargetDistance + new Vector3(0, targetTF.position.y, 0);

        //  - we get a random angle to rotate this vector around a circle
        float randomAngle = Random.Range(0.0f, 360.0f);

        //  - create our rotation quaternion, which will transform our starting point vector [unclear]
        Vector3 rotationAxis = Vector3.up;  // as our object is on XZ-Plane, we'll use up vector as the rotation axis.

        //  - we'll rotate the starting position vector with the rotation quaternion to get the final 
        //    random position around the circle of this object.
        Vector3 randomPoint = Quaternion.AngleAxis(randomAngle, rotationAxis) * startingPoint;

        //  - finally, we'll set the target object's position. 
        TargetObject.SetPositionAndRotation(randomPoint, targetTF.rotation);
        
        bTargetReady = true;
    }

	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (bTargetReady)   Launch();
            else                SetNewTarget();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            this.transform.SetPositionAndRotation(initialPosition, initialRotation);
            rigid.velocity = Vector3.zero;
        }

        if (!bTouchingGround)
        {
            transform.rotation = Quaternion.LookRotation(rigid.velocity) * initialRotation;
        }
	}

    void OnCollisionEnter()
    {
        bTouchingGround = true;
    }

    void OnCollisionExit()
    {
        bTouchingGround = false;
    }
}
