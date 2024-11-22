
namespace TC2.Base.Components
{
	public static partial class ProjectileExt
	{
		public static Sound.Handle[] snd_ricochet =
		{
			"ricochet.00"
		};

		[ISystem.Event<Projectile.ImpactEvent>(ISystem.Mode.Single, ISystem.Scope.Region, order: 100)]
		public static void OnImpact(ISystem.Info info, ref Region.Data region, ref XorRandom random, ref Projectile.ImpactEvent ev,
		[Source.Owned] ref Projectile.Data projectile, Entity ent_projectile)
		{
			if (projectile.ricochet_count > 0 && ev.flags.HasNone(Projectile.ImpactEvent.Flags.Is_Ricochet))
			{
				var ricochet_chance = (ev.ricochet_base + ev.ricochet_extra) * (1.00f - (ev.hit_applied_ratio * 0.95f)) * projectile.ricochet_chance_multiplier;
				if (ev.flags.TryAddFlag(Projectile.ImpactEvent.Flags.Is_Ricochet, ev.random.NextFloat01() < ricochet_chance))
				{

				}
			}

			if (ev.flags.HasAny(Projectile.ImpactEvent.Flags.Is_Ricochet))
			{
				ev.armor_pierce = 0.00f;
			
				projectile.ent_owner = default;
				projectile.faction_id = default;

#if CLIENT
				if (ev.speed > 100.00f) Sound.Play(snd_ricochet.GetRandom(ref random), ev.hit_position, volume: random.NextFloatExtra(0.40f, 0.10f), pitch: random.NextFloatRange(0.70f, 1.60f), size: 0.10f, priority: 0.25f, dist_multiplier: random.NextFloatExtra(1.50f, 0.10f));
#endif
			}

#if SERVER
			if (ev.damage > 0.00f && ev.flags.HasNone(Projectile.ImpactEvent.Flags.No_Damage))
			{
				//App.WriteLine($"hit {ev.hit_dot} {ev.damage_type} {ev.hit_material_type}");

				var impulse = projectile.mass * ev.speed * projectile.knockback_multiplier * ev.hit_dot;

				Damage.Hit(ent_attacker: ent_projectile, ent_owner: projectile.ent_owner, ent_target: ev.ent_hit,
					position: ev.flags.HasAny(Projectile.ImpactEvent.Flags.Is_World) ? ev.hit_position_raw : ev.hit_position, velocity: ev.hit_direction * ev.speed, normal: ev.hit_normal,
					damage_integrity: ev.damage, damage_durability: ev.damage, damage_terrain: ev.damage * projectile.terrain_damage_mult,
					armor_pierce: ev.armor_pierce * ev.hit_dot,
					target_material_type: ev.hit_material_type, damage_type: ev.damage_type,
					yield: 0.80f, size: Maths.SnapCeil(Maths.Max(0.125f, projectile.size), 0.125f) * random.NextFloatExtra(2.50f, 1.00f), impulse: impulse, stun: projectile.stun_multiplier * ev.hit_applied_ratio,
					faction_id: projectile.faction_id, flags: Damage.Flags.No_Loot_Pickup);
			}
#endif

//			if (ev.flags.HasAny(Projectile.ImpactEvent.Flags.Is_Ricochet))
//			{
//				//ev.speed *= Maths.Lerp((ev.random.NextFloat01() * (1.00f - ev.hit_applied_ratio)).Clamp01(), 1.00f, projectile.ricochet_velocity_ratio);
//				//projectile.velocity = ev.ricochet_direction.RotateByRad(ev.random.NextFloat(0.30f)) * ev.speed;
//				//projectile.angular_velocity += ev.random.NextFloat(3);
//				//projectile.angular_velocity *= ev.random.NextFloatExtra(1.10f, 0.20f);

//				//App.WriteLine("ricocheted");
//#if CLIENT
//				if (ev.speed > 100.00f) Sound.Play(snd_ricochet.GetRandom(ref random), ev.hit_position, volume: random.NextFloatExtra(0.40f, 0.10f), pitch: random.NextFloatRange(0.70f, 1.60f), size: 0.10f, priority: 0.25f, dist_multiplier: random.NextFloatExtra(1.50f, 0.10f));
//#endif
//			}

			//App.WriteLine($"dot: {ev.hit_dot}; mat: {ev.hit_material_type}; chance: {ricochet_chance}; rc: {projectile.ricochet_count}; speed: {ev.speed}; flags: {ev.flags}");
		}
	}

	public static partial class Rocket
	{
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct Data(): IComponent
		{
			public float mass = 1.00f;
			public float force;
			public float fuel_time = 1.00f;
			public float smoke_amount = 1.00f;
			public float delay;
			public Vector2 velocity;

			[Net.Ignore, Save.Ignore, Asset.Ignore] public float smoke_accumulator = 0.00f;
		}

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateBody(ISystem.Info info, Entity entity, [Source.Owned] ref Rocket.Data rocket, [Source.Owned] ref Body.Data body, [Source.Owned] ref Transform.Data transform)
		{
			if (rocket.delay <= 0.00f)
			{
				if (rocket.fuel_time > 0.00f)
				{
					var dir = transform.GetDirection();
					body.AddForce(dir * (rocket.force));
				}

				rocket.fuel_time = Maths.Max(rocket.fuel_time - App.fixed_update_interval_s, 0.00f);
			}
			else
			{
				rocket.delay -= info.DeltaTime;
			}
		}

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateProjectile(ISystem.Info info, Entity entity, [Source.Owned] ref Rocket.Data rocket, [Source.Owned] ref Projectile.Data projectile, [Source.Owned] ref Transform.Data transform)
		{
			if (rocket.delay <= 0.00f)
			{
				if (rocket.fuel_time > 0.00f)
				{
					//var dir = projectile.velocity.GetNormalized(out var vel);
					//if (projectile.rotation != 0.00f)
					//{
					//	dir = dir.RotateByRad(-projectile.rotation);
					//}

					var dir = transform.GetDirection();
					var step = dir * ((rocket.force / rocket.mass) * App.fixed_update_interval_s);

					projectile.velocity += step;
					rocket.fuel_time = Maths.Max(rocket.fuel_time - App.fixed_update_interval_s, 0.00f);
				}
			}
			else
			{
				rocket.delay -= info.DeltaTime;
			}
		}

#if CLIENT
		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSmokeBody(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity, [Source.Owned] ref Rocket.Data rocket, [Source.Owned] in Body.Data body, [Source.Owned] in Transform.Data transform)
		{
			if (rocket.fuel_time > 0.00f && rocket.delay <= 0.00f)
			{
				var modifier = Maths.Clamp(rocket.fuel_time, 0.00f, 1.00f);

				rocket.smoke_accumulator += rocket.smoke_amount;

				while (rocket.smoke_accumulator >= 1.00f)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = texture_smoke,
						pos = transform.position,
						lifetime = random.NextFloatRange(10.00f, 15.00f),
						fps = random.NextByteRange(1, 3),
						frame_count = 64,
						frame_count_total = 64,
						frame_offset = random.NextByteRange(0, 64),
						scale = random.NextFloatRange(0.10f, 0.15f) * modifier,
						angular_velocity = random.NextFloatRange(-0.70f, 0.70f),
						force = new Vector2(random.NextFloatRange(4.00f, 8.00f), -random.NextFloatRange(0.10f, 0.50f)),
						rotation = random.NextFloat(10.00f),
						growth = random.NextFloatRange(0.10f, 0.20f),
						color_a = new Color32BGRA(140, 220, 220, 220).WithAlphaMult(modifier * random.NextFloatRange(0.70f, 1.00f)),
						color_b = new Color32BGRA(000, 150, 150, 150),
						drag = 0.15f,
						vel = -body.GetVelocity() * 0.25f
					});

					rocket.smoke_accumulator -= 1.00f;
				}

				//Particle.Spawn(ref region, particle);
			}
		}

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSmokeProjectile(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity, [Source.Owned] ref Rocket.Data rocket, [Source.Owned] in Projectile.Data projectile, [Source.Owned] in Transform.Data transform)
		{
			if (rocket.fuel_time > 0.00f && rocket.delay <= 0.00f && projectile.elapsed >= 0.15f)
			{
				var modifier = Maths.Clamp(rocket.fuel_time, 0.00f, 1.00f);

				rocket.smoke_accumulator += rocket.smoke_amount;

				var dir = transform.GetDirection();
				var speed = projectile.velocity.Length();
				// .GetNormalized(out var vel);
				//if (projectile.rotation != 0.00f) dir = dir.RotateByRad(-projectile.rotation);

				while (rocket.smoke_accumulator >= 1.00f)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = texture_smoke,
						pos = transform.position - (dir * speed * App.fixed_update_interval_s * 4.00f),
						lifetime = random.NextFloatRange(10.00f, 15.00f),
						fps = random.NextByteRange(1, 3),
						frame_count = 64,
						frame_count_total = 64,
						frame_offset = random.NextByteRange(0, 64),
						scale = random.NextFloatRange(0.15f, 0.20f),
						angular_velocity = random.NextFloatRange(-0.70f, 0.70f),
						force = new Vector2(random.NextFloatRange(4.00f, 8.00f), -random.NextFloatRange(0.10f, 0.50f)),
						rotation = random.NextFloat(10.00f),
						growth = random.NextFloatRange(0.10f, 0.20f),
						color_a = new Color32BGRA(140, 220, 220, 220).WithAlphaMult(modifier * random.NextFloatRange(0.70f, 1.00f)),
						color_b = new Color32BGRA(000, 150, 150, 150),
						drag = random.NextFloatRange(0.06f, 0.08f),
						stretch = new Vector2(1, 2),
						vel = (-dir * speed * 0.40f) + random.NextUnitVector2Range(0.50f, 1.50f),
					});

					rocket.smoke_accumulator -= 1.00f;
				}

				//Particle.Spawn(ref region, particle);
			}
		}

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateLight(ISystem.Info info, Entity entity, [Source.Owned] in Rocket.Data rocket, [Source.Owned, Pair.Component<Rocket.Data>] ref Light.Data light)
		{
			var modifier = Maths.Clamp(rocket.fuel_time * 1.50f, 0.00f, 1.00f);
			light.intensity = modifier;
		}
#endif
	}
}
