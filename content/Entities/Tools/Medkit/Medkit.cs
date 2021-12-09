namespace TC2.Base.Components
{
	public static class Medkit
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public struct Data: IComponent
		{
			[Statistics.Info("Healing Amount", description: "Base amount of health this heals", format: "{0:0}", comparison: Statistics.Comparison.Higher)]
			public float power;

			[Statistics.Info("Healing Cooldown", description: "Time between heals", format: "{0:0.##}s", comparison: Statistics.Comparison.Lower)]
			public float cooldown;
			
			[Statistics.Info("Healing Distance", description: "How far away this can heal things", format: "{0:0}", comparison: Statistics.Comparison.Higher)]
			public float max_distance;

			[Statistics.Info("Healing Cap", description: "Can heal up to this percentage", format: "{0:P2}", comparison: Statistics.Comparison.Higher)]
			public float heal_cap = 1.00f; //How high the health can go (1 is the highest possible)

			[Statistics.Info("Critical Heal", description: "Increases healing at lower hp", format: "{0:P2}", comparison: Statistics.Comparison.Higher)]
			public float critical_heal = 0.00f; //Adds healing a low health (half the effect a 50%, no bonus at 100%, negative values may mean you cant heal low hp targets)

			[Save.Ignore] public float next_use;
		}

#if CLIENT
		public struct MedkitGUI: IGUICommand
		{
			public Transform.Data transform;
			public Control.Data control;
			public Medkit.Data medkit;
			public Specialization.Medic.Data medic;
			public Entity entity;

			public void Draw()
			{
				ref var region = ref Client.GetRegion();

				var max_distance = this.medkit.max_distance;
				this.medic.ApplyReach(ref max_distance);

				var pos_w_base = this.transform.GetInterpolatedPosition();
				var dir = (this.control.mouse.position - pos_w_base).GetNormalized(out var len);

				var pos_w = pos_w_base + (dir * Maths.Clamp(len, 0.00f, max_distance));
				var pos_c = GUI.WorldToCanvas(pos_w);

				if (len < max_distance)
				{
					Span<OverlapResult> hits = stackalloc OverlapResult[16];
					if (region.TryOverlapPointAll(pos_w, 0.20f, ref hits, mask: Physics.Layer.Organic))
					{
						ref var nearest = ref hits.GetNearest();

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

		[ISystem.GUI(ISystem.Mode.Single)]
		public static void OnGUI(ISystem.Info info, Entity entity,
		[Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Medkit.Data medkit, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control,
		[Source.Parent] in Player.Data player, [Source.Parent, Optional] in Specialization.Medic.Data medic)
		{
			if (player.IsLocal())
			{
				var gui = new MedkitGUI()
				{
					entity = entity,
					transform = transform,
					medkit = medkit,
					medic = medic,
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
		[Source.Owned] ref Medkit.Data medkit, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] in Body.Data body,
		[Source.Parent, Optional] in Player.Data player, [Source.Parent, Optional] in Specialization.Medic.Data medic)
		{
			if (control.mouse.GetKey(Mouse.Key.Left) && info.WorldTime >= medkit.next_use)
			{
				ref var region = ref info.GetRegion();
				medkit.next_use = info.WorldTime + medkit.cooldown;

				var max_distance = medkit.max_distance;
				medic.ApplyReach(ref max_distance);

				var dir = (control.mouse.position - transform.position).GetNormalized(out var len);
				if (len < max_distance)
				{
					Span<OverlapResult> hits = stackalloc OverlapResult[16];
					if (region.TryOverlapPointAll(control.mouse.position, 0.20f, ref hits, mask: Physics.Layer.Organic))
					{
						var random = XorRandom.New();
						var total_healed_amount = 0.00f;

						var power = medkit.power;
						medic.ApplyEffectiveness(ref power);

						foreach (ref var hit in hits)
						{
							ref var health = ref hit.entity.GetComponent<Health.Data>();
							if (!health.IsNull())
							{
								var healed = false;

								if (medic.level_reconstruction > 0 && (1.00f - health.primary) <= (medic.level_reconstruction / 3.00f))
								{
									var heal_normalized = Maths.Normalize(power * 0.25f, health.max);
									heal_normalized = heal_normalized + heal_normalized * medkit.critical_heal * (1.00f - health.primary);

									var healed_amount = MathF.Min(Maths.Clamp(medkit.heal_cap - health.primary, 0.00f, medkit.heal_cap), heal_normalized);
									health.primary += healed_amount;
									total_healed_amount += healed_amount * health.max;

									healed |= healed_amount > 0.00f;
								}

								{
									var heal_normalized = Maths.Normalize(power, health.max);
									heal_normalized = heal_normalized + heal_normalized * medkit.critical_heal * (1.00f - health.primary);


									var healed_amount = MathF.Min(Maths.Clamp(medkit.heal_cap - health.secondary, 0.00f, medkit.heal_cap), heal_normalized);
									health.secondary += healed_amount;
									total_healed_amount += healed_amount * health.max;

									healed |= healed_amount > 0.00f;
								}

								if (healed)
								{
									hit.entity.MarkModified<Health.Data>(sync: true);
								}
							}
						}

						if (total_healed_amount > 0.00f)
						{
							Sound.Play(ref region, sounds.GetShuffledRandom(ref random), control.mouse.position, volume: 0.50f, priority: 0.50f);
							Experience.Add(entity, Experience.Type.Medicine, random.NextFloatRange(total_healed_amount * 0.05f, total_healed_amount * 0.10f));
						}
					}
				}
			}
		}
#endif
	}
}
