using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using INab.WorldAlchemy;

public class UnitRagdollSpawner : MonoBehaviour
{
    [SerializeField] private Transform ragdollPrefab;
    [SerializeField] private Transform originalRootBone;

    private HealthSystem healthSystem;

    [SerializeField] private SeeThroughDetect seethrough;

    private Unit unit;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        unit = GetComponent<Unit>();

        healthSystem.OnDead += HealthSystem_OnDead;
    }

    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
       Transform ragdollTransform = Instantiate(ragdollPrefab, transform.position, transform.rotation);


        //…Ë÷√seetrough
        if (!unit.IsEnemy())
        {
            seethrough.targetTransform = ragdollTransform;
        }
      

       UnitRogdoll unitRogdoll = ragdollTransform.GetComponent<UnitRogdoll>();
       unitRogdoll.Setup(originalRootBone);
      
    }
}
