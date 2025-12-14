using UnityEngine;

public class TestUnit : MonoBehaviour
{
    [SerializeField] HealthProvider HealthProvider;
    [SerializeField] RaceSkillConfig RaceSkillConfig;
    [SerializeField] RaceStatsConfig RaceStatsConfig;
    [SerializeField] UnitRace UnitRace;

    void Start()
    {
        HealthProvider.Init(new HealthData
        {
            MaxHP = RaceStatsConfig.GetStats(UnitRace).MaxHealth,
        });
    }
}
