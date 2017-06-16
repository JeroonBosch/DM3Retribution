using UnityEngine;


public class CameraController : MonoBehaviour {

	void FixedUpdate() {

		float xAxisValue = Input.GetAxis ("Horizontal");
		float zAxisValue = Input.GetAxis ("Vertical");

		if (Camera.current != null) {
			Camera.current.transform.Translate (new Vector3 (xAxisValue * 0.2f, zAxisValue * 0.2f, 0));
		}
	}
}
