using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;

namespace One
{
	public class DependencyResolver : MonoBehaviour
	{
		[SerializeField] GameObject[] injectablePrefabs = null;
		[SerializeField] ScriptableObject[] globalInjectableResources = null;

		Dictionary<Type, GameObject> injectablePrefabPrototypeMap = new Dictionary<Type, GameObject>();
		Dictionary<Type, Type> pureClassInjectableMap = new Dictionary<Type, Type>();

		Dictionary<Type, object> globalInjectables = new Dictionary<Type, object>();
		static Dictionary<Type, object> persistentInjectables = new Dictionary<Type, object>();

		private static DependencyResolver resolverInstance;
		public static DependencyResolver Instance
		{
			get
			{
				if(resolverInstance == null)
				{
					resolverInstance = UnityEngine.Object.Instantiate(Resources.Load<DependencyResolver>("DependencyResolver"));
				}

				return resolverInstance;
			}

			private set => resolverInstance = value;
		}

		private void Awake()
		{
			if (resolverInstance != null && resolverInstance != this)
			{				
				Destroy(resolverInstance);				
			}

			resolverInstance = this;
			Initialize();			

			InjectDependenciesAtScenes();
		}

		private void Initialize()
		{
			foreach(var resource in globalInjectableResources)
			{
				AddInjectableToMap(resource);
			}

			foreach(var prefab in injectablePrefabs)
			{
				var monoBehaviors = prefab.GetComponentsInChildren<MonoBehaviour>();
				foreach(var monoBehavior in monoBehaviors)
				{
					var injectableAttribute = monoBehavior.GetType().GetCustomAttribute<GlobalInjectable>();
					if(injectableAttribute != null)
					{
						foreach(var type in injectableAttribute.InjectedAsTypes)
						{
							injectablePrefabPrototypeMap.Add(type, prefab);
						}
					}
				}
			}

			var ruleProviders = GetComponents<InjectionRuleProvider>();
			foreach(var provider in ruleProviders)
			{
				var rules = provider.InjectionRules;
				foreach(var rule in rules)
				{
					pureClassInjectableMap.Add(rule.Key, rule.Value);
				}
			}
		}

		List<MonoBehaviour> cachedInjectionReceivers = new List<MonoBehaviour>();
		private void InjectDependenciesAtScenes()
		{
			cachedInjectionReceivers.Clear();
			for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; ++sceneIndex)
			{
				Scene scene = SceneManager.GetSceneAt(sceneIndex);
				if (scene.rootCount == 0)
				{
					Debug.LogWarning("Passed an empty scene to dependency injection");
				}

				foreach (var rootGO in scene.GetRootGameObjects())
				{
					MonoBehaviour[] monoBehaviours = rootGO.GetComponentsInChildren<MonoBehaviour>();
					foreach (MonoBehaviour monoBeh in monoBehaviours)
					{
						var type = monoBeh.GetType();
						if (type.GetCustomAttribute<InjectionReceiver>() != null)
						{
							cachedInjectionReceivers.Add(monoBeh);
						}

						AddInjectableToMap(monoBeh);
					}
				}
			}

			foreach (var injectionReceiver in cachedInjectionReceivers)
			{
				InjectDependencies(injectionReceiver);
			}
		}

		private void InjectDependencies(GameObject injectionReceiver)
		{
			MonoBehaviour[] monoBehaviours = injectionReceiver.GetComponentsInChildren<MonoBehaviour>();
			foreach (MonoBehaviour monoBeh in monoBehaviours)
			{
				InjectDependencies(monoBeh);
			}
		}

		private void InjectDependencies(object classInstance)
		{
			var currentType = classInstance.GetType();
			var injectionReceiverAttr = currentType.GetCustomAttribute<InjectionReceiver>();
			while (currentType != null && injectionReceiverAttr != null)
			{
				//TODO: component injection
				foreach (var fieldInfo in
					currentType.GetFields(BindingFlags.Instance |
					BindingFlags.Public | BindingFlags.NonPublic))
				{
					Inject injectAttribute = fieldInfo.GetCustomAttribute<Inject>();
					if (injectAttribute != null)
					{
						switch (injectAttribute.Type)
						{
							case InjectionType.Global:
								InjectGlobal(classInstance, fieldInfo);
								break;
							case InjectionType.Unique:
								InjectUnique(classInstance, fieldInfo);
								break;
							case InjectionType.Component:
								InjectDownInHierarchy(classInstance as MonoBehaviour, fieldInfo);
								break;
							case InjectionType.ComponentUpInHierarchy:
								InjectUpInHierarchy(classInstance as MonoBehaviour, fieldInfo);
								break;
						}
					}

				}
				currentType = currentType.BaseType;
				injectionReceiverAttr = currentType.GetCustomAttribute<InjectionReceiver>();
			}
		}

		private void AddInjectablesToMap(IEnumerable<object> injectables)
		{
			foreach(var injectable in injectables)
			{
				AddInjectableToMap(injectable);
			}
		}

		private void AddInjectablesToMap(params object[] injectables)
		{
			AddInjectablesToMap(injectables as IEnumerable<object>);
		}

		private void AddInjectableToMap(object injectable)
		{
			var type = injectable.GetType();
			var injectableAttribute = type.GetCustomAttribute<GlobalInjectable>();
			if (injectableAttribute != null)
			{
				var injectableMap = injectableAttribute.IsPersistent ? globalInjectables : persistentInjectables;
				foreach (var injectedType in injectableAttribute.InjectedAsTypes)
				{
					injectableMap.Add(injectedType, injectable);
				}
			}
		}

		private void SetPureClassInjectableMapping<InjectedType, CreatedType>()
		{
			pureClassInjectableMap.Add(typeof(InjectedType), typeof(CreatedType));
		}		

		private void InjectGlobal(object classInstance, FieldInfo member)
		{
			object injectable = null;
			if(globalInjectables.TryGetValue(member.FieldType, out injectable) || 
				persistentInjectables.TryGetValue(member.FieldType, out injectable))
			{
				InjectIntoMember(classInstance, member, injectable);
			}
			else
			{
				if(injectablePrefabPrototypeMap.TryGetValue(member.FieldType, out var prefab))
				{
					var instance = Instantiate(prefab);
					AddInjectablesToMap(instance.GetComponentsInChildren<MonoBehaviour>());
					InjectIntoMember(classInstance, member, instance.GetComponentInChildren(member.FieldType));
					return;
				}
				else if(pureClassInjectableMap.TryGetValue(member.FieldType, out var newInstanceType))
				{
					var instance = CreateClassInstance(newInstanceType);
					AddInjectableToMap(instance);
					InjectIntoMember(classInstance, member, instance);
					return;
				}
				else if(!member.FieldType.IsInterface &&
					!member.FieldType.IsAbstract && 
					!member.FieldType.IsSubclassOf(typeof(MonoBehaviour)))
				{
					Debug.Log($"No suitable creation rule for {member.FieldType.Name} has been found. Creating default object");
					var instance = CreateClassInstance(member.FieldType);
					AddInjectableToMap(instance);
					InjectIntoMember(classInstance, member, instance);
					return;
				}

				Debug.LogWarning("Couldn't find an injectable of type " + member.FieldType + " for field " +
						member.Name + " in class " + classInstance.GetType().Name);				
			}
		}

		private void InjectUnique(object classInstance, FieldInfo member)
		{
			if(member.FieldType.IsSubclassOf(typeof(MonoBehaviour)))
			{
				Debug.LogWarning("Unique mono behavior injection is not supported yet");
			}
			else
			{
				InjectIntoMember(classInstance, member, CreateClassInstance(member.FieldType));
			}
		}

		private void InjectDownInHierarchy(MonoBehaviour classInstance, FieldInfo member)
		{
			Assert.IsNotNull(classInstance);
			var component = classInstance.GetComponentInChildren(member.FieldType);
			if (component != null)
			{
				InjectIntoMember(classInstance, member, component);
			}
			else
			{
				AddDefaultComponent(classInstance, member);
			}
		}

		private void InjectUpInHierarchy(MonoBehaviour classInstance, FieldInfo member)
		{
			Assert.IsNotNull(classInstance);
			Transform current = classInstance.transform;
			while(current != null)
			{
				var component = current.GetComponent(member.FieldType);
				if(component != null)
				{
					InjectIntoMember(classInstance, member, component);
					return;
				}
				current = current.parent;
			}
			AddDefaultComponent(classInstance, member);
		}

		private void AddDefaultComponent(MonoBehaviour classInstance, FieldInfo member)
		{
			Debug.LogWarning("Couldn't find a component of type " + member.FieldType + " for field " +
			member.Name + " in class " + classInstance.GetType().Name + " up in hierarchy from object "
			+ classInstance.name + ". Adding default if possible.");
			if (!member.FieldType.IsInterface)
			{
				var createdComponent = classInstance.gameObject.AddComponent(member.FieldType);
				InjectIntoMember(classInstance, member, createdComponent);
			}
			else if (pureClassInjectableMap.TryGetValue(member.FieldType, out var newInstanceType))
			{
				var instance = CreateClassInstance(newInstanceType);
				InjectIntoMember(classInstance, member, instance);
			}
		}

		private void InjectIntoMember(object classInstance, FieldInfo FieldInfo, object memberValue)
		{
			var field = FieldInfo as FieldInfo;
			field.SetValue(classInstance, memberValue);
		}

		private object CreateClassInstance(System.Type instanceType)
		{
			object instance = Activator.CreateInstance(instanceType);
			InjectDependencies(instance);
			return instance;
		}

		public Class CreateClassInstance<Class>() where Class : new()
		{
			var instance = new Class();
			InjectDependencies(instance);
			return instance;
		}

		public new T Instantiate<T>(T original) where T : MonoBehaviour
		{
			return Instantiate(original.gameObject).GetComponent<T>();
		}

		public GameObject Instantiate(GameObject original)
		{
			var instance = UnityEngine.Object.Instantiate(original);
			InjectDependencies(instance);
			return instance;
		}
	}
}
