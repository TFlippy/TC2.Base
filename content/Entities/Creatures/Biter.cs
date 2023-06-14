
namespace TC2.Base.Components
{
	public static partial class Biter
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Biter.State>()]
		public partial struct Data: IComponent
		{
			public Sound.Handle sound;

			public float damage_base;
			public float damage_bonus;

			public float cooldown = 0.50f;
			public float max_distance = 1.00f;
			public float aoe = 0.125f;
			public float thickness = 0.125f;
			public float velocity = 30.00f;
			public float force = 500.00f;
			public float knockback = 1.00f;

			public float penetration_falloff = 0.75f;
			public int penetration = 1;

			public Damage.Type damage_type;

			public Physics.Layer hit_mask;
			public Physics.Layer hit_exclude;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct State: IComponent
		{
			[Net.Ignore, Save.Ignore] public float next_attack;
		}

		[ISystem.LateUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void Update(ref Region.Data region, ISystem.Info info, Entity entity, ref XorRandom random,
		[Source.Owned] in Biter.Data biter, [Source.Owned] ref Biter.State biter_state, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Control.Data control, [Source.Owned] in Transform.Data transform)
		{
			if (organic_state.consciousness_shared > 0.30f && info.WorldTime > biter_state.next_attack && control.mouse.GetKey(Mouse.Key.Left))
			{
				biter_state.next_attack = info.WorldTime + biter.cooldown;

				var dir = (control.mouse.position - transform.position).GetNormalized(out var len);
				len = MathF.Min(len, biter.max_distance);

				body.AddForce(dir * body.GetMass() * App.tickrate * biter.velocity);

				Span<LinecastResult> results = stackalloc LinecastResult[16];
				if (region.TryLinecastAll(transform.position, transform.position + (dir * len), biter.thickness, ref results, mask: biter.hit_mask, exclude: biter.hit_exclude))
				{
					results.SortByDistance();

					var parent = body.GetParent();
					var modifier = 1.00f;
					var flags = Damage.Flags.None;
					var hit_terrain = false;

					var penetration = biter.penetration;
					for (var i = 0; i < results.Length && penetration >= 0; i++)
					{
						ref var result = ref results[i];
						if (result.entity == parent || result.entity_parent == parent || result.entity == entity) continue;

						var is_terrain = !result.entity.IsValid();
						if (is_terrain)
						{
							if (hit_terrain) continue;
							hit_terrain = true;
						}

#if CLIENT
						Shake.Emit(ref region, transform.position, 0.40f, 0.40f, radius: 2.00f);
#endif

#if SERVER
						var damage = (biter.damage_base + random.NextFloatRange(0.00f, biter.damage_bonus)) * modifier;
						//Damage.Hit(attacker: entity, owner: parent, target: result.entity,
						//	world_position: result.world_position, direction: dir, normal: -dir,
						//	damage: damage * modifier, damage_type: biter.damage_type,
						//	target_material_type: result.material_type, knockback: biter.knockback, size: biter.aoe, flags: flags, speed: 8.00f);

						Damage.Hit(ent_attacker: entity, ent_owner: parent, ent_target: result.entity,
							position: result.world_position, velocity: dir * 8.00f, normal: -dir,
							damage_integrity: damage, damage_durability: damage, damage_terrain: damage,
							target_material_type: result.material_type, damage_type: biter.damage_type,
							yield: 0.00f, size: biter.aoe, impulse: biter.force, flags: flags);
#endif

						//flags |= Damage.Flags.No_Sound;
						modifier *= biter.penetration_falloff;
						penetration--;
					}
				}
			}
		}
	}
}
