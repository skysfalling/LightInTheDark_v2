using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    PlayerController _playerController;

    public Transform target; // Assign the player or target object in the inspector
    public GameObject snowballPrefab; // Assign this in the inspector
    public float throwForce = 10f;
    public float move_lerpSpeed = 0.05f;
    public float rotation_lerpSpeed = 1f;

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
        RotateTowardsTarget();

        if (IsInFollowRange) {
            MoveTowardsTarget();
        }
    }

    void MoveTowardsTarget()
    {
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, move_lerpSpeed * Time.deltaTime);
    }

    void RotateTowardsTarget()
    {
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0; // Keep the rotation only on the Y-axis
        Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationToTarget, rotation_lerpSpeed * Time.deltaTime);
    }

    void ThrowSnowball()
    {
        if (!IsInThrowRange) { return; }

        // Instantiate the snowball
        GameObject snowball = Instantiate(snowballPrefab, throwPoint.position, transform.rotation);
        snowball.GetComponent<ThrowableObject>().parentEntity = this.gameObject;

        // Apply a force to the snowball
        Rigidbody rb = snowball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }
    }

    public void Hit()
    {
        Debug.Log("Hit Enemy", this.gameObject);
        Destroy(gameObject);
    }
}
