using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityObjectPoolCompnnet : MonoBehaviour
{
	public void ChangeParentToUnityObjectPoolComponent(GameObject prefab)
	{
		prefab.transform.SetParent(this.transform, false);
	}
}
