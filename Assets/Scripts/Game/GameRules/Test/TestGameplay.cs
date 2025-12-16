using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TestGameplay : MonoBehaviour
{
    [SerializeField] GameRulesManager gameRulesManager;
    [SerializeField] List<GameObject> allPlayers;

    public void Awake()
    {
        gameRulesManager.InitializeGameRules(new TeamSwitchRule(), CreateTeamMembers(allPlayers));
    }

    private List<TeamMember> CreateTeamMembers(List<GameObject> allPlayers)
    {
        var result = new List<TeamMember>();
        foreach (var item in allPlayers)
        {
            result.Add(item.GetComponent<TeamMember>());
        }
        return result;
    }
}
