using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // 用于加载新场景

public class BackgroundCarousel : MonoBehaviour
{
    [Header("背景轮换相关")]
    [SerializeField] private Image image1; // 第一张图片
    [SerializeField] private Image image2; // 第二张图片
    [SerializeField] private float fadeDuration = 2f; // 淡入淡出的时间
    [SerializeField] private float displayDuration = 5f; // 每张图片展示时间
    [SerializeField] private Sprite[] backgroundImages; // 背景图片数组（四张图片）

    private int currentImageIndex = 0; // 当前展示的图片索引
    private bool isFading = false; // 防止重复淡入淡出

    [Header("加载页面相关")]
    [SerializeField] private GameObject loadingPanel; // Loading 页面
    [SerializeField] private Slider progressBar; // 进度条
    [SerializeField] private Button startGameButton; // 开始游戏按钮
    [SerializeField] private string sceneToLoad = "level1"; // 要加载的场景名
    [SerializeField] private Animation loadingIconAnimation; // Loading 图标的 Animation 组件
    [SerializeField] private TextMeshProUGUI progressText; // 显示进度百分比的文本
    [SerializeField] private float progressSimSpeed = 0.5f; // 进度条增长速度


    private bool sceneIsLoaded = false; // 场景是否加载完成的标记

    private void Start()
    {
        // 初始化背景轮换
        PreloadImages();
        StartCoroutine(RotateBackgrounds());

        // 初始化加载页面
        loadingPanel.SetActive(false); // 确保加载页面默认隐藏
        progressBar.value = 0f; // 初始化进度条
        progressText.text = "0%"; // 初始化进度文本
        loadingIconAnimation.gameObject.SetActive(false);

        startGameButton.onClick.AddListener(OnStartGameClicked); // 绑定按钮点击事件
    }

    private void PreloadImages()
    {
        // 初始化第一张和第二张图片
        image1.sprite = backgroundImages[currentImageIndex];
        image1.color = Color.white;
        image2.color = new Color(1, 1, 1, 0);
    }

    private IEnumerator RotateBackgrounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(displayDuration); // 等待展示时间

            if (!isFading)
            {
                StartCoroutine(FadeToNextImage());
            }
        }
    }

    private IEnumerator FadeToNextImage()
    {
        isFading = true;
        int nextImageIndex = (currentImageIndex + 1) % backgroundImages.Length;
        image2.sprite = backgroundImages[nextImageIndex];

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.SmoothStep(0f, 1f, timer / fadeDuration);
            image1.color = new Color(1, 1, 1, 1 - alpha);
            image2.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        // 交换图片
        image1.color = new Color(1, 1, 1, 0);
        image2.color = Color.white;
        currentImageIndex = nextImageIndex;
        Image temp = image1;
        image1 = image2;
        image2 = temp;
        isFading = false;
    }

    private void OnStartGameClicked()
    {
        // 显示加载页面
        loadingPanel.SetActive(true);
        loadingIconAnimation.gameObject.SetActive(true);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("Button_1");
        }
        else
        {
            Debug.LogWarning("AudioManager.Instance is null.");
        }

        if (loadingIconAnimation != null) loadingIconAnimation.Play();

        // 启动进度条模拟
        StartCoroutine(LoadGameScene());

    }

    private IEnumerator LoadGameScene()
    {
        // 启动异步场景加载
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // 计算加载进度
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";

            // 当加载进度达到 90% 时，允许激活场景
            if (operation.progress >= 0.9f)
            {
                // 等待一小段时间，模拟加载完成后的停顿
                yield return new WaitForSeconds(0.5f);

                // 激活场景
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }




}