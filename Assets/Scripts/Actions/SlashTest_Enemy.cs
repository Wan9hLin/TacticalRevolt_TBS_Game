using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashTest_Enemy : MonoBehaviour
{
    public Slash_enemy slash1;
    public Slash_enemy slashHit;



    private void Start()
    {
        slash1.slashObject.SetActive(false);
        slashHit.slashObject.SetActive(false);



        if (TryGetComponent<ShootAction>(out ShootAction shootAction))
        {
            shootAction.OnShoot += ShootAction_OnShoot;
            
        }

    }

  

    private void ShootAction_OnShoot(object sender, ShootAction.OnShootEventArgs e)
    {
        StartCoroutine(SlashAttack());
        StartCoroutine(HitAttack());
    }

    IEnumerator SlashAttack()
    {
        yield return new WaitForSeconds(slash1.delay);
        slash1.slashObject.SetActive(true);

        yield return new WaitForSeconds(1);
        slash1.slashObject.SetActive(false);

    }

    IEnumerator HitAttack()
    {
        yield return new WaitForSeconds(slashHit.delay);
        slashHit.slashObject.SetActive(true);

        yield return new WaitForSeconds(1);
        slashHit.slashObject.SetActive(false);

    }
}

[System.Serializable]

public class Slash_enemy
{
    public GameObject slashObject;
    public float delay;
}
