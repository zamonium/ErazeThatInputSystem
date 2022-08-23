using System.Collections.Generic;
using UnityEngine;

namespace MFE.Eraze
{
    public class LinePool : Pool
    {
        protected override void Start()
        {
            m_goReference = m_lLines[DataManager.playerData.iCurrentLine];

            base.Start();
        }

        #region Variables & Properties

        [SerializeField]
        List<GameObject> m_lLines;
        
        #endregion
    }
}