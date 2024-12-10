using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

/**
 * This component instantiates a given prefab at random time intervals and random bias from its object position.
 */
public class TimedSpawnerRandom: MonoBehaviour {
    [SerializeField] GameObject prefabToSpawn;

    [Tooltip("Minimum time between consecutive spawns, in seconds")] 
    [SerializeField] float minTimeBetweenSpawns = 0.2f;

    [Tooltip("Maximum time between consecutive spawns, in seconds")] 
    [SerializeField] float maxTimeBetweenSpawns = 1.0f;

    [Tooltip("Maximum distance in X between spawner and spawned objects, in meters")] 
    [SerializeField] float maxXDistance = 1.5f;

    [Tooltip("Maximum distance in X between spawner and spawned objects, in meters")] 
    [SerializeField] float maxYDistance = 1.5f;

    [SerializeField] Transform parentOfAllInstances;
    private float zValue = 0;


    void Start() {
         SpawnRoutine();
    }

    async void SpawnRoutine() {
        while (true) {
            float timeBetweenSpawnsInSeconds = Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns);
            await Awaitable.WaitForSecondsAsync(timeBetweenSpawnsInSeconds);
            if (!this) break;   // might be destroyed when moving to a new scene
            Vector3 positionOfSpawnedObject = new Vector3(
                transform.position.x + Random.Range(-maxXDistance, +maxXDistance),
                transform.position.y + Random.Range(-maxYDistance, +maxYDistance),
                transform.position.z + zValue++);
            GameObject newObject = Instantiate(prefabToSpawn.gameObject, positionOfSpawnedObject, Quaternion.identity);
            newObject.transform.parent = parentOfAllInstances;
        }
    }
}
