using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vortex : MonoBehaviour
{
    public bool loopDisrupt;

    [Space(10)]
    private float currCircleAngle = 0f; // Current angle of rotation
    public float circleSpeed = 100f; // Speed of rotation
    public float circleRadius = 100f; // Radius of circle

    [Space(10)]
    public float disruptDelay = 2;
    public float disruptDuration = 5;
    public float disruptSpeed = 50;
    public float disruptRadius = 50;

    [Space(10)]
    public int maxObjects = 25;
    public float objectScale;
    public List<GameObject> prefabs;
    private List<GameObject> objects = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DisruptRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        FillObjectList();
        CircleTransform();
    }

    public void FillObjectList()
    {
        if (objects.Count < maxObjects)
        {
            GameObject newObject = Instantiate(prefabs[Random.Range(0, prefabs.Count)], transform);
            objects.Add(newObject);
        }
    }


    public void CircleTransform()
    {
        currCircleAngle += circleSpeed * Time.deltaTime; // Update angle of rotation

        Vector3 targetPos = transform.position;
        targetPos.z = 0f; // Ensure target position is on the same plane as objects to circle

        for (int i = 0; i < objects.Count; i++)
        {
            float angleRadians = (currCircleAngle + (360f / objects.Count) * i) * Mathf.Deg2Rad; // Calculate angle in radians for each object
            Vector3 newPos = targetPos + new Vector3(Mathf.Cos(angleRadians) * circleRadius, Mathf.Sin(angleRadians) * circleRadius, 0f); // Calculate new position for object
            objects[i].transform.position = Vector3.Lerp(objects[i].transform.position, newPos, Time.deltaTime); // Move object towards new position using Lerp
        }
    }

    IEnumerator DisruptRoutine()
    {
        yield return new WaitForSeconds(disruptDelay);

        float originalSpeed = circleSpeed;
        float originalRadius = circleRadius;

        circleSpeed = disruptSpeed;
        circleRadius = disruptRadius;


        if (loopDisrupt)
        {
            yield return new WaitForSeconds(disruptDuration);

            circleSpeed = originalSpeed;
            circleRadius = originalRadius;


            StartCoroutine(DisruptRoutine());
        }
    }

    public void Disrupt()
    {
        StartCoroutine(DisruptRoutine());
    }

}
