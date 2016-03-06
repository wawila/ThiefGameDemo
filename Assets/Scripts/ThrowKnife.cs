using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Networking;

public class ThrowKnife : MonoBehaviour {

    public float speed = 15;

    private Player player;
    private Controller2D controller;

    private int faceDir;

    void  Start()
    {
        player = (Player)FindObjectOfType(typeof (Player));
        controller = (Controller2D) player.GetComponent(typeof (Controller2D));
        faceDir = controller.collisions.faceDir;
        bool wallSticking = controller.animator.GetBool("WallSticking");
        var renderer = (SpriteRenderer)GetComponentInParent(typeof(SpriteRenderer));

        if (faceDir < 0)
        {
            speed = -speed;

            
            renderer.flipX = true;

            if (wallSticking)
            {
                speed = -speed;
                renderer.flipX = false;
            }
        }
        else if (faceDir > 0 && wallSticking)
        {
                speed = -speed;
                renderer.flipX = true;
        }
    
    }
     
	void Update ()
	{
        Destroy(gameObject, 0.6f);
        GetComponent<Rigidbody2D>().velocity = new Vector2(speed, GetComponent<Rigidbody2D>().velocity.y);
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag != "Player")
        {
            Destroy(gameObject);
        }
    }
}
