using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdController : MonoBehaviour
{

    public void OnMouseDown ()
    {
        OnAdClicked ();
    }

    public void OnAdClicked ()
    {
        Debug.Log ("Ad Clicked!");
        Application.OpenURL("https://www.mopub.com/click-test/");
    }
}
