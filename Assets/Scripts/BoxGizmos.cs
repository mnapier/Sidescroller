using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class BoxGizmos : MonoBehaviour {

	public Color gizmoColor;

	void OnDrawGizmos() {
		Gizmos.color = gizmoColor;
		Gizmos.DrawCube (transform.position + GetComponent<BoxCollider> ().center, GetComponent<BoxCollider> ().size);
	}
}
