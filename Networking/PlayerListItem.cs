using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Photon.Realtime.Player player;
    [SerializeField] private int actorNumber;
    [SerializeField] private GameObject kickButton;
    public void SetUp(Photon.Realtime.Player _player, bool isMaster)
    {
        player = _player;
        text.text = _player.NickName;
        actorNumber = _player.ActorNumber;
        
        if (isMaster)
        {
            kickButton.SetActive(true);
        }
        
        if (player.IsMasterClient)
        {
            kickButton.SetActive(false);
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }

    public void Kick()
    {
        PhotonNetwork.CloseConnection(player);
    }
}
