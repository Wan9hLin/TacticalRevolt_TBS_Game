using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GogoGaga.UWI;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Level2Scripting : MonoBehaviour
{
    
    [SerializeField] private GameObject device_fake;
    [SerializeField] private Door device;
    [SerializeField] private Door device_2;
    [SerializeField] private GameObject Car;
    [SerializeField] private GameObject LeaveArea;
    [SerializeField] private GameObject deviceUI;
    [SerializeField] private GameObject waypoint3;
    [SerializeField] private GameObject waypoint4;


    private WaypointWorldMarker waypointWorldMarker;
    private WaypointWorldMarker waypointWorldMarker_2;
    private WaypointWorldMarker waypointWorldMarker_3;
    private WaypointWorldMarker waypointWorldMarker_4;


    [SerializeField] private List<GameObject> enemy1List;
    [SerializeField] private GameObject hider1;

    [SerializeField] private List<GameObject> enemy2List;
    [SerializeField] private GameObject hider2;

    private bool fisrtDeviceShow = false;

    private int device2OpenedTurnNumber = -1; // 初始值设为 -1，表示尚未打开
    private int remainingEnemyTurns = -1; // -1 表示倒计时尚未开始

    [SerializeField] private Image BeforeLeaveUI;
    [SerializeField] private TextMeshProUGUI LeaveNoText;
    [SerializeField] private GameObject warningUI; // 引用警告 UI 的 GameObject


    //任务系统
    [SerializeField] private GameObject Task1;
    [SerializeField] private GameObject Task2;
    [SerializeField] private GameObject Task3;
    private Animator task1Animator;
    private Animator task2Animator;
    private Animator task3Animator;

    [Header("加载页面")]
    [SerializeField] private GameObject loadingPanel; // 加载页面
    [SerializeField] private Slider progressBar; // 进度条
    [SerializeField] private TextMeshProUGUI progressText; // 进度文本
    [SerializeField] private Animation loadingIconAnimation; // 加载图标动画
    [SerializeField] private float fakeLoadSpeed = 0.5f; // 进度条增长速度
    [SerializeField] private string nextSceneName; // 下一个场景的名称
    [SerializeField] private float delayBeforeLoading = 3f; // 开始加载前的延迟时间

    private bool CanLeave = false;
    private bool isCommondo = false;
    private bool isMedic = false;
    private bool isHeavy = false;
    private bool isSniper = false;
    private int LeaveCount = 0;

    private void Start()
    {
        Debug.Log(UnitManager.Instance.GetFriendlyUnitList().Count + 1);
        device.gameObject.SetActive(false);
        device_fake.SetActive(true);
        deviceUI.SetActive(false);

        waypointWorldMarker = device.gameObject.GetComponent<WaypointWorldMarker>();
        EnemyAI.OnScoutModeEnded += EnemyAI_OnScoutModeEnded;

        waypointWorldMarker_2 = device_2.GetComponent<WaypointWorldMarker>();
        waypointWorldMarker_3 = waypoint3.GetComponent<WaypointWorldMarker>();
        waypointWorldMarker_4 = waypoint4.GetComponent<WaypointWorldMarker>();

        device_2.enabled = !device_2.enabled;
        waypointWorldMarker_2.enabled = !waypointWorldMarker_2.enabled;
        waypointWorldMarker_3.enabled = !waypointWorldMarker_3.enabled;
        waypointWorldMarker_4.enabled = !waypointWorldMarker_4.enabled;


        //任务系统
        task1Animator = Task1.GetComponent<Animator>();
        task2Animator = Task2.GetComponent<Animator>();
        task3Animator = Task3.GetComponent<Animator>();
        
        // 注册事件监听
        LevelGrid.Instance.OnAnyUnitMoveGridPosition += LevelGrid_OnAnyUnitMoveGridPosition;

        EnemyAI.ResetScoutMode();


        device.OnDoorOpened += (object sender, EventArgs e) =>
        {
            waypointWorldMarker.enabled = !waypointWorldMarker.enabled;
            Debug.Log("CoreDeivce Installed!");
            AudioManager.Instance.Play("CoreData");
            waypointWorldMarker_2.enabled = !waypointWorldMarker_2.enabled;
            device_2.enabled = !device_2.enabled;
            task2Animator.SetBool("Active", true);
        };

        // 确保 Car 初始时是隐藏的
        BeforeLeaveUI.gameObject.SetActive(false);
        Car.SetActive(false);
        LeaveArea.SetActive(false);

        // 订阅 TurnSystem 的回合变化事件
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

        device_2.OnDoorOpened += (object sender, EventArgs e) =>
        {
            // 开始敌人回合的倒计时
            AudioManager.Instance.Play("LeaveSign");
            remainingEnemyTurns = 5;
            BeforeLeaveUI.gameObject.SetActive(true);
            LeaveNoText.text = $"{remainingEnemyTurns}";
            Debug.Log($"Device_2 opened at turn {device2OpenedTurnNumber}");


            hider1.SetActive(false);
            SetActiveGameObjectList(enemy1List, true);
            EnemyAI.isInScoutMode = false;
            StartCoroutine(ShowWarningUICoroutine());
            waypointWorldMarker_3.enabled = !waypointWorldMarker_3.enabled;

        };
    }

    private void LevelGrid_OnAnyUnitMoveGridPosition(object sender, LevelGrid.OnAnyUnitMovedGridPositionEventArgs e)
    {
        if (e.toGridPosition.z <= 5 && e.toGridPosition.z >= 2 && e.toGridPosition.x <= 38 && e.toGridPosition.x >= 35 && !e.unit.IsEnemy()) 
        {
            if (CanLeave && e.unit.CompareTag("Commando") && !isCommondo)
            {
                LeaveCount++;
                isCommondo = true;
                Debug.Log("Commado Enter" + LeaveCount);
            }
            if (CanLeave && e.unit.CompareTag("Sniper") && !isSniper)
            {
                LeaveCount++;
                isSniper = true;
                Debug.Log("Sniper Enter" + LeaveCount);
            }
            if (CanLeave && e.unit.CompareTag("Medic") && !isMedic)
            {
                LeaveCount++;
                isMedic = true;
                Debug.Log("Medic Enter" + LeaveCount);
            }
            if (CanLeave && e.unit.CompareTag("Heavy") && !isHeavy)
            {
                LeaveCount++;
                isHeavy = true;
                Debug.Log("Heavy Enter" + LeaveCount);
            }
         
        }

        if (LeaveCount >= UnitManager.Instance.GetFriendlyUnitList().Count)
        {
            task1Animator.SetBool("Active", true);
            task3Animator.SetBool("Active", true);
            StartCoroutine(TriggerSceneLoading());
        }


    }

    private void EnemyAI_OnScoutModeEnded(object sender, EventArgs e)
    {
        if (!fisrtDeviceShow)
        {
            device.gameObject.SetActive(true);
            device_fake.SetActive(false);
            deviceUI.SetActive(true);

            fisrtDeviceShow = true;
        }
    }

    private void SetActiveGameObjectList(List<GameObject> gameObjectList, bool isActive)
    {
        foreach (GameObject gameObject in gameObjectList)
        {
            gameObject.SetActive(isActive);
        }
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        // 检查倒计时是否已经开始
        if (remainingEnemyTurns >= 0)
        {
            if (TurnSystem.Instance.IsPlayerTurn())
            {
                // 玩家回合，更新界面显示
                LeaveNoText.text = $"{remainingEnemyTurns}";
            
                if(remainingEnemyTurns == 3)
                {
                    hider2.SetActive(false);
                    SetActiveGameObjectList(enemy2List, true);
                    EnemyAI.isInScoutMode = false;
                    StartCoroutine(ShowWarningUICoroutine());
                    waypointWorldMarker_4.enabled = !waypointWorldMarker_4.enabled;
                }

                // 如果剩余敌人回合数小于等于 0，表示倒计时结束
                if (remainingEnemyTurns <= 0)
                {
                    // 让 Car 出现，隐藏倒计时 UI
                    Car.SetActive(true);
                    LeaveArea.SetActive(true);
                    BeforeLeaveUI.gameObject.SetActive(false);
                    Debug.Log($"Car appeared at turn {TurnSystem.Instance.GetTurnNumber()}");
                    CanLeave = true;
                    // 取消订阅事件，避免重复触发
                    TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
                }
            }
            else
            {
                // 敌人回合，减少剩余敌人回合数
                remainingEnemyTurns--;

                if (remainingEnemyTurns == 4)
                {
                    waypointWorldMarker_3.enabled = !waypointWorldMarker_3.enabled;
                }

                if (remainingEnemyTurns == 2)
                {
                    waypointWorldMarker_4.enabled = !waypointWorldMarker_4.enabled;
                }

                // 更新界面显示
                LeaveNoText.text = $"{remainingEnemyTurns}";
            }
        }
    }

    private IEnumerator ShowWarningUICoroutine()
    {
        warningUI.SetActive(true);
        AudioManager.Instance.Play("EnemyDetected");
        yield return new WaitForSeconds(3.5f);  // 持续显示 3 秒
        warningUI.SetActive(false);
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
