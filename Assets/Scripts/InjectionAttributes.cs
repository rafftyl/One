using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace One
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class GlobalInjectable : Attribute
	{
		Type[] types;
		public IEnumerable<Type> InjectedAsTypes => types;
		public bool IsPersistent { get; set; }

		public GlobalInjectable(params System.Type[] types)
		{
			this.types = types;
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class InjectionReceiver : Attribute
	{
	}

	public enum InjectionType
	{
		Global,
		Unique,
		Component,
		ComponentUpInHierarchy
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class Inject : Attribute
	{
		public InjectionType Type { get; private set; }

		public Inject(InjectionType injectionType = InjectionType.Global)
		{
			Type = injectionType;
		}
	}
}
