
namespace TC2.Base.Components
{
	public static partial class Food
	{
		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct Effect: IComponent
		{
			[Statistics.Info("Food", description: "TODO: Desc", format: "{0:0.##} ml", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
			public float amount;

			public float amount_bloodstream;
			//public float amount_metabolized;

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
		[ISystem.Event<Consumable.ConsumeEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnConsume(ISystem.Info info, Entity entity, ref Consumable.ConsumeEvent data, [Source.Owned] in Consumable.Data consumable, [Source.Owned] ref Food.Effect food_src)
		{
			ref var food_dst = ref data.ent_organic.GetOrAddComponent<Food.Effect>(sync: true);

			switch (consumable.action)
			{
				case Consumable.Action.Inject:
				{
					food_dst.amount_bloodstream += food_src.amount * data.amount_modifier;
				}
				break;

				case Consumable.Action.Drink:
				{
					var ratio = Maths.SumRatio(food_src.amount * data.amount_modifier, food_dst.amount);

					food_dst.amount += food_src.amount * data.amount_modifier;
					food_dst.release_rate_target = Maths.Lerp(food_dst.release_rate_target + (food_dst.release_rate_target * ratio), consumable.release_rate, ratio);
					food_dst.release_step = Maths.Lerp(food_dst.release_step, consumable.release_step, ratio);

					//App.WriteLine($"ratio: {ratio}");
				}
				break;

				default:
				{
					var ratio = Maths.SumRatio(food_src.amount * data.amount_modifier, food_dst.amount);

					food_dst.amount += food_src.amount * data.amount_modifier;
					food_dst.release_rate_target = Maths.Lerp(food_dst.release_rate_target + (food_dst.release_rate_target * ratio), consumable.release_rate, ratio);
					food_dst.release_step = Maths.Lerp(food_dst.release_step, consumable.release_step, ratio);

					//App.WriteLine($"ratio: {ratio}");
				}
				break;
			}
		}
#endif

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateStats(ISystem.Info info, Entity entity,
		[Source.All] ref Food.Effect food, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			var modifier = food.modifier_current;
			var modifier_jitter = food.modifier_current + (food.jitter_current * 0.25f);
			var hiccup_modifier = food.hiccup_current * food.hiccup_current;

			organic.consciousness *= Maths.Lerp01(1.00f, 0.05f, (modifier * 0.30f) + (hiccup_modifier * modifier * 0.30f));
			organic.endurance *= Maths.Lerp01(1.00f, 1.75f, (modifier * 1.70f));
			organic.dexterity *= Maths.Lerp01(1.00f, 0.20f, (modifier * 1.40f) + (hiccup_modifier * 0.50f));
			organic.strength *= Maths.Lerp01(1.00f, 1.30f, (modifier * 1.50f));
			organic.motorics *= Maths.Lerp01(1.00f, 0.40f, (modifier_jitter * 0.40f) + (hiccup_modifier * 0.80f));
			organic.coordination *= Maths.Lerp01(1.00f, 0.10f, (modifier_jitter * 0.10f) + (hiccup_modifier * 0.90f));
			organic.absorption *= Maths.Lerp01(1.00f, 2.50f, (modifier * 1.50f));
			organic.pain_modifier *= Maths.Lerp01(1.00f, 0.20f, (modifier * 1.20f));
		}

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateHead(ISystem.Info info, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.All] ref Food.Effect food, ref XorRandom random, ref Region.Data region, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Head.Data head)
		{
			var modifier = food.modifier_current + (food.jitter_current * 0.75f);

			if (info.WorldTime >= food.next_sound && food.hiccup_current > 0.90f)
			{
				Sound.Play(ref region, sounds_drunk.GetRandom(ref random), transform.position, volume: random.NextFloatRange(0.30f, 0.50f), pitch: head.voice_pitch * random.NextFloatRange(0.90f, 1.10f));

				food.next_sound = info.WorldTime + random.NextFloatRange(2.00f, 5.00f);
			}
		}
#endif

		public static float metabolization_modifier = 0.20f;
		public static float elimination_modifier = 0.10f;
		public static float modifier_lerp = 0.02f;

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateAmount(ISystem.Info info, Entity entity, ref XorRandom random, [Source.Owned] ref Food.Effect food, [Source.Owned, Override] in Organic.Data organic)
		{
			food.release_rate_current = Maths.MoveTowards(food.release_rate_current, food.release_rate_target, food.release_step * info.DeltaTime);

			if (food.amount > Maths.epsilon)
			{
				var amount_released = Maths.Clamp(food.release_rate_current, 0.00f, food.amount) * info.DeltaTime;

				food.amount_bloodstream += amount_released;
				food.amount -= amount_released;
			}

			var total_mass = 70.00f; // TODO

			var amount_metabolized = Maths.Clamp(food.amount_bloodstream * metabolization_modifier, 0.10f, food.amount_bloodstream) * info.DeltaTime;

			//food.amount_metabolized = amount_metabolized;
			food.modifier_current = Maths.Lerp(food.modifier_current, (amount_metabolized * 0.40f) / (total_mass * 0.001f), modifier_lerp);
			food.hiccup_current = Maths.Max(food.hiccup_current - (info.DeltaTime * 0.20f), 0.00f);

			food.jitter_current = Maths.Perlin(info.WorldTime, 0.00f, 0.30f);
			food.amount_bloodstream -= amount_metabolized * elimination_modifier;

#if SERVER
			var modifier = food.modifier_current;
			if (modifier > 0.30f && food.hiccup_current < 0.05f && random.NextBool(modifier * 0.005f))
			{
				food.hiccup_current = 1.00f;
				food.Sync(entity);
			}

			if (food.amount <= 0.01f && food.amount_bloodstream <= 0.01f)
			{
				entity.RemoveComponent<Food.Effect>();
			}
#endif
		}

#if CLIENT
		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Shared)]
		public static void OnGUI(ISystem.Info info, Entity entity, [Source.Shared] in Player.Data player, [Source.Owned] in Food.Effect food)
		{
			var color = GUI.font_color_default;
			//color = Color32BGRA.FromHSV((1.00f - Maths.Clamp01(food.modifier_current)) * 2.00f, 1.00f, 1.00f);

			if (food.modifier_current <= 0.05f) color = GUI.font_color_default;
			else if (food.modifier_current <= 0.10f) color = GUI.font_color_green;
			else if (food.modifier_current <= 0.30f) color = GUI.font_color_yellow;
			else if (food.modifier_current <= 0.55f) color = GUI.font_color_orange;
			else color = GUI.font_color_red;

			IStatusEffect.ScheduleDraw(new()
			{
				icon = "ui_icon_effect.food",
				//icon_extra = "beer",
				value = $"Drunk\n{food.modifier_current.Clamp01():P2}",
				text_color = color,
				name = $"Food\nAmount: {food.amount:0.00}\nActive: {food.amount_bloodstream:0.00}"
			});
		}

		[ISystem.PreUpdate.Reset(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Shared)]
		public static void UpdateCamera(ISystem.Info info, Entity entity, [Source.Singleton] ref Camera.Singleton camera, [Source.Shared] in Player.Data player, [Source.Owned] in Food.Effect food)
		{
			var modifier = MathF.Pow(food.modifier_current, 1.30f);

			var rot = 0.00f;
			rot += MathF.Sin(info.WorldTime / 2.53f) * 0.18f * modifier;
			rot += MathF.Cos(info.WorldTime / 1.43f) * 0.15f * modifier;
			rot += MathF.Sin(380 + info.WorldTime / 0.70f) * 0.10f * modifier;

			camera.zoom_modifier *= 1.00f - (Maths.Perlin(info.WorldTime * 0.15f, 0.00f, 4.00f) * 0.30f * modifier);
			camera.rotation = rot;

			Drunk.Color.a.ClampMinRef(Maths.Clamp(modifier, 0.00f, 0.95f));

			ref var low_pass = ref Audio.LowPass;
			low_pass.frequency = Maths.Lerp01(low_pass.frequency, 1000.00f, MathF.Pow(Maths.Max(food.modifier_current - 0.20f, 0.00f), 0.50f));
			low_pass.resonance = Maths.Lerp01(low_pass.resonance, 0.200f, MathF.Pow(food.modifier_current, 0.30f));
		}
#endif
	}
}