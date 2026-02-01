using UnityEngine;
using UnityEngine.SceneManagement;

public enum MusicState
{
    Menu,
    Gameplay,
    GameOver,
    GameWin
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Sources")]
    public AudioSource menuSource;
    public AudioSource fireSource;
    public AudioSource frozenSource;
    public AudioSource gameOverSource;
    public AudioSource gameWinSource;

    [Header("Settings")]
    public float fadeSpeed = 1.5f;

    MusicState currentState;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        StopAllMusic();
        PlayMenuMusic();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (currentState != MusicState.Gameplay)
            return;

        UpdateGameplayMusic();
    }

    // =======================
    // Scene Handling
    // =======================

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenu":
                PlayMenuMusic();
                break;

            case "Game":
                PlayGameplayMusic();
                break;

            case "GameOver":
                PlayGameOverMusic();
                break;

            case "GameWin":
                PlayGameWinMusic();
                break;
        }
    }

    // =======================
    // Public API
    // =======================

    public void PlayMenuMusic()
    {
        SetState(MusicState.Menu);
        PlayExclusive(menuSource);
    }

    public void PlayGameplayMusic()
    {
        SetState(MusicState.Gameplay);

        fireSource.volume = 0f;
        frozenSource.volume = 0f;

        if (!fireSource.isPlaying) fireSource.Play();
        if (!frozenSource.isPlaying) frozenSource.Play();
    }

    public void PlayFireMusic()
    {
        SetState(MusicState.Gameplay);
        fireSource.Play();
    }

    public void PlayFrozenMusic()
    {
        SetState(MusicState.Gameplay);
        frozenSource.Play();
    }

    public void PlayGameOverMusic()
    {
        SetState(MusicState.GameOver);
        PlayExclusive(gameOverSource);
    }

    public void PlayGameWinMusic()
    {
        SetState(MusicState.GameWin);
        PlayExclusive(gameWinSource);
    }

    // =======================
    // Internals
    // =======================

    void SetState(MusicState newState)
    {
        currentState = newState;
        StopAllMusic();
    }

    void StopAllMusic()
    {
        menuSource.Stop();
        fireSource.Stop();
        frozenSource.Stop();
        gameOverSource.Stop();
        gameWinSource.Stop();
    }

    void PlayExclusive(AudioSource source)
    {
        StopAllMusic();
        source.volume = 1f;
        source.Play();
    }

    void UpdateGameplayMusic()
    {
        if (MaskManager.Instance == null)
            return;

        if (MaskManager.Instance.currentOwner == MaskOwner.Player)
        {
            fireSource.volume = Mathf.MoveTowards(fireSource.volume, 1f, Time.deltaTime / fadeSpeed);
            frozenSource.volume = Mathf.MoveTowards(frozenSource.volume, 0f, Time.deltaTime / fadeSpeed);
        }
        else
        {
            frozenSource.volume = Mathf.MoveTowards(frozenSource.volume, 1f, Time.deltaTime / fadeSpeed);
            fireSource.volume = Mathf.MoveTowards(fireSource.volume, 0f, Time.deltaTime / fadeSpeed);
        }

        //float t = 1f - MaskManager.Instance.GetTimerNormalized();
        //fireSource.pitch = Mathf.Clamp(1f + t * 0.1f, 1f, 1.1f);
    }
}
