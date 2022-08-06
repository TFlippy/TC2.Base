
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

				melee_state.last_hit = info.WorldTime;
				melee_state.next_hit = info.WorldTime + melee.cooldown;

				var pos = transform.LocalToWorld(melee.hit_offset);

				var dir = default(Vector2);
				var len = melee.max_distance;

				switch (melee.attack_type)
				{
					default:
					case Melee.AttackType.Swing:
					{
						dir = transform.LocalToWorldDirection(melee.hit_direction.RotateByRad(-melee.swing_rotation * 0.50f));
					}
					break;

					case Melee.AttackType.Thrust:
					{
						dir = (control.mouse.position - transform.position).GetNormalized();
					}
					break;
				}

#if CLIENT
				Sound.Play(melee.sound_swing, transform.position, volume: melee.sound_volume, random.NextFloatRange(0.90f, 1.10f) * melee.sound_pitch, size: melee.sound_size);
#endif

				Span<LinecastResult> results = stackalloc LinecastResult[16];
				if (region.TryLinecastAll(pos, pos + (dir * len), melee.thickness, ref results, mask: melee.hit_mask, exclude: melee.hit_exclude))
				{
					results.SortByDistance();

					var parent = body.GetParent();
					var modifier = 1.00f;
					var flags = Damage.Flags.None;
					var hit_terrain = false;

					var penetration = melee.penetration;
					for (var i = 0; i < results.Length && penetration >= 0; i++)
					{
						ref var result = ref results[i];
						if (result.entity == parent || result.entity_parent == parent || result.entity == entity) continue;
						if (faction.id != 0 && result.GetFactionID() == faction.id) continue;

						var is_terrain = !result.entity.IsValid();
						if (is_terrain)
						{
							if (hit_terrain) continue;
							hit_terrain = true;
						}

#if CLIENT
						var shake_mult = Maths.Clamp(melee.knockback, 0.00f, 1.00f);
						Shake.Emit(ref region, transform.position, 0.40f * shake_mult, 0.40f * shake_mult, radius: 2.00f);
#endif

#if SERVER
						var damage = melee.damage_base + random.NextFloatRange(0.00f, melee.damage_bonus);
						Damage.Hit(attacker: entity, owner: parent, target: result.entity,
							world_position: result.world_position, direction: dir, normal: -dir,
							damage: damage * modifier, damage_type: melee.damage_type, yield: melee.yield, primary_damage_multiplier: melee.primary_damage_multiplier, secondary_damage_multiplier: melee.secondary_damage_multiplier, terrain_damage_multiplier: melee.terrain_damage_multiplier,
							target_material_type: result.material_type, knockback: melee.knockback, size: melee.aoe, flags: flags, faction_id: faction.id);
#endif

						var data = new Melee.HitEvent();
						data.ent_attacker = entity;
						data.ent_owner = parent;
						data.ent_target = result.entity;
						data.world_position = result.world_position;
						data.direction = dir;
						data.random = random;
						data.target_material_type = result.material_type;

						if (melee.flags.HasAny(Melee.Flags.Sync_Hit_Event))
						{
#if SERVER
							entity.NotifyDeferred(data, sync: true);
#endif
						}
						else
						{
							entity.Notify(ref data);
						}

						flags |= Damage.Flags.No_Sound;
						modifier *= melee.penetration_falloff;
						penetration--;
					}
				}
			}
		}
	}
}
