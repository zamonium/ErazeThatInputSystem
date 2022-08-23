using UnityEngine;

namespace MFE.Eraze
{
    public class DisableCollision : MonoBehaviour
    {
        void Awake()
        {
            m_kRigidbody = GetComponent<Rigidbody2D>();

            GameManager.OnRestart += SimulatePhysics;

            if (m_kTrailRenderer)
                gameObject.transform.Rotate(Vector3.forward, Random.Range(0, 360));
        }

        public void SimulatePhysics(float fXPosition)
        {
            m_kRigidbody.simulated = true;

            if (m_kTrailRenderer)
                gameObject.transform.Rotate(Vector3.forward, Random.Range(0, 360));
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (m_kRigidbody.simulated)
                m_kRigidbody.simulated = false;
        }

        private void OnDestroy()
        {
            GameManager.OnRestart -= SimulatePhysics;
        }

        #region Variables & Properties

        private Rigidbody2D m_kRigidbody;

        [SerializeField]
        SpriteRenderer m_kTrailRenderer;

        #endregion
    }
}