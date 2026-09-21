using TMPro;
using UnityEngine;

public class PlayerScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerName;
    [SerializeField] private TMP_Text _score;

    private OnlinePlayer _player;

    public void SetPlayer(OnlinePlayer player)
    {
        if(_player == player)
            return;

        _player = player;

        _playerName.text =
            $"PLAYER {player.Object.InputAuthority.PlayerId}";

        UpdateScore();
    }

    public void UpdateScore()
    {
        if(_player == null)
            return;

        _score.text = _player.Score.ToString();
    }

    public void Clear()
    {
        _player = null;

        _playerName.text = "";
        _score.text = "";
    }
}