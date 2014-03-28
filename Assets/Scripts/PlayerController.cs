using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerPhysics))]
public class PlayerController : Entity {

	// Player Handling
	public float gravity = 20;
	public float walkSpeed = 8;
	public float runSpeed = 12;
	public float acceleration = 30;
	public float jumpHeight = 12;
	public float slideDeceleration = 10;

	private float initiateSlideThreshold = 9;

	// System
	private float animationSpeed;
	private float currentSpeed;
	private float targetSpeed;
	private Vector2 amountToMove;
	private float moveDirectionX;

	// States
	private bool jumping;
	private bool sliding;
	private bool wallHolding;
	private bool stopSliding;

	// Components
	private PlayerPhysics playerPhysics;
	private Animator animator;
	private GameManager manager;

	// Use this for initialization
	void Start () {
		playerPhysics = GetComponent<PlayerPhysics> ();
		animator = GetComponent<Animator> ();
		manager = Camera.main.GetComponent<GameManager> ();
		animator.SetLayerWeight (1, 1);
	}
	
	// Update is called once per frame
	void Update () {
		// Reset acceleration upon collision
		if (playerPhysics.movementStopped) {
			targetSpeed = 0;
			currentSpeed = 0;
		}

		// If player is touching the ground
		if (playerPhysics.grounded) {
			amountToMove.y = 0;

			if (wallHolding) {
				wallHolding = false;
				animator.SetBool("Wall Hold", false);
			}

			// Jump logic
			if (jumping) {
				jumping = false;
				animator.SetBool("Jumping", false);
			}

			// Slide logic
			if (sliding) {
				if (Mathf.Abs(currentSpeed) < 0.25f || stopSliding) {
					stopSliding = false;
					sliding = false;
					animator.SetBool("Sliding", false);
					playerPhysics.ResetCollider();
				}
			}

			// Slide input
			if (Input.GetButtonDown("Slide")) {
				if (Mathf.Abs(currentSpeed) > initiateSlideThreshold) {
					sliding = true;
					animator.SetBool("Sliding", true);
					targetSpeed = 0;

					playerPhysics.SetCollider(new Vector3(10.3f, 1.5f, 3), new Vector3(0.35f, 0.75f, 0));
				}
			}
		} else {
			if (!wallHolding) {
				if (playerPhysics.canWallHold) {
					wallHolding = true;
					animator.SetBool("Wall Hold", true);
				}
			}
		}

		// Jump input
		if (Input.GetButtonDown("Jump")) {
			if (sliding) {
				stopSliding = true;
			} else if (playerPhysics.grounded || wallHolding) {
				amountToMove.y = jumpHeight;
				jumping = true;
				animator.SetBool("Jumping", true);
				
				if (wallHolding) {
					wallHolding = false;
					animator.SetBool("Wall Hold", false);
				}
			}
		}

		animationSpeed = IncrementTowards (animationSpeed, Mathf.Abs (targetSpeed), acceleration);
		animator.SetFloat ("Speed", animationSpeed);

		// Input
		moveDirectionX = Input.GetAxisRaw ("Horizontal");
		if (!sliding) {
			float speed = (Input.GetButton ("Run")) ? runSpeed : walkSpeed;
			targetSpeed = moveDirectionX * speed;
			currentSpeed = IncrementTowards (currentSpeed, targetSpeed, acceleration);

			// Face direction
			if (moveDirectionX != 0 && !wallHolding) {
				transform.eulerAngles = (moveDirectionX > 0) ? Vector3.up * 180 : Vector3.zero;
			}
		} else {
			currentSpeed = IncrementTowards (currentSpeed, targetSpeed, slideDeceleration);
		}

		// Set amount to move
		amountToMove.x = currentSpeed;

		if (wallHolding) {
			amountToMove.x = 0;
			if (Input.GetAxisRaw("Vertical") != -1) {
				amountToMove.y = 0;
			}
		}

		amountToMove.y -= gravity * Time.deltaTime;
		playerPhysics.Move (amountToMove * Time.deltaTime, moveDirectionX);
	}

	void OnTriggerEnter(Collider c) {
		if (c.tag == "Checkpoint") {
			manager.SetCheckpoint(c.transform.position);
		}
		if (c.tag == "Finish") {
			manager.EndLevel();
		}
	}

	// Increase n towards target by speed
	private float IncrementTowards(float n, float target, float acceleration) {
		if (n == target) {
			return n;
		} else {
			float dir = Mathf.Sign(target - n); // must n be increaded or decreased to get closer to target
			n += acceleration * Time.deltaTime * dir;
			return (dir == Mathf.Sign(target - n)) ? n : target; // if n has now passed target then return target, otherwise return n
		}
	}
}
