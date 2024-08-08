
namespace TC2.Base.Components
{
	public static partial class Overheat
	{
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[IComponent.Data(Net.SendType.Unreliable, name: "Overheating", region_only: true), IComponent.With<Overheat.State>()]
		public partial struct Data: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,
			}

			public Temperature temperature_medium = Temperature.Celsius(150.00f);
			public Temperature temperature_high = Temperature.Celsius(525.00f);

			[Statistics.Info("Maximum Temperature", description: "Maximum operating temperature.", format: "{0:0.##} K", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public Temperature temperature_critical = Temperature.Celsius(620.00f);

			[Statistics.Info("Cooling Rate", description: "Cooling rate.", format: "{0:0.##} kW", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public Power cool_rate = 1.00f;

			[Statistics.Info("Heat Capacity (Extra)", description: "Extra heat capacity.", format: "{0:0.##} kJ", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public Energy heat_capacity_extra;

			public float heat_capacity_mult = 1.00f;

			public Overheat.Data.Flags flags;

			[Editor.Picker.Position(true)]
			public Vector2 offset;

			public Vector2 size = new Vector2(0.50f, 0.25f);

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Overheated = 1 << 0
			}

			public Temperature temperature_current = Temperature.Ambient;
			public Energy heat_capacity = 1.00f;

			public Overheat.State.Flags flags;

			[Net.Ignore, Save.Ignore] public Temperature temperature_ambient = Temperature.Ambient;
			[Net.Ignore, Save.Ignore] public Energy heat_capacity_inv = 1.00f;

			[Net.Ignore, Save.Ignore] public float t_next_ambient;
			[Net.Ignore, Save.Ignore] public float t_next_steam;

			public State()
			{

			}
		}

		public static void AddEnergy(ref this Overheat.State overheat_state, Energy energy)
		{
			//var heat = amount / Maths.Max(overheat_state.heat_capacity_extra + (mass * 0.10f), 1.00f);
			overheat_state.temperature_current.m_value.FMA(energy.m_value, overheat_state.heat_capacity_inv.m_value);
		}

		public static void AddPower(ref this Overheat.State overheat_state, Power power, float dt)
		{
			//var heat = amount / Maths.Max(overheat_state.heat_capacity_extra + (mass * 0.10f), 1.00f);

			power.m_value *= dt;
			overheat_state.temperature_current.m_value.FMA(power.m_value, overheat_state.heat_capacity_inv.m_value);
		}

		public const float interval_ambient = 0.23f;

		[ISystem.Modified.Component<Body.Data>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnBodyModified(ISystem.Info info, Entity entity,
		[Source.Owned] ref Overheat.Data overheat, [Source.Owned] ref Overheat.State overheat_state,
		[Source.Owned] in Body.Data body)
		{
			overheat_state.heat_capacity.m_value = Maths.Max(Maths.FMA(body.GetMass(), overheat.heat_capacity_mult, overheat.heat_capacity_extra.m_value), 0.001f);
			overheat_state.heat_capacity_inv.m_value = 1.00f / overheat_state.heat_capacity.m_value;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region,
		[Source.Owned] ref Overheat.Data overheat, [Source.Owned] ref Overheat.State overheat_state,
		[Source.Owned] in Body.Data body)
		{
			var temperature_ambient = overheat_state.temperature_ambient;
			var temperature_current = overheat_state.temperature_current;

			var time = info.WorldTime;
			if (time >= overheat_state.t_next_ambient)
			{
				overheat_state.t_next_ambient = time + interval_ambient;
				overheat_state.temperature_ambient = temperature_ambient = region.GetAmbientTemperature(body.GetPosition());
			}

			Phys.TransferHeatAmbientSimpleFast(ref temperature_current, temperature_ambient, overheat_state.heat_capacity_inv, overheat.cool_rate, info.DeltaTime);
			if (!overheat_state.flags.TryAddFlag(Overheat.State.Flags.Overheated, temperature_current >= overheat.temperature_critical))
			{
				overheat_state.flags.RemoveFlag(Overheat.State.Flags.Overheated, temperature_current <= overheat.temperature_medium);
			}

			overheat_state.temperature_current = temperature_current;

			//if (overheat.flags.HasAny(Overheat.Flags.Overheated))
			//{
			//	if (temperature_current <= overheat.temperature_medium)
			//	{
			//		overheat.flags &= ~Overheat.Flags.Overheated;
			//	}
			//}

			//#if SERVER
			//			if (info.WorldTime >= overheat.next_sync && overheat.heat_current > ambient_temperature)
			//			{
			//				overheat.Sync(entity);
			//				overheat.next_sync = info.WorldTime + 3.00f;
			//			}
			//#endif
		}

#if SERVER
		[ISystem.Update.F(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.38f)]
		public static void OnUpdate_HeatDamage(ISystem.Info info, Entity entity, ref XorRandom random,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Health.Data health, [Source.Owned] ref Overheat.Data overheat, [Source.Owned] ref Body.Data body)
		{
			//var heat_excess = Maths.Max(overheat.temperature_current - overheat.temperature_critical, 0.00f);
			//if (heat_excess > Maths.epsilon && random.NextBool(heat_excess * 0.01f))
			//{
			//	var damage = Maths.Lerp(heat_excess, health.GetMaxHealth(), 0.35f) * random.NextFloatRange(0.80f, 1.20f);
			//	Damage.Hit(ent_attacker: entity, ent_owner: entity, ent_target: entity, position: transform.position, velocity: random.NextUnitVector2Range(1, 1), normal: random.NextUnitVector2Range(1, 1), damage_integrity: damage, damage_durability: damage, damage_terrain: damage, target_material_type: body.GetMaterial(), damage_type: Damage.Type.Fire, yield: 0.00f, xp_modifier: 0.00f, impulse: 0.00f, flags: Damage.Flags.No_Loot_Pickup);
			//}
		}
#endif

#if CLIENT
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Overheat.Data overheat, [Source.Owned] ref Overheat.State overheat_state, 
		[Source.Owned, Pair.Component<Overheat.Data>, Optional(true)] ref Light.Data light,
		[Source.Owned, Pair.Component<Overheat.Data>, Optional(true)] ref Sound.Emitter sound_emitter)
		{
			//light.offset = overheat.offset;

			var time = info.WorldTime;
			var temperature_current = overheat_state.temperature_current;
			
			if (light.IsNotNull())
			{
				light.color = Temperature.GetGlowColor(temperature_current); // = Maths.Max(overheat.temperature_current - 150.00f, 0.00f) / 250.00f;
			}

			if (sound_emitter.IsNotNull())
			{
				sound_emitter.offset = overheat.offset;
				sound_emitter.volume_mult = Maths.Clamp(Maths.Max(temperature_current - overheat.temperature_medium, 0.00f) / 4000.00f, 0.00f, 0.20f);
				sound_emitter.pitch_mult = 0.50f + Maths.Clamp(Maths.Max(temperature_current - overheat.temperature_medium, 0.00f) / 4000.00f, 0.00f, 0.40f);
			}

			if (temperature_current >= 100.00f && time >= overheat_state.t_next_steam)
			{
				overheat_state.t_next_steam = time + 0.04f;

				//var dir = transform.GetDirection();
				var intensity = Maths.Clamp(Maths.Max(temperature_current - overheat.temperature_medium, 0.00f) / 50.00f, 0.00f, 0.30f);

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

		//[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateSound(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity, [Source.Owned] in Transform.Data transform,
		//[Source.Owned] in Overheat.Data overheat, [Source.Owned] ref Overheat.State overheat_state, [Source.Owned, Pair.Component<Overheat.Data>] ref Sound.Emitter sound_emitter)
		//{
		//	sound_emitter.offset = overheat.offset;
		//	sound_emitter.volume_mult = Maths.Clamp(Maths.Max(overheat.temperature_current - overheat.temperature_medium, 0.00f) / 4000.00f, 0.00f, 0.20f);
		//	sound_emitter.pitch_mult = 0.50f + Maths.Clamp(Maths.Max(overheat.temperature_current - overheat.temperature_medium, 0.00f) / 4000.00f, 0.00f, 0.40f);

		//	if (overheat.temperature_current >= 100.00f && info.WorldTime >= overheat.t_next_steam)
		//	{
		//		var dir = transform.GetDirection();
		//		var intensity = Maths.Clamp(Maths.Max(overheat.temperature_current - overheat.temperature_medium, 0.00f) / 50.00f, 0.00f, 0.30f);

		//		overheat.t_next_steam = info.WorldTime + 0.04f;
		//		Particle.Spawn(ref region, new Particle.Data()
		//		{
		//			texture = texture_smoke,
		//			lifetime = random.NextFloatRange(0.50f, 1.00f),
		//			pos = transform.LocalToWorld(overheat.offset + (new Vector2(random.NextFloatRange(-overheat.size.X, overheat.size.X), random.NextFloatRange(-overheat.size.Y, overheat.size.Y)) * 0.50f)), // + (dir * random.NextFloatRange(0.00f, 1.00f))),
		//			vel = new Vector2(random.NextFloatRange(-intensity, intensity), random.NextFloatRange(0.00f, -intensity * 10)), // (-dir * random.NextFloatRange(3.00f, 6.00f) * intensity) + new Vector2(0, -2),
		//			force = new Vector2(0.10f, -0.40f),
		//			fps = random.NextByteRange(15, 20),
		//			frame_count = 64,
		//			frame_count_total = 64,
		//			frame_offset = random.NextByteRange(0, 64),
		//			scale = random.NextFloatRange(0.30f, 0.50f),
		//			rotation = random.NextFloat(10.00f),
		//			angular_velocity = random.NextFloatRange(1.00f, 3.00f),
		//			growth = 0.40f,
		//			color_a = new Color32BGRA(0xc0ffffff).WithAlphaMult(intensity),
		//			color_b = new Color32BGRA(0x00ffffff),
		//		});
		//	}
		//}
#endif
	}
}
