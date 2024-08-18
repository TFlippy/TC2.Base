
namespace TC2.Base.Components
{
	public static partial class Heat
	{
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[IComponent.Data(Net.SendType.Unreliable, name: "Heating", region_only: true), IComponent.With<Heat.State>()]
		public partial struct Data: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Exclude_Body_Mass = 1u << 0,
				No_Smoke = 1u << 1,
			}

			public Temperature temperature_medium = Temperature.Celsius(150.00f);
			public Temperature temperature_high = Temperature.Celsius(525.00f);

			[Statistics.Info("Temperature (Max)", description: "Maximum operating temperature.", format: "{0:0.##} K", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public Temperature temperature_critical = Temperature.Celsius(620.00f);

			[Statistics.Info("Dissipation", description: "Cooling rate.", format: "{0:0.##} kW", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public Power cool_rate = 1.00f;

			[Statistics.Info("Heat Capacity (Extra)", description: "Extra heat capacity.", format: "{0:0.##} kJ", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public Energy heat_capacity_extra;

			public float heat_capacity_mult = 1.00f;

			public Heat.Data.Flags flags;

			[Editor.Picker.Position(true)]
			public Vector2 offset;

			[Editor.Picker.Position(true)]
			public Vector2 smoke_offset;

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

			public Heat.State.Flags flags;

			[Save.Ignore] public Energy heat_capacity_inv = 1.00f;

			[Net.Ignore, Save.Ignore] public Temperature temperature_ambient = Temperature.Ambient;

			[Net.Ignore, Save.Ignore] public float t_next_ambient;
			[Net.Ignore, Save.Ignore] public float t_next_steam;

			public State()
			{

			}
		}


#if CLIENT
		public struct HeatGUI: IGUICommand
		{
			public Entity ent_heat;

			public Transform.Data transform;

			public Heat.Data heat;
			public Heat.State heat_state;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Heat"u8, this.ent_heat, size: new(48, 96 * 1), min_width: 48))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (GUI.Group.New(size: GUI.Rm))
						{
							GUI.SameLine(24);
							GUI.DrawTemperatureRange(this.heat_state.temperature_current, this.heat.temperature_critical, 2000, size: new(24, GUI.RmY));
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region, order: -100)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Heat.Data heat, [Source.Owned] in Heat.State heat_state)
		{
			if (interactable.IsActive())
			{
				var gui = new HeatGUI()
				{
					ent_heat = entity,

					transform = transform,

					heat = heat,
					heat_state = heat_state
				};
				gui.Submit();
			}
		}
#endif

		public static void AddEnergy(ref this Heat.State heat_state, Energy energy)
		{
			//var heat = amount / Maths.Max(heat_state.heat_capacity_extra + (mass * 0.10f), 1.00f);
			heat_state.temperature_current.m_value.FMA(energy.m_value, heat_state.heat_capacity_inv.m_value);
		}

		public static void AddPower(ref this Heat.State heat_state, Power power, float dt)
		{
			//var heat = amount / Maths.Max(heat_state.heat_capacity_extra + (mass * 0.10f), 1.00f);

			power.m_value *= dt;
			heat_state.temperature_current.m_value.FMA(power.m_value, heat_state.heat_capacity_inv.m_value);
		}

		public const float interval_ambient = 0.23f;

		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnAttach(ISystem.Info info, ref Region.Data region, 
		Entity ent_transform, Entity ent_transform_parent,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned] in Transform.Data transform, [Source.Parent] in Transform.Data transform_parent, 
		[Source.Parent] in Joint.Base joint_base)
		{
			App.WriteLine("add heat attached");
		}

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.273f)]
		public static void OnUpdateAttached(ISystem.Info info, ref Region.Data region,
		Entity ent_transform, Entity ent_transform_parent,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned] in Transform.Data transform, [Source.Parent] in Transform.Data transform_parent,
		[Source.Parent] in Joint.Base joint_base)
		{
			App.WriteLine("update heat attached");
		}

		[ISystem.Modified.Component<Body.Data>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnBodyModified(ISystem.Info info, Entity entity,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned] in Body.Data body)
		{
			if (heat.flags.HasAny(Heat.Data.Flags.Exclude_Body_Mass))
			{
				heat_state.heat_capacity.m_value = Maths.Max(heat.heat_capacity_extra.m_value, 0.10f);
			}
			else
			{
				heat_state.heat_capacity.m_value = Maths.Max(Maths.FMA(body.GetMass().ClampMin(1.00f), heat.heat_capacity_mult, heat.heat_capacity_extra.m_value), 0.10f);
			}

			heat_state.heat_capacity_inv.m_value = 1.00f / heat_state.heat_capacity.m_value;

#if SERVER
			heat.Sync(entity, true);
			heat_state.Sync(entity, true);
#endif
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned] in Transform.Data transform)
		{
			var temperature_ambient = heat_state.temperature_ambient;
			var temperature_current = heat_state.temperature_current;

			var time = info.WorldTime;
			if (time >= heat_state.t_next_ambient)
			{
				heat_state.t_next_ambient = time + interval_ambient;
				heat_state.temperature_ambient = temperature_ambient = region.GetAmbientTemperature(transform.LocalToWorld(heat.offset));
			}

			Phys.TransferHeatAmbientSimpleFast(ref temperature_current, temperature_ambient, heat_state.heat_capacity_inv, heat.cool_rate, info.DeltaTime);
			if (!heat_state.flags.TryAddFlag(Heat.State.Flags.Overheated, temperature_current >= heat.temperature_critical))
			{
				heat_state.flags.RemoveFlag(Heat.State.Flags.Overheated, temperature_current <= heat.temperature_medium);
			}

			heat_state.temperature_current = temperature_current;

			//if (heat.flags.HasAny(Heat.Flags.Heated))
			//{
			//	if (temperature_current <= heat.temperature_medium)
			//	{
			//		heat.flags &= ~Heat.Flags.Heated;
			//	}
			//}

			//#if SERVER
			//			if (info.WorldTime >= heat.next_sync && heat.heat_current > ambient_temperature)
			//			{
			//				heat.Sync(entity);
			//				heat.next_sync = info.WorldTime + 3.00f;
			//			}
			//#endif
		}

#if SERVER
		[ISystem.PostUpdate.F(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.38f)]
		public static void OnUpdate_HeatDamage(ISystem.Info info, Entity entity, ref XorRandom random,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Health.Data health, 
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state, [Source.Owned] ref Body.Data body)
		{
			var heat_excess = Maths.Max(heat_state.temperature_current - heat.temperature_critical, 0.00f);
			if (heat_excess > Maths.epsilon && random.NextBool(heat_excess * 0.01f))
			{
				var damage = Maths.Lerp(heat_excess, health.GetMaxHealth(), 0.35f) * random.NextFloatRange(0.80f, 1.20f);
				Damage.Hit(ent_attacker: entity, ent_owner: entity, ent_target: entity, position: transform.position, velocity: random.NextUnitVector2Range(1, 1), normal: random.NextUnitVector2Range(1, 1), damage_integrity: damage, damage_durability: damage, damage_terrain: damage, target_material_type: body.GetMaterial(), damage_type: Damage.Type.Phlogiston, yield: 0.90f, xp_modifier: 0.00f, impulse: 0.00f, flags: Damage.Flags.No_Loot_Pickup);
			}
		}
#endif

#if CLIENT
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned, Pair.Component<Heat.Data>, Optional(true)] ref Light.Data light,
		[Source.Owned, Pair.Component<Heat.Data>, Optional(true)] ref Sound.Emitter sound_emitter)
		{
			//light.offset = heat.offset;

			var time = info.WorldTime;
			var temperature_current = heat_state.temperature_current;

			if (light.IsNotNull())
			{
				light.color = Temperature.GetGlowColor(temperature_current); // = Maths.Max(heat.temperature_current - 150.00f, 0.00f) / 250.00f;
			}

			if (sound_emitter.IsNotNull())
			{
				//sound_emitter.offset = heat.offset;
				sound_emitter.volume_mult = Maths.Clamp(Maths.Max(temperature_current - heat.temperature_medium, 0.00f) / 4000.00f, 0.00f, 0.20f);
				sound_emitter.pitch_mult = 0.50f + Maths.Clamp(Maths.Max(temperature_current - heat.temperature_medium, 0.00f) / 4000.00f, 0.00f, 0.40f);
			}

			if (heat.flags.HasNone(Heat.Data.Flags.No_Smoke) && temperature_current >= Temperature.Celsius(150.00f) && time >= heat_state.t_next_steam)
			{
				heat_state.t_next_steam = time + 0.04f;

				//var dir = transform.GetDirection();
				var intensity = Maths.Clamp(Maths.Max(temperature_current - heat.temperature_medium, 0.00f) / 50.00f, 0.00f, 0.30f);

				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = texture_smoke,
					lifetime = random.NextFloatRange(0.50f, 1.00f),
					pos = transform.LocalToWorld(heat.smoke_offset + (new Vector2(random.NextFloatRange(-heat.size.X, heat.size.X), random.NextFloatRange(-heat.size.Y, heat.size.Y)) * 0.50f)), // + (dir * random.NextFloatRange(0.00f, 1.00f))),
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
		//[Source.Owned] in Heat.Data heat, [Source.Owned] ref Heat.State heat_state, [Source.Owned, Pair.Component<Heat.Data>] ref Sound.Emitter sound_emitter)
		//{
		//	sound_emitter.offset = heat.offset;
		//	sound_emitter.volume_mult = Maths.Clamp(Maths.Max(heat.temperature_current - heat.temperature_medium, 0.00f) / 4000.00f, 0.00f, 0.20f);
		//	sound_emitter.pitch_mult = 0.50f + Maths.Clamp(Maths.Max(heat.temperature_current - heat.temperature_medium, 0.00f) / 4000.00f, 0.00f, 0.40f);

		//	if (heat.temperature_current >= 100.00f && info.WorldTime >= heat.t_next_steam)
		//	{
		//		var dir = transform.GetDirection();
		//		var intensity = Maths.Clamp(Maths.Max(heat.temperature_current - heat.temperature_medium, 0.00f) / 50.00f, 0.00f, 0.30f);

		//		heat.t_next_steam = info.WorldTime + 0.04f;
		//		Particle.Spawn(ref region, new Particle.Data()
		//		{
		//			texture = texture_smoke,
		//			lifetime = random.NextFloatRange(0.50f, 1.00f),
		//			pos = transform.LocalToWorld(heat.offset + (new Vector2(random.NextFloatRange(-heat.size.X, heat.size.X), random.NextFloatRange(-heat.size.Y, heat.size.Y)) * 0.50f)), // + (dir * random.NextFloatRange(0.00f, 1.00f))),
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
