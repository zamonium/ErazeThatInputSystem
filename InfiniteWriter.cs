using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MFE.Eraze
{
    public class InfiniteWriter : MonoBehaviour
    {
        #region Variables

        [SerializeField]
        public Pool m_kPool;

        private bool m_bActive;

        Vector2 v2CurrentPosition;
        eGesture eCurrentDirection;

        [SerializeField]
        private float m_fMinDistance;

        [SerializeField]
        private bool m_bShowDirection;
        [SerializeField]
        GameObject m_goArrow;

        private float m_fCooldownTime = 0.1f;
        private float m_fCooldown;
        private bool IsInGesture { get; set; }
        private float currentY;

        private Camera cam;

        #endregion

        private void Awake()
        {
            cam = Camera.main;
        }

        public void Init()
        {
            if (!m_bActive)
            {
                v2CurrentPosition = new Vector2(PlayerController.Instance.Position.x + 3f, GameData.LowLaneY + 0.1f);
                eCurrentDirection = eGesture.E;

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
                    currentY = cam.ScreenToWorldPoint(Input.mousePosition).y;
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
                            float fY = cam.ScreenToWorldPoint(Input.mousePosition).y;

                            float fOffset = fY - currentY;

                            if (Mathf.Abs(fOffset) > m_fMinDistance)
                            {
                                if (fOffset > 0)
                                    eCurrentDirection = eGesture.NE;
                                else
                                    eCurrentDirection = eGesture.SE;

                                currentY = fY;
                            }
                        }
                    }

                    m_fCooldown = Time.time + m_fCooldownTime;

                }

                if (v2CurrentPosition.x - PlayerController.Instance.Position.x < GameData.InfinityLaneDistance)
                    NewLine();
            }
        }

        private void NewLine()
        {
            LineCreator m_kLine;

            m_kLine = m_kPool.GetElement().GetComponent<LineCreator>();

            v2CurrentPosition += Vector2.right * m_kLine.m_fCurrentLength;

            if (eCurrentDirection == eGesture.NE && v2CurrentPosition.y > GameData.InfinityHighLaneY)
            {
                eCurrentDirection = eGesture.E;

                UIManager.Instance.ShowInputError(v2CurrentPosition + Vector2.right * m_kLine.m_fCurrentLength, eInputErrortype.WriteTooHigh);
            }

            if (eCurrentDirection == eGesture.SE && v2CurrentPosition.y < GameData.infinityLowLaneY)
            {
                eCurrentDirection = eGesture.E;

                UIManager.Instance.ShowInputError(v2CurrentPosition + Vector2.right * m_kLine.m_fCurrentLength, eInputErrortype.WriteTooLow);
            }

            m_kLine.CreateLine(v2CurrentPosition, eCurrentDirection, m_kPool);

            if (eCurrentDirection != eGesture.E && m_bShowDirection)
                AddArrow(eCurrentDirection);

            if (eCurrentDirection == eGesture.NE)
            {
                v2CurrentPosition += Vector2.up * m_kLine.m_fCurrentLength;

            }
            else if (eCurrentDirection == eGesture.SE)
            {
                v2CurrentPosition += Vector2.down * m_kLine.m_fCurrentLength;
            }

            eCurrentDirection = eGesture.E;
        }

        private void AddArrow(eGesture eDirection)
        {
            m_goArrow.transform.position = v2CurrentPosition;

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

    }
}