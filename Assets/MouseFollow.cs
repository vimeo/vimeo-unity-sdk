using UnityEngine;

public class MouseFollow : MonoBehaviour
{
	[SerializeField] private float mouseSensitivity = 100.0f;
	[SerializeField] private float clampAngle = 80.0f;
	private Vector3 currentRotation;

	private void Start ()
	{
		currentRotation = transform.localRotation.eulerAngles;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update ()
	{
		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = -Input.GetAxis("Mouse Y");

		currentRotation.y += mouseX * mouseSensitivity * Time.deltaTime;
		currentRotation.x += mouseY * mouseSensitivity * Time.deltaTime;

		currentRotation.x = Mathf.Clamp(currentRotation.x, -clampAngle, clampAngle);

		Quaternion localRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0.0f);
		transform.rotation = localRotation;
	}
}
