using Unity.Netcode;

public class HealthComponent : NetworkBehaviour
{
    public float maxHealth = 20;
    public float initialHealth = 20;

    public readonly NetworkVariable<float> Health = new();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Health.Value = initialHealth;
        }
    }
}
