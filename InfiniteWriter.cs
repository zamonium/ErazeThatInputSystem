using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MFE.Eraze
{
    public class InfiniteWriter : MonoBehaviour
    {
        private void Awake()
        {
            m_camera = Camera.main;
        }

        public void Init()
        {
            if (!m_bActive)
            {
                m_v2CurrentPosition = new Vector2(PlayerController.Instance.Position.x + 3f, GameData.LowLaneY + 0.1f);
                m_eCurrentDirection = eGesture.E;

                NewLine();

                m_fCooldown = Time.time + m_fCooldownTime;

                m_bActive = true;

                if (m_goArrow)
                    m_goArrow.SetActive(false);

                Debug.Log("Init infinite line");
            }
        }

        public void Stop()
        {
            m_bActive = false;
        }

        public void ResetLine()
        {
            m_kPool.RestoreAll();

            if (m_goArrow)
                m_goArrow.SetActive(false);
        }

        private void Update()
        {
            if (m_bActive && GameManager.m_eState == eGameState.Playing)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    IsInGesture = true;
                    currentY = m_camera.ScreenToWorldPoint(Input.mousePosition).y;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    IsInGesture = false;
                }

                if (Time.time > m_fCooldown)
                {
                    if (!IsOverUI())
                    {
                        if (Input.GetMouseButton(0) && IsInGesture)
                        {
                            float fY = m_camera.ScreenToWorldPoint(Input.mousePosition).y;

                            float fOffset = fY - currentY;

                            if (Mathf.Abs(fOffset) > m_fMinDistance)
                            {
                                if (fOffset > 0)
                                    m_eCurrentDirection = eGesture.NE;
                                else
                                    m_eCurrentDirection = eGesture.SE;

                                currentY = fY;
                            }
                        }
                    }

                    m_fCooldown = Time.time + m_fCooldownTime;
                }

                if (m_v2CurrentPosition.x - PlayerController.Instance.Position.x < GameData.InfinityLaneDistance)
                    NewLine();
            }
        }

        private void NewLine()
        {
            LineCreator m_kLine;

            m_kLine = m_kPool.GetElement().GetComponent<LineCreator>();

            m_v2CurrentPosition += Vector2.right * m_kLine.m_fCurrentLength;

            if (m_eCurrentDirection == eGesture.NE && m_v2CurrentPosition.y > GameData.InfinityHighLaneY)
            {
                m_eCurrentDirection = eGesture.E;

                UIManager.Instance.ShowInputError(m_v2CurrentPosition + Vector2.right * m_kLine.m_fCurrentLength, eInputErrortype.WriteTooHigh);
            }

            if (m_eCurrentDirection == eGesture.SE && m_v2CurrentPosition.y < GameData.infinityLowLaneY)
            {
                m_eCurrentDirection = eGesture.E;

                UIManager.Instance.ShowInputError(m_v2CurrentPosition + Vector2.right * m_kLine.m_fCurrentLength, eInputErrortype.WriteTooLow);
            }

            m_kLine.CreateLine(m_v2CurrentPosition, m_eCurrentDirection, m_kPool);

            if (m_eCurrentDirection != eGesture.E && m_bShowDirection)
                AddArrow(m_eCurrentDirection);

            if (m_eCurrentDirection == eGesture.NE)
            {
                m_v2CurrentPosition += Vector2.up * m_kLine.m_fCurrentLength;

            }
            else if (m_eCurrentDirection == eGesture.SE)
            {
                m_v2CurrentPosition += Vector2.down * m_kLine.m_fCurrentLength;
            }

            m_eCurrentDirection = eGesture.E;
        }

        private void AddArrow(eGesture eDirection)
        {
            m_goArrow.transform.position = m_v2CurrentPosition;

            if (eDirection == eGesture.NE)
            {
                m_goArrow.transform.position += Vector3.up * 1.2f;

                m_goArrow.transform.rotation = Quaternion.identity;
            }
            else
            {
                m_goArrow.transform.position += Vector3.down * 1.2f;

                m_goArrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180f));
            }

            m_goArrow.SetActive(true);

            StartCoroutine(ArrowCoroutine());
        }

        IEnumerator ArrowCoroutine()
        {
            int timer = 5;
            while (timer > 0)
            {
                while (GameManager.m_eState == eGameState.Pause)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                yield return new WaitForSeconds(0.1f);
                timer -= 1;
            }

            m_goArrow.SetActive(false);
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

        #region Variables & Properties

        private bool IsInGesture { get; set; }

        [SerializeField]
        public Pool m_kPool;

        private Camera m_camera;

        [SerializeField]
        private GameObject m_goArrow;

        private Vector2 m_v2CurrentPosition;
        private eGesture m_eCurrentDirection;

        [SerializeField]
        private float m_fMinDistance;

        private float m_fCooldownTime = 0.1f;
        private float m_fCooldown;
        private float currentY;

        [SerializeField]
        private bool m_bShowDirection;

        private bool m_bActive;

        #endregion
    }
}