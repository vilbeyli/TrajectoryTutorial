using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // launch variables
    [SerializeField] private Transform TargetObjectTF;
    [Range(1.0f, 15.0f)] public float TargetRadius;
    [Range(20.0f, 75.0f)] public float LaunchAngle;
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

    // resets the projectile to its initial position
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

    // returns the distance between the red dot and the TargetObject's y-position
    // this is a very little offset considered the ranges in this demo so it shouldn't make a big difference.
    // however, if this code is tested on smaller values, the lack of this offset might introduce errors.
    // to be technically accurate, consider using this offset together with the target platform's y-position.
    float GetPlatformOffset()
    {
        float platformOffset = 0.0f;
        // 
        //          (SIDE VIEW OF THE PLATFORM)
        //
        //                   +------------------------- Mark (Sprite)
        //                   v
        //                  ___                                          -+-
        //    +-------------   ------------+         <- Platform (Cube)   |  platformOffset
        // ---|--------------X-------------|-----    <- TargetObject     -+-
        //    +----------------------------+
        //

        // we're iterating through Mark (Sprite) and Platform (Cube) Transforms. 
        foreach (Transform childTransform in TargetObjectTF.GetComponentsInChildren<Transform>())
        {
            // take into account the y-offset of the Mark gameobject, which essentially
            // is (y-offset + y-scale/2) of the Platform as we've set earlier through the editor.
            if (childTransform.name == "Mark")
            {
                platformOffset = childTransform.localPosition.y;
                break;
            }
        }
        return platformOffset;
    }

    // launches the object towards the TargetObject with a given LaunchAngle
    void Launch()
    {
        // think of it as top-down view of vectors: 
        //   we don't care about the y-component(height) of the initial and target position.
        Vector3 projectileXZPos = new Vector3(transform.position.x, 0.0f, transform.position.z);
        Vector3 targetXZPos = new Vector3(TargetObjectTF.position.x, 0.0f, TargetObjectTF.position.z);
        
        // rotate the object to face the target
        transform.LookAt(targetXZPos);

        // shorthands for the formula
        float R = Vector3.Distance(projectileXZPos, targetXZPos);
        float G = Physics.gravity.y;
        float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);
        float H = (TargetObjectTF.position.y + GetPlatformOffset()) - transform.position.y;

        // calculate the local space components of the velocity 
        // required to land the projectile on the target object 
        float Vz = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)) );
        float Vy = tanAlpha * Vz;

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
        Transform targetTF = TargetObjectTF.GetComponent<Transform>(); // shorthand
        
        // To acquire our new target from a point around the projectile object:
        // - we start with a vector in the XZ-Plane (ground), let's pick right (1, 0, 0).
        //    (or pick left, forward, back, or any perpendicular vector to the rotation axis, which is up)
        // - We'll use a quaternion to rotate our vector. To create a rotation quaternion, we'll be using
        //    the AngleAxis() function, which takes a rotation angle and a rotation amount in degrees as parameters.
        Vector3 rotationAxis = Vector3.up;  // as our object is on the XZ-Plane, we'll use up vector as the rotation axis.
        float randomAngle = Random.Range(0.0f, 360.0f);
        Vector3 randomVectorOnGroundPlane = Quaternion.AngleAxis(randomAngle, rotationAxis) * Vector3.right;

        // Add a random offset to the height of the target location:
        // - If the RandomizeHeightOffset flag is turned on, pick a random number between 0.2f and 1.0f to make sure
        //    we're somewhat above or below the ground. If the flag is off, just pick 1.0f. Finally, scale this number
        //    with the TargetHeightOffsetFromGround.
        // - We want to randomly determine if the target is above or below ground. 
        //    Randomly assign the multiplier -1.0f or 1.0f
        // - Create an offset vector from the random height and add the offset vector to the random point on the plane
        float heightOffset = (RandomizeHeightOffset ? Random.Range(0.2f, 1.0f) : 1.0f) * TargetHeightOffsetFromGround;
        float aboveOrBelowGround = (Random.Range(0.0f, 1.0f) > 0.5f ? 1.0f : -1.0f);
        Vector3 heightOffsetVector = new Vector3(0, heightOffset, 0) * aboveOrBelowGround;
        Vector3 randomPoint = randomVectorOnGroundPlane * TargetRadius + heightOffsetVector;

        //  - finally, we'll set the target object's position and update our state. 
        TargetObjectTF.SetPositionAndRotation(randomPoint, targetTF.rotation);
        bTargetReady = true;
    }
}
