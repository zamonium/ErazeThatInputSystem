using UnityEngine;

namespace MFE.Eraze
{
    public class EnemyCollider : MonoBehaviour
    {
        public delegate void ColliderCallback(string sName);
        public static event ColliderCallback OnColliderDisable;

        public static event ColliderCallback OnPlayerCollision;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (OnPlayerCollision != null)
                    OnPlayerCollision(transform.parent.parent.name);
            }

            if ((gameObject.activeSelf && collision.gameObject.CompareTag("Rubber") && Vector2.Distance(collision.transform.position, transform.position) <= collision.collider.bounds.size.x / 2)
                || collision.gameObject.CompareTag("RubberShield"))
            {
                gameObject.SetActive(false);

                if (OnColliderDisable != null)
                {
                    OnColliderDisable(transform.parent.parent.name);
                }
            }
        }
    }
}