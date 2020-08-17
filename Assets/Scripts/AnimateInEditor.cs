using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AnimateInEditor : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        GetComponent<Animator>().Update(Time.deltaTime);
    }
}
