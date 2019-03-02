using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace One.Test
{
	public class TestChildComponent : MonoBehaviour
	{
		public void Method()
		{
			Debug.Log("Called method on test child component " + name);
		}
	}
}