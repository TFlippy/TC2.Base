
namespace TC2.Base.Components
{
	public static partial class Pneumoconiosis
	{
		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct Effect: IComponent
		{
			public float modifier_current;

			public float amount_carbon;
			public float amount_silica;
			public float amount_metals;
			public float amount_spores;
			public float amount_dust;

			[Net.Ignore, Save.Ignore] public float t_next_cough;
		}

		public static Sound.Handle[] sounds_cough =
		[
			"cough.00",
			"cough.01",
			"cough.02",
			"cough.03",
			"cough.04",
		];

		public static Gradient<float> gr_consciousness = new Gradient<float>(1.00f, 1.00f, 0.99f, 0.98f, 0.98f, 0.96f, 0.93f, 0.90f, 0.85f, 0.82f, 0.78f, 0.72f, 0.64f, 0.57f, 0.51f, 0.42f, 0.30f, 0.14f, 0.00f);
		public static Gradient<float> gr_endurance = new Gradient<float>(1.00f, 0.98f, 0.95f, 0.93f, 0.90f, 0.87f, 0.82f, 0.75f, 0.65f, 0.50f, 0.35f, 0.20f);
		public static Gradient<float> gr_dexterity = new Gradient<float>(1.00f, 1.00f, 0.99f, 0.98f, 0.98f, 0.96f, 0.93f, 0.90f, 0.85f, 0.82f, 0.78f, 0.70f);
		public static Gradient<float> gr_strength = new Gradient<float>(1.00f, 1.00f, 0.99f, 0.98f, 0.98f, 0.96f, 0.93f, 0.90f, 0.85f, 0.82f, 0.78f, 0.70f);
		public static Gradient<float> gr_pain_modifier = new Gradient<float>(1.00f, 1.00f, 1.02f, 1.05f, 1.08f, 1.12f, 1.15f, 1.20f);

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateStats(ISystem.Info info, Entity entity,
		[Source.All] ref Pneumoconiosis.Effect pneumoconiosis, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			var modifier_a = pneumoconiosis.modifier_current;

			organic.consciousness *= gr_consciousness.GetValue(modifier_a * 1.00f);
			organic.endurance *= gr_endurance.GetValue(modifier_a * 1.00f);
			organic.dexterity *= gr_dexterity.GetValue(modifier_a * 1.00f);
			organic.strength *= gr_strength.GetValue(modifier_a * 1.00f);
			organic.pain_modifier *= gr_pain_modifier.GetValue(modifier_a * 1.00f);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateAmount(ISystem.Info info, Entity entity, [Source.Owned] ref Pneumoconiosis.Effect pneumoconiosis, [Source.Owned, Override] in Organic.Data organic)
		{

		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateMovement(ISystem.Info info, [Source.Owned, Override] ref Runner.Data runner, [Source.Parent] in Pneumoconiosis.Effect pneumoconiosis)
		{
			//var modifier = pneumoconiosis.modifier_current;

			//runner.crouch_speed_modifier = Maths.Lerp01(runner.crouch_speed_modifier, 0.80f, modifier * 3.00f);
			//runner.max_speed *= Maths.Lerp01(1.00f, 1.40f, modifier * 3.00f);
			//runner.walk_lerp = Maths.Lerp01(runner.walk_lerp, 0.35f, modifier * 3.00f);
		}

#if CLIENT
		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Shared)]
		public static void OnGUI(ISystem.Info info, Entity entity, [Source.Shared] in Player.Data player, [Source.Owned] in Pneumoconiosis.Effect pneumoconiosis)
		{
			var color = GUI.font_color_default;
			//color = Color32BGRA.FromHSV((1.00f - Maths.Clamp01(alcohol.modifier_current)) * 2.00f, 1.00f, 1.00f);

			if (pneumoconiosis.modifier_current <= 0.05f) color = GUI.font_color_default;
			else if (pneumoconiosis.modifier_current <= 0.35f) color = GUI.font_color_green;
			else if (pneumoconiosis.modifier_current <= 0.55f) color = GUI.font_color_yellow;
			else if (pneumoconiosis.modifier_current <= 0.75f) color = GUI.font_color_orange;
			else color = GUI.font_color_red;

			IStatusEffect.ScheduleDraw(new()
			{
				icon = "ui_icon_effect.pneumoconiosis",
				value = $"Severity\n{pneumoconiosis.modifier_current:0.00}",
				text_color = color,
				name = $"Dusty Lungs"
			});
		}

		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Shared)]
		public static void UpdateCamera(ref XorRandom random, ISystem.Info info, Entity entity, [Source.Singleton] ref Camera.Singleton camera, [Source.Shared] in Player.Data player, [Source.Owned] in Pneumoconiosis.Effect pneumoconiosis, [Source.Singleton] ref Head.Singleton head_global)
		{
			var modifier = MathF.Pow(pneumoconiosis.modifier_current, 1.40f);


		}
#endif
	}
}