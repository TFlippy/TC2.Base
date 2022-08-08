
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


		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateBody(ISystem.Info info, Entity entity, [Source.Owned] ref Rocket.Data rocket, [Source.Owned] ref Projectile.Data projectile)
		{
			if (rocket.fuel_time > 0.00f)
			{
				var dir = projectile.velocity.GetNormalized();
				projectile.velocity += dir * ((rocket.force / rocket.mass) * App.fixed_update_interval_s);
			}

			rocket.fuel_time = MathF.Max(rocket.fuel_time - App.fixed_update_interval_s, 0.00f);
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateProjectile(ISystem.Info info, Entity entity, [Source.Owned] ref Rocket.Data rocket, [Source.Owned] ref Projectile.Data projectile)
		{
			if (rocket.fuel_time > 0.00f)
			{
				var dir = projectile.velocity.GetNormalized();
				projectile.velocity += dir * ((rocket.force / rocket.mass) * App.fixed_update_interval_s);
			}

			rocket.fuel_time = MathF.Max(rocket.fuel_time - App.fixed_update_interval_s, 0.00f);
		}

#if CLIENT
		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateSmoke(ISystem.Info info, Entity entity, [Source.Owned] ref Rocket.Data rocket, [Source.Owned] in Projectile.Data projectile, [Source.Owned] in Transform.Data transform)
		{
			if (rocket.fuel_time > 0.00f)
			{
				var modifier = Maths.Clamp(rocket.fuel_time, 0.00f, 1.00f);

				var random = XorRandom.New();
				ref var region = ref info.GetRegion();

				rocket.smoke_accumulator += rocket.smoke_amount;

				while (rocket.smoke_accumulator >= 1.00f)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = texture_smoke,
						pos = transform.position,
						lifetime = random.NextFloatRange(5.00f, 10.00f),
						fps = random.NextByteRange(1, 3),
						frame_count = 64,
						frame_count_total = 64,
						frame_offset = random.NextByteRange(0, 64),
						scale = random.NextFloatRange(0.05f, 0.125f) * modifier,
						angular_velocity = random.NextFloatRange(-0.20f, 0.20f),
						vel = -projectile.velocity * 0.25f,
						force = new Vector2(0, -random.NextFloatRange(0.00f, 0.40f)),
						rotation = random.NextFloat(10.00f),
						growth = random.NextFloatRange(0.30f, 0.50f),
						color_a = new Color32BGRA(150, 220, 220, 220).WithAlphaMult(modifier),
						color_b = new Color32BGRA(000, 150, 150, 150),
						drag = 0.15f
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
