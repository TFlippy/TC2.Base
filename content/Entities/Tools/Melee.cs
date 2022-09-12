
namespace TC2.Base.Components
{
	public static partial class Melee
	{
		public enum Category: byte
		{
			Undefined = 0,

			Pointed,
			Bladed,
			Blunt,
			Misc
		}

		public enum AttackType: byte
		{
			Undefined = 0,

			Swing,
			Thrust
		}

		[Flags]
		public enum Flags: uint
		{
			None = 0,

			No_Handle = 1 << 0,
			Use_RMB = 1 << 1,
			Sync_Hit_Event = 1 << 2
		}

		[IEvent.Data]
		public partial struct HitEvent: IEvent
		{
			public Entity ent_attacker;
			public Entity ent_target;
			public Entity ent_owner;
			public XorRandom random;
			public Material.Type target_material_type;
			public Vector2 world_position;
			public Vector2 direction;
		}

		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Melee.State>]
		public partial struct Data: IComponent
		{
			public Sound.Handle sound_swing = default;
			public float sound_volume = 1.00f;
			public float sound_size = 2.00f;
			public float sound_pitch = 1.00f;

			public Vector2 hit_offset = new(0.00f, 0.00f);
			public Vector2 hit_direction = new(1.00f, 0.00f);

			public Vector2 swing_offset = new(1.00f, 1.00f);
			public float swing_rotation = -2.50f;

			public Melee.AttackType attack_type = AttackType.Swing;

			[Statistics.Info("Base Damage", description: "Base damage", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float damage_base = default;

			[Statistics.Info("Bonus Damage", description: "Random additional damage", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float damage_bonus = default;

			[Statistics.Info("Primary Damage", description: "TODO: Desc", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float primary_damage_multiplier = 1.00f;

			[Statistics.Info("Secondary Damage", description: "TODO: Desc", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float secondary_damage_multiplier = 1.00f;

			[Statistics.Info("Terrain Damage", description: "Damage to terrain", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float terrain_damage_multiplier = 1.00f;

			[Statistics.Info("Cooldown", description: "Time between attacks", format: "{0:0.##}s", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Medium)]
			public float cooldown = default;

			[Statistics.Info("Reach", description: "Melee weapon range", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float max_distance = default;

			[Statistics.Info("Area of Effect", description: "Size of the affected area", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float aoe = default;

			[Statistics.Info("Thickness", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Low)]
			public float thickness = 0.30f;

			[Statistics.Info("Knockback", description: "Multiplies knockback of the damage", format: "{0:0.##}x", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float knockback = 1.00f;

			[Statistics.Info("Yield", description: "Affects amount of material obtained from harvesting", format: "{0:P2}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float yield = 1.00f;

			[Statistics.Info("Penetration Falloff", description: "Modifies damage after each penetration", format: "{0:P2}", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Low)]
			public float penetration_falloff = 0.75f;

			[Statistics.Info("Penetration", description: "How many objects are hit in single strike", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public int penetration = default;

			[Statistics.Info("Damage Type", description: "Type of damage dealt", format: "{0}", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
			public Damage.Type damage_type = default;

			[Statistics.Info("Category", description: "TODO: Desc", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			public Melee.Category category = default;

			public Melee.Flags flags = default;

			public Physics.Layer hit_mask = default;
			public Physics.Layer hit_exclude = default;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct State: IComponent
		{
			[Save.Ignore, Net.Ignore] public float next_hit;
			[Save.Ignore, Net.Ignore] public float last_hit;
		}

#if CLIENT
		public struct BlockGUI: IGUICommand
		{
			public Transform.Data transform;
			public Vector2 pos_target;
			public Vector2 pos_hit;
			public Melee.Data melee;
			public Melee.State melee_state;
			public Entity entity;
			public Color32BGRA color;

			public void Draw()
			{
				ref var region = ref Client.GetRegion();

				var radius = this.melee.thickness;
				var c_radius = radius * GUI.GetWorldToCanvasScale();
				var c_pos = GUI.WorldToCanvas(this.pos_target);

				if (this.melee.category == Melee.Category.Pointed)
				{
					//if (!entity.IsValid())
					{
						GUI.DrawTerrainOutline(ref region, this.pos_hit, radius, Color32BGRA.Yellow.WithAlphaMult(0.75f));
					}

					GUI.DrawCircleFilled(c_pos, c_radius, color.WithAlphaMult(0.10f), segments: 16);
					GUI.DrawCircle(c_pos, c_radius, color.WithAlphaMult(0.40f), thickness: 1.00f, segments: 16);
					GUI.DrawCircleFilled(GUI.WorldToCanvas(this.pos_hit), 3.00f, color, segments: 4);
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single)]
		public static void OnGUI(ISystem.Info info, Entity entity, [Source.Parent] in Interactor.Data interactor, [Source.Parent] in Player.Data player,
		[Source.Owned] in Melee.Data melee, [Source.Owned] ref Melee.State melee_state,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] in Body.Data body, [Source.Parent, Optional] in Faction.Data faction)
		{
			if (player.IsLocal())
			{
				var random = XorRandom.New();
				ref var region = ref info.GetRegion();

				var parent = body.GetParent();

				var pos = transform.LocalToWorld(melee.hit_offset);
				var dir = default(Vector2);

				//switch (melee.attack_type)
				//{
				//	default:
				//	case Melee.AttackType.Swing:
				//	{
				//		dir = transform.LocalToWorldDirection(melee.hit_direction.RotateByRad(-melee.swing_rotation * 0.25f));
				//	}
				//	break;

				//	case Melee.AttackType.Thrust:
				//	{
				//		dir = (control.mouse.position - transform.position).GetNormalized();
				//	}
				//	break;
				//}
				dir = (control.mouse.position - transform.position).GetNormalized();


				//var len = melee.max_distance; // MathF.Min(melee.max_distance, Vector2.Distance(control.mouse.position, pos));
				var pos_target = Maths.ClampRadius(control.mouse.position, pos, melee.max_distance); //  pos + (dir * len);
				var len = Vector2.Distance(pos, pos_target);
				var pos_hit = pos_target;
				var modifier = 1.00f;
				var flags = Damage.Flags.None;
				var penetration = melee.penetration;
				var hit_any = false;
				var hit_terrain = false;
				var hit_solid = false;
				var index_max = -1;
				var dist_max = -1.00f;

				Span<LinecastResult> results = stackalloc LinecastResult[16];
				if (region.TryLinecastAll(pos, pos_target, melee.thickness, ref results, mask: melee.hit_mask, exclude: melee.hit_exclude & ~(Physics.Layer.Ignore_Melee)))
				{
					results.SortByDistance();

					// Find first solid/blocking shape
					for (var i = 0; i < results.Length; i++)
					{
						ref var result = ref results[i];
						if (result.layer.HasAny(Physics.Layer.Solid | Physics.Layer.World) && result.mask.HasAny(Physics.Layer.Solid) && !result.layer.HasAny(Physics.Layer.Ignore_Melee))
						{
							//if (result.alpha <= 0.00f) continue;
							if (result.entity == parent || result.entity_parent == parent || result.entity == entity) continue;
							if (faction.id != 0 && result.GetFactionID() == faction.id) continue;

							//Melee.Hit(ref region, entity, parent, result.entity, result.world_position, dir, -dir, result.material_type, in melee, ref melee_state, ref random, damage_multiplier: modifier, faction: faction.id);

							//modifier *= melee.penetration_falloff;
							//penetration = 0;
							//hit_terrain |= !result.entity.IsValid();
							//hit_any = true;
							//hit_solid = true;

							dist_max = MathF.Max(dist_max, result.alpha * melee.max_distance);
							index_max = i;

							break;

							//pos_target = pos + (dir * len * result.alpha) + (dir * melee.thickness);
							//pos_target = pos + (dir * len * result.alpha) + (dir * melee.thickness);
							//pos_hit = result.world_position;
						}
					}

					if (dist_max < 0.00f)
					{
						dist_max = len;
					}

					if (len <= dist_max + melee.thickness)
					{
						for (var i = 0; i < results.Length && penetration >= 0; i++)
						{
							ref var result = ref results[i];
							//if (!result.layer.HasAny(Physics.Layer.Solid | Physics.Layer.World))
							{
								//if (result.alpha <= 0.00f) continue;
								if (result.entity == parent || result.entity_parent == parent || result.entity == entity) continue;
								if (faction.id != 0 && result.GetFactionID() == faction.id) continue;

								var closest_result = result.GetClosestPoint(pos_target, true);
								if (melee.category == Melee.Category.Pointed && !(result.layer.HasAny(Physics.Layer.Solid | Physics.Layer.World) && result.mask.HasAny(Physics.Layer.Solid) && !result.layer.HasAny(Physics.Layer.Ignore_Melee)) && Vector2.DistanceSquared(closest_result.world_position, pos_target) > (melee.thickness * melee.thickness)) continue;

								modifier *= melee.penetration_falloff;
								penetration--;

								hit_terrain |= !result.entity.IsValid();
								hit_any = true;

								//pos_target = closest_result.world_position; // pos + (dir * len * result.alpha) + (dir * melee.thickness);
								//pos_target = pos + (dir * dist_max);
								pos_hit = closest_result.world_position;

								if (i == index_max) break;
							}
						}
					}
					else if (index_max >= 0)
					{
						ref var result = ref results[index_max];

						hit_terrain |= !result.entity.IsValid();
						hit_any = true;
						hit_solid = true;

						var closest_result = result.GetClosestPoint(result.world_position, true);

						//pos_target = result.world_position;
						pos_hit = closest_result.world_position;
					}

					//pos_target = pos_target;
				}

				if (!hit_any)
				{
					var material_type = default(Material.Type);
					if (Terrain.TryGetTileAtWorldPosition(ref region.GetTerrain(), pos_target, out var tile))
					{
						material_type = tile.Block.material_type;
					}

					if (material_type != Material.Type.None)
					{
						hit_terrain = true;
					}
				}

				var gui = new BlockGUI()
				{
					entity = entity,
					transform = transform,
					melee = melee,
					melee_state = melee_state,
					pos_target = pos_target.ClampRadius(pos, dist_max + melee.thickness),
					pos_hit = pos_hit,
					//valid = hit_any
				};

				if (hit_terrain) gui.color = Color32BGRA.Yellow;
				else if (hit_any) gui.color = Color32BGRA.Red;
				else gui.color = Color32BGRA.Grey;

				gui.Submit();
			}
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void OnSpriteUpdate(ISystem.Info info, Entity entity, [Source.Owned] in Melee.Data melee, [Source.Owned] in Melee.State melee_state, [Source.Owned] ref Animated.Renderer.Data renderer)
		{
			var elapsed = info.WorldTime - melee_state.last_hit;
			var max = melee_state.next_hit - melee_state.last_hit;
			var alpha = 1.00f - Maths.Clamp(elapsed / (max * 0.80f), 0.00f, 1.00f);

			switch (melee.attack_type)
			{
				case Melee.AttackType.Swing:
				{
					renderer.offset = melee.swing_offset * alpha * alpha;
					renderer.rotation = melee.swing_rotation * alpha * alpha;
				}
				break;

				case Melee.AttackType.Thrust:
				{
					renderer.offset = new Vector2(melee.max_distance * 0.75f * alpha * alpha * alpha, 0.00f);
					renderer.rotation = 0.00f;
				}
				break;
			}
		}
#endif

		public static void Hit(ref Region.Data region, Entity ent_attacker, Entity ent_owner, Entity ent_target, Vector2 hit_pos, Vector2 dir, Vector2 normal, Material.Type material_type, in Melee.Data melee, ref Melee.State melee_state, ref XorRandom random, float damage_multiplier = 1.00f, IFaction.Handle faction = default)
		{
#if CLIENT
			var shake_mult = Maths.Clamp(melee.knockback, 0.00f, 1.00f);
			Shake.Emit(ref region, hit_pos, 0.40f * shake_mult, 0.40f * shake_mult, radius: 2.00f);
#endif

#if SERVER
			var damage = melee.damage_base + random.NextFloatRange(0.00f, melee.damage_bonus) * damage_multiplier;
			Damage.Hit(attacker: ent_attacker, owner: ent_owner, target: ent_target,
				world_position: hit_pos, direction: dir, normal: normal,
				damage: damage, damage_type: melee.damage_type, yield: melee.yield, primary_damage_multiplier: melee.primary_damage_multiplier, secondary_damage_multiplier: melee.secondary_damage_multiplier, terrain_damage_multiplier: melee.terrain_damage_multiplier,
				target_material_type: material_type, knockback: melee.knockback, size: melee.aoe, faction_id: faction.id);
#endif

			var data = new Melee.HitEvent();
			data.ent_attacker = ent_attacker;
			data.ent_owner = ent_owner;
			data.ent_target = ent_target;
			data.world_position = hit_pos;
			data.direction = dir;
			data.random = random;
			data.target_material_type = material_type;

			if (melee.flags.HasAny(Melee.Flags.Sync_Hit_Event))
			{
#if SERVER
				ent_attacker.NotifyDeferred(data, sync: true);
#endif
			}
			else
			{
				ent_attacker.Notify(ref data);
			}
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] in Melee.Data melee, [Source.Owned] ref Melee.State melee_state,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] in Body.Data body, [Source.Parent, Optional] in Faction.Data faction)
		{
			var key = melee.flags.HasAny(Melee.Flags.Use_RMB) ? Mouse.Key.Right : Mouse.Key.Left;

			if (control.mouse.GetKey(key) && info.WorldTime >= melee_state.next_hit)
			{
				var random = XorRandom.New();
				ref var region = ref info.GetRegion();

				var parent = body.GetParent();

				melee_state.last_hit = info.WorldTime;
				melee_state.next_hit = info.WorldTime + melee.cooldown;

				var pos = transform.LocalToWorld(melee.hit_offset);
				var dir = default(Vector2);

				//switch (melee.attack_type)
				//{
				//	default:
				//	case Melee.AttackType.Swing:
				//	{
				//		dir = transform.LocalToWorldDirection(melee.hit_direction.RotateByRad(-melee.swing_rotation * 0.25f));
				//	}
				//	break;

				//	case Melee.AttackType.Thrust:
				//	{
				//		dir = (control.mouse.position - transform.position).GetNormalized();
				//	}
				//	break;
				//}
				dir = (control.mouse.position - transform.position).GetNormalized();

				//var len = MathF.Min(melee.max_distance, Vector2.Distance(control.mouse.position, pos));
				//var pos_target = pos + (dir * len);
				//var pos_hit = pos_target;
				//var modifier = 1.00f;
				//var flags = Damage.Flags.None;
				//var penetration = melee.penetration;
				//var hit_any = false;
				//var hit_terrain = false;
				//var hit_solid = false;

				var pos_target = Maths.ClampRadius(control.mouse.position, pos, melee.max_distance); //  pos + (dir * len);
				var len = Vector2.Distance(pos, pos_target);
				var pos_hit = pos_target;
				var modifier = 1.00f;
				var flags = Damage.Flags.None;
				var penetration = melee.penetration;
				var hit_any = false;
				var hit_terrain = false;
				var hit_solid = false;

				//App.WriteLine(dir);

#if CLIENT
				Sound.Play(melee.sound_swing, transform.position, volume: melee.sound_volume, random.NextFloatRange(0.90f, 1.10f) * melee.sound_pitch, size: melee.sound_size);
#endif

				{
					Span<LinecastResult> results = stackalloc LinecastResult[16];
					if (region.TryLinecastAll(pos, pos_target, melee.thickness, ref results, mask: melee.hit_mask, exclude: melee.hit_exclude & ~(Physics.Layer.Ignore_Melee)))
					{
						results.SortByDistance();
						var index_max = -1;
						var dist_max = -1.00f;

						// Find first solid/blocking shape
						for (var i = 0; i < results.Length; i++)
						{
							ref var result = ref results[i];
							if (result.layer.HasAny(Physics.Layer.Solid | Physics.Layer.World) && result.mask.HasAny(Physics.Layer.Solid) && !result.layer.HasAny(Physics.Layer.Ignore_Melee))
							{
								//if (result.alpha <= 0.00f) continue;
								if (result.entity == parent || result.entity_parent == parent || result.entity == entity) continue;
								if (faction.id != 0 && result.GetFactionID() == faction.id) continue;

								//Melee.Hit(ref region, entity, parent, result.entity, result.world_position, dir, -dir, result.material_type, in melee, ref melee_state, ref random, damage_multiplier: modifier, faction: faction.id);

								//modifier *= melee.penetration_falloff;
								//penetration = 0;
								//hit_terrain |= !result.entity.IsValid();
								//hit_any = true;
								//hit_solid = true;

								dist_max = MathF.Max(dist_max, result.alpha * melee.max_distance);
								index_max = i;

								break;

								//pos_target = pos + (dir * len * result.alpha) + (dir * melee.thickness);
								//pos_target = pos + (dir * len * result.alpha) + (dir * melee.thickness);
								//pos_hit = result.world_position;
							}
						}

						if (dist_max < 0.00f)
						{
							dist_max = len;
						}

						if (len <= dist_max + melee.thickness)
						{
							for (var i = 0; i < results.Length && penetration >= 0; i++)
							{
								ref var result = ref results[i];
								//if (!result.layer.HasAny(Physics.Layer.Solid | Physics.Layer.World))
								{
									//if (result.alpha <= 0.00f) continue;
									if (result.entity == parent || result.entity_parent == parent || result.entity == entity) continue;
									if (faction.id != 0 && result.GetFactionID() == faction.id) continue;

									var closest_result = result.GetClosestPoint(pos_target, true);
									if (melee.category == Melee.Category.Pointed && !(result.layer.HasAny(Physics.Layer.Solid | Physics.Layer.World) && result.mask.HasAny(Physics.Layer.Solid) && !result.layer.HasAny(Physics.Layer.Ignore_Melee)) && Vector2.DistanceSquared(closest_result.world_position, pos_target) > (melee.thickness * melee.thickness)) continue;

									modifier *= melee.penetration_falloff;
									penetration--;

									hit_terrain |= !result.entity.IsValid();
									hit_any = true;

									//pos_target = closest_result.world_position; // pos + (dir * len * result.alpha) + (dir * melee.thickness);
									//pos_target = pos + (dir * dist_max);
									pos_hit = closest_result.world_position;

									Melee.Hit(ref region, entity, parent, result.entity, pos_hit, dir, -dir, result.material_type, in melee, ref melee_state, ref random, damage_multiplier: modifier, faction: faction.id);

									//App.WriteLine(result.entity.IsAlive());

									if (i == index_max) break;
								}
							}
						}
						else if (index_max >= 0)
						{
							ref var result = ref results[index_max];

							hit_terrain |= !result.entity.IsValid();
							hit_any = true;
							hit_solid = true;

							var closest_result = result.GetClosestPoint(result.world_position, true);

							//pos_target = result.world_position;
							pos_hit = closest_result.world_position;

							Melee.Hit(ref region, entity, parent, result.entity, pos_hit, dir, -dir, result.material_type, in melee, ref melee_state, ref random, damage_multiplier: modifier, faction: faction.id);
						}

						//results.SortByDistance();
						//var index_max = results.Length;

						//for (var i = 0; i < index_max && !hit_solid && penetration >= 0; i++)
						//{
						//	ref var result = ref results[i];
						//	if (result.layer.HasAny(Physics.Layer.Solid | Physics.Layer.World) && result.mask.HasAny(Physics.Layer.Solid) && !result.layer.HasAny(Physics.Layer.Ignore_Melee))
						//	{
						//		//if (result.alpha <= 0.00f) continue;
						//		if (result.entity == parent || result.entity_parent == parent || result.entity == entity) continue;
						//		if (faction.id != 0 && result.GetFactionID() == faction.id) continue;

						//		//modifier *= melee.penetration_falloff;
						//		//penetration = 0;
						//		hit_terrain |= !result.entity.IsValid();
						//		hit_any = true;
						//		hit_solid = true;
						//		index_max = i;

						//		pos_target = pos + (dir * len * result.alpha) + (dir * melee.thickness);
						//		pos_hit = result.world_position;

						//		Melee.Hit(ref region, entity, parent, result.entity, result.world_position, dir, -dir, result.material_type, in melee, ref melee_state, ref random, damage_multiplier: modifier, faction: faction.id);
						//	}
						//}

						//if (!hit_solid)
						//{
						//	for (var i = 0; i < results.Length && penetration >= 0; i++)
						//	{
						//		ref var result = ref results[i];

						//		//if (!result.layer.HasAny(Physics.Layer.Solid | Physics.Layer.World))
						//		{
						//			//if (result.alpha <= 0.00f) continue;
						//			if (result.entity == parent || result.entity_parent == parent || result.entity == entity) continue;
						//			if (faction.id != 0 && result.GetFactionID() == faction.id) continue;

						//			var closest_result = result.GetClosestPoint(pos_target, true);
						//			if (Vector2.DistanceSquared(closest_result.world_position, pos_target) > (melee.aoe * melee.aoe)) continue;

						//			Melee.Hit(ref region, entity, parent, result.entity, closest_result.world_position, dir, -dir, result.material_type, in melee, ref melee_state, ref random, damage_multiplier: modifier, faction: faction.id);

						//			modifier *= melee.penetration_falloff;
						//			penetration--;
						//			hit_any = true;

						//			pos_hit = closest_result.world_position;
						//		}
						//	}
						//}
					}
				}

				if (!hit_any)
				{
					var material_type = default(Material.Type);
					if (Terrain.TryGetTileAtWorldPosition(ref region.GetTerrain(), pos_target, out var tile))
					{
						material_type = tile.Block.material_type;
					}

					if (material_type != Material.Type.None)
					{
						Melee.Hit(ref region, entity, parent, default, pos_target, dir, -dir, material_type, in melee, ref melee_state, ref random, damage_multiplier: modifier, faction: faction.id);
					}
				}
			}
		}
	}
}
