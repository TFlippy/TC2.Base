
namespace TC2.Base.Components
{
	public static partial class Alcohol
	{
		[IComponent.Data(Net.SendType.Reliable), ITrait.Data(Net.SendType.Reliable)]
		public partial struct Effect: IComponent, ITrait
		{
			public float amount;

			[Net.Ignore] public float modifier_current;
			[Net.Ignore] public float jitter_current;
			public float hiccup_current;

			[Save.Ignore, Net.Ignore] public float next_sound;
		}

		public static Sound.Handle[] sounds_drunk =
		{
			"drunk.00",
			"drunk.01",
			"drunk.02",
			"drunk.03",
			"drunk.04",
		};

#if SERVER
		[ISystem.Event<Consumable.ConsumeEvent>(ISystem.Mode.Single)]
		public static void OnConsume(ISystem.Info info, Entity entity, ref Consumable.ConsumeEvent data, [Source.Owned, Trait.Of<Consumable.Data>] ref Alcohol.Effect alcohol)
		{
			ref var alcohol_new = ref data.ent_organic.GetOrAddComponent<Alcohol.Effect>(sync: true);
			alcohol_new.amount += alcohol.amount;
		}
#endif

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateStats(ISystem.Info info, Entity entity,
		[Source.All] ref Alcohol.Effect alcohol, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			var modifier = alcohol.modifier_current;
			var modifier_jitter = alcohol.modifier_current + (alcohol.jitter_current * 0.25f);
			var hiccup_modifier = alcohol.hiccup_current * alcohol.hiccup_current;

			organic.consciousness *= Maths.Lerp01(1.00f, 0.05f, (modifier_jitter * 0.50f) + (hiccup_modifier * modifier * 0.30f));
			organic.endurance *= Maths.Lerp01(1.00f, 1.75f, (modifier * 1.70f));
			organic.dexterity *= Maths.Lerp01(1.00f, 0.20f, (modifier * 1.40f) + (hiccup_modifier * 0.50f));
			organic.strength *= Maths.Lerp01(1.00f, 1.30f, (modifier * 1.50f));
			organic.motorics *= Maths.Lerp01(1.00f, 0.40f, (modifier_jitter * 1.10f) + (hiccup_modifier * 0.80f));
			organic.coordination *= Maths.Lerp01(1.00f, 0.10f, (modifier_jitter * 0.30f) + (hiccup_modifier * 0.90f));
			organic.pain_modifier *= Maths.Lerp01(1.00f, 0.20f, (modifier * 1.20f));
		}

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateHead(ISystem.Info info, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.All] ref Alcohol.Effect alcohol, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Head.Data head)
		{
			var modifier = alcohol.modifier_current + (alcohol.jitter_current * 0.75f);

			if (info.WorldTime >= alcohol.next_sound && alcohol.hiccup_current > 0.90f)
			{
				var random = XorRandom.New();
				Sound.Play(ref info.GetRegion(), sounds_drunk.GetRandom(ref random), transform.position, volume: random.NextFloatRange(0.30f, 0.50f), pitch: head.voice_pitch * random.NextFloatRange(0.90f, 1.10f));

				alcohol.next_sound = info.WorldTime + random.NextFloatRange(2.00f, 5.00f);
			}
		}
#endif

		[ISystem.VeryLateUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateAmount(ISystem.Info info, Entity entity, [Source.Owned] ref Alcohol.Effect alcohol)
		{
			var jitter = Maths.Perlin(info.WorldTime, 0.00f, 0.30f);

			var amount_consumed = (1.00f + (alcohol.amount * 0.010f)) * App.fixed_update_interval_s;
			amount_consumed = Maths.Clamp(amount_consumed, 0.00f, alcohol.amount);

			alcohol.jitter_current = jitter;
			alcohol.modifier_current = alcohol.amount * 0.010f * 8.00f * App.fixed_update_interval_s;
			alcohol.hiccup_current = MathF.Max(alcohol.hiccup_current - (App.fixed_update_interval_s * 0.20f), 0.00f);
			alcohol.amount -= amount_consumed;

#if SERVER
			var random = XorRandom.New();

			var modifier = alcohol.modifier_current;
			if (modifier > 0.30f && alcohol.hiccup_current < 0.05f && random.NextBool(modifier * 0.005f))
			{
				alcohol.hiccup_current = 1.00f;
				alcohol.Sync(entity);
			}

			if (alcohol.amount <= 0.00f)
			{
				entity.RemoveComponent<Alcohol.Effect>();
			}
#endif
		}

#if CLIENT
		[ISystem.PreUpdate.Reset(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateCamera(ISystem.Info info, Entity entity, [Source.Global] ref Camera.Global camera, [Source.Shared] in Player.Data player, [Source.Owned] in Alcohol.Effect alcohol)
		{
			if (player.IsLocal())
			{
				var modifier = MathF.Pow(alcohol.modifier_current, 1.30f);

				var rot = 0.00f;
				rot += MathF.Sin(info.WorldTime / 2.53f) * 0.18f * modifier;
				rot += MathF.Cos(info.WorldTime / 1.43f) * 0.15f * modifier;
				rot += MathF.Sin(380 + info.WorldTime / 0.70f) * 0.10f * modifier;

				camera.zoom_modifier *= 1.00f - (Maths.Perlin(info.WorldTime * 0.15f, 0.00f, 4.00f) * 0.30f * modifier);
				camera.rotation = rot;

				Drunk.Color.W = Maths.Clamp(modifier, 0.00f, 0.95f);

				ref var low_pass = ref Audio.LowPass;
				low_pass.frequency = Maths.Lerp01(low_pass.frequency, 1000.00f, MathF.Pow(MathF.Max(alcohol.modifier_current - 0.20f, 0.00f), 0.50f));
				low_pass.resonance = Maths.Lerp01(low_pass.resonance, 0.200f, MathF.Pow(alcohol.modifier_current, 0.30f));
			}
		}
#endif
	}
}