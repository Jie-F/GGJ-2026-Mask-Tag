using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    public Slider volumeSlider;
    public Slider sensitivitySlider;
    public Toggle fullscreenToggle;

    void Start()
    {
        // Load saved settings or defaults
        volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        ApplyVolume(volumeSlider.value);
        ApplySensitivity(sensitivitySlider.value);
        ApplyFullscreen(fullscreenToggle.isOn);
    }

    // ===== APPLY METHODS =====

    public void ApplyVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void ApplySensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);

        PlayerLook look = FindObjectOfType<PlayerLook>();
        if (look != null)
            look.SetSensitivity(value);
    }

    public void ApplyFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }
}
