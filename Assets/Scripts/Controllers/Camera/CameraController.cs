using App.Configs;
using UnityEngine;
using Zenject;

namespace Controllers
{
    public class CameraController : IInitializable, ILateTickable
    {
        private CameraConfig cameraConfig;
        private Camera mainCamera;

        private Vector3 currentVelocity;
        private Quaternion targetRotation;
        private float xRotation;
        private float yRotation;
        private float scrollVelocity;

        public CameraController(CameraConfig cameraConfig)
        {
            this.cameraConfig = cameraConfig;
        }

        public void Initialize()
        {
            mainCamera = Camera.main;

            xRotation = mainCamera.transform.eulerAngles.x;
            yRotation = mainCamera.transform.eulerAngles.y;
            targetRotation = mainCamera.transform.rotation;
        }

        public void LateTick()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            Movement();
            Rotation();
            Scroll();
        }

        private void Movement()
        {
            var targetVelocity = Vector3.zero;

            var speedModifier = Input.GetKey(KeyCode.LeftShift)
                ? cameraConfig.movementSpeed * 3
                : cameraConfig.movementSpeed;

            var movementInputX = Input.GetAxisRaw("Horizontal");
            var movementInputZ = Input.GetAxisRaw("Vertical");

            var currentY = mainCamera.transform.position.y;

            targetVelocity += mainCamera.transform.right * movementInputX;

            var forwardFlat = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
            targetVelocity += forwardFlat * movementInputZ;

            if (targetVelocity != Vector3.zero)
                targetVelocity = targetVelocity.normalized * speedModifier;

            var t = 1f - Mathf.Exp(-cameraConfig.acceleration * Time.deltaTime);
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, t);

            var newPosition = mainCamera.transform.position + currentVelocity * Time.deltaTime;

            newPosition.x = Mathf.Clamp(newPosition.x, cameraConfig.cameraClampX.Min, cameraConfig.cameraClampX.Max);
            newPosition.z = Mathf.Clamp(newPosition.z, cameraConfig.cameraClampZ.Min, cameraConfig.cameraClampZ.Max);
            newPosition.y = currentY;

            mainCamera.transform.position = newPosition;
        }

        private void Rotation()
        {
            if (Input.GetMouseButton(2))
            {
                var mouseX = Input.GetAxis("Mouse X") * cameraConfig.sensitivity;
                var mouseY = Input.GetAxis("Mouse Y") * cameraConfig.sensitivity;

                if (mouseX != 0 || mouseY != 0)
                {
                    yRotation += mouseX;
                    xRotation -= mouseY;
                    xRotation = Mathf.Clamp(xRotation, cameraConfig.cameraRotatnionY.Min, cameraConfig.cameraRotatnionY.Max);

                    targetRotation = Quaternion.Euler(xRotation, yRotation, 0f);
                }

                var t = 1f - Mathf.Exp(-cameraConfig.rotationDamping * Time.deltaTime);
                mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, t);
            }
        }

        private void Scroll()
        {
            var scrollInput = Input.GetAxis("Mouse ScrollWheel");

            if (Mathf.Abs(scrollInput) > 0.01f)
                scrollVelocity += -scrollInput * cameraConfig.scrollSpeed;

            if (Mathf.Abs(scrollVelocity) > 0.001f)
            {
                var newPos = mainCamera.transform.position + new Vector3(0f, scrollVelocity * Time.deltaTime, 0f);
                newPos.y = Mathf.Clamp(newPos.y, cameraConfig.minZoom, cameraConfig.maxZoom);
                mainCamera.transform.position = newPos;
            }

            scrollVelocity = Mathf.Lerp(scrollVelocity, 0f, Time.deltaTime * cameraConfig.scrollDamping);
        }
    }
}