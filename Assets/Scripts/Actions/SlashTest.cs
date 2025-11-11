using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashTest : MonoBehaviour
{
   
    public Slash slash1;
    public Slash slashHit;

 

    private void Start()
    {
        slash1.slashObject.SetActive(false);
        slashHit.slashObject.SetActive(false);

       

        if (TryGetComponent<SwordAction>(out SwordAction swordAction))
        {
            swordAction.OnSwordActionStarted += SwordAction_OnSwordActionStarted;           
        }

    }



    private void SwordAction_OnSwordActionStarted(object sender, EventArgs e)
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

public class Slash
{
    public GameObject slashObject;
    public float delay;
}
