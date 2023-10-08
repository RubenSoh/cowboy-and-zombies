using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CazGun : NetworkBehaviour
{
    private LineRenderer laserLine;

    private CazPlayer player;
    
    [ServerRpc]
    public void ShootGunServerRpc(Vector3 lookWorldPosition)
    {
        if (!NetworkManager.ConnectedClients.ContainsKey(OwnerClientId)) return;

        var client = NetworkManager.ConnectedClients[OwnerClientId];
        var castRay = new Ray(client.PlayerObject.transform.position + new Vector3(-0.31f, 1.822f, -0.443f),
            lookWorldPosition);

        ShootGunClientRpc(client.PlayerObject.transform.position, lookWorldPosition);

        if (!Physics.Raycast(castRay, out var rayCastHit, 100.0f)) return;
        
        var hitObject = rayCastHit.transform.gameObject;
        Debug.Log("Raycast hit " + hitObject.name);
        if (hitObject == null || !hitObject.TryGetComponent(out HealthComponent health)) return;
        
        Debug.Log("Removing 10 health from " + hitObject.name + " with " + health.Health.Value);
        health.Health.Value -= 10;

        if (health.Health.Value <= 0 && hitObject.GetComponent<CazZombie>())
        {
            player.money.Value += 10;
        }
    }

    [ClientRpc]
    private void ShootGunClientRpc(Vector3 clientPosition, Vector3 lookWorldPosition)
    {
        laserLine.SetPosition(0, clientPosition);
        laserLine.SetPosition(1, clientPosition + lookWorldPosition * 10);

        StartCoroutine(ShotEffect());
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<CazPlayer>();
        laserLine = GetComponent<LineRenderer>();
    }

    private IEnumerator ShotEffect()
    {
        var shootSound = GetComponentInChildren<AudioSource>();
        shootSound.Play();

        // Turn on our line renderer
        laserLine.enabled = true;

        //Wait for .07 seconds
        yield return new WaitForSeconds(0.5f);

        // Deactivate our line renderer after waiting
        laserLine.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
    }
}