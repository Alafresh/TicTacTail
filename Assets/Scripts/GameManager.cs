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

    public enum PlayerType
    {
        none,
        Cross,
        Circle
    }
        
    private PlayerType localPlayerType;
    private PlayerType currentPlayerType;

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }

        if(IsServer)
        {
            currentPlayerType = PlayerType.Cross;
        }

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
        Debug.Log("Clicked on " + x + ", " + y);

        if(playerType != currentPlayerType)
        {
            return;
        }

        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = GetLocalPlayerType()
        });

        switch(currentPlayerType)
        {
            default:
            case PlayerType.Cross:
                currentPlayerType = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayerType = PlayerType.Cross;
                break;
        }

    }
    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }
}
