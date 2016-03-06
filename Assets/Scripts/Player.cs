using System.Diagnostics;
using UnityEngine;
using System.Collections;



[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour
{

    public Transform firePoint;
    public Transform armPoint;
    public GameObject arm;
    public GameObject knife;

	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
	float moveSpeed = 5;

	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;

	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	float timeToWallUnstick;

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;

    Animator playerAnimator;
	Controller2D controller;
    Animator armAnimator;
    SpriteRenderer armRenderer;

    private bool armPointFlipped;

	void Start() {
		controller = GetComponent<Controller2D> ();
        armAnimator = (Animator)arm.GetComponent(typeof(Animator));
        armRenderer = (SpriteRenderer)arm.GetComponent(typeof(SpriteRenderer));
	    playerAnimator = (Animator) GetComponent(typeof (Animator));

		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
	}

	void Update() {

        if (Input.GetKeyDown(KeyCode.R))
            controller.animator.SetLayerWeight(1, 1);

		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		int wallDirX = (controller.collisions.left) ? -1 : 1;
		float targetVelocityX = input.x * moveSpeed;
        bool wallSliding = false;

		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);

		
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
		{
		    wallSliding = true;
		    WallSlide(wallDirX, input);
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			WallJump(wallSliding, wallDirX, input);
		}
		if (Input.GetKeyUp (KeyCode.Space)) {
			if (velocity.y > minJumpVelocity) {
				velocity.y = minJumpVelocity;
			}
		}

        if (Input.GetKeyDown(KeyCode.R) && armAnimator.GetBool("AllowAttacking"))
        {
            RestoreArmPosition();
            UndoArmRendererFlip();
            StartAttack(wallSliding);
        }
	    SwitchLayerByArmAnimation();

		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime, input);

		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}			
	}

    private void SwitchLayerByArmAnimation()
    {
        if (armAnimator.GetCurrentAnimatorStateInfo(0).IsName("ArmSwing"))
        {
            controller.animator.SetLayerWeight(1, 1);
        }
        else if (armAnimator.GetCurrentAnimatorStateInfo(0).IsName("Standby"))
        {
            controller.animator.SetLayerWeight(1, 0);
        }
    }

    private void WallSlide(int wallDirX, Vector2 input)
    {
        if (velocity.y < -wallSlideSpeedMax)
            velocity.y = -wallSlideSpeedMax;

        if (timeToWallUnstick > 0)
        {
            velocityXSmoothing = 0;
            velocity.x = 0;

            if (input.x != wallDirX && input.x != 0)
                timeToWallUnstick -= Time.deltaTime;
            else
                timeToWallUnstick = wallStickTime;
        }
        else
        {
            timeToWallUnstick = wallStickTime;
        }
    }

    private void WallJump(bool wallSliding, int wallDirX, Vector2 input)
    {
        if (wallSliding)
        {
            if (wallDirX == input.x)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (input.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
        if (controller.collisions.below)
        {
            velocity.y = maxJumpVelocity;
        }
    }

    private void StartAttack(bool wallSliding)
    {
        armAnimator.SetTrigger("Attack");
        Vector3 knifeSource = firePoint.position;
        int faceDir = controller.collisions.faceDir;

        if (wallSliding)
        {
            knifeSource = FlipProjectileSource(faceDir, knifeSource);
            SetSlidingArmPosition(faceDir);
        }
        else if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Crouch") ||
            playerAnimator.GetCurrentAnimatorStateInfo(1).IsName("Crouch"))
        {
            knifeSource.Set(knifeSource.x, knifeSource.y -0.23f, knifeSource.z);
            SetCrouchingArmPosition(faceDir);
        }
        else if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") ||
            playerAnimator.GetCurrentAnimatorStateInfo(1).IsName("Idle"))
        {
            SetStandingArmPosition(faceDir);
        }

        Instantiate(knife, knifeSource, firePoint.rotation);
    }

    private Vector3 FlipProjectileSource(int faceDir, Vector3 source)
    {
        if (faceDir < 0)
            source.Set((float)(source.x + 0.5), source.y, source.z);

        else if (faceDir > 0)
            source.Set((float)(source.x - 0.5), source.y, source.z);
        
        return source;
    }

    private void SetStandingArmPosition(int faceDir)
    {
        if (faceDir < 0)
            AdjustArmPosition(0.02f, 0.0f, 10f);
        else if (faceDir > 0)
            AdjustArmPosition(-0.02f, 0.0f, 10f);
    }

    private void SetCrouchingArmPosition(int faceDir)
    {
        if (faceDir < 0)
            AdjustArmPosition(0.1f, -0.23f, 10f);
        else if (faceDir > 0)
            AdjustArmPosition(-0.1f, -0.23f, 10f);
    }

    private void SetSlidingArmPosition(int faceDir)
    {
        FlipArmRenderer();
        if (faceDir < 0)
            AdjustArmPosition(-0.15f, -0.05f, 10f);
        else if (faceDir > 0)
            AdjustArmPosition(0.15f, -0.05f, 10f);
    }

    private void AdjustArmPosition(float x, float y, float z)
    {
        Vector3 armSource = armPoint.position;
        armSource.x = armSource.x + x;
        armSource.y = armSource.y + y;
        armSource.z = armSource.y + z;

        armPoint.position = armSource;
    }

    private void FlipArmRenderer()
    {
        armRenderer.flipX = true;
        armPointFlipped = true;
    }

    private void UndoArmRendererFlip()
    {
        if (armPointFlipped)
        {
            armRenderer.flipX = false;
            armPointFlipped = false;
        }
    }

    private void RestoreArmPosition()
    {
        Vector3 originalPosition = transform.position;
        originalPosition.z = 1;
        armPoint.position = originalPosition;
    }
}
