
namespace TC2.Base.Components
{
	public static partial class Drill
	{
		public static readonly Texture.Handle texture_stone = "StoneGib";
		public static readonly Texture.Handle texture_smoke = "LargeSmoke";

		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			[Statistics.Info("Damage", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float damage = default;

			[Statistics.Info("Damage Multiplier (Terrain)", description: "TODO: Desc", format: "{0:0.##}x", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float damage_terrain_multiplier = 2.00f;

			[Statistics.Info("Reach", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float max_distance = default;

			[Statistics.Info("Speed", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float speed = default;

			[Statistics.Info("Area of Effect", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float radius = default;

			public Physics.Layer hit_mask = Physics.Layer.World | Physics.Layer.Destructible;
			public Physics.Layer hit_exclude = Physics.Layer.Crate | Physics.Layer.Climbable;

			[Save.Ignore, Net.Ignore] public float last_hit = default;
			[Save.Ignore, Net.Ignore] public float next_hit = default;

			public Data()
			{

			}
		}

#if CLIENT
		public struct DrillGUI: IGUICommand
		{
			public Transform.Data transform;
			public Vector2 world_position;
			public Drill.Data drill;
			public Entity entity;
			public Specialization.Miner.Data mining;
			public bool valid;

			public void Draw()
			{
				ref var region = ref this.entity.GetRegion();

				var wpos = this.world_position;
				var radius = this.mining.ApplySize(this.drill.radius);

				GUI.DrawTerrainOutline(ref region, wpos, radius * 2.00f);
			}
		}

		[ISystem.GUI(ISystem.Mode.Single)]
		public static void OnGUI(ISystem.Info info, Entity entity, [Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Drill.Data drill, [Source.Owned] in Transform.Data transform, [Source.Parent] in Player.Data player, [Source.Owned] in Control.Data control, [Source.Parent, Optional] in Specialization.Miner.Data mining)
		{
			if (player.IsLocal())
			{
				var dir = transform.GetDirection();
				var len = (control.mouse.position - transform.position).Length();
				var hit_position = transform.position + (dir * Maths.Clamp(len, 0.25f, drill.max_distance));

				var gui = new DrillGUI()
				{
					entity = entity,
					transform = transform,
					drill = drill,
					world_position = hit_position,
					mining = mining,
					valid = true
				};

				//if (info.GetRegion().TryLinecast(transform.position, hit_position, drill.radius, out var hit, mask: Physics.Layer.World, query_flags: Physics.QueryFlag.Static))
				//{
				//	gui.world_position = hit.world_position;
				//}

				gui.Submit();
			}
		}
#endif

		[ISystem.Update(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] ref Drill.Data drill, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] in Body.Data body,
		[Source.Owned] ref Sound.Emitter sound_emitter, [Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned, Optional] ref Overheat.Data overheat, [Source.Parent, Optional] in Faction.Data faction)
		{
			if (control.mouse.GetKey(Mouse.Key.Left) && !overheat.flags.HasAll(Overheat.Flags.Overheated))
			{
				var random = XorRandom.New();
				if (info.WorldTime >= drill.next_hit)
				{
					ref var region = ref info.GetRegion();

					drill.next_hit = info.WorldTime + MathF.ReciprocalEstimate(drill.speed);

					var max_distance = drill.max_distance;

					var dir = transform.GetDirection();
					var len = max_distance;
					var normal = -dir;

					var pos_a = transform.position + (dir * 0.35f);
					var pos_b = transform.position + (dir * max_distance * 0.75f);

					var modifier = 1.00f;
					overheat.heat_current += 4.00f;

					Span<LinecastResult> results = stackalloc LinecastResult[16];
					if (region.TryLinecastAll(pos_a, pos_b, drill.radius, ref results, mask: drill.hit_mask, exclude: drill.hit_exclude))
					{
						results.Sort(static (a, b) => a.alpha.CompareTo(b.alpha));

						var parent = body.GetParent();

						var damage_base = drill.damage;
						var damage_bonus = 0.00f; // random.NextFloatRange(0.00f, melee.damage_bonus);
						var damage = damage_base + damage_bonus;

						var flags = Damage.Flags.None;

						var penetration = 2;

						var hit_terrain = false;

						for (var i = 0; i < results.Length && penetration >= 0; i++)
						{
							ref var hit = ref results[i];
							if (hit.entity == parent || hit.entity_parent == parent || hit.entity == entity) continue;

							var hit_faction_id = hit.GetFactionID();
							if (hit_faction_id != 0 && hit_faction_id == faction.id) continue;

							var is_terrain = !hit.entity.IsValid();
							if (is_terrain)
							{
								if (hit_terrain) continue;
								hit_terrain = true;
							}

							var material_type = hit.material_type;
							var heat = drill.damage * 0.015f * modifier;

							switch (material_type)
							{
								case Material.Type.Metal:
								{
									heat *= 3.00f;
								}
								break;

								case Material.Type.Glass:
								case Material.Type.Stone:
								{
									heat *= 1.20f;
								}
								break;

								case Material.Type.Gravel:
								{
									heat *= 1.10f;
								}
								break;

								case Material.Type.Mushroom:
								case Material.Type.Fabric:								
								case Material.Type.Rubber:
								case Material.Type.Wood:
								{
									heat *= 0.20f;
								}
								break;

								case Material.Type.Flesh:
								case Material.Type.Insect:
								{
									heat *= -0.70f;
								}
								break;
							}

#if SERVER
							var damage_final = damage * modifier;
							if (is_terrain) damage_final *= drill.damage_terrain_multiplier;

							Damage.Hit(entity, parent, hit.entity, hit.world_position, dir, -dir, damage_final, hit.material_type, Damage.Type.Drill, knockback: 0.25f, size: drill.radius * 1.50f, xp_modifier: 0.80f, flags: flags, yield: 0.90f, primary_damage_multiplier: 1.00f, secondary_damage_multiplier: 1.00f, terrain_damage_multiplier: 1.00f);
#endif

							flags |= Damage.Flags.No_Sound;
							modifier *= 0.70f;
							penetration--;

							overheat.heat_current += heat;
						}
					}

#if CLIENT
					var mod = MathF.Pow(Maths.Clamp(len, 0.00f, max_distance) / max_distance, 3.00f);
					var mod_inv = 1.00f - mod;

					Shake.Emit(ref region, pos_b, 0.30f * mod_inv, 0.20f);

					sound_emitter.volume = 1.00f;
					sound_emitter.pitch = Maths.Damp(sound_emitter.pitch, 0.80f + (mod * 0.50f), 2.00f, App.fixed_update_interval_s);
#endif
				}

#if CLIENT
				renderer.offset = random.NextVector2(0.10f);
#endif
			}
			else
			{
#if CLIENT
				sound_emitter.volume = 0.00f;
				sound_emitter.pitch = 1.00f;

				renderer.offset = default;
#endif
			}
		}
	}
}
