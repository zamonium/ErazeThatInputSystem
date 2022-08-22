using UnityEngine;

namespace MFE.Eraze
{
    public class BaseInput : MonoBehaviour
    {
        private GameObject m_goPlayer;

        public virtual void Action(Vector2 v2BeginTouch, Vector3 v3Begin, eGesture eDirection) { }
        public virtual void Action(Vector3 v3Begin) { }

        private void Awake()
        {
            m_goPlayer = GameObject.FindGameObjectWithTag("Player");
        }

        protected bool CanDraw(Vector2 mousePos)
        {
            return mousePos.x >= m_goPlayer.transform.position.x + 1.6f;//offset 1 chunk + 0.1
        }

        protected bool CanDrawBack(Vector2 mousePos)
        {
            return mousePos.x >= m_goPlayer.transform.position.x + 4.6f;//offset 2 chunk + 0.1
        }
    }
}