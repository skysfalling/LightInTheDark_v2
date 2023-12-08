using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    PlayerController _playerController;

    public Transform target; // Assign the player or target object in the inspector
    public GameObject snowballPrefab; // Assign this in the inspector
    public float throwForce = 10f;
    public float lerpSpeed = 0.05f;
    public float throwRange = 10f; // Range within which the AI will start throwing snowballs
    public Transform throwPoint;

    private bool IsInThrowRange => Vector3.Distance(transform.position, target.position) <= throwRange;
    private bool IsInFollowRange => Vector3.Distance(transform.position, target.position) > throwRange;

    private void Start()
    {
        _playerController = FindObjectOfType<PlayerController>();
        target.position = _playerController.transform.position;

        InvokeRepeating("ThrowSnowball", 0, 2);
    }

    void Update()
    {

        if (IsInThrowRange)
        {
            RotateTowardsTarget();
        }
        else if (IsInFollowRange) {
            MoveTowardsTarget();
        }
    }

    void MoveTowardsTarget()
    {
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
    }

    void RotateTowardsTarget()
    {
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0; // Keep the rotation only on the Y-axis
        Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationToTarget, lerpSpeed * Time.deltaTime);
    }

    void ThrowSnowball()
    {
        if (!IsInThrowRange) { return; }

        // Instantiate the snowball
        GameObject snowball = Instantiate(snowballPrefab, throwPoint.position, transform.rotation);

        // Apply a force to the snowball
        Rigidbody rb = snowball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }
    }
}
