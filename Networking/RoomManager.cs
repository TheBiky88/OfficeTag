using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private static RoomManager instance;
    public static RoomManager Instance { get { return instance; } }

    public List<Game.Player> players = new List<Game.Player>();
    public List<Color> playerColors = new List<Color>();

    public int gameTime;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
    }

    public override void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 1)
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Launcher.Instance.currentMapInt);
            stream.SendNext(Launcher.Instance.gameTime);
        }

        else
        {
            Launcher.Instance.currentMapInt = (int)stream.ReceiveNext();
            Launcher.Instance.gameTime = (int)stream.ReceiveNext();
            Launcher.Instance.UpdateGameSettings();
        }
    }
}