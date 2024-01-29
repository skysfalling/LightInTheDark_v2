using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public enum SoundState { MENU, ACTIVE_MUSIC }

public class SoundManager : MonoBehaviour
{
    private GameManager gameManager;
    public Transform playerTransform;

    [Space(10)]
    public SoundState state = SoundState.ACTIVE_MUSIC;

    // OVERALL VOLUME CONTROL


    [Header("Background Music Event")]
	public StudioEventEmitter backgroundEmitter;
	public EventReference backgroundMusicEvent;
    public EventInstance backgroundMusicInstance;

    [Header("Game State")]
    public int musicIntensity;      // the current music intensity
    public bool deathMarch;
    public bool lifeFlowerHealed;

    [Space(10)]
    public LayerMask enemyLayer;    // the layer where enemies are located
    public float outerDetectionRadius = 50;   // the radius of the sphere used to detect enemies
    public float innerDetectionRadius = 25;
    List<Collider2D> outerDetectionOverlap;
    List<Collider2D> innerDetectionOverlap;

    [Space(10)]
    public float curEntityIntensity = 0;
    public int maxEntityIntensity = 5;      // the maximum number of entities that can be in the sphere
    public float closestDistance;   // the closest distance to an enemy collider

    [Header("FMOD Parameters")]
    public float thatManProximity;
    FMOD.Studio.PARAMETER_DESCRIPTION thatManProximityParameter;
    public float leviathanProximity;
    public float lifeFlowerProximity;

    [Header("ONE SHOT FMOD EVENTS")]
	public string lightPickupSound = "event:/lightPickup";

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();


        backgroundMusicInstance = RuntimeManager.CreateInstance(backgroundMusicEvent);
        backgroundMusicInstance.start();

    }

    private void Update()
    {
        if (playerTransform)
        {
            // use an overlap sphere to check all colliders in the detection radius
            outerDetectionOverlap = new List<Collider2D>(Physics2D.OverlapCircleAll(playerTransform.position, outerDetectionRadius, enemyLayer));
            innerDetectionOverlap = new List<Collider2D>(Physics2D.OverlapCircleAll(playerTransform.position, innerDetectionRadius, enemyLayer));

            // remove all extra colliders from outer detection
            for (int i = outerDetectionOverlap.Count - 1; i >= 0; i--)
            {
                Collider2D col = outerDetectionOverlap[i];
                if (innerDetectionOverlap.Contains(col))
                {
                    outerDetectionOverlap.RemoveAt(i);
                }
            }


            // << MUSIC INTENSITY >>
            SetMusicIntensity();
            backgroundMusicInstance.setParameterByName("musicIntensity", musicIntensity);

            // << ENTITY PROXIMITY >>
            List<Collider2D> proximityOverlap = new List<Collider2D>(Physics2D.OverlapCircleAll(playerTransform.position, outerDetectionRadius, enemyLayer));
            if (proximityOverlap.Count > 0)
            {
                Collider2D closestMan = GetClosestColliderWithTag(proximityOverlap, "That Man");
                thatManProximity = GetProximityFloat(closestMan.transform);
                backgroundMusicInstance.setParameterByName("thatManProximity", thatManProximity);

            }
            else
            {
                backgroundMusicInstance.setParameterByName("thatManProximity", -1);

            }


            LogParameters(backgroundMusicInstance);
        }
        else
        {
            try
            {
                playerTransform = gameManager.levelManager.player.transform;
            }
            catch { Debug.LogWarning("SoundManager cannot find Player"); }
        }



    }

    public void SetMusicIntensity()
    {
        // calculate entity count by detection weight
        float outerEntityCount = (float)outerDetectionOverlap.Count * 0.5f;
        float innerEntityCount = (float)innerDetectionOverlap.Count * 1;
        float fullEntityCount = outerEntityCount + innerEntityCount;

        fullEntityCount += ( CountCollidersWithTag(outerDetectionOverlap, "That Man") * 0.5f );
        fullEntityCount += CountCollidersWithTag(innerDetectionOverlap, "That Man");

        fullEntityCount += ( CountCollidersWithTag(outerDetectionOverlap, "Leviathan") * 0.5f);
        fullEntityCount += CountCollidersWithTag(innerDetectionOverlap, "Leviathan");

        // determine percentage
        curEntityIntensity =  fullEntityCount / maxEntityIntensity;


        if (curEntityIntensity < 0.25f)
        {
            musicIntensity = 0;
        }
        else if (curEntityIntensity < 0.5f)
        {
            musicIntensity = 1;
        }
        else if (curEntityIntensity < 0.75f)
        {
            musicIntensity = 2;
        }
        else
        {
            musicIntensity = 3;
        }
    }



	// Play a single clip through the sound effects source.
	public void Play(string path)
	{
		FMODUnity.RuntimeManager.PlayOneShot(path);
	}

    public float GetProximityFloat(Transform target)
    {
        float proximity = target.transform.position.x - playerTransform.position.x;
        proximity /= outerDetectionRadius;

        return proximity;
    }

    public Collider2D GetClosestColliderWithTag(List<Collider2D> colliders, string tag)
    {
        float closestDistance = Mathf.Infinity;
        Collider2D closestCollider = null;

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag(tag))
            {
                float distance = Vector2.Distance(collider.transform.position, playerTransform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCollider = collider;
                }
            }
        }

        return closestCollider;
    }

    public float GetClosestDistanceWithTag(List<Collider2D> colliders, string tag)
    {
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag(tag))
            {
                float distance = Vector2.Distance(collider.transform.position, playerTransform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }
        }

        if (closestDistance == Mathf.Infinity) { return 0; }
        return closestDistance;
    }

    public int CountCollidersWithTag(List<Collider2D> colliders, string tag)
    {
        int count = 0;

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag(tag))
            {
                count++;
            }
        }

        return count;
    }

    public static void LogParameters(EventInstance instance)
    {
        EventDescription eventDescription;
        instance.getDescription(out eventDescription);

        int numParameters;
        eventDescription.getParameterDescriptionCount(out numParameters);

        for (int i = 0; i < numParameters; i++)
        {
            PARAMETER_DESCRIPTION parameterDescription;
            eventDescription.getParameterDescriptionByIndex(i, out parameterDescription);

            float value;
            instance.getParameterByID(parameterDescription.id, out value);


            Debug.LogFormat("Parameter '{0}': {1}", (string)parameterDescription.name, value);


        }
    }

    public void OnDrawGizmos()
    {
        try
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, outerDetectionRadius);
            Gizmos.DrawWireSphere(playerTransform.position, innerDetectionRadius);

            Gizmos.color = Color.white;

        }
        catch { }

    }

}