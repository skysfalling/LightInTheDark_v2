using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemySpawner : MonoBehaviour
{
    string _prefix = "<< SPAWNER >> ";
    PlayerController _playerController;
    public GameObject enemyPrefab;

    public Vector2 activateRange = new Vector2(10, 20);
    public float spawnDelay = 10f;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnerRoutine", 0, spawnDelay);
    }

    public void SpawnerRoutine()
    {
        _playerController = FindObjectOfType<PlayerController>();
        if (_playerController == null) return;

        if (IsPlayerInActivateRange())
        {
            //Debug.Log($"{_prefix} Player is in range {activateRange}", this.gameObject);
            GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            enemy.GetComponent<EnemyAI>().target = _playerController.transform;
        }
    }

    public bool IsPlayerInActivateRange()
    {
        float distance = Vector3.Distance(transform.position, _playerController.transform.position);
        if (distance >= activateRange.x &&  distance <= activateRange.y)
        {
            return true;
        }
        return false;

    }
}
