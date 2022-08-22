using GooglePlayGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFE.Eraze
{
    public class Writer : BaseInput
    {
        #region Variables

        [SerializeField]
        public Pool m_kPool;

        private int m_iLinesCurrent;

        private bool m_bActive;
        private float m_fCoolDown;

        private LineCreator m_kCurrentLine;

        private const float mk_fChunkLength = 3f;

        [SerializeField]
        GameObject m_LeftErrorPanel;

        [SerializeField]
        AudioSource audioSource;
        [SerializeField]
        List<AudioClip> writeAudio;

        #endregion

        public delegate void WriterCallback();
        public static event WriterCallback OnDrawLine;

        public bool CanWrite { get { return m_iLinesCurrent > 0; } }

        private void Start()
        {
            m_iLinesCurrent = GameData.MaxInputLines;

            UIManager.Instance.UpdateLinesUsable(m_iLinesCurrent);

            PlayerController.OnPowerUp += PowerUp;


            Activate(true);
        }

        public void Activate(bool bActive)
        {
            m_bActive = bActive;

            if (bActive && m_iLinesCurrent < GameData.MaxInputLines)
                m_fCoolDown = Time.time + 1 * Time.timeScale;
        }

        private void Update()
        {
            if (m_bActive && m_iLinesCurrent < GameData.MaxInputLines && Time.time > m_fCoolDown)
            {
                m_iLinesCurrent++;

                if (!UIManager.Instance.IsWriteInputUsable)
                    UIManager.Instance.WriteInputUsable();

                UIManager.Instance.IncreaseLinesUsable();

                m_fCoolDown = Time.time + 1 * Time.timeScale;
            }
        }


        private void PowerUp(bool bActive)
        {
            if (PlayerController.Instance.m_ePowerUp != ePowerUp.TimeWarp)
                return;

            if (m_fCoolDown > Time.time)
            {
                float fNewCooldown = m_fCoolDown - Time.time;

                if (bActive)
                {
                    fNewCooldown /= GameData.TimeWarpDivider;
                }
                else
                {
                    fNewCooldown *= GameData.TimeWarpDivider;
                }

                m_fCoolDown = Time.time + fNewCooldown;
            }
        }


        public override void Action(Vector2 v2BeginTouch, Vector3 v3BeginWorld, eGesture eDirection)
        {
            if (DrawingForward(eDirection))
            {
                if (CanDraw(v3BeginWorld))
                {
                    switch (eDirection)
                    {
                        case eGesture.N:
                            UIManager.Instance.ShowInputError(v3BeginWorld, eInputErrortype.WriteVerticalLineUp);
                            Debug.Log("Input error vertical line up");
                            break;

                        case eGesture.S:
                            UIManager.Instance.ShowInputError(v3BeginWorld, eInputErrortype.WriteVerticalLineDown);
                            Debug.Log("Input error vertical line down");


                            break;
                        default:
                            CheckLine(v3BeginWorld, eDirection);
                            break;
                    }
                }
                else
                {
                    UIManager.Instance.ShowInputError(v3BeginWorld, eInputErrortype.WriteTooCloseToScrib);
                    Debug.Log("Input error line forward too close to Scrib");

                    m_LeftErrorPanel.SetActive(true);
                    StartCoroutine(UiErrorDisable(m_LeftErrorPanel));
                }
            }
            else
            {
                if (CanDrawBack(v3BeginWorld))
                {
                    CheckLine(v3BeginWorld, eDirection);
                }
                else
                {
                    UIManager.Instance.ShowInputError(v3BeginWorld, eInputErrortype.WriteTooCloseToScrib);
                    Debug.Log("Input error line backward too close to Scrib");

                    m_LeftErrorPanel.SetActive(true);
                    StartCoroutine(UiErrorDisable(m_LeftErrorPanel));
                }
            }
        }

        IEnumerator UiErrorDisable(GameObject go)
        {
            yield return new WaitForSeconds(0.1f * Time.timeScale);

            go.SetActive(false);
        }

        private void CheckLine(Vector2 v2NewPosition, eGesture eDirection)
        {
            float fY = GetGroundY(v2NewPosition.y);

            if (fY != 0)
            {
                v2NewPosition = new Vector2(v2NewPosition.x, fY + 0.1f);
            }
            else
            {
                if (v2NewPosition.y > 0)
                {
                    UIManager.Instance.ShowInputError(v2NewPosition, eInputErrortype.WriteTooHigh);
                    Debug.Log("Input error line too high");

                }
                else
                {
                    UIManager.Instance.ShowInputError(v2NewPosition, eInputErrortype.WriteTooLow);
                    Debug.Log("Input error line too low");

                }

                return;
            }


            float fX = GetXOffset(v2NewPosition);

            if (fX != 1 && fX != -1)
            {
                v2NewPosition += Vector2.right * fX;

                if (DrawingForward(eDirection))
                {
                    if (!CanDraw(v2NewPosition))
                    {
                        UIManager.Instance.ShowInputError(v2NewPosition - Vector2.right * fX, eInputErrortype.NoWriteAnchor);
                        Debug.Log("Input error no anchor");

                        return;
                    }
                }
                else
                {
                    if (!CanDrawBack(v2NewPosition))
                    {
                        UIManager.Instance.ShowInputError(v2NewPosition - Vector2.right * fX, eInputErrortype.NoWriteAnchor);
                        Debug.Log("Input error no anchor");

                        return;
                    }
                }
            }
            else
            {
                //make maybe ui signal that can't write without anchors

                //or even better add a bool check to see if it can write 
                //and in this case Anchorpos = pos.x mod 3 if <=1.5 else (pox mod 3) + 1 

                UIManager.Instance.ShowInputError(v2NewPosition, eInputErrortype.NoWriteAnchor);
                Debug.Log("Input error no anchor");

                return;
            }

            m_iLinesCurrent--;
            UIManager.Instance.DecreaseLinesUsable();

            if (m_iLinesCurrent == 0)
                UIManager.Instance.WriteInputUnusable();

            m_fCoolDown = Time.time + 1 * Time.timeScale;

            m_kCurrentLine = m_kPool.GetElement().GetComponent<LineCreator>();
            m_kCurrentLine.CreateLine(v2NewPosition, eDirection, m_kPool);

            if (audioSource)
            {
                AudioClip toBePlayed = writeAudio[Random.Range(0, writeAudio.Count)];

                audioSource.PlayOneShot(toBePlayed);
            }

            DataManager.playerData.iLinesDrawed++;

            if (OnDrawLine != null)
                OnDrawLine();
        }

        private float GetGroundY(float fYPos)
        {
            float result = 0;

            if (fYPos >= GameData.HighLaneY - GameData.LaneDistance / 3 && fYPos < GameData.HighLaneY + 2 * GameData.LaneDistance / 3)
                result = GameData.HighLaneY;
            else if (fYPos >= GameData.MediumLaneY - GameData.LaneDistance / 3 && fYPos < GameData.MediumLaneY + 2 * GameData.LaneDistance / 3)
                result = GameData.MediumLaneY;
            else if (fYPos >= GameData.LowLaneY - GameData.LaneDistance / 3 && fYPos < GameData.LowLaneY + 2 * GameData.LaneDistance / 3)
                result = GameData.LowLaneY;

            return result;
        }

        private float GetXOffset(Vector2 v2Pos)
        {
            float leftPos = -1;
            float rightPos = 1;

            RaycastHit2D hit, hit2;

            hit = Physics2D.Raycast(v2Pos, Vector2.left, mk_fChunkLength, LayerMask.GetMask("Anchor"));
            Debug.DrawRay(v2Pos, Vector2.left, Color.red, 10);

            if (hit)
            {
                if (v2Pos.x >= hit.transform.position.x)
                {
                    leftPos = hit.transform.position.x;
                }

            }

            hit2 = Physics2D.Raycast(v2Pos, Vector2.right, mk_fChunkLength, LayerMask.GetMask("Anchor"));
            Debug.DrawRay(v2Pos, Vector2.right, Color.blue, 10);

            if (hit2)
            {
                if (v2Pos.x < hit2.transform.position.x)
                {
                    rightPos = hit2.transform.position.x;
                }

            }

            if (!hit && !hit2)
            {
                Debug.Log("Error - no anchor found");
                return 1;
            }

            if (rightPos == 1)
            {
                return leftPos - v2Pos.x;
            }
            else if (leftPos == -1)
            {
                return rightPos - v2Pos.x;
            }

            if (rightPos - v2Pos.x < v2Pos.x - leftPos)
                return rightPos - v2Pos.x;
            else
                return leftPos - v2Pos.x;
        }

        protected bool DrawingForward(eGesture eDirection)
        {
            bool bForward = true;

            if (eDirection == eGesture.NW || eDirection == eGesture.W || eDirection == eGesture.SW)
                bForward = false;

            return bForward;
        }

        public void Reset()
        {
            m_kCurrentLine = null;

            m_kPool.RestoreAll();

            m_iLinesCurrent = GameData.MaxInputLines;

            UIManager.Instance.UpdateLinesUsable(m_iLinesCurrent);
        }

        private void OnDestroy()
        {
            PlayerController.OnPowerUp -= PowerUp;
        }
    }
}