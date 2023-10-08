using System;
using Unity.Netcode;
using UnityEngine;

public class ZombieSpawner : NetworkBehaviour
{
    public GameObject zombiePrefab;
    private CazGameManager gameManager;

    public int level = 0;
    // default 30 seconds per zombie wave
    public double waveTime = 30;
    public double spawnTime = 1;
    public int waveSize = 5;

    private double _lastWaveTime = 0;
    private double _lastSpawnTime = 0;
    private int _zombiesRemaining = 0;

    private void Awake()
    {
        gameManager = GameObject.Find("CazGameManager").GetComponent<CazGameManager>();
    }

    private void FixedUpdate()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        
        if (gameManager.currentLevel != level)
        {
            _lastSpawnTime = -1;
            _lastWaveTime = -1;
            return;
        }
        
        var currentTime = Time.timeAsDouble;
        
        if (currentTime - _lastWaveTime > waveTime)
        {
            _lastWaveTime = currentTime;
            _zombiesRemaining = waveSize;
        }

        if (_zombiesRemaining > 0 && currentTime - _lastSpawnTime > spawnTime)
        {
            _lastSpawnTime = currentTime;
            _zombiesRemaining--;
            SpawnZombie();
        }
        
    }

    private void SpawnZombie()
    {
        // Create a new zombie
        var zombie = Instantiate(zombiePrefab, gameObject.transform);
        
        // spawn it over the network
        zombie.GetComponent<NetworkObject>().Spawn();
    }
}
