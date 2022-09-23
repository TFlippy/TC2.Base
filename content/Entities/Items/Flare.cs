
using Keg.Extensions;

namespace TC2.Base.Components
{
	public static partial class Flare
	{
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";
		public static readonly Texture.Handle tex_light = "light_invsqr";
		public static readonly Texture.Handle texture_metal_spark = "Metal_Spark";

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent
		{
			public float lifetime = 8.00f;
			public float smoke_amount = 1.00f;
			public float sparks_amount = 1.00f;

			public float flicker = 0.80f;
			public float flicker_interval = 0.01f;
			public float lerp = 0.10f;
			public float size = 512.00f;

			[Net.Ignore, Save.Ignore] public float intensity_target;
			[Net.Ignore, Save.Ignore] public float next_flicker_time;

			[Net.Ignore, Save.Ignore] public float smoke_accumulator = 0.00f;
			[Net.Ignore, Save.Ignore] public float sparks_accumulator = 0.00f;

			public Data()
			{

			}
		}

#if CLIENT
		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateSmoke(ISystem.Info info, Entity entity, [Source.Owned] ref Flare.Data flare, [Source.Owned, Pair.Of<Flare.Data>] ref Light.Data light, [Source.Owned] in Projectile.Data projectile, [Source.Owned] in Transform.Data transform)
		{
			if (flare.lifetime > 0.00f)
			{
				var modifier = Maths.Clamp(flare.lifetime * 0.10f, 0.00f, 1.00f);

				var random = XorRandom.New();
				ref var region = ref info.GetRegion();

				if (flare.smoke_amount > 0.00f)
				{
					flare.smoke_accumulator += flare.smoke_amount * modifier;
					while (flare.smoke_accumulator >= 1.00f)
					{
						Particle.Spawn(ref region, new Particle.Data()
						{
							texture = texture_smoke,
							pos = transform.position + random.NextUnitVector2Range(0.00f, projectile.size),
							lifetime = random.NextFloatRange(5.00f, 20.00f),
							fps = random.NextByteRange(1, 3),
							frame_count = 64,
							frame_count_total = 64,
							frame_offset = random.NextByteRange(0, 64),
							scale = random.NextFloatRange(0.20f, 0.40f) * modifier,
							angular_velocity = random.NextFloatRange(-0.50f, 0.50f),
							vel = random.NextUnitVector2Range(5.00f, 10.00f),
							force = new Vector2(0, -random.NextFloatRange(-4.00f, 5.00f)) + random.NextUnitVector2Range(2.00f, 4.00f),
							rotation = random.NextFloat(10.00f),
							growth = random.NextFloatRange(0.30f, 0.80f),
							color_a = new Color32BGRA(150, 220, 220, 220).WithAlphaMult(0.50f * modifier),
							color_b = new Color32BGRA(000, 150, 150, 150),
							drag = random.NextFloatRange(0.05f, 0.15f)
						});

						flare.smoke_accumulator -= 1.00f;
					}
				}

				if (flare.sparks_amount > 0.00f)
				{
					flare.sparks_accumulator += flare.sparks_amount * modifier;
					while (flare.sparks_accumulator >= 1.00f)
					{
						Particle.Spawn(ref region, new Particle.Data()
						{
							texture = texture_metal_spark,
							lifetime = random.NextFloatRange(2.00f, 3.00f) * modifier,
							pos = transform.position + random.NextVector2(0.25f),
							vel = random.NextUnitVector2Range(5, 10) + new Vector2(random.NextFloatRange(-4.00f, 4.00f), random.NextFloatRange(10.00f, 20.00f)),
							fps = 1,
							frame_count = 1,
							frame_offset = random.NextByteRange(0, 4),
							frame_count_total = 4,
							scale = random.NextFloatRange(0.70f, 0.90f),
							growth = -random.NextFloatRange(0.10f, 0.20f),
							force = new Vector2(0.00f, random.NextFloatRange(5.00f, 30.00f)),
							stretch = new Vector2(random.NextFloatRange(3.00f, 8.00f), random.NextFloatRange(0.50f, 0.75f)),
							drag = random.NextFloatRange(0.00f, 0.03f),
							lit = 1.00f,
							color_a = random.NextColor32Range(0xffffffff, 0xffffc0b0),
							color_b = random.NextColor32Range(0x00ffffff, 0x00ffc0b0),
							face_dir_ratio = 1.00f
						});

						flare.sparks_accumulator -= 1.00f;
					}
				}

				for (var i = 0; i < 1; i++)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = tex_light,
						lifetime = random.NextFloatRange(0.05f, 0.10f),
						pos = transform.position,
						vel = random.NextUnitVector2Range(10.00f, 30.00f),
						force = Region.gravity,
						scale = random.NextFloatRange(40.00f, 80.00f) * modifier,
						rotation = random.NextFloatRange(-3.50f, 3.50f),
						angular_velocity = random.NextFloat(0.50f),
						growth = random.NextFloatRange(30.00f, 100.00f),
						drag = random.NextFloatRange(0.07f, 0.25f),
						color_a = ColorBGRA.Lerp(light.color, ColorBGRA.White, random.NextFloatRange(0.00f, 0.50f)),
						color_b = light.color * 0.50f,
						glow = random.NextFloatRange(4.00f, 8.00f) * modifier,
					});
				}

				flare.lifetime -= info.DeltaTime;
			}
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateLight(ISystem.Info info, [Source.Owned] ref Flare.Data flare, [Source.Owned, Pair.Of<Flare.Data>] ref Light.Data light)
		{
			var modifier = Maths.Clamp(flare.lifetime * 0.25f, 0.00f, 1.00f);

			if (info.WorldTime >= flare.next_flicker_time)
			{
				var random = XorRandom.New();

				flare.intensity_target = 1.00f - random.NextFloatRange(0.00f, flare.flicker);
				flare.next_flicker_time = info.WorldTime + flare.flicker_interval;
			}

			light.intensity = Maths.Lerp(light.intensity, flare.intensity_target * modifier, flare.lerp);
			light.scale = new Vector2(flare.size * modifier);
		}
#endif
	}
}
