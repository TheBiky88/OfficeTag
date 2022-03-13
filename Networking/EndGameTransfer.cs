using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using Photon.Realtime;

public class EndGameTransfer : MonoBehaviourPunCallbacks
{
    private static EndGameTransfer instance;
    public static EndGameTransfer Instance { get { return instance; } }

    public List<Game.PlayerStats> playerStats;
    public string roomName;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            Launcher.Instance.playerStats = playerStats;
            Launcher.Instance.roomName = roomName;
            Destroy(gameObject);
        }
    }
}