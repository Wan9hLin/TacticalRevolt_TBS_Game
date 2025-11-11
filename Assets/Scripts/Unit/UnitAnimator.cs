using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform bulletprojectilePrefab;
    [SerializeField] private Transform shootPointTransform;
    [SerializeField] private Transform rifleTransform;
    [SerializeField] private Transform swordTransform;


    private Unit mindControlTargetUnit; // 用于存储 MindControlAction 的目标单位
    [SerializeField] private Transform mindPointTransform;
    [SerializeField] private Transform mindProjectilePrefab;
    
    private MindControlAction mindControlAction;
    private Animator MindTargetAnimator;


    private Unit unit;

    private void Awake()
    {
        unit = GetComponent<Unit>();

        if (TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving;
            moveAction.OnStopMoving += MoveAction_OnStopMoving;
        }

        if (TryGetComponent<ShootAction>(out ShootAction shootAction))
        {
            shootAction.OnShoot += ShootAction_OnShoot;
            shootAction.OnShootDamaged += ShootAction_OnShootDamaged;
            shootAction.OnShootMissed += ShootAction_OnShootMissed;

            shootAction.OnAimingStarted += ShootAction_OnAimingStarted;
            shootAction.OnAimingEnded += ShootAction_OnAimingEnded;


        }

        if (TryGetComponent<SwordAction>(out SwordAction swordAction))
        {
            swordAction.OnSwordActionStarted += SwordAction_OnSwordActionStarted;
            swordAction.OnSwordActionCompleted += SwordAction_OnSwordActionCompleted;

        }
        if (TryGetComponent<GrendAction>(out GrendAction grendAction))
        {
            grendAction.OnGrenade += GrendAction_OnGrenade;
            grendAction.OnGrenadeEnded += GrendAction_OnGrenadeEnded;
        }

        if(TryGetComponent<MindControlAction>(out MindControlAction mindControlAction))
        {
            MindControlAction.OnAnyMindControl += MindControlAction_OnAnyMindControl;
        }

        EnemyAI.OnScoutModeEnded += EnemyAI_OnScoutModeEnded;

    }

    private void OnDestroy()
    {
        EnemyAI.OnScoutModeEnded -= EnemyAI_OnScoutModeEnded;
        MindControlAction.OnAnyMindControl -= MindControlAction_OnAnyMindControl;
    }

    private void EnemyAI_OnScoutModeEnded(object sender, EventArgs e)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator is null in UnitAnimator.");
            return;
        }

        if (unit.IsInCover())
        {
            animator.SetBool("isCover", true);
        }
        else
        {            
            animator.SetTrigger("ExitScout");
        }

    }


    private void ShootAction_OnAimingEnded(object sender, EventArgs e)
    {
        if (unit.IsInCover())
        {
            animator.SetBool("isCover", true);
        }
        else
        {
            animator.SetBool("isCover", false);
        }
    }

    private void ShootAction_OnAimingStarted(object sender, EventArgs e)
    {
        animator.SetBool("isCover", false);
        
    }

    private void ShootAction_OnShootMissed(object sender, ShootAction.OnShootEventArgs e)
    {

        Animator targetUnitAnimator = e.targetUnit.GetAnimator();
        targetUnitAnimator.SetTrigger("Dodge");
    }

    private void ShootAction_OnShootDamaged(object sender, ShootAction.OnShootEventArgs e)
    {
        Animator targetUnitAnimator = e.targetUnit.GetAnimator();
        targetUnitAnimator.SetTrigger("Damage");
    }

    private void Start()
    {
        EquipRifle();
    }

    private void GrendAction_OnGrenade(object sender, EventArgs e)
    {
        animator.SetBool("isCover", false);
        animator.SetTrigger("Grenade");
        AudioManager.Instance.Play("GrenadeAction");
    }
    private void GrendAction_OnGrenadeEnded(object sender, EventArgs e)
    {
        if (unit.IsInCover())
        {
            animator.SetBool("isCover", true);
        }
    }


    private void SwordAction_OnSwordActionCompleted(object sender, EventArgs e)
    {
        EquipRifle();

        if (unit.IsInCover())
        {
            animator.SetBool("isCover", true);
        }
    }

    private void SwordAction_OnSwordActionStarted(object sender, EventArgs e)
    {
        animator.SetBool("isCover", false);
        EquipSword();
        animator.SetTrigger("SwordSlash");
        AudioManager.Instance.Play("SwordSlash");


    }

    private void MoveAction_OnStartMoving(object sender, EventArgs e)
    {
        animator.SetBool("isCover", false);
        animator.SetBool("isWalking", true);
        PlayMoveAudio();
    }

    private void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        animator.SetBool("isWalking", false);
        AudioManager.Instance.Stop("MoveActionSound");

        if (unit.IsInCover())
        {
            animator.SetBool("isCover", true);
        }
        else
        {
            animator.SetBool("isCover", false);
        }

    }

    private void ShootAction_OnShoot(object sender, ShootAction.OnShootEventArgs e)
    {
        int bulletsPerShot = e.bulletsPerShot;
        StartCoroutine(ShootMultipleBullets(bulletsPerShot, e));
    }

    private void MindControlAction_OnAnyMindControl(object sender, MindControlAction.OnMindControlEventArgs e)
    {
        if (e.controllingUnit == unit)
        {
            // 保存目标单位和 MindControlAction 的引用
            mindControlTargetUnit = e.targetUnit;
            mindControlAction = e.mindControlAction;

            MindTargetAnimator = mindControlTargetUnit.GetAnimator();

            // 播放射击动画
            animator.SetTrigger("MindControlCast");
           
            // 播放特效，可以在此实例化特效对象
            // ...
        }
    }

    // 在动画事件中调用
    
    public void MindControlCastAnimationComplete()
    {
        if (mindControlAction != null)
        {
            mindControlAction.OnMindControlAnimationComplete();
            
            // 重置引用
            mindControlAction = null;
            mindControlTargetUnit = null;
        }
    }

    public void MindControlShootComplete()
    {
        MindTargetAnimator.SetTrigger("MindPain");
        AudioManager.Instance.Play("MindControlVoice");
    }


    // 该方法将在动画事件中被调用
    public void SpawnMindControlBullet()
    {
        if (mindControlTargetUnit == null)
        {
            Debug.LogError("MindControl target unit is null.");
            return;
        }

        AudioManager.Instance.Play("MindControlShoot");

        // 实例化子弹特效
        Transform bulletProjectileTransform = Instantiate(mindProjectilePrefab, mindPointTransform.position, Quaternion.identity);
        BulletProjectile bulletProjectile = bulletProjectileTransform.GetComponent<BulletProjectile>();

        // 设置子弹的飞行方向指向目标单位
        Vector3 targetPosition = mindControlTargetUnit.GetWorldPosition();
        targetPosition.y = mindPointTransform.position.y; // 调整高度

        Vector3 shootDirection = (targetPosition - mindPointTransform.position).normalized;
        bulletProjectileTransform.forward = shootDirection;

        // 设置子弹飞向目标
        bulletProjectile.Setup(targetPosition, true);
        

        ScreenShake.Instance.Shake(1f);

        // 选项：在使用后重置目标单位引用
        mindControlTargetUnit = null;


    }



    // 新增协程：处理多个子弹的射击
    private IEnumerator ShootMultipleBullets(int bulletsPerShot, ShootAction.OnShootEventArgs e)
    {
        animator.SetTrigger("Shoot"); // 触发射击动画

        PlayShootAudio_Single();
        

        // 提前捕获目标位置
        Vector3 targetUnitShootAtPosition;
        if (e.targetUnit != null)
        {
            targetUnitShootAtPosition = e.targetUnit.GetWorldPosition();
            targetUnitShootAtPosition.y = shootPointTransform.position.y;
        }
        else
        {
            // 如果目标已被销毁，设定一个默认的位置或跳过剩余的子弹
            Debug.LogWarning("Target unit has been destroyed.");
            yield break; // 终止协程，避免进一步错误
        }

        // 提前捕获命中结果
        bool firstHit = e.shootingUnit.GetComponent<ShootAction>().GetHitToPass();

        for (int i = 0; i < bulletsPerShot; i++)
        {
            PlayShootAudio_Multiple();
            
            // 实例化子弹
            Transform bulletProjectileTransform =
                Instantiate(bulletprojectilePrefab, shootPointTransform.position, Quaternion.identity);
            BulletProjectile bulletProjectile = bulletProjectileTransform.GetComponent<BulletProjectile>();

            // 设置子弹前进方向
            Vector3 shootDirection = (targetUnitShootAtPosition - shootPointTransform.position).normalized;
            bulletProjectileTransform.forward = shootDirection;

            // 设置是否命中：如果第一颗子弹命中，所有子弹都显示命中特效
            bool isHit = firstHit;

            bulletProjectile.Setup(targetUnitShootAtPosition, isHit);

            // 触发相机震动
            ScreenShake.Instance.Shake();

            // 等待一小段时间再发射下一颗子弹
            yield return new WaitForSeconds(0.1f);
        }

        // 如果单位在掩体中，设置动画参数
        if (unit.IsInCover())
        {
            animator.SetBool("isCover", true);
        }
    }

    private void EquipSword()
    {
        swordTransform.gameObject.SetActive(true);
        rifleTransform.gameObject.SetActive(false);
    }

    private void EquipRifle()
    {
        swordTransform.gameObject.SetActive(false);
        rifleTransform.gameObject.SetActive(true);

    }

    //播放声音函数
    private void PlayShootAudio_Multiple()
    {
        if (unit.IsEnemy())
        {
            if(unit.CompareTag("NormalEnemy"))
            {
                // 播放射击音效
                AudioManager.Instance.Play("EnemyShoot");
             
            }
            else if (unit.CompareTag("CoverUse"))
            {
                AudioManager.Instance.Play("CoverUseShoot");
            }
            else if (unit.CompareTag("Special"))
            {
                AudioManager.Instance.Play("SpecialShoot");
            }
        }     
    }

    private void PlayShootAudio_Single()
    {

        if (unit.IsEnemy())
        {
            if (unit.CompareTag("SwordEnemy"))
            {
                AudioManager.Instance.Play("SwordSlash");
            }

            //有人被控制
            if (MindControlAction.isAnyUnitMindControlled)
            {
                if (unit.CompareTag("Commando"))
                {
                    AudioManager.Instance.Play("CommondoShoot");
                }
                else if (unit.CompareTag("Sniper"))
                {
                    AudioManager.Instance.Play("SniperShoot");
                }
                else if (unit.CompareTag("Medic"))
                {
                    AudioManager.Instance.Play("MedicShoot");
                }
                else if (unit.CompareTag("Heavy"))
                {
                    AudioManager.Instance.Play("GrendierShoot");
                }
            }
            
            
        }
        else
        {
            if (unit.CompareTag("Commando"))
            {
                AudioManager.Instance.Play("CommondoShoot");
            }
            else if (unit.CompareTag("Sniper"))
            {
                AudioManager.Instance.Play("SniperShoot");
            }
            else if (unit.CompareTag("Medic"))
            {
                AudioManager.Instance.Play("MedicShoot");
            }
            else if (unit.CompareTag("Heavy"))
            {
                AudioManager.Instance.Play("GrendierShoot");
            }

        }              
    }

    private void PlayMoveAudio()
    {
        AudioManager.Instance.Play("MoveActionSound");

        if (!unit.IsEnemy())
        {
            if (unit.CompareTag("Commando"))
            {
                AudioManager.Instance.Play("CommondoMove");
            }
            else if (unit.CompareTag("Sniper"))
            {
                AudioManager.Instance.Play("SniperMove");
            }
            else if (unit.CompareTag("Medic"))
            {
                AudioManager.Instance.Play("MedicMove");
            }
            else if (unit.CompareTag("Heavy"))
            {
                AudioManager.Instance.Play("GrendierMove");
            }
            else if (unit.CompareTag("Scientist"))
            {
                AudioManager.Instance.Play("ScientistMove");
            }
        }       
                       
    }
}