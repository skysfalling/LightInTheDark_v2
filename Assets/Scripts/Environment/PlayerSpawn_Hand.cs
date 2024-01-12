using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn_Hand : MonoBehaviour
{
    public bool playerSpawned;

    [Space(10)]
    public GameObject spawnHand;
    public GameObject player;

    [Space(10)]
    // on start move hand from start point -> hand let go point and them move player from that point to playerPosTarget;
    public Transform handLetGoPoint;
    public Transform playerPositionTarget;
    private Vector2 handStartPos;

    [Space(10)]
    public float handSpeed;
    public float playerSpeed;

    // Start is called before the first frame update
    void Start()
    {
        handStartPos = spawnHand.transform.position;

    }

    public void StartSpawnRoutine()
    {
        StartCoroutine(FullSpawnRoutine(1));
    }

    IEnumerator FullSpawnRoutine(float spawnDelay)
    {
        playerSpawned = false;
        player.transform.parent = spawnHand.transform;
        player.GetComponent<PlayerMovement>().state = PlayerState.INACTIVE;

        yield return new WaitForSeconds(spawnDelay);

        // << MOVE HAND >>
        while (Vector2.Distance(spawnHand.transform.position, handLetGoPoint.position) > 2)
        {
            spawnHand.transform.position = Vector3.Lerp(spawnHand.transform.position, handLetGoPoint.position, handSpeed * Time.deltaTime);

            yield return null;
        }

        player.transform.parent = null;


        // << MOVE PLAYER TO SPAWN POINT >>
        while (Vector2.Distance(player.transform.position, playerPositionTarget.position) > 2)
        {
            player.transform.position = Vector3.Lerp(player.transform.position, playerPositionTarget.position, playerSpeed * Time.deltaTime);

            yield return null;
        }

        player.GetComponent<PlayerMovement>().moveTarget = player.transform.position;
        player.GetComponent<PlayerMovement>().state = PlayerState.IDLE;
        playerSpawned = true;


        // << MOVE HAND BACK TO START >>
        while (Vector2.Distance(spawnHand.transform.position, handStartPos) > 2)
        {
            spawnHand.transform.position = Vector3.Lerp(spawnHand.transform.position, handStartPos, handSpeed * Time.deltaTime);

            yield return null;
        }


    }
}
