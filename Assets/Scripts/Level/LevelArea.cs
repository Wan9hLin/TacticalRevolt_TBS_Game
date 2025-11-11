using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelArea : MonoBehaviour
{

    public event EventHandler OnScientistReached;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Scientist"))
        {
            Debug.Log("Scientist enter!");
            OnScientistReached?.Invoke(this, EventArgs.Empty);
        }
    }
}
