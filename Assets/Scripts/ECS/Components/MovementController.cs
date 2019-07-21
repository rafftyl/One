using Unity.Entities;
using Unity.Mathematics;

public struct MovementController : IComponentData
{
	public float3 movementVector;
	public float3 lookAtVector;
}
