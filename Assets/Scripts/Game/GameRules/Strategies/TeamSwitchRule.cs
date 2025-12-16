// ============================================
// —“–ј“≈√≤я 2: Team Switch on Death (зм≥на команди)
// ѕ≥сл€ смерт≥ гравець переходить до команди вбивц≥
// ============================================
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamSwitchRule : GameRuleStrategy
{
    public override void Initialize(List<TeamMember> players)
    {
        base.Initialize(players);
    }

    protected override void ProcessPlayerDeath(GameObject source, TeamMember player)
    {
        // «находимо хто вбив гравц€
        var killer = source.GetComponent<TeamMember>();

        if (killer != null && killer.Team != player.Team)
        {
            // ѕереводимо гравц€ до команди вбивц≥
            TeamSide oldTeam = player.Team;
            player.SetTeam(killer.Team);

            Debug.Log($"[TeamSwitch] {player.name} переходить з команди {oldTeam} до команди {killer.Team}");

            // ¬≥дновлюЇмо здоров'€
            var healthProvider = player.GetComponent<IHealthProvider>();
            if (healthProvider != null)
            {
                healthProvider.Revive(null);
            }

            // ћожна зм≥нити в≥зуальне в≥дображенн€ (кол≥р, ск≥н ≥ т.д.)
            UpdatePlayerVisuals(player);
        }
        else
        {
            // якщо вбивц€ не визначений - просто деактивуЇмо
            Debug.Log($"[TeamSwitch] {player.name} вибуваЇ з гри");
            player.gameObject.SetActive(false);
        }
    }

    protected override void CheckVictoryCondition()
    {
        // √рупуЇмо активних гравц≥в по командах
        var playersByTeam = allPlayers
            .Where(p => p.gameObject.activeSelf)
            .GroupBy(p => p.Team)
            .ToList();

        // якщо вс≥ гравц≥ в одн≥й команд≥ - вона перемогла
        if (playersByTeam.Count == 1)
        {
            var winningTeam = playersByTeam[0].Key;
            var winners = playersByTeam[0].ToList();

            Debug.Log($"[TeamSwitch]  оманда {winningTeam} об'Їднала вс≥х гравц≥в!");

            var result = new VictoryResult(VictoryType.TeamVictory)
            {
                WinningTeam = winningTeam,
                Winners = winners
            };

            AnnounceVictory(result);
        }
        // якщо не залишилось активних гравц≥в
        else if (playersByTeam.Count == 0)
        {
            Debug.Log("[TeamSwitch] √ра зак≥нчена - не залишилось гравц≥в");
            AnnounceVictory(new VictoryResult(VictoryType.Draw));
        }
    }

    private void UpdatePlayerVisuals(TeamMember player)
    {
        // “ут можна зм≥нити кол≥р, матер≥ал, або ≥нш≥ в≥зуальн≥ елементи
        var renderer = player.GetComponent<Renderer>();
        if (renderer != null)
        {
            // ѕриклад: зм≥нити кол≥р в залежност≥ в≥д команди
            renderer.material.color = player.Team == 0 ? Color.red : Color.blue;
        }
    }
}