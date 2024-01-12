using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 700.0f;
    public float maxVelocity = 100f;

    public GameObject snowballPrefab;
    public Transform throwPoint;
    public float throwForce = 100f;

    private Rigidbody _rigidbody;

    private bool _fireInput;
    private float _autoFireRate = 0.25f;

    public int hitCount = 0;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        InvokeRepeating("AutoFireRoutine", 0, _autoFireRate);
    }

    void Update()
    {
        // Interaction
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Interact();
        }

        if (Input.GetKey(KeyCode.Space) )
        {
            _fireInput = true;
        }
        else { _fireInput = false; }
    }

    void FixedUpdate()
    {
        // Movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movementDirection = new Vector3(horizontal, 0, vertical);
        movementDirection = transform.TransformDirection(movementDirection);
        movementDirection *= speed;

        if (Mathf.Abs(vertical) > 0.1f)
        {
            MovePlayer(movementDirection);
        }
        
        if (Mathf.Abs(horizontal) > 0.01f) // horz input deadzone
        {
            if (_fireInput)
            {
                RotatePlayer(movementDirection, rotationSpeed * 0.25f);
            }
            else
            {
                RotatePlayer(movementDirection, rotationSpeed);
            }
        }

        ClampVelocity();
    }

    void MovePlayer(Vector3 movementDirection)
    {
        // Apply a force to move the player
        _rigidbody.AddForce(movementDirection.normalized * speed, ForceMode.Acceleration);
    }

    void RotatePlayer(Vector3 movementDirection, float speed)
    {
        if (movementDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg;
            Quaternion toRotation = Quaternion.Euler(0, angle, 0);
            _rigidbody.rotation = Quaternion.RotateTowards(_rigidbody.rotation, toRotation, speed * Time.fixedDeltaTime);
        }
    }

    void ClampVelocity()
    {
        Vector3 velocity = _rigidbody.velocity;

        // Clamp the velocity if it exceeds the maximum allowed velocity
        if (velocity.magnitude > maxVelocity)
        {
            velocity = velocity.normalized * maxVelocity;
            _rigidbody.velocity = velocity;
        }
    }

    void Interact()
    {
        Debug.Log("Interact");
    }

    void AutoFireRoutine()
    {
        if (_fireInput)
        {
            ThrowSnowball();
        }
    }

    void ThrowSnowball()
    {
        if (snowballPrefab != null && throwPoint != null)
        {
            // Instantiate the snowball at the throw point
            GameObject snowball = Instantiate(snowballPrefab, throwPoint.position, throwPoint.rotation);
            snowball.GetComponent<ThrowableObject>().parentEntity = this.gameObject;

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

    public void Hit()
    {
        //Debug.Log("Hit Player");
    }

    public void ConfirmHit()
    {
        hitCount++;
    }
}

