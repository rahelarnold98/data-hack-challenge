using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RaceOverlayUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text[] rowTexts; // assign 4 entries in Unity Inspector
    public TMP_Text highlightText;


    public void ShowFinalResults(List<RaceCompetitorView> competitors)
    {
        if (titleText != null)
            titleText.text = "Men 500 m – Final A";

        for (int i = 0; i < rowTexts.Length; i++)
        {
            if (i < competitors.Count)
            {
                var c = competitors[i];
                // Example: "1. Steven DUBOIS (CAN) 40.745"
                rowTexts[i].text = $"{c.finalRank}. {c.name} ({c.country})  {c.finalTime}";
            }
            else
            {
                rowTexts[i].text = "";
            }
        }
    }
    
    public void ShowFastestLap(string label)
    {
        if (highlightText != null)
        {
            highlightText.text = label;
        }
    }

}

[System.Serializable]
public class RaceCompetitorView
{
    public int finalRank;
    public string name;
    public string country;
    public string finalTime;
}