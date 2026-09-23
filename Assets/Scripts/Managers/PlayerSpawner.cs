using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject _playerPrefab;

   
    [SerializeField] private Transform[] _spawnTransforms;

   
    private bool _initialized;
    
    public void PlayerJoined(PlayerRef player)
    {
        var playersCount = Runner.SessionInfo.PlayerCount;
        
        if (_initialized && playersCount >= 2)
        {
            CreatePlayer(0);
            return;
        }
        
        if (player == Runner.LocalPlayer)
        {
            if(playersCount < 2)
            {
                _initialized = true;
            }
            else
            {
                CreatePlayer(playersCount - 1);
            }
        }
    }

    void CreatePlayer(int spawnPointIndex)
    {
        _initialized = false;

        NetworkObject playerObject = Runner.Spawn(
        _playerPrefab,
        _spawnTransforms[spawnPointIndex].position,
        _spawnTransforms[spawnPointIndex].rotation,
        Runner.LocalPlayer
    );

        Runner.SetPlayerObject(
            Runner.LocalPlayer,
            playerObject
        );
    }
}
