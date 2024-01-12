using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemySpawner : MonoBehaviour
{
    string _prefix = "<< SPAWNER >> ";
    PlayerController _playerController;
    WorldSpawnMap _worldSpawnMap;
    public List<GameObject> enemyPrefabs;

    public Vector2 activateRange = new Vector2(10, 20);
    public float activateDelay = 10f;
    public float spawnDelay = 10f;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnerRoutine", activateDelay, spawnDelay);

        _worldSpawnMap = FindObjectOfType<WorldSpawnMap>();
    }

    public void SpawnerRoutine()
    {
        _playerController = FindObjectOfType<PlayerController>();
        if (_playerController == null) return;

        if (IsPlayerInActivateRange())
        {
            //Debug.Log($"{_prefix} Player is in range {activateRange}", this.gameObject);
            GameObject enemy = Instantiate(GetRandomEnemy(), transform.position, Quaternion.identity);
            enemy.GetComponent<EnemyAI>().target = _playerController.transform;
            enemy.transform.parent = null;
            _worldSpawnMap.RegisterAI(enemy.GetComponent<EnemyAI>());
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

    public GameObject GetRandomEnemy()
    {
        return enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
    }
}
