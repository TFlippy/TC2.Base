
namespace TC2.Base.Components
{
	public static partial class Alcohol
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Effect: IComponent
		{
			[Statistics.Info("Alcohol", description: "TODO: Desc", format: "{0:0.##} ml", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
			public float amount;

			public float amount_active;
			public float amount_metabolized;

			public float release_rate_current;
			public float release_rate_target;
			public float release_step;

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
		public static void OnConsume(ISystem.Info info, Entity entity, ref Consumable.ConsumeEvent data, [Source.Owned] in Consumable.Data consumable, [Source.Owned] ref Alcohol.Effect alcohol)
		{
			ref var alcohol_new = ref data.ent_organic.GetOrAddComponent<Alcohol.Effect>(sync: true);

			var ratio = Maths.Ratio(alcohol.amount, alcohol_new.amount);

			alcohol_new.amount += alcohol.amount;
			alcohol_new.release_rate_target = Maths.Lerp(alcohol_new.release_rate_target + (alcohol_new.release_rate_target * ratio), consumable.release_rate, ratio);
			alcohol_new.release_step = Maths.Lerp(alcohol_new.release_step, consumable.release_step, ratio);

			App.WriteLine($"ratio: {ratio}");
		}
#endif

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateStats(ISystem.Info info, Entity entity,
		[Source.All] ref Alcohol.Effect alcohol, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			var modifier = alcohol.modifier_current;
			var modifier_jitter = alcohol.modifier_current + (alcohol.jitter_current * 0.25f);
			var hiccup_modifier = alcohol.hiccup_current * alcohol.hiccup_current;

			organic.consciousness *= Maths.Lerp01(1.00f, 0.05f, (modifier * 0.30f) + (hiccup_modifier * modifier * 0.30f));
			organic.endurance *= Maths.Lerp01(1.00f, 1.75f, (modifier * 1.70f));
			organic.dexterity *= Maths.Lerp01(1.00f, 0.20f, (modifier * 1.40f) + (hiccup_modifier * 0.50f));
			organic.strength *= Maths.Lerp01(1.00f, 1.30f, (modifier * 1.50f));
			organic.motorics *= Maths.Lerp01(1.00f, 0.40f, (modifier_jitter * 1.10f) + (hiccup_modifier * 0.80f));
			organic.coordination *= Maths.Lerp01(1.00f, 0.10f, (modifier_jitter * 0.30f) + (hiccup_modifier * 0.90f));
			organic.absorption *= Maths.Lerp01(1.00f, 1.50f, (modifier * 1.50f));
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

		public static float metabolization_modifier = 0.10f;
		public static float elimination_modifier = 0.25f;

		[ISystem.VeryLateUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateAmount(ISystem.Info info, Entity entity, [Source.Owned] ref Alcohol.Effect alcohol, [Source.Owned, Override] in Organic.Data organic)
		{
			alcohol.release_rate_current = Maths.MoveTowards(alcohol.release_rate_current, alcohol.release_rate_target, alcohol.release_step * App.fixed_update_interval_s);

			if (alcohol.amount > 0.00f)
			{
				var amount_released = Maths.Clamp(alcohol.release_rate_current, 0.00f, alcohol.amount) * App.fixed_update_interval_s;

				alcohol.amount_active += amount_released;
				alcohol.amount -= amount_released;
			}

			var total_mass = 70.00f; // TODO

			var amount_metabolized = Maths.Clamp(alcohol.amount_active * organic.absorption * metabolization_modifier, 0.00f, alcohol.amount_active) * App.fixed_update_interval_s;

			alcohol.amount_metabolized = amount_metabolized;
			alcohol.modifier_current = amount_metabolized / (total_mass * 0.001f);
			alcohol.hiccup_current = MathF.Max(alcohol.hiccup_current - (App.fixed_update_interval_s * 0.20f), 0.00f);

			alcohol.jitter_current = Maths.Perlin(info.WorldTime, 0.00f, 0.30f);
			alcohol.amount_active -= amount_metabolized * elimination_modifier;

#if SERVER
			var random = XorRandom.New();

			var modifier = alcohol.modifier_current;
			if (modifier > 0.30f && alcohol.hiccup_current < 0.05f && random.NextBool(modifier * 0.005f))
			{
				alcohol.hiccup_current = 1.00f;
				alcohol.Sync(entity);
			}

			if (alcohol.amount <= 0.01f && alcohol.amount_active <= 0.01f)
			{
				entity.RemoveComponent<Alcohol.Effect>();
			}
#endif
		}

#if CLIENT
		[ISystem.PreUpdate.Reset(ISystem.Mode.Single)]
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

				Drunk.Color.W = Drunk.Color.W.Max(Maths.Clamp(modifier, 0.00f, 0.95f));

				ref var low_pass = ref Audio.LowPass;
				low_pass.frequency = Maths.Lerp01(low_pass.frequency, 1000.00f, MathF.Pow(MathF.Max(alcohol.modifier_current - 0.20f, 0.00f), 0.50f));
				low_pass.resonance = Maths.Lerp01(low_pass.resonance, 0.200f, MathF.Pow(alcohol.modifier_current, 0.30f));
			}
		}
#endif
	}
}