using Unity.Netcode;
using UnityEngine;

public class NetworkingMenu : MonoBehaviour
{
    public GameObject uiContainer;

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        uiContainer.SetActive(false);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        uiContainer.SetActive(false);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        uiContainer.SetActive(false);
    }

}
