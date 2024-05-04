using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;


public class GameManager : NetworkBehaviour
{
    public NetworkVariable<int> currentTurn = new NetworkVariable<int>(0);
    public static GameManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    private async void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            Debug.Log("Client with id " + clientId + " joined");
            if (NetworkManager.Singleton.IsHost &&
            NetworkManager.Singleton.ConnectedClients.Count == 2)
            {
                SpwanBoard();
            }
        };

        

    }

    [SerializeField] private GameObject boardPrefab;
    private GameObject newBoard;
    private void SpwanBoard()
    {
        newBoard = Instantiate(boardPrefab);
        newBoard.GetComponent<NetworkObject>().Spawn();
    }
    public void StartHost()
    {
       
        NetworkManager.Singleton.StartHost();
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TextMeshProUGUI msgText;

    public void ShowMsg(string msg)
    {
        if (msg.Equals("won"))
        {
            msgText.text = "You Won";
            gameEndPanel.SetActive(true);
            
            ShowOpponentMsg("You Lose");
        }
        else if (msg.Equals("draw"))
        {
            msgText.text = "Game Draw";
            gameEndPanel.SetActive(true);
            ShowOpponentMsg("Game Draw");
        }
    }


    private void ShowOpponentMsg(string msg)
    {
        if (IsHost)
        {
            
            OpponentMsgClientRpc(msg);
        }
        else
        {
            
            OpponentMsgServerRpc(msg);
        }
    }

    [ClientRpc]
    private void OpponentMsgClientRpc(string msg)
    {
        if (IsHost) return;
        msgText.text = msg;
        gameEndPanel.SetActive(true);
    }


    [ServerRpc(RequireOwnership = false)]
    private void OpponentMsgServerRpc(string msg)
    {
        msgText.text = msg;
        gameEndPanel.SetActive(true);
    }



    public void Restart()
    {
        

        if (!IsHost)
        {
            RestartServerRpc();
            gameEndPanel.SetActive(false);
        }
        else
        {
            Destroy(newBoard);
            SpwanBoard();
            RestartClientRpc();
        }

        
    }

    [ServerRpc(RequireOwnership = false)]
    private void RestartServerRpc()
    {
        Destroy(newBoard);
        SpwanBoard();
        gameEndPanel.SetActive(false);
    }


    [ClientRpc]
    private void RestartClientRpc()
    {
        gameEndPanel.SetActive(false);
    }
}