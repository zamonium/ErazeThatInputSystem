using UnityEngine;
using UnityEngine.EventSystems;

namespace MFE.Eraze
{
    public enum eGesture
    {
        E = 0,
        NE,
        N,
        NW,
        W,
        SW,
        S,
        SE,
        Invalid
    }

    public enum eInputErrortype
    {
        NoEraseInput = 0,
        NoWriteInput = 1,
        NoWriteAnchor = 2,
        WriteTooCloseToScrib = 3,
        WriteTooHigh = 4,
        WriteTooLow = 5,
        WriteVerticalLineUp = 6,
        WriteVerticalLineDown = 7
    }

    public class InputController : MonoBehaviour
    {
        public delegate void InputCallback();
        public static event InputCallback OnChangeMechanic;

        private void Awake()
        {
            m_camera = Camera.main;
        }

        public void Init()
        {
            m_bIsInGesture = false;

            m_fMinGestureDistance = Screen.width / 20; 
            Debug.Log("Minimum gesture distance " + m_fMinGestureDistance);

            ResetPositions();

            m_kCurrent = m_kWriter;
        }

        public void Activate(bool bActivate)
        {
            m_bActive = bActivate;

            if (UIManager.Instance.NormalInput)
            {
                UIManager.Instance.ActivateMechanic(m_bActive);

                if (!bActivate && GameManager.m_eState == eGameState.EndGame)
                    StopAction();
            }
            else
                UIManager.Instance.ActivateMechanicInfiniteLine(m_bActive);
        }

        public void PrepareForInfiniteLine()
        {
            StopAction();

            m_bActive = false;

            UIManager.Instance.ActivateMechanic(false);
            UIManager.Instance.ShowInfiniteLineContainer();
        }

        public void StartInfiniteLine()
        {
            m_bActive = true;

            SpecialInputActive = true;

            UIManager.Instance.ActivateMechanicInfiniteLine(true);

            if (m_kInfiniteLine)
            {
                m_kInfiniteLine.Init();
            }
        }

        public void StopInfiniteLine()
        {
            SpecialInputActive = false;

            if (m_kInfiniteLine)
            {
                m_kInfiniteLine.Stop();
            }

            UIManager.Instance.HideInfiniteLineContainer();

            UIManager.Instance.ActivateMechanic(true);
        }

        public void ResetInfiniteLine()
        {
            if (m_kInfiniteLine)
            {
                m_kInfiniteLine.ResetLine();
            }
        }

        void Update()
        {
            if (Active)
            {
                if (IsOverUI())
                {
                    if (m_bIsInGesture)
                    {
                        StopAction();
                    }

                    return;
                }

                if (SpecialInputActive)
                    return;

                if (Input.GetMouseButtonDown(0))
                {
                    SaveFirstPosition();

                    m_fTimeStart = Time.time;
                }
                else if (Input.GetMouseButton(0) && !m_bIsInGesture)
                {
                    if (m_v2BeginTouchPosition == Vector2.zero)
                    {
                        SaveFirstPosition();
                    }

                    if ((m_v2BeginTouchPosition != (Vector2)Input.mousePosition && Vector2.Distance(m_v2BeginTouchPosition, Input.mousePosition) >= m_fMinGestureDistance)
                        || Time.time > m_fTimeStart + m_fMaxTapTime)
                    {
                        ExecuteAction();
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (!m_bIsInGesture)
                    {
                        SaveLastPosition();

                        CheckTap();

                        if (m_kCurrent == m_kEraser)
                            m_kEraser.StopErase();
                    }
                    else
                    {
                        StopAction();
                    }
                }
            }
        }

        private void StopAction()
        {
            m_bIsInGesture = false;

            if (m_kCurrent == m_kEraser)
                m_kEraser.StopErase();

            ResetPositions();
        }

        private void SaveFirstPosition()
        {
            m_v2BeginTouchPosition = Input.mousePosition;
            m_v3BeginWorldPosition = m_camera.ScreenToWorldPoint(m_v2BeginTouchPosition);
            m_v3BeginWorldPosition += Vector3.forward * 10;
        }

        private void SaveLastPosition()
        {
            m_v2EndTouchPosition = Input.mousePosition;
        }

        private void ExecuteAction()
        {
            SaveLastPosition();

            m_bIsInGesture = true;

            ActionMechanic();
        }

        private eGesture CheckGesture()
        {
            eGesture eRecognizedGesture;

            if (m_v2BeginTouchPosition != m_v2EndTouchPosition && Vector2.Distance(m_v2BeginTouchPosition, m_v2EndTouchPosition) >= m_fMinGestureDistance)
            {
                eRecognizedGesture = getDirection();

                Debug.Log("Gesture " + eRecognizedGesture);
                Vector3 rayWorld = m_camera.ScreenToWorldPoint(m_v2BeginTouchPosition);

                Vector3 oldRubberPosition = new Vector3(rayWorld.x, rayWorld.y, 0);

                rayWorld = m_camera.ScreenToWorldPoint(m_v2EndTouchPosition);

                Vector3 newRubberPosition = new Vector3(rayWorld.x, rayWorld.y, 0);

                Debug.DrawLine(oldRubberPosition, newRubberPosition, Color.red, 10);
            }
            else
            {
                Debug.Log("Distance too small for gesture");

                eRecognizedGesture = eGesture.Invalid;
            }

            return eRecognizedGesture;
        }

        private void ResetPositions()
        {
            m_v2BeginTouchPosition = Vector2.zero;
            m_v2EndTouchPosition = Vector2.zero;

            m_v3BeginWorldPosition = Vector3.zero;

            m_fTimeStart = 0;
        }

        protected virtual eGesture getDirection()
        {
            Vector3 vTo = new Vector3(m_v2EndTouchPosition.x, m_v2EndTouchPosition.y, 0);
            Vector3 vFrom = new Vector3(m_v2BeginTouchPosition.x, m_v2BeginTouchPosition.y, 0);
            Vector3 vDirection = vTo - vFrom;

            float fAngle = getAngle(Vector3.right, Vector3.forward, vDirection);
            if (fAngle < 0)
                fAngle += 360f;

            eGesture eResult = (eGesture)(int)((fAngle + 22.5f) % 360 / 45);

            return eResult;
        }

        protected static float getAngle(Vector3 v3OldDir, Vector3 v3Up, Vector3 v3NewDir)
        {
            Vector3 v3Right = Vector3.Cross(v3Up, v3OldDir);
            float fAngle = Vector3.Angle(v3NewDir, v3OldDir);
            float fSign = (Vector3.Dot(v3NewDir, v3Right) > 0.0f) ? 1.0f : -1.0f;
            return (fSign * fAngle);
        }

        private void ActionMechanic()
        {
            if (m_kCurrent == m_kWriter)
            {
                if (m_kWriter.CanWrite)
                {
                    eGesture eRecognizedGesture = CheckGesture();

                    if (eRecognizedGesture != eGesture.Invalid)
                        m_kWriter.Action(m_v2BeginTouchPosition, m_v3BeginWorldPosition, eRecognizedGesture);
                }
                else
                {
                    UIManager.Instance.ShowInputError(m_v3BeginWorldPosition, eInputErrortype.NoWriteInput);

                    Debug.Log("Input error no input lines line");
                }
            }
            else
            {
                m_kEraser.Action(m_v3BeginWorldPosition);
            }

            ResetPositions();
        }

        protected virtual void CheckTap()
        {
            if (Time.time - m_fTimeStart <= m_fMaxTapTime &&
                Vector2.Distance(m_v2BeginTouchPosition, m_v2EndTouchPosition) < m_fMinGestureDistance)
            {
                Debug.Log("Tap " + Vector2.Distance(m_v2BeginTouchPosition, m_v2EndTouchPosition));

                ChangeMechanic();
            }

            ResetPositions();
        }

        private void ChangeMechanic()
        {
            if (m_audioSource && GameManager.m_eState == eGameState.Playing)
                m_audioSource.PlayOneShot(m_changeMechanicAudio);

            if (m_kCurrent == m_kWriter)
                m_kCurrent = m_kEraser;
            else
                m_kCurrent = m_kWriter;

            UIManager.Instance.UpdateMechanic(m_kCurrent == m_kWriter);

            if (OnChangeMechanic != null)
                OnChangeMechanic();
        }

        private bool IsOverUI()
        {
            bool bResult = false;

#if UNITY_EDITOR

            bResult = EventSystem.current.IsPointerOverGameObject();
#endif

#if UNITY_IOS || UNITY_ANDROID

            if (Input.touchCount > 0)
            {
                int id = Input.touches[0].fingerId;
                if (EventSystem.current.IsPointerOverGameObject(id))
                {
                    bResult = true;
                }
            }

#endif

            return bResult;
        }

        public void ResetInput()
        {
            m_bActive = true;

            ResetPositions();

            m_kWriter.Reset();

            m_kEraser.Reset();

            if (m_kCurrent == m_kEraser)
                ChangeMechanic();
        }

        #region Variables & Properties

        public bool Active { get { return m_bActive; } }
        public bool SpecialInputActive { get; set; }

        [SerializeField]
        private Eraser m_kEraser;
        [SerializeField]
        private Writer m_kWriter;
        [SerializeField]
        private InfiniteWriter m_kInfiniteLine;

        private BaseInput m_kCurrent;

        private Camera m_camera;

        [SerializeField]
        private AudioSource m_audioSource;
        [SerializeField]
        private AudioClip m_changeMechanicAudio;

        protected Vector2 m_v2BeginTouchPosition, m_v2EndTouchPosition;
        private Vector3 m_v3BeginWorldPosition;

        [SerializeField]
        private float m_fMaxTapTime;

        private float m_fTimeStart;
        private float m_fMinGestureDistance;

        private bool m_bActive;
        private bool m_bIsInGesture;

        #endregion
    }
}