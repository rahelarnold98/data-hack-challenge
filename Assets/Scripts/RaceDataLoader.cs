using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class RaceDataLoader : MonoBehaviour
{
    [Header("Data")]
    public TextAsset jsonFile;

    [Header("UI")]
    public RaceOverlayUI overlayUI;

    [Header("Live")]
    [Tooltip("If < 0, automatically shows the latest common lap between all competitors.")]
    public int currentLapNumber = -1;

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

        var competitorById = root.Data.Competitors.ToDictionary(c => c.Id, c => c);

        int lapToShow = currentLapNumber > 0 ? currentLapNumber : GetLatestCommonLap(finalA);
        int totalLaps = finalA.Competitors.Max(c => c.Laps?.Length ?? 0);

        if (lapToShow > 0)
        {
            var lapView = BuildLapLeaderboard(finalA, lapToShow, competitorById);
            overlayUI.ShowLapResults(lapToShow, totalLaps, lapView);
        }
        else
        {
            Debug.LogWarning("Could not determine a common lap — showing final results instead.");
            ShowFinals(finalA, competitorById);
        }

        ShowFastestLap(finalA, competitorById);
    }

    
    private void ShowFinals(Heat finalA, Dictionary<string, EventCompetitor> competitorById)
    {
        var viewList = new List<RaceCompetitorView>();
        foreach (var c in finalA.Competitors)
        {
            var view = new RaceCompetitorView
            {
                finalRank = c.FinalRank,
                finalTime = c.FinalResult
            };

            if (competitorById.TryGetValue(c.CompetitionCompetitorId, out var ec))
            {
                var p = ec.Competitor.Person;
                view.name = $"{p.FirstName} {p.LastName}";
                view.country = ec.Competitor.StartedForNfCode;
            }
            else
            {
                view.name = $"Bib {c.BibNumber}";
                view.country = "";
            }
            viewList.Add(view);
        }

        viewList = viewList.OrderBy(v => v.finalRank).ToList();
        overlayUI.ShowFinalResults(viewList);
    }

   
    private List<RaceCompetitorView> BuildLapLeaderboard(
        Heat heat,
        int lapNumber,
        Dictionary<string, EventCompetitor> competitorById)
    {
        var list = new List<RaceCompetitorView>();

        foreach (var c in heat.Competitors)
        {
            var lap = c.Laps?.FirstOrDefault(l =>
            {
                if (int.TryParse(l.LapNumber, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                    return n == lapNumber;

                var m = Regex.Match(l.LapNumber ?? "", @"\d+");
                return m.Success && int.Parse(m.Value) == lapNumber;
            });

            string name, country;
            if (competitorById.TryGetValue(c.CompetitionCompetitorId, out var ec))
            {
                name = $"{ec.Competitor.Person.FirstName} {ec.Competitor.Person.LastName}";
                country = ec.Competitor.StartedForNfCode;
            }
            else
            {
                name = $"Bib {c.BibNumber}";
                country = "";
            }

            int rank = lap?.Rank ?? int.MaxValue;
            string lapTime = lap?.LapTime ?? "";
            string displayTime = !string.IsNullOrEmpty(lap?.Time) ? lap.Time : lapTime;
            string diff = lap?.ResultDiff ?? "";

            list.Add(new RaceCompetitorView
            {
                name = name,
                country = country,
                finalRank = rank,
                finalTime = string.IsNullOrEmpty(diff) ? displayTime : $"{displayTime} ({diff})"
            });
        }

        return list
            .OrderBy(v => v.finalRank == int.MaxValue ? 9999 : v.finalRank)
            .ThenBy(v => v.name)
            .ToList();
    }

    
    private int GetLatestCommonLap(Heat heat)
    {
        var perCompetitorSets = heat.Competitors
            .Select(c => c.Laps?
                .Select(l =>
                {
                    if (int.TryParse(l.LapNumber, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                        return n;

                    var m = Regex.Match(l.LapNumber ?? "", @"\d+");
                    return m.Success ? int.Parse(m.Value) : -1;
                })
                .Where(n => n > 0)
                .ToHashSet() ?? new HashSet<int>())
            .ToList();

        if (perCompetitorSets.Count == 0)
            return -1;

        var common = new HashSet<int>(perCompetitorSets[0]);
        foreach (var s in perCompetitorSets.Skip(1))
            common.IntersectWith(s);

        return common.Count > 0 ? common.Max() : -1;
    }
    
    
    private void ShowFastestLap(Heat heat, Dictionary<string, EventCompetitor> competitorById)
    {
        RaceCompetitorView fastestLapCompetitor = null;
        string fastestLapNumber = "";
        float fastestLapTime = float.MaxValue;

        foreach (var c in heat.Competitors)
        {
            competitorById.TryGetValue(c.CompetitionCompetitorId, out var ec);

            string fullName = ec != null
                ? $"{ec.Competitor.Person.FirstName} {ec.Competitor.Person.LastName}"
                : $"Bib {c.BibNumber}";
            string country = ec != null ? ec.Competitor.StartedForNfCode : "";

            if (c.Laps == null) continue;

            foreach (var lap in c.Laps)
            {
                if (float.TryParse(lap.LapTime, NumberStyles.Float, CultureInfo.InvariantCulture, out float t))
                {
                    if (t < fastestLapTime)
                    {
                        fastestLapTime = t;
                        fastestLapNumber = lap.LapNumber;
                        fastestLapCompetitor = new RaceCompetitorView
                        {
                            name = fullName,
                            country = country
                        };
                    }
                }
            }
        }

        if (fastestLapCompetitor != null)
        {
            string label = $"Fastest lap: {fastestLapCompetitor.name} ({fastestLapCompetitor.country}) – Lap {fastestLapNumber} in {fastestLapTime:F3} s";
            overlayUI.ShowFastestLap(label);
        }
    }
    
    
    /*private void Update()
    {
        // Press Right/Left Arrow to switch laps during Play Mode
        if (Input.GetKeyDown(KeyCode.RightArrow)) currentLapNumber++;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) currentLapNumber = Mathf.Max(1, currentLapNumber - 1);

        // Rebuild if you changed currentLapNumber manually
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            var root = JsonUtility.FromJson<Root>(jsonFile.text);
            var finalsRound = root.Data.Rounds.First(r => r.Name == "Finals");
            var finalA = finalsRound.Heats.First(h => h.Name.Contains("Final"));
            var competitorById = root.Data.Competitors.ToDictionary(c => c.Id, c => c);

            int totalLaps = finalA.Competitors.Max(c => c.Laps?.Length ?? 0);
            int lapToShow = Mathf.Clamp(currentLapNumber, 1, Mathf.Max(1, totalLaps));
            var lapView = BuildLapLeaderboard(finalA, lapToShow, competitorById);
            overlayUI.ShowLapResults(lapToShow, totalLaps, lapView);
        }
    }*/

}
