using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    // 无参数事件函数
    public void OnAnimationEnd()
    {
        // 停用对象
        gameObject.SetActive(false);

        // 或者销毁对象
        // Destroy(gameObject);
    }

    public void OnAnimationStart()
    {
        AudioManager.Instance.Play("SpaceCraftDown");
    }

   
 
}
