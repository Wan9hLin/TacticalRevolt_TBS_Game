using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWarningUI : MonoBehaviour
{
    [SerializeField] private GameObject warningUI; // 引用警告 UI 的 GameObject
    private bool hasShownWarning = false;

    private void Start()
    {
        if (warningUI == null)
        {
            Debug.LogError("Warning UI is not assigned in PlayerWarningUI script.");
            return;
        }

        warningUI.SetActive(false); // 初始化时隐藏警告 UI

        // 订阅侦察模式结束事件
        EnemyAI.OnScoutModeEnded += EnemyAI_OnScoutModeEnded;
    }

    private void OnDestroy()
    {
        // 解除事件订阅
        EnemyAI.OnScoutModeEnded -= EnemyAI_OnScoutModeEnded;
    }


    private void EnemyAI_OnScoutModeEnded(object sender, EventArgs e)
    {        
           
        StartCoroutine(ShowWarningUICoroutine());
        
    }

    private IEnumerator ShowWarningUICoroutine()
    {
        warningUI.SetActive(true);
        AudioManager.Instance.Play("Warning");
        yield return new WaitForSeconds(3.5f);  // 持续显示 3 秒
        warningUI.SetActive(false);
        AudioManager.Instance.Stop("Warning");
    }
}
