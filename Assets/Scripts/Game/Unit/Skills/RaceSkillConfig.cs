using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RaceSkillEntry
{
    public UnitRace Race;
    public List<SkillData> Skills;
}

[CreateAssetMenu(fileName = "RaceSkillConfig", menuName = "Scriptable Objects/RaceSkillConfig")]
public class RaceSkillConfig : ScriptableObject
{
    [SerializeField]
    private List<RaceSkillEntry> _entries = new();

    public IReadOnlyList<SkillData> GetSkills(UnitRace race)
    {
        foreach (var entry in _entries)
        {
            if (entry.Race == race)
                return entry.Skills;
        }

        return null; // або порожній список, якщо хочеш
    }
}
