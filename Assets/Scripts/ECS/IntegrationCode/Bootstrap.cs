using UnityEngine;
using Unity.Entities;

public class Bootstrap
{
	private static EntityManager entityManager;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
	{
		entityManager = World.Active.EntityManager;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void InitializeWithScene()
	{
		Debug.Log("Initializing scene");
	}
}
