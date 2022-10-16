
namespace TC2.Base.Components
{
	public static partial class Paxilon
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Effect: IComponent
		{
			[Statistics.Info("Paxilon Hydrochlorate", description: "TODO: Desc", format: "{0:0.##} mg", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
			public float amount;

			public float amount_active;
			public float amount_metabolized;
			public float amount_withdrawal;

			public float release_rate_current;
			public float release_rate_target;
			public float release_step;

			[Net.Ignore] public float modifier_current;
		}

#if SERVER
		[ISystem.Event<Consumable.ConsumeEvent>(ISystem.Mode.Single)]
		public static void OnConsume(ISystem.Info info, Entity entity, ref Consumable.ConsumeEvent data, [Source.Owned] in Consumable.Data consumable, [Source.Owned] ref Paxilon.Effect paxilon)
		{
			ref var paxilon_new = ref data.ent_organic.GetOrAddComponent<Paxilon.Effect>(sync: true);

			var ratio = Maths.SumRatio(paxilon.amount, paxilon_new.amount);

			paxilon_new.amount += paxilon.amount;
			paxilon_new.release_rate_target = Maths.Lerp(paxilon_new.release_rate_target + (paxilon_new.release_rate_target * ratio), consumable.release_rate, ratio);
			paxilon_new.release_step = Maths.Lerp(paxilon_new.release_step, consumable.release_step, ratio);

			App.WriteLine($"ratio: {ratio}");
		}
#endif

		public static Gradient gr_consciousness = new Gradient(1.00f, 0.75f, 0.60f, 0.50f, 0.50f);
		public static Gradient gr_dexterity = new Gradient(1.00f, 0.50f, 0.00f);
		public static Gradient gr_strength = new Gradient(1.00f, 0.30f, 0.00f);
		public static Gradient gr_motorics = new Gradient(1.00f, 0.20f, 0.00f);
		public static Gradient gr_coordination = new Gradient(1.00f, 0.50f, 0.10f, 0.00f);
		public static Gradient gr_pain_modifier = new Gradient(1.00f, 0.95f, 0.70f, 0.30f, 0.00f);

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateStats(ISystem.Info info, Entity entity,
		[Source.All] ref Paxilon.Effect paxilon, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			var modifier_a = paxilon.modifier_current;

			organic.consciousness *= gr_consciousness.GetValue(modifier_a * 1.90f);
			organic.dexterity *= gr_dexterity.GetValue(modifier_a * 0.90f);
			organic.strength *= gr_strength.GetValue(modifier_a * 4.90f);
			organic.motorics *= gr_motorics.GetValue(modifier_a * 1.20f);
			organic.coordination *= gr_coordination.GetValue(modifier_a * 1.20f);
			organic.pain_modifier *= gr_pain_modifier.GetValue(modifier_a * 1.00f);
		}

		public static float metabolization_modifier = 0.05f;
		public static float elimination_modifier = 0.75f;

		[ISystem.VeryLateUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateAmount(ISystem.Info info, Entity entity, [Source.Owned] ref Paxilon.Effect paxilon, [Source.Owned, Override] in Organic.Data organic)
		{
			paxilon.release_rate_current = Maths.MoveTowards(paxilon.release_rate_current, paxilon.release_rate_target, paxilon.release_step * App.fixed_update_interval_s);

			if (paxilon.amount > 0.00f)
			{
				var amount_released = Maths.Clamp(paxilon.release_rate_current, 0.00f, paxilon.amount) * App.fixed_update_interval_s;

				paxilon.amount_active += amount_released;
				paxilon.amount -= amount_released;
			}

			var total_mass = 70.00f; // TODO

			paxilon.amount_metabolized = Maths.Clamp(paxilon.amount_active * organic.absorption * metabolization_modifier, 0.00f, paxilon.amount_active) * App.fixed_update_interval_s;
			paxilon.amount_active -= paxilon.amount_metabolized * elimination_modifier;

			paxilon.modifier_current = paxilon.amount_metabolized / (total_mass * 0.001f) * 1.00f;

#if SERVER
			if (paxilon.amount <= 0.01f && paxilon.amount_active <= 0.01f)
			{
				entity.RemoveComponent<Paxilon.Effect>();
			}
#endif
		}

#if CLIENT
		[ISystem.PreUpdate.Reset(ISystem.Mode.Single)]
		public static void UpdateCamera(ISystem.Info info, Entity entity, [Source.Global] ref Camera.Global camera, [Source.Shared] in Player.Data player, [Source.Owned] in Paxilon.Effect paxilon)
		{
			if (player.IsLocal())
			{
				var modifier = MathF.Pow(paxilon.modifier_current, 1.10f);

				camera.damp_modifier /= 1.00f + (modifier * 1.50f);

				Drunk.Color.W = MathF.Max(Drunk.Color.W, Maths.Clamp(modifier * 0.85f, 0.00f, 0.95f));
			}
		}
#endif
	}
}