using UnityEngine;
using System.Collections;

public class AnimationFunctions : MonoBehaviour
{

    public Animator myAnimator;


	void Start () {
	
	}
	
	void Update () {
	
	}

    public void setFalse(string boolName)
    {
        myAnimator.SetBool(boolName, false);
    }

    public void setTrue(string boolName)
    {
        myAnimator.SetBool(boolName, true);
    }
}
