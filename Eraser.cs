using UnityEngine;

namespace MFE.Eraze
{
    public class Eraser : BaseInput
    {
        public delegate void EraserCallback();
        public static event EraserCallback OnFinishRubberPoints;

        private void Start()
        {
            m_camera = Camera.main;

            BaseEnemy.OnEnemyDisabled += RestoreElements;
            PlayerController.OnPowerUp += PowerUp;

            if (particle)
                particle.SetActive(false);

            m_fMaxTimeRubber = m_fDefaultMaxTimeRubber;
            m_fCurrentUseRubber = m_fMaxTimeRubber;
            m_fCooldownTime = m_fMaxTimeRubber / 2f;

            UpdateUIEraser();
        }

        private void UpdateUIEraser()
        {
            UIManager.Instance.SetMaxValueEraseSlider = m_fMaxTimeRubber;
            UIManager.Instance.SetEraseSlider = m_fCurrentUseRubber;
        }

        public override void Action(Vector3 v3Begin)
        {
            //Debug.Log("Action Eraser Called");

            if (particle)
                particle.SetActive(true);

            if (m_fCurrentUseRubber > 0)
            {
                m_v3OldRubberPosition = v3Begin;

                m_bCanErase = true;
            }
            else
            {
                UIManager.Instance.ShowInputError(v3Begin, eInputErrortype.NoEraseInput);
                Debug.Log("Input error no more rubber");
            }
        }

        private void Update()
        {
            if (GameManager.m_eState != eGameState.Playing)
                return;

            Vector3 v3NewRubberPosition = m_camera.ScreenToWorldPoint(Input.mousePosition);
            v3NewRubberPosition += Vector3.forward * 10; //to get it to z = 0

            if (m_bCanErase && m_fCurrentUseRubber > 0 && Vector2.Distance(m_v3OldRubberPosition, v3NewRubberPosition) > m_fDistanceBetweenRubberPoint)
            {
                if (!m_bIsErasing)
                {
                    CheckEnemy(m_v3OldRubberPosition);

                    FillRubberPoints(m_v3OldRubberPosition, v3NewRubberPosition);

                    m_v3OldRubberPosition = v3NewRubberPosition;
                }
            }

            if (m_bCanErase)
            {
                m_fCurrentUseRubber -= Time.deltaTime;

                if (m_fCurrentUseRubber <= 0)
                {
                    m_fCurrentUseRubber = 0;

                    m_fCooldown = Time.time + m_fCooldownTime;

                    UIManager.Instance.EraseInputUnusable();

                    UIManager.Instance.ShowInputError(v3NewRubberPosition, eInputErrortype.NoEraseInput);
                    Debug.Log("Input error rubber finished");

                    StopErase();
                }
            }
            else
            {
                if (m_fCurrentUseRubber >= m_fMaxTimeRubber)
                {
                    m_fCurrentUseRubber = m_fMaxTimeRubber;
                    UIManager.Instance.SetEraseSlider = m_fCurrentUseRubber;
                    return;
                }

                if (m_fCooldown > Time.time)
                {
                    return;
                }
                else
                {
                    if (m_fCooldown != 0)
                    {
                        UIManager.Instance.EraseInputUsable();

                        m_fCooldown = 0;
                    }
                }

                m_fCurrentUseRubber += Time.deltaTime;
            }

            UIManager.Instance.SetEraseSlider = m_fCurrentUseRubber;
        }

        private void PowerUp(bool bActive)
        {
            if (PlayerController.Instance.m_ePowerUp != ePowerUp.TimeWarp)
                return;

            if (bActive)
            {
                m_fMaxTimeRubber = m_fTimeWarpMaxTimeRubber;
                m_fCurrentUseRubber *= m_fTimeWarpMaxTimeRubber;

                if (m_fCooldown <= Time.time)
                {
                    float fStartTime = m_fCooldown - m_fCooldownTime;
                    float fInPowerUpFor = Time.time - fStartTime;

                    float fIncrement = m_fTimeWarpMaxTimeRubber - ((fInPowerUpFor * m_fTimeWarpMaxTimeRubber) / m_fDefaultMaxTimeRubber);
                    float fNewCoolDown = fInPowerUpFor + fIncrement;

                    m_fCooldown = fStartTime + fNewCoolDown;
                }

                m_fCooldownTime = m_fTimeWarpMaxTimeRubber / 2f;
            }
            else
            {
                m_fMaxTimeRubber = m_fDefaultMaxTimeRubber;
                m_fCurrentUseRubber /= m_fTimeWarpMaxTimeRubber;

                if (m_fCooldown <= Time.time)
                {
                    float fStartTime = m_fCooldown - m_fCooldownTime;
                    float fInPowerUpFor = Time.time - fStartTime;

                    float fIncrement = m_fDefaultMaxTimeRubber - ((fInPowerUpFor * m_fDefaultMaxTimeRubber) / m_fTimeWarpMaxTimeRubber);
                    float fNewCoolDown = fInPowerUpFor + fIncrement;

                    m_fCooldown = fStartTime + fNewCoolDown;
                }

                m_fCooldownTime = m_fDefaultMaxTimeRubber / 2f;
            }

            UpdateUIEraser();
        }

        private void FillRubberPoints(Vector3 v3OldPos, Vector3 v3NewPos)
        {
            if (!m_bCanErase || v3OldPos == Vector3.zero)
                return;

            EnemySearch kEnemy = CheckEnemy(v3OldPos, v3NewPos);

            if (kEnemy.distance < 0)
            {
                m_bIsErasing = false;

                return;
            }

            Vector3 direction = (v3NewPos - v3OldPos).normalized;

            if (kEnemy.distance == 0)
            {
                v3OldPos += direction * m_fDistanceBetweenRubberPoint;

                Erase(kEnemy.transform, v3OldPos);

                float distance = Vector2.Distance(v3OldPos, v3NewPos);

                if (distance > m_fDistanceBetweenRubberPoint)
                {
                    FillRubberPoints(v3OldPos, v3NewPos);
                }
                else
                {
                    m_bIsErasing = false;

                    return;
                }
            }
            else
            {
                v3OldPos += direction * kEnemy.distance;

                Erase(kEnemy.transform, v3OldPos);

                float distance = Vector2.Distance(v3OldPos, v3NewPos);

                if (distance > m_fDistanceBetweenRubberPoint)
                {
                    FillRubberPoints(v3OldPos, v3NewPos);
                }
                else
                {
                    m_bIsErasing = false;

                    return;
                }
            }
        }

        private void CheckEnemy(Vector3 v3Position)
        {
            Collider2D hit = Physics2D.OverlapCircle(v3Position, m_fRubberRadius, LayerMask.GetMask("Enemy"));

            if (hit)
            {
                m_bIsErasing = true;

                Erase(hit.transform.parent.parent, v3Position);
            }
        }

        private void Erase(Transform tEnemy, Vector3 v3Position)
        {
            bool bErased = m_kRubberPool.GetElement(v3Position, tEnemy);

            if (!bErased && OnFinishRubberPoints != null)
            {
                OnFinishRubberPoints();

                return;
            }

            m_v3OldRubberPosition = v3Position;

            if (particle)
                particle.transform.position = v3Position;

            if (audioSource && !audioSource.isPlaying)
            {
                audioSource.Play();
            }

            //Debug.Log("Spawn Rubber point at " + v3Position);
        }

        private EnemySearch CheckEnemy(Vector3 v3StartPosition, Vector3 v3EndPosition)
        {
            EnemySearch result;
            result.distance = -1;
            result.transform = null;

            float distance = Vector3.Distance(v3StartPosition, v3EndPosition);
            Vector3 direction = (v3EndPosition - v3StartPosition).normalized;

            RaycastHit2D hit = Physics2D.CircleCast(v3StartPosition, m_fRubberRadius, direction, distance, LayerMask.GetMask("Enemy"));

            if (hit)
            {
                Debug.DrawLine(v3StartPosition, v3EndPosition + direction * hit.distance, Color.white, 10);
                result.distance = hit.distance;
                result.transform = hit.transform.parent.parent;
            }
            else
            {
                Debug.DrawLine(v3StartPosition, v3EndPosition + direction * distance, Color.red, 10);
            }

            return result;
        }

        public void StopErase()
        {
            m_bCanErase = false;
            m_bIsErasing = false;
            m_v3OldRubberPosition = Vector3.zero;

            if (audioSource)
                audioSource.Stop();

            if (particle)
                particle.SetActive(false);
        }

        public void Reset()
        {
            StopErase();
        }

        public void RestoreElements(DisableCollision[] list)
        {
            m_kRubberPool.RestoreElements(list);
        }

        private void OnDestroy()
        {
            BaseEnemy.OnEnemyDisabled -= RestoreElements;

            PlayerController.OnPowerUp -= PowerUp;
        }

        #region Variables & Properties

        struct EnemySearch
        {
            public float distance;
            public Transform transform;
        }

        public Pool m_kRubberPool;

        Camera m_camera;

        [SerializeField]
        private GameObject particle;

        [SerializeField]
        private AudioSource audioSource;
        
        private Vector3 m_v3OldRubberPosition;

        [SerializeField]
        private float m_fRubberRadius; //must be the same as LinePrefab -> EdgeCollider2D-EdgeRadius

        [SerializeField]
        private float m_fDistanceBetweenRubberPoint;

        [SerializeField]
        private float m_fDefaultMaxTimeRubber;
        [SerializeField]
        private float m_fTimeWarpMaxTimeRubber;
        
        private float m_fMaxTimeRubber;

        private float m_fCurrentUseRubber;
        private float m_fCooldownTime;
        private float m_fCooldown;

        private bool m_bCanErase;
        private bool m_bIsErasing;

        #endregion
    }
}