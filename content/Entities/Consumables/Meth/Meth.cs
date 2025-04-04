
namespace TC2.Base.Components
{
	public static partial class Meth
	{
		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
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
		[ISystem.Event<Consumable.ConsumeEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnConsume(ISystem.Info info, Entity entity, ref Consumable.ConsumeEvent data, [Source.Owned] in Consumable.Data consumable, [Source.Owned] ref Meth.Effect meth)
		{
			ref var meth_new = ref data.ent_organic.GetOrAddComponent<Meth.Effect>(sync: true);

			var ratio = Maths.SumRatio(meth.amount, meth_new.amount);

			meth_new.amount += meth.amount;
			meth_new.release_rate_target = Maths.Lerp(meth_new.release_rate_target + (meth_new.release_rate_target * ratio), consumable.release_rate, ratio);
			meth_new.release_step = Maths.Lerp(meth_new.release_step, consumable.release_step, ratio);

			//App.WriteLine($"ratio: {ratio}");
		}
#endif

		public static Gradient<float> gr_consciousness = [1.00f, 1.35f, 1.55f, 1.62f, 1.74f, 1.66f, 1.54f, 1.50f, 1.30f, 0.90f, 0.80f, 0.75f, 0.70f, 0.00f];
		public static Gradient<float> gr_endurance = [1.00f, 1.05f, 1.10f, 1.15f, 1.30f, 1.40f, 1.55f, 1.70f, 1.90f, 2.10f, 2.50f, 3.00f];
		public static Gradient<float> gr_dexterity = [1.00f, 1.15f, 1.30f, 1.45f, 1.35f, 1.50f, 1.65f, 1.75f, 1.85f, 1.10f, 0.85f, 0.80f];
		public static Gradient<float> gr_strength = [1.00f, 1.02f, 1.04f, 1.07f, 1.10f, 1.25f, 1.50f, 1.80f, 2.10f, 2.40f, 2.60f, 2.80f];
		public static Gradient<float> gr_motorics = [1.00f, 1.05f, 1.15f, 1.25f, 1.40f, 1.60f, 1.85f, 1.90f, 1.95f, 1.85f, 1.40f, 1.20f, 0.95f, 0.80f, 0.60f];
		public static Gradient<float> gr_coordination = [1.00f, 1.05f, 1.10f, 1.20f, 1.25f, 1.22f, 0.98f, 0.82f, 0.77f, 0.69f, 0.60f, 0.50f];
		public static Gradient<float> gr_pain_modifier = [1.00f, 1.00f, 0.95f, 0.85f, 0.80f, 0.70f, 0.45f, 0.30f, 0.14f, 0.08f, 0.04f, 0.02f];
		public static Gradient<float> gr_work_modifier = [1.00f, 1.11f, 1.16f, 1.22f, 1.34f, 1.60f, 1.55f, 1.20f, 0.98f, 0.90f, 0.60f, 0.10f];
		public static Gradient<float> gr_smartness = [1.01f, 1.02f, 1.06f, 1.11f, 1.25f, 1.40f, 1.17f, 1.02f, 0.90f, 0.85f, 0.70f, 0.65f];

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
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
			organic.work_speed = gr_work_modifier.GetValue(modifier_a * 1.00f);
			organic.smartness = gr_smartness.GetValue(modifier_a * 1.00f);

			var modifier_b = (meth.modifier_withdrawal - meth.modifier_current).Clamp0X();
			if (modifier_b > Maths.epsilon)
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
		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateControls(ISystem.Info info, Entity entity, ref XorRandom random, [Source.SharedAny] ref Meth.Effect meth, [Source.SharedAny, Override] in Organic.Data organic, [Source.Owned] in Arm.Data arm, [Source.SharedAny] ref Control.Data control)
		{
			if (info.WorldTime >= meth.next_spasm)
			{
				if (meth.modifier_current > 0.25f && random.NextBool(0.001f * meth.modifier_current))
				{
					//App.WriteLine("spasm");

					control.mouse.SetKeyPressed(Mouse.Key.Left, true);
					meth.next_spasm = info.WorldTime + (random.NextFloatRange(3.00f, 15.00f) * Maths.Lerp01(1.00f, 0.50f, meth.modifier_current));
				}
			}
		}
#endif

		public static float metabolization_modifier = 0.02f;
		public static float elimination_modifier = 0.40f;

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateAmount(ISystem.Info info, Entity entity, [Source.Owned] ref Meth.Effect meth, [Source.Owned, Override] in Organic.Data organic)
		{
			meth.release_rate_current = Maths.MoveTowards(meth.release_rate_current, meth.release_rate_target, meth.release_step * App.fixed_update_interval_s);

			if (meth.amount > Maths.epsilon)
			{
				var amount_released = Maths.Clamp(meth.release_rate_current, 0.00f, meth.amount) * App.fixed_update_interval_s;

				meth.amount_active += amount_released;
				meth.amount -= amount_released;
			}

			var total_mass = 70.00f; // TODO

			meth.amount_metabolized = Maths.Clamp(meth.amount_active * organic.absorption * metabolization_modifier, 0.10f, meth.amount_active) * App.fixed_update_interval_s;
			meth.amount_withdrawal = Maths.Lerp2(meth.amount_withdrawal, meth.amount_metabolized * 0.65f, 0.0008f, 0.0001f);
			meth.amount_active -= meth.amount_metabolized * elimination_modifier;

			//meth.modifier_current = meth.amount_metabolized / (total_mass * 0.001f);
			meth.modifier_current = Maths.Lerp(meth.modifier_current, (meth.amount_metabolized * 1.50f) / (total_mass * 0.001f), 0.02f);
			meth.modifier_withdrawal = meth.amount_withdrawal / (total_mass * 0.001f);

#if SERVER
			if (meth.amount <= 0.01f && meth.amount_active <= 0.01f && meth.modifier_withdrawal <= 0.05f)
			{
				entity.RemoveComponent<Meth.Effect>();
			}
#endif
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned), HasRelation(Source.Modifier.Shared, Relation.Type.Seat, false)]
		public static void UpdatePuncher(ISystem.Info info, Entity entity, Entity ent_body,
		[Source.Owned] in Arm.Data arm, [Source.Owned, Override] ref Puncher.Data puncher, [Source.Shared] in Meth.Effect meth)
		{
			var modifier = meth.modifier_current;
			puncher.cooldown *= Maths.Lerp01(1.00f, 0.50f, modifier * 2.50f);
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateMovement(ISystem.Info info, [Source.Owned, Override] ref Runner.Data runner, [Source.Parent] in Meth.Effect meth)
		{
			var modifier = meth.modifier_current;

			runner.air_brake_modifier += Maths.Lerp01(0.00f, 0.50f, modifier * 3.00f);
			runner.crouch_speed_modifier = Maths.Lerp01(runner.crouch_speed_modifier, 0.80f, modifier * 3.00f);
			runner.max_speed *= Maths.Lerp01(1.00f, 1.20f, modifier * 3.00f);
			runner.walk_lerp = Maths.Lerp01(runner.walk_lerp, 0.75f, modifier * 3.00f);

			var jump_multiplier = Maths.Lerp01(1.00f, 1.30f, modifier * 3.00f);
			runner.jump_decay /= jump_multiplier * 1.30f;
			runner.max_jump_speed *= jump_multiplier;
		}

#if CLIENT
		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Shared)]
		public static void OnGUI(ISystem.Info info, Entity entity, [Source.Shared] in Player.Data player, [Source.Owned] in Meth.Effect meth)
		{
			var color = GUI.font_color_default;
			//color = Color32BGRA.FromHSV((1.00f - Maths.Clamp01(alcohol.modifier_current)) * 2.00f, 1.00f, 1.00f);

			if (meth.modifier_current <= 0.05f) color = GUI.font_color_default;
			else if (meth.modifier_current <= 0.15f) color = GUI.font_color_green;
			else if (meth.modifier_current <= 0.45f) color = GUI.font_color_yellow;
			else if (meth.modifier_current <= 0.75f) color = GUI.font_color_orange;
			else color = GUI.font_color_red;

			IStatusEffect.ScheduleDraw(new()
			{
				icon = "ui_icon_effect.alcohol",
				//icon_extra = "beer",
				value = $"Stimmed\n{meth.modifier_current.Clamp01():P2}",
				text_color = color,
				name = $"Meth\nAmount: {meth.amount:0.00}\nActive: {meth.amount_active:0.00}"
			});
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Shared)]
		public static void UpdateCamera(ref XorRandom random, ISystem.Info info, Entity entity, 
		[Source.Singleton] ref Camera.Singleton camera, [Source.Shared] in Player.Data player, 
		[Source.Owned] in Meth.Effect meth, [Source.Singleton] ref Head.Singleton head_global)
		{
			var modifier = MathF.Pow(meth.modifier_current, 1.40f);

			var pos_modifier = MathF.Pow((meth.modifier_current - 0.20f).Clamp0X(), 1.20f);
			if (pos_modifier > Maths.epsilon) camera.position_offset = random.NextUnitVector2Range(0.00f, 0.40f) * pos_modifier;

			camera.damp_modifier *= 1.00f + (modifier * 14.00f);
			camera.distance_modifier += modifier * 0.10f;
			camera.distance_modifier *= 1.00f + (modifier * 2.00f);
			camera.zoom_modifier *= Maths.Lerp01(1.00f, 0.30f, (modifier - 0.20f).Clamp0X());

			Drunk.Color.a = Maths.Max(Drunk.Color.a, Maths.Clamp(modifier * 0.70f, 0.00f, 0.95f));

			ref var low_pass = ref Audio.LowPass;
			low_pass.frequency = Maths.Lerp01(low_pass.frequency, 10000.00f, modifier * 2.50f);
			low_pass.resonance = MathF.Pow(0.70f + (Maths.Clamp01((modifier - 0.20f) * 2.00f) * 8.50f), 2.50f);

			ref var high_pass = ref Audio.HighPass;
			high_pass.frequency = Maths.Lerp01(high_pass.frequency, 150.00f, modifier * 2.50f);
			high_pass.resonance = MathF.Pow(0.70f + (Maths.Clamp01((modifier - 0.30f) * 2.00f) * 5.50f), 1.50f);

			if (meth.modifier_current < meth.modifier_withdrawal)
			{
				Drunk.Color.a = Maths.Max(Drunk.Color.a, Maths.Clamp(meth.modifier_withdrawal * 1.50f, 0.00f, 0.90f));
			}

			head_global.tinnitus_volume = MathF.Pow(Maths.Clamp01(modifier - 0.25f) * 2.00f, 2.00f);

			Postprocess.Contrast = Maths.Lerp(Postprocess.Contrast, Postprocess.Contrast + 0.25f, (modifier * 2.50f).Clamp01());
			Postprocess.Brightness = Maths.Lerp(Postprocess.Brightness, Postprocess.Brightness + 0.55f, (modifier * 1.10f).Clamp01());
			Postprocess.Saturation = Maths.Lerp(Postprocess.Saturation, Postprocess.Saturation + 0.30f, (modifier * 1.20f).Clamp01());
		}
#endif
	}
}