using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    private static Launcher instance;
    public static Launcher Instance { get { return instance; } }

    [Header("Create Room Menu")]
    [SerializeField] TMP_InputField roomNameInputField;

    [Header("Find Room Menu")]
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    private List<RoomInfo> roomInfoList;

    [Header("Room Menu")]
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject[] hostButtons;
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text mapName;
    [SerializeField] string[] mapNames;

    public int currentMapInt;
    public int gameTime = 120;

    [SerializeField] private float AFKTimer = 300;
    private bool afkKick;

    [Header("Score Menu")]
    [SerializeField] Transform scoreListContent;
    [SerializeField] GameObject scoreListItemPrefab;
    [SerializeField] GameObject scoreTransferPrefab;
    [SerializeField] private GameObject scoreTransferObject;

    [Header("Error Menu")]
    [SerializeField] TMP_Text errorText;


    private byte maxPlayers = 8;
    private RoomOptions roomOptions = new RoomOptions();

    private List<GameObject> menuScoreBoardList;
    private bool returnToRoom;

    [HideInInspector] public List<Game.PlayerStats> playerStats;
    [HideInInspector] public string roomName;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else instance = this;

        currentMapInt = 1;
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();

        roomOptions.MaxPlayers = maxPlayers;
        roomInfoList = new List<RoomInfo>();
        PhotonNetwork.EnableCloseConnection = true;

        string nickName = PlayerPrefs.GetString("name", "Player: " + Random.Range(0, 1000).ToString("0000"));
        if (!nickName.Contains("Rick") && !nickName.Contains("Rik"))
        {
            StartCoroutine(AudioManager.Instance.MenuMusic());
        }
        else
        {
            AudioManager.Instance.ForceRickRoll();
        }

        if (playerStats != null)
        {
            OpenScoreMenu();
        }
    }

    private IEnumerator IAFKTimer()
    {
        ResetAFKTimer();
        while (AFKTimer > 0)
        {
            AFKTimer--;
            yield return new WaitForSeconds(1);
        }

        if (AFKTimer == 0)
        {
            afkKick = true;
            Disconnect();
            MenuManager.Instance.OpenMenu("Disconnected");
        }
    }

    public void ResetAFKTimer()
    {
        AFKTimer = 300;
    }

    public void ConnectToGame()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.MaxCcuReached)
        {
            ThrowError("Servers are experiencing some issues.\nPlease try again later!");
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        StartCoroutine("IAFKTimer");
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
        if (scoreTransferObject)
        {
            Destroy(scoreTransferObject);
            scoreTransferObject = null;
        }
        StopCoroutine("IAFKTimer");
    }

    public void ShowStatsMenu()
    {
        Disconnect();
        OpenScoreMenu();
        ShowScore(playerStats);
        playerStats = null;
    }

    public override void OnJoinedLobby()
    {
        scoreTransferObject = Instantiate(scoreTransferPrefab);

        if (returnToRoom)
        {
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, null);
            returnToRoom = false;
        }
        else
        {
            MenuManager.Instance.OpenMenu("LobbyFinder");
        }
    }

    public void ThrowError(string message)
    {
        errorText.text = message;
        MenuManager.Instance.OpenMenu("Error");
    }

    public void CreateRoom()
    {
        PhotonNetwork.NickName = PlayerPrefs.GetString("name", "Player: " + Random.Range(0, 1000).ToString("0000"));
        //var profanties = MenuManager.Instance.ProfanityFilter.co
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        else if (MenuManager.Instance.ProfanityFilter.ContainsProfanity(roomNameInputField.text))
        {
            OnCreateRoomFailed(420, "Watch your profanity! Please choose a normal name!");
            return;
        }

        PhotonNetwork.CreateRoom(roomNameInputField.text, roomOptions);
        Debug.Log("CreateRoom");
        MenuManager.Instance.OpenMenu("Loading");
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient) HostButtonsEnabler(true);
        else HostButtonsEnabler(false);
        UpdateGameSettings();
        MenuManager.Instance.OpenMenu("Room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        EndGameTransfer.Instance.roomName = PhotonNetwork.CurrentRoom.Name;

        foreach(Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(PhotonNetwork.PlayerList[i], PhotonNetwork.IsMasterClient);
        }
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        ResetAFKTimer();
        foreach (Transform transform in playerListContent)
        {
            Destroy(transform.gameObject);
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(PhotonNetwork.PlayerList[i], PhotonNetwork.IsMasterClient);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        string newMessage = "Room Creation Failed: " + message;
        ThrowError(newMessage);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        Debug.Log("Leave Room");
        MenuManager.Instance.OpenMenu("Loading");
    }

    private void ShowScore(List<Game.PlayerStats> playerStats)
    {
        menuScoreBoardList = new List<GameObject>();

        for (int i = 0; i < playerStats.Count; i++)
        {
            GameObject item = Instantiate(scoreListItemPrefab, scoreListContent);
            menuScoreBoardList.Add(item);
            item.GetComponent<ScoreListItem>().Setup(playerStats[i]);
        }
        #region old sort
        for (int i = 1; i < playerStats.Count; i++)
        {
            int j = i - 1;
            while (j >= 0 && playerStats[j].Points < playerStats[i].Points)
            {
                playerStats[j + 1] = playerStats[j];
                j = j - 1;
            }
            playerStats[j + 1] = playerStats[i];
        }

        for (int i = 0; i < playerStats.Count; i++)
        {
            for (int j = 0; j < menuScoreBoardList.Count; j++)
            {
                if (playerStats[i].NickName == menuScoreBoardList[j].GetComponent<ScoreListItem>().NickName)
                {
                    menuScoreBoardList[j].transform.SetSiblingIndex(i);
                    break;
                }
            }
        }
        #endregion
    }

    private void OpenScoreMenu()
    {
        ShowScore(playerStats);
        MenuManager.Instance.OpenMenu("EndGame");
    }

    public void ReturnToRoom()
    {
        ConnectToGame();
        Debug.Log("Return to room");
        MenuManager.Instance.OpenMenu("Loading");
        returnToRoom = true;
    }

    public void StartGame()
    {
        RoomManager.Instance.gameTime = gameTime;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel(currentMapInt);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom");
        if (afkKick)
        {
            return;
        }
        else
        {
            if (MenuManager.Instance.activeMenu.menuName != "Menu") MenuManager.Instance.OpenMenu("Loading");
        }
    }

    public void JoinRoom(RoomInfo info)
    {
        if (info.PlayerCount < maxPlayers)
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("name", "Player: " + Random.Range(0, 1000).ToString("0000"));
            PhotonNetwork.JoinRoom(info.Name);
            Debug.Log("JoinRoom");
            MenuManager.Instance.OpenMenu("Loading");
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform t in roomListContent)
        {
            Destroy(t.gameObject);
        }

        for (int i = 0; i <= roomList.Count - 1; i++)
        {
            if (roomList[i].RemovedFromList)
            {
                for (int j = 0; j < roomInfoList.Count; j++)
                {
                    if (roomInfoList[j].Name.Equals(roomList[i])) roomInfoList.RemoveAt(j);
                }
            }

            if (!roomInfoList.Contains(roomList[i])) roomInfoList.Add(roomList[i]);

            for (int j = 0; j < roomInfoList.Count; j++)
            {
                if (roomInfoList[j].Name.Equals(roomList[i].Name)) roomInfoList[j] = roomList[i];
            }
        }

        if (!(roomInfoList.Count == 0))
        {
            for (int i = 0; i < roomInfoList.Count; i++)
            {
                if (roomInfoList[i].RemovedFromList == false)
                {
                    Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomInfoList[i]);
                }
            }
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient) HostButtonsEnabler(true);
        else HostButtonsEnabler(false);
        
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer, PhotonNetwork.IsMasterClient);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient) HostButtonsEnabler(true);
        else HostButtonsEnabler(false);
    }

    public override void OnCreatedRoom()
    {
        PhotonNetwork.CurrentRoom.IsVisible = true;
    }

    private void HostButtonsEnabler(bool enable)
    {
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        foreach (GameObject button in hostButtons)
        {
            if (button == hostButtons[0])
            {
                if (players.Length > 1)
                {
                    button.SetActive(enable);
                }
                else
                {
                    button.SetActive(false);
                }
            }

            else
            {
                button.SetActive(enable);
            }
        }
    }

    public void OnGameDurationSliderValueChanged()
    {
        ResetAFKTimer();
        gameTime = (int)hostButtons[3].GetComponent<Slider>().value;
        timerText.text = $"{gameTime} seconds";
    }

    public void ChangeMapButton(bool next)
    {
        ResetAFKTimer();
        if (next)
        {
            currentMapInt++;
            if (currentMapInt > mapNames.Length) currentMapInt = 1;
        }
        else
        {
            currentMapInt--;
            if (currentMapInt < 1) currentMapInt = mapNames.Length;
        }

        mapName.text = mapNames[currentMapInt - 1];
    }

    public void UpdateGameSettings()
    {
        mapName.text = mapNames[currentMapInt - 1];
        timerText.text = $"{gameTime} seconds";
    }

    public void ResetAFK()
    {
        afkKick = false;
    }
}