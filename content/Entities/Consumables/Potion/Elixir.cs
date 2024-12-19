
namespace TC2.Base.Components
{
	public static partial class Elixir
	{
		[Flags]
		public enum Flags: ushort
		{
			None = 0,


		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct Data(): IComponent
		{
			public ISubstance.Handle h_substance_solvent;
			public Elixir.Flags flags;

			public float amount;
			public float release_step;

			[Asset.Ignore] public float amount_bloodstream;

			[Asset.Ignore] public float release_rate_current;
			[Asset.Ignore] public float release_rate_target;
			[Asset.Ignore] public float hiccup_current;

			[Asset.Ignore, Net.Ignore] public float modifier_current;
			[Asset.Ignore, Net.Ignore] public float jitter_current;
			[Asset.Ignore, Net.Ignore] public float t_next_sound;
		}

#if SERVER
		[ISystem.Event<Consumable.ConsumeEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnConsume(ISystem.Info info, Entity entity, ref Consumable.ConsumeEvent data, [Source.Owned] in Consumable.Data consumable, [Source.Owned] ref Elixir.Data elixir_src)
		{
			ref var elixir_dst = ref data.ent_organic.GetOrAddComponent<Elixir.Data>(sync: true);

			switch (consumable.action)
			{
				case Consumable.Action.Inject:
				{
					elixir_dst.amount_bloodstream += elixir_src.amount * data.amount_modifier;
				}
				break;

				case Consumable.Action.Drink:
				{
					var ratio = Maths.SumRatio(elixir_src.amount * data.amount_modifier, elixir_dst.amount);

					elixir_dst.amount += elixir_src.amount * data.amount_modifier;
					elixir_dst.release_rate_target = Maths.Lerp(elixir_dst.release_rate_target + (elixir_dst.release_rate_target * ratio), consumable.release_rate, ratio);
					elixir_dst.release_step = Maths.Lerp(elixir_dst.release_step, consumable.release_step, ratio);

					//App.WriteLine($"ratio: {ratio}");
				}
				break;

				default:
				{
					var ratio = Maths.SumRatio(elixir_src.amount * data.amount_modifier, elixir_dst.amount);

					elixir_dst.amount += elixir_src.amount * data.amount_modifier;
					elixir_dst.release_rate_target = Maths.Lerp(elixir_dst.release_rate_target + (elixir_dst.release_rate_target * ratio), consumable.release_rate, ratio);
					elixir_dst.release_step = Maths.Lerp(elixir_dst.release_step, consumable.release_step, ratio);

					//App.WriteLine($"ratio: {ratio}");
				}
				break;
			}
		}
#endif

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateStats(ISystem.Info info, Entity entity,
		[Source.All] ref Elixir.Data elixir, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{

		}

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateHead(ISystem.Info info, Entity entity, ref XorRandom random, ref Region.Data region,
		[Source.Owned] in Transform.Data transform, [Source.All] ref Elixir.Data elixir, [Source.Owned] ref Head.Data head)
		{

		}

		//[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		//public static void UpdateRegen(ISystem.Info info, Entity entity, [Source.Owned, Override] ref Regen.Data regen, [Source.Any] in Elixir.Effect elixir)
		//{
		//	var modifier = elixir.modifier_current + (elixir.jitter_current * 0.75f);
		//}
#endif

		public static float metabolization_modifier = 0.20f;
		public static float elimination_modifier = 0.10f;
		public static float modifier_lerp = 0.001f;

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateAmount(ISystem.Info info, Entity entity, ref XorRandom random, [Source.Owned] ref Elixir.Data elixir, [Source.Owned, Override] in Organic.Data organic)
		{

		}

#if CLIENT
		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Shared)]
		public static void OnGUI(ISystem.Info info, Entity entity, [Source.Shared] in Player.Data player, [Source.Owned] in Elixir.Data elixir)
		{
			var color = GUI.font_color_default;
			//color = Color32BGRA.FromHSV((1.00f - Maths.Clamp01(elixir.modifier_current)) * 2.00f, 1.00f, 1.00f);

			if (elixir.modifier_current <= 0.05f) color = GUI.font_color_default;
			else if (elixir.modifier_current <= 0.10f) color = GUI.font_color_green;
			else if (elixir.modifier_current <= 0.30f) color = GUI.font_color_yellow;
			else if (elixir.modifier_current <= 0.85f) color = GUI.font_color_orange;
			else color = GUI.font_color_red;

			IStatusEffect.ScheduleDraw(new()
			{
				icon = "ui_icon_effect.elixir",
				//icon_extra = "beer",
				value = $"Drunk\n{elixir.modifier_current * 10.00f:0.00} \u2030",
				text_color = color,
				name = $"Elixir\nAmount: {elixir.amount:0.00}\nActive: {elixir.amount_bloodstream:0.00}"
			});
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Shared)]
		public static void UpdateCamera(ISystem.Info info, Entity entity, [Source.Singleton] ref Camera.Singleton camera, [Source.Shared] in Player.Data player, [Source.Owned] in Elixir.Data elixir)
		{

		}
#endif
	}
}