using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFE.Eraze
{
    public class LineCreator : MonoBehaviour
    {
        private Pool m_kPool;

        private LineRenderer m_kLineRenderer;
        private EdgeCollider2D m_kEdgeCollider;

        private List<Vector2> m_lv2FingerPosition;

        private bool m_bDrawing;
        private Vector2 m_v2Direction;
        private eGesture m_eDirection;

        [SerializeField]
        bool bDinamicCreation;

        [SerializeField]
        private bool m_bNormalLength;

        [HideInInspector]
        public float m_fCurrentLength;

        [Header("Anchors")]
        [SerializeField]
        private bool bAnchors;
        [SerializeField]
        private GameObject m_goAnchor;

        public float currentY { get { return m_lv2FingerPosition[0].y; } }


        void Awake()
        {
            m_lv2FingerPosition = new List<Vector2>();

            m_fCurrentLength = m_bNormalLength ? GameData.LaneDistance : GameData.LaneDistance / 2;

            m_kLineRenderer = GetComponent<LineRenderer>();
            m_kEdgeCollider = GetComponent<EdgeCollider2D>();
        }

        private void Update()
        {
            if (GameManager.m_eState != eGameState.Playing)
                return;

            if (m_bDrawing)
            {
                m_lv2FingerPosition[1] += m_v2Direction * 0.5f;
                m_kLineRenderer.SetPosition(1, m_lv2FingerPosition[1]);

                m_kEdgeCollider.points = m_lv2FingerPosition.ToArray();

                if (Vector2.Distance(m_lv2FingerPosition[0], m_lv2FingerPosition[1]) > m_fCurrentLength)
                {
                    if (m_eDirection == eGesture.E)
                        m_lv2FingerPosition[1] = m_lv2FingerPosition[0] + Vector2.right * m_fCurrentLength;
                    else if (m_eDirection == eGesture.NE)
                        m_lv2FingerPosition[1] = m_lv2FingerPosition[0] + Vector2.right * m_fCurrentLength + Vector2.up * m_fCurrentLength;
                    else if (m_eDirection == eGesture.SE)
                        m_lv2FingerPosition[1] = m_lv2FingerPosition[0] + Vector2.right * m_fCurrentLength + Vector2.down * m_fCurrentLength;
                    else if (m_eDirection == eGesture.W)
                        m_lv2FingerPosition[1] = m_lv2FingerPosition[0] + Vector2.left * m_fCurrentLength;
                    else if (m_eDirection == eGesture.NW)
                        m_lv2FingerPosition[1] = m_lv2FingerPosition[0] + Vector2.left * m_fCurrentLength + Vector2.up * m_fCurrentLength;
                    else if (m_eDirection == eGesture.SW)
                        m_lv2FingerPosition[1] = m_lv2FingerPosition[0] + Vector2.left * m_fCurrentLength + Vector2.down * m_fCurrentLength;

                    m_kLineRenderer.SetPosition(1, m_lv2FingerPosition[1]);

                    m_kEdgeCollider.points = m_lv2FingerPosition.ToArray();

                    if (bAnchors)
                        m_goAnchor.transform.position = m_lv2FingerPosition[1] + Vector2.down * 0.05f;

                    m_bDrawing = false;
                }
            }
        }

        public void CreateLine(Vector2 v2NewPosition, eGesture eDirection, Pool kPool)
        {
            m_kPool = kPool;

            m_lv2FingerPosition.Clear();
            m_lv2FingerPosition.Add(v2NewPosition);

            if (bDinamicCreation)
            {
                m_lv2FingerPosition.Add(v2NewPosition);

                m_kLineRenderer.SetPosition(0, m_lv2FingerPosition[0]);
                m_kLineRenderer.SetPosition(1, m_lv2FingerPosition[1]);

                m_kEdgeCollider.points = m_lv2FingerPosition.ToArray();

                m_v2Direction = GetDirection(eDirection);
                m_eDirection = eDirection;

                m_bDrawing = true;
            }
            else
            {
                if (eDirection == eGesture.E)
                    m_lv2FingerPosition.Add(v2NewPosition + Vector2.right * m_fCurrentLength);
                else if (eDirection == eGesture.NE)
                    m_lv2FingerPosition.Add(v2NewPosition + Vector2.right * m_fCurrentLength + Vector2.up * m_fCurrentLength);
                else if (eDirection == eGesture.SE)
                    m_lv2FingerPosition.Add(v2NewPosition + Vector2.right * m_fCurrentLength + Vector2.down * m_fCurrentLength);
                else if (eDirection == eGesture.W)
                    m_lv2FingerPosition.Add(v2NewPosition + Vector2.left * m_fCurrentLength);
                else if (eDirection == eGesture.NW)
                    m_lv2FingerPosition.Add(v2NewPosition + Vector2.left * m_fCurrentLength + Vector2.up * m_fCurrentLength);
                else if (eDirection == eGesture.SW)
                    m_lv2FingerPosition.Add(v2NewPosition + Vector2.left * m_fCurrentLength + Vector2.down * m_fCurrentLength);

                m_kLineRenderer.SetPosition(0, m_lv2FingerPosition[0]);
                m_kLineRenderer.SetPosition(1, m_lv2FingerPosition[1]);

                m_kEdgeCollider.points = m_lv2FingerPosition.ToArray();

                if (bAnchors)
                    m_goAnchor.transform.position = m_lv2FingerPosition[1] + Vector2.down * 0.05f;
            }
        }

        private Vector2 GetDirection(eGesture eDirection)
        {
            Vector2 v2Direction = Vector2.zero;

            if (eDirection == eGesture.NE || eDirection == eGesture.E || eDirection == eGesture.SE)
                v2Direction.x = 1;
            else if (eDirection == eGesture.SW || eDirection == eGesture.W || eDirection == eGesture.NW)
                v2Direction.x = -1;

            if (eDirection == eGesture.NE || eDirection == eGesture.NW)
                v2Direction.y = 1;
            else if (eDirection == eGesture.SE || eDirection == eGesture.SW)
                v2Direction.y = -1;

            return v2Direction;
        }

        private void OnBecameInvisible()
        {
            if (isActiveAndEnabled && PlayerController.Instance && PlayerController.Instance.transform.position.x - 1.5f > transform.position.x)
            {
                m_kPool.RestoreElement(gameObject);
            }
        }
    }
}