using System.Collections.Generic;
using UnityEngine;

namespace MFE.Eraze
{
	public class Pool : MonoBehaviour
	{
		protected virtual void Start()
		{
			m_dPool = new Dictionary<GameObject, bool>(m_iMaxPool);
			
			for (int i = 0; i < m_iMaxPool; i++)
			{
				GameObject obj = Instantiate(m_goReference, gameObject.transform);
				obj.name += i;
				obj.SetActive(false);
				
				m_dPool.Add(obj, true);//true means free
			}
		}

		public void RestoreAll()
		{
			foreach(GameObject go in m_dPool.Keys)
            {
				if (go.activeInHierarchy)
				{
					RestoreElement(go);
				}
            }
		}

		public void RestoreElement(GameObject goToBeRestored)
		{
			if (goToBeRestored.activeInHierarchy && !m_dPool[goToBeRestored])
			{
				goToBeRestored.SetActive(false);
				goToBeRestored.transform.parent = gameObject.transform;
				m_dPool[goToBeRestored] = true;
			}
            else
            {
				Debug.Log("Element not active");
            }
		}

		public void RestoreElements(DisableCollision[] akToRestore)
		{
			foreach (DisableCollision go in akToRestore)
			{
				RestoreElement(go.gameObject);

				go.SimulatePhysics(0);
			}
		}

		public bool GetElement(Vector3 vPosition, Transform tParent = null)
		{
			//check if the pool has free elements
			//if so set active

			GameObject go = GetFreeElement();
			if (go == null)
			{
				Debug.Log("No more elements in the pool\n");

				return false;
			}
			else
			{
				m_dPool[go] = false;

				go.transform.position = vPosition;

				if (tParent)
					go.transform.parent = tParent;

				go.SetActive(true);

				return true;
			}
		}

		public GameObject GetElement()
		{
			GameObject goFreeElement = GetFreeElement();
			if (goFreeElement == null)
			{
				Debug.Log("No more elements in the pool\n");
				return null;
			}
			else
			{
				m_dPool[goFreeElement] = false;

				goFreeElement.SetActive(true);

				return goFreeElement;
			}
		}

		private GameObject GetFreeElement()
		{
			foreach(GameObject go in m_dPool.Keys)
			{
				if (m_dPool[go])
					return go;
			}

			return null;
		}

		#region Variables & Properties

		public bool CanGetElement { get { return GetFreeElement() != null; } }

		[SerializeField]
		protected GameObject m_goReference;

		private Dictionary<GameObject, bool> m_dPool;

		[SerializeField]
		private int m_iMaxPool;

		#endregion
	}
}