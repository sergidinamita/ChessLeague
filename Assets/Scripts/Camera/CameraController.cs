using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float mouseSensitivity = 3.0f;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    float MouseZoomSpeed = 15.0f;

    [SerializeField]
    float TouchZoomSpeed = 0.1f;

    [SerializeField]
    float ZoomMinBound = 0.1f;

    [SerializeField]
    float ZoomMaxBound = 179.9f;

    private float rotationY;
    private float rotationX;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private float distanceFromTarget = 3.0f;

    private Vector3 currentRotation;
    private Vector3 smoothVelocity = Vector3.zero;

    [SerializeField]
    private float smoothTime = 0.2f;

    [SerializeField]
    private Vector2 rotationXMinMax = new Vector2(-40, 40);

    [SerializeField]
    private Slider sensivitySlider;

    private void Start()
    {
        sensivitySlider.value = mouseSensitivity;
        rotationX = 34;

        if (!Connection.Instance.Host)   
        {
            transform.position = new Vector3(3.5f, 6.7103f, 13.448f);
            transform.Rotate(68f, 180f, 0);
            rotationX = 34;
            rotationY = 180;
        }

    }
    void Update()
    {
        if (Input.touchCount == 2)
        {

            // get current touch positions
            Touch tZero = Input.GetTouch(0);
            Touch tOne = Input.GetTouch(1);
            // get touch position from the previous frame
            Vector2 tZeroPrevious = tZero.position - tZero.deltaPosition;
            Vector2 tOnePrevious = tOne.position - tOne.deltaPosition;

            float oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
            float currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);

            // get offset value
            float deltaDistance = oldTouchDistance - currentTouchDistance;
            Zoom(deltaDistance, TouchZoomSpeed);
        }
        else if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            rotationY += mouseX;
            rotationX -= mouseY;

            // Apply clamping for x rotation 
            rotationX = Mathf.Clamp(rotationX, rotationXMinMax.x, rotationXMinMax.y);

            Vector3 nextRotation = new Vector3(rotationX, rotationY);

            // Apply damping between rotation changes
            currentRotation = Vector3.SmoothDamp(currentRotation, nextRotation, ref smoothVelocity, smoothTime);
            transform.localEulerAngles = currentRotation;

            // Substract forward vector of the GameObject to point its forward vector to the target
            transform.position = target.position - transform.forward * distanceFromTarget;
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Zoom(scroll, MouseZoomSpeed);
        if (cam.fieldOfView < ZoomMinBound)
        {
            cam.fieldOfView = 0.1f;
        }
        else
        if (cam.fieldOfView > ZoomMaxBound)
        {
            cam.fieldOfView = 179.9f;
        }

    }
    void Zoom(float deltaMagnitudeDiff, float speed)
    {

        cam.fieldOfView += deltaMagnitudeDiff * speed;
        // set min and max value of Clamp function upon your requirement
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, ZoomMinBound, ZoomMaxBound);
    }

    public void SensivityChanged()
    {
        mouseSensitivity = sensivitySlider.value;
    }
}