using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityPipeline.Microsoft.CodeAnalysis.CSharp.Syntax;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game")]
    [SerializeField, Min(1f)]
    private float _gameDuration = 60f;

    [Header("Result UI")]
    [SerializeField] private GameObject _winImage;
    [SerializeField] private GameObject _loseImage;

    private List<PlayerRef> _players;

    public IReadOnlyList<PlayerRef> Players => _players;

    [Networked]
    public NetworkBool GameStarted { get; set; }

    public int ConnectedPlayers => _players.Count;
    public int RequiredPlayers => 2;

    [Networked]
    public TickTimer GameTimer { get; private set; }

    private bool _gameFinished;

    private void Awake()
    {
        Instance = this;

        _players = new List<PlayerRef>();
    }

    public void AddToList(OnlinePlayer player)
    {
        PlayerRef playerRef =
            player.Object.InputAuthority;

        Debug.Log(
            $"GAMEMANAGER | Registrando player: {playerRef}"
        );

        if(!_players.Contains(playerRef))
        {
            _players.Add(playerRef);

            Debug.Log(
                $"GAMEMANAGER | Player agregado: {playerRef} | " +
                $"Total: {_players.Count}"
            );
        }

        TryStartGame();
    }

    private void TryStartGame()
    {
        if(!HasStateAuthority)
            return;

        if(GameStarted)
            return;

        if(_players.Count < 2)
            return;

        GameStarted = true;

        GameTimer = TickTimer.CreateFromSeconds(
            Runner,
            _gameDuration
        );

        Debug.Log(
            $"GAME STARTED | Duration: {_gameDuration}"
        );
    }

    public override void FixedUpdateNetwork()
    {
        if(!HasStateAuthority)
            return;

        if(!GameStarted || _gameFinished)
            return;

        if(!GameTimer.Expired(Runner))
            return;

        FinishGame();
    }

    private void FinishGame()
    {
        if(_gameFinished)
            return;

        _gameFinished = true;

        Debug.Log("GAME FINISHED");

        if(_players.Count < 2)
        {
            Debug.LogWarning(
                "No hay suficientes jugadores para determinar un ganador."
            );

            return;
        }

        OnlinePlayer player1 =
            GetOnlinePlayer(_players[0]);

        OnlinePlayer player2 =
            GetOnlinePlayer(_players[1]);

        if(player1 == null || player2 == null)
        {
            Debug.LogWarning(
                "No se pudieron obtener ambos jugadores."
            );

            return;
        }

        Debug.Log(
            $"FINAL SCORE | " +
            $"Player 1: {player1.Score} | " +
            $"Player 2: {player2.Score}"
        );

        if(player1.Score > player2.Score)
        {
            RPC_ShowWin(
                _players[0]
            );

            RPC_ShowLose(
                _players[1]
            );
        } else if(player2.Score > player1.Score)
        {
            RPC_ShowWin(
                _players[1]
            );

            RPC_ShowLose(
                _players[0]
            );
        } else
        {
            Debug.Log("GAME FINISHED | EMPATE");

            RPC_ShowDraw();
        }
    }
    public float GetRemainingTime()
    {
        if(!GameStarted)
            return _gameDuration;

        float remaining =
            GameTimer.RemainingTime(Runner) ?? 0f;

        return Mathf.Max(
            remaining,
            0f
        );
    }

    private OnlinePlayer GetOnlinePlayer(
        PlayerRef playerRef)
    {
        if(!Runner.TryGetPlayerObject(
            playerRef,
            out NetworkObject playerObject))
        {
            return null;
        }

        return playerObject.GetComponent<OnlinePlayer>();
    }

    [Rpc]
    private void RPC_ShowWin(
        [RpcTarget] PlayerRef player)
    {
        if(player != Runner.LocalPlayer)
            return;

        Win();
    }

    [Rpc]
    private void RPC_ShowLose(
        [RpcTarget] PlayerRef player)
    {
        if(player != Runner.LocalPlayer)
            return;

        Defeat();
    }

    [Rpc]
    private void RPC_ShowDraw()
    {
        if(_winImage != null)
        {
            _winImage.SetActive(true);
        }
    }

    private void Win()
    {
        if(_winImage != null)
        {
            _winImage.SetActive(true);
        }
    }

    private void Defeat()
    {
        if(_loseImage != null)
        {
            _loseImage.SetActive(true);
        }
    }
}
