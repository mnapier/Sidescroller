using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ragdoll : MonoBehaviour {

	private List<Transform> poseBones = new List<Transform> ();
	private List<Transform> ragdollBones = new List<Transform> ();

	public void CopyPose(Transform pose) {
		AddChildren (pose, poseBones);
		AddChildren (transform, ragdollBones);

		foreach (Transform poseBone in poseBones) {
			foreach (Transform ragdollBone in ragdollBones) {
				if (ragdollBone.name == poseBone.name) {
					ragdollBone.eulerAngles = poseBone.eulerAngles;
					break;
				}
			}
		}
	}

	private void AddChildren(Transform parent, List<Transform> list) {
		list.Add (parent);

		foreach (Transform t in parent) {
			AddChildren(t, list);
		}
	}
}
