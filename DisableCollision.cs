using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFE.Eraze
{
    public class DisableCollision : MonoBehaviour
    {
        private Rigidbody2D m_kRigidbody;

        [SerializeField]
        SpriteRenderer trailRenderer;

        void Awake()
        {
            m_kRigidbody = GetComponent<Rigidbody2D>();

            GameManager.OnRestart += SimulatePhysics;

            if (trailRenderer)
                gameObject.transform.Rotate(Vector3.forward, Random.Range(0, 360));
        }

        public void SimulatePhysics(float fXPosition)
        {
            m_kRigidbody.simulated = true;

            if (trailRenderer)
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
    }
}