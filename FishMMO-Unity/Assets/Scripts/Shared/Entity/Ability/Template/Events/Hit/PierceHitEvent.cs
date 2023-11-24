﻿using UnityEngine;

namespace FishMMO.Shared
{
	[CreateAssetMenu(fileName = "New Pierce Hit Event", menuName = "Character/Ability/Hit Event/Pierce", order = 1)]
	public sealed class PierceHitEvent : HitEvent
	{
		public override int Invoke(Character attacker, Character defender, TargetInfo hitTarget, GameObject abilityObject)
		{
			return 1;
		}
	}
}