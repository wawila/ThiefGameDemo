using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class ThrowKnife : MonoBehaviour {

    public float speed;
    private Vector2 input;
    private int faceDir = 1;
    private Controller2D c2d;
    private int previousFaceDir = 1;

    void  Start()
    {
        

    }
     
	void Update ()
	{
        
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (input.x < 0)
        {
            previousFaceDir = faceDir;
            faceDir = -1;
        }
        else if (input.x > 0)
        {
            previousFaceDir = faceDir;
            faceDir = 1;
        }
        
        speed *= faceDir;

        Destroy(gameObject, 0.6f);
        GetComponent<Rigidbody2D>().velocity = new Vector2(speed, GetComponent<Rigidbody2D>().velocity.y);
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        //Destroy(gameObject);
    }
}
