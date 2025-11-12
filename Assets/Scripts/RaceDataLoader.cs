using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class RaceDataLoader : MonoBehaviour
{
    [Header("Data")]
    public TextAsset jsonFile;

    [Header("UI")]
    public RaceOverlayUI overlayUI;


    public int currentLapNumber = -1;

    public int TotalLaps { get; private set; } = 0;
    public List<float> LapEndTimes { get; private set; } = new List<float>(); 
    public int FastestLapNumber { get; private set; } = 1;
    public string FastestLapLabel { get; private set; } = "";


    [Serializable] public class Root { public Data Data; }
    [Serializable] public class Data { public EventCompetitor[] Competitors; public Round[] Rounds; }
    [Serializable] public class EventCompetitor { public string Id; public CompetitorDetail Competitor; public int BibNumber; }
    [Serializable] public class CompetitorDetail { public Person Person; public string StartedForNfCode; }
    [Serializable] public class Person { public string FirstName; public string LastName; }
    [Serializable] public class Round { public string Name; public Heat[] Heats; }
    [Serializable] public class Heat { public string Name; public HeatCompetitor[] Competitors; }
    [Serializable] public class HeatCompetitor
    {
        public string CompetitionCompetitorId;
        public int BibNumber;
        public int FinalRank;
        public string FinalResult;
        public string ResultStatus;
        public Lap[] Laps;
    }
    [Serializable] public class Lap
    {
        public string LapNumber;  
        public int Rank;           
        public string LapTime;     
        public string Time;        
        public string ResultDiff;  
    }

    private Root root;
    private Heat finalA;
    private Dictionary<string, EventCompetitor> competitorById;

    private void Start()
    {
        if (jsonFile == null) { Debug.LogError("RaceDataLoader: jsonFile not assigned!"); return; }
        if (overlayUI == null) { Debug.LogError("RaceDataLoader: overlayUI not assigned!"); return; }

        root = JsonUtility.FromJson<Root>(jsonFile.text);

        var finalsRound = root.Data.Rounds.FirstOrDefault(r => r.Name == "Finals");
        if (finalsRound == null) { Debug.LogError("No 'Finals' round found in JSON."); return; }

        finalA = finalsRound.Heats.FirstOrDefault(h => h.Name.Contains("Final"));
        if (finalA == null) { Debug.LogError("No 'Final A' heat found in Finals."); return; }

        competitorById = root.Data.Competitors.ToDictionary(c => c.Id, c => c);

        TotalLaps = GetTotalLaps(finalA);
        LapEndTimes = BuildLapTimelineSeconds(finalA); 
        
        int autoLapToShow = (currentLapNumber > 0) ? currentLapNumber : GetLatestCommonLap(finalA);
        if (autoLapToShow > 0)
        {
            var lapView = BuildLapLeaderboard(finalA, autoLapToShow, competitorById);
            overlayUI.ShowLapResults(autoLapToShow, TotalLaps, lapView, "Men 500 m – Final A");
        }
        else
        {
            ShowFinals(finalA, competitorById);
        }

        ShowFastestLap(finalA, competitorById);
    }


    public void RenderForLiveLap(int liveLap)
    {
        if (liveLap < 0)
        {
            int lapToShow = GetLatestCommonLap(finalA);
            if (lapToShow > 0)
            {
                var lapView = BuildLapLeaderboard(finalA, lapToShow, competitorById);
                overlayUI.ShowLapResults(lapToShow, TotalLaps, lapView, "Men 500 m – Final A");
            }
            else
            {
                ShowFinals(finalA, competitorById);
            }
            return;
        }

        if (liveLap <= 1)
        {
            var people = BuildEntrantsList(finalA, competitorById);
            overlayUI.ShowNamesOnly(people, "Men 500 m – Final A", lapNumber: 1, totalLaps: TotalLaps);
        }
        else if (liveLap >= 2 && liveLap <= TotalLaps)
        {
            int lapToShow = liveLap - 1;
            var lapView = BuildLapLeaderboard(finalA, lapToShow, competitorById);
            overlayUI.ShowLapResults(lapToShow, TotalLaps, lapView, "Men 500 m – Final A");
        }
        else
        {
            ShowFinals(finalA, competitorById);
        }
    }

    private List<RaceCompetitorView> BuildEntrantsList(Heat heat, Dictionary<string, EventCompetitor> byId)
    {
        var list = new List<RaceCompetitorView>();
        foreach (var c in heat.Competitors.OrderBy(x => x.BibNumber))
        {
            if (byId.TryGetValue(c.CompetitionCompetitorId, out var ec))
            {
                list.Add(new RaceCompetitorView
                {
                    name = $"{ec.Competitor.Person.FirstName} {ec.Competitor.Person.LastName}",
                    country = ec.Competitor.StartedForNfCode
                });
            }
            else
            {
                list.Add(new RaceCompetitorView { name = $"Bib {c.BibNumber}", country = "" });
            }
        }
        return list;
    }

    private void ShowFinals(Heat heat, Dictionary<string, EventCompetitor> byId)
    {
        var viewList = new List<RaceCompetitorView>();
        foreach (var c in heat.Competitors)
        {
            var view = new RaceCompetitorView
            {
                finalRank = c.FinalRank,
                finalTime = c.FinalResult
            };

            if (byId.TryGetValue(c.CompetitionCompetitorId, out var ec))
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
        overlayUI.ShowFinalResults(viewList, "Men 500 m – Final A");

    }

    private List<RaceCompetitorView> BuildLapLeaderboard(Heat heat, int lapNumber, Dictionary<string, EventCompetitor> byId)
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
            if (byId.TryGetValue(c.CompetitionCompetitorId, out var ec))
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

        if (perCompetitorSets.Count == 0) return -1;

        var common = new HashSet<int>(perCompetitorSets[0]);
        foreach (var s in perCompetitorSets.Skip(1)) common.IntersectWith(s);

        return common.Count > 0 ? common.Max() : -1;
    }

    private int GetTotalLaps(Heat heat)
    {
        return heat.Competitors.Max(c => c.Laps?.Select(l =>
        {
            if (int.TryParse(l.LapNumber, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)) return n;
            var m = Regex.Match(l.LapNumber ?? "", @"\d+");
            return m.Success ? int.Parse(m.Value) : 0;
        }).DefaultIfEmpty(0).Max() ?? 0);
    }
    

private List<float> BuildLapTimelineSeconds(Heat heat)
{
    var result = new List<float>();
    if (heat?.Competitors == null || heat.Competitors.Length == 0) return result;

    var perCompLapCum = new List<Dictionary<int, float>>();
    var perCompLapRank = new List<Dictionary<int, int>>();

    foreach (var c in heat.Competitors)
    {
        var map = new Dictionary<int, float>();
        var ranks = new Dictionary<int, int>();
        if (c?.Laps == null || c.Laps.Length == 0)
        {
            perCompLapCum.Add(map);
            perCompLapRank.Add(ranks);
            continue;
        }

        float cum = 0f;
        foreach (var l in c.Laps.OrderBy(LapNumberAsInt))
        {
            int n = LapNumberAsInt(l);
            if (n <= 0) continue;

            float tCum = ParseTimeSeconds(l.Time);
            if (tCum > 0f)
            {
                cum = tCum;
            }
            else
            {
                cum += ParseTimeSeconds(l.LapTime);
            }

            if (!map.ContainsKey(n)) map[n] = cum;
            if (!ranks.ContainsKey(n)) ranks[n] = l.Rank;
        }
        perCompLapCum.Add(map);
        perCompLapRank.Add(ranks);
    }

    float last = 0f;
    for (int lap = 1; lap <= TotalLaps; lap++)
    {
        float chosen = 0f;

        float leaderTime = 0f;
        bool haveLeader = false;
        for (int i = 0; i < perCompLapCum.Count; i++)
        {
            if (perCompLapRank[i].TryGetValue(lap, out int rank) && rank == 1 &&
                perCompLapCum[i].TryGetValue(lap, out float t))
            {
                leaderTime = t;
                haveLeader = true;
                break;
            }
        }

        if (haveLeader)
        {
            chosen = leaderTime;
        }
        else
        {
            var times = new List<float>();
            foreach (var map in perCompLapCum)
                if (map.TryGetValue(lap, out float t) && t > 0f)
                    times.Add(t);

            if (times.Count > 0)
            {
                times.Sort();
                int mid = times.Count / 2;
                chosen = (times.Count % 2 == 1) ? times[mid] : 0.5f * (times[mid - 1] + times[mid]);
            }
        }

        if (chosen <= last) chosen = last + 0.001f;

        result.Add(chosen);
        last = chosen;
    }

    return result;
}


    private static int LapNumberAsInt(Lap l)
    {
        if (int.TryParse(l.LapNumber, out var n)) return n;
        var m = Regex.Match(l.LapNumber ?? "", @"\d+");
        return m.Success ? int.Parse(m.Value) : 0;
    }

    private static float MaxCumulativeSeconds(HeatCompetitor c)
    {
        if (c.Laps == null) return 0f;
        float max = 0f, cumBySum = 0f;
        foreach (var l in c.Laps)
        {
            float cumulative = ParseTimeSeconds(l.Time);
            if (cumulative > 0f) max = Mathf.Max(max, cumulative);
            cumBySum += ParseTimeSeconds(l.LapTime);
        }
        return Mathf.Max(max, cumBySum);
    }

    public static float ParseTimeSeconds(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0f;
        s = s.Trim();

        var parts = s.Split(':');
        if (parts.Length == 2 && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var sec) &&
            int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var min))
        {
            return min * 60f + sec;
        }

        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var onlySec))
            return onlySec;

        return 0f;
    }

    private void ShowFastestLap(Heat heat, Dictionary<string, EventCompetitor> byId)
    {
        RaceCompetitorView fastestLapCompetitor = null;
        string fastestLapNumber = "";
        float fastestLapTime = float.MaxValue;

        foreach (var c in heat.Competitors)
        {
            byId.TryGetValue(c.CompetitionCompetitorId, out var ec);

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
}
