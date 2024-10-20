
namespace TC2.Base.Components
{
	public static partial class Morphine
	{
		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct Effect: IComponent
		{
			[Statistics.Info("Morphine", description: "TODO: Desc", format: "{0:0.##} mg", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
			public float amount;

			public float amount_active;
			public float amount_metabolized;
			public float amount_withdrawal;

			public float release_rate_current;
			public float release_rate_target;
			public float release_step;

			[Net.Ignore] public float modifier_current;
			[Net.Ignore] public float modifier_withdrawal;
		}

#if SERVER
		[ISystem.Event<Consumable.ConsumeEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnConsume(ISystem.Info info, Entity entity, ref Consumable.ConsumeEvent data, [Source.Owned] in Consumable.Data consumable, [Source.Owned] ref Morphine.Effect morphine)
		{
			ref var morphine_new = ref data.ent_organic.GetOrAddComponent<Morphine.Effect>(sync: true);

			var ratio = Maths.SumRatio(morphine.amount, morphine_new.amount);

			morphine_new.amount += morphine.amount;
			morphine_new.release_rate_target = Maths.Lerp(morphine_new.release_rate_target + (morphine_new.release_rate_target * ratio), consumable.release_rate, ratio);
			morphine_new.release_step = Maths.Lerp(morphine_new.release_step, consumable.release_step, ratio);

			//App.WriteLine($"ratio: {ratio}");
		}
#endif

		public static Gradient<float> gr_consciousness = new Gradient<float>(1.00f, 0.97f, 0.95f, 0.85f, 0.75f, 0.65f, 0.60f, 0.50f, 0.20f, 0.00f);
		public static Gradient<float> gr_dexterity = new Gradient<float>(1.00f, 0.80f, 0.70f, 0.50f, 0.35f, 0.25f, 0.20f, 0.10f, 0.00f);
		public static Gradient<float> gr_strength = new Gradient<float>(1.00f, 0.80f, 0.75f, 0.60f, 0.40f, 0.30f, 0.20f, 0.10f, 0.00f);
		public static Gradient<float> gr_motorics = new Gradient<float>(1.00f, 0.90f, 0.85f, 0.70f, 0.65f, 0.50f, 0.30f, 0.10f, 0.00f);
		public static Gradient<float> gr_coordination = new Gradient<float>(1.00f, 0.90f, 0.80f, 0.65f, 0.50f, 0.35f, 0.20f, 0.10f, 0.00f);
		public static Gradient<float> gr_pain_modifier = new Gradient<float>(1.00f, 0.80f, 0.40f, 0.20f, 0.10f, 0.05f, 0.00f);

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateStats(ISystem.Info info, Entity entity,
		[Source.All] ref Morphine.Effect morphine, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			var modifier_a = morphine.modifier_current;

			organic.consciousness *= gr_consciousness.GetValue(modifier_a * 1.10f);
			organic.dexterity *= gr_dexterity.GetValue(modifier_a * 0.80f);
			organic.strength *= gr_strength.GetValue(modifier_a * 1.50f);
			organic.motorics *= gr_motorics.GetValue(modifier_a * 0.50f);
			organic.coordination *= gr_coordination.GetValue(modifier_a * 0.90f);
			organic.pain_modifier *= gr_pain_modifier.GetValue(modifier_a * 1.50f);
		}

		public static float metabolization_modifier = 0.10f;
		public static float elimination_modifier = 0.40f;

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateAmount(ISystem.Info info, Entity entity, [Source.Owned] ref Morphine.Effect morphine, [Source.Owned, Override] in Organic.Data organic)
		{
			morphine.release_rate_current = Maths.MoveTowards(morphine.release_rate_current, morphine.release_rate_target, morphine.release_step * App.fixed_update_interval_s);

			if (morphine.amount > Maths.epsilon)
			{
				var amount_released = Maths.Clamp(morphine.release_rate_current, 0.00f, morphine.amount) * App.fixed_update_interval_s;

				morphine.amount_active += amount_released;
				morphine.amount -= amount_released;
			}

			var total_mass = 70.00f; // TODO

			morphine.amount_metabolized = Maths.Clamp(morphine.amount_active * organic.absorption * metabolization_modifier, 0.00f, morphine.amount_active) * App.fixed_update_interval_s;
			morphine.amount_withdrawal = Maths.Lerp2(morphine.amount_withdrawal, morphine.amount_metabolized * 0.65f, 0.0008f, 0.0001f);
			morphine.amount_active -= morphine.amount_metabolized * elimination_modifier;

			morphine.modifier_current = morphine.amount_metabolized / (total_mass * 0.001f) * 0.50f;
			morphine.modifier_withdrawal = morphine.amount_withdrawal / (total_mass * 0.001f) * 0.50f;

#if SERVER
			if (morphine.amount <= 0.01f && morphine.amount_active <= 0.01f && morphine.modifier_withdrawal <= 0.05f)
			{
				entity.RemoveComponent<Morphine.Effect>();
			}
#endif
		}

#if CLIENT
		[ISystem.PreUpdate.Reset(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Shared)]
		public static void UpdateCamera(ISystem.Info info, Entity entity, [Source.Singleton] ref Camera.Singleton camera, [Source.Shared] in Player.Data player, [Source.Owned] in Morphine.Effect morphine)
		{
			var modifier = MathF.Pow(morphine.modifier_current, 1.10f);

			camera.damp_modifier /= 1.00f + (modifier * 1.40f);

			Drunk.Color.a = Maths.Max(Drunk.Color.a, Maths.Clamp(modifier * 1.25f, 0.00f, 0.95f));
		}
#endif
	}
}