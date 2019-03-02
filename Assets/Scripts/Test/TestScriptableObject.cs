using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace One.Test
{
	[CreateAssetMenu(fileName = "TestScriptableObject", menuName = "One/Test Scriptable Object")]
	[GlobalInjectable(typeof(TestScriptableObject), IsPersistent = true)]
	public class TestScriptableObject : ScriptableObject
	{
		public void Method()
		{
			Debug.Log("Called Method() on TestScriptableObject");
		}
	}
}