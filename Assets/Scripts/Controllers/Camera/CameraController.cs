using UnityEngine;
using Zenject;

namespace Controllers
{
    public class CameraController : IInitializable, ILateTickable
    {
        private Camera mainCamera;

        private float movementSpeed = 4f;
        private float acceleration = 10f;
        private Vector3 currentVelocity;

        private float sensitivity = 600f;
        private float rotationDamping = 20f;
        private float xRotation;
        private float yRotation;
        private Quaternion targetRotation;

        private float scrollSpeed = 50f;
        private float scrollDamping = 15f;
        private float scrollVelocity;

        public void Initialize()
        {
            mainCamera = Camera.main;
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
            Vector3 targetVelocity = Vector3.zero;

            var speedModifier = Input.GetKey(KeyCode.LeftShift) ? movementSpeed * 3 : movementSpeed;
            var movementInputX = Input.GetAxisRaw("Horizontal");
            var movementInputZ = Input.GetAxisRaw("Vertical");

            var currentY = mainCamera.transform.position.y;

            if (movementInputX != 0f)
                targetVelocity += mainCamera.transform.right * movementInputX;

            if (movementInputZ != 0f)
            {
                var forwardFlat = mainCamera.transform.forward;
                forwardFlat.y = 0f;
                forwardFlat.Normalize();
                targetVelocity += forwardFlat * movementInputZ;
            }

            if (targetVelocity != Vector3.zero)
                targetVelocity = targetVelocity.normalized * speedModifier;

            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);

            mainCamera.transform.position += currentVelocity * Time.deltaTime;
            mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, currentY, mainCamera.transform.position.z);
        }

        private void Rotation()
        {
            if (Input.GetMouseButton(2))
            {
                var mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
                var mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

                if (mouseX == 0 && mouseY == 0)
                    return;

                yRotation += mouseX;
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, 20f, 90f);

                targetRotation = Quaternion.Euler(xRotation, yRotation, 0f);
            }

            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * rotationDamping);
        }

        private void Scroll()
        {
            var scrollInput = Input.GetAxis("Mouse ScrollWheel");
            var minY = 2f;
            var maxY = 20f;

            if (Mathf.Abs(scrollInput) > 0.01f)
                scrollVelocity += -scrollInput * scrollSpeed;

            if (Mathf.Abs(scrollVelocity) > 0.001f)
            {
                var newPos = mainCamera.transform.position + new Vector3(0f, scrollVelocity * Time.deltaTime, 0f);
                newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
                mainCamera.transform.position = newPos;
            }

            scrollVelocity = Mathf.Lerp(scrollVelocity, 0f, Time.deltaTime * scrollDamping);
        }
    }
}