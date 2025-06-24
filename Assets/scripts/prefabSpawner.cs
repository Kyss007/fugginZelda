using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class prefabSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;

    public void spawn()
    {
        Instantiate(prefabToSpawn, this.transform.position, Quaternion.identity);
    }
}
