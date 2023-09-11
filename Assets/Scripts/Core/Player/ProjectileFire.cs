using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class ProjectileFire : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform firePortPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject muzzleFlashGameObject;
    [SerializeField] private Collider2D playerCollider;
    [Header("Settings")]
    [SerializeField] private float projectileSpeed = 30f;
    [SerializeField] private float fireRateInSeconds = 0.75f;
    [SerializeField] private float muzzleFlashDuration = 0.3f;

    private static ProjectileFire serverProjectileFire = null;
    private bool firing = false;
    private bool fireCoolDownReady = false;
    private GameObject spawnedPrefab = null;
    private float fireRateTimer;
    private float muzzleFlashTimer;
    private List<Projectile> activeProjectiles = new List<Projectile>();
    private Projectile spawnedProjectile = null;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }
        inputReader.PrimaryFireEvent.AddListener(HandleFilre);
        Debug.Log("1");
        if (IsServer)
        {
            serverProjectileFire = this;
        }
        else
        {
            CallForServerAlliveProjectilesServerRpc();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }
        inputReader.PrimaryFireEvent.RemoveListener(HandleFilre);
    }

    private void Update()
    {
        if (muzzleFlashTimer > 0f)
        {
            muzzleFlashTimer -= Time.deltaTime;
            if (muzzleFlashTimer <= 0f)
            {
                muzzleFlashGameObject.SetActive(false);
            }
        }

        if (!IsOwner) { return; }
        if (!fireCoolDownReady)
        {
            fireRateTimer -= Time.deltaTime;
            if (fireRateTimer < 0)
            {
                fireCoolDownReady = true;
            }
        }
        if (firing && fireCoolDownReady)
        {
            fireRateTimer = fireRateInSeconds;
            fireCoolDownReady = false;
            FireProjectileServerRpc(firePortPoint.position, firePortPoint.up);
            FireProjectile(firePortPoint.position, firePortPoint.up);
        }
    }

    private void HandleFilre(bool firePressed)
    {
        firing = firePressed;
    }

    private void FireProjectile(Vector3 position, Vector3 direction)
    {
        muzzleFlashGameObject.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;
        spawnedPrefab = Instantiate(clientProjectilePrefab, position, Quaternion.identity);
        spawnedPrefab.transform.up = direction;
        Physics2D.IgnoreCollision(playerCollider, spawnedPrefab.GetComponent<Collider2D>());
        if (spawnedPrefab.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }
    }

    [ServerRpc]
    private void FireProjectileServerRpc(Vector3 position, Vector3 direction, ServerRpcParams serverRpc = default)
    {
        Debug.Log($"Sender Id: {serverRpc.Receive.SenderClientId}");
        spawnedPrefab = Instantiate(serverProjectilePrefab, position, Quaternion.identity);
        spawnedPrefab.transform.up = direction;
        Physics2D.IgnoreCollision(playerCollider, spawnedPrefab.GetComponent<Collider2D>());
        if (spawnedPrefab.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDamageOnContact))
        {
            dealDamageOnContact.SetOwner(OwnerClientId);
        }
        if (spawnedPrefab.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }
        FireProjectileClientRpc(position, direction);
        if(!IsServer) { return; }
        if (serverProjectileFire != null && spawnedPrefab.TryGetComponent<Projectile>(out spawnedProjectile))
        {
            spawnedProjectile.OnProjectileDestroy.AddListener(ClearProjectilesList);
            serverProjectileFire.activeProjectiles.Add(spawnedProjectile);
        }
    }

    [ClientRpc]
    private void FireProjectileClientRpc(Vector3 position, Vector3 direction)
    {
        if(IsOwner) { return; }
        FireProjectile(position, direction);
    }

    private void ClearProjectilesList(Projectile projectile)
    {
        serverProjectileFire.activeProjectiles.Remove(projectile);
    }

    [ServerRpc]
    private void CallForServerAlliveProjectilesServerRpc(ServerRpcParams serverRpc = default)
    {
        foreach (Projectile item in serverProjectileFire.activeProjectiles)
        {
            Debug.Log("3");
            SpawnMissingProjectileClientRpc(serverRpc.Receive.SenderClientId, item.transform.position, item.transform.rotation, item.Rigidbody2D.velocity, item.LifeTimer);
        }
    }

    [ClientRpc]
    private void SpawnMissingProjectileClientRpc(ulong ownerClientId, Vector3 position, Quaternion rotation, Vector3 velocity, float lifeTimer)
    {
        Debug.Log("4");
        if (ownerClientId == OwnerClientId)
        {
            Debug.Log($"Client with id: {OwnerClientId} has been called");
            spawnedPrefab = Instantiate(clientProjectilePrefab, position, Quaternion.identity);
            spawnedPrefab.transform.rotation = rotation;
            if (spawnedPrefab.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.velocity = velocity;
            }
            if (spawnedPrefab.TryGetComponent<Projectile>(out spawnedProjectile))
            {
                spawnedProjectile.LifeTimer = lifeTimer;
            }
        }
    }
}