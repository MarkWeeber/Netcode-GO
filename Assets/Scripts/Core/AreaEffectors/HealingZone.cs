using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealingZone : NetworkBehaviour
{
    [Header("Refernces")]
    [SerializeField] private Image healPowerBar;
    [Header("Settings")]
    [SerializeField] private int maxHealPower = 30;
    [SerializeField] private float healCooldown = 45f;
    [SerializeField] private float healTickRate = 1f;
    [SerializeField] private int coinsPerTick = 10;
    [SerializeField] private int healthPerTick = 10;

    private List<PlayerInstance> playersInZone = new List<PlayerInstance>();
    private NetworkVariable<int> HealPower = new NetworkVariable<int>();
    private float remainingCooldown;
    private float tickTimer;

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            HealPower.OnValueChanged += HandleHealPowerChange;
            HandleHealPowerChange(0, HealPower.Value);
        }
        if (IsServer)
        {
            HealPower.Value = maxHealPower;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            HealPower.OnValueChanged -= HandleHealPowerChange;
        }
    }

    private void Update()
    {
        ManageHealZone();
    }

    private void ManageHealZone()
    {
        if (!IsServer) { return; }
        if (remainingCooldown > 0f)
        {
            remainingCooldown -= Time.deltaTime;
            if (remainingCooldown <= 0f)
            {
                HealPower.Value = maxHealPower;
            }
            else
            {
                return;
            }
        }
        tickTimer += Time.deltaTime;
        if (tickTimer >= 1 / healTickRate)
        {
            foreach (PlayerInstance player in playersInZone)
            {
                if (HealPower.Value == 0)
                {
                    break;
                }
                if (player.Health.CurrentHealth.Value == player.Health.maxHealth)
                {
                    continue;
                }
                if (player.Inventory.TotalCoins.Value < coinsPerTick)
                {
                    continue;
                }
                player.Inventory.TotalCoins.Value -= coinsPerTick;
                player.Health.RestoreHealth(healthPerTick);
                HealPower.Value -= 1;
                if (HealPower.Value == 0)
                {
                    remainingCooldown = healCooldown;
                    break;
                }
            }
            tickTimer = tickTimer % (1 / healTickRate);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
        {
            return;
        }
        if (collision.attachedRigidbody.TryGetComponent<PlayerInstance>(out PlayerInstance player))
        {
            playersInZone.Add(player);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsServer)
        {
            return;
        }
        if (collision.attachedRigidbody.TryGetComponent<PlayerInstance>(out PlayerInstance player))
        {
            playersInZone.Remove(player);
        }
    }

    private void HandleHealPowerChange(int oldHealPowerValue, int newHealPowerValue)
    {
        healPowerBar.fillAmount = (float) newHealPowerValue / maxHealPower;
    }
}
