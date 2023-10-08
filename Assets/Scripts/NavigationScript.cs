using UnityEngine;
using UnityEngine.AI;

public class NavigationScript : MonoBehaviour
{

    public Transform player;
    private NavMeshAgent agent;
    
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (agent.isOnNavMesh && player)
        {
            agent.destination = player.transform.position;
        }
    }
}
