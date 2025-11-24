using System.Collections;
using System.Net;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SnakeScript : MonoBehaviour
{
    [Header("Waypoint Settings")]
    [SerializeField] private Transform[] waypoints; // Array of waypoints to move through
    [SerializeField] private float moveSpeed = 2f; // Speed of movement along the path
    [SerializeField] private float waitTimeAtEnd = 0.5f; // Time to wait at the final waypoint
    
    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player"; // Tag of the player object
    [SerializeField] private LayerMask playerLayer = -1; // Layer mask for player detection
    
    [Header("References")]
    [SerializeField] private GamePlay gamePlay; // Reference to GamePlay script to check if player is moving
    
    private bool isMovingPlayer = false; // Flag to prevent multiple simultaneous movements
    private Transform playerTransform; // Cached player transform

    public int endPoint;
    
    private void Start()
    {
        // Ensure collider is set as trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("SnakeScript requires a Collider2D component!");
        }
        
        // Find GamePlay if not assigned
        if (gamePlay == null)
        {
            gamePlay = FindObjectOfType<GamePlay>();
            if (gamePlay == null)
            {
                Debug.LogWarning("GamePlay script not found! Player movement check may not work correctly.");
            }
        }
        
        // Validate waypoints
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning($"SnakeScript on {gameObject.name} needs at least 2 waypoints to function properly!");
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (!IsPlayer(other))
        {
            return;
        }
        
        // Cache player transform
        if (playerTransform == null)
        {
            playerTransform = other.transform;
        }
        
        // Check if player is not moving and we're not already moving the player
        if (!IsPlayerMoving() && !isMovingPlayer)
        {
            // Start moving player through waypoints
            StartCoroutine(MovePlayerThroughWaypoints());
        }
    }
    
    /// <summary>
    /// Checks if the colliding object is the player
    /// </summary>
    private bool IsPlayer(Collider2D other)
    {
        // Check by tag
        if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag))
        {
            return true;
        }
        
        // Check by layer
        if (playerLayer != -1 && ((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if the player is currently moving (via GamePlay script)
    /// </summary>
    private bool IsPlayerMoving()
    {
        if (gamePlay != null)
        {
            return gamePlay.IsMoving();
        }
        
        // If GamePlay is not available, assume player is not moving
        return false;
    }
    
    /// <summary>
    /// Coroutine that smoothly moves the player through all waypoints
    /// </summary>
    private IEnumerator MovePlayerThroughWaypoints()
    {
        if (waypoints == null || waypoints.Length < 2 || playerTransform == null)
        {
            yield break;
        }
        
        isMovingPlayer = true;
        
        // Move through each waypoint
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                Debug.LogWarning($"Waypoint {i} is null! Skipping...");
                continue;
            }
            
            Vector3 targetPosition = waypoints[i].position;
            targetPosition.z = playerTransform.position.z; // Preserve Z position
            
            // Move to this waypoint
            yield return StartCoroutine(MoveToPosition(playerTransform, targetPosition));
        }
        
        // Wait at the final waypoint
        if (waitTimeAtEnd > 0)
        {
            yield return new WaitForSeconds(waitTimeAtEnd);
        }
        
        isMovingPlayer = false;

        gamePlay.totalResult = endPoint;
        gamePlay.currentPosition = endPoint - 1;


        Debug.Log($"Player finished moving through {waypoints.Length} waypoints on {gameObject.name}");
    }
    
    /// <summary>
    /// Smoothly moves a transform to a target position
    /// </summary>
    private IEnumerator MoveToPosition(Transform targetTransform, Vector3 targetPosition)
    {
        Vector3 startPosition = targetTransform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float duration = distance / moveSpeed;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Smooth movement using ease-in-out
            t = Mathf.SmoothStep(0f, 1f, t);
            
            // Interpolate position
            targetTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            
            yield return null;
        }
        
        // Ensure we end exactly at the target position
        targetTransform.position = targetPosition;
    }
    
    /// <summary>
    /// Draws waypoints in the editor for visualization
    /// </summary>
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            return;
        }
        
        // Draw waypoints
        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);
            }
        }
        
        // Draw lines between waypoints
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
    
    /// <summary>
    /// Draws the trigger area in the editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            
            if (col is BoxCollider2D boxCol)
            {
                Gizmos.DrawCube(transform.position + (Vector3)boxCol.offset, boxCol.size);
            }
            else if (col is CircleCollider2D circleCol)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
            }
            else if (col is CapsuleCollider2D capsuleCol)
            {
                Gizmos.DrawCube(transform.position + (Vector3)capsuleCol.offset, capsuleCol.size);
            }
        }
    }
}

