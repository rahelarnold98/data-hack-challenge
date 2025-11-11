using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceDataLoader : MonoBehaviour
{
    [Header("Data")]
    public TextAsset jsonFile;

    [Header("UI")]
    public RaceOverlayUI overlayUI;

    [Serializable]
    public class Root
    {
        public Data Data;
    }

    [Serializable]
    public class Data
    {
        public EventCompetitor[] Competitors;
        public Round[] Rounds;
    }

    [Serializable]
    public class EventCompetitor
    {
        public string Id;
        public CompetitorDetail Competitor;
        public int BibNumber;
    }

    [Serializable]
    public class CompetitorDetail
    {
        public Person Person;
        public string StartedForNfCode;
    }

    [Serializable]
    public class Person
    {
        public string FirstName;
        public string LastName;
    }

    [Serializable]
    public class Round
    {
        public string Name; 
        public Heat[] Heats;
    }

    [Serializable]
    public class Heat
    {
        public string Name;          
        public HeatCompetitor[] Competitors;
    }

    [Serializable]
    public class HeatCompetitor
    {
        public string CompetitionCompetitorId; 
        public int BibNumber;
        public int FinalRank;
        public string FinalResult;   
        public string ResultStatus;  
        public Lap[] Laps;
    }

    [Serializable]
    public class Lap
    {
        public string LapNumber;
        public int Rank;
        public string LapTime;
        public string Time;
        public string ResultDiff;
    }

    private void Start()
    {
        if (jsonFile == null)
        {
            Debug.LogError("RaceDataLoader: jsonFile not assigned!");
            return;
        }

        if (overlayUI == null)
        {
            Debug.LogError("RaceDataLoader: overlayUI not assigned!");
            return;
        }

        var root = JsonUtility.FromJson<Root>(jsonFile.text);

        var finalsRound = root.Data.Rounds.FirstOrDefault(r => r.Name == "Finals");
        if (finalsRound == null)
        {
            Debug.LogError("No 'Finals' round found in JSON.");
            return;
        }

        var finalA = finalsRound.Heats.FirstOrDefault(h => h.Name.Contains("Final"));
        if (finalA == null)
        {
            Debug.LogError("No 'Final A' heat found in Finals.");
            return;
        }

        var competitorById = new Dictionary<string, EventCompetitor>();
        foreach (var c in root.Data.Competitors)
        {
            competitorById[c.Id] = c;
        }

        var viewList = new List<RaceCompetitorView>();
        foreach (var c in finalA.Competitors)
        {
            var view = new RaceCompetitorView();
            view.finalRank = c.FinalRank;
            view.finalTime = c.FinalResult;

            if (competitorById.TryGetValue(c.CompetitionCompetitorId, out var ec))
            {
                var p = ec.Competitor.Person;
                var country = ec.Competitor.StartedForNfCode;
                view.name = $"{p.FirstName} {p.LastName}";
                view.country = country;
            }
            else
            {
                view.name = $"Bib {c.BibNumber}";
                view.country = "";
            }

            viewList.Add(view);
        }

        viewList = viewList
            .OrderBy(v => v.finalRank)
            .ToList();

        overlayUI.ShowFinalResults(viewList);
    }
}
