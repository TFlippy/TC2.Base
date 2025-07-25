﻿namespace TC2.Base.Components
{
	public static class Medkit
	{
		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			[Statistics.Info("Healing Amount", description: "Base amount of health this heals", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float power = 100.00f;

			[Statistics.Info("Healing (Integrity)", description: "TODO: Desc", format: "{0:P2}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float integrity_multiplier = 1.00f;

			[Statistics.Info("Healing (Durability)", description: "TODO: Desc", format: "{0:P2}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float durability_multiplier = 1.00f;

			[Statistics.Info("Cooldown", description: "Time between heals", format: "{0:0.##}s", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Medium)]
			public float cooldown = 0.50f;

			[Statistics.Info("Reach", description: "How far away this can heal", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float max_distance = 1.00f;

			[Statistics.Info("Area of Effect", description: "Size of the affected area", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float aoe = 0.50f;

			[Statistics.Info("Healing Min (Integrity)", description: "Can heal as long as above this percentage", format: "{0:P2}", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Medium)]
			public float heal_min_integrity = 1.00f; // Can heal only as long as above this % plus the reconstruction medic skill

			[Statistics.Info("Healing Min (Durability)", description: "Can heal as long as above this percentage", format: "{0:P2}", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Medium)]
			public float heal_min_durability = 0.50f; // Can heal as long as above this %

			[Statistics.Info("Critical Heal", description: "Increases healing at lower health", format: "{0:P2}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float critical_heal = 0.00f; // Adds healing a low health (half the effect a 50%, no bonus at 100%, negative values may mean you cant heal low hp targets)

			[Statistics.Info("Pain", description: "Adds or reduces pain", format: "{0:0}", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Medium)]
			public float pain = 0.00f;

			[Save.Ignore, Net.Ignore] public float next_use;
		}

#if CLIENT
		public struct MedkitGUI: IGUICommand
		{
			public Transform.Data transform;
			public Control.Data control;
			public Medkit.Data medkit;
			public Entity entity;

			public void Draw()
			{
				ref var region = ref Client.GetRegion();

				var max_distance = this.medkit.max_distance;

				var pos_w_base = this.transform.GetInterpolatedPosition();
				var dir = (this.control.mouse.position - pos_w_base).GetNormalized(out var len);

				var pos_w = pos_w_base + (dir * Maths.Clamp(len, 0.00f, max_distance));
				var pos_c = region.WorldToCanvas(pos_w);

				if (len < max_distance)
				{
					Span<OverlapResult> hits = stackalloc OverlapResult[16];
					if (region.TryOverlapPointAll(this.control.mouse.position, this.medkit.aoe, ref hits, mask: Physics.Layer.Organic, require: Physics.Layer.Organic | Physics.Layer.Destructible, exclude: Physics.Layer.Dead))
					{
						hits.SortByDistance();

						//ref var nearest = ref hits.GetNearest();

						foreach (ref var hit in hits)
						{
							ref var health = ref hit.entity.GetComponent<Health.Data>();
							if (health.IsNotNull())
							{
								var health_norm = health.GetHealthNormalized();
								if (health_norm < 1.00f)
								{
									var integrity = health.integrity;
									var durability = health.durability;

									var color = Color32BGRA.FromHSV(health_norm * 2.00f, 1.00f, 1.00f).WithAlphaMult(0.75f);
									GUI.DrawEntity(hit.entity, color: color);
									GUI.DrawText($"Integrity: {integrity * 100.00f:0}%\nDurability: {durability * 100.00f:0}%", region.WorldToCanvas(hit.world_position) + new Vector2(-32, -56), font: GUI.Font.Superstar, size: 16.00f, color: color);
								}
								else
								{
									var color = Color32BGRA.Green.WithAlphaMult(0.50f);
									GUI.DrawEntity(hit.entity, color: color);
								}
							}
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent)]
		public static void OnGUI(ISystem.Info info, Entity entity,
		[Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Medkit.Data medkit, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control,
		[Source.Parent] in Player.Data player)
		{
			var gui = new MedkitGUI()
			{
				entity = entity,
				transform = transform,
				medkit = medkit,
				control = control
			};
			gui.Submit();
		}
#endif

		public static readonly ShuffledList<Sound.Handle> sounds = new()
		{
			"medkit_use_00",
			"medkit_use_01",
			"medkit_use_02",
			"medkit_use_03"
		};

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity, ref XorRandom random, ref Region.Data region,
		[Source.Owned] ref Medkit.Data medkit, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] in Body.Data body,
		[Source.Parent, Optional] in Player.Data player)
		{
			if (control.mouse.GetKey(Mouse.Key.Left) && info.WorldTime >= medkit.next_use)
			{
				medkit.next_use = info.WorldTime + medkit.cooldown;

				var max_distance = medkit.max_distance;

				var dir = (control.mouse.position - transform.position).GetNormalized(out var len);
				if (len < max_distance)
				{
					Span<OverlapResult> hits = stackalloc OverlapResult[16];
					if (region.TryOverlapPointAll(control.mouse.position, medkit.aoe, ref hits, mask: Physics.Layer.Organic, require: Physics.Layer.Organic | Physics.Layer.Destructible, exclude: Physics.Layer.Dead))
					{
						hits.SortByDistance();

						var total_healed_amount = 0.00f;
						var power = medkit.power;

						foreach (ref var hit in hits)
						{
							var healed_amount_max = 0.00f;

							ref var health = ref hit.entity.GetComponent<Health.Data>();
							if (!health.IsNull())
							{
								var heal_integrity_max = Maths.Clamp(1.00f - medkit.heal_min_integrity, 0.00f, 1.00f);
								if (heal_integrity_max > Maths.epsilon && (1.00f - health.integrity) <= heal_integrity_max)
								{
									var heal_normalized = Maths.Normalize(power * 0.25f, health.GetMaxHealth());
									heal_normalized += heal_normalized * medkit.critical_heal * (1.00f - health.integrity);

									var heal_amount = Maths.Min(Maths.Clamp(1.00f - health.integrity, 0.00f, heal_integrity_max), heal_normalized);
									health.integrity += heal_amount;
									total_healed_amount += heal_amount * health.GetMaxHealth();

									healed_amount_max = Maths.Max(healed_amount_max, heal_amount);
								}

								var heal_durability_max = Maths.Clamp(1.00f - medkit.heal_min_durability, 0.00f, 1.00f);
								if (heal_durability_max > Maths.epsilon && (1.00f - health.durability) <= heal_durability_max)
								{
									var heal_normalized = Maths.Normalize(power, health.max);
									heal_normalized += heal_normalized * medkit.critical_heal * (1.00f - health.integrity);

									var heal_amount = Maths.Min(Maths.Clamp(1.00f - health.durability, 0.00f, heal_durability_max), heal_normalized);
									health.durability += heal_amount;
									total_healed_amount += heal_amount * health.GetMaxHealth();

									healed_amount_max = Maths.Max(healed_amount_max, heal_amount);
								}

								if (healed_amount_max > Maths.epsilon)
								{
									hit.entity.MarkModified<Health.Data>(sync: true);
								}
							}

							if (healed_amount_max > Maths.epsilon)
							{
								ref var organic_state = ref hit.entity.GetComponent<Organic.State>();
								if (!organic_state.IsNull())
								{
									// Adds or reduces pain
									var pain_amount = Maths.Max(medkit.pain, -organic_state.pain);
									organic_state.pain += pain_amount;

									if (pain_amount != 0.00f)
									{
										organic_state.Modified(hit.entity, sync: true);
									}
								}
							}
						}

						if (total_healed_amount > Maths.epsilon)
						{
							Sound.Play(ref region, sounds.GetShuffledRandom(ref random), control.mouse.position, volume: 0.50f, priority: 0.50f);
							//Experience.Add(entity, Experience.Type.Medicine, random.NextFloatRange(total_healed_amount * 0.05f, total_healed_amount * 0.10f));
						}
					}
				}
			}
		}
#endif
	}
}
