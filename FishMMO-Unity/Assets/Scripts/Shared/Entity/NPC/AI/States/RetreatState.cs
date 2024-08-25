﻿using UnityEngine;
using UnityEngine.AI;

namespace FishMMO.Shared
{
	public class RetreatState : BaseAIState
	{
		/// <summary>
		/// Distance to retreat from the target
		/// </summary>
		public float RetreatDistance = 10.0f;
		/// <summary>
		/// Distance to maintain from the target once retreated
		/// </summary>
		public float SafeDistance = 20.0f;

		public override void Enter(AIController controller)
		{
			if (controller.Target == null)
			{
				controller.TransitionToDefaultState(); // Or another default state
				return;
			}

			// Calculate retreat position
			Vector3 retreatDirection = (controller.Transform.position - controller.Target.position).normalized;
			Vector3 retreatPosition = controller.Transform.position + retreatDirection * RetreatDistance;

			// Set the destination for the retreat
			NavMeshHit hit;
			if (NavMesh.SamplePosition(retreatPosition, out hit, RetreatDistance, NavMesh.AllAreas))
			{
				controller.Agent.SetDestination(hit.position);
			}
		}

		public override void Exit(AIController controller)
		{
		}

		public override void UpdateState(AIController controller)
		{
			if (controller.Target == null)
			{
				controller.TransitionToDefaultState(); // Transition if target is lost
				return;
			}

			// Check if the AI has reached the retreat destination
			if (!controller.Agent.pathPending &&
				controller.Agent.remainingDistance < 1.0f)
			{
				// Check distance from the target
				float distanceToTarget = Vector3.Distance(controller.Transform.position, controller.Target.position);
				if (distanceToTarget > SafeDistance)
				{
					controller.TransitionToDefaultState(); // Transition to another state if safe distance is maintained
				}
				else
				{
					// Continue retreating if not yet at safe distance
					Vector3 retreatDirection = (controller.Transform.position - controller.Target.position).normalized;
					Vector3 retreatPosition = controller.Transform.position + retreatDirection * RetreatDistance;

					NavMeshHit hit;
					if (NavMesh.SamplePosition(retreatPosition, out hit, RetreatDistance, NavMesh.AllAreas))
					{
						controller.Agent.SetDestination(hit.position);
					}
				}
			}
		}
	}
}