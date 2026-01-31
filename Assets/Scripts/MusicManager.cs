using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource frozenSource;
    public AudioSource fireSource;

    public float fadeSpeed = 2f;

    void Update()
    {
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

        // Intensity ramp
        float t = 1f - MaskManager.Instance.GetTimerNormalized();
        fireSource.pitch = 1f + t * 0.1f;
    }

    public void StopMusic()
    {
        frozenSource.Stop();
        fireSource.Stop();
    }
}
