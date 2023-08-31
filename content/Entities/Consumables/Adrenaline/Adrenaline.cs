
namespace TC2.Base.Components
{
	public static partial class Adrenaline
	{
		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct Effect: IComponent
		{
			[Statistics.Info("Adrenaline", description: "TODO: Desc", format: "{0:0.##} mg", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
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
		public static void OnConsume(ISystem.Info info, Entity entity, ref Consumable.ConsumeEvent data, [Source.Owned] in Consumable.Data consumable, [Source.Owned] ref Adrenaline.Effect adrenaline)
		{
			ref var adrenaline_new = ref data.ent_organic.GetOrAddComponent<Adrenaline.Effect>(sync: true);

			var ratio = Maths.SumRatio(adrenaline.amount, adrenaline_new.amount);

			adrenaline_new.amount += adrenaline.amount;
			adrenaline_new.release_rate_target = Maths.Lerp(adrenaline_new.release_rate_target + (adrenaline_new.release_rate_target * ratio), consumable.release_rate, ratio);
			adrenaline_new.release_step = Maths.Lerp(adrenaline_new.release_step, consumable.release_step, ratio);

			//App.WriteLine($"ratio: {ratio}");
		}
#endif

		public static Gradient<float> gr_consciousness = new Gradient<float>(1.00f, 1.55f, 1.75f, 1.60f, 1.10f);
		public static Gradient<float> gr_endurance = new Gradient<float>(1.00f, 1.25f, 1.60f, 2.10f, 2.50f, 3.00f);
		public static Gradient<float> gr_dexterity = new Gradient<float>(1.00f, 1.25f, 1.35f, 0.80f, 0.70f);
		public static Gradient<float> gr_strength = new Gradient<float>(1.00f, 1.45f, 1.80f, 2.10f, 2.40f, 2.80f);
		public static Gradient<float> gr_motorics = new Gradient<float>(1.00f, 1.15f, 1.25f, 1.40f, 1.60f, 1.85f);
		public static Gradient<float> gr_coordination = new Gradient<float>(1.00f, 0.95f, 0.80f, 0.60f, 0.35f);
		public static Gradient<float> gr_pain_modifier = new Gradient<float>(1.00f, 0.75f, 0.60f, 0.40f, 0.20f);

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateStats(ISystem.Info info, Entity entity,
		[Source.All] ref Adrenaline.Effect adrenaline, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			var modifier_a = adrenaline.modifier_current;

			organic.consciousness *= gr_consciousness.GetValue(modifier_a * 1.00f);
			organic.endurance *= gr_endurance.GetValue(modifier_a * 1.00f);
			organic.dexterity *= gr_dexterity.GetValue(modifier_a * 1.00f);
			organic.strength *= gr_strength.GetValue(modifier_a * 1.00f);
			organic.motorics *= gr_motorics.GetValue(modifier_a * 1.00f);
			organic.coordination *= gr_coordination.GetValue(modifier_a * 1.00f);
			organic.pain_modifier *= gr_pain_modifier.GetValue(modifier_a * 1.00f);

			var modifier_b = (adrenaline.modifier_withdrawal - adrenaline.modifier_current).Clamp0X();
			if (modifier_b > 0.00f)
			{
				organic.endurance *= Maths.Lerp01(1.00f, 0.50f, modifier_b * 1.50f);
				organic.strength *= Maths.Lerp01(1.00f, 0.50f, modifier_b * 1.10f);
				organic.dexterity *= Maths.Lerp01(1.00f, 0.40f, modifier_b * 1.40f);
				organic.motorics *= Maths.Lerp01(1.00f, 0.50f, modifier_b * 1.40f);
				organic.coordination *= Maths.Lerp01(1.00f, 0.30f, modifier_b * 1.50f);
			}
		}

		public static float metabolization_modifier = 0.20f;
		public static float elimination_modifier = 2.00f;
		public static float weight_scale = 4.00f;

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateAmount(ISystem.Info info, Entity entity, [Source.Owned] ref Adrenaline.Effect adrenaline, [Source.Owned, Override] in Organic.Data organic)
		{
			//elimination_modifier = 2;
			//weight_scale = 4;
			adrenaline.release_rate_current = Maths.MoveTowards(adrenaline.release_rate_current, adrenaline.release_rate_target, adrenaline.release_step * App.fixed_update_interval_s);

			if (adrenaline.amount > 0.00f)
			{
				var amount_released = Maths.Clamp(adrenaline.release_rate_current, 0.00f, adrenaline.amount) * App.fixed_update_interval_s;

				adrenaline.amount_active += amount_released;
				adrenaline.amount -= amount_released;
			}

			var total_mass = 70.00f; // TODO

			adrenaline.amount_metabolized = Maths.Clamp(adrenaline.amount_active * organic.absorption * metabolization_modifier, 0.10f, adrenaline.amount_active) * App.fixed_update_interval_s;
			adrenaline.amount_withdrawal = Maths.Lerp2(adrenaline.amount_withdrawal, adrenaline.amount_metabolized * 0.65f, 0.004f, 0.0008f);
			adrenaline.amount_active -= adrenaline.amount_metabolized * elimination_modifier;

			//adrenaline.modifier_current = adrenaline.amount_metabolized / (total_mass * 0.001f);
			adrenaline.modifier_current = Maths.Lerp(adrenaline.modifier_current, (adrenaline.amount_metabolized * weight_scale) / (total_mass * 0.001f), 0.02f);
			adrenaline.modifier_withdrawal = Maths.Lerp(adrenaline.modifier_withdrawal, (adrenaline.amount_withdrawal * weight_scale * 3) / (total_mass * 0.001f), 0.007f);

#if SERVER
			if (adrenaline.amount <= 0.0002f && adrenaline.amount_active <= 0.0002f && adrenaline.modifier_withdrawal <= 0.002f)
			{
				entity.RemoveComponent<Adrenaline.Effect>();
			}
#endif
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned), HasRelation(Source.Modifier.Shared, Relation.Type.Seat, false)]
		public static void UpdatePuncher(ISystem.Info info, Entity entity, Entity ent_body,
		[Source.Owned] in Arm.Data arm, [Source.Owned, Override] ref Puncher.Data puncher, [Source.Owned] ref Puncher.State puncher_state, [Source.Shared] in Adrenaline.Effect adrenaline)
		{
			var modifier = adrenaline.modifier_current;
			puncher.cooldown *= Maths.Lerp01(1.00f, 0.50f, modifier * 2.50f);
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateMovement(ISystem.Info info, [Source.Owned, Override] ref Runner.Data runner, [Source.Parent] in Adrenaline.Effect adrenaline)
		{
			var modifier = adrenaline.modifier_current;

			runner.crouch_speed_modifier = Maths.Lerp01(runner.crouch_speed_modifier, 0.80f, modifier * 3.00f);
			runner.max_speed *= Maths.Lerp01(1.00f, 1.40f, modifier * 3.00f);
			runner.walk_lerp = Maths.Lerp01(runner.walk_lerp, 0.35f, modifier * 3.00f);
		}

#if CLIENT
		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Shared)]
		public static void OnGUI(ISystem.Info info, Entity entity, [Source.Shared] in Player.Data player, [Source.Owned] in Adrenaline.Effect adrenaline)
		{
			var color = GUI.font_color_default;
			//color = Color32BGRA.FromHSV((1.00f - Maths.Clamp01(alcohol.modifier_current)) * 2.00f, 1.00f, 1.00f);

			if (adrenaline.modifier_current <= 0.05f) color = GUI.font_color_default;
			else if (adrenaline.modifier_current <= 0.35f) color = GUI.font_color_green;
			else if (adrenaline.modifier_current <= 0.55f) color = GUI.font_color_yellow;
			else if (adrenaline.modifier_current <= 0.75f) color = GUI.font_color_orange;
			else color = GUI.font_color_red;

			IStatusEffect.ScheduleDraw(new()
			{
				icon = "ui_icon_effect.alcohol",
				//icon_extra = "beer",
				value = $"Stimmed\n{adrenaline.modifier_current.Clamp01():P2} / {adrenaline.modifier_withdrawal.Clamp01():P2}",
				text_color = color,
				name = $"Adrenaline\nAmount: {adrenaline.amount:0.00}\nActive: {adrenaline.amount_active:0.00}"
			});
		}

		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Shared)]
		public static void UpdateCamera(ref XorRandom random, ISystem.Info info, Entity entity, [Source.Singleton] ref Camera.Singleton camera, [Source.Shared] in Player.Data player, [Source.Owned] in Adrenaline.Effect adrenaline, [Source.Singleton] ref Head.Singleton head_global)
		{
			var modifier = MathF.Pow(adrenaline.modifier_current, 1.40f);

			var pos_modifier = MathF.Pow((adrenaline.modifier_current - 0.10f).Clamp0X(), 1.30f);
			if (pos_modifier > 0.00f) camera.position_offset = random.NextUnitVector2Range(0.00f, 0.40f) * pos_modifier;

			camera.damp_modifier *= 1.00f + (modifier * 2.00f);
			camera.zoom_modifier *= Maths.Lerp01(1.00f, 0.60f, (modifier * 3.50f).Clamp0X());

			Drunk.Color.W = MathF.Max(Drunk.Color.W, Maths.Clamp(MathF.Max(modifier, adrenaline.modifier_withdrawal) * 3.70f, 0.00f, 0.55f));

			ref var low_pass = ref Audio.LowPass;
			low_pass.frequency = 10000.00f;
			low_pass.resonance = MathF.Pow(0.70f + (modifier * 8.50f), 2.50f);

			ref var high_pass = ref Audio.HighPass;
			high_pass.frequency = 150.00f;
			high_pass.resonance = MathF.Pow(0.70f + (modifier * 5.50f), 1.50f);

			Postprocess.Contrast = Maths.Lerp(Postprocess.Contrast, Postprocess.Contrast + 0.25f, (modifier * 8.50f).Clamp01());
			Postprocess.Brightness = Maths.Lerp(Postprocess.Brightness, Postprocess.Brightness + 0.55f, (modifier * 4.00f).Clamp01());
			Postprocess.Saturation = Maths.Lerp(Postprocess.Saturation, Postprocess.Saturation - 0.90f, (modifier * 8.00f).Clamp01());
		}
#endif
	}
}