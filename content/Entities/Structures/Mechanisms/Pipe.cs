#define USE_SIMD

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
			public readonly Amount moles_total;
			private readonly uint unused;

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

		public struct Particulates
		{
			public Mass ash;
			public Mass soot;
			public Mass dust;
			public Mass unused;

			public Particulates(Mass ash, Mass soot, Mass dust, Mass unused)
			{
				this.ash = ash;
				this.soot = soot;
				this.dust = dust;
				this.unused = unused;
			}

			public readonly Mass GetMass()
			{
				var xmm0 = Vec4f.From(this);
				return xmm0.Sum();
			}

			public static Air.Particulates operator *(Air.Particulates a, float value)
			{
				var xmm0 = Vec4f.From(a);
				xmm0 *= value;
				return xmm0.As<Air.Particulates>();
			}

			public static Air.Particulates operator +(Air.Particulates a, Air.Particulates b)
			{
				var xmm0 = Vec4f.From(a);
				var xmm1 = Vec4f.From(b);
				xmm0 += xmm1;
				return xmm0.As<Air.Particulates>();
			}

			public static Air.Particulates operator -(Air.Particulates a, Air.Particulates b)
			{
				var xmm0 = Vec4f.From(a);
				var xmm1 = Vec4f.From(b);
				xmm0 -= xmm1;
				return xmm0.As<Air.Particulates>();
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

			public Composition(Amount moles_n2, Amount moles_o2, Amount moles_co2, Amount moles_h2o, Amount moles_h2, Amount moles_co, Amount moles_so2, Amount moles_no2)
			{
				this.moles_n2 = moles_n2;
				this.moles_o2 = moles_o2;
				this.moles_co2 = moles_co2;
				this.moles_h2o = moles_h2o;

				this.moles_h2 = moles_h2;
				this.moles_co = moles_co;
				this.moles_so2 = moles_so2;
				this.moles_no2 = moles_no2;
			}

			public readonly float GetTotalMoles()
			{
#if USE_SIMD
				var ymm0 = Vec8f.From(this);
				return ymm0.Sum();
#else
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
#endif
			}

			public readonly Mass GetMass()
			{
#if USE_SIMD
				var ymm0 = Vec8f.From(this);
				var ymm1 = new Vec8f
				(
					Phys.n2_molar_mass,
					Phys.o2_molar_mass,
					Phys.co2_molar_mass,
					Phys.h2o_molar_mass,

					Phys.h2_molar_mass,
					Phys.co_molar_mass,
					Phys.so2_molar_mass,
					Phys.no2_molar_mass
				);
				ymm0 *= ymm1;
				return ymm0.Sum();
#else
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
#endif
			}

			public readonly Energy GetHeatCapacity()
			{
#if USE_SIMD
				var ymm0 = Vec8f.From(this);
				var ymm1 = new Vec8f
				(
					Phys.n2_molar_heat_capacity,
					Phys.o2_molar_heat_capacity,
					Phys.co2_molar_heat_capacity,
					Phys.h2o_molar_heat_capacity,

					Phys.h2_molar_heat_capacity,
					Phys.co_molar_heat_capacity,
					Phys.so2_molar_heat_capacity,
					Phys.no2_molar_heat_capacity
				);
				ymm0 *= ymm1;
				return ymm0.Sum();
#else
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
#endif
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

			public static Air.Composition operator *(Air.Composition air, float value)
			{
#if USE_SIMD
				var ymm0 = Vec8f.From(air);
				ymm0 *= value;
				return ymm0.As<Air.Composition>();
#else
				air.moles_o2 *= value;
				air.moles_h2 *= value;
				air.moles_n2 *= value;
				air.moles_co *= value;
				air.moles_co2 *= value;
				air.moles_so2 *= value;
				air.moles_no2 *= value;
				air.moles_h2o *= value;

				return air;
#endif
			}

			public static Air.Composition operator +(Air.Composition a, Air.Composition b)
			{
#if USE_SIMD
				var ymm0 = Vec8f.From(a);
				var ymm1 = Vec8f.From(b);
				ymm0 += ymm1;
				return ymm0.As<Air.Composition>();
#else
				a.moles_o2 += b.moles_o2;
				a.moles_h2 += b.moles_h2;
				a.moles_n2 += b.moles_n2;
				a.moles_co += b.moles_co;
				a.moles_co2 += b.moles_co2;
				a.moles_so2 += b.moles_so2;
				a.moles_no2 += b.moles_no2;
				a.moles_h2o += b.moles_h2o;

				return a;
#endif
			}

			public static Air.Composition operator -(Air.Composition a, Air.Composition b)
			{
#if USE_SIMD
				var ymm0 = Vec8f.From(a);
				var ymm1 = Vec8f.From(b);
				ymm0 -= ymm1;
				return ymm0.As<Air.Composition>();
#else
				a.moles_o2 -= b.moles_o2;
				a.moles_h2 -= b.moles_h2;
				a.moles_n2 -= b.moles_n2;
				a.moles_co -= b.moles_co;
				a.moles_co2 -= b.moles_co2;
				a.moles_so2 -= b.moles_so2;
				a.moles_no2 -= b.moles_no2;
				a.moles_h2o -= b.moles_h2o;

				return a;
#endif
			}

			public static Air.Composition operator checked -(Air.Composition a, Air.Composition b)
			{
#if USE_SIMD
				var ymm0 = Vec8f.From(a);
				var ymm1 = Vec8f.From(b);
				ymm0 -= ymm1;
				ymm0 = Vec8f.Max(ymm0, Vec8f.Zero);
				return ymm0.As<Air.Composition>();
#else
				a.moles_o2 -= b.moles_o2;
				a.moles_h2 -= b.moles_h2;
				a.moles_n2 -= b.moles_n2;
				a.moles_co -= b.moles_co;
				a.moles_co2 -= b.moles_co2;
				a.moles_so2 -= b.moles_so2;
				a.moles_no2 -= b.moles_no2;
				a.moles_h2o -= b.moles_h2o;

				return a;
#endif
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

				public Temperature temperature = Temperature.Ambient;

				[Save.Ignore, Net.Ignore] public Area vent_area_total_cached;
				[Save.Ignore, Net.Ignore] public Amount moles_total_cached;
				[Save.Ignore, Net.Ignore] public Pressure pressure_cached;
				[Save.Ignore, Net.Ignore] public Density density_cached;
				[Save.Ignore, Net.Ignore] public Mass mass_cached;

				[Save.Ignore, Net.Ignore] public float vent_y_top_cached;
				[Save.Ignore, Net.Ignore] public float vent_y_bottom_cached;

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

		[ISystem.PreUpdate.Reset(ISystem.Mode.Single, ISystem.Scope.Region, order: 100, flags: ISystem.Flags.Unchecked | ISystem.Flags.Experimental), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void System_ResetContainer(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Air.Container.Data container)
		{
			if (Vent.Data.is_debug)
			{
				if (Vent.Data.time_step > 0 && (region.GetCurrentTick() % Vent.Data.time_step) != 0) return;
			}

			container.vent_area_total_cached = 0.00f;

			container.vent_y_bottom_cached = -10.00f;
			container.vent_y_top_cached = 10.00f;
		}

		//[ISystem.Modified(ISystem.Mode.Single, ISystem.Scope.Region, order: 200)]
		//public static void System_ModifiedVentContainer(ISystem.Info info, ref Region.Data region, Entity entity,
		//[Source.Owned] ref Air.Container.Data air_container, [HasTag("static", true, Source.Modifier.Owned)] bool is_static,
		//[Source.Owned, Pair.All] ref Vent.Data vent)
		//{
		//	air_container. Maths.Min
		//}

		[ISystem.PreUpdate.A(ISystem.Mode.Single, ISystem.Scope.Region, order: -200, flags: ISystem.Flags.Unchecked | ISystem.Flags.Experimental), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void System_UpdateVentContainer1(ISystem.Info info, ref Region.Data region, Entity entity, ref XorRandom random,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Air.Container.Data air_container,
		[Source.Owned, Pair.All] ref Vent.Data vent)
		{
			if (Vent.Data.is_debug)
			{
				if (Vent.Data.time_step > 0 && (region.GetCurrentTick() % Vent.Data.time_step) != 0) return;
			}

			if (vent.flow_rate.m_value.Abs() > Maths.epsilon && vent.blob.moles_total > Maths.epsilon)
			{
				var air_vent = vent.blob.air;
				var air_cont = air_container.air;

				if (vent.flow_rate.m_value.IsNegative())
				{
					var air_tmp = checked(air_cont - air_vent);
					air_container.air = air_tmp;
					air_container.mass_cached = Maths.Max(air_container.mass_cached - vent.blob.mass, 0.00f);
				}
				//else
				//{
				//	var air_tmp = air_cont + air_vent;
				//	air_container.air = air_tmp;
				//	air_container.mass_cached += vent.blob.mass;

				//	var mass_ratio = Maths.Normalize01(vent.blob.mass, air_container.mass_cached);
				//	air_container.temperature = Maths.Lerp(air_container.temperature, vent.blob.temperature, mass_ratio * 0.80f);

				//	vent.blob = default;
				//}
			}

			air_container.vent_y_bottom_cached = Maths.Max(vent.offset.Y, air_container.vent_y_bottom_cached);
			air_container.vent_y_top_cached = Maths.Min(vent.offset.Y, air_container.vent_y_top_cached);

			air_container.vent_area_total_cached += vent.cross_section * vent.modifier;
		}

		[ISystem.PreUpdate.B(ISystem.Mode.Single, ISystem.Scope.Region, order: 200, flags: ISystem.Flags.Unchecked | ISystem.Flags.Experimental), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void System_UpdateVentConnections(ISystem.Info info, ref Region.Data region, Entity entity, ref XorRandom random,
		[Source.Owned] ref Pipe.Data pipe, [Source.Owned] ref Pipe.State pipe_state, [Source.Owned] in Transform.Data transform)
		{
			if (Vent.Data.is_debug)
			{
				if (Vent.Data.time_step > 0 && (region.GetCurrentTick() % Vent.Data.time_step) != 0) return;
			}

			if (pipe.a.TryGetHandle(out var vent_a) && pipe.b.TryGetHandle(out var vent_b))
			{
				//var sync = false;

				var flow_rate_a = vent_a.data.flow_rate;
				var flow_rate_b = vent_b.data.flow_rate;

				var flow_rate_delta = ((flow_rate_a.m_value.Abs() - flow_rate_b.m_value.Abs()) * 0.50f);

				if (vent_a.data.type == Vent.Type.Output)
				{
					//var air_tmp = vent_a.data.blob.air; // vent_b.data.blob.air + vent_a.data.blob.air;
					//vent_b.data.blob = new Blob(air_tmp, Maths.SumWeighted(vent_a.data.blob.temperature, vent_b.data.blob.temperature, vent_a.data.blob.mass, vent_b.data.blob.mass));
					//vent_b.data.blob = new Blob(air_tmp, vent_a.data.blob.temperature);
					//vent_b.data.flow_rate = vent_a.data.flow_rate.m_value.Abs();
					//vent_b.data.flow_rate = 4;

					//vent_b.data.blob.air = vent_a.data.blob.air;
					//vent_b.data.particulates = vent_a.data.particulates;
					////vent_b.data.flow_rate = flow_rate_delta;

					////vent_a.data.blob = default;
					////vent_a.data.particulates = default;
					//vent_a.data.flow_rate = -vent_b.data.flow_rate;

					vent_b.data.blob = vent_a.data.blob;
					vent_b.data.particulates = vent_a.data.particulates;
					vent_b.data.flow_rate = flow_rate_delta.Abs();

					vent_b.data.pressure_outside = vent_a.data.pressure_inside;
					vent_a.data.pressure_outside = vent_b.data.pressure_inside;

					vent_a.data.flow_rate = -flow_rate_delta.Abs();

					//vent_b.data.velocity = vent_a.data.velocity.Abs();

					//vent_a.data.blob = default;
					//vent_a.data.particulates = default;

					//vent_a.data.flow_rate = -8.00f;
				}
				//else
				//{
				//	vent_b.data.blob = vent_a.data.blob;
				//	vent_b.data.particulates = vent_a.data.particulates;
				//	vent_b.data.flow_rate = vent_

				//	vent_a.data.blob = default;
				//	vent_a.data.particulates = default;
				//}

				//var flow_rate_a = vent_a.data.flow_rate;
				//var flow_rate_b = vent_b.data.flow_rate;

				//var flow_rate_delta = (flow_rate_a - flow_rate_b);

				//if (vent_a.data.flow_rate < vent_b.data.flow_rate)
				//{

				//	//vent.blob = default;
				//}
				//else
				//{

				//}

				//#if SERVER
				//				if (sync)
				//				{
				//					//entity.SyncComponent(ref pipe_state);
				//					pipe_state.Sync(entity);
				//				}
				//#endif
			}
		}

		[ISystem.PreUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region, order: 100, flags: ISystem.Flags.Unchecked | ISystem.Flags.Experimental), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void System_UpdateVentContainer2(ISystem.Info info, ref Region.Data region, Entity entity, ref XorRandom random,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Air.Container.Data air_container,
		[Source.Owned, Pair.All] ref Vent.Data vent)
		{
			if (Vent.Data.is_debug)
			{
				if (Vent.Data.time_step > 0 && (region.GetCurrentTick() % Vent.Data.time_step) != 0) return;
			}

			//var pressure_a = vent.pressure_outside;
			//var pressure_b = vent.pressure_inside;

			if (vent.flow_rate.m_value.Abs() > Maths.epsilon && vent.blob.moles_total > Maths.epsilon)
			{
				var air_vent = vent.blob.air;
				var air_cont = air_container.air;

				if (vent.flow_rate.m_value.IsNegative())
				{
					//var air_tmp = checked(air_cont - air_vent);
					//air_container.air = air_tmp;
					//air_container.mass_cached = Maths.Max(air_container.mass_cached - vent.blob.mass, 0.00f);
					//vent.particulates = default;
				}
				else
				{
					var air_tmp = air_cont + air_vent;
					air_container.air = air_tmp;
					air_container.mass_cached += vent.blob.mass;

					var mass_ratio = Maths.Normalize01(vent.blob.mass, air_container.mass_cached);
					air_container.temperature = Maths.Lerp(air_container.temperature, vent.blob.temperature, mass_ratio * 0.80f);
				}

				//vent.blob = default;
			}
		}

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region, order: 100, flags: ISystem.Flags.Unchecked | ISystem.Flags.Experimental), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void System_RefreshContainer(ref Region.Data region,
		[Source.Owned] ref Air.Container.Data container)
		{
			if (Vent.Data.is_debug)
			{
				if (Vent.Data.time_step > 0 && (region.GetCurrentTick() % Vent.Data.time_step) != 0) return;
			}

			//var ts = Timestamp.Now();
			var air = container.air;

			var volume = container.volume;
			var temperature = container.temperature;

			var moles_total = air.GetTotalMoles();
			var mass = air.GetMass();

			var density = mass.GetDensity(volume);

			Phys.IdealGasLawFast(out var pressure, volume, moles_total, temperature);

			container.moles_total_cached = moles_total;
			container.pressure_cached = pressure;
			container.density_cached = density; // mass / volume;
			container.mass_cached = mass;

			//var ts_elapsed = ts.GetMilliseconds();
			//App.WriteLine($"{ts_elapsed:0.0000} ms");

			//container.vent_area_total_cached = 0.00f;
		}

		//[ISystem.PreUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region, order: -300)]
		//public static void System_RefreshVentContainer(ISystem.Info info, ref Region.Data region, Entity entity,
		//[Source.Owned] ref Air.Container.Data container, [Source.Owned, Pair.All] ref Vent.Data vent)
		//{
		//	container.vent_area_total_cached += vent.cross_section * vent.modifier;
		//}

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, order: 500, flags: ISystem.Flags.Unchecked | ISystem.Flags.Experimental), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void System_UpdateVent(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] in Transform.Data transform, [HasTag("static", true, Source.Modifier.Owned)] bool is_static,
		[Source.Owned, Pair.All] ref Vent.Data vent, [Source.Owned] in Air.Container.Data air_container)
		{


			var area = vent.cross_section * vent.modifier;
			var dt = info.DeltaTime;


			var vent_ratio = Maths.Normalize01(area, air_container.vent_area_total_cached);


			//var flow_rate = (Volume)0.00f;

			var pressure_inside = air_container.pressure_cached;
			var pressure_outside = vent.pressure_outside;

			var density_inside = air_container.density_cached;
			var density_outside = air_container.density_cached;

			//var flow_rate_in = (Volume)0.00f;
			//var flow_rate_out = (Volume)0.00f;

			region.GetAtmospherićInfo(transform.LocalToWorld(vent.offset).Y, out var altitude, out var temperature_ambient, out var pressure_ambient);

			if (vent.flags.HasAny(Vent.Data.Flags.Has_Pipe))
			{
				density_outside = Phys.GetAirDensity(pressure_outside, vent.blob.temperature);
			}
			else
			{
				// TODO: cache this for static entities, since their position doesn't change
				pressure_outside = pressure_ambient;
				density_outside = Phys.GetAirDensity(pressure_ambient, temperature_ambient);

				//pressure_outside = region.GetAtmosphericPressure(transform.position.Y);
				//density_outside = Phys.GetAirDensity(pressure_outside, Phys.ambient_temperature)
			}

			var height = air_container.vent_y_bottom_cached - vent.offset.Y;
			var delta_p = pressure_inside - pressure_outside;

			var flow_rate = vent.flow_rate;
			var flow_rate_convection = (Volume)0.00f;

#if DEBUG
			if (Vent.Data.is_debug)
			{
				if (Vent.Data.time_step > 0 && (region.GetCurrentTick() % Vent.Data.time_step) != 0) goto end;
			}
#endif

			if (area > Maths.epsilon)
			{



				//Phys.OrificeFlow(out flow_rate_out, area, density_inside, pressure_inside, pressure_outside);
				//Phys.OrificeFlow(out flow_rate_in, area, density_outside, pressure_outside, pressure_inside);

				//flow_rate = vent.flow_rate.m_value.Abs();
				//flow_rate = flow_rate_in - flow_rate_out;

				Phys.OrificeFlowDual(out var flow_rate_target, area, density_inside, density_outside, pressure_inside, pressure_outside);

				if (!vent.flags.HasAny(Vent.Data.Flags.Has_Pipe))
				{
					if (height > Maths.epsilon)
					{
						Phys.VentilationAirFlow(area, height, air_container.temperature, temperature_ambient, out flow_rate_convection);
					}

					if (flow_rate_target.m_value.IsNegative())
					{
						flow_rate_target -= flow_rate_convection;
					}
				}

				//flow_rate_target -= flow_rate_convection;

				//flow_rate_target = Maths.ClampMagnitude(flow_rate_target, air_container.volume) * vent_ratio;

				Maths.MoveTowardsDamped(ref flow_rate.m_value, flow_rate_target, delta_p.m_value.Abs() * 100, 0.10f);
				//flow_rate = flow_rate_target;
				if (flow_rate.m_value.Abs() > Maths.epsilon)
				{

					//if (pressure_inside > pressure_outside)
					//if (flow_rate_out > flow_rate_in)
					if (flow_rate.m_value.IsNegative())
					{
						//Maths.MoveTowardsDamped(ref flow_rate, Maths.Clamp(flow_rate_out, 0.00f, air_container.volume), delta_p * dt, 0.10f);

						var volume = Maths.Abs(flow_rate) * dt;
						var ratio = Maths.Normalize(volume, air_container.volume);

						var air = air_container.air;
						air *= ratio;

						vent.blob = new Air.Blob(air, air_container.temperature);
					}
					else if (vent.flags.HasAny(Vent.Data.Flags.Has_Pipe))
					{

					}
					else
					{
						
						//Maths.MoveTowardsDamped(ref flow_rate, Maths.Clamp(flow_rate_in, 0.00f, air_container.volume), delta_p * dt, 0.10f);
						//flow_rate = Maths.Clamp(flow_rate_in, 0.00f, air_container.volume);

						var volume = Maths.Abs(flow_rate) * dt;
						var mass = density_outside.m_value * volume;

						var air = new Air.Composition();
						air.moles_n2 = (mass * Phys.air_n2_ratio) * Phys.n2_molar_mass_inv;
						air.moles_o2 = (mass * Phys.air_o2_ratio) * Phys.o2_molar_mass_inv;
						air.moles_co2 = (mass * Phys.air_co2_ratio) * Phys.co2_molar_mass_inv;

						vent.blob = new Air.Blob(air, temperature_ambient);
					}
				}

				vent.flow_rate = flow_rate;
				vent.velocity = vent.flow_rate / vent.cross_section;
			}
			else
			{
				vent.blob = default;

				vent.flow_rate = 0.00f;
				vent.velocity = 0.00f;
			}

			vent.pressure_outside = pressure_outside;
			vent.pressure_inside = pressure_inside;

#if DEBUG
			end:
			{
#if CLIENT
				{
					var color = vent.flow_rate.m_value.IsPositive() ? Color32BGRA.Blue : Color32BGRA.Red;
					var radius = MathF.Sqrt(vent.cross_section / MathF.PI);

					region.DrawDebugCircle(transform.LocalToWorld(vent.offset), radius: radius,
						color: color.WithAlpha(150), filled: false);

					region.DrawDebugCircle(transform.LocalToWorld(vent.offset), radius: radius * vent.modifier,
						color: color.WithAlpha(50), filled: true);

					//region.DrawDebugCircle(transform.LocalToWorld(vent.offset), radius: radius_actual,
					//	color: Color32BGRA.Orange.WithAlpha(50), filled: true);

					region.DrawDebugDir(transform.LocalToWorld(vent.offset),
						dir: Maths.RadToDir(transform.LocalToWorldRotation(vent.rotation)) * Maths.Abs(vent.velocity).Sqrt().WithSign(-vent.velocity),
						color: color, thickness: 4.00f);

					region.DrawDebugText(transform.LocalToWorld(vent.offset + new Vector2(0.75f, -1.00f)),
					//$"in: {flow_rate_in:0.0000} m³/s\n" +
					//$"out: {flow_rate_out:0.0000} m³/s\n" +
					$"flow: {vent.flow_rate:+0.0000;-0.0000} m³/s\n" +

					$"delta_p: {delta_p:+0.0000;-0.0000}\n" +
					$"vent_ratio: {vent_ratio:0.00}\n" +
					//$"mass_ratio: {Maths.Normalize(vent.blob.mass, air_container.mass_cached):0.0000}\n" +

					$"height: {height:0.00}\n" +
					$"temperature: {air_container.temperature:0.00}\n" +
					$"temperature_ambient: {temperature_ambient:0.00}\n" +

					$"density_inside: {density_inside:0.0000}\n" +
					$"density_outside: {density_outside:0.0000}\n" +

					$"pressure_inside: {pressure_inside:0.0000}\n" +
					$"pressure_outside: {pressure_outside:0.0000}\n" +

					$"velocity: {vent.velocity:+0.000;-0.000} m/s\n" +
					"", Color32BGRA.White);
				}
#endif
			}
#endif
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
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";
		public static readonly Texture.Handle texture_light = "light.circle.00";

		public enum Type: ushort
		{
			Undefined = 0,

			Input,
			Output,




			//[Save.Ignore] Has_Pressure_Cached = 1u << 31,
		}

		[IComponent.Data(Net.SendType.Reliable, region_only: true), ITrait.Data(Net.SendType.Reliable, region_only: true)]
		public struct Data: IComponent, ITrait
		{
#if DEBUG
			public static ulong time_step = 10;
			public static bool is_debug = false;
#else
			public const ulong time_step = 1;
			public const bool is_debug = false;
#endif

			[Flags]
			public enum Flags: ushort
			{
				None = 0,

				Has_Pipe = 1 << 0,

				//[Save.Ignore] Has_Pressure_Cached = 1u << 31,
			}

			public Air.Blob blob;
			public Air.Particulates particulates;

			[Editor.Picker.Position(true, true)]
			public Vector2 offset;

			[Editor.Slider.Clamped(-MathF.Tau, +MathF.Tau, snap: MathF.Tau / 32.00f, mark_modified: true)]
			public float rotation;
			public Area cross_section = Area.Circle(10.00f.cm()); // TODO: dumb name, rename this

			public float modifier = 1.00f;
			public Vent.Data.Flags flags;
			public Vent.Type type;

			[Save.Ignore, Net.Ignore] public Color32BGRA color_smoke;
			[Save.Ignore, Net.Ignore] public Pressure pressure_outside = Phys.atmospheric_pressure_kordel; // TODO: is this really needed?
			[Save.Ignore, Net.Ignore] public Pressure pressure_inside = Phys.atmospheric_pressure_kordel; // TODO: is this really needed?
			[Save.Ignore, Net.Ignore] public Volume flow_rate;
			[Save.Ignore, Net.Ignore] public float velocity;

			[Save.Ignore, Net.Ignore] public float t_next_smoke;

			public Data()
			{

			}
		}



		// TODO: vectorize this
		public static Color32BGRA GetSmokeColor(ref readonly this Vent.Data vent)
		{
			var air = vent.blob.air;
			var prt = vent.particulates;

			var mass_air = vent.blob.mass;
			var mass_prt = prt.GetMass();

			var mass_total = mass_air + mass_prt;
			var mass_total_inv = mass_total.m_value.RcpFast();

			var h2o_ratio = air.moles_h2o.m_value * Phys.h2o_molar_mass * mass_total_inv;
			var so2_ratio = air.moles_so2.m_value * Phys.so2_molar_mass * mass_total_inv;
			var no2_ratio = air.moles_no2.m_value * Phys.no2_molar_mass * mass_total_inv;

			var ash_ratio = prt.ash.m_value * mass_total_inv;
			var soot_ratio = prt.soot.m_value * mass_total_inv;
			var dust_ratio = prt.dust.m_value * mass_total_inv;

			var color = ColorBGRA.White.WithAlphaMult(0.30f);
			color = ColorBGRA.Lerp(color, new ColorBGRA(0.40f, 0.655f, 0.443f, 0.306f), (no2_ratio * 2.80f).Clamp01());
			color = ColorBGRA.Lerp(color, new ColorBGRA(0.40f, 0.755f, 0.790f, 0.26f), (so2_ratio * 3.80f).Clamp01());
			color = ColorBGRA.Lerp(color, new ColorBGRA(0.80f, 0.98f, 0.98f, 0.98f), (h2o_ratio * 10.00f).Clamp01());
			color = ColorBGRA.Lerp(color, new ColorBGRA(1.20f, 0.255f, 0.242f, 0.24f), (soot_ratio * 4.50f).Clamp01());
			color = ColorBGRA.Lerp(color, new ColorBGRA(6.40f, 0.655f, 0.683f, 0.616f), (ash_ratio * 3.40f).Clamp01());
			color = ColorBGRA.Lerp(color, 0xff837462, dust_ratio);
			color.a = Maths.Clamp01(color.a);
			//color = ColorBGRA.Lerp(color, new ColorBGRA(0.20f * ash_ratio, 0.755f, 0.743f, 0.746f), ash_ratio);
			//color += (new ColorBGRA(0.60f, 0.95f, 0.95f, 0.95f) * h2o_ratio);
			//color += (new ColorBGRA(0.60f, 0.95f, 0.95f, 0.95f) * h2o_ratio);

			//App.WriteLine(vent.blob.temperature);

			return color;
		}

#if CLIENT
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, order: 1000)]
		public static void System_VentEffects(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Transform.Data transform, [Source.Owned, Pair.All] ref Vent.Data vent)
		{
			if (info.WorldTime >= vent.t_next_smoke && !vent.flags.HasAny(Vent.Data.Flags.Has_Pipe))
			{
				if (vent.flow_rate < -0.001f && vent.blob.temperature > 400.00f)
				{
					var flow_rate_abs = vent.flow_rate.m_value.Abs();
					var modifier = MathF.Sqrt(flow_rate_abs * 2.00f) * 0.30f;
					var vel = Maths.Min(vent.velocity.Abs(), 20.00f);

					var color_smoke = vent.GetSmokeColor();

					vent.t_next_smoke = info.WorldTime + Maths.Lerp01(0.40f, 0.10f, Maths.Normalize01(vel * 0.90f, 8.00f)); //  (0.40f * Maths.Mulpo(purity, 0.40f));
					vent.color_smoke = color_smoke;

					if (modifier > 0.02f && color_smoke.IsVisible() && Camera.IsVisible(Camera.CullType.Rect2x, transform.position))
					{
						var dir = Maths.RadToDir(transform.LocalToWorldRotation(vent.rotation));

						var pos = transform.LocalToWorld(vent.offset);
						Particle.Spawn(ref region, new Particle.Data()
						{
							texture = texture_smoke,
							lifetime = Maths.Clamp(random.NextFloatRange(2.00f, 6.00f) * modifier, 1.00f, 4.00f),
							pos = pos + random.NextVector2(0.10f) + (dir * 0.50f),
							//vel = new Vector2(0.70f, -1.10f) * (1.00f + (modifier2 * 0.10f)),
							vel = dir * vel * random.NextFloatRange(0.90f, 1.10f),
							force = new Vector2(2.50f, -2.50f) * random.NextFloatRange(0.90f, 1.10f),
							fps = random.NextByteRange(6, 10),
							frame_count = 64,
							frame_count_total = 64,
							frame_offset = random.NextByteRange(0, 64),
							scale = (modifier * 0.22f) + random.NextFloatRange(0.19f, 0.28f),
							rotation = random.NextFloatRange(-10.00f, 10.00f),
							angular_velocity = random.NextFloat(0.60f),
							growth = 0.25f * (1.00f + (modifier.Clamp01() * 0.15f)) * random.NextFloatRange(0.90f, 1.10f),
							drag = random.NextFloatRange(0.015f, 0.04f),
							color_a = color_smoke,
							color_b = color_smoke.WithAlpha(0)
						});
					}
				}
				else
				{
					vent.t_next_smoke = info.WorldTime + random.NextFloatRange(0.50f, 1.00f);
				}
			}
		}
#endif
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


					resizable.cap_a_rotation = transform_a.LocalToWorldRotation(vent_a.data.rotation); // transform_a.LocalToWorldDirection(vent_a.data.direction).GetAngleRadiansFast(); //.LocalToWorldRotation(MathF.PI);
					resizable.cap_b_rotation = transform_b.LocalToWorldRotation(vent_b.data.rotation);
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

					var dir_a = transform_a.LocalToWorldRotation(vent_a.data.rotation).RadToDir();
					var dir_b = transform_b.LocalToWorldRotation(vent_b.data.rotation).RadToDir();

					renderer.p0 = pos_a + (dir_a * 0.250f);
					renderer.p3 = pos_b + (dir_b * 0.250f);

					//renderer.p1 = pos_a + (new Vector2(pos_b.X - pos_a.X, 0.00f));
					//renderer.p2 = renderer.p1 + new Vector2(0.00f, 0.00f);

					renderer.p1 = pos_a + (dir_a * 2.50f);
					renderer.p2 = pos_b + (dir_b * 2.50f);

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
