using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class Settings : MonoBehaviour
{
    public Slider volumeSlider;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    public AudioSource musicSource;

    private Resolution[] resolutions;

    void Start()
    {
        // --- Inicializar Resoluciones ---
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        // --- Cargar configuraciones guardadas ---
        LoadSettings();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume; // Esto controla el volumen general (opcional)
        if (musicSource != null)
        {
            musicSource.volume = volume; // Esto controla solo la música
        }
        PlayerPrefs.SetFloat("volume", volume);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("quality", qualityIndex);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("resolution", resolutionIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("fullscreen", isFullscreen ? 1 : 0);
    }

    public void LoadSettings()
    {
        // Volumen
        float volume = PlayerPrefs.GetFloat("volume", 1f); // 1f por defecto
        AudioListener.volume = volume;
        volumeSlider.value = volume;

        // Calidad
        int quality = PlayerPrefs.GetInt("quality", QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(quality);
        qualityDropdown.value = quality;
        qualityDropdown.RefreshShownValue();

        // Fullscreen
        bool isFullscreen = PlayerPrefs.GetInt("fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = isFullscreen;
        fullscreenToggle.isOn = isFullscreen;

        // Resolución y dropdown
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int defaultResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            // Si no hay resolución guardada, por defecto ponemos 1920x1080
            if (PlayerPrefs.HasKey("resolution") == false)
            {
                if (resolutions[i].width == 1920 && resolutions[i].height == 1080)
                {
                    defaultResolutionIndex = i;
                }
            }
            else
            {
                // Si hay resolución guardada, cargamos esa
                if (i == PlayerPrefs.GetInt("resolution"))
                {
                    defaultResolutionIndex = i;
                }
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = defaultResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Aplicar la resolución guardada o la por defecto 1920x1080
        Screen.SetResolution(resolutions[defaultResolutionIndex].width, resolutions[defaultResolutionIndex].height, isFullscreen);
    }


    public void ResetToDefaults()
    {
        // Opcional: Botón para resetear
        PlayerPrefs.DeleteAll();
        LoadSettings();
    }
}
