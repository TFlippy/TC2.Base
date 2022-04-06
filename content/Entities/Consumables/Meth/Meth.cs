
namespace TC2.Base.Components
{
	public static partial class Meth
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Effect: IComponent
		{
			[Statistics.Info("Meth", description: "TODO: Desc", format: "{0:0.##} ml", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
			public float amount;

			public float amount_active;
			public float amount_metabolized;
			public float modifier_withdrawal;

			public float release_rate_current;
			public float release_rate_target;
			public float release_step;

			[Net.Ignore] public float modifier_current;

			[Save.Ignore, Net.Ignore] public float next_spasm;
		}

#if SERVER
		[ISystem.Event<Consumable.ConsumeEvent>(ISystem.Mode.Single)]
		public static void OnConsume(ISystem.Info info, Entity entity, ref Consumable.ConsumeEvent data, [Source.Owned] in Consumable.Data consumable, [Source.Owned] ref Meth.Effect meth)
		{
			ref var meth_new = ref data.ent_organic.GetOrAddComponent<Meth.Effect>(sync: true);

			var ratio = Maths.Ratio(meth.amount, meth_new.amount);

			meth_new.amount += meth.amount;
			meth_new.release_rate_target = Maths.Lerp(meth_new.release_rate_target + (meth_new.release_rate_target * ratio), consumable.release_rate, ratio);
			meth_new.release_step = Maths.Lerp(meth_new.release_step, consumable.release_step, ratio);

			App.WriteLine($"ratio: {ratio}");
		}
#endif

		public static Gradient gr_consciousness = new Gradient(1.00f, 1.35f, 1.55f, 1.62f, 1.74f, 1.66f, 1.54f, 1.50f, 1.30f, 0.90f, 0.75f, 0.50f, 0.30f, 0.00f);
		public static Gradient gr_endurance = new Gradient(1.00f, 1.05f, 1.10f, 1.15f, 1.30f, 1.40f, 1.55f, 1.70f, 1.90f, 2.10f, 2.50f, 3.00f);
		public static Gradient gr_dexterity = new Gradient(1.00f, 1.15f, 1.30f, 1.45f, 1.35f, 1.10f, 0.80f, 0.60f, 0.50f, 0.40f, 0.30f, 0.20f);
		public static Gradient gr_strength = new Gradient(1.00f, 1.02f, 1.04f, 1.07f, 1.10f, 1.25f, 1.50f, 1.80f, 2.10f, 2.40f, 2.60f, 2.80f);
		public static Gradient gr_motorics = new Gradient(1.00f, 1.45f, 1.70f, 1.85f, 1.90f, 1.80f, 1.05f, 0.98f, 0.95f, 0.90f, 0.80f, 0.70f, 0.60f, 0.50f, 0.40f);
		public static Gradient gr_coordination = new Gradient(1.00f, 1.05f, 1.10f, 1.10f, 1.10f, 1.05f, 0.98f, 0.95f, 0.90f, 0.80f, 0.70f, 0.60f, 0.50f, 0.40f);

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateStats(ISystem.Info info, Entity entity,
		[Source.All] ref Meth.Effect meth, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			if (meth.modifier_current >= meth.modifier_withdrawal * 0.75f)
			{
				var modifier = meth.modifier_current;

				organic.consciousness *= gr_consciousness.GetValue(modifier * 1.00f);
				organic.endurance *= gr_endurance.GetValue(modifier * 1.00f);
				organic.dexterity *= gr_dexterity.GetValue(modifier * 1.00f);
				organic.strength *= gr_strength.GetValue(modifier * 1.00f);
				organic.motorics *= gr_motorics.GetValue(modifier * 1.00f);
				organic.coordination *= gr_coordination.GetValue(modifier * 1.00f);
			}
			else
			{
				var modifier = meth.modifier_withdrawal;

				organic.consciousness *= Maths.Lerp01(1.00f, 0.60f, modifier);
				organic.endurance *= Maths.Lerp01(1.00f, 0.50f, modifier);
				organic.dexterity *= Maths.Lerp01(1.00f, 0.30f, modifier);
				organic.strength *= Maths.Lerp01(1.00f, 0.50f, modifier);
				organic.motorics *= Maths.Lerp01(1.00f, 0.40f, modifier);
				organic.coordination *= Maths.Lerp01(1.00f, 0.80f, modifier);
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateAmount(ISystem.Info info, Entity entity, [Source.Owned] ref Meth.Effect meth, [Source.Owned, Override] in Organic.Data organic)
		{
			meth.release_rate_current = Maths.MoveTowards(meth.release_rate_current, meth.release_rate_target, meth.release_step * App.fixed_update_interval_s);

			if (meth.amount > 0.00f)
			{
				var amount_released = Maths.Clamp(meth.release_rate_current, 0.00f, meth.amount) * App.fixed_update_interval_s;

				meth.amount_active += amount_released;
				meth.amount -= amount_released;
			}

			var total_mass = 70.00f; // TODO

			var amount_metabolized = Maths.Clamp(meth.amount_active * organic.absorption, 0.00f, meth.amount_active) * App.fixed_update_interval_s;

			meth.amount_metabolized = amount_metabolized;
			meth.modifier_current = amount_metabolized / (total_mass * 0.001f);
			meth.modifier_withdrawal = Maths.Lerp2(meth.modifier_withdrawal, meth.modifier_current * 0.65f, 0.0008f, 0.0001f);
			meth.amount_active -= amount_metabolized;

#if SERVER
			if (meth.amount <= 0.01f && meth.amount_active <= 0.01f && meth.modifier_withdrawal <= 0.01f)
			{
				entity.RemoveComponent<Meth.Effect>();
			}
#endif
		}

#if CLIENT
		[ISystem.PreUpdate.Reset(ISystem.Mode.Single)]
		public static void UpdateCamera(ISystem.Info info, Entity entity, [Source.Global] ref Camera.Global camera, [Source.Shared] in Player.Data player, [Source.Owned] in Meth.Effect meth)
		{
			if (player.IsLocal())
			{
				if (meth.modifier_current >= meth.modifier_withdrawal * 0.75f)
				{
					var modifier = MathF.Pow(meth.modifier_current, 1.30f);
					var random = XorRandom.New();

					camera.position_offset = random.NextUnitVector2Range(0.00f, 0.15f * modifier);
					camera.damp_modifier *= 1.00f + (modifier * 5.00f);

					Drunk.Color.W = Maths.Clamp(modifier * 0.50f, 0.00f, 0.50f);

					ref var low_pass = ref Audio.LowPass;
					low_pass.frequency = 10000.00f;
					low_pass.resonance = 2.50f;

					ref var high_pass = ref Audio.HighPass;
					high_pass.frequency = 150.00f;
					high_pass.resonance = 1.50f;
				}
				else
				{
					Drunk.Color.W = Maths.Clamp(meth.modifier_withdrawal * 1.50f, 0.00f, 0.90f);
				}
			}
		}
#endif
	}
}