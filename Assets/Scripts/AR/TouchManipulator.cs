using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace SatwaLangka.AR
{
    /// <summary>
    /// Handles touch-based Y-axis rotation (1-finger swipe)
    /// and clamped pinch-to-scale (2-finger pinch) with surface grounding preservation.
    /// Supports New Input System, Unity Remote 5 (Legacy Input), and Editor Mouse.
    /// </summary>
    public class TouchManipulator : MonoBehaviour
    {
        [Header("Rotation (Y-axis only)")]
        [SerializeField] private float rotationSpeed = 0.45f;
        [SerializeField] private bool invertRotation = false;

        [Header("Zoom / Scale (Unlimited)")]
        [SerializeField] private float minScaleMultiplier = 0.01f;  // Hampir tidak ada batas kecil
        [SerializeField] private float maxScaleMultiplier = 50f;    // Hampir tidak ada batas besar

        private Vector2 previousTouchPosition;
        private bool hasPreviousTouch = false;
        private float initialPinchDistance;
        private Vector3 initialPinchScale;
        private Vector3 _baseScale = Vector3.one;
        private bool _baseScaleSet = false;
        private float _initialGroundY;
        private bool _groundYSet = false;

        public bool IsTouching { get; private set; }

#if UNITY_EDITOR
        private Mouse mouse;
        private Vector2 lastLegacyMousePos;
        private bool isLegacyMouseDown = false;
#endif

        private void OnEnable()
        {
            try
            {
                EnhancedTouchSupport.Enable();
            }
            catch {}

#if UNITY_EDITOR
            mouse = Mouse.current;
#endif
        }

        private void OnDisable()
        {
            try
            {
                EnhancedTouchSupport.Disable();
            }
            catch {}
        }

        private void Start()
        {
            if (!_baseScaleSet)
            {
                SetBaseScale(transform.localScale);
            }
        }

        public void SetBaseScale(Vector3 scale)
        {
            _baseScale = scale;
            _baseScaleSet = true;
            RecordGroundY();
        }

        private void RecordGroundY()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            if (renderers != null && renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                _initialGroundY = b.min.y;
                _groundYSet = true;
            }
        }

        void Update()
        {
            if (!_baseScaleSet)
            {
                _baseScale = transform.localScale;
                _baseScaleSet = true;
                RecordGroundY();
            }

            bool handled = false;

            // 1. Check New Input System active touches
            try
            {
                var touches = Touch.activeTouches;
                if (touches.Count > 0)
                {
                    handled = true;
                    IsTouching = true;

                    if (touches.Count == 1)
                    {
                        var t0 = touches[0];
                        bool began = t0.phase == UnityEngine.InputSystem.TouchPhase.Began;
                        HandleYRotation(t0.screenPosition, began);
                    }
                    else if (touches.Count >= 2)
                    {
                        var t0 = touches[0];
                        var t1 = touches[1];
                        bool began = t0.phase == UnityEngine.InputSystem.TouchPhase.Began || t1.phase == UnityEngine.InputSystem.TouchPhase.Began;
                        HandlePinchScale(t0.screenPosition, t1.screenPosition, began);
                    }
                }
            }
            catch {}

            // 2. Check Legacy Input touches (Unity Remote 5 / Mobile touch)
            if (!handled)
            {
                try
                {
                    int legacyTouchCount = UnityEngine.Input.touchCount;
                    if (legacyTouchCount > 0)
                    {
                        handled = true;
                        IsTouching = true;

                        if (legacyTouchCount == 1)
                        {
                            var t = UnityEngine.Input.GetTouch(0);
                            bool began = t.phase == UnityEngine.TouchPhase.Began;
                            HandleYRotation(t.position, began);
                        }
                        else if (legacyTouchCount >= 2)
                        {
                            var t0 = UnityEngine.Input.GetTouch(0);
                            var t1 = UnityEngine.Input.GetTouch(1);
                            bool began = t0.phase == UnityEngine.TouchPhase.Began || t1.phase == UnityEngine.TouchPhase.Began;
                            HandlePinchScale(t0.position, t1.position, began);
                        }
                    }
                }
                catch {}
            }

            if (!handled)
            {
                hasPreviousTouch = false;
                IsTouching = false;
            }

#if UNITY_EDITOR
            HandleEditorMouse();
#endif
        }

        private void HandleYRotation(Vector2 screenPos, bool began)
        {
            if (began || !hasPreviousTouch)
            {
                previousTouchPosition = screenPos;
                hasPreviousTouch = true;
                return;
            }

            float deltaX = screenPos.x - previousTouchPosition.x;
            if (Mathf.Abs(deltaX) > 0.05f)
            {
                float rotDir = invertRotation ? 1f : -1f;
                // Rotate strictly on Y axis in World space
                transform.Rotate(Vector3.up, deltaX * rotationSpeed * rotDir, Space.World);
            }
            previousTouchPosition = screenPos;
        }

        private void HandlePinchScale(Vector2 pos0, Vector2 pos1, bool began)
        {
            if (began || initialPinchDistance <= 0.001f)
            {
                initialPinchDistance = Vector2.Distance(pos0, pos1);
                initialPinchScale = transform.localScale;
                return;
            }

            float currentDistance = Vector2.Distance(pos0, pos1);
            if (initialPinchDistance > 0.01f)
            {
                float factor = currentDistance / initialPinchDistance;
                ApplyScaleWithGroundCompensation(initialPinchScale.x * factor);
            }
        }

        public void ApplyScaleWithGroundCompensation(float targetScalar)
        {
            float minS = _baseScale.x * minScaleMultiplier;
            float maxS = _baseScale.x * maxScaleMultiplier;
            float clamped = Mathf.Clamp(targetScalar, minS, maxS);

            // Preserve XYZ ratio dari baseScale agar model tidak distorted
            float ratio = _baseScale.x > 0.0001f ? clamped / _baseScale.x : 1f;
            transform.localScale = _baseScale * ratio;

            // Preserve ground contact so model feet never float or sink during scaling
            if (_groundYSet)
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
                if (renderers != null && renderers.Length > 0)
                {
                    Bounds b = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                    float currentMinY = b.min.y;
                    float deltaY = _initialGroundY - currentMinY;
                    transform.position += new Vector3(0, deltaY, 0);
                }
            }
        }

#if UNITY_EDITOR
        private void HandleEditorMouse()
        {
            // 1. Check Legacy Mouse (also forwarded by Unity Remote touch simulation)
            try
            {
                if (UnityEngine.Input.GetMouseButtonDown(0))
                {
                    lastLegacyMousePos = UnityEngine.Input.mousePosition;
                    isLegacyMouseDown = true;
                }
                else if (UnityEngine.Input.GetMouseButton(0) && isLegacyMouseDown)
                {
                    Vector2 currentPos = UnityEngine.Input.mousePosition;
                    float deltaX = currentPos.x - lastLegacyMousePos.x;
                    if (Mathf.Abs(deltaX) > 0.05f)
                    {
                        float rotDir = invertRotation ? 1f : -1f;
                        transform.Rotate(Vector3.up, deltaX * rotationSpeed * rotDir, Space.World);
                    }
                    lastLegacyMousePos = currentPos;
                }
                else if (UnityEngine.Input.GetMouseButtonUp(0))
                {
                    isLegacyMouseDown = false;
                }
            }
            catch {}

            // 2. Check New Input System PC Mouse
            if (mouse != null)
            {
                if (mouse.leftButton.isPressed)
                {
                    Vector2 delta = mouse.delta.ReadValue();
                    if (delta.sqrMagnitude > 0.01f)
                    {
                        transform.Rotate(Vector3.up, -delta.x * rotationSpeed * 12f, Space.World);
                    }
                }

                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    float currentScale = transform.localScale.x;
                    float newScale = currentScale + (scroll > 0 ? 0.05f : -0.05f) * _baseScale.x;
                    ApplyScaleWithGroundCompensation(newScale);
                }
            }
        }
#endif
    }
}
