using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : NetworkBehaviour
{
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private float lifeTime = 1.5f;
    public float LifeTimer { get => lifeTimer; set => lifeTimer = value; }
    public Rigidbody2D Rigidbody2D { get => rb; }
    public UnityEvent<Projectile> OnProjectileDestroy = new UnityEvent<Projectile>();

    private Rigidbody2D rb;
    private float lifeTimer;
    private void Awake()
    {
        lifeTimer = lifeTime;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer < 0)
        {
            DestroySelf(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.CheckLayer(targetMask, other.gameObject.layer))
        {
            DestroySelf(this);
        }
    }

    private void DestroySelf(Projectile projectile)
    {
        OnProjectileDestroy?.Invoke(projectile);
        Destroy(gameObject);
    }
}
