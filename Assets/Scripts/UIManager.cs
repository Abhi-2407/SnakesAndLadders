using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject HomeScreen;
    public GameObject GameOverScreen;
    public GameObject TryAgainScreen;
    public GameObject ModeScreen;
    public GameObject EquationScreen;

    public GameObject HeaderBar;

    public GamePlay gamePlay;
    public GameTimer gameTimer;

    public GameObject Scanner;

    void Start()
    {
        HomeScreen.SetActive(true);
    }

    public void StartGame()
    {
        HomeScreen.SetActive(false);
        ModeScreen.SetActive(true);
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

    public void TestMode()
    {
        ModeScreen.SetActive(false);
     
    }

    public void PlayGame()
    {
        HeaderBar.SetActive(true);
        gamePlay.gameState = GameState.START;
        gameTimer.RestartTimer();
        EquationScreen.SetActive(false);

        Restart();
    }

    public void PracticeMode()
    {
        ModeScreen.SetActive(false);
        HeaderBar.SetActive(false);
        EquationScreen.SetActive(true);
    }

    public void OpenEquastionScreen()
    {
        EquationScreen.SetActive(true);
    }

    public void ScanCode()
    {
        Scanner.SetActive(true);
    }

}
