using Unity.Entities;

public struct MovementComponent : IComponentData
{
	public float translationSpeed;
	public float rotationSpeed;
}
