namespace TC2.Base.Components
{
	public static partial class Mover
	{
		[Flags]
		public enum StateFlags: byte
		{
			None = 0,

			Revolved = 1 << 0
		}

		public enum Status: byte
		{
			Idle = 0,
		}

		[Flags]
		public enum Flags: ushort
		{
			None = 0,

			Synchronous = 1 << 0
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Net.Segment.A, Save.Force, Editor.Picker.Position(relative: true)] public required Vec2f offset;
			[Net.Segment.A, Save.Force, Editor.Picker.Direction(normalize: true)] public required Vec2f direction = Vec2f.Down;

			[Net.Segment.A, Save.Force] public required float length = 1.00f;
			[Net.Segment.A, Save.Force] public required Mass mass = 50.00f;

			[Net.Segment.C, Asset.Ignore] public Mover.Status status;
			[Net.Segment.C, Asset.Ignore] public Mover.StateFlags state_flags;
			[Net.Segment.C] public Mover.Flags flags;
			[Net.Segment.C, Asset.Ignore] public float distance_current;
			[Net.Segment.C, Asset.Ignore] public float progress_current;

			[Net.Segment.D, Asset.Ignore] public float progress_delta;
			[Net.Segment.D, Asset.Ignore, Unused] public float velocity_current;
		}

		[ISystem.PostUpdate.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Renderer([Source.Owned] ref Mover.Data mover,
		[Source.Owned, Pair.Component<Mover.Data>] ref Animated.Renderer.Data renderer,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{
			renderer.offset = mover.offset + (mover.direction * mover.distance_current); // TODO: use FMA
		}

		[ISystem.PostUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Simulation(ref Region.Data region, [Source.Owned] ref Mover.Data mover,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{


			//var distance_old = piston.current_distance;
			//var velocity_new = piston.current_velocity;

			//var distance_new = Maths.FMA(piston.current_velocity, App.fixed_update_interval_s_f32, distance_old);
			//var distance_new_clamped = Maths.Clamp(distance_new, 0.00f, piston.length);

			//piston.impact_kinetic_energy = 0.00f;

			//var force = -distance_new_clamped * piston.spring_constant;
			//force += region.GetGravity().Y * piston.mass;
			//var acceleration = force / piston.mass;
			//velocity_new += acceleration * App.fixed_update_interval_s_f32;

			//if (piston.status == Status.Impacted)
			//{
			//	piston.status = Status.Retracting;
			//	piston.flags.RemoveFlag(Flags.Impacted);
			//}
			//else if (distance_new > piston.length)
			//{
			//	if (piston.current_velocity > 2.00f)
			//	{
			//		var energy_impact = Energy.GetKineticEnergy(piston.mass, piston.current_velocity);
			//		piston.impact_momentum = piston.current_velocity * piston.mass;
			//		velocity_new -= Energy.GetVelocity(energy_impact, piston.mass); // * 0.05f;

			//		piston.impact_kinetic_energy += energy_impact;
			//		piston.flags.AddFlag(Flags.Impacted);
			//		piston.status = Status.Impacted;
			//	}
			//	//velocity_new *= 0.10f;
			//}
			//else if (distance_new < 0.00f)
			//{
			//	velocity_new *= -0.10f;
			//}
			//else
			//{
			//	piston.status = Status.Idle;
			//}

			//velocity_new *= piston.damping;

			//piston.prev_velocity = piston.current_velocity;
			//piston.current_velocity = velocity_new;
			//piston.current_distance = distance_new_clamped;
		}
	}

	public static partial class Linkage
	{
		[Flags]
		public enum Flags: ushort
		{
			None = 0,

		}

		[Flags]
		public enum StateFlags: byte
		{
			None = 0,
		
		}

		public enum Type: byte
		{
			Undefined = 0,

			Inline,
			Offset
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Net.Segment.A, Save.Force] public float length_a;
			[Net.Segment.A, Save.Force] public float length_b;
			[Net.Segment.A, Save.Force] public float length_c;
			[Net.Segment.A, Save.Force] public float length_d;

			//[Net.Segment.A, Save.Force] public required Mass mass = 50.00f;

			[Net.Segment.C, Asset.Ignore] public Linkage.Type type;
			[Net.Segment.C, Asset.Ignore] public Linkage.StateFlags state_flags;
			[Net.Segment.C] public Linkage.Flags flags;
			[Net.Segment.C] public float eccentricity;
			//[Net.Segment.C, Asset.Ignore] public float distance_current;
			//[Net.Segment.C, Asset.Ignore] public float progress_current;

			//[Net.Segment.D, Asset.Ignore] public float progress_delta;
			//[Net.Segment.D, Asset.Ignore, Unused] public float velocity_current;
		}

		[ISystem.PostUpdate.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnPostUpdate_A([Source.Owned] ref Linkage.Data linkage, [Source.Owned] ref Transform.Data transform)
		{

		}

		[ISystem.PostUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnPostUpdate_D(ref Region.Data region, [Source.Owned] ref Linkage.Data linkage,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{

		}

#if CLIENT
		[ISystem.Render(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Render(ISystem.Info info, ref Region.Data region, [Source.Owned] ref Linkage.Data linkage,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{

		}
#endif
	}

	// TODO: split this so it also covers non-damped pistons or slider linkages
	public static partial class Piston
	{
		[Flags]
		public enum Flags: ushort
		{
			None = 0,

			No_Simulate = 1 << 0,

			//Active = 1 << 0, // Piston is currently active and moving
			//Idle = 1 << 1, // Piston is idle and not moving
			//Extended = 1 << 2, // Piston is fully extended
			Impacted = 1 << 3, // Piston has impacted
		}

		public enum Status: byte
		{
			Idle = 0, // Piston is idle and not moving
			Impacted, // Piston has impacted
			Retracting, // Piston is retracting
			Moving, // Piston is currently moving
			Retracted, // Piston is fully retracted
			Pushing, // Piston is currently pushing
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Net.Segment.A, Save.Force, Editor.Picker.Position(relative: true)] public required Vec2f offset;
			[Net.Segment.A, Save.Force, Editor.Picker.Direction(normalize: true)] public required Vec2f direction = Vec2f.Down;

			[Net.Segment.A, Save.Force] public required float length = 1.00f;
			[Net.Segment.A, Save.Force] public required Mass mass = 50.00f;
			[Net.Segment.A, Save.Force] public float damping = 0.92f;
			[Net.Segment.A, Save.Force] public float spring_constant = 250.00f;
			//public Sound.Handle h_sound;

			[Net.Segment.C, Asset.Ignore] public Piston.Status status;
			[Net.Segment.C, Asset.Ignore] private byte unused_c_00;
			[Net.Segment.C] public Piston.Flags flags;
			[Net.Segment.C, Asset.Ignore] public float current_distance;
			[Net.Segment.C, Asset.Ignore] private float unused_c_01;
			[Net.Segment.C, Asset.Ignore] public float impact_momentum; // TODO: move this to Press.State

			[Net.Segment.D, Asset.Ignore] private float unused_d_00;
			[Net.Segment.D, Asset.Ignore, Unused] public float prev_velocity;
			[Net.Segment.D, Asset.Ignore] public float current_velocity;
			[Net.Segment.D, Asset.Ignore] public Energy impact_kinetic_energy; // TODO: move this to Press.State
		}

		[ISystem.PostUpdate.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Renderer([Source.Owned] ref Piston.Data piston,
		[Source.Owned, Pair.Component<Piston.Data>] ref Animated.Renderer.Data renderer)
		{
			renderer.offset = piston.offset.AddY(piston.current_distance);
			//renderer_slider.offset = press.slider_offset + new Vector2(0.00f, MathF.Pow((MathF.Cos(axle_state.rotation) + 1.00f) * 0.50f, press.speed) * press.slider_length);
		}

		[ISystem.PostUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Simulation(ref Region.Data region, [Source.Owned] ref Piston.Data piston)
		{
			if (piston.flags.HasAny(Piston.Flags.No_Simulate)) return;

			var distance_old = piston.current_distance;
			var velocity_new = piston.current_velocity;

			var distance_new = Maths.FMA(piston.current_velocity, App.fixed_update_interval_s_f32, distance_old);
			var distance_new_clamped = Maths.Clamp(distance_new, 0.00f, piston.length);

			piston.impact_kinetic_energy = 0.00f;

			var force = -distance_new_clamped * piston.spring_constant;
			force += region.GetGravity().Y * piston.mass;
			var acceleration = force / piston.mass;
			velocity_new += acceleration * App.fixed_update_interval_s_f32;

			if (piston.status == Status.Impacted)
			{
				piston.status = Status.Retracting;
				piston.flags.RemoveFlag(Flags.Impacted);
			}
			else if (distance_new > piston.length)
			{
				if (piston.current_velocity > 2.00f)
				{
					var energy_impact = Energy.GetKineticEnergy(piston.mass, piston.current_velocity);
					piston.impact_momentum = piston.current_velocity * piston.mass;
					velocity_new -= Energy.GetVelocity(energy_impact, piston.mass); // * 0.05f;

					piston.impact_kinetic_energy += energy_impact;
					piston.flags.AddFlag(Flags.Impacted);
					piston.status = Status.Impacted;
				}
				//velocity_new *= 0.10f;
			}
			else if (distance_new < 0.00f)
			{
				velocity_new *= -0.10f;
			}
			else
			{
				piston.status = Status.Idle;
			}

			velocity_new *= piston.damping;

			piston.prev_velocity = piston.current_velocity;
			piston.current_velocity = velocity_new;
			piston.current_distance = distance_new_clamped;
		}
	}
}
