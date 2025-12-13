using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RaceStats
{
    public int MaxHealth;
    public float MoveSpeed;
}

[Serializable]
public class RaceStatsEntry
{
    public UnitRace Race;
    public RaceStats Stats;
}

[CreateAssetMenu(fileName = "RaceStatsConfig", menuName = "Scriptable Objects/RaceStatsConfig")]
public class RaceStatsConfig : ScriptableObject
{
    [SerializeField]
    private List<RaceStatsEntry> _entries = new();

    public RaceStats GetStats(UnitRace race)
    {
        foreach (var entry in _entries)
        {
            if (entry.Race == race)
                return entry.Stats;
        }

        Debug.LogWarning($"No stats config for race {race}");
        return null;
    }
}
