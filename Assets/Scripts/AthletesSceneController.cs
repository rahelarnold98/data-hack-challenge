using System.Collections.Generic;
using UnityEngine;

public class AthletesSceneController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private List<AthleteData> athletes = new();

    [Header("UI")]
    [SerializeField] private Transform contentParent;

    [SerializeField] private AthleteCardView cardPrefab;

    private void Start()
    {
        Populate();
    }


    private void Populate()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        foreach (var athlete in athletes)
        {
            var card = Instantiate(cardPrefab, contentParent);
            card.Setup(athlete);
        }
    }
}