using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRogdoll : MonoBehaviour
{
    [SerializeField] private Transform ragdollRootBone;

    
    public void Setup(Transform originalRootBone)
    {
        MatchAllChildTransforms(originalRootBone, ragdollRootBone);

        Vector3 randomDir = new Vector3(Random.Range(-1f, +1f), 0, Random.Range(-1f, +1f));
        ApplyExplosionToRagdoll(ragdollRootBone, 300f, transform.position + randomDir, 10f);
    }

    private void MatchAllChildTransforms(Transform root, Transform clone)
    {
        foreach(Transform child in root)
        {
            Transform cloneChild = clone.Find(child.name);
            if(cloneChild != null)
            {
                cloneChild.position = child.position;
                cloneChild.rotation = child.rotation;

                MatchAllChildTransforms(child, cloneChild);
            }

        }
    }

    private void ApplyExplosionToRagdoll(Transform root, float explosionForce, Vector3 explosionPosition, float explosionRange)
    {
        foreach(Transform child in root)
        {
          if (child.TryGetComponent<Rigidbody>(out Rigidbody childRigidbody))
          {
                childRigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRange);
          }

          ApplyExplosionToRagdoll(child, explosionForce, explosionPosition, explosionRange);

        }

    }
}
