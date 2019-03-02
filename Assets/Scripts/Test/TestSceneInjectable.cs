using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace One.Test
{
	interface ITestInterfaceTwo
	{
		void Method();
	}

	[GlobalInjectable(typeof(ITestInterfaceTwo), typeof(TestSceneInjectable))]
	public class TestSceneInjectable : MonoBehaviour, ITestInterfaceTwo
	{
		public void Method()
		{
			Debug.Log("Called Method() on TestSceneInjectable");
		}
	}
}