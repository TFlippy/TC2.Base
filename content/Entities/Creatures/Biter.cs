
namespace TC2.Base.Components
{
	public static partial class Biter
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Biter.State>()]
		public partial struct Data: IComponent
		{
			public Sound.Handle sound = default;

			public float damage_base = default;
			public float damage_bonus = default;

			public float cooldown = 0.50f;
			public float max_distance = 1.00f;
			public float aoe = 0.125f;
			public float thickness = 0.125f;
			public float velocity = 30.00f;
			public float knockback = 1.00f;

			public float penetration_falloff = 0.75f;
			public int penetration = 1;

			public Damage.Type damage_type = default;
			public Physics.Layer hit_mask = default;
			public Physics.Layer hit_exclude = default;

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
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] in Biter.Data biter, [Source.Owned] ref Biter.State biter_state, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Control.Data control, [Source.Owned] in Transform.Data transform)
		{
			if (organic_state.consciousness_shared > 0.30f && info.WorldTime > biter_state.next_attack && control.mouse.GetKey(Mouse.Key.Left))
			{
				ref var region = ref info.GetRegion();
				var random = XorRandom.New();

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
						var damage = biter.damage_base + random.NextFloatRange(0.00f, biter.damage_bonus);
						Damage.Hit(attacker: entity, owner: parent, target: result.entity,
							world_position: result.world_position, direction: dir, normal: -dir,
							damage: damage * modifier, damage_type: biter.damage_type,
							target_material_type: result.material_type, knockback: biter.knockback, size: biter.aoe, flags: flags, speed: 8.00f);
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
