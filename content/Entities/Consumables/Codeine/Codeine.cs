
namespace TC2.Base.Components
{
	public static partial class Codeine
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Effect: IComponent
		{
			[Statistics.Info("Codeine", description: "TODO: Desc", format: "{0:0.##} mg", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
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
		[ISystem.Event<Consumable.ConsumeEvent>(ISystem.Mode.Single)]
		public static void OnConsume(ISystem.Info info, Entity entity, ref Consumable.ConsumeEvent data, [Source.Owned] in Consumable.Data consumable, [Source.Owned] ref Codeine.Effect codeine)
		{
			ref var codeine_new = ref data.ent_organic.GetOrAddComponent<Codeine.Effect>(sync: true);

			var ratio = Maths.Ratio(codeine.amount, codeine_new.amount);

			codeine_new.amount += codeine.amount;
			codeine_new.release_rate_target = Maths.Lerp(codeine_new.release_rate_target + (codeine_new.release_rate_target * ratio), consumable.release_rate, ratio);
			codeine_new.release_step = Maths.Lerp(codeine_new.release_step, consumable.release_step, ratio);

			App.WriteLine($"ratio: {ratio}");
		}
#endif

		public static Gradient gr_consciousness = new Gradient(1.00f, 0.97f, 0.95f, 0.85f, 0.75f, 0.65f, 0.60f, 0.50f, 0.20f, 0.00f);
		public static Gradient gr_dexterity = new Gradient(1.00f, 0.80f, 0.70f, 0.50f, 0.35f, 0.25f, 0.20f, 0.10f, 0.00f);
		public static Gradient gr_strength = new Gradient(1.00f, 0.80f, 0.75f, 0.60f, 0.40f, 0.30f, 0.20f, 0.10f, 0.00f);
		public static Gradient gr_motorics = new Gradient(1.00f, 0.90f, 0.85f, 0.70f, 0.65f, 0.50f, 0.30f, 0.10f, 0.00f);
		public static Gradient gr_coordination = new Gradient(1.00f, 0.90f, 0.80f, 0.65f, 0.50f, 0.35f, 0.20f, 0.10f, 0.00f);
		public static Gradient gr_pain_modifier = new Gradient(1.00f, 0.80f, 0.40f, 0.20f, 0.10f, 0.05f, 0.00f);

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateStats(ISystem.Info info, Entity entity,
		[Source.All] ref Codeine.Effect codeine, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			var modifier_a = codeine.modifier_current;

			organic.consciousness *= gr_consciousness.GetValue(modifier_a * 0.90f);
			organic.dexterity *= gr_dexterity.GetValue(modifier_a * 0.90f);
			organic.strength *= gr_strength.GetValue(modifier_a * 0.90f);
			organic.motorics *= gr_motorics.GetValue(modifier_a * 0.30f);
			organic.coordination *= gr_coordination.GetValue(modifier_a * 0.60f);
			organic.pain_modifier *= gr_pain_modifier.GetValue(modifier_a * 1.00f);
		}

		public static float metabolization_modifier = 0.06f;
		public static float elimination_modifier = 0.10f;

		[ISystem.VeryLateUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateAmount(ISystem.Info info, Entity entity, [Source.Owned] ref Codeine.Effect codeine, [Source.Owned, Override] in Organic.Data organic)
		{
			codeine.release_rate_current = Maths.MoveTowards(codeine.release_rate_current, codeine.release_rate_target, codeine.release_step * App.fixed_update_interval_s);

			if (codeine.amount > 0.00f)
			{
				var amount_released = Maths.Clamp(codeine.release_rate_current, 0.00f, codeine.amount) * App.fixed_update_interval_s;

				codeine.amount_active += amount_released;
				codeine.amount -= amount_released;
			}

			var total_mass = 70.00f; // TODO

			codeine.amount_metabolized = Maths.Clamp(codeine.amount_active * organic.absorption * metabolization_modifier, 0.00f, codeine.amount_active) * App.fixed_update_interval_s;
			codeine.amount_withdrawal = Maths.Lerp2(codeine.amount_withdrawal, codeine.amount_metabolized * 0.65f, 0.0008f, 0.0001f);
			codeine.amount_active -= codeine.amount_metabolized * elimination_modifier;

			codeine.modifier_current = codeine.amount_metabolized / (total_mass * 0.001f) * 0.50f;
			codeine.modifier_withdrawal = codeine.amount_withdrawal / (total_mass * 0.001f) * 0.50f;

#if SERVER
			if (codeine.amount <= 0.01f && codeine.amount_active <= 0.01f && codeine.modifier_withdrawal <= 0.05f)
			{
				entity.RemoveComponent<Codeine.Effect>();
			}
#endif
		}

#if CLIENT
		[ISystem.PreUpdate.Reset(ISystem.Mode.Single)]
		public static void UpdateCamera(ISystem.Info info, Entity entity, [Source.Global] ref Camera.Global camera, [Source.Shared] in Player.Data player, [Source.Owned] in Codeine.Effect codeine)
		{
			if (player.IsLocal())
			{
				var modifier = MathF.Pow(codeine.modifier_current, 1.10f);

				camera.damp_modifier /= 1.00f + (modifier * 1.20f);

				Drunk.Color.W = MathF.Max(Drunk.Color.W, Maths.Clamp(modifier * 1.15f, 0.00f, 0.95f));
			}
		}
#endif
	}
}