using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum GameState
{
    DEFAULT,
    START,
    OVER
}

public class GamePlay : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private Transform playerCharacter; // Reference to the 2D character transform
    [SerializeField] private float jumpHeight = 1f; // Height of the jump arc
    [SerializeField] private float jumpDuration = 0.5f; // Time to complete one tile jump
    [SerializeField] private ParticleSystem jumpEffect;

    [Header("Board Settings")]
    [SerializeField] private string tileNamePrefix = "Square"; // Prefix for tile names (e.g., "Square (1)", "Square (2)")
    [SerializeField] private int totalTiles = 100; // Total number of tiles on the board

    [Header("UI")]
    public TextMeshProUGUI result;
    public TextMeshProUGUI scoreTxt;

    // Game state
    public int diceCount;
    public int totalResult;
    public int currentPosition = 0; // Current tile position (0-99, where 0 is start)
    private bool isMoving = false; // Flag to prevent multiple moves at once
    public TextMeshProUGUI ScanText;
    public ParticleSystem[] EmojiScare;
    public GameObject winPrompt;
    public GameObject gameOverPrompt;

    [Header("Effects")]
    [SerializeField] private ParticleSystem confetti;
    [SerializeField] private ParticleSystem starEffect;
    public TextMeshProUGUI scoreTxtGO;
    public TextMeshProUGUI scoreTxtGO2;

    // Store all tile positions
    public Transform[] tiles;

    public int correctAnswer;
    public int wrongAnswer;

    public int score;
    public int life = 3;
    public GameObject[] lifeObj;

    public TextMeshProUGUI ScoreSummery;

    public GameTimer gameTimer;
    public DiceController dice;

    public GameObject Scanner;

    public GameState gameState = GameState.DEFAULT;

    public void SetUp()
    {
        totalResult = 1;
        currentPosition = 0;
        score = 0;
        life = 3;
        //scoreTxt.text = score.ToString();
        for (int i = 0; i < 3; i++)
        {
            lifeObj[i].SetActive(true);
        }

        playerCharacter.position = new Vector3(-6.35f, -4f, 2);
    }

    private void Start()
    {
        //InitializeTiles();

        // If player character is not set, try to find it
        if (playerCharacter == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerCharacter = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Player character not found! Please assign it in the inspector or tag it as 'Player'.");
            }
        }

        // Move player to starting position (tile 0 or 1)
        if (playerCharacter != null && tiles != null && tiles.Length > 0)
        {
            MovePlayerToTile(0, false); // Start at position 0 without animation
        }
    }

    /// <summary>
    /// Finds and stores all tile positions from the scene
    /// </summary>
    private void InitializeTiles()
    {
        tiles = new Transform[totalTiles];

        // Find all tiles by name pattern
        for (int i = 0; i < totalTiles; i++)
        {
            string tileName = $"{tileNamePrefix} ({i})";
            GameObject tileObj = GameObject.Find(tileName);

            if (tileObj != null)
            {
                tiles[i] = tileObj.transform;
            }
            else
            {
                Debug.LogWarning($"Tile {tileName} not found!");
            }
        }

        // Alternative: Find all tiles by searching for objects with "Square" in name
        if (tiles[0] == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            List<Transform> foundTiles = new List<Transform>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains(tileNamePrefix))
                {
                    // Try to extract number from name
                    string name = obj.name;
                    int startIndex = name.IndexOf('(') + 1;
                    int endIndex = name.IndexOf(')');

                    if (startIndex > 0 && endIndex > startIndex)
                    {
                        string numberStr = name.Substring(startIndex, endIndex - startIndex);
                        if (int.TryParse(numberStr, out int tileNumber))
                        {
                            if (tileNumber >= 1 && tileNumber <= totalTiles)
                            {
                                foundTiles.Add(obj.transform);
                            }
                        }
                    }
                }
            }

            // Sort tiles by their number
            foundTiles.Sort((a, b) =>
            {
                int numA = ExtractTileNumber(a.name);
                int numB = ExtractTileNumber(b.name);
                return numA.CompareTo(numB);
            });

            if (foundTiles.Count == totalTiles)
            {
                tiles = foundTiles.ToArray();
                Debug.Log($"Found {foundTiles.Count} tiles automatically!");
            }
            else
            {
                Debug.LogWarning($"Found {foundTiles.Count} tiles, expected {totalTiles}. Some tiles may be missing.");
            }
        }
    }

    /// <summary>
    /// Extracts tile number from GameObject name
    /// </summary>
    private int ExtractTileNumber(string name)
    {
        int startIndex = name.IndexOf('(') + 1;
        int endIndex = name.IndexOf(')');

        if (startIndex > 0 && endIndex > startIndex)
        {
            string numberStr = name.Substring(startIndex, endIndex - startIndex);
            if (int.TryParse(numberStr, out int tileNumber))
            {
                return tileNumber;
            }
        }
        return 0;
    }

    /// <summary>
    /// Adds two numbers (defaults the second addend to 1 so callers can provide a single value).
    /// </summary>
    public int AddNumbers(int firstNumber, int secondNumber = 1)
    {
        return firstNumber + secondNumber;
    }

    public void DesplayEquation()
    {
        string txt = totalResult + "+" + diceCount + "=?";

        StartCoroutine(WriteOnebyOneText(txt));
    }

    public IEnumerator WriteOnebyOneText(string text)
    {
        char[] chars = text.ToCharArray();
        string txt = "";
        for (int i = 0; i < chars.Length; i++)
        {
            txt = txt + chars[i].ToString();
            result.text = txt;

            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// Hook this up to DiceController's onRollComplete to accumulate the rolled value and move the character.
    /// </summary>
    public void HandleDiceRoll(int tileValue)
    {
        if (tileValue == (totalResult + diceCount))
        {
            int newTotal = totalResult + diceCount;
            result.text = totalResult + "+" + diceCount + "=" + newTotal;
            starEffect.Play();
            //StartCoroutine(WriteOnebyOneText(txt));

            // Calculate target position (1-based, clamped to board limits)
            int targetPosition1Based = Mathf.Clamp(newTotal, 1, totalTiles);
            totalResult = targetPosition1Based;

            // Calculate number of tiles to move from current position
            int currentPos1Based = GetCurrentPosition(); // Get current position (1-based)
            int tilesToMove = targetPosition1Based - currentPos1Based;

            // Move character tile by tile
            if (tilesToMove > 0)
            {
                MovePlayerByTiles(tilesToMove);
            }
            else if (tilesToMove < 0)
            {
                // Handle backward movement if needed (for snakes/ladders logic)
                Debug.Log($"Player would move backward by {Mathf.Abs(tilesToMove)} tiles. Not implemented yet.");
            }

            correctAnswer++;
        }
        else
        {
            wrongAnswer++;

            int i = Random.Range(0, EmojiScare.Length);
            EmojiScare[i].Play();

            Invoke(nameof(ResetScanner), 3.0f);
        }
    }

    void ResetScanner()
    {
        ScanText.text = "";

        LifeHandle();

        if (life > 0)
            Scanner.SetActive(true);
    }

    void LifeHandle()
    {
        life--;

        for (int i = 0; i < (3 - life); i++)
        {
            lifeObj[i].SetActive(false);
        }

        if(life <= 0)
        {
            GameLoss();
        }
    }

    public void SetResult()
    {
        int TotalAnswer = correctAnswer + wrongAnswer;
        score = (correctAnswer * 100) / TotalAnswer;

        ScoreSummery.text = "----------------- GAME SCORE SUMMERY ----------------- \n" +
            "Score : " + score + "%\n" +
            "Active Time : " + (gameTimer.timeLimit - gameTimer.CurrentTime) + "s\n" +
            "Idle Time : " + "0s\n\n" +
            "Total Responses : " + TotalAnswer + "\n" +
            "Correct Answers : " + correctAnswer + "\n" +
            "Wrong Answers : " + wrongAnswer;
    }

    public void GameOver()
    {
        gameState = GameState.OVER;
        ScanText.text = "";
        SetResult();
        winPrompt.SetActive(true);
        scoreTxtGO.text = score.ToString();
        confetti.Play();
        gameTimer.StopTimer();
    }

    public void GameLoss()
    {
        gameState = GameState.OVER;
        ScanText.text = "";
        SetResult();
        gameOverPrompt.SetActive(true);
        scoreTxtGO2.text = score.ToString();
        gameTimer.StopTimer();
    }

    /// <summary>
    /// Moves the player character to a specific tile position
    /// </summary>
    /// <param name="tileIndex">Target tile index (0-based)</param>
    /// <param name="animate">Whether to animate the movement with jumping</param>
    public void MovePlayerToTile(int tileIndex, bool animate = true)
    {
        if (isMoving)
        {
            Debug.LogWarning("Player is already moving! Wait for current movement to complete.");
            return;
        }

        if (tileIndex < 0 || tileIndex >= tiles.Length)
        {
            Debug.LogWarning($"Invalid tile index: {tileIndex}. Must be between 0 and {tiles.Length - 1}");
            return;
        }

        if (tiles[tileIndex] == null)
        {
            Debug.LogWarning($"Tile at index {tileIndex} is null!");
            return;
        }

        if (playerCharacter == null)
        {
            Debug.LogWarning("Player character is not assigned!");
            return;
        }

        currentPosition = tileIndex;

        if (animate)
        {
            StartCoroutine(MovePlayerCoroutine(tileIndex));
        }
        else
        {
            // Move instantly without animation
            Vector3 targetPos = tiles[tileIndex].position;
            playerCharacter.position = new Vector3(targetPos.x, targetPos.y, playerCharacter.position.z);
        }
    }

    /// <summary>
    /// Moves the player character sequentially through multiple tiles one by one
    /// </summary>
    /// <param name="targetTileIndex">Final target tile index (0-based)</param>
    private IEnumerator MovePlayerSequentiallyCoroutine(int targetTileIndex)
    {
        isMoving = true;

        int startPosition = currentPosition;
        int endPosition = Mathf.Clamp(targetTileIndex, 0, tiles.Length - 1);

        // Move one tile at a time from current position to target
        for (int i = startPosition + 1; i <= endPosition; i++)
        {
            if (i >= tiles.Length || tiles[i] == null)
            {
                Debug.LogWarning($"Cannot move to tile {i + 1} - tile is null or out of bounds!");
                break;
            }

            // Move to next tile (using single tile movement that doesn't manage isMoving flag)
            yield return StartCoroutine(MoveToSingleTileCoroutine(i));
            currentPosition = i;

            // Small delay between jumps (optional, for better visibility)
            yield return new WaitForSeconds(0.1f);
        }

        isMoving = false;

        FinishMove();


        Debug.Log($"Player finished moving to tile {currentPosition + 1}");
    }

    public void FinishMove()
    {
        ScanText.text = "";
        result.text = "";

        dice.ResetDice();

        Debug.Log($"FinishMove");
    }

    /// <summary>
    /// Coroutine that handles smooth jumping movement between tiles
    /// </summary>
    private IEnumerator MovePlayerCoroutine(int targetTileIndex)
    {
        isMoving = true;

        Vector3 startPos = playerCharacter.position;
        Vector3 targetPos = tiles[targetTileIndex].position;
        targetPos.z = startPos.z; // Preserve Z position

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            // Smooth curve using ease-in-out
            t = Mathf.SmoothStep(0f, 1f, t);

            // Calculate position with jump arc
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            // Add jump arc (parabolic curve)
            float jumpArc = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            currentPos.y += jumpArc;

            playerCharacter.position = currentPos;

            //jumpEffect.Play();

            yield return null;
        }

        // Ensure we end exactly at the target position
        playerCharacter.position = targetPos;
        ScanText.text = "";
        Debug.Log($"Player moved to tile {targetTileIndex + 1}");
        isMoving = false;
    }

    /// <summary>
    /// Coroutine that handles smooth jumping movement to a single tile (used in sequential movement)
    /// </summary>
    private IEnumerator MoveToSingleTileCoroutine(int targetTileIndex)
    {
        Vector3 startPos = playerCharacter.position;
        Vector3 targetPos = tiles[targetTileIndex].position;
        targetPos.z = startPos.z; // Preserve Z position

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            // Smooth curve using ease-in-out
            t = Mathf.SmoothStep(0f, 1f, t);

            // Calculate position with jump arc
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            // Add jump arc (parabolic curve)
            float jumpArc = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            currentPos.y += jumpArc;

            playerCharacter.position = currentPos;

            yield return null;
        }

        jumpEffect.Play();

        // Ensure we end exactly at the target position
        playerCharacter.position = targetPos;

        if (targetTileIndex >= 99)
        {
            GameOver();
        }
    }

    /// <summary>
    /// Moves player by a specific number of tiles forward, one tile at a time
    /// </summary>
    /// <param name="numberOfTiles">Number of tiles to move forward</param>
    public void MovePlayerByTiles(int numberOfTiles)
    {
        if (isMoving)
        {
            Debug.LogWarning("Player is already moving! Wait for current movement to complete.");
            return;
        }

        if (numberOfTiles <= 0)
        {
            Debug.LogWarning($"Invalid number of tiles to move: {numberOfTiles}");
            return;
        }

        int targetPosition = Mathf.Clamp(currentPosition + numberOfTiles, 0, totalTiles - 1);

        // Move sequentially tile by tile
        StartCoroutine(MovePlayerSequentiallyCoroutine(targetPosition));
    }

    /// <summary>
    /// Gets the current tile position (1-based)
    /// </summary>
    public int GetCurrentPosition()
    {
        return currentPosition + 1; // Return 1-based position
    }

    /// <summary>
    /// Checks if the player is currently moving
    /// </summary>
    public bool IsMoving()
    {
        return isMoving;
    }
}