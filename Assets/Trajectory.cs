using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
    // launch variables
    [SerializeField] private Transform TargetObject;
    [Range(1.0f, 6.0f)] public float TargetRadius;
    [Range(20.0f, 70.0f)] public float LaunchAngle;

    // state
    private bool bTargetReady;
    private bool bTouchingGround;

    // cache
    private Rigidbody rigid;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    
    //-----------------------------------------------------------------------------------------------

    // Use this for initialization
    void Start()
    {   
        bTargetReady = false;
        rigid = GetComponent<Rigidbody>();
        bTouchingGround = true;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
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
            // update the rotation of the projectile during trajectory motion
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

    // launches the object towards the TargetObject with a given LaunchAngle
    void Launch()
    {
        // rotate the object to face the target
        transform.LookAt(TargetObject.position);

        // shorthands for the formula
        float R = Vector3.Distance(transform.position, TargetObject.position);
        float G = Physics.gravity.y;
        float alpha = LaunchAngle * Mathf.Deg2Rad;  // in radians

        // calculate initial speed required to land the projectile on target object using the formula (9)
        float V0 = Mathf.Sqrt(-R * G / Mathf.Sin(2 * alpha));    // initial speed
        float Vy = V0 * Mathf.Sin(alpha); // velocity component in upward  direction of local space
        float Vz = V0 * Mathf.Cos(alpha); // velocity component in forward direction of local space

        // create the velocity vector in local space and get it in global space
        Vector3 localVelocity = new Vector3(0f, Vy, Vz);
        Vector3 globalVelocity = transform.TransformDirection(localVelocity);

        // launch the object by setting its initial velocity and flipping its state
        rigid.velocity = globalVelocity;
        bTargetReady = false;
    }

    // Sets a random target around the object based on the TargetRadius
    void SetNewTarget()
    {
        Transform targetTF = TargetObject.GetComponent<Transform>(); // shorthand
        
        // To acquire our new target from a point around the projectile object:
        // - we start with a vector in the XZ-Plane (ground), let's pick right (1, 0, 0).
        //   (or pick left, forward, back, or any perpendicular vector to the rotation axis, which is up)
        // - We'll use a quaternion to rotate our vector. To create a rotation quaternion, we'll be using
        //   the AngleAxis() function, which takes a rotation angle and a rotation amount in degrees as parameters.
        Vector3 rotationAxis = Vector3.up;  // as our object is on the XZ-Plane, we'll use up vector as the rotation axis.
        float randomAngle = Random.Range(0.0f, 360.0f);
        Vector3 randomVector = Quaternion.AngleAxis(randomAngle, rotationAxis) * Vector3.right;

        // - scale the randomVector with the target distance
        // - we also add an offset which makes the starting position at the same height level as the target
        Vector3 randomPoint = randomVector * TargetRadius + new Vector3(0, targetTF.position.y, 0);

        //  - finally, we'll set the target object's position and update our state. 
        TargetObject.SetPositionAndRotation(randomPoint, targetTF.rotation);
        bTargetReady = true;
    }
}
