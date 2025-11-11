using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEventReceiver : MonoBehaviour
{
    private UnitAnimator unitAnimator;

    private void Awake()
    {
        // 从父游戏对象获取 UnitAnimator 组件
        unitAnimator = GetComponentInParent<UnitAnimator>();
        if (unitAnimator == null)
        {
            Debug.LogError("Cannot find UnitAnimator");
        }
    }

    // 该方法将由动画事件调用
    public void SpawnMindControlBullet()
    {
        if (unitAnimator != null)
        {
            unitAnimator.SpawnMindControlBullet();
        }
    }

    public void MindControlCastAnimationComplete()
    {

        if (unitAnimator != null)
        {
            unitAnimator.MindControlCastAnimationComplete();
        }
    }
    public void MindControlShootComplete()
    {
        if(unitAnimator != null)
        {
            unitAnimator.MindControlShootComplete();
        }
    }




}
