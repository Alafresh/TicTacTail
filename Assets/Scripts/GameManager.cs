using System;
using UnityEngine;
using Unity.Netcode;
using NUnit.Framework.Internal;
using System.Collections.Generic;

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
    public event EventHandler<OnGameWinEventArgs> OnGameWin;

    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
    }

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB,
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }
        
    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayerType = new NetworkVariable<PlayerType>();
    private PlayerType[,] playerTypesArray;
    private List<Line> lineList;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There can only be one GameManager.");
        }
        Instance = this;
        
        playerTypesArray = new PlayerType[3, 3];

        lineList = new List<Line>()
        {
            // Horitzontal
            new Line
            {
                gridVector2IntList = new List<Vector2Int> { 
                    new Vector2Int(0, 0), 
                    new Vector2Int(1, 0), 
                    new Vector2Int(2, 0) 
                }, 
                centerGridPosition = new Vector2Int(1, 0),
                orientation = Orientation.Horizontal,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 1)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Horizontal,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 2),
                    new Vector2Int(2, 2)
                },
                centerGridPosition = new Vector2Int(1, 2),
                orientation = Orientation.Horizontal,
            },
            // Vertical
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 2)
                },
                centerGridPosition = new Vector2Int(0, 1),
                orientation = Orientation.Vertical,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {
                    new Vector2Int(1, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(1, 2)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Vertical,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {
                    new Vector2Int(2, 0),
                    new Vector2Int(2, 1),
                    new Vector2Int(2, 2)
                },
                centerGridPosition = new Vector2Int(2, 1),
                orientation = Orientation.Vertical,
            },
            // Diagonal
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 2)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalA,
            },new Line
            {
                gridVector2IntList = new List<Vector2Int> {
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 0)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalB,
            },
        };
    }

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


    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if(playerType != currentPlayerType.Value)
        {
            return;
        }

        if (playerTypesArray[x, y] != PlayerType.None)
        {
            return;
        }

        playerTypesArray[x, y] = playerType;

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
        TestWinner();
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            playerTypesArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypesArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypesArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
            );
    }

    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        return aPlayerType != PlayerType.None &&
            aPlayerType == bPlayerType && 
            bPlayerType == cPlayerType;
    }

    private void TestWinner()
    {
        foreach(Line line in lineList)
        {
            if(TestWinnerLine(line))
            {
                Debug.Log("Winner");
                currentPlayerType.Value = PlayerType.None;
                OnGameWin?.Invoke(this, new OnGameWinEventArgs
                {
                    line = line
                });
                break;
            }
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
