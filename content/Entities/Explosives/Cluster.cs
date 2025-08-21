namespace TC2.Base.Components
{
	public static partial class Cluster
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			No_Impact = 1 << 0,
			Reverse_Direction = 1 << 1,
			No_Velocity_Rotate = 1 << 2,
			Align_Normal = 1 << 3,
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			public Vector2 offset;
			public Vector2 velocity;

			public float radius = 0.250f;

			public Prefab.Handle prefab;
			public float spread;
			public float speed;

			public float damage_modifier = 1.00f;

			public float speed_modifier_min = 0.30f;
			public float speed_modifier_max = 1.30f;

			public float lifetime_modifier_min = 0.50f;
			public float lifetime_modifier_max = 1.00f;

			public float min_explode_lifetime;

			public int count;

			public Cluster.Flags flags;
		}

#if SERVER
		[Shitcode]
		[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region), HasComponent<Body.Data>(Source.Modifier.Owned, false)]
		public static void OnRemoveProjectile(ref XorRandom random, ref Region.Data region, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Cluster.Data cluster, [Source.Owned] in Projectile.Data projectile)
		{
			if (cluster.count > 0 && cluster.prefab.id != 0)
			{
				if (projectile.flags.HasAny(Projectile.Flags.Impact) && cluster.flags.HasAny(Cluster.Flags.No_Impact)) return;
				if (projectile.elapsed < cluster.min_explode_lifetime) return;

				for (var i = 0; i < cluster.count; i++)
				{
					var projectile_init =
					(
						damage_mult: cluster.damage_modifier,
						vel: projectile.velocity.RotateByRad(random.NextFloat(cluster.spread)) * random.NextFloatRange(cluster.speed_modifier_min, cluster.speed_modifier_max),
						lifetime_mult: random.NextFloatRange(cluster.lifetime_modifier_min, cluster.lifetime_modifier_max),
						owner: projectile.ent_owner,
						faction_id: projectile.faction_id
					);

					if (cluster.speed > Maths.epsilon)
					{
						projectile_init.vel += new Vector2(cluster.speed, 0.00f).RotateByRad(random.NextFloat(cluster.spread)) * random.NextFloatRange(cluster.speed_modifier_min, cluster.speed_modifier_max);
					}

					if (cluster.flags.HasAny(Cluster.Flags.No_Velocity_Rotate))
					{
						projectile_init.vel += cluster.velocity * random.NextFloatRange(cluster.speed_modifier_min, cluster.speed_modifier_max);
					}
					else
					{
						projectile_init.vel += transform.LocalToWorldDirection(cluster.velocity * random.NextFloatRange(cluster.speed_modifier_min, cluster.speed_modifier_max));
					}

					if (cluster.flags.HasAny(Cluster.Flags.Reverse_Direction))
					{
						projectile_init.vel *= -1.00f;
					}

					region.SpawnPrefab(cluster.prefab, transform.position).ContinueWith(ent =>
					{
						ref var projectile = ref ent.GetComponent<Projectile.Data>();
						if (!projectile.IsNull())
						{
							projectile.damage_base *= projectile_init.damage_mult;
							projectile.damage_bonus *= projectile_init.damage_mult;
							projectile.velocity = projectile_init.vel;
							projectile.ent_owner = projectile_init.owner;
							projectile.faction_id = projectile_init.faction_id;
							projectile.lifetime *= projectile_init.lifetime_mult;
							projectile.Sync(ent, true);
						}

						ref var explosive = ref ent.GetComponent<Explosive.Data>();
						if (!explosive.IsNull())
						{
							explosive.ent_owner = projectile_init.owner;
							explosive.Sync(ent, true);
						}
					});
				}
			}
		}

		[Shitcode]
		[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region), HasComponent<Projectile.Data>(Source.Modifier.Owned, false)]
		public static void OnRemoveBody(ref XorRandom random, ref Region.Data region, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Cluster.Data cluster, [Source.Owned] in Body.Data body, [Source.Owned] in Explosive.Data explosive)
		{
			if (explosive.flags.HasAll(Explosive.Flags.Primed) && cluster.count > 0 && cluster.prefab.id != 0)
			{
				var dir = (Vec2f)body.GetVelocity().GetNormalized(out var vel);
				if (cluster.flags.HasAny(Flags.Align_Normal) && body.TryGetArbiterNormal(ref dir))
				{

				}

				for (var i = 0; i < cluster.count; i++)
				{
					var projectile_init =
					(
						damage_mult: cluster.damage_modifier,
						vel: Vec2f.RotateByRad(dir, random.NextFloat(cluster.spread)) * (Maths.Max(vel * random.NextFloatRange(cluster.speed_modifier_min, cluster.speed_modifier_max), 1.00f) + cluster.speed),
						lifetime_mult: random.NextFloatRange(cluster.lifetime_modifier_min, cluster.lifetime_modifier_max),
						owner: body.GetParent(),
						faction_id: body.GetFaction()
					);

					if (cluster.speed > Maths.epsilon)
					{
						projectile_init.vel += new Vector2(cluster.speed, 0.00f).RotateByRad(random.NextFloat(cluster.spread)) * random.NextFloatRange(cluster.speed_modifier_min, cluster.speed_modifier_max);
					}

					if (cluster.flags.HasAny(Cluster.Flags.No_Velocity_Rotate))
					{
						projectile_init.vel += cluster.velocity * random.NextFloatRange(cluster.speed_modifier_min, cluster.speed_modifier_max);
					}
					else
					{
						projectile_init.vel += transform.LocalToWorldDirection(cluster.velocity * random.NextFloatRange(cluster.speed_modifier_min, cluster.speed_modifier_max));
					}

					if (cluster.flags.HasAny(Cluster.Flags.Reverse_Direction))
					{
						projectile_init.vel *= -1.00f;
					}

					region.SpawnPrefab(cluster.prefab, transform.position + cluster.offset + random.NextUnitVector2Range(cluster.radius * 0.50f, cluster.radius)).ContinueWith(ent =>
					{
						ref var projectile = ref ent.GetComponent<Projectile.Data>();
						if (projectile.IsNotNull())
						{
							projectile.damage_base *= projectile_init.damage_mult;
							projectile.damage_bonus *= projectile_init.damage_mult;
							projectile.velocity = projectile_init.vel;
							projectile.ent_owner = projectile_init.owner;
							projectile.faction_id = projectile_init.faction_id;
							projectile.lifetime *= projectile_init.lifetime_mult;
							projectile.Sync(ent, true);
						}

						ref var explosive = ref ent.GetComponent<Explosive.Data>();
						if (explosive.IsNotNull())
						{
							explosive.ent_owner = projectile_init.owner;
							explosive.Sync(ent, true);
						}
					});
				}
			}
		}
#endif
	}
}
