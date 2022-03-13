using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private static MenuManager instance;
    public static MenuManager Instance { get { return instance; } }

    public Menu activeMenu;
    public ProfanityFilter.ProfanityFilter ProfanityFilter;


    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else instance = this;

        ProfanityFilter = new ProfanityFilter.ProfanityFilter();
    }

    [SerializeField] private Menu[] menus;

    public void OpenMenu(string menuName)
    {
        Launcher.Instance.ResetAFKTimer();
        foreach (Menu menu in menus)
        {
            if (menu.menuName == menuName)
            {
                menu.Open();
                activeMenu = menu;
            }

            else if (menu.open)
            {
                CloseMenu(menu);
            }
        }
    }

    public void OpenMenu(Menu menu)
    {
        if (menu.menuName == "Menu")
        {
            if (PhotonNetwork.IsConnected) Launcher.Instance.Disconnect();
            OpenMenu(menu.menuName);
        }

        else if (menu.menuName == "LobbyFinder" && !PhotonNetwork.IsConnected)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Launcher.Instance.ThrowError("No internet connection available.\nCheck your internet connection!");
            }

            else
            {
                Launcher.Instance.ConnectToGame();
            }
        }

        else
        {
            OpenMenu(menu.menuName);
        }
    }

    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
