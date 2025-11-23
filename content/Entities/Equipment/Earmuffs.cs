namespace TC2.Base.Components
{
	public static partial class Earmuffs
	{
		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct Data(): IComponent
		{
			[Editor.Slider.Clamped(1.00f, 22000.00f, 1.00f)]
			public float frequency_high_pass = 100.00f;
			[Editor.Slider.Clamped(1.00f, 22000.00f, 1.00f)]
			public float frequency_low_pass = 200.00f;
			[Editor.Slider.Clamped(0.00f, 1.00f, 0.01f)]
			public float modifier = 0.50f;
			[Editor.Slider.Clamped(0.00f, 2.00f, 0.01f)]
			public float volume_modifier = 1.00f;
		}

		[ISystem.Event<Health.DamageEvent>(ISystem.Mode.Single, ISystem.Scope.Region, order: -110)]
		public static void OnDamage(ISystem.Info info, Entity entity, ref Health.DamageEvent data, 
		[Source.Owned] ref Health.Data health, [Source.Parent<Equip.Data>] ref Equipment.Data equipment, [Source.Parent<Equip.Data>] ref Earmuffs.Data earmuffs)
		{
			//App.WriteLine($"OnDamage {data.damage_type}; {data.damage_integrity}; {data.stun}; {data.knockback}");

			switch (data.damage_type)
			{
				case Damage.Type.Motion_Impulse:
				{
					data.stun *= 0.32f;
				}
				break;

				case Damage.Type.Explosion:
				{
					data.stun *= 0.25f;
				}
				break;

				case Damage.Type.Shockwave:
				{
					data.stun *= 0.10f;
				}
				break;
			}
		}

#if CLIENT
		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent)]
		public static void UpdateEffects(ISystem.Info info, Entity entity,
		[Source.Parent<Equip.Data>] in Equipment.Data equipment, [Source.Owned] in Equipment.Slot.Data equipment_slot,
		[Source.Parent<Equip.Data>] ref Earmuffs.Data earmuffs)
		{
			//App.WriteLine("earmuffs");

			var modifier_low = Maths.PowFast(earmuffs.modifier * 4.00f, 0.50f).Clamp01();
			var modifier_high = Maths.PowFast(earmuffs.modifier * 1.00f, 0.30f).Clamp01();
			var modifier_mix = earmuffs.modifier.Clamp01();

			ref var low_pass = ref Audio.LowPass;
			low_pass.frequency = Maths.Lerp(low_pass.frequency, earmuffs.frequency_low_pass, modifier_low);
			//low_pass.resonance = MathF.Pow(0.70f + (Maths.Clamp01((modifier - 0.20f) * 2.00f) * 8.50f), 2.50f);

			ref var high_pass = ref Audio.HighPass;
			high_pass.frequency = Maths.Lerp(high_pass.frequency, earmuffs.frequency_high_pass, modifier_high);
			//high_pass.resonance = MathF.Pow(0.70f + (Maths.Clamp01((modifier - 0.30f) * 2.00f) * 5.50f), 1.50f);

			ref var mixer = ref Audio.Equalizer;

			mixer.volume_in *= earmuffs.volume_modifier;
			mixer.volume_out_wet = Maths.Lerp(mixer.volume_out_wet, 0.50f, modifier_mix);
			mixer.volume_out_dry = Maths.Lerp(mixer.volume_out_dry, 0.50f, 1.00f - modifier_mix);
		}
#endif
	}
}
