using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class RaceOverlayUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text[] rowTexts;         
    public TMP_Text highlightText;      
    [SerializeField] private TMP_Text headerText; 

    private string currentEventTitle = "Men 500 m – Final A";
    private int currentLap = -1;
    private int totalLaps = -1;
    
    public int FastestLapNumber { get; private set; } = -1;
    public string FastestLapLabel { get; private set; } = "";



    private void UpdateHeader()
    {
        if (headerText == null) return;

        string lapInfo = "";
        if (currentLap > 0 && totalLaps > 0)
            lapInfo = $"Lap {currentLap} / {totalLaps}";
        else if (currentLap > 0)
            lapInfo = $"Lap {currentLap}";
        else if (totalLaps > 0)
            lapInfo = $"Laps: {totalLaps}";

        if (!string.IsNullOrEmpty(lapInfo))
            headerText.text = $"{currentEventTitle}   |   {lapInfo}";
        else
            headerText.text = currentEventTitle;
    }


    public void SetHeader(string title, int lapNumber = -1, int total = -1)
    {
        currentEventTitle = title ?? currentEventTitle;
        currentLap = lapNumber;
        totalLaps = total;
        UpdateHeader();
    }

    public void ShowNamesOnly(List<RaceCompetitorView> people, string title = null, int lapNumber = 1, int totalLaps = -1)
    {
        SetHeader(title, lapNumber, totalLaps);

        for (int i = 0; i < rowTexts.Length; i++)
        {
            if (i < people.Count)
            {
                var p = people[i];
                rowTexts[i].text = $"{p.name} ({p.country})";
            }
            else
            {
                rowTexts[i].text = "";
            }
        }
    }

    public void ShowLapResults(int lapNumber, int totalLaps, List<RaceCompetitorView> results, string title = null)
    {
        SetHeader(title, lapNumber, totalLaps);

        for (int i = 0; i < rowTexts.Length; i++)
        {
            if (i < results.Count)
            {
                var c = results[i];
                string rank = (c.finalRank >= int.MaxValue) ? "-" : c.finalRank.ToString();
                rowTexts[i].text = $"{rank}. {c.name} ({c.country})  {c.finalTime}";
            }
            else
            {
                rowTexts[i].text = "";
            }
        }
    }

    public void ShowFinalResults(List<RaceCompetitorView> competitors, string title = null)
    {
        SetHeader(title ?? "Men 500 m – Final A", -1, -1);

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

    
    public void ShowFastestLap(string label)
    {
        if (highlightText == null) return;

        bool hasText = !string.IsNullOrEmpty(label);
        highlightText.gameObject.SetActive(hasText);  
        highlightText.text = hasText ? label : string.Empty;
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
