using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    public enum STATE { NULL, FOLLOW, THROW, HIT }
    public STATE currState;

    PlayerController _playerController;
    Rigidbody _rigidbody;

    public Transform target; // Assign the player or target object in the inspector
    public GameObject snowballPrefab; // Assign this in the inspector
    public float throwForce = 10f;
    public float moveSpeed = 0.05f;
    public float rotation_lerpSpeed = 1f;
    public float maxVelocity = 100f;

    public float throwRange = 10f; // Range within which the AI will start throwing snowballs
    public Transform throwPoint;

    private bool IsInThrowRange => Vector3.Distance(transform.position, target.position) <= throwRange;
    private bool IsInFollowRange => Vector3.Distance(transform.position, target.position) > throwRange;

    [Header("Animations")]
    public Image image;
    public Sprite defaultImage;
    public Sprite throwImage;
    public Sprite hitImage;


    private void Start()
    {
        _playerController = FindObjectOfType<PlayerController>();
        _rigidbody = GetComponent<Rigidbody>();

        target.position = _playerController.transform.position;

        InvokeRepeating("ThrowSnowball", 0, 2);
    }

    void SlowUpdate()
    {

    }

    void Update()
    {
        RotateTowardsTarget();

        if (IsInFollowRange && currState != STATE.HIT) {

            SetState(STATE.FOLLOW);

            MoveTowardsTarget();
        }

    }

    public void SetState(STATE state)
    {
        if (currState != state)
        {
            currState = state;

            switch(state)
            {
                case STATE.FOLLOW:
                    image.sprite = defaultImage;
                    break;
                case STATE.THROW:
                    image.sprite = throwImage; 
                    break;
                case STATE.HIT:
                    image.sprite = hitImage; 
                    break;
                default:
                    image.sprite = defaultImage;
                    break;
            }

            Invoke("ResetImage", 1);
        }
    }

    public void ResetImage()
    {
        image.sprite = defaultImage;
    }

    // ========================== INTERACTIONS =====================
    void MoveTowardsTarget()
    {
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        Vector3 direction = (targetPosition - _rigidbody.position).normalized;
        Vector3 desiredVelocity = direction * moveSpeed; // 'speed' should be defined in your class

        // Interpolate velocity
        _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, desiredVelocity, 1 * Time.fixedDeltaTime);
    }

    void RotateTowardsTarget()
    {
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0; // Keep the rotation only on the Y-axis
        Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationToTarget, rotation_lerpSpeed * Time.deltaTime);
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

    void ThrowSnowball()
    {
        if (!IsInThrowRange) { return; }
        SetState(STATE.THROW);

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
        image.sprite = hitImage;

        _playerController.ConfirmHit();

        Destroy(gameObject, 1);
    }
}
