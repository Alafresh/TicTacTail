using System;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;

    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayerChanged;

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }
        
    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayerType = new NetworkVariable<PlayerType>();

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn: " + NetworkManager.Singleton.LocalClientId);
        if (NetworkManager.Singleton.LocalClientId == 0) {
            localPlayerType = PlayerType.Cross;
        }
        else {
            localPlayerType = PlayerType.Circle;
        }
        
        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentPlayerType.OnValueChanged += (PlayerType previousValue, PlayerType newValue) =>
        {
            OnCurrentPlayerChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentPlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There can only be one GameManager.");
        }
        Instance = this;
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if(playerType != currentPlayerType.Value)
        {
            return;
        }

        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = playerType,
        });
        switch (currentPlayerType.Value)
        {
            default:
            case PlayerType.Cross:
                currentPlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayerType.Value = PlayerType.Cross;
                break;
        }
    }



    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }
    public PlayerType GetCurrentPlayerType()
    {
        return currentPlayerType.Value;
    }
}
