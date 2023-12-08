using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 700.0f;

    public GameObject snowballPrefab;
    public Transform throwPoint;
    public float throwForce = 100f;

    private CharacterController _characterController;
    private Rigidbody _rigidbody;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movementDirection = new Vector3(horizontal, 0, vertical);
        movementDirection = transform.TransformDirection(movementDirection);
        movementDirection *= speed;

        _characterController.Move(movementDirection * Time.deltaTime);

        // Rotation
        if (movementDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        // Interaction
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Interact();
        }
    }

    void Interact()
    {
        ThrowSnowball();
    }

    void ThrowSnowball()
    {
        if (snowballPrefab != null && throwPoint != null)
        {
            // Instantiate the snowball at the throw point
            GameObject snowball = Instantiate(snowballPrefab, throwPoint.position, throwPoint.rotation);

            // Apply a force to the snowball
            Rigidbody rb = snowball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(throwPoint.forward * throwForce, ForceMode.Impulse);
            }

            Destroy(snowball, 5);
        }
        else
        {
            Debug.LogError("Snowball prefab or throw point not assigned!");
        }
    }
}

