using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class PlayerPhysics : MonoBehaviour {

	public LayerMask collisionMask;

	private BoxCollider collider;
	private Vector3 size;
	private Vector3 center;

	private Vector3 origionalSize;
	private Vector3 origionalCenter;
	private float colliderScale;

	private int collisionDivisionX = 3;
	private int collisionDivisionY = 10;

	// Distance above the ground
	private float skin = 0.005f;

	public bool grounded;
	public bool movementStopped;
	public bool canWallHold;

	private Transform platform;
	private Vector3 platformPositionOld;
	private Vector3 deltaPlatformPosition;

	Ray ray;
	RaycastHit hit;

	void Start() {
		collider = GetComponent<BoxCollider>();
		colliderScale = transform.localScale.x;

		origionalSize = collider.size;
		origionalCenter = collider.center;
		SetCollider (origionalSize, origionalCenter);
	}

	public void Move(Vector2 moveAmount, float moveDirectionX) {

		float deltaY = moveAmount.y;
		float deltaX = moveAmount.x;
		Vector2 playerPosition = transform.position;

		if (platform) {
			deltaPlatformPosition = platform.position - platformPositionOld;
		} else {
			deltaPlatformPosition = Vector3.zero;
		}


		#region Vertical Collisions
		// Check collisions top and bottom
		grounded = false;

		for (int cnt = 0; cnt < collisionDivisionX; cnt++) {
			float direction = Mathf.Sign(deltaY);
			float x = (playerPosition.x + center.x - size.x / 2) + size.x / (collisionDivisionX - 1) * cnt;	// left, center and then rightmost point of collider
			float y = playerPosition.y + center.y + size.y / 2 * direction; // Bottom of collider

			ray = new Ray(new Vector2(x, y), new Vector2(0, direction));
			Debug.DrawRay(ray.origin, ray.direction);

			if (Physics.Raycast(ray, out hit, Mathf.Abs(deltaY) + skin, collisionMask)) {
				platform = hit.transform;
				platformPositionOld = platform.position;

				// Get Distance between player and ground
				float distanceFromGround = Vector3.Distance(ray.origin, hit.point);

				// Stop player's downwards movement after coming within skin width of a collider
				if (distanceFromGround > skin) {
					//Debug.Log("Player is still above the ground");
					deltaY = distanceFromGround * direction - skin * direction;
				} else {
					//Debug.Log("Player has touched the ground");
					deltaY = 0;
				}

				grounded = true;
				break;
			} else {
				platform = null;
			}
		}
		#endregion

		#region Horizontal Collisions
		// Check collisions left and right
		movementStopped = false;
		canWallHold = false;

		if (deltaX != 0) {
			for (int cnt = 0; cnt < collisionDivisionY; cnt++) {
				float direction = Mathf.Sign (deltaX);
				float x = playerPosition.x + center.x + size.x / 2 * direction;
				float y = playerPosition.y + center.y - size.y / 2 + size.y / (collisionDivisionY - 1) * cnt; // side of collider

				ray = new Ray (new Vector2 (x, y), new Vector2 (direction, 0));
				Debug.DrawRay (ray.origin, ray.direction);

				if (Physics.Raycast (ray, out hit, Mathf.Abs (deltaX) + skin, collisionMask)) {
					if (hit.collider.tag == "Wall Jump") {
						if (Mathf.Sign(deltaX) == Mathf.Sign(moveDirectionX) && moveDirectionX != 0) {
							canWallHold = true;
						}
					}

					//Debug.Log("Player is intersecting with the ground");

					// Get Distance between player and ground
					float distanceFromGround = Vector3.Distance (ray.origin, hit.point);

					// Stop player's downwards movement after coming within skin width of a collider
					if (distanceFromGround > skin) {
							//Debug.Log("Player is still above the ground");
							deltaX = distanceFromGround * direction - skin * direction;
					} else {
							//Debug.Log("Player has touched the ground");
							deltaX = 0;
					}

					movementStopped = true;
					break;
				}
			}
		}
		#endregion

		if (!grounded && !movementStopped) {
			Vector3 playerDirection = new Vector3 (deltaX, deltaY);
			Vector3 origin = new Vector3 (playerPosition.x + center.x + size.x / 2 * Mathf.Sign (deltaX), playerPosition.y + center.y + size.y / 2 * Mathf.Sign (deltaY));
			Debug.DrawRay (origin, playerDirection.normalized);

			ray = new Ray (origin, playerDirection.normalized);

			if (Physics.Raycast (ray, Mathf.Sqrt (deltaX * deltaX + deltaY * deltaY), collisionMask)) {
				grounded = true;
				deltaY = 0;
			}
		}

		Vector2 finalTransform = new Vector2(deltaX + deltaPlatformPosition.x, deltaY);

		transform.Translate (finalTransform, Space.World);
	}

	public void SetCollider(Vector3 newSize, Vector3 newCenter) {
		collider.size = newSize;
		collider.center = newCenter;

		size = newSize * colliderScale;
		center = newCenter * colliderScale;
	}

	public void ResetCollider() {
		SetCollider (origionalSize, origionalCenter);
	}
}
