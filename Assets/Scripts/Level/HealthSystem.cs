using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public event EventHandler OnDead;
    public event EventHandler OnDamaged;
    public event EventHandler OnHealed;

    private Unit unit;
   

    [SerializeField] private int health = 100;
    private int healthMax;

    private void Awake()
    {
        healthMax = health;
        unit = GetComponent<Unit>();
    }


    public void Damage(int damageAmount)
    {
        health -= damageAmount;
        if(health < 0)
        {
            health = 0;
        }

        OnDamaged?.Invoke(this, EventArgs.Empty);
       

        if (health == 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        health += healAmount;

        if (unit.CompareTag("Commando"))
        {
            if (health > 110)
            {
                health = 110;
            }
        }
        else if (unit.CompareTag("Sniper") || unit.CompareTag("Medic") || unit.CompareTag("NormalEnemy") 
            || unit.CompareTag("CoverUse") || unit.CompareTag("Special") || unit.CompareTag("Scientist") || unit.CompareTag("Hostage"))
        {
            if (health > 100)
            {
                health = 100;
            }
        }
        else if (unit.CompareTag("Heavy"))
        {
            if (health > 120)
            {
                health = 120;
            }
        }
        else if (unit.CompareTag("SwordEnemy"))
        {
            if (health > 150)
            {
                health = 150;
            }
        }




        //OnDamaged?.Invoke(this, EventArgs.Empty);
        OnHealed?.Invoke(this, EventArgs.Empty);
    }

    private void Die()
    {

        OnDead?.Invoke(this, EventArgs.Empty);
    }


    public float GetHealthNormalized()
    {
        return (float)health / healthMax;
    }

    public int GetHealth()
    {
        return health;
    }


    public int GetMaxHealth()
    {
        return healthMax;
    }

}
