using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GogoGaga.UWI;
using UnityEngine.SceneManagement;

public class LevelScripting : MonoBehaviour
{
    [Header("任务相关对象")]
    [SerializeField] private List<GameObject> hider1List;
   
    [SerializeField] private List<GameObject> hider3List;

    [SerializeField] private Door door1;
    [SerializeField] private GameObject door2;
    [SerializeField] private SwitchDoor door3;
    [SerializeField] private InteractBeam interactBeam;

    [SerializeField] private List<GameObject> enemy1List;
    [SerializeField] private List<GameObject> enemy2List;
    [SerializeField] private List<GameObject> enemy3List;

    private bool hasShownSecondHider = false;


    [Header("任务系统")]
    [SerializeField] private GameObject Task1;
    [SerializeField] private GameObject Task2;
    [SerializeField] private GameObject Task3;
    private Animator task1Animator;
    private Animator task2Animator;
    private Animator task3Animator;
    private WaypointWorldMarker waypointWorldMarker;
    private WaypointWorldMarker waypointWorldMarker_2;
    private WaypointWorldMarker waypointWorldMarker_3;

    [Header("加载页面")]
    [SerializeField] private GameObject loadingPanel; // 加载页面
    [SerializeField] private Slider progressBar; // 进度条
    [SerializeField] private TextMeshProUGUI progressText; // 进度文本
    [SerializeField] private Animation loadingIconAnimation; // 加载图标动画
    [SerializeField] private float fakeLoadSpeed = 0.5f; // 进度条增长速度
    [SerializeField] private string nextSceneName; // 下一个场景的名称
    [SerializeField] private float delayBeforeLoading = 3f; // 开始加载前的延迟时间

    private void Start()
    {
        // 注册事件监听
        LevelGrid.Instance.OnAnyUnitMoveGridPosition += LevelGrid_OnAnyUnitMoveGridPosition;

        //任务系统
        task1Animator = Task1.GetComponent<Animator>();
        task2Animator = Task2.GetComponent<Animator>();
        task3Animator = Task3.GetComponent<Animator>();
        waypointWorldMarker = door1.gameObject.GetComponent<WaypointWorldMarker>();
        waypointWorldMarker_2 = door2.gameObject.GetComponent<WaypointWorldMarker>();
        waypointWorldMarker_3 = interactBeam.gameObject.GetComponent<WaypointWorldMarker>();
        loadingPanel.SetActive(false);
        loadingIconAnimation.gameObject.SetActive(false);

        waypointWorldMarker_2.enabled = !waypointWorldMarker_2.enabled;
        waypointWorldMarker_3.enabled = !waypointWorldMarker_3.enabled;

        door1.OnDoorOpened += (object sender, EventArgs e) =>
        {
            EnemyAI.ResetScoutMode();
            SetActiveGameObjectList(hider1List, false);
            SetActiveGameObjectList(enemy1List, true);

            task1Animator.SetBool("Active", true);
            waypointWorldMarker.enabled = !waypointWorldMarker.enabled;
            waypointWorldMarker_2.enabled = !waypointWorldMarker_2.enabled;
            AudioManager.Instance.Play("MetalDoorOpen");

        };

        door3.OnDoorOpened += (object sender, EventArgs e) =>
        {

            SetActiveGameObjectList(hider3List, false);
            SetActiveGameObjectList(enemy3List, true);
            AudioManager.Instance.Play("LaserClose");
        };

        interactBeam.OnBeamOpened += (object sender, EventArgs e) =>
        {
            task2Animator.SetBool("Active", true);
            task3Animator.SetBool("Active", true);

            // 触发场景加载逻辑
            StartCoroutine(TriggerSceneLoading());
        };




    }

    private void LevelGrid_OnAnyUnitMoveGridPosition(object sender, LevelGrid.OnAnyUnitMovedGridPositionEventArgs e)
    {
        if(e.toGridPosition.x == 6 && !hasShownSecondHider && !e.unit.IsEnemy())
        {
          
            hasShownSecondHider = true;

            EnemyAI.ResetScoutMode();
            Animator Door2animator = door2.GetComponent<Animator>();
            Door2animator.SetTrigger("isOpen");
            AudioManager.Instance.Play("SlideDoorOpen");
            SetActiveGameObjectList(enemy2List, true);

            waypointWorldMarker_2.enabled = !waypointWorldMarker_2.enabled;
            waypointWorldMarker_3.enabled = !waypointWorldMarker_3.enabled;

        }
    }

    private void SetActiveGameObjectList(List<GameObject> gameObjectList, bool isActive)
    {
        foreach (GameObject gameObject in gameObjectList)
        {
            gameObject.SetActive(isActive);
        }
    }

    private IEnumerator TriggerSceneLoading()
    {
        // 1. 等待指定延迟时间
        yield return new WaitForSeconds(delayBeforeLoading);

        // 2. 显示加载页面
        loadingPanel.SetActive(true);
        loadingIconAnimation.gameObject.SetActive(true);
        if (loadingIconAnimation != null)
        {
            loadingIconAnimation.Play();
        }

        // 3. 启动加载进度逻辑
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        float simulatedProgress = 0f;

        // 模拟进度条增长
        while (simulatedProgress < 1f)
        {
            simulatedProgress += fakeLoadSpeed * Time.deltaTime;
            progressBar.value = simulatedProgress;
            progressText.text = $"{Mathf.RoundToInt(simulatedProgress * 100)}%";
            yield return null;
        }

        // 确保进度条增长完成
        progressBar.value = 1f;
        progressText.text = "100%";

        // 延迟片刻，切换到下一个场景
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(nextSceneName);
    }


}
