
namespace TC2.Base.Components
{
	public static partial class Meth
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Effect: IComponent
		{
			[Statistics.Info("Methamphetamine", description: "TODO: Desc", format: "{0:0.##} mg", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
			public float amount;

			public float amount_active;
			public float amount_metabolized;
			public float amount_withdrawal;

			public float release_rate_current;
			public float release_rate_target;
			public float release_step;

			[Net.Ignore] public float modifier_current;
			[Net.Ignore] public float modifier_withdrawal;

			[Save.Ignore, Net.Ignore] public float next_spasm;
		}

#if SERVER
		[ISystem.Event<Consumable.ConsumeEvent>(ISystem.Mode.Single)]
		public static void OnConsume(ISystem.Info info, Entity entity, ref Consumable.ConsumeEvent data, [Source.Owned] in Consumable.Data consumable, [Source.Owned] ref Meth.Effect meth)
		{
			ref var meth_new = ref data.ent_organic.GetOrAddComponent<Meth.Effect>(sync: true);

			var ratio = Maths.SumRatio(meth.amount, meth_new.amount);

			meth_new.amount += meth.amount;
			meth_new.release_rate_target = Maths.Lerp(meth_new.release_rate_target + (meth_new.release_rate_target * ratio), consumable.release_rate, ratio);
			meth_new.release_step = Maths.Lerp(meth_new.release_step, consumable.release_step, ratio);

			App.WriteLine($"ratio: {ratio}");
		}
#endif

#if CLIENT
		[IGlobal.Data(persist: false), IComponent.With<Sound.Emitter>()]
		public struct Global: IGlobal
		{
			public float tinnitus_volume = 0.00f;

			public Global()
			{

			}
		}

		public static Sound.Handle sound_tinnitus = "tinnitus.loop.00";

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateGlobalSound(ISystem.Info info, Entity entity, [Source.Owned] ref Meth.Global meth_global, [Source.Owned] ref Sound.Emitter sound_emitter)
		{
			sound_emitter.file = sound_tinnitus;
			sound_emitter.volume = Maths.Lerp(sound_emitter.volume, meth_global.tinnitus_volume, 0.10f);
			sound_emitter.pitch = 1.10f;
			sound_emitter.mix_3d = 0.00f;

			meth_global.tinnitus_volume = 0.00f;
		}
#endif

		public static Gradient gr_consciousness = new Gradient(1.00f, 1.35f, 1.55f, 1.62f, 1.74f, 1.66f, 1.54f, 1.50f, 1.30f, 0.90f, 0.80f, 0.75f, 0.70f, 0.00f);
		public static Gradient gr_endurance = new Gradient(1.00f, 1.05f, 1.10f, 1.15f, 1.30f, 1.40f, 1.55f, 1.70f, 1.90f, 2.10f, 2.50f, 3.00f);
		public static Gradient gr_dexterity = new Gradient(1.00f, 1.15f, 1.30f, 1.45f, 1.35f, 1.20f, 1.15f, 1.00f, 0.95f, 0.90f, 0.85f, 0.80f);
		public static Gradient gr_strength = new Gradient(1.00f, 1.02f, 1.04f, 1.07f, 1.10f, 1.25f, 1.50f, 1.80f, 2.10f, 2.40f, 2.60f, 2.80f);
		public static Gradient gr_motorics = new Gradient(1.00f, 1.05f, 1.15f, 1.25f, 1.40f, 1.60f, 1.85f, 1.90f, 1.95f, 1.85f, 1.80f, 1.70f, 1.65f, 1.60f, 1.60f);
		public static Gradient gr_coordination = new Gradient(1.00f, 1.05f, 1.10f, 1.20f, 1.25f, 1.22f, 1.20f, 1.15f, 1.10f, 1.10f, 1.10f, 1.10f);
		public static Gradient gr_pain_modifier = new Gradient(1.00f, 1.00f, 0.95f, 0.85f, 0.80f, 0.70f, 0.45f, 0.30f, 0.14f, 0.08f, 0.04f, 0.02f);

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateStats(ISystem.Info info, Entity entity,
		[Source.All] ref Meth.Effect meth, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			var modifier_a = meth.modifier_current;

			organic.consciousness *= gr_consciousness.GetValue(modifier_a * 1.00f);
			organic.endurance *= gr_endurance.GetValue(modifier_a * 1.00f);
			organic.dexterity *= gr_dexterity.GetValue(modifier_a * 1.00f);
			organic.strength *= gr_strength.GetValue(modifier_a * 1.00f);
			organic.motorics *= gr_motorics.GetValue(modifier_a * 1.00f);
			organic.coordination *= gr_coordination.GetValue(modifier_a * 1.00f);
			organic.pain_modifier *= gr_pain_modifier.GetValue(modifier_a * 1.00f);

			var modifier_b = (meth.modifier_withdrawal - meth.modifier_current).Clamp0X();
			if (modifier_b > 0.00f)
			{
				organic.consciousness *= Maths.Lerp01(1.00f, 0.60f, modifier_b);
				organic.endurance *= Maths.Lerp01(1.00f, 0.50f, modifier_b);
				organic.dexterity *= Maths.Lerp01(1.00f, 0.30f, modifier_b);
				organic.strength *= Maths.Lerp01(1.00f, 0.50f, modifier_b);
				organic.motorics *= Maths.Lerp01(1.00f, 0.40f, modifier_b);
				organic.coordination *= Maths.Lerp01(1.00f, 0.80f, modifier_b);
			}
		}

#if SERVER
		[ISystem.EarlyUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateControls(ISystem.Info info, Entity entity, [Source.SharedAny] ref Meth.Effect meth, [Source.SharedAny, Override] in Organic.Data organic, [Source.Owned] in Arm.Data arm, [Source.SharedAny] ref Control.Data control)
		{
			if (info.WorldTime >= meth.next_spasm)
			{
				var random = XorRandom.New();
				if (meth.modifier_current > 0.25f && random.NextBool(0.001f * meth.modifier_current))
				{
					App.WriteLine("spasm");

					control.mouse.SetKeyPressed(Mouse.Key.Left, true);
					meth.next_spasm = info.WorldTime + (random.NextFloatRange(3.00f, 15.00f) * Maths.Lerp01(1.00f, 0.50f, meth.modifier_current));
				}
			}
		}
#endif

		public static float metabolization_modifier = 0.02f;
		public static float elimination_modifier = 0.40f;

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

			meth.amount_metabolized = Maths.Clamp(meth.amount_active * organic.absorption * metabolization_modifier, 0.00f, meth.amount_active) * App.fixed_update_interval_s;
			meth.amount_withdrawal = Maths.Lerp2(meth.amount_withdrawal, meth.amount_metabolized * 0.65f, 0.0008f, 0.0001f);
			meth.amount_active -= meth.amount_metabolized * elimination_modifier;

			meth.modifier_current = meth.amount_metabolized / (total_mass * 0.001f);
			meth.modifier_withdrawal = meth.amount_withdrawal / (total_mass * 0.001f);

#if SERVER
			if (meth.amount <= 0.01f && meth.amount_active <= 0.01f && meth.modifier_withdrawal <= 0.05f)
			{
				entity.RemoveComponent<Meth.Effect>();
			}
#endif
		}

#if CLIENT
		[ISystem.PreUpdate.Reset(ISystem.Mode.Single)]
		public static void UpdateCamera(ISystem.Info info, Entity entity, [Source.Global] ref Camera.Global camera, [Source.Shared] in Player.Data player, [Source.Owned] in Meth.Effect meth, [Source.Global] ref Meth.Global meth_global)
		{
			if (player.IsLocal())
			{
				var modifier = MathF.Pow(meth.modifier_current, 1.10f);
				var random = XorRandom.New();

				var pos_modifier = MathF.Pow((meth.modifier_current - 0.20f).Clamp0X(), 1.20f);
				if (pos_modifier > 0.00f) camera.position_offset = random.NextUnitVector2Range(0.00f, 0.40f) * pos_modifier;

				camera.damp_modifier *= 1.00f + (modifier * 5.00f);
				camera.zoom_modifier *= Maths.Lerp01(1.00f, 0.30f, (modifier - 0.20f).Clamp0X());

				Drunk.Color.W = MathF.Max(Drunk.Color.W, Maths.Clamp(modifier * 0.70f, 0.00f, 0.95f));

				ref var low_pass = ref Audio.LowPass;
				low_pass.frequency = 10000.00f;
				low_pass.resonance = MathF.Pow(0.70f + (modifier * 8.50f), 2.50f);

				ref var high_pass = ref Audio.HighPass;
				high_pass.frequency = 150.00f;
				high_pass.resonance = MathF.Pow(0.70f + (modifier * 5.50f), 1.50f);

				if (meth.modifier_current < meth.modifier_withdrawal)
				{
					Drunk.Color.W = MathF.Max(Drunk.Color.W, Maths.Clamp(meth.modifier_withdrawal * 1.50f, 0.00f, 0.90f));
				}

				meth_global.tinnitus_volume = MathF.Pow(Maths.Clamp01(modifier - 0.50f) * 2.00f, 3.00f);
			}
		}
#endif
	}
}