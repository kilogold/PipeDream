using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour
{
    public event System.Action<GameObject> OnNodeInteraction;
    public event System.Action<GameObject> OnAnimationCompleted;

    public bool clickable = true;

    private void OnMouseDown()
    {
        if (!clickable)
            return;

        OnNodeInteraction?.Invoke(gameObject);
    }

    public void TriggerAnimationComplete()
    {
        OnAnimationCompleted?.Invoke(gameObject);
    }
}
