using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFE.Eraze
{
    public class InputControllerTutorial : InputController
    {
        public bool AllowChangeMechanics { get; set; }

        public bool StopLines { get; set; }

        public bool OnlyHorizontal { get; set; }

        protected override eGesture getDirection()
        {
            Vector3 vTo = new Vector3(m_v2EndTouchPosition.x, m_v2EndTouchPosition.y, 0);
            Vector3 vFrom = new Vector3(m_v2BeginTouchPosition.x, m_v2BeginTouchPosition.y, 0);
            Vector3 vDirection = vTo - vFrom;

            float fAngle = getAngle(Vector3.right, Vector3.forward, vDirection);
            if (fAngle < 0) fAngle += 360f;
            eGesture result = (eGesture)(int)((fAngle + 22.5f) % 360 / 45);

            if (!StopLines)
            {
                if (OnlyHorizontal && result == eGesture.E)
                    return result;
                else if (!OnlyHorizontal && result == eGesture.NE)
                    return result;
                else
                    result = eGesture.N;

            }
            else
                result = eGesture.N;

            return result;
        }

        protected override void CheckTap()
        {
            if (AllowChangeMechanics)
                base.CheckTap();
        }
    }
}