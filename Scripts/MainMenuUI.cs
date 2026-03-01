using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button optionsButton;
    public Button exitButton;

    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public Slider loadingSlider;
    public TMP_Text loadingText;

    [Header("Options Panel")]
    public GameObject optionsPanel;
    public Button lowQualityButton;
    public Button mediumQualityButton;
    public Button highQualityButton;
    public Button ultraQualityButton;
    public Button closeOptionsButton;

    [Header("Scene Settings")]
    [Tooltip("Scene index to load when Play is clicked (default: 1)")]
    public int gameSceneIndex = 1;

    void Start()
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }
        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OpenOptions);
        }
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked);
        }

        if (lowQualityButton != null)
        {
            lowQualityButton.onClick.AddListener(SetLowQuality);
        }
        if (mediumQualityButton != null)
        {
            mediumQualityButton.onClick.AddListener(SetMediumQuality);
        }
        if (highQualityButton != null)
        {
            highQualityButton.onClick.AddListener(SetHighQuality);
        }
        if (ultraQualityButton != null)
        {
            ultraQualityButton.onClick.AddListener(SetUltraQuality);
        }
        if (closeOptionsButton != null)
        {
            closeOptionsButton.onClick.AddListener(CloseOptions);
        }

        // Load saved quality setting and force realtime lighting
        int savedQuality = PlayerPrefs.GetInt("QualityLevel", 5); // Default to Ultra
        QualitySettings.SetQualityLevel(savedQuality, true);
        ForceRealtimeLighting();
    }

    public void OnPlayClicked()
    {
        StartCoroutine(LoadSceneAsync(gameSceneIndex));
    }

    public void OnExitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void OpenOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    public void SetLowQuality()
    {
        QualitySettings.SetQualityLevel(1, true);
        ForceRealtimeLighting();
        PlayerPrefs.SetInt("QualityLevel", 1);
        PlayerPrefs.Save();
        Debug.Log("Quality set to Low");
    }

    public void SetMediumQuality()
    {
        QualitySettings.SetQualityLevel(2, true);
        ForceRealtimeLighting();
        PlayerPrefs.SetInt("QualityLevel", 2);
        PlayerPrefs.Save();
        Debug.Log("Quality set to Medium");
    }

    public void SetHighQuality()
    {
        QualitySettings.SetQualityLevel(3, true);
        ForceRealtimeLighting();
        PlayerPrefs.SetInt("QualityLevel", 3);
        PlayerPrefs.Save();
        Debug.Log("Quality set to High");
    }

    public void SetUltraQuality()
    {
        QualitySettings.SetQualityLevel(5, true);
        ForceRealtimeLighting();
        PlayerPrefs.SetInt("QualityLevel", 5);
        PlayerPrefs.Save();
        Debug.Log("Quality set to Ultra");
    }

    private void ForceRealtimeLighting()
    {
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.High;
        QualitySettings.realtimeReflectionProbes = true;
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        if (loadingSlider != null)
        {
            loadingSlider.value = 0;
        }
        if (loadingText != null)
        {
            loadingText.text = "Loading... 0%";
        }

        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        float minimumLoadTime = 2f;
        float elapsedTime = 0f;
        float displayProgress = 0f;

        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;
            
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);
            displayProgress = Mathf.Min(realProgress, timeProgress);

            if (loadingSlider != null)
            {
                loadingSlider.value = displayProgress;
            }

            if (loadingText != null)
            {
                loadingText.text = $"Loading... {(displayProgress * 100f):F0}%";
            }

            if (operation.progress >= 0.9f && elapsedTime >= minimumLoadTime)
            {
                if (loadingSlider != null) loadingSlider.value = 1f;
                if (loadingText != null) loadingText.text = "Loading... 100%";
                
                yield return new WaitForSeconds(0.3f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
