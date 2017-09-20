using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenController : MonoBehaviour
{

    private string[] m_animations = new string[] {
        "wave",
        "die",
        "jump",
        "sit",
        "wipe",
        "check_time",
//        "lean",
        "cross_arms",
        "hands_on_hips",
        "smoke",
        "dance"
    };

    public void OnMouseDown ()
    {
        string animation = m_animations [Random.Range (0, m_animations.Length)];
        GetComponent<Animator> ().SetTrigger (animation);
    }

    public void OnCollisionEnter (Collision col)
    {
        if (col.gameObject.CompareTag ("Floor")) {
            // TODO: Orient vertically
            GetComponent<Animator> ().SetBool ("Grounded_b", true);
            GetComponent<Rigidbody> ().isKinematic = true;
        }
    }
}
