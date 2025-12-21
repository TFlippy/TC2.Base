namespace TC2.Base.Components
{
	public static partial class Piston
	{
		[Flags]
		public enum Flags: ushort
		{
			None = 0,

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
		public static void OnUpdateRenderer(/*ISystem.Info info,*/
		/*[Source.Owned] in Transform.Data transform, */[Source.Owned] ref Piston.Data piston,
		[Source.Owned, Pair.Component<Piston.Data>] ref Animated.Renderer.Data renderer)
		{
			renderer.offset = piston.offset.AddY(piston.current_distance);
			//renderer_slider.offset = press.slider_offset + new Vector2(0.00f, MathF.Pow((MathF.Cos(axle_state.rotation) + 1.00f) * 0.50f, press.speed) * press.slider_length);
		}

		[ISystem.PostUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(/*ISystem.Info info, */ref Region.Data region, /*ref XorRandom random, Entity ent_piston,*/
		//[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Piston.Data piston)
		{
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

		[NotImplemented]
		[ISystem.Event<Essence.PulseEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnEssencePulseEvent(ref Region.Data region, ISystem.Info info, Entity ent_piston, ref Essence.PulseEvent ev,
		[Source.Owned] in Transform.Data transform, /*[Source.Owned] ref Control.Data control,*/
		[Source.Owned] ref Piston.Data piston, [Source.Owned] ref Essence.Emitter.Data essence_emitter)
		{
			//App.WriteLine("essence pulse event", color: App.Color.Magenta);
		}

		//		[ISystem.PostUpdate.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		//		public static void OnUpdate_Essence(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_piston,
		//		[Source.Owned] in Transform.Data transform, /*[Source.Owned] ref Control.Data control,*/
		//		[Source.Owned] ref Piston.Data piston, [Source.Owned, Pair.Component<Piston.Data>] ref Essence.Emitter.Data essence_emitter
		//		/*[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state*/)
		//		{
		//			//if (control.mouse.GetKey(Mouse.Key.Left))

		//			if (essence_emitter.state_flags.HasAny(Essence.Emitter.StateFlags.Pulse))
		//			{
		//				//App.WriteLine("press pressed", color: App.Color.Magenta);


		//#if SERVER
		//				var power = (essence_emitter.current_emit * Essence.GetKineticPower(essence_emitter.h_essence_charge).m_value); //.Abs();
		//				//var speed_add = Energy.GetVelocity(power, piston.mass);

		//				var speed_add = Maths.SqrtSigned((power + power) / piston.mass);
		//				//piston.current_speed += Energy.GetVelocity(Essence.GetForce()
		//				piston.current_speed += speed_add;
		//				//var energy_impact = Energy.GetKineticEnergy(piston.mass, piston.current_speed);
		//				//App.WriteValue(energy_impact);

		//				//App.WriteValue(speed_add);
		//				piston.Sync(ent_piston, true);

		//#endif

		//#if CLIENT
		//				var h_essence = essence_emitter.h_essence; // new IEssence.Handle("motion");
		//				ref var essence_data = ref h_essence.GetData();
		//				if (essence_data.IsNotNull())
		//				{
		//					var pos = transform.LocalToWorld(essence_emitter.offset);
		//					var dir = transform.LocalToWorldDirection(essence_emitter.direction);

		//					var intensity = 1.00f;
		//					var color_a = ColorBGRA.Lerp(essence_data.color_emit, ColorBGRA.White, 0.50f);
		//					var color_b = essence_data.color_emit.WithColorMult(0.20f).WithAlphaMult(0.00f);

		//					//App.WriteLine("essence");

		//					//Sound.Play(region: ref region, sound: essence_emitter.h_sound_emit, world_position: pos, volume: 1.00f, pitch: 1.00f, size: 0.35f, dist_multiplier: 0.65f);
		//					Sound.Play(region: ref region, h_soundmix: essence_emitter.h_soundmix_test, random: ref random, pos: pos); //, volume: 1.00f, pitch: 1.00f, size: 0.35f, dist_multiplier: 0.65f);
		//					Shake.Emit(region: ref region, world_position: pos, trauma: 0.35f, max: 0.50f, radius: 10.00f);

		//					Particle.Spawn(ref region, new Particle.Data()
		//					{
		//						texture = Light.tex_light_circle_00,
		//						lifetime = 0.20f,
		//						pos = pos - dir,
		//						vel = dir * 20.00f,
		//						drag = 0.20f,
		//						frame_count = 1,
		//						frame_count_total = 1,
		//						frame_offset = 0,
		//						scale = 1.00f,
		//						stretch = new Vector2(1.00f, 0.50f),
		//						face_dir_ratio = 1.00f,
		//						growth = 50.00f,
		//						color_a = color_a,
		//						color_b = color_b,
		//						glow = 20.00f * intensity
		//					});

		//					//Particle.Spawn(ref region, new Particle.Data()
		//					//{
		//					//	texture = Light.tex_light_circle_04,
		//					//	lifetime = random.NextFloatRange(1.00f, 1.25f),
		//					//	pos = data.world_position + (dir * 0.50f),
		//					//	scale = random.NextFloatRange(1.00f, 1.50f),
		//					//	growth = random.NextFloatRange(1.50f, 2.00f),
		//					//	stretch = new Vector2(0.90f, 0.60f),
		//					//	rotation = dir.GetAngleRadiansFast(),
		//					//	face_dir_ratio = 1.00f,
		//					//	color_a = new Vector4(1.00f, 0.70f, 0.40f, 30.00f),
		//					//	color_b = new Vector4(0.20f, 0.00f, 0.00f, 0.00f),
		//					//	glow = 1.00f
		//					//});
		//				}
		//#endif
		//			}
		//		}

		//[ISystem.PreUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void PostUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_piston,
		//[Source.Owned] in Transform.Data transform, /*[Source.Owned] ref Control.Data control,*/
		//[Source.Owned] ref Piston.Data piston,
		//[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		//{
		//	//#if SERVER
		//	//			if (control.mouse.GetKey(Mouse.Key.Left))
		//	//			{
		//	//				App.WriteLine("press pressed", color: App.Color.Magenta);


		//	//				piston.current_speed = 25.00f;

		//	//				piston.Sync(ent_piston, true);
		//	//			}
		//	//#endif

		//	//App.WriteLine("press");
		//}
	}
}
