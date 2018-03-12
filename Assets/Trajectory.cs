using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
    // launch variables
    [SerializeField] private Transform TargetObject;
    [Range(1.0f, 15.0f)] public float TargetRadius;
    [Range(20.0f, 70.0f)] public float LaunchAngle;
    [Range(0.0f, 10.0f)] public float TargetHeightOffsetFromGround;
    public bool RandomizeHeightOffset;

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
        rigid = GetComponent<Rigidbody>();
        bTargetReady = false;
        bTouchingGround = true;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    void ResetToInitialState()
    {
        rigid.velocity = Vector3.zero;
        this.transform.SetPositionAndRotation(initialPosition, initialRotation);
        bTouchingGround = true;
        bTargetReady = false;
    }

	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (bTargetReady)
            {
                Launch();
            }
            else
            {
                ResetToInitialState();
                SetNewTarget();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetToInitialState();
        }

        if (!bTouchingGround && !bTargetReady)
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
        Vector3 projectileXZPos = new Vector3(transform.position.x, 0.0f, transform.position.z);
        Vector3 targetXZPos = new Vector3(TargetObject.position.x, 0.0f, TargetObject.position.z);

        // rotate the object to face the target
        transform.LookAt(targetXZPos);

        // shorthands for the formula        
        float R = Vector3.Distance(projectileXZPos, targetXZPos);
        float G = Physics.gravity.y;
        float alpha = LaunchAngle * Mathf.Deg2Rad;  // in radians
        float H = (transform.position.y - (TargetObject.position.y + 0.31f));
        float phi = Mathf.Atan(R / H);  // in radians
        float R_sqr = R * R;

        float Denominator = H + Mathf.Cos(2.0f * alpha - phi) * Mathf.Sqrt(H*H + R_sqr);

        // calculate initial speed required to land the projectile on target object using the formula (9)
        //float V0 = Mathf.Sqrt(-R * G / Mathf.Sin(2 * alpha));    // initial speed
        float V0 = Mathf.Sqrt( -(R_sqr * G) / Denominator);    // initial speed
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
        Vector3 randomVectorOnGroundPlane = Quaternion.AngleAxis(randomAngle, rotationAxis) * Vector3.right;

        // - scale the randomVector with the target distance
        // - we also add a random height offset 
        float heightOffset = RandomizeHeightOffset ? Random.Range(TargetHeightOffsetFromGround/5.0f, TargetHeightOffsetFromGround) : TargetHeightOffsetFromGround;
        Vector3 heightOffsetVector = new Vector3(0, -heightOffset, 0);
        Vector3 randomPoint = randomVectorOnGroundPlane * TargetRadius + heightOffsetVector;

        //  - finally, we'll set the target object's position and update our state. 
        TargetObject.SetPositionAndRotation(randomPoint, targetTF.rotation);
        bTargetReady = true;
    }
}
