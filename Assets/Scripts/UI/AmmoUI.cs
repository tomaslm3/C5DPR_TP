using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text _currentAmmoText;
    [SerializeField] private TMP_Text _magazinesText; 

    [Header("Visual feedback")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _emptyColor = Color.red;

    private OnlinePlayer _localPlayer;

    private void Update()
    {
        if(_localPlayer == null)
        {
            _localPlayer = FindLocalPlayer();

            if(_localPlayer == null)
                return;
        }

        Refresh();
    }

    private OnlinePlayer FindLocalPlayer()
    {
        OnlinePlayer[] players =
            FindObjectsByType<OnlinePlayer>(FindObjectsSortMode.None);

        foreach(OnlinePlayer player in players)
        {
            if(player.HasInputAuthority)
                return player;
        }

        return null;
    }

    private void Refresh()
    {
        int currentAmmo = _localPlayer.CurrentAmmo;
        int capacity = _localPlayer.MagazineCapacity;

        if(_currentAmmoText != null)
        {
            _currentAmmoText.text = $"{currentAmmo} / {capacity}";
            _currentAmmoText.color =
                currentAmmo <= 0 ? _emptyColor : _normalColor;
        }

        if(_magazinesText != null)
        {
            int fullMagazines = CountNonEmptyMagazines();
            int totalMagazines = _localPlayer.MagazineCount;

            _magazinesText.text =
                $"Cargadores: {fullMagazines}/{totalMagazines}";
        }
    }

    private int CountNonEmptyMagazines()
    {
        int count = 0;

        for(int i = 0; i < _localPlayer.MagazineCount; i++)
        {
            if(_localPlayer.GetMagazineAmmo(i) > 0)
                count++;
        }

        return count;
    }
}