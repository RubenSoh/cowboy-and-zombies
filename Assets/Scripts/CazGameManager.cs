using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class CazGameManager : MonoBehaviour
{
    public int currentLevel = 0;

    private BarricadeObject[] barricades; 

    private void Awake()
    {
        barricades = FindObjectsByType<BarricadeObject>(FindObjectsSortMode.None);
    }

    public void LevelUp(int toLevel)
    {
        if (toLevel <= currentLevel) return;

        foreach (var barricade in barricades)
        {
            if (barricade.level > currentLevel && barricade.level <= toLevel)
            {
                StartCoroutine(DestroyBarricade(barricade));
            }
        }

        currentLevel = toLevel;
    }

    IEnumerator DestroyBarricade(BarricadeObject barricade)
    {
        barricade.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(0.4f);
        Destroy(barricade.gameObject);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
