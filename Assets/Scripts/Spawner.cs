using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// The main Spawner behaviour.
/// </summary>
public class Spawner : MonoBehaviour
{
    /// <summary>
    /// Should we spawn obstacles?
    /// </summary>
    public bool spawnObstacles = true;

    /// <summary>
    /// Mean frequency of spawning as n per second.
    /// </summary>
    public AnimationCurve spawnFrequencyMean;
    
    /// <summary>
    /// Standard deviation of the frequency of spawning as n per second.
    /// </summary>
    public AnimationCurve spawnFrequencyStd;
    
    /// <summary>
    /// Position offset of the spawned obstacles.
    /// </summary>
    public float3 spawnOffset = new float3(0.0f, 0.0f, 0.0f);
    
    /// <summary>
    /// Size of the spawned obstacles.
    /// </summary>
    public AnimationCurve spawnSize;

    /// <summary>
    /// Obstacle speed over time
    /// </summary>
    public AnimationCurve obstacleSpeed;
    
    /// <summary>
    /// Layer used for the spawned obstacles.
    /// </summary>
    public string spawnLayer = "Obstacle";

    /// <summary>
    /// Prefab used for the spawned obstacles.
    /// </summary>
    public Obstacle obstaclePrefab;

    /// <summary>
    /// Accumulated time since the game start.
    /// </summary>
    private float elapsedTime = 0.0f;

    /// <summary>
    /// Accumulated time since the last spawn in seconds.
    /// </summary>
    private float spawnAccumulator = 0.0f;

    /// <summary>
    /// Number of seconds since the last spawn.
    /// </summary>
    private float nextSpawnIn = 0.0f;

    /// <summary>
    /// Called before the first frame update.
    /// </summary>
    void Start()
    { ResetSpawn(); }

    /// <summary>
    /// Update called once per frame.
    /// </summary>
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (spawnObstacles)
        { // Check if we should spawn.
            spawnAccumulator += Time.deltaTime;
            if (spawnAccumulator >= nextSpawnIn)
            { // Spawn at most one obstacle per frame.
                spawnAccumulator -= nextSpawnIn;
                nextSpawnIn = GetNextSpawn();
                
                SpawnObstacle();
            }
        }
    }

    /// <summary>
    /// Spawn obstacle if there is enough space.
    /// </summary>
    public void SpawnObstacle()
    {
        // Spawn the obstacle.
        var obstacle = Instantiate(obstaclePrefab, transform);
        var sizeNow = spawnSize.Evaluate(elapsedTime);

        // Move it to the target location.
        var spawnDown = RandomBool();
        obstacle.transform.position += (Vector3)(spawnDown ? 
            spawnOffset + (1.0f - sizeNow) / 2.0f : 
            -spawnOffset - (1.0f - sizeNow) / 2.0f
        );
        
        // Scale it.
        obstacle.transform.localScale = new Vector3(sizeNow, sizeNow, sizeNow);

        // Set speed
        obstacle.movementSpeed = obstacleSpeed.Evaluate(elapsedTime);
        
        // Move the obstacle into the correct layer.
        obstacle.gameObject.layer = LayerMask.NameToLayer(spawnLayer);
    }

    /// <summary>
    /// Clear all currently generated obstacles.
    /// </summary>
    public void ClearObstacles()
    {
        // Get obstacle layer to filter with.
        var obstacleLayer = LayerMask.NameToLayer(spawnLayer);
        foreach (Transform child in transform)
        { // Go through all children and destroy any obstacle found.
            if (child.gameObject.layer == obstacleLayer) 
            { Destroy(child.gameObject); }
        }
    }
    
    /// <summary>
    /// Reset the spawner to default state.
    /// </summary>
    public void ResetSpawn()
    {
        spawnAccumulator = 0.0f;
        nextSpawnIn = GetNextSpawn();
    }

    /// <summary>
    /// Calculate next random spawn time
    /// </summary>
    public float GetNextSpawn()
    {
        return Math.Max(0.2f, RandomNormal(
            spawnFrequencyMean.Evaluate(elapsedTime),
            spawnFrequencyStd.Evaluate(elapsedTime)
        ));
    }

    /// <summary>
    /// Modify current speed of all of the obstacles.
    /// </summary>
    public void ModifyObstacleSpeed(float multiplier)
    {
        // Get obstacle layer to filter with.
        var obstacleLayer = LayerMask.NameToLayer(spawnLayer);
        // Modify only the x-axis movement.
        var xMultiplier = new Vector2(multiplier, 1.0f);
        foreach (Transform child in transform)
        { // Iterate through all children, modifying current speed of obstacles.
            if (child.gameObject.layer == obstacleLayer) 
            { child.GetComponent<Rigidbody2D>().velocity *= xMultiplier; }
        }
    }

    /// <summary>
    /// Simple RNG for Normal distributed numbers with given
    /// mean and standard deviation.
    /// </summary>
    /// <param name="mean">Mean of the generated values.</param>
    /// <param name="std">Standard deviation of the generated values.</param>
    /// <returns>Returns random value from the normal distribution.</returns>
    public static float RandomNormal(float mean, float std)
    {
        var v1 = 1.0f - Random.value;
        var v2 = 1.0f - Random.value;
        
        var standard = Math.Sqrt(-2.0f * Math.Log(v1)) * Math.Sin(2.0f * Math.PI * v2);
        
        return (float)(mean + std * standard);
    }
    
    /// <summary>
    /// Generate a random bool - coin flip.
    /// </summary>
    /// <returns>Return a random boolean value.</returns>
    public static bool RandomBool()
    { return Random.value >= 0.5; }
}
