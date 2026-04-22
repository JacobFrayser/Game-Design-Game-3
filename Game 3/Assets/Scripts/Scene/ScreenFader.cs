using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [Header("UI Reference")]
    public Image fadeImage;

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

    void OnDisable()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
    }
}