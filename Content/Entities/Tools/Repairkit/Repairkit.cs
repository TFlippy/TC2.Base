namespace TC2.Base.Components
{
	public static class Repairkit
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public struct Data: IComponent
		{
			[Statistics.Info("Healing Amount", description: "Base amount of health this heals", format: "{0:0}", comparison: Statistics.Comparison.Higher)]
			public float power;

			[Statistics.Info("Healing (Primary)", description: "TODO: Desc", format: "{0:P2}", comparison: Statistics.Comparison.Higher)]
			public float primary_multiplier = 1.00f;

			[Statistics.Info("Healing (Secondary)", description: "TODO: Desc", format: "{0:P2}", comparison: Statistics.Comparison.Higher)]
			public float secondary_multiplier = 1.00f;

			[Statistics.Info("Cooldown", description: "Time between heals", format: "{0:0.##}s", comparison: Statistics.Comparison.Lower)]
			public float cooldown;

			[Statistics.Info("Reach", description: "How far away this can heal", format: "{0:0}", comparison: Statistics.Comparison.Higher)]
			public float max_distance;

			[Statistics.Info("Area of Effect", description: "Size of the affected area", format: "{0:0.##}", comparison: Statistics.Comparison.Higher)]
			public float aoe = 0.25f;

			[Save.Ignore, Net.Ignore] public float next_use;
		}

#if CLIENT
		public struct RepairkitGUI: IGUICommand
		{
			public Transform.Data transform;
			public Control.Data control;
			public Repairkit.Data repairkit;
			//public Specialization.Medic.Data medic;
			public Entity entity;

			public void Draw()
			{
				ref var region = ref Client.GetRegion();

				var max_distance = this.repairkit.max_distance;
				//this.medic.ApplyReach(ref max_distance);

				var pos_w_base = this.transform.GetInterpolatedPosition();
				var dir = (this.control.mouse.position - pos_w_base).GetNormalized(out var len);

				var pos_w = pos_w_base + (dir * Maths.Clamp(len, 0.00f, max_distance));
				var pos_c = GUI.WorldToCanvas(pos_w);

				if (len < max_distance)
				{
					Span<OverlapResult> hits = stackalloc OverlapResult[16];
					if (region.TryOverlapPointAll(pos_w, 0.20f, ref hits, mask: Physics.Layer.Entity))
					{
						ref var nearest = ref hits.GetNearest();

						ref var organic = ref nearest.entity.GetComponent<Organic.Data>();

						if (organic.IsNull())
						{
							ref var health = ref nearest.entity.GetComponent<Health.Data>();
							if (!health.IsNull())
							{
								var primary = health.primary;
								var secondary = health.secondary;

								var color = Color32BGRA.FromHSV(MathF.Min(primary, secondary) * 2.00f, 1.00f, 1.00f);
								GUI.DrawEntity(nearest.entity, color: color);
								GUI.DrawText($"Primary: {primary * 100.00f:0}%\nSecondary: {secondary * 100.00f:0}%", pos_c + new Vector2(-32, -56), font: GUI.Font.Superstar, size: 16.00f, color: color);
							}
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single)]
		public static void OnGUI(ISystem.Info info, Entity entity,
		[Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Repairkit.Data repairkit, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control,
		[Source.Parent] in Player.Data player)
		{
			if (player.IsLocal())
			{
				var gui = new RepairkitGUI()
				{
					entity = entity,
					transform = transform,
					repairkit = repairkit,
					control = control
				};
				gui.Submit();
			}
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
		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] ref Repairkit.Data repairkit, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] in Body.Data body,
		[Source.Parent, Optional] in Player.Data player)
		{
			if (control.mouse.GetKey(Mouse.Key.Left) && info.WorldTime >= repairkit.next_use)
			{
				ref var region = ref info.GetRegion();
				repairkit.next_use = info.WorldTime + repairkit.cooldown;

				var max_distance = repairkit.max_distance;
				//medic.ApplyReach(ref max_distance);

				var dir = (control.mouse.position - transform.position).GetNormalized(out var len);
				if (len < max_distance)
				{
					Span<OverlapResult> hits = stackalloc OverlapResult[16];
					if (region.TryOverlapPointAll(control.mouse.position, repairkit.aoe, ref hits, mask: Physics.Layer.Entity))
					{
						var random = XorRandom.New();
						var total_healed_amount = 0.00f;

						var power = repairkit.power;
						//medic.ApplyEffectiveness(ref power);

						foreach (ref var hit in hits)
						{

							ref var organic = ref hit.entity.GetComponent<Organic.Data>();
							ref var harvestable = ref hit.entity.GetComponent<Harvestable.Data>();

							if (organic.IsNull() && harvestable.IsNull())
							{
								var healed_amount_max = 0.00f;

								ref var health = ref hit.entity.GetComponent<Health.Data>();
								if (!health.IsNull())
								{
									/*var heal_primary_max = 1.00f;
									if (heal_primary_max > 0.00f && (1.00f - health.primary) <= heal_primary_max)
									{
										var heal_normalized = Maths.Normalize(power * 0.25f, health.max);

										var heal_amount = MathF.Min(Maths.Clamp(1.00f - health.primary, 0.00f, heal_primary_max), heal_normalized);
										health.primary += heal_amount;
										total_healed_amount += heal_amount * health.max;

										healed_amount_max = MathF.Max(healed_amount_max, heal_amount);
									}*/

									var heal_secondary_max = 1.00f;
									if (heal_secondary_max > 0.00f && (1.00f - health.secondary) <= heal_secondary_max)
									{
										var heal_normalized = Maths.Normalize(power, health.max);

										var heal_amount = MathF.Min(Maths.Clamp(1.00f - health.secondary, 0.00f, heal_secondary_max), heal_normalized);
										health.secondary += heal_amount;
										total_healed_amount += heal_amount * health.max;

										healed_amount_max = MathF.Max(healed_amount_max, heal_amount);
									}

									if (healed_amount_max > 0.00f)
									{
										hit.entity.MarkModified<Health.Data>(sync: true);
									}
								}
							}
						}

						if (total_healed_amount > 0.00f)
						{
							//Sound.Play(ref region, sounds.GetShuffledRandom(ref random), control.mouse.position, volume: 0.50f, priority: 0.50f);
							//Experience.Add(entity, Experience.Type.Medicine, random.NextFloatRange(total_healed_amount * 0.05f, total_healed_amount * 0.10f));
						}
					}
				}
			}
		}
#endif
	}
}
