using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// USED FOR "STATIC" TENTACLES THAT GO TO THE PREVIOUS POSITION OF THE HEAD
// SNAKE LIKE MOVEMENT

public class TentacleTwo : MonoBehaviour
{
    public int length;
    public LineRenderer lineRend; 
    public Vector3[] segmentPoses; 
    private Vector3[] segmentV;

    public Transform targetDir; 
    public float targetDist;
    public float smoothSpeed;

    [Space(10)]
    public float wiggleSpeed;
    public float wiggleMagnitude;
    public Transform wiggleDir;

    [Space(10)]
    public Transform tailObject;

    private void Start()
    {
        lineRend.positionCount = length;
        segmentPoses = new Vector3[length];
        segmentV = new Vector3[length];

        ResetPos();
    }


    private void Update()
    {
        // wiggle taile
        wiggleDir.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * wiggleSpeed) * wiggleMagnitude);


        // move taile to follow
        segmentPoses[0] = targetDir.position;

        for (int i = 1; i < segmentPoses.Length; i++)
        {
            Vector3 targetPos = segmentPoses[i - 1] + (segmentPoses[i] - segmentPoses[i - 1]).normalized * targetDist;
            segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], targetPos, ref segmentV[i], smoothSpeed);
        }

        lineRend.SetPositions(segmentPoses);



        // << SET TAIL OBJECT >>
        if (tailObject)
        {
            tailObject.position = segmentPoses[segmentPoses.Length - 1];

        }
    }

    private void ResetPos()
    {
        segmentPoses[0] = targetDir.position;
        for (int i = 1; i < length; i++)
        {
            segmentPoses[i] = segmentPoses[i - 1] + targetDir.right * targetDist;
        }
        lineRend.SetPositions(segmentPoses);
    }
}
