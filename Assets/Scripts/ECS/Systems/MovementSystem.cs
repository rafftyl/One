using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public class MovementSystem : JobComponentSystem
{
	[BurstCompile]
	private struct MovementJob : IJobForEach<Translation, Rotation, MovementComponent, MovementController>
	{
		public float deltaTime;
		public void Execute(ref Translation position,
			ref Rotation rotation,
			[ReadOnly] ref MovementComponent movementComponent,
			[ReadOnly] ref MovementController movementController)
		{			
			float3 moveSpeed = movementController.movementVector * movementComponent.translationSpeed;
			moveSpeed.y = 0;
			position.Value += moveSpeed * deltaTime;

			Quaternion targetRotation = Quaternion.LookRotation(movementController.lookAtVector);
			rotation.Value = Quaternion.RotateTowards(rotation.Value,
				targetRotation,
				movementComponent.rotationSpeed * deltaTime);
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var movementJob = new MovementJob() { deltaTime = Time.deltaTime };
		return movementJob.Schedule(this, inputDeps);
	}
}
