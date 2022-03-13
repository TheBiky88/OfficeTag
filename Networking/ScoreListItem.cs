using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    public int Points;
    public string NickName;

    public void Setup(Game.PlayerStats playerStats)
    {
        NickName = playerStats.NickName;
        Points = playerStats.Points;

        text.text = $"{NickName}: {Points}";
    }
}
