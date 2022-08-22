using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFE.Eraze
{
	public class Pool : MonoBehaviour
	{
		#region Variables

		[SerializeField]
		protected GameObject m_goReference;

		[SerializeField]
		private int m_iMaxPool;

		private GameObject[] pool;
		private bool[] freeElements;//true means free
		#endregion

		public bool CanGetElement { get { return getFreeElement() >= 0; } }


		protected virtual void Start()
		{
			pool = new GameObject[m_iMaxPool];
			freeElements = new bool[m_iMaxPool];
			for (int i = 0; i < m_iMaxPool; i++)
			{
				pool[i] = Instantiate(m_goReference, gameObject.transform);
				pool[i].name += i;
				pool[i].SetActive(false);
				freeElements[i] = true;
			}
		}

		public void RestoreAll()
		{
			for (int i = 0; i < m_iMaxPool; i++)
			{
				if (pool[i].activeInHierarchy)
				{
					Restore(i);
				}
			}
		}

		public void RestoreElement(GameObject goToBeRestored)
		{
			for (int i = 0; i < m_iMaxPool; i++)
			{
				if (pool[i].activeInHierarchy && pool[i] == goToBeRestored)
				{
					Restore(i);

					return;
				}
			}

			Debug.Log("Element not found");
		}

		private void Restore(int iIndex)
		{
			pool[iIndex].SetActive(false);
			pool[iIndex].transform.parent = gameObject.transform;
			freeElements[iIndex] = true;
		}

		public void RestoreElements(DisableCollision[] list)
		{
			foreach (DisableCollision go in list)
			{
				int i = FindElement(go.name);

				if (i != -1)
				{
					Restore(i);

					go.SimulatePhysics(0);
				}
				else
					Debug.Log("Element not found");
			}
		}

		private int FindElement(string sName)
		{
			for (int i = 0; i < m_iMaxPool; i++)
			{
				if (pool[i] && pool[i].name == sName)
				{
					return i;
				}
			}

			return -1;
		}

		public bool GetElement(Vector3 vPosition, Transform tParent = null)
		{
			//check if there is free elements
			int indexFreeElement = getFreeElement();
			if (indexFreeElement == -1)
			{
				Debug.Log("No more elements in the pool\n");

				return false;
			}
			else
			{
				freeElements[indexFreeElement] = false;

				pool[indexFreeElement].transform.position = vPosition;

				if (tParent)
					pool[indexFreeElement].transform.parent = tParent;

				pool[indexFreeElement].SetActive(true);

				return true;

			}
		}

		public GameObject GetElement()
		{
			int indexFreeElement = getFreeElement();
			if (indexFreeElement == -1)
			{
				Debug.Log("No more elements in the pool\n");
				return null;
			}
			else
			{
				freeElements[indexFreeElement] = false;

				pool[indexFreeElement].SetActive(true);

				return pool[indexFreeElement];
			}
		}

		private int getFreeElement()
		{
			for (int i = 0; i < m_iMaxPool; i++)
			{
				if (freeElements[i])
					return i;
			}

			return -1;
		}
	}
}