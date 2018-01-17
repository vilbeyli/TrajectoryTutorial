using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
    [SerializeField] private Transform TargetObject;

    [Range(1.0f, 6.0f)] public float TargetDistance;
    [Range(20.0f, 70.0f)] public float LaunchAngle;

    private bool bTargetReady;

    // Use this for initialization
    void Start()
    {   
        bTargetReady = true;
    }

    // launches the object towards the TargetObject with a given LaunchAngle
    void Launch()
    {
        bTargetReady = false;
    }

    // Acquires a random target around the object based on the TargetDistance
    void AcquireTarget()
    {
        Transform targetTF = TargetObject.GetComponent<Transform>(); // shorthand

        // Think of randomly acquiring a point TargetDistance away from us as the following:
        // - we start with a vector in the XZ-Plane (ground), let's pick right and scale it w/ TargetDistance.
        //   (or pick left, forward, back, or any perpendicular vector to the rotation axis)
        // - we also add an offset which makes the starting position at the same height level as the target
        Vector3 startingPoint = Vector3.right * TargetDistance + new Vector3(0, targetTF.position.y, 0);

        //  - we get a random angle to rotate this vector around a circle
        float randomAngleRadians = Random.Range(0.0f, 360.0f);

        //  - create our rotation quaternion, which will transform our starting point vector
        Vector3 rotationAxis = Vector3.up;  // as our object is on XZ-Plane, we'll use up vector as the rotation axis.
        Quaternion rotationQuat = Quaternion.AngleAxis(randomAngleRadians, rotationAxis);

        //  - we'll rotate the starting position vector with the rotation quaternion to get the final 
        //    random position around the circle of this object.
        Vector3 randomPoint = rotationQuat * startingPoint;

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
            else                AcquireTarget();
        }
	}
}
