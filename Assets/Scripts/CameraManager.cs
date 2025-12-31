using UnityEngine;
using Cinemachine;
using System; // Required for EventHandler
#if UNITY_EDITOR
using UnityEditor; // Required for Editor quit functionality
#endif

public class CameraManager : MonoBehaviour
{
    public event EventHandler OnEndGame; // Custom event for game end

    public CinemachineVirtualCamera thirdPersonVCam; // Assign in Inspector
    public CinemachineVirtualCamera povVCam;         // Assign in Inspector (child of cockpit)
    public GameObject spaceship;                    // Assign spaceship root GameObject in Inspector
    public GameObject cockpit;                      // Assign cockpit root GameObject in Inspector (child of spaceship)
    public Transform player;                        // Assign the player GameObject's Transform (optional, for repositioning)
    public GameObject actorPrefab;                 // Assign a prefab for the new actor (if used)
    public AudioClip cutsceneSound;                // Assign a sound effect (if used)
    public ParticleSystem particleEffectPrefab;    // Assign a particle effect prefab (if used)
    public GameObject endScreen;                   // Assign a UI canvas or black screen (optional)

    private bool isThirdPersonActive = true;
    private float moveSpeed = 5.0f; // Default forward speed
    private float maxSpeed = 15.0f; // Top speed limit
    private float accelerationIncrement = 2.0f; // Acceleration increment
    private float currentSpeed = 0.0f; // Current speed
    private bool isCutsceneActive = false;
    private bool isGameEnded = false; // Flag to track game end

    private AudioSource audioSource; // For playing sound effects

    void Start()
    {
        if (thirdPersonVCam == null || povVCam == null)
        {
            Debug.LogError("CameraManager: One or both CinemachineVirtualCameras are not assigned!");
            return;
        }
        if (spaceship == null || cockpit == null)
        {
            Debug.LogError("CameraManager: Spaceship or Cockpit is not assigned!");
            return;
        }

        // Ensure cockpit is a child of spaceship
        if (!cockpit.transform.IsChildOf(spaceship.transform))
        {
            cockpit.transform.SetParent(spaceship.transform, true);
            Debug.LogWarning("CameraManager: Cockpit was not a child of Spaceship; re-parented automatically.");
        }

        // Add AudioSource if not present
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        var brain = Camera.main?.GetComponent<CinemachineBrain>();
        if (brain != null)
        {
            brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
        }
        else
        {
            Debug.LogWarning("CameraManager: CinemachineBrain not found!");
        }

        SetCameraState(isThirdPersonActive);
        Debug.Log($"Start: ThirdPersonCam active: {thirdPersonVCam.gameObject.activeSelf}, POVCam active: {povVCam.gameObject.activeSelf}");
    }

    void Update()
    {
        if (isGameEnded) return; // Exit Update if game ended to block all input

        if (Input.GetKeyDown(KeyCode.R))
        {
            SwapCameras();
        }

        // Quit game with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }

        // Handle movement and acceleration only if not in cutscene or game ended
        if (!isCutsceneActive && spaceship != null)
        {
            currentSpeed = 0f; // Reset speed by default
            if (!isThirdPersonActive || Input.GetKey(KeyCode.T))
            {
                currentSpeed = moveSpeed; // Default speed of 5
                if (Input.GetKey(KeyCode.T)) // Accelerate with 'T' key
                {
                    currentSpeed += accelerationIncrement; // Add 2 to current speed
                    currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed); // Limit to 15
                }

                // Move spaceship forward (cockpit follows as child)
                spaceship.transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime, Space.Self);
            }
        }
    }

    public void TriggerCutscene()
    {
        if (!isCutsceneActive)
        {
            isCutsceneActive = true;
            StartCoroutine(PlayCutscene());
        }
    }

    private System.Collections.IEnumerator PlayCutscene()
    {
        Debug.Log("Final Cutscene Started");

        // Pause spaceship movement
        float originalSpeed = currentSpeed;
        currentSpeed = 0f;

        // Example cutscene logic (adjust based on your needs)
        if (actorPrefab != null)
        {
            Vector3 spawnPosition = spaceship.transform.position + spaceship.transform.forward * 10f;
            GameObject newActor = Instantiate(actorPrefab, spawnPosition, Quaternion.identity);
            Animator actorAnimator = newActor.GetComponent<Animator>();
            if (actorAnimator != null)
            {
                actorAnimator.SetTrigger("StartAnimation");
            }
        }

        if (cutsceneSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cutsceneSound);
        }

        if (particleEffectPrefab != null)
        {
            ParticleSystem particles = Instantiate(particleEffectPrefab, spaceship.transform.position, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
        }

        // Wait for cutscene duration (adjust as per your cutscene length)
        yield return new WaitForSeconds(5f);

        // End game after cutscene
        EndGame();
    }

    private void EndGame()
    {
        isGameEnded = true;
        currentSpeed = 0f;

        // Disable and hide all critical GameObjects
        if (spaceship != null)
        {
            spaceship.SetActive(false);
            foreach (Transform child in spaceship.transform)
            {
                child.gameObject.SetActive(false);
                Renderer childRenderer = child.GetComponent<Renderer>();
                if (childRenderer != null) childRenderer.enabled = false;
                Collider childCollider = child.GetComponent<Collider>();
                if (childCollider != null) childCollider.enabled = false;
            }
            Renderer spaceshipRenderer = spaceship.GetComponent<Renderer>();
            if (spaceshipRenderer != null) spaceshipRenderer.enabled = false;
            Collider spaceshipCollider = spaceship.GetComponent<Collider>();
            if (spaceshipCollider != null) spaceshipCollider.enabled = false;
        }
        if (cockpit != null)
        {
            cockpit.SetActive(false);
            Renderer cockpitRenderer = cockpit.GetComponent<Renderer>();
            if (cockpitRenderer != null) cockpitRenderer.enabled = false;
            Collider cockpitCollider = cockpit.GetComponent<Collider>();
            if (cockpitCollider != null) cockpitCollider.enabled = false;
        }
        if (thirdPersonVCam != null) thirdPersonVCam.gameObject.SetActive(false);
        if (povVCam != null) povVCam.gameObject.SetActive(false);
        if (player != null) player.gameObject.SetActive(false);

        // Freeze time to stop all updates
        Time.timeScale = 0f;

        // Show end screen if assigned
        if (endScreen != null) endScreen.SetActive(true);

        // Trigger the EndGame event for SceneReloadManager
        if (OnEndGame != null) OnEndGame.Invoke(this, EventArgs.Empty);

        // Destroy this instance to fully stop all updates
        Destroy(this);

        Debug.Log("Game ended: All objects disabled, renderers/colliders off, time frozen, script destroyed.");
    }

    private void SwapCameras()
    {
        if (thirdPersonVCam == null || povVCam == null || spaceship == null || cockpit == null || isGameEnded)
        {
            Debug.LogError("CameraManager: Cannot swap due to unassigned references or game ended!");
            return;
        }

        isThirdPersonActive = !isThirdPersonActive;
        Debug.Log($"Swapping to {(isThirdPersonActive ? "Third-Person" : "POV")} mode");

        // Reposition player to spaceship when switching to POV
        if (!isThirdPersonActive && player != null)
        {
            player.position = spaceship.transform.position;
            player.rotation = spaceship.transform.rotation;
        }

        SetCameraState(isThirdPersonActive);
    }

    private void SetCameraState(bool isThirdPerson)
    {
        if (isGameEnded) return; // Prevent state changes after game end

        thirdPersonVCam.Priority = isThirdPerson ? 10 : 0;
        povVCam.Priority = isThirdPerson ? 0 : 10;

        // Keep spaceship active; toggle cockpit visibility
        spaceship.SetActive(true);
        cockpit.SetActive(!isThirdPerson);

        // Re-ensure cockpit is a child if detached
        if (!cockpit.transform.IsChildOf(spaceship.transform))
        {
            cockpit.transform.SetParent(spaceship.transform, true);
            Debug.LogWarning("CameraManager: Cockpit detached; re-parented during SetCameraState.");
        }

        Debug.Log($"SetCameraState: isThirdPerson: {isThirdPerson}");
        Debug.Log($"Spaceship active: {spaceship.activeSelf}, Cockpit active: {cockpit.activeSelf}");
        Debug.Log($"ThirdPersonCam priority: {thirdPersonVCam.Priority}, POVCam priority: {povVCam.Priority}");
        Debug.Log($"POVCam active: {povVCam.gameObject.activeSelf}, Cockpit visible: {cockpit.activeInHierarchy}");
    }

    // Optional: Handle trigger detection if integrated
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CutsceneTrigger") && spaceship != null && spaceship.CompareTag("Spaceship") && !isGameEnded)
        {
            Debug.Log("You are in the trigger zone");
            TriggerCutscene();
        }
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false; // Stop play mode in Editor
        #else
        Application.Quit(); // Quit the application in build
        #endif
        Debug.Log("Quit command issued.");
    }
}