using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform[] spawnPoints;

    private void Start()
    {
        foreach(Transform spawnPoint in spawnPoints)
        {
            spawnPoint.gameObject.SetActive(false);
        }
    }

    public Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

}
