using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace One
{
	/// <summary>
	/// Modify this class to define pure class interface injection rules
	/// </summary>
	public class DefaultInjectionRuleProvider : InjectionRuleProvider
	{
		public override Dictionary<Type, Type> InjectionRules => new Dictionary<Type, Type>();
	}
}
