using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{

    [SerializeField] private Transform bulletHitVfxPrefab;
    [SerializeField] private GameObject muzzlePrefab; // 发射初始特效
    [SerializeField] private float speed;

    private Vector3 targetPosition;
    private bool isHit;
    private Vector3 lastMovDir; // 保存最后的移动方向

    public void Setup(Vector3 targetPosition, bool isHit)
    {
        this.targetPosition = targetPosition;
        this.isHit = isHit;

        // 创建发射时的初始特效
        if (muzzlePrefab != null)
        {
            // 使用子弹的 forward 方向设置 muzzleVFX 的旋转
            Quaternion muzzleRotation = Quaternion.LookRotation(transform.forward);
            var muzzleVFX = Instantiate(muzzlePrefab, transform.position, muzzleRotation);

            var psMuzzle = muzzleVFX.GetComponent<ParticleSystem>();
            if (psMuzzle != null)
            {
                Destroy(muzzleVFX, psMuzzle.main.duration);
            }
            else
            {
                if (muzzleVFX.transform.childCount > 0)
                {
                    var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                    Destroy(muzzleVFX, psChild.main.duration);
                }
                else
                {
                    // 如果没有子粒子系统，设定一个默认的销毁时间
                    Destroy(muzzleVFX, 2f);
                }
            }
        }
    }

    private void Update()
    {
        if (speed > 0)
        {
            Vector3 movDir = (targetPosition - transform.position).normalized;
            lastMovDir = movDir; // 保存移动方向


            float distanceBeforeMoving = Vector3.Distance(transform.position, targetPosition);

            transform.position += movDir * speed * Time.deltaTime;

            float distanceAfterMoving = Vector3.Distance(transform.position, targetPosition);

            // 检查是否到达目标位置
            if (distanceBeforeMoving < distanceAfterMoving)
            {
                transform.position = targetPosition;
                OnHit();
            }
        }
        else
        {
            Debug.LogWarning("Bullet speed is not set!");
        }
    }


    private void OnHit()
    {
        if (isHit && bulletHitVfxPrefab != null)
        {
            // 使用移动方向的反方向作为接触法线
            Vector3 contactNormal = -lastMovDir;

            // 计算旋转，使特效的Y轴对齐于接触法线
            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, contactNormal);

            // 实例化命中特效并应用旋转
            var hitVFX = Instantiate(bulletHitVfxPrefab, targetPosition, hitRotation);

            // 可选：销毁命中特效以避免内存泄漏
            var psHit = hitVFX.GetComponent<ParticleSystem>();
            if (psHit != null)
            {
                Destroy(hitVFX.gameObject, psHit.main.duration);
            }
            else
            {
                if (hitVFX.transform.childCount > 0)
                {
                    var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                    Destroy(hitVFX.gameObject, psChild.main.duration);
                }
                else
                {
                    // 如果没有子粒子系统，设定一个默认的销毁时间
                    Destroy(hitVFX.gameObject, 2f);
                }
            }
        }

        Destroy(gameObject); // 销毁子弹对象
    }


}
