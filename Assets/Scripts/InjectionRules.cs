using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace One
{
	public partial class DependencyResolver
	{
		partial void SetPureClassInjectables()
		{
			SetPureClassInjectableMapping<Test.TestPureClass, Test.TestPureClass>();
		}
	}
}
