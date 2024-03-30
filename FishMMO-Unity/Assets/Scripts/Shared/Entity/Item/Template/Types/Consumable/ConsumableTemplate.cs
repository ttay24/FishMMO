﻿namespace FishMMO.Shared
{
	public abstract class ConsumableTemplate : BaseItemTemplate
	{
		public ConsumableType ConsumableType;
		public uint ChargeCost = 1;
		public float Cooldown;

		public bool CanConsume(Character character, Item item)
		{
			return character != null &&
				   item != null &&
				   item.IsStackable &&
				   item.Stackable.Amount > 1 &&
				   character.TryGet(out ICooldownController cooldownController) &&
				   !cooldownController.IsOnCooldown(ConsumableType.ToString());
		}

		public virtual bool Invoke(Character character, Item item)
		{
			if (CanConsume(character, item) &&
				character.TryGet(out ICooldownController cooldownController))
			{
				if (Cooldown > 0.0f)
				{
					cooldownController.AddCooldown(ConsumableType.ToString(), new CooldownInstance(Cooldown));
				}
				if (item.IsStackable && item.Stackable.Amount > ChargeCost)
				{
					//consume charges
					item.Stackable.Remove(ChargeCost);

					if (item.Stackable.Amount < 1)
					{
						item.Destroy();
					}
				}
				else
				{
					item.Destroy();
				}
				return true;
			}
			return false;
		}
	}
}