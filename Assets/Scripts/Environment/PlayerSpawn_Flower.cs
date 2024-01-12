using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn_Flower : MonoBehaviour
{
    public bool playerSpawned;

    [Space(10)]
    public GameObject player;
    public Transform playerPositionTarget;

    [Space(10)]
    public GameObject spawnEffect;

    [Space(10)]
    public float playerSpeed;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnFromFlower(1));
    }

    IEnumerator SpawnFromFlower(float spawnDelay)
    {
        playerSpawned = false;
        player.GetComponent<PlayerMovement>().state = PlayerState.INACTIVE;
        player.GetComponent<BoxCollider2D>().enabled = false;
        player.GetComponent<PlayerAnimator>().spriteParent.SetActive(false);


        yield return new WaitForSeconds(spawnDelay);

        player.GetComponent<PlayerAnimator>().spriteParent.SetActive(true);
        GameObject effect = Instantiate(spawnEffect, player.transform);
        Destroy(effect, 5);


        // << MOVE PLAYER TO SPAWN POINT >>
        while (Vector2.Distance(player.transform.position, playerPositionTarget.position) > 2)
        {
            player.transform.position = Vector3.Lerp(player.transform.position, playerPositionTarget.position, playerSpeed * Time.deltaTime);

            yield return null;
        }

        player.GetComponent<PlayerMovement>().moveTarget = player.transform.position;
        player.GetComponent<PlayerMovement>().state = PlayerState.IDLE;
        player.GetComponent<BoxCollider2D>().enabled = true;
        playerSpawned = true;

    }
}
