using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace One.Test
{
	[InjectionReceiver]
	public class TestPureClass
	{
		[Inject] TestGlobalInjectable globalInjectable = null;

		int counter = 0;
		public void Method()
		{
			++counter;
			Debug.Log("Test pure class with injected " + globalInjectable.name + " and counter " + counter);
		}
	}

	[InjectionReceiver]
	public class InjectionTestMonoBehavior : MonoBehaviour
	{
		//[Inject(InjectionType.Component)] TestSiblingComponent sibling = null;
		//[Inject(InjectionType.Component)] TestChildComponent child = null;
		[Inject] ITestInterfaceOne interfaceOne = null;
		[Inject] ITestInterfaceTwo interfaceTwo = null;
		[Inject(InjectionType.Unique)] TestPureClass pureClassUnique = null;
		[Inject] TestPureClass pureClassShared= null;

		private void Start()
		{
			//sibling.Method();
			//child.Method();
			interfaceOne.Method();
			interfaceTwo.Method();
			pureClassUnique.Method();
			pureClassShared.Method();
		}
	}
}