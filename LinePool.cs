using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFE.Eraze
{
    public class LinePool : Pool
    {
        [SerializeField]
        List<GameObject> Lines;

        protected override void Start()
        {
            m_goReference = Lines[DataManager.playerData.iCurrentLine];

            base.Start();
        }
	}
}