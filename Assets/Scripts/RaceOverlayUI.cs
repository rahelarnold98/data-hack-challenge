using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RaceOverlayUI : MonoBehaviour
{
    [Header("UI References")]
    //public TMP_Text titleText;
    public TMP_Text[] rowTexts; // assign 4 entries in Unity Inspector
    public TMP_Text highlightText;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TMPro.TextMeshProUGUI titleText;      
    [SerializeField] private TMPro.TextMeshProUGUI lapText;      



    public void ShowFinalResults(List<RaceCompetitorView> competitors)
    {
        if (titleText != null)
            titleText.text = "Men 500 m – Final A";

        for (int i = 0; i < rowTexts.Length; i++)
        {
            if (i < competitors.Count)
            {
                var c = competitors[i];
                rowTexts[i].text = $"{c.finalRank}. {c.name} ({c.country})  {c.finalTime}";
            }
            else
            {
                rowTexts[i].text = "";
            }
        }
    }
    
// Backwards-compatible: called without total laps
    public void ShowLapResults(int lapNumber, List<RaceCompetitorView> results)
    {
        // If you still want a single-line header, keep this.
        // But usually you'll want the event title untouched and only update lapText.
        SetLapIndicator(lapNumber, -1);

        ClearResults();
        ShowFinalResults(results); // reuse your list rendering
    }

    public void ShowLapResults(int lapNumber, int totalLaps, List<RaceCompetitorView> results)
    {
        SetLapIndicator(lapNumber, totalLaps);

        ClearResults();
        ShowFinalResults(results);
    }

    private void SetLapIndicator(int lapNumber, int totalLaps)
    {
        if (lapText == null) return;

        if (totalLaps > 0)
            lapText.text = $"Lap {lapNumber} / {totalLaps}";
        else
            lapText.text = $"Lap {lapNumber}";
    }

    public void ShowHeader(string text)
    {
        if (titleText != null)
            titleText.text = text;
    }


    
    public void ShowFastestLap(string label)
    {
        if (highlightText != null)
        {
            highlightText.text = label;
        }
    }

    public void ClearResults()
    {
        // Clear your UI list (depends on your implementation)
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