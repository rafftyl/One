using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace One
{
	public class DependencyResolver : MonoBehaviour
	{
		[SerializeField] GameObject[] globalInjectablePrefabs = null;
		[SerializeField] ScriptableObject[] globalInjectableResources = null;

		Dictionary<System.Type, object> globalInjectables = new Dictionary<System.Type, object>();
		Dictionary<System.Type, object> persistentInjectables = new Dictionary<System.Type, object>();

		private static DependencyResolver resolverInstance;
		public static DependencyResolver Instance
		{
			get
			{
				if(resolverInstance == null)
				{
					resolverInstance = Object.Instantiate(Resources.Load<DependencyResolver>("DependencyResolver"));
				}

				return resolverInstance;
			}

			private set => resolverInstance = value;
		}

		private void Awake()
		{
			if(resolverInstance != null && resolverInstance != this)
			{
				Destroy(this);
			}
		}

		public void InjectDependencies()
		{
			for(int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; ++sceneIndex)
			{
				Scene scene = SceneManager.GetSceneAt(sceneIndex);
				if(scene.rootCount == 0)
				{
					Debug.LogWarning("Passed an empty scene to dependency injection");
				}

				foreach(var rootGO in scene.GetRootGameObjects())
				{
					InjectDependencies(rootGO);
				}
			}
		}

		List<MemberInfo> membersToInject = new List<MemberInfo>();
		public void InjectDependencies(GameObject injectionReceiver)
		{
			MonoBehaviour[] monoBehaviours = injectionReceiver.GetComponentsInChildren<MonoBehaviour>();
			foreach(MonoBehaviour monoBeh in monoBehaviours)
			{
				InjectDependencies(monoBeh);
			}
		}

		public void InjectDependencies(object classInstance)
		{
			var currentType = classInstance.GetType();
			var injectionReceiverAttr = currentType.GetCustomAttribute<InjectionReceiver>();
			while (currentType != null && injectionReceiverAttr != null)
			{
				foreach(var memberInfo in
					currentType.GetMembers(BindingFlags.Instance |
					BindingFlags.Public | BindingFlags.NonPublic))
				{
					Inject injectAttribute = memberInfo.GetCustomAttribute<Inject>();
					if(injectAttribute != null)
					{
						switch (injectAttribute.Type)
						{
							case InjectionType.Global:
								InjectGlobal(classInstance, memberInfo);
								break;
							case InjectionType.Unique:
								InjectUnique(classInstance, memberInfo);
								break;
							case InjectionType.Component:
								InjectDownInHierarchy(classInstance, memberInfo);
								break;
							case InjectionType.ComponentUpInHierarchy:
								InjectUpInHierarchy(classInstance, memberInfo);
								break;
						}
					}
				}
				currentType = currentType.BaseType;
				injectionReceiverAttr = currentType.GetCustomAttribute<InjectionReceiver>();
			}
		}

		private void InjectGlobal(object classInstance, MemberInfo member)
		{

		}

		private void InjectUnique(object classInstance, MemberInfo member)
		{

		}

		private void InjectDownInHierarchy(object classInstance, MemberInfo member)
		{

		}

		private void InjectUpInHierarchy(object classInstance, MemberInfo member)
		{

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
			var instance = Object.Instantiate(original);
			InjectDependencies(instance);
			return instance;
		}
	}
}
