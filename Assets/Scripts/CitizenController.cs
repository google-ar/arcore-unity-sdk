using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenController : MonoBehaviour
{

    public void OnMouseDown ()
    {
        GetComponent<Animator> ().SetTrigger ("wave");
    }
}
