
namespace TC2.Base.Components
{
	public static partial class Overheat
	{
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Overheated = 1 << 0
		}

		[IComponent.Data(Net.SendType.Unreliable, name: "Overheating")]
		public partial struct Data: IComponent
		{
			public float heat_current = 0.00f;
			public float heat_medium = 50.00f;
			public float heat_high = 100.00f;

			[Statistics.Info("Maximum Temperature", description: "Maximum operating temperature.", format: "{0:0.##}°C", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float heat_critical = 200.00f;

			[Statistics.Info("Cooling Rate", description: "Cooling rate.", format: "{0:0.##}°C/s", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float cool_rate = 10.00f;

			[Statistics.Info("Heat Capacity (Extra)", description: "Extra heat capacity.", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float capacity_extra = 0.00f;

			public Overheat.Flags flags;

			[Editor.Picker.Position(true)]
			public Vector2 offset;

			public Vector2 size = new Vector2(0.50f, 0.25f);

			[Net.Ignore, Save.Ignore] public float next_steam;

			public Data()
			{

			}
		}

		public static void AddHeat(ref this Overheat.Data overheat, float amount, float mass)
		{
			var heat = amount / MathF.Max(overheat.capacity_extra + (mass * 0.10f), 1.00f);
			overheat.heat_current += heat;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, interval: 0.10f)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region,
		[Source.Owned] ref Overheat.Data overheat, [Source.Owned] ref Control.Data control, [Source.Owned] in Body.Data body)
		{
			var ambient_temperature = Maths.KelvinToCelsius(Region.ambient_temperature);

			overheat.heat_current = Maths.MoveTowards(overheat.heat_current, ambient_temperature, overheat.cool_rate * info.DeltaTime);

			if (overheat.heat_current >= overheat.heat_critical)
			{
				overheat.flags |= Overheat.Flags.Overheated;
			}

			if (overheat.flags.HasAll(Overheat.Flags.Overheated))
			{
				if (overheat.heat_current <= overheat.heat_medium)
				{
					overheat.flags &= ~Overheat.Flags.Overheated;
				}
			}

//#if SERVER
//			if (info.WorldTime >= overheat.next_sync && overheat.heat_current > ambient_temperature)
//			{
//				overheat.Sync(entity);
//				overheat.next_sync = info.WorldTime + 3.00f;
//			}
//#endif
		}

#if CLIENT
		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateLight(ISystem.Info info, Entity entity,
		[Source.Owned] in Overheat.Data overheat, [Source.Owned, Pair.Of<Overheat.Data>] ref Light.Data light)
		{
			//light.offset = overheat.offset;
			light.intensity = MathF.Max(overheat.heat_current - 150.00f, 0.00f) / 250.00f;
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateSound(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity, [Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Overheat.Data overheat, [Source.Owned, Pair.Of<Overheat.Data>] ref Sound.Emitter sound_emitter)
		{
			sound_emitter.offset = overheat.offset;
			sound_emitter.volume = Maths.Clamp(MathF.Max(overheat.heat_current - overheat.heat_medium, 0.00f) / 4000.00f, 0.00f, 0.20f);
			sound_emitter.pitch = 0.50f + Maths.Clamp(MathF.Max(overheat.heat_current - overheat.heat_medium, 0.00f) / 4000.00f, 0.00f, 0.40f);

			if (overheat.heat_current >= 100.00f && info.WorldTime >= overheat.next_steam)
			{
				var dir = transform.GetDirection();
				var intensity = Maths.Clamp(MathF.Max(overheat.heat_current - overheat.heat_medium, 0.00f) / 50.00f, 0.00f, 0.30f);

				overheat.next_steam = info.WorldTime + 0.04f;
				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = texture_smoke,
					lifetime = random.NextFloatRange(0.50f, 1.00f),
					pos = transform.LocalToWorld(overheat.offset + (new Vector2(random.NextFloatRange(-overheat.size.X, overheat.size.X), random.NextFloatRange(-overheat.size.Y, overheat.size.Y)) * 0.50f)), // + (dir * random.NextFloatRange(0.00f, 1.00f))),
					vel = new Vector2(random.NextFloatRange(-intensity, intensity), random.NextFloatRange(0.00f, -intensity * 10)), // (-dir * random.NextFloatRange(3.00f, 6.00f) * intensity) + new Vector2(0, -2),
					force = new Vector2(0.10f, -0.40f),
					fps = random.NextByteRange(15, 20),
					frame_count = 64,
					frame_count_total = 64,
					frame_offset = random.NextByteRange(0, 64),
					scale = random.NextFloatRange(0.30f, 0.50f),
					rotation = random.NextFloat(10.00f),
					angular_velocity = random.NextFloatRange(1.00f, 3.00f),
					growth = 0.40f,
					color_a = new Color32BGRA(0xc0ffffff).WithAlphaMult(intensity),
					color_b = new Color32BGRA(0x00ffffff),
				});
			}
		}
#endif
	}
}
