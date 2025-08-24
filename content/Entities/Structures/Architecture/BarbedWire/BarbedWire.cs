
namespace TC2.Base.Components
{
	public static partial class BarbedWire
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data(): IComponent
		{
			public float damage = 10.00f;
			public float speed_multiplier = 0.30f;
			public float max_force;
			public float min_bias;

			public Damage.Type damage_type = Damage.Type.Scratch;
			[Save.Ignore, Net.Ignore] public float t_next_hit;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, Entity entity, ref XorRandom random, 
		[Source.Owned] ref BarbedWire.Data barbed_wire, [Source.Owned] ref Body.Data body, [Source.Owned] in Transform.Data transform)
		{
			//var ts = Timestamp.Now();

			if (body.HasArbiters())
			{
#if SERVER
				var hit = false;
				var can_hit = info.WorldTime >= barbed_wire.t_next_hit;
#endif

				foreach (var arbiter in body.GetArbiters())
				{
					arbiter.MultiplyVelocity(barbed_wire.speed_multiplier);

					//arbiter.AddForce(arbiter.GetNormal() * arbiter.GetContact().GetBias() * barbed_wire.max_force);

#if SERVER
					if (can_hit)
					{
						var material_type = arbiter.GetMaterial();
						switch (material_type)
						{
							case Material.Type.Flesh:
							case Material.Type.Insect:
							{
								hit = true;

								var vel_sq = arbiter.GetBodyVelocity().LengthSquared();

								var ent_hit = arbiter.GetEntity();
								if (ent_hit.IsValid() && vel_sq > 0.40f && random.NextBool(vel_sq * 0.10f))
								{
									//entity.Hit(ent_hit, ent_hit, arbiter.GetPosition(), -arbiter.GetNormal(), arbiter.GetNormal(), damage: barbed_wire.damage, target_material_type: material_type, damage_type: Damage.Type.Scratch, speed: 2.00f);

									var damage = barbed_wire.damage;

									Damage.Hit(ent_attacker: entity, ent_owner: entity, ent_target: ent_hit,
										position: arbiter.GetBodyPosition(), velocity: -arbiter.GetNormal() * 2.00f, normal: arbiter.GetNormal(),
										damage_integrity: damage, damage_durability: damage, damage_terrain: damage,
										target_material_type: material_type, damage_type: barbed_wire.damage_type,
										yield: 0.00f, size: 0.50f, impulse: 0.00f, flags: Damage.Flags.No_Loot_Pickup | Damage.Flags.No_Loot_Notification);


									can_hit = false;
								}
							}
							break;
						}
					}
#endif
				}

#if SERVER
				if (hit)
				{
					barbed_wire.t_next_hit = info.WorldTime + 0.40f;
				}
#endif
			}
			//App.WriteLine($"{ts.GetMilliseconds():0.0000} ms");
		}
	}
}
