using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [Header("UI Reference")]
    public Image fadeImage;
    public Image flashImage;

    [Header("Settings")]
    public float fadeSpeed = 1f;

    private Coroutine currentRoutine;

    void Awake()
    {
        // Singleton (prevents duplicates across scenes)
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
        // Start fully black → fade into scene
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 1f);

            currentRoutine = StartCoroutine(FadeIn());
        }
    }

    public void FadeToScene(string sceneName)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeOut(sceneName));
    }

    IEnumerator FadeOut(string sceneName)
    {
        if (fadeImage == null)
            yield break;

        fadeImage.gameObject.SetActive(true);

        float t = 0f;

        while (t < 1f)
        {
            if (fadeImage == null) yield break;

            t += Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, t);

            yield return null;
        }

        SceneManager.LoadScene(sceneName);

        currentRoutine = StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        if (fadeImage == null)
            yield break;

        float t = 1f;

        while (t > 0f)
        {
            if (fadeImage == null) yield break;

            t -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, t);

            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0f);
        fadeImage.gameObject.SetActive(false);
    }

    public void Flash(Color color, float duration)
    {
        StartCoroutine(FlashRoutine(color, duration));
    }

    private IEnumerator FlashRoutine(Color color, float duration)
    {
        flashImage.color = color;
        flashImage.gameObject.SetActive(true);

        // Fade from 0.35 alpha to full transparent
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.35f, 0f, elapsed / duration);
            flashImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        flashImage.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
    }

    // Used as separate function from above for same-scene fading (on death, for example)
    public void FadeToBlack(float duration, System.Action onComplete = null)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
        currentRoutine = StartCoroutine(FadeToBlackRoutine(duration, onComplete));
    }

    public void FadeFromBlack(float duration)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
        currentRoutine = StartCoroutine(FadeIn());
    }

    private IEnumerator FadeToBlackRoutine(float duration, System.Action onComplete)
    {
        if (fadeImage == null) yield break;

        // Set fade image to be active so it's visible when fading
        fadeImage.gameObject.SetActive(true);

        // Do the fade
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            fadeImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(t));
            yield return null;
        }

        onComplete?.Invoke();
    }
}