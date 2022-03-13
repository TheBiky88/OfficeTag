using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using System;

namespace Player
{
    public class NameTag : MonoBehaviour
    {
        [SerializeField] private GameObject canvas;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Color taggerColor;
        [SerializeField] private TextMeshProUGUI m_BountyText;
        private PlayerStatus PlayerStatus;

        void Start()
        {
            PhotonView pv = GetComponent<PhotonView>();
            PlayerStatus = GetComponent<PlayerStatus>();
            if (PhotonNetwork.IsConnected)
            {
                int playerIndex = Array.IndexOf(PhotonNetwork.PlayerList, pv.Owner);

                text.text = pv.Owner.NickName;
                text.color = RoomManager.Instance.playerColors[playerIndex];

                m_BountyText.color = RoomManager.Instance.playerColors[playerIndex];
                m_BountyText.gameObject.SetActive(false);

                PlayerStatus.Bounty.OnBountyChange += OnBountyUpdate;
            }
        }

        public void OnBountyUpdate(int bounty)
        {
            if (bounty <= 0)
            {
                m_BountyText.gameObject.SetActive(false);
            }
            else
            {
                if (!m_BountyText.gameObject.activeInHierarchy)
                {
                    m_BountyText.gameObject.SetActive(true);
                }
                m_BountyText.text = "Bounty: " + bounty;
            }
        }

        private void Update()
        {
            canvas.transform.LookAt(Camera.main.transform);
            canvas.transform.Rotate(Vector3.up * 180);

            if (PlayerStatus.PlayerRole == PlayerRoleEnum.Tagger)
            {
                text.outlineWidth = 0.5f;
                text.outlineColor = taggerColor;
            }
            else
            {
                text.outlineWidth = 0.3f;
                text.outlineColor = Color.grey;

            }
        }
    }
}