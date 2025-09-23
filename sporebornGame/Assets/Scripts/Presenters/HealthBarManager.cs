using System.Collections.Generic;
using UnityEngine;


public class HealthBarManager : MonoBehaviour
{
    public GameObject heart;
    public HealthModel playerHealth;

    List<HealthHeart> hearts = new List<HealthHeart>();

    private void OnEnable()
    {
        playerHealth.OnHealthChanged += DrawHearts;
    }

    private void OnDisable()
    {
        playerHealth.OnHealthChanged -= DrawHearts;
    }

    private void Start()
    {
        DrawHearts();

    }

    public void CreateEmptyHeart()
    {
        GameObject newHeart = Instantiate(heart);
        newHeart.transform.SetParent(transform);

        HealthHeart heartComponent = newHeart.GetComponent<HealthHeart>();
        heartComponent.SetHeartImage(HealthHeart.HeartStatus.Empty);
        hearts.Add(heartComponent);
    }

    public void ClearHearts()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }

        hearts = new List<HealthHeart>();
    }

    public void DrawHearts()
    {
        ClearHearts();
        float maxHealthRemainder = playerHealth.maxHealth % 2;
        int heartsToMake = (int)((playerHealth.maxHealth / 2) + maxHealthRemainder);

        for (int i = 0; i < heartsToMake; i++)
        {
            CreateEmptyHeart();
        }

        for (int i = 0; i < hearts.Count; i++)
        {
            int heartStatusRemainder = (int)Mathf.Clamp(playerHealth.currHealth - (i * 2), 0, 2);
            hearts[i].SetHeartImage((HealthHeart.HeartStatus)heartStatusRemainder);
        }
    }

}
