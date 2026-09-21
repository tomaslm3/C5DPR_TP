using Fusion;
using UnityEngine;

public class ScoreboardUI : MonoBehaviour
{
    [SerializeField] private PlayerScoreUI[] _playerScores;

    private void Update()
    {
        if(GameManager.Instance == null)
        {
            Debug.LogWarning("SCOREBOARD | GameManager.Instance == null");
            return;
        }

        RefreshPlayers();
        RefreshScores();
    }

    private void RefreshPlayers()
    {
        var players = GameManager.Instance.Players;

        Debug.Log(
            $"SCOREBOARD | Players registrados: {players.Count}"
        );

        for(int i = 0; i < _playerScores.Length; i++)
        {
            if(i >= players.Count)
            {
                _playerScores[i].Clear();
                continue;
            }

            PlayerRef playerRef = players[i];

            Debug.Log(
                $"SCOREBOARD | Buscando PlayerObject de {playerRef}"
            );

            if(!GameManager.Instance.Runner.TryGetPlayerObject(
                playerRef,
                out NetworkObject playerObject))
            {
                Debug.LogWarning(
                    $"SCOREBOARD | No existe PlayerObject para {playerRef}"
                );

                continue;
            }

            OnlinePlayer player =
                playerObject.GetComponent<OnlinePlayer>();

            if(player == null)
            {
                Debug.LogError(
                    $"SCOREBOARD | PlayerObject de {playerRef} " +
                    $"no tiene OnlinePlayer."
                );

                continue;
            }

            Debug.Log(
                $"SCOREBOARD | Player encontrado: {playerRef} | " +
                $"Score: {player.Score}"
            );

            _playerScores[i].SetPlayer(player);
        }
    }

    private void RefreshScores()
    {
        foreach(PlayerScoreUI scoreUI in _playerScores)
        {
            scoreUI.UpdateScore();
        }
    }
}