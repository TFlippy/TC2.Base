
namespace TC2.Base.Components
{
	public static partial class Essence
	{
		public static partial class Beam
		{
			[Flags]
			public enum Flags: byte
			{
				None = 0,


			}

			[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Global | IComponent.Scope.Region)]
			public partial struct Data(): IComponent
			{
				public IEssence.Handle h_essence;
				public Essence.Beam.Flags flags;
				public byte unused_00;
				public uint unused_01;

				public float intensity;
				public float curl;
				public float amount;
				[Editor.Slider.Clamped(min: 0.00f, max: 16.00f, snap: 0.001f)]
				public float radius;

				[Save.Force, Editor.Picker.Direction(normalize: true)] public Vec2f direction;
				public float unused_02;
				public float unused_03;
			}

			[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdate_A(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity entity,
			[Source.Owned] ref Transform.Data transform,
			[Source.Owned] ref Essence.Beam.Data beam)
			{

			}

			[ISystem.Update.E(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdate_Physics(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity entity,
			[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body,
			[Source.Owned] ref Essence.Beam.Data beam)
			{
				//body.arb
			}

#if CLIENT
			[ISystem.PostUpdate.A(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnPostUpdate_A(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity entity,
			[Source.Owned] in Transform.Data transform,
			[Source.Owned] ref Essence.Beam.Data beam)
			{
				var h_essence = beam.h_essence;
				if (h_essence)
				{
					var intensity = beam.intensity;
					if (intensity > 0.10f)
					{
						//App.WriteValue(intensity);

						// new IEssence.Handle("motion");
						ref var essence_data = ref h_essence.GetData();
						if (essence_data.IsNotNull())
						{
							var pos = transform.position;
							var dir = transform.LocalToWorldDirection(beam.direction);

							var color_a = ColorBGRA.Lerp(essence_data.color_emit, ColorBGRA.White, 0.50f);
							var color_b = essence_data.color_emit.WithColorMult(0.20f).WithAlphaMult(0.00f);

							//App.WriteLine("essence");

							//Sound.Play(region: ref region, sound: essence_emitter.h_sound_emit, world_position: pos, volume: 1.00f, pitch: 1.00f, size: 0.35f, dist_multiplier: 0.65f);
							//Sound.Play(region: ref region, h_soundmix: essence_emitter.h_soundmix_pulse, random: ref random, pos: pos,
							//	volume: Maths.Lerp01(0.50f, 1.50f, intensity),
							//	pitch: Maths.Lerp01(0.72f, 1.05f, intensity)); //, volume: 1.00f, pitch: 1.00f, size: 0.35f, dist_multiplier: 0.65f);

							//Sound.Play(region: ref region, world_position: pos, sound: essence_data.sound_impulse,
							//	volume: Maths.Lerp01(0.50f, 1.50f, intensity),
							//	pitch: Maths.Lerp01(1.18f, 0.95f, intensity)); //, volume: 1.00f, pitch: 1.00f, size: 0.35f, dist_multiplier: 0.65f);
							//if (intensity > 0.30f) Shake.Emit(region: ref region, world_position: pos, trauma: 0.45f * intensity, max: 0.50f, radius: 10.00f);

							//if (random.NextBool(0.40f))
							{
								Particle.Spawn(ref region, new Particle.Data()
								{
									texture = Light.tex_light_circle_00,
									lifetime = 0.20f,
									pos = pos + dir,
									vel = dir * beam.unused_02,
									drag = 0.0020f,
									frame_count = 1,
									frame_count_total = 1,
									frame_offset = 0,
									scale = beam.radius * 40,
									stretch = new Vector2(2.00f, 0.50f),
									face_dir_ratio = 1.00f,
									growth = 10.00f,
									color_a = color_a,
									color_b = color_b,
									force = dir.GetPerpendicular() * beam.curl * 100, //.RotateByRad(beam.curl) * beam.unused_02 * 1000,
									glow = 20.00f * intensity
								});
							}

							//Particle.Spawn(ref region, new Particle.Data()
							//{
							//	texture = Light.tex_light_circle_04,
							//	lifetime = random.NextFloatRange(1.00f, 1.25f),
							//	pos = data.world_position + (dir * 0.50f),
							//	scale = random.NextFloatRange(1.00f, 1.50f),
							//	growth = random.NextFloatRange(1.50f, 2.00f),
							//	stretch = new Vector2(0.90f, 0.60f),
							//	rotation = dir.GetAngleRadiansFast(),
							//	face_dir_ratio = 1.00f,
							//	color_a = new Vector4(1.00f, 0.70f, 0.40f, 30.00f),
							//	color_b = new Vector4(0.20f, 0.00f, 0.00f, 0.00f),
							//	glow = 1.00f
							//});

						}
					}
				}
			}
#endif
		}









	}
}
