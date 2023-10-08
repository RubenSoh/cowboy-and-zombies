using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CazZombie : NetworkBehaviour
{

    public Material undamagedMaterial;
    public Material damagedMaterial;
    public double damageDuration = 0.2;
    public double attackDuration = 1;
    public float attackDistance = 2;
    public float attackDamage = 4;
    
    private double lastDamageTime = -1;
    private double lastAttackTime = -1;
    private HealthComponent Health;

    private MeshRenderer Renderer;

    private AudioSource AttackSound;
    // Start is called before the first frame update
    void Start()
    {
        Renderer = GetComponentInChildren<MeshRenderer>();
        Health = GetComponent<HealthComponent>();
        AttackSound = GetComponent<AudioSource>();
        Health.Health.OnValueChanged += OnHealthChange;
    }

    private void OnHealthChange(float oldHealth, float newHealth)
    {
        if (oldHealth > newHealth)
        {
            lastDamageTime = Time.timeAsDouble;
        }

        if (newHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        var currentTime = Time.timeAsDouble;
        if (lastDamageTime > 0 && currentTime - lastDamageTime < damageDuration)
        {
            Renderer.material = damagedMaterial;
        }
        else
        {
            Renderer.material = undamagedMaterial;
        }

        if (currentTime - lastAttackTime > attackDuration)
        {
            TryAttackPlayer();
        }
    }

    void TryAttackPlayer()
    {
        var thisPos = transform.position;
        var players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float distToClosest = float.MaxValue;
        foreach (var player in players)
        {
            float dist = Vector3.Distance(thisPos, player.transform.position);
            if (closest == null || dist < distToClosest)
            {
                closest = player;
                distToClosest = dist;
            }
        }


        if (distToClosest > attackDistance)
        {
            return;
        }
        
        lastAttackTime = Time.timeAsDouble;
        HealthComponent playerHealth = closest.GetComponent<HealthComponent>();
        playerHealth.Health.Value -= attackDamage;
        AttackSound.Play();
    }
}
