using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Manages scene transitions with a fade-in/fade-out effect.
/// Implemented as a DontDestroyOnLoad Singleton.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Fade Settings")]
    [Tooltip("The UI Image used for the fade effect. Should cover the whole screen.")]
    [SerializeField] private Image fadeImage;
    [Tooltip("The duration of the fade-in and fade-out animations.")]
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeImage == null)
        {
            Debug.LogError("[SceneTransitionManager] Fade Image is not assigned!", this);
            this.enabled = false;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called automatically when a new scene is loaded.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Automatically fade in when a new scene loads.
        FadeIn();
    }

    /// <summary>
    /// Loads a new scene with a fade-out/fade-in transition.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // Ensure fade image is ready for fade out
        fadeImage.gameObject.SetActive(true);
        Tween fadeOutTween = fadeImage.DOFade(1f, fadeDuration);
        yield return fadeOutTween.WaitForCompletion();

        // Load the new scene
        SceneManager.LoadScene(sceneName);

        // OnSceneLoaded will handle the fade-in automatically.
    }

    private void FadeIn()
    {
        if (fadeImage == null) return;

        // Ensure fade image is ready for fade in
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1f);
        
        Tween fadeInTween = fadeImage.DOFade(0f, fadeDuration);
        fadeInTween.OnComplete(() => fadeImage.gameObject.SetActive(false));
    }
}
