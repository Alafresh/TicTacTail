using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrow;
    [SerializeField] private GameObject circleArrow;
    [SerializeField] private GameObject crossYouText;
    [SerializeField] private GameObject circleYouText;

    private void Awake()
    {
        crossArrow.SetActive(false);
        circleArrow.SetActive(false);
        crossYouText.SetActive(false);
        circleYouText.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayerChanged += GameManager_OnCurrentPlayerChanged;
    }

    private void GameManager_OnCurrentPlayerChanged(object sender, System.EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
        {
            crossYouText.SetActive(true);
        }
        else
        {
            circleYouText.SetActive(true);
        }
        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayerType() == GameManager.PlayerType.Cross)
        {
            crossArrow.SetActive(true);
            circleArrow.SetActive(false);
        }
        else
        {
            crossArrow.SetActive(false);
            circleArrow.SetActive(true);
        }
    }
}
