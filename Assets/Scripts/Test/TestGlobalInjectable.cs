using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace One.Test
{
	interface ITestInterfaceOne
	{
		void Method();
	}

	[GlobalInjectable(typeof(ITestInterfaceOne), typeof(TestGlobalInjectable))]
	[InjectionReceiver]
	public class TestGlobalInjectable : MonoBehaviour, ITestInterfaceOne
	{
		[Inject] TestSceneInjectable sceneInjectable = null;
		public void Method()
		{
			Debug.Log("Called Method() on TestGlobalInjectable with injected " + sceneInjectable.name);
		}
	}
}
