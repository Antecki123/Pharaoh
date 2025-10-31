using UnityEngine;

namespace Views.Ui.Buildings
{
    public abstract class BuildingTooltipUI : MonoBehaviour
    {
        protected Transform buildingTransform;
        private RectTransform rectTransform;
        private Camera mainCamera;

        [Header("Config")]
        private Vector3 worldOffset = new Vector3(0f, 0f, 0f);
        private Vector2 screenOffset = new Vector2(100f, -150f);
        private Vector2 horizontalScreenEdgeOffset = new Vector2(20f, 20f);
        private Vector2 veritcalScreenEdgeOffset = new Vector2(120f, 20f);
        private float edgeMarginPercent = 0.2f;

        private int currentSide = 0;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            mainCamera = Camera.main;
        }

        private void Update()
        {
            var buildingPos = buildingTransform.position + worldOffset;
            var screenPos = mainCamera.WorldToScreenPoint(buildingPos);

            if (screenPos.z < 0)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            float screenMargin = Screen.width * edgeMarginPercent;

            if (screenPos.x < screenMargin)
                currentSide = -1;
            else if (screenPos.x > Screen.width - screenMargin)
                currentSide = 1;

            float adjustedXOffset = screenOffset.x;
            if (currentSide == -1)
                adjustedXOffset = Mathf.Abs(screenOffset.x);
            else if (currentSide == 1)
                adjustedXOffset = -Mathf.Abs(screenOffset.x);

            var x = screenPos.x + adjustedXOffset;
            var y = screenPos.y + screenOffset.y;

            var halfWidth = rectTransform.rect.width * 0.5f;
            var halfHeight = rectTransform.rect.height * 0.5f;

            x = Mathf.Clamp(x, halfWidth + horizontalScreenEdgeOffset.x,
                               Screen.width - halfWidth - horizontalScreenEdgeOffset.y);
            y = Mathf.Clamp(y, halfHeight + veritcalScreenEdgeOffset.x,
                               Screen.height - halfHeight - veritcalScreenEdgeOffset.y);

            rectTransform.position = new Vector3(x, y, 0f);
        }
    }
}