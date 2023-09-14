using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnDestroy : MonoBehaviour
{
    [SerializeField] private GameObject spawnPrefab;

    private void OnDestroy()
    {
        GameObject spawnedObject = Instantiate(spawnPrefab, transform.position, Quaternion.identity);
        spawnedObject.transform.parent = null;
    }
}
