using UnityEngine;
using System.Collections.Generic;

public class HeartUI : MonoBehaviour
{
    public List<GameObject> AliveHearts = new List<GameObject>();
    public List<GameObject> DeadHearts = new List<GameObject>();

    public Movement movement;

    int HealthIndex = 3;

    public void RemoveHeart()
    {
        AliveHearts[HealthIndex].SetActive(false);
        HealthIndex--;
    }

    public void AddHeart()
    {
        if (HealthIndex >= AliveHearts.Count - 1) return;
        HealthIndex++;
        AliveHearts[HealthIndex].SetActive(true);
    }

    public void FullHeal()
    {
        for (int i = 0; i < AliveHearts.Count; i++)
            AliveHearts[i].SetActive(true);
        HealthIndex = AliveHearts.Count - 1;
    }

    private void Update()
    {
        if (HealthIndex < 0)
        {
            movement.Die();
        }
    }
}
