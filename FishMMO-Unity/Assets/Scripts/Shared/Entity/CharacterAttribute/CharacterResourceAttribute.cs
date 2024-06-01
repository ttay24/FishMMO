﻿namespace FishMMO.Shared
{
	public class CharacterResourceAttribute : CharacterAttribute
	{
		private float currentValue;

		public float CurrentValue { get { return currentValue; } }

		public override string ToString()
		{
			return Template.Name + ": " + (int)currentValue + "/" + FinalValue;
		}

		public CharacterResourceAttribute(int templateID, int initialValue, float currentValue, int modifier) : base(templateID, initialValue, modifier)
		{
			this.currentValue = currentValue;
		}

		public void AddToCurrentValue(float value)
		{
			float tmp = currentValue;
			currentValue += value;
			if (currentValue == tmp)
			{
				return;
			}
			if (currentValue > this.FinalValue)
			{
				currentValue = this.FinalValue;
			}
			Internal_OnAttributeChanged(this);
		}

		public void SetCurrentValue(float value, bool updateInternal = true)
		{
			currentValue = value;
			if (updateInternal)
			{
				Internal_OnAttributeChanged(this);
			}
		}

		public void Consume(float amount)
		{
			currentValue -= amount;
			if (currentValue <= 0.001f)
			{
				currentValue = 0.0f;
			}
			//UnityEngine.Debug.Log($"Consumed {amount} {Template.Name} - [{currentValue}/{FinalValue}]");
			Internal_OnAttributeChanged(this);
		}

		public void Gain(float amount)
		{
			currentValue += amount;
			if (currentValue >= FinalValue)
			{
				currentValue = FinalValue;
			}
			Internal_OnAttributeChanged(this);
		}

		protected override void Internal_OnAttributeChanged(CharacterAttribute attribute)
		{
			base.Internal_OnAttributeChanged(attribute);
		}
	}
}