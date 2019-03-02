using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace One.Test
{
	public class TestSiblingComponent : MonoBehaviour
	{
		public void Method()
		{
			Debug.Log("Called method on test sibling component " + name);
		}
	}
}