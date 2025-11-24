using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject HomeScreen;
    public GameObject GameOverScreen;
    public GameObject TryAgainScreen;

    public GamePlay gamePlay;
    public GameTimer gameTimer;

    void Start()
    {

    }

    public void StartGame()
    {
        gamePlay.gameState = GameState.START;
        HomeScreen.SetActive(false);
        gameTimer.RestartTimer();

    }

    public void Home()
    {
        HomeScreen.SetActive(true);
        GameOverScreen.SetActive(false);
        TryAgainScreen.SetActive(false);
    }

    public void Restart()
    {
        gamePlay. SetUp();
        gamePlay.gameState = GameState.START;
        HomeScreen.SetActive(false);
        GameOverScreen.SetActive(false);
        TryAgainScreen.SetActive(false);
        gameTimer.RestartTimer();
    }
}
