using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportController : MonoBehaviour
{
    [Header("Input")]
    public InputAction teleportAction; 

    [Header("References")]
    public GameObject ghostPrefab;    
    public GameObject pearlPrefab;     
    public Transform player;          

    [Header("Settings")]
    public float maxRange = 5f;
    public float cooldown = 1.5f;
    public float pearlSpeed = 10f;

    private GameObject currentGhost;
    private float lastTeleportTime = -999f;
    private bool isHolding = false;

    private Vector3 targetPosition;

    void OnEnable()
    {
        teleportAction.Enable();
    }

    void OnDisable()
    {
        teleportAction.Disable();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        float input = teleportAction.ReadValue<float>();

        // HOLD
        if (input > 0.5f && Time.time >= lastTeleportTime + cooldown)
        {
            if (!isHolding)
            {
                StartAim();
            }

            UpdateGhostPosition();
        }
        // RELEASE
        else if (isHolding)
        {
            ReleaseTeleport();
        }
    }

    void StartAim()
    {
        isHolding = true;

        if (ghostPrefab != null)
        {
            currentGhost = Instantiate(ghostPrefab);
        }
    }

    void UpdateGhostPosition()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0;

        Vector3 dir = mouseWorld - player.position;
        float distance = dir.magnitude;

        dir.Normalize();

        float clampedDistance = Mathf.Min(distance, maxRange);

        targetPosition = player.position + dir * clampedDistance;

        if (currentGhost != null)
        {
            currentGhost.transform.position = targetPosition;
        }
    }

    void ReleaseTeleport()
    {
        isHolding = false;

        if (currentGhost != null)
        {
            Destroy(currentGhost);
        }

        SpawnPearl();
        lastTeleportTime = Time.time;
    }

    void SpawnPearl()
    {
        if (pearlPrefab == null) return;

        GameObject pearl = Instantiate(pearlPrefab, player.position, Quaternion.identity);

        StartCoroutine(MovePearl(pearl));
    }

    System.Collections.IEnumerator MovePearl(GameObject pearl)
    {
        while (pearl != null && Vector3.Distance(pearl.transform.position, targetPosition) > 0.1f)
        {
            pearl.transform.position = Vector3.MoveTowards(
                pearl.transform.position,
                targetPosition,
                pearlSpeed * Time.deltaTime
            );

            yield return null;
        }

        // TELEPORT
        player.position = targetPosition;

        if (pearl != null)
        {
            Destroy(pearl);
        }
    }
}