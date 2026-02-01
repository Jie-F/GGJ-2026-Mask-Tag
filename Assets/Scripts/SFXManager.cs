using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Assign your SFX here")]
    public AudioClip teleportClip;
    //public AudioClip hitClip;
    //public AudioClip coinClip;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Add a single AudioSource to play sounds
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0; // 2D sound
    }

    // ======================
    // Public methods to play sounds
    // ======================
    public void PlayTeleport() => audioSource.PlayOneShot(teleportClip);

    //public void PlayHit() => audioSource.PlayOneShot(hitClip);

    //public void PlayCoin() => audioSource.PlayOneShot(coinClip);
}
