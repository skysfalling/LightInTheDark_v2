using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindManager : MonoBehaviour
{
    public enum WindState { START, END, NONE }
    public WindState windState;

    public List<Rigidbody> rigidbodies;  // The list of Rigidbodies to apply the wind force to
    public float maxWindForce = 10.0f;  // The maximum wind force to apply
    public float windDuration = 10.0f;  // The duration of the wind force
    public Vector3 windDirection = Vector3.right;  // The direction of the wind

    public float currentWindForce = 0.0f;
    public float currentTime = 0;

    public void Start()
    {
        StartCoroutine(WindCycle(1, windDuration));
    }

    public IEnumerator WindCycle(float delay, float duration)
    {
        // init
        windState = WindState.NONE;
        currentWindForce = 0;
        currentTime = 0;

        // wait for delay
        yield return new WaitForSeconds(delay);

        windState = WindState.START;

        // wait for delay
        yield return new WaitForSeconds(duration * 0.5f);

        windState = WindState.END;

        // wait for delay
        yield return new WaitForSeconds(duration * 0.5f);


        StartCoroutine(WindCycle(1, windDuration));
    }




    private void FixedUpdate()
    {
        if (windState == WindState.START)
        {
            currentWindForce = Mathf.Lerp(0, maxWindForce, currentTime / (windDuration * 0.5f));
            currentTime += Time.deltaTime;
        }
        else if (windState == WindState.END)
        {
            currentWindForce = Mathf.Lerp(maxWindForce, 0, currentTime / windDuration);
            currentTime += Time.deltaTime;
        }


        // Apply the wind force to each Rigidbody in the list
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.AddForce(windDirection.normalized * currentWindForce, ForceMode.Acceleration);
        }

    }
}

