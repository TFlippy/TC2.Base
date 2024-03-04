
using System.Runtime.Intrinsics;

namespace TC2.Base.Components
{
	public static partial class Air
	{
		public readonly struct Blob
		{
			public readonly Air.Composition air;

			public readonly Temperature temperature;
			public readonly Mass mass;
			//public readonly Volume volume;
			public readonly Amount moles_total;

			public Blob(Composition air, Temperature temperature)
			{
				this.air = air;
				this.temperature = temperature;

				this.moles_total = air.GetTotalMoles();
				this.mass = air.GetMass();
			}

			public readonly Volume GetVolume(Pressure pressure)
			{
				Phys.IdealGasLaw(pressure, out var volume, this.moles_total, this.temperature);
				return volume;
			}

			public readonly Density GetDensity(Volume volume)
			{
				return this.mass / volume;
			}
		}

		public struct Composition
		{
			//public static readonly Air.Composition Ambient = new Air.Composition
			//(
			//	moles_o2:  Phys.air_o2_ratio
			//);

			// more common gases - operating with lower half of vector registers is faster
			public Amount moles_n2;
			public Amount moles_o2;
			public Amount moles_co2;
			public Amount moles_h2o;

			// less common gases
			public Amount moles_h2;
			public Amount moles_co;
			public Amount moles_so2;
			public Amount moles_no2;

			public Composition(Amount moles_o2, Amount moles_h2, Amount moles_n2, Amount moles_co, Amount moles_co2, Amount moles_so2, Amount moles_no2, Amount moles_h2o)
			{
				this.moles_o2 = moles_o2;
				this.moles_h2 = moles_h2;
				this.moles_n2 = moles_n2;
				this.moles_co = moles_co;
				this.moles_co2 = moles_co2;
				this.moles_so2 = moles_so2;
				this.moles_no2 = moles_no2;
				this.moles_h2o = moles_h2o;
			}

			// TODO: vectorize this
			public readonly float GetTotalMoles()
			{
				var moles_total =
				this.moles_o2 +
				this.moles_h2 +
				this.moles_n2 +
				this.moles_co +
				this.moles_co2 +
				this.moles_so2 +
				this.moles_no2 +
				this.moles_h2o;

				return moles_total;
			}

			// TODO: vectorize this
			public readonly Mass GetMass()
			{
				var mass_total =
				(this.moles_o2 * Phys.o2_molar_mass) +
				(this.moles_h2 * Phys.h2_molar_mass) +
				(this.moles_n2 * Phys.n2_molar_mass) +
				(this.moles_co * Phys.co_molar_mass) +
				(this.moles_co2 * Phys.co2_molar_mass) +
				(this.moles_so2 * Phys.so2_molar_mass) +
				(this.moles_no2 * Phys.no2_molar_mass) +
				(this.moles_h2o * Phys.h2o_molar_mass);
				return mass_total.m_value;
			}

			// TODO: vectorize this
			public readonly Energy GetHeatCapacity()
			{
				var heat_capacity =
				(this.moles_o2 * Phys.o2_molar_heat_capacity) +
				(this.moles_h2 * Phys.h2_molar_heat_capacity) +
				(this.moles_n2 * Phys.n2_molar_heat_capacity) +
				(this.moles_co * Phys.co_molar_heat_capacity) +
				(this.moles_co2 * Phys.co2_molar_heat_capacity) +
				(this.moles_so2 * Phys.so2_molar_heat_capacity) +
				(this.moles_no2 * Phys.no2_molar_heat_capacity) +
				(this.moles_h2o * Phys.h2o_molar_heat_capacity);
				return heat_capacity;
			}

			public readonly Volume GetVolume(Temperature temperature, Pressure pressure)
			{
				Phys.IdealGasLaw(pressure, out var volume, this.GetTotalMoles(), temperature);
				return volume;
			}

			public readonly Density GetDensity(Volume volume)
			{
				return this.GetMass() / volume;
			}

			// TODO: vectorize this
			public static Air.Composition operator *(Air.Composition air, float value)
			{
				air.moles_o2 *= value;
				air.moles_h2 *= value;
				air.moles_n2 *= value;
				air.moles_co *= value;
				air.moles_co2 *= value;
				air.moles_so2 *= value;
				air.moles_no2 *= value;
				air.moles_h2o *= value;
				
				return air;
			}

			// TODO: vectorize this
			public static Air.Composition operator +(Air.Composition a, Air.Composition b)
			{
				a.moles_o2 += b.moles_o2;
				a.moles_h2 += b.moles_h2;
				a.moles_n2 += b.moles_n2;
				a.moles_co += b.moles_co;
				a.moles_co2 += b.moles_co2;
				a.moles_so2 += b.moles_so2;
				a.moles_no2 += b.moles_no2;
				a.moles_h2o += b.moles_h2o;

				return a;
			}

			// TODO: vectorize this
			public static Air.Composition operator -(Air.Composition a, Air.Composition b)
			{
				a.moles_o2 -= b.moles_o2;
				a.moles_h2 -= b.moles_h2;
				a.moles_n2 -= b.moles_n2;
				a.moles_co -= b.moles_co;
				a.moles_co2 -= b.moles_co2;
				a.moles_so2 -= b.moles_so2;
				a.moles_no2 -= b.moles_no2;
				a.moles_h2o -= b.moles_h2o;

				return a;
			}
		}

		public static partial class Container
		{
			[IComponent.Data(Net.SendType.Unreliable, region_only: true), ITrait.Data(Net.SendType.Unreliable, region_only: true)]
			public struct Data: IComponent, ITrait
			{
				[Flags]
				public enum Flags: uint
				{
					None = 0u,
				}

				public Air.Composition air;

				public Volume volume = 1.00f.m3();

				public Air.Container.Data.Flags flags;

				public Amount moles_total_cached;
				public Pressure pressure_cached;
				public Density density_cached;
				public Mass mass_cached;
				public Temperature temperature_cached = Temperature.Ambient;

				public Data()
				{

				}

				//public readonly Pressure GetPressure()
				//{
				//	Phys.IdealGasLaw(out var pressure, this.volume, this.air.GetTotalMoles(), this.temperature);
				//	return pressure;
				//}
			}
		}

		//public static void TakeAmbient(ref readonly Region.Data region, Volume volume, out Air.Blob blob)
		//{
		//	var density = region.GetAirDensityAtAltitude(altitude);

		//	var air = new Air.Composition();
		//	air.moles_n2 =
		//}

		//public static void TakeAmbient(ref readonly Region.Data region, Volume volume, float altitude, out Air.Blob blob)
		//{
		//	var density = region.GetAirDensityAtAltitude(altitude);

		//	var air = new Air.Composition();
		//	air.moles_n2 = 
		//}
	}

	public static partial class Vent
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true), ITrait.Data(Net.SendType.Reliable, region_only: true)]
		public struct Data: IComponent, ITrait
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0u,

				Has_Pipe = 1u << 0,

				[Save.Ignore] Has_Pressure_Cached = 1u << 31,
			}

			public Air.Blob blob;

			public Inventory.Type type;
			public Vent.Data.Flags flags;

			[Editor.Picker.Position(true, true)]
			public Vector2 offset;
			[Editor.Picker.Direction(true, true), Obsolete]
			public Vector2 direction = new(0, -1);

			public Area cross_section = Area.Circle(10.00f.cm());

			public Pressure pressure_outside = Phys.atmospheric_pressure_kordel;
			public Pressure pressure_inside = Phys.atmospheric_pressure_kordel;

			public float rotation;
			public float velocity;
			public float modifier = 1.00f;
			public Volume flow_rate;

			[Save.Ignore, Net.Ignore] public float t_next_smoke;

			public Data()
			{

			}
		}

		public static Volume GetFlowRate(ref readonly this Vent.Data vent)
		{
			Phys.VolumetricFlowRate(vent.cross_section * vent.modifier, vent.velocity, out var flow_rate);
			return flow_rate;
		}

		public static float GetFlowVelocity(ref readonly this Vent.Data vent)
		{
			Phys.VolumetricFlowRate(vent.cross_section, out var velocity, vent.flow_rate);
			return velocity;
		}
	}

	public static partial class Pipe
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public struct Data: IComponent, ILink
		{
			public readonly Entity EntityA => this.a.entity;
			public readonly Entity EntityB => this.b.entity;
			public readonly ulong ComponentA => this.a.ComponentID;
			public readonly ulong ComponentB => this.b.ComponentID;

			public float scale_multiplier = 1.00f;
			public float length;
			public Area cross_section = Area.Circle(10.00f.cm());

			public EntRef<Vent.Data> a;
			public EntRef<Vent.Data> b;

			public Data()
			{

			}
		}

		//[IRelation.Data(Net.SendType.Reliable, IRelation.Flags.Acyclic | IRelation.Flags.Reflexive | IRelation.Flags.Tag)]
		//public struct Link: IRelation
		//{

		//}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public struct State: IComponent
		{
			public Pressure pressure;
			public Volume volume;
			public Volume flow;

			[Net.Ignore, Save.Ignore] public float next_tick;
		}

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.20f)]
		public static void OnLateUpdate(ISystem.Info info, Entity entity, ref XorRandom random, [Source.Owned] in Pipe.Data pipe, [Source.Owned] ref Pipe.State pipe_state, [Source.Owned] in Transform.Data transform)
		{
			var ts = Timestamp.Now();

			if (info.WorldTime >= pipe_state.next_tick)
			{
				var sync = false;

				if (sync)
				{
					//entity.SyncComponent(ref pipe_state);
					pipe_state.Sync(entity);
				}
			}
		}
#endif

#if CLIENT
		[ISystem.PreRender(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateResizable(ISystem.Info info, Entity entity, [Source.Owned] ref Pipe.Data pipe, [Source.Owned] ref Pipe.State pipe_state, [Source.Owned] ref Resizable.Data resizable, [Source.Owned] ref Transform.Data transform)
		{
			if (resizable.flags.HasAny(Resizable.Flags.Dynamic) && pipe.a.TryGetHandle(out var vent_a) && pipe.b.TryGetHandle(out var vent_b))
			{
				ref var transform_a = ref pipe.a.entity.GetComponent<Transform.Data>();
				ref var transform_b = ref pipe.b.entity.GetComponent<Transform.Data>();

				if (transform_a.IsNotNull() && transform_b.IsNotNull())
				{
					var pos_a = transform_a.LocalToWorld(vent_a.data.offset);
					var pos_b = transform_b.LocalToWorld(vent_b.data.offset);

					resizable.a = transform.WorldToLocal(pos_a);
					resizable.b = transform.WorldToLocal(pos_b);

					//App.WriteLine("test");

					//resizable.cap_a_rotation = transform_a.LocalToWorldRotation(MathF.PI);
					//resizable.cap_b_rotation = transform_b.LocalToWorldRotation(0.00f);
					//entity.GetRegion().DrawDebugText(transform.position, $"{vent_a.data.direction}; {vent_a.data.direction.GetAngleRadians()}", Color32BGRA.White);


					resizable.cap_a_rotation = transform_a.LocalToWorldDirection(vent_a.data.direction).GetAngleRadiansFast(); // transform_a.LocalToWorldDirection(vent_a.data.direction).GetAngleRadiansFast(); //.LocalToWorldRotation(MathF.PI);
					resizable.cap_b_rotation = transform_b.LocalToWorldDirection(vent_b.data.direction).GetAngleRadiansFast();
				}
			}
		}

		[ISystem.PreRender(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateRenderer(ISystem.Info info, Entity entity, [Source.Owned] ref Pipe.Data pipe, [Source.Owned] ref Pipe.State pipe_state, [Source.Owned] ref Rope.Renderer.Data renderer, [Source.Owned] ref Transform.Data transform)
		{
			if (pipe.a.TryGetHandle(out var vent_a) && pipe.b.TryGetHandle(out var vent_b))
			{
				ref var transform_a = ref pipe.a.entity.GetComponent<Transform.Data>();
				ref var transform_b = ref pipe.b.entity.GetComponent<Transform.Data>();

				if (transform_a.IsNotNull() && transform_b.IsNotNull())
				{
					var pos_a = transform_a.LocalToWorld(vent_a.data.offset);
					var pos_b = transform_b.LocalToWorld(vent_b.data.offset);

					var delta = (pos_b - pos_a);
					var dir = delta.GetNormalized(out var len);

					//var rect = new AABB(pos_a, pos_b);

					//App.WriteLine("test");

					renderer.p0 = pos_a + transform_a.LocalToWorldDirection(vent_a.data.direction * 0.250f);
					renderer.p3 = pos_b + transform_b.LocalToWorldDirection(vent_b.data.direction * 0.250f);

					//renderer.p1 = pos_a + (new Vector2(pos_b.X - pos_a.X, 0.00f));
					//renderer.p2 = renderer.p1 + new Vector2(0.00f, 0.00f);

					renderer.p1 = pos_a + transform_a.LocalToWorldDirection(vent_a.data.direction * 2.50f);
					renderer.p2 = pos_b + transform_b.LocalToWorldDirection(vent_b.data.direction * 2.50f);

					//renderer.p1 = new Vector2(pos_a.X, pos_b.Y);
					//renderer.p2 = new Vector2(pos_b.X, pos_a.Y);

					//var a = Vector2.Lerp(renderer.p0, renderer.p3, 0.10f);
					//var b = Vector2.Lerp(renderer.p3, renderer.p0, 0.10f);

					//var dir = renderer.p0 - renderer.p3;
					//dir = dir.GetNormalized(out var len);

					//var (sin, cos) = MathF.SinCos(App.GetCurrentTime());

					//var len = (Vector2.Distance(renderer.p0, renderer.p1) + Vector2.Distance(renderer.p2, renderer.p3)) * 0.50f;

					//App.WriteLine("test");

					//if (info.WorldTime >= pipe_state.next_tick)
					//{
					//	pipe_state.next_tick = info.WorldTime + (pipe.interval * 0.10f);
					//	pipe_state.load -= pipe_state.load * 1.60f; // Maths.Lerp(pipe_state.load, 0.00f, 0.30f);
					//}

					//a += dir * load;
					//b += dir * load;

					//renderer.p1 = Vector2.Lerp(renderer.p1, a, 0.40f);
					//renderer.p2 = Vector2.Lerp(renderer.p2, b, 0.40f);

					//var sag = Maths.Max(0.00f, (len - 2.00f) * 0.10f);

					//renderer.p1 = Vector2.Lerp(renderer.p1, renderer.p2, 0.20f);
					//renderer.p2 = Vector2.Lerp(renderer.p2, renderer.p1, 0.20f);

					//renderer.p1 += new Vector2(0, sag * 4);
					//renderer.p2 += new Vector2(0, sag * 2); // + (dir * load);

					//renderer.z = (entity.id & ECS.id_mask_entity) * 0.0005f;

					renderer.scale = len * pipe.scale_multiplier;
				}
			}
		}
#endif


#if SERVER
		[ISystem.Modified<Pipe.Data>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnModified(ISystem.Info info, Entity entity, [Source.Owned] ref Pipe.Data pipe, [Source.Owned] ref Pipe.State pipe_state, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Resizable.Data resizable)
		{
			if (pipe.a.TryGetHandle(out var vent_a) && pipe.b.TryGetHandle(out var vent_b))
			{
				vent_a.data.flags.AddFlag(Vent.Data.Flags.Has_Pipe);
				vent_b.data.flags.AddFlag(Vent.Data.Flags.Has_Pipe);

				ref var transform_a = ref pipe.a.entity.GetComponent<Transform.Data>();
				ref var transform_b = ref pipe.b.entity.GetComponent<Transform.Data>();

				if (transform_a.IsNotNull() && transform_b.IsNotNull())
				{
					var pos_a = transform_a.LocalToWorld(vent_a.data.offset);
					var pos_b = transform_b.LocalToWorld(vent_b.data.offset);

					transform.position = pos_a; // (pos_a + pos_b) * 0.50f;
					transform.Sync(entity);

					resizable.a = transform.WorldToLocal(pos_a);
					resizable.b = transform.WorldToLocal(pos_b);
					resizable.Modified(entity, true);

					//var shape_ptr = entity.GetTraitUnsafe<Body.Data, Shape.Line>();
					//if (shape_ptr != null)
					//{
					//	shape_ptr->a = transform.WorldToLocal(pos_a);
					//	shape_ptr->b = transform.WorldToLocal(pos_b);

					//	entity.MarkModified<Body.Data, Shape.Line>();

					//	entity.SyncTrait<Body.Data, Shape.Line>(shape_ptr);
					//}
				}

				vent_a.Sync();
				vent_b.Sync();
			}
		}

		[ISystem.Remove(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnRemove(ISystem.Info info, Entity entity, [Source.Owned] ref Pipe.Data pipe, [Source.Owned] ref Pipe.State pipe_state)
		{
			if (pipe.a.TryGetHandle(out var vent_a))
			{
				vent_a.data.flags.RemoveFlag(Vent.Data.Flags.Has_Pipe);
				vent_a.Sync();
			}

			if (pipe.b.TryGetHandle(out var vent_b))
			{
				vent_b.data.flags.RemoveFlag(Vent.Data.Flags.Has_Pipe);
				vent_b.Sync();
			}
		}
#endif
	}

}
