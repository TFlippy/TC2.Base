
namespace TC2.Base.Components
{
	public static partial class Rocket
	{
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent
		{
			public float mass = 1.00f;
			public float force = default;
			public float fuel_time = 1.00f;
			public float smoke_amount = 1.00f;
			public Vector2 velocity;

			[Net.Ignore, Save.Ignore] public float smoke_accumulator = 0.00f;

			public Data()
			{

			}
		}


		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateBody(ISystem.Info info, Entity entity, [Source.Owned] ref Rocket.Data rocket, [Source.Owned] ref Body.Data body, [Source.Owned] ref Transform.Data transform)
		{
			if (rocket.fuel_time > 0.00f)
			{
				var dir = transform.GetDirection();
				body.AddForce(dir * (rocket.force));
			}

			rocket.fuel_time = MathF.Max(rocket.fuel_time - App.fixed_update_interval_s, 0.00f);
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateProjectile(ISystem.Info info, Entity entity, [Source.Owned] ref Rocket.Data rocket, [Source.Owned] ref Projectile.Data projectile, [Source.Owned] ref Transform.Data transform)
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
			}

			rocket.fuel_time = MathF.Max(rocket.fuel_time - App.fixed_update_interval_s, 0.00f);
		}

#if CLIENT
		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateSmokeBody(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity, [Source.Owned] ref Rocket.Data rocket, [Source.Owned] in Body.Data body, [Source.Owned] in Transform.Data transform)
		{
			if (rocket.fuel_time > 0.00f)
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

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateSmokeProjectile(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity, [Source.Owned] ref Rocket.Data rocket, [Source.Owned] in Projectile.Data projectile, [Source.Owned] in Transform.Data transform)
		{
			if (rocket.fuel_time > 0.00f && projectile.elapsed >= 0.15f)
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

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateLight(ISystem.Info info, Entity entity, [Source.Owned] in Rocket.Data rocket, [Source.Owned, Pair.Of<Rocket.Data>] ref Light.Data light)
		{
			var modifier = Maths.Clamp(rocket.fuel_time * 1.50f, 0.00f, 1.00f);
			light.intensity = modifier;
		}
#endif
	}
}
