using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timeLimit = 300f; // 300 seconds (5 minutes)
    public bool autoStart = false;
    
    [Header("UI References - Separate Digit Text Boxes")]
    public TextMeshProUGUI firstDigitText;    // For the first digit (3, 2, 1, 0)
    public TextMeshProUGUI secondDigitText;   // For the second digit (0-9)
    public TextMeshProUGUI thirdDigitText;    // For the third digit (0-9)
    
    [Header("Optional UI Elements")]
    public TextMeshProUGUI timerText;         // Full time display (optional)
    public Slider timerSlider;                // Progress bar (optional)
    public Image timerFillImage;              // Fill image for progress (optional)
    
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;
    public float warningThreshold = 0.3f;     // 30% of time remaining
    public float criticalThreshold = 0.1f;    // 10% of time remaining
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent onTimerComplete;
    public UnityEngine.Events.UnityEvent onTimerStart;
    public UnityEngine.Events.UnityEvent onTimerPause;
    public UnityEngine.Events.UnityEvent onTimerResume;
    
    // Private variables
    private float currentTime;
    public bool isRunning = false;
    private bool isPaused = false;
    private Coroutine timerCoroutine;
    
    // Properties
    public float CurrentTime => currentTime;
    public float TimeRemaining => currentTime;
    public float TimeElapsed => timeLimit - currentTime;
    public bool IsRunning => isRunning;
    public bool IsPaused => isPaused;
    public float Progress => 1f - (currentTime / timeLimit);
    
    void Start()
    {
        InitializeTimer();

        if (autoStart)
        {
            StartTimer();
        }
    }

    public void RestartTimer()
    {
        InitializeTimer();

        //if (autoStart)
        {
            StartTimer();
        }
    }
    
    void InitializeTimer()
    {
        currentTime = timeLimit;
        UpdateUI();
        UpdateVisuals();
    }
    
    public void StartTimer()
    {
        currentTime = timeLimit;
        if (!isRunning)
        {
            isRunning = true;
            isPaused = false;
            timerCoroutine = StartCoroutine(TimerCoroutine());
            onTimerStart?.Invoke();
        }
    }
    
    public void PauseTimer()
    {
        if (isRunning && !isPaused)
        {
            isPaused = true;
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
            }
            onTimerPause?.Invoke();
        }
    }
    
    public void ResumeTimer()
    {
        if (isRunning && isPaused)
        {
            isPaused = false;
            timerCoroutine = StartCoroutine(TimerCoroutine());
            onTimerResume?.Invoke();
        }
    }
    
    public void StopTimer()
    {
        isRunning = false;
        isPaused = false;
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        InitializeTimer();
    }
    
    public void ResetTimer()
    {
        StopTimer();
        InitializeTimer();
    }
    
    public void SetTimeLimit(float newTimeLimit)
    {
        timeLimit = newTimeLimit;
        InitializeTimer();
    }
    
    private IEnumerator TimerCoroutine()
    {
        while (isRunning && !isPaused)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                TimerComplete();
                break;
            }
            
            UpdateUI();
            UpdateVisuals();
            yield return null;
        }
    }
    
    private void TimerComplete()
    {
        isRunning = false;
        onTimerComplete?.Invoke();
        UpdateUI();
        UpdateVisuals();
    }
    
    private void UpdateUI()
    {
        // Update separate digit text boxes
        UpdateDigitDisplay();
        
        
        
        // Update optional progress bar
        if (timerSlider != null)
        {
            timerSlider.value = Progress;
        }
    }
    
    private void UpdateDigitDisplay()
    {
        int totalSeconds = Mathf.CeilToInt(currentTime);
        
        // Calculate individual digits from total seconds
        int firstDigit = totalSeconds / 100;      // First digit (3, 2, 1, 0)
        int secondDigit = (totalSeconds / 10) % 10;  // Second digit (0-9)
        int thirdDigit = totalSeconds % 10;       // Third digit (0-9)
        
        // Update first digit text (hundreds)
        if (firstDigitText != null)
        {
            firstDigitText.text = firstDigit.ToString();
        }
        
        // Update second digit text (tens)
        if (secondDigitText != null)
        {
            secondDigitText.text = secondDigit.ToString();
        }
        
        // Update third digit text (ones)
        if (thirdDigitText != null)
        {
            thirdDigitText.text = thirdDigit.ToString();
        }

        // Update optional full timer text
        if (timerText != null)
        {
            timerText.text = totalSeconds.ToString();
            //timerText.text = FormatTime(currentTime);
        }
    }
    
    private void UpdateVisuals()
    {
        if (timerFillImage != null)
        {
            float progress = Progress;

            timerFillImage.fillAmount = 1 - progress;

            if (progress >= 1f - criticalThreshold)
            {
                timerFillImage.color = criticalColor;
            }
            else if (progress >= 1f - warningThreshold)
            {
                timerFillImage.color = warningColor;
            }
            else
            {
                timerFillImage.color = normalColor;
            }
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        int totalSeconds = Mathf.CeilToInt(timeInSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        
        return string.Format("{0:0}:{1:00}", minutes, seconds);
    }
    
    // Public methods for external control
    public void AddTime(float additionalTime)
    {
        currentTime = Mathf.Min(currentTime + additionalTime, timeLimit);
        UpdateUI();
        UpdateVisuals();
    }
    
    public void SubtractTime(float timeToSubtract)
    {
        currentTime = Mathf.Max(currentTime - timeToSubtract, 0f);
        UpdateUI();
        UpdateVisuals();
    }
    
    // Get current time as separate digits for external use
    public (int first, int second, int third) GetCurrentDigits()
    {
        int totalSeconds = Mathf.CeilToInt(currentTime);
        
        int firstDigit = totalSeconds / 100;      // First digit (3, 2, 1, 0)
        int secondDigit = (totalSeconds / 10) % 10;  // Second digit (0-9)
        int thirdDigit = totalSeconds % 10;       // Third digit (0-9)
        
        return (firstDigit, secondDigit, thirdDigit);
    }
    
    // Debug methods
    [ContextMenu("Start Timer")]
    private void DebugStartTimer()
    {
        StartTimer();
    }
    
    [ContextMenu("Stop Timer")]
    private void DebugStopTimer()
    {
        StopTimer();
    }
    
    [ContextMenu("Reset Timer")]
    private void DebugResetTimer()
    {
        ResetTimer();
    }
}

