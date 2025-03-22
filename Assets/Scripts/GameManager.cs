using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;

    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There can only be one GameManager.");
        }
        Instance = this;
    }

    public void ClickedOnGridPosition(int x, int y)
    {
        Debug.Log("Clicked on " + x + ", " + y);
        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs { 
            x = x, y = y 
        });
    }
}
