using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

[RequireComponent(typeof(SpriteRenderer))]
public class DiceController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer faceRenderer;
    [SerializeField, Tooltip("Sprites matching the numbers 1-6 (or however many sides you show).")] private Sprite[] faceSprites;
    [SerializeField, Tooltip("Sprites matching the numbers 1-6 (or however many sides you show).")] private Sprite[] faceSpritesDummy;
    [SerializeField, Tooltip("How long the dice continues to rotate before landing.")] private float rollDuration = 0.8f;
    [SerializeField, Tooltip("Speed at which the face rotates during the roll animation.")] private float spinSpeed = 720f;
    //[SerializeField] private UnityEvent<int> onRollComplete;

    public bool IsRolling { get; private set; }
    
    public GamePlay gamePlay;

    private void Reset()
    {
        faceRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        if (gamePlay.gameState == GameState.START)
        {
            //Roll();
        }
    }

    public void Roll(Action<int> callback = null)
    {
        if (IsRolling || faceSprites == null || faceSprites.Length == 0)
        {
            return;
        }

        StartCoroutine(RollCoroutine(callback));
    }

    private IEnumerator RollCoroutine(Action<int> callback)
    {
        IsRolling = true;
        float elapsed = 0f;
        int finalFaceIndex = gamePlay.equations[gamePlay.counter].number2;

        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;

            faceRenderer.transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);
            faceRenderer.sprite = faceSpritesDummy[UnityEngine.Random.Range(0, faceSpritesDummy.Length)];

            yield return null;
        }

        faceRenderer.transform.rotation = Quaternion.identity;
        faceRenderer.sprite = faceSprites[finalFaceIndex - 1];

        gamePlay.diceCount = finalFaceIndex;
        IsRolling = false;

        gamePlay.DesplayEquation();

        gamePlay.Scanner.SetActive(true);
    }

    public void SetFaceSprites(Sprite[] sprites)
    {
        faceSprites = sprites;
    }

    public void ResetDice()
    {
        faceRenderer.sprite = faceSpritesDummy[UnityEngine.Random.Range(0, faceSpritesDummy.Length)];
    }
}

