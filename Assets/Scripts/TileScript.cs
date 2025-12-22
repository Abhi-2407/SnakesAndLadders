using System.Data;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public int value;
    public GamePlay gamePlay;

    void Start()
    {
        value = transform.GetSiblingIndex() + 1;
    }

    private void OnMouseDown()
    {
        if (gamePlay.gameState == GameState.START)
        {
            gamePlay.HandleDiceRoll(value);
        }
    }
}
