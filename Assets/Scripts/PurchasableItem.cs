using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PurchasableItem : NetworkBehaviour
{
    public GameObject item;
    public int levelUp = -1;
    public int price;
    
    void OnMouseEnter()
    {
        GetComponent<Light>().intensity = 1.25f;
        GetComponent<TextMeshPro>().color = Color.yellow;
    }

    void OnMouseExit()
    {
        GetComponent<Light>().intensity = 0.5f;
        GetComponent<TextMeshPro>().color = Color.white;
    }
}
