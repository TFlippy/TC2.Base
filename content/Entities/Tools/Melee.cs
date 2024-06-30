
using Keg.Engine.Infrastructure;

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
			Sync_Hit_Event = 1 << 2,
			No_Material_Filter = 1 << 3,
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

		[IComponent.Data(Net.SendType.Reliable, region_only: true), IComponent.With<Melee.State>]
		public partial struct Data: IComponent
		{
			public static Sound.Handle sound_swing_default = "tool_swing_00";

			public Sound.Handle sound_swing = sound_swing_default;
			public float sound_volume = 1.00f;
			public float sound_size = 2.00f;
			public float sound_pitch = 1.00f;

			[Editor.Picker.Position(true, true)]
			public Vector2 hit_offset = new(0.00f, 0.00f);

			[Editor.Picker.Direction(true, true)]
			public Vector2 hit_direction = new(1.00f, 0.00f);

			[Editor.Picker.Position(true, true)]
			public Vector2 swing_offset = new(1.00f, 1.00f);
			public float swing_rotation = -2.50f;

			public Melee.AttackType attack_type = AttackType.Swing;

			[Statistics.Info("Base Damage", description: "Base damage", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float damage_base;

			[Statistics.Info("Bonus Damage", description: "Random additional damage", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float damage_bonus;

			[Statistics.Info("Primary Damage", description: "TODO: Desc", format: "{0:0.##}x", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float primary_damage_multiplier = 1.00f;

			[Statistics.Info("Secondary Damage", description: "TODO: Desc", format: "{0:0.##}x", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float secondary_damage_multiplier = 1.00f;

			[Statistics.Info("Terrain Damage", description: "Damage to terrain", format: "{0:0.##}x", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float terrain_damage_multiplier = 1.00f;

			[Statistics.Info("Pain", description: "Pain multiplier.", format: "{0:0.##}x", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float pain_multiplier = 1.00f;

			[Statistics.Info("Stun", description: "Stun multiplier.", format: "{0:0.##}x", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float stun_multiplier = 1.00f;

			[Statistics.Info("Disarm Chance", description: "TODO: Desc", format: "{0:P2}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float disarm_chance = 0.02f;

			[Statistics.Info("Cooldown", description: "Time between attacks", format: "{0:0.##}s", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Medium)]
			public float cooldown;

			[Statistics.Info("Reach", description: "Melee weapon range", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float max_distance;

			[Statistics.Info("Area of Effect", description: "Size of the affected area", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float aoe;

			[Statistics.Info("Thickness", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Low)]
			public float thickness = 0.30f;

			[Statistics.Info("Knockback", description: "Multiplies knockback of the damage", format: "{0:0.##}x", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float knockback = 1.00f;

			[Statistics.Info("Yield", description: "Affects amount of material obtained from harvesting", format: "{0:P2}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float yield = 1.00f;

			[Statistics.Info("Penetration Falloff", description: "Modifies damage after each penetration", format: "{0:P2}", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Low)]
			public float penetration_falloff = 0.75f;

			[Statistics.Info("Penetration", description: "How many objects are hit in single strike", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public int penetration;

			[Statistics.Info("Damage Type", description: "Type of damage dealt", format: "{0}", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
			public Damage.Type damage_type;

			[Statistics.Info("Category", description: "TODO: Desc", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			public Melee.Category category;

			public Melee.Flags flags;

			public Physics.Layer hit_mask;
			public Physics.Layer hit_require;
			public Physics.Layer hit_exclude;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State: IComponent
		{
			[Save.Ignore, Net.Ignore] public float next_hit;
			[Save.Ignore, Net.Ignore] public float last_hit;
		}

		public static bool CanHitMaterial(Damage.Type damage_type, Material.Type material_type)
		{
			switch (damage_type)
			{
				case Damage.Type.Axe:
				{
					return material_type switch
					{
						Material.Type.Tree => true,
						Material.Type.Wood => true,
						Material.Type.Stone => false,
						Material.Type.Gravel => false,
						Material.Type.Flesh => true,
						Material.Type.Glass => true,
						Material.Type.Liquid => false,
						Material.Type.Metal => false,
						Material.Type.Soil => false,
						Material.Type.Powder => false,
						Material.Type.Foliage => true,
						Material.Type.Fabric => true,
						Material.Type.Rubber => true,
						Material.Type.Mushroom => true,
						Material.Type.Insect => true,
						Material.Type.Paper => true,
						Material.Type.Leather => true,
						Material.Type.Wire => true,
						Material.Type.Bone => true,
						Material.Type.Bramble => true,
						Material.Type.Chitin => true,
						Material.Type.Rubble => false,
						Material.Type.Scrap => false,
						Material.Type.Wreck => false,
						Material.Type.Tool => false,
						_ => false
					};
				}
				break;

				case Damage.Type.Slash:
				case Damage.Type.Stab:
				{
					return material_type switch
					{
						Material.Type.Wood => false,
						Material.Type.Stone => false,
						Material.Type.Gravel => false,
						Material.Type.Flesh => true,
						Material.Type.Glass => true,
						Material.Type.Liquid => false,
						Material.Type.Metal => false,
						Material.Type.Soil => false,
						Material.Type.Powder => false,
						Material.Type.Foliage => true,
						Material.Type.Fabric => true,
						Material.Type.Rubber => true,
						Material.Type.Mushroom => true,
						Material.Type.Insect => true,
						Material.Type.Paper => true,
						Material.Type.Leather => true,
						Material.Type.Wire => true,
						Material.Type.Bone => true,
						Material.Type.Chitin => true,
						Material.Type.Rubble => false,
						Material.Type.Scrap => false,
						Material.Type.Tool => false,
						_ => false
					};
				}
				break;

				case Damage.Type.Club:
				case Damage.Type.Crush:
				case Damage.Type.Blunt:
				case Damage.Type.Impact:
				{
					return material_type switch
					{
						Material.Type.Wood => true,
						Material.Type.Stone => false,
						Material.Type.Gravel => false,
						Material.Type.Flesh => true,
						Material.Type.Glass => true,
						Material.Type.Liquid => false,
						Material.Type.Metal => true,
						Material.Type.Soil => false,
						Material.Type.Powder => false,
						Material.Type.Foliage => false,
						Material.Type.Fabric => true,
						Material.Type.Rubber => true,
						Material.Type.Mushroom => true,
						Material.Type.Insect => true,
						Material.Type.Paper => true,
						Material.Type.Leather => true,
						Material.Type.Wire => false,
						Material.Type.Bone => true,
						Material.Type.Chitin => true,
						Material.Type.Rubble => false,
						Material.Type.Scrap => true,
						Material.Type.Wreck => true,
						Material.Type.Tool => false,
						_ => false
					};
				}
				break;

				case Damage.Type.Pickaxe:
				case Damage.Type.Drill:
				{
					return material_type switch
					{
						Material.Type.Tree => true,
						Material.Type.Wood => true,
						Material.Type.Stone => true,
						Material.Type.Gravel => true,
						Material.Type.Flesh => true,
						Material.Type.Glass => true,
						Material.Type.Liquid => false,
						Material.Type.Metal => true,
						Material.Type.Metal_Frame => true,
						Material.Type.Metal_Pole => true,
						Material.Type.Metal_Solid => true,
						Material.Type.Metal_Sheet => true,
						Material.Type.Cermet => true,
						Material.Type.Soil => true,
						Material.Type.Powder => true,
						Material.Type.Foliage => true,
						Material.Type.Fabric => true,
						Material.Type.Rubber => true,
						Material.Type.Mushroom => true,
						Material.Type.Insect => true,
						Material.Type.Paper => true,
						Material.Type.Leather => true,
						Material.Type.Wire => true,
						Material.Type.Bone => true,
						Material.Type.Chitin => true,
						Material.Type.Rubble => true,
						Material.Type.Scrap => true,
						Material.Type.Tool => true,
						_ => false
					};
				}
				break;

				case Damage.Type.Shovel:
				{
					return material_type switch
					{
						Material.Type.Wood => false,
						Material.Type.Stone => false,
						Material.Type.Gravel => true,
						Material.Type.Flesh => true,
						Material.Type.Glass => true,
						Material.Type.Liquid => false,
						Material.Type.Metal => false,
						Material.Type.Soil => true,
						Material.Type.Powder => true,
						Material.Type.Foliage => true,
						Material.Type.Fabric => false,
						Material.Type.Rubber => false,
						Material.Type.Mushroom => true,
						Material.Type.Insect => true,
						Material.Type.Paper => false,
						Material.Type.Leather => false,
						Material.Type.Wire => false,
						Material.Type.Bone => false,
						Material.Type.Chitin => false,
						Material.Type.Rubble => true,
						Material.Type.Scrap => true,
						Material.Type.Tool => false,
						_ => false
					};
				}
				break;

				case Damage.Type.Bite:
				case Damage.Type.Sting:
				{
					return material_type switch
					{
						Material.Type.Wood => true,
						Material.Type.Stone => false,
						Material.Type.Gravel => false,
						Material.Type.Flesh => true,
						Material.Type.Glass => true,
						Material.Type.Liquid => false,
						Material.Type.Metal => false,
						Material.Type.Soil => false,
						Material.Type.Powder => false,
						Material.Type.Foliage => false,
						Material.Type.Fabric => true,
						Material.Type.Rubber => true,
						Material.Type.Mushroom => true,
						Material.Type.Insect => true,
						Material.Type.Paper => true,
						Material.Type.Leather => true,
						Material.Type.Wire => false,
						Material.Type.Bone => true,
						Material.Type.Chitin => true,
						Material.Type.Rubble => false,
						Material.Type.Scrap => false,
						Material.Type.Tool => false,
						_ => false
					};
				}
				break;

				case Damage.Type.Fire:
				{
					return material_type switch
					{
						Material.Type.Tree => true,
						Material.Type.Wood => true,
						Material.Type.Stone => false,
						Material.Type.Gravel => false,
						Material.Type.Flesh => true,
						Material.Type.Glass => false,
						Material.Type.Liquid => false,
						Material.Type.Metal => false,
						Material.Type.Soil => false,
						Material.Type.Powder => false,
						Material.Type.Foliage => true,
						Material.Type.Fabric => true,
						Material.Type.Rubber => true,
						Material.Type.Mushroom => true,
						Material.Type.Insect => true,
						Material.Type.Paper => true,
						Material.Type.Leather => true,
						Material.Type.Wire => false,
						Material.Type.Bone => false,
						Material.Type.Chitin => true,
						Material.Type.Rubble => false,
						Material.Type.Scrap => false,
						Material.Type.Tool => false,
						_ => false
					};
				}
				break;

				case Damage.Type.Ram:
				case Damage.Type.Punch:
				{
					return material_type switch
					{
						Material.Type.Tree => true,
						Material.Type.Wood => true,
						Material.Type.Stone => false,
						Material.Type.Gravel => false,
						Material.Type.Flesh => true,
						Material.Type.Glass => true,
						Material.Type.Liquid => false,
						Material.Type.Metal => true,
						Material.Type.Cermet => true,
						Material.Type.Soil => false,
						Material.Type.Powder => false,
						Material.Type.Foliage => true,
						Material.Type.Fabric => true,
						Material.Type.Rubber => true,
						Material.Type.Mushroom => true,
						Material.Type.Insect => true,
						Material.Type.Paper => true,
						Material.Type.Leather => true,
						Material.Type.Wire => false,
						Material.Type.Bone => true,
						Material.Type.Chitin => true,
						Material.Type.Rubble => false,
						Material.Type.Scrap => true,
						Material.Type.Tool => true,
						_ => false
					};
				}
				break;

				case Damage.Type.Saw:
				{
					return material_type switch
					{
						Material.Type.Tree => true,
						Material.Type.Wood => true,
						Material.Type.Stone => false,
						Material.Type.Gravel => false,
						Material.Type.Flesh => true,
						Material.Type.Glass => true,
						Material.Type.Liquid => false,
						Material.Type.Metal => true,
						Material.Type.Soil => false,
						Material.Type.Powder => false,
						Material.Type.Foliage => true,
						Material.Type.Fabric => true,
						Material.Type.Rubber => true,
						Material.Type.Mushroom => true,
						Material.Type.Insect => true,
						Material.Type.Paper => true,
						Material.Type.Leather => true,
						Material.Type.Wire => true,
						Material.Type.Bone => true,
						Material.Type.Chitin => true,
						Material.Type.Rubble => false,
						Material.Type.Scrap => true,
						Material.Type.Tool => false,
						_ => false
					};
				}
				break;

				default:
				{
					return false;
				}
			}
		}

		[Flags]
		public enum HitResults: uint
		{
			None = 0u,

			Any = 1u << 0,
			Solid = 1u << 1,
			Terrain = 1u << 2,
		}

		// TODO: come up with a better name
		public static void IterateHit(ref Region.Data region, ref XorRandom random, Entity ent_melee, Entity ent_parent, Vector2 pos, Vector2 pos_target, Vector2 dir, float len, IFaction.Handle h_faction, in Melee.Data melee, ref Melee.State melee_state, out Vector2 pos_hit, out float modifier, out Melee.HitResults hit_results, out float dist_max, bool draw_gui, bool do_hit)
		{
			var penetration = melee.penetration;
			var index_max = -1;
			pos_hit = pos_target;
			modifier = 1.00f;
			dist_max = -1.00f;
			hit_results = Melee.HitResults.None;

			Span<LinecastResult> results = FixedArray.CreateSpan16<LinecastResult>(out var buffer); // LinecastResult[16];
			if (region.TryLinecastAll(pos, pos_target, melee.thickness, ref results, mask: melee.hit_mask, require: melee.hit_require, exclude: melee.hit_exclude & ~(Physics.Layer.Ignore_Melee)))
			{
				results.SortByDistance();

				// Find first solid/blocking shape
				for (var i = 0; i < results.Length; i++)
				{
					ref var result = ref results[i];
					if (result.layer.HasAny(Physics.Layer.Solid | Physics.Layer.World | Physics.Layer.Shield) && (result.layer.HasAny(Physics.Layer.Shield) || result.mask.HasAny(Physics.Layer.Solid)) && !result.layer.HasAny(Physics.Layer.Ignore_Melee))
					{
						if (result.entity == ent_parent || result.entity_parent == ent_parent || result.entity == ent_melee) continue;
						if (result.mask.HasNone(Physics.Layer.Solid))
						{
							if (h_faction.id != 0 && result.GetFactionHandle() == h_faction.id) continue;
							if (result.layer.HasNone(Physics.Layer.World | Physics.Layer.Shield) && (melee.flags.HasNone(Melee.Flags.No_Material_Filter) && !Melee.CanHitMaterial(melee.damage_type, result.material_type))) continue;
						}

						dist_max = Maths.Max(dist_max, result.alpha.Clamp01() * melee.max_distance);
						index_max = i;

#if CLIENT
						if (draw_gui && result.entity.IsValid())
						{
							GUI.DrawEntity(result.entity, color: Color32BGRA.Red.WithAlphaMult(0.40f));
							GUI.SetCursor(App.CursorType.Attack, 1000);
						}
#endif

						break;
					}
				}

				if (dist_max <= 0.00f)
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
							if (result.entity == ent_parent || result.entity_parent == ent_parent || result.entity == ent_melee) continue;
							if (result.mask.HasNone(Physics.Layer.Solid))
							{
								if (h_faction.id != 0 && result.GetFactionHandle() == h_faction.id) continue;
								if (!result.layer.HasAny(Physics.Layer.World | Physics.Layer.Shield) && (!melee.flags.HasAny(Melee.Flags.No_Material_Filter) && !Melee.CanHitMaterial(melee.damage_type, result.material_type))) continue;
							}

							var closest_result = result.GetClosestPoint(pos_target, true);
							if (!(result.layer.HasAny(Physics.Layer.Solid | Physics.Layer.World) && result.mask.HasAny(Physics.Layer.Solid) && !result.layer.HasAny(Physics.Layer.Ignore_Melee)) && Vector2.DistanceSquared(closest_result.world_position, pos_target) > (melee.thickness * melee.thickness)) continue;

							modifier *= melee.penetration_falloff;
							penetration--;

							if (!result.entity.IsValid()) hit_results.SetFlag(Melee.HitResults.Terrain, true);
							hit_results.SetFlag(Melee.HitResults.Any, true);

							pos_hit = closest_result.world_position;

#if CLIENT
							if (draw_gui && result.entity.IsValid())
							{
								GUI.DrawEntity(result.entity, color: Color32BGRA.Red.WithAlphaMult(0.40f));
								GUI.SetCursor(App.CursorType.Attack, 1000);
							}
#endif

							if (do_hit)
							{
								Melee.Hit(ref region, ent_melee, ent_parent, result.entity, pos_hit, dir, (closest_result.normal_raw - dir).GetNormalizedFast(), result.material_type, in melee, ref melee_state, ref random, damage_multiplier: modifier, h_faction: h_faction);
							}

							if (i == index_max) break;
						}
					}
				}
				else if (index_max >= 0)
				{
					ref var result = ref results[index_max];

					if (!result.entity.IsValid()) hit_results.SetFlag(Melee.HitResults.Terrain, true);
					hit_results.SetFlag(Melee.HitResults.Any | HitResults.Solid, true);

					var closest_result = result.GetClosestPoint(result.world_position, true);

					//pos_target = result.world_position;
					pos_hit = closest_result.world_position;

					if (do_hit)
					{
						Melee.Hit(ref region, ent_melee, ent_parent, result.entity, pos_hit, dir, (closest_result.normal_raw - dir).GetNormalizedFast(), result.material_type, in melee, ref melee_state, ref random, damage_multiplier: modifier, h_faction: h_faction);
					}
				}
			}

			if (!hit_results.HasAny(Melee.HitResults.Any))
			{
				var material_type = default(Material.Type);
				if (Terrain.TryGetTileAtWorldPosition(ref region.GetTerrain(), pos_target, out var tile))
				{
					material_type = tile.MaterialType;
				}

				if (material_type != Material.Type.None)
				{
					hit_results.AddFlag(HitResults.Terrain);

					if (do_hit)
					{
						Melee.Hit(ref region, ent_melee, ent_parent, default, pos_target, dir, -dir, material_type, in melee, ref melee_state, ref random, damage_multiplier: modifier, h_faction: h_faction);
					}
				}
			}
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
				var c_radius = radius * region.GetWorldToCanvasScale();
				var c_pos = region.WorldToCanvas(this.pos_target);

				//if (this.melee.category == Melee.Category.Pointed)
				{
					//if (!entity.IsValid())
					{
						GUI.DrawTerrainOutline(ref region, this.pos_hit, radius, Color32BGRA.Yellow.WithAlphaMult(0.75f));
					}

					GUI.DrawCircleFilled(c_pos, c_radius, this.color.WithAlphaMult(0.10f), segments: 16);
					GUI.DrawCircle(c_pos, c_radius, this.color.WithAlphaMult(0.40f), thickness: 1.00f, segments: 16);
					GUI.DrawCircleFilled(region.WorldToCanvas(this.pos_hit), 3.00f, this.color, segments: 4);
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent), HasRelation(Source.Modifier.Owned, Relation.Type.Stored, false)]
		public static void OnGUI(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Parent] in Player.Data player,
		[Source.Owned] in Melee.Data melee, [Source.Owned] ref Melee.State melee_state,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] in Body.Data body, [Source.Parent, Optional] in Faction.Data faction)
		{
			if (!GUI.IsHovered)
			{
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

				var pos_target = Maths.ClampRadius(control.mouse.position, pos, melee.max_distance);
				var len = Vector2.Distance(pos, pos_target);

				Melee.IterateHit(region: ref region, random: ref random, ent_melee: entity, ent_parent: parent, pos: pos, pos_target: pos_target, dir: dir, len: len, h_faction: faction.id, melee: in melee, melee_state: ref melee_state, pos_hit: out var pos_hit, modifier: out var modifier, hit_results: out var hit_results, dist_max: out var dist_max, draw_gui: true, do_hit: false);

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

				if (hit_results.HasAny(Melee.HitResults.Terrain)) gui.color = Color32BGRA.Yellow;
				else if (hit_results.HasAny(Melee.HitResults.Any)) gui.color = Color32BGRA.Red;
				else gui.color = Color32BGRA.Grey;

				gui.Submit();
			}
		}

		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region), HasRelation(Source.Modifier.Owned, Relation.Type.Stored, false)]
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

		public static void Hit(ref Region.Data region, Entity ent_attacker, Entity ent_owner, Entity ent_target, Vector2 hit_pos, Vector2 dir, Vector2 normal, Material.Type material_type, in Melee.Data melee, ref Melee.State melee_state, ref XorRandom random, float damage_multiplier = 1.00f, IFaction.Handle h_faction = default)
		{
#if CLIENT
			var shake_mult = Maths.Clamp(melee.knockback, 0.00f, 1.00f);

			//Sound.Play(melee.sound_swing, hit_pos, volume: melee.sound_volume, random.NextFloatRange(0.90f, 1.10f) * melee.sound_pitch, size: melee.sound_size);
			Shake.Emit(ref region, hit_pos, 0.40f * shake_mult, 0.40f * shake_mult, radius: 2.00f);
#endif

#if SERVER
			var damage = melee.damage_base + random.NextFloatRange(0.00f, melee.damage_bonus) * damage_multiplier;
			//Damage.Hit(attacker: ent_attacker, owner: ent_owner, target: ent_target,
			//	world_position: hit_pos, direction: dir, normal: normal,
			//	damage: damage, damage_type: melee.damage_type, yield: melee.yield, primary_damage_multiplier: melee.primary_damage_multiplier, secondary_damage_multiplier: melee.secondary_damage_multiplier, terrain_damage_multiplier: melee.terrain_damage_multiplier,
			//	target_material_type: material_type, knockback: melee.knockback, size: melee.aoe, faction_id: faction.id);

			Damage.Hit(ent_attacker: ent_attacker, ent_owner: ent_owner, ent_target: ent_target,
				position: hit_pos, velocity: dir * 8.00f, normal: normal,
				damage_integrity: damage * melee.primary_damage_multiplier, damage_durability: damage * melee.secondary_damage_multiplier, damage_terrain: damage * melee.terrain_damage_multiplier,
				target_material_type: material_type, damage_type: melee.damage_type,
				yield: melee.yield, size: melee.aoe, impulse: melee.knockback, faction_id: h_faction.id, pain: melee.pain_multiplier, stun: melee.stun_multiplier);

			if (melee.disarm_chance > 0.00f && random.NextBool(melee.disarm_chance) && ent_target.IsValid())
			{
				// TODO: hack, find some cleaner way to target's arm entity
				ref var npc = ref ent_target.GetComponent<NPC.Data>();
				if (npc.IsNotNull())
				{
					var ent_holder_arm = npc.ent_arm;
					if (ent_holder_arm.IsAlive())
					{
						Arm.Drop(ent_holder_arm, direction: -normal);
					}
				}
			}
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
				ent_attacker.TriggerEventDeferred(data, sync: true);
#endif
			}
			else
			{
				ent_attacker.TriggerEvent(ref data);
			}
		}

		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region)]
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.50f)]
		public static void UpdateHoldable([Source.Owned] in Melee.Data melee, [Source.Owned] ref Holdable.Data holdable, [Source.Owned, Optional] in Aimable.Data aimable)
		{
			holdable.hints.AddFlag(NPC.ItemHints.Melee | NPC.ItemHints.Weapon | NPC.ItemHints.Short_Range | NPC.ItemHints.Usable);
			holdable.grip_min = aimable.deadzone;
		}

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, order: 100), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateHead(ISystem.Info info, ref Region.Data region, Entity entity, ref XorRandom random,
		[Source.Owned] in Head.Data head, [Source.Owned] in Melee.Data melee, [Source.Owned] ref Melee.State melee_state,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Control.Data control, [Source.Owned] in Transform.Data transform)
		{
			var max = Maths.Min(0.20f, melee.cooldown * 0.50f);

			var delta = info.WorldTime - melee_state.last_hit;
			if (delta < max)
			{
				var t = Maths.EaseInOut(Maths.NormalizeClamp(info.WorldTime - melee_state.last_hit, max).Inv01(), Maths.Easing.Bounce) - 0.50f;

				var dir = (control.mouse.position - transform.position).GetNormalized(out var len);
				len = Maths.Min(len, melee.max_distance);

				body.AddForceWorld(dir * body.GetMass() * App.tickrate * 25.00f * t, transform.LocalToWorld(melee.swing_offset));

				//App.WriteLine(t);
			}
		}

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasRelation(Source.Modifier.Owned, Relation.Type.Stored, false)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Melee.Data melee, [Source.Owned] ref Melee.State melee_state,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] in Body.Data body, [Source.Parent, Optional] in Faction.Data faction)
		{
			var key = melee.flags.HasAny(Melee.Flags.Use_RMB) ? Mouse.Key.Right : Mouse.Key.Left;

			if (control.mouse.GetKey(key) && info.WorldTime >= melee_state.next_hit)
			{
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

				var pos_target = Maths.ClampRadius(control.mouse.position, pos, melee.max_distance); //  pos + (dir * len);
				var len = Vector2.Distance(pos, pos_target);

#if CLIENT
				Sound.Play(melee.sound_swing, transform.position, volume: melee.sound_volume, random.NextFloatRange(0.90f, 1.10f) * melee.sound_pitch, size: melee.sound_size);
#endif

				Melee.IterateHit(region: ref region, random: ref random, ent_melee: entity, ent_parent: parent, pos: pos, pos_target: pos_target, dir: dir, len: len, h_faction: faction.id, melee: in melee, melee_state: ref melee_state, pos_hit: out var pos_hit, modifier: out var modifier, hit_results: out var hit_results, dist_max: out var dist_max, draw_gui: false, do_hit: true);
			}
		}
	}
}
