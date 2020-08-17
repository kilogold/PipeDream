using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCallback : MonoBehaviour
{
    public void OnAnimationComplete()
    {
        var userInput = GetComponent<UserInput>();

        if(!userInput)
        {
            userInput = GetComponentInParent<UserInput>();
        }

        userInput.TriggerAnimationComplete();
    }

    public void OnPlayAnimation(bool inverted)
    {
        if(transform.parent.name.Equals("PipeH"))
        {
            var rend = GetComponent<SpriteRenderer>();
            rend.flipX = !rend.flipX;
        }

        GetComponent<Animator>().enabled = true;
    }
}
