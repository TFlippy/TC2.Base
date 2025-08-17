namespace TC2.Base.Components
{
	public static partial class Actuator
	{
		public enum Type: byte
		{
			Undefined = 0,
		}

		[Flags]
		public enum Flags: ushort
		{
			None = 0,
		}

		// TODO: make it not depend on essences
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Editor.Picker.Position(true)] public Vec2f offset;

			public float load_base;

			public float force_modifier = 1.00f;
			public float torque_modifier = 1.00f;
			public float acceleration_modifier = 1.00f;
			public float brake_modifier = 1.00f;

			public float speed_max = 20.00f;

			public IEssence.Handle h_essence;
			public Actuator.Flags flags;

			public Actuator.Type type;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info,
		[Source.Owned] ref Actuator.Data actuator,
		[Source.Owned] ref Motor.Data motor, [Source.Owned] ref Motor.State motor_state,
		[Source.Owned] ref Essence.Container.Data essence_container)
		{
			var dt = info.DeltaTime;
			//var torque = actuator.force * actuator.piston_radius;

			var speed_old = motor_state.speed_current;
			essence_container.rate = motor_state.power_target_ratio;

			//essence_container.rate.MoveTowardsDamped(motor_state.power_target_ratio, dt, 0.05f);

			if (motor.flags.HasAny(Motor.Flags.Constant_Speed))
			{
				motor_state.speed_target_ratio = motor_state.power_target_ratio;
			}

			motor_state.speed_current.MoveTowardsDamped((actuator.speed_max * motor_state.speed_target_ratio) * (Maths.Max(1.00f, motor_state.utilization_ratio).RcpFast()), actuator.acceleration_modifier * dt, 0.05f);
			//motor_state.speed_current.MoveTowardsDamped((actuator.speed_max * motor_state.speed_target_ratio), actuator.acceleration_modifier * dt, 0.05f);

			motor_state.speed_max = actuator.speed_max;
			motor_state.acceleration_current = motor_state.speed_current - speed_old;
			motor_state.force_available = 0.00f;
			motor_state.load_base = actuator.load_base;

			ref var essence_data = ref essence_container.h_essence.GetData();
			if (essence_data.IsNotNull())
			{
				var essence_amount = essence_container.available * essence_container.rate_current;
				var force = essence_data.emit_force * essence_amount * actuator.torque_modifier;

				motor_state.force_available = force;


				//axle_state.ApplyTorque(force, actuator_state.speed_current);
			}
		}

		//[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void Update(ISystem.Info info,
		//[Source.Owned] ref Actuator.Data actuator, [Source.Owned] ref Actuator.State actuator_state,
		//[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state, [Source.Owned] ref Essence.Container.Data essence_container)
		//{
		//	var dt = info.DeltaTime;
		//	//var torque = actuator.force * actuator.piston_radius;

		//	actuator_state.speed_current.MoveTowardsDamped(actuator.speed_max * actuator_state.speed_target_ratio, actuator.acc * dt, 0.05f);
		//	essence_container.rate.MoveTowardsDamped(actuator_state.speed_target_ratio, 0.50f * dt, 0.05f);


		//	ref var essence_data = ref essence_container.h_essence.GetData();
		//	if (essence_data.IsNotNull())
		//	{
		//		var essence_amount = essence_container.available * essence_container.rate;
		//		var force = essence_data.force_emit * essence_amount * actuator.force_efficiency;

		//		axle_state.ApplyTorque(force, actuator_state.speed_current);
		//	}
		//}

#if CLIENT
		public partial struct ActuatorGUI: IGUICommand
		{
			public Entity ent_interactable;
			public Entity ent_actuator;

			public Actuator.Data actuator;

			public Inventory1.Data inventory;
			//public Axle.Data axle;
			//public Axle.State axle_state;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Actuator"u8, this.ent_interactable, identifier_dock: "motor.embed"u8))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						//GUI.DrawInventory(this.inventory.GetHandle());
						//GUI.Text($"Test");

						//using (GUI.Group.New(new(GUI.RmX, 96)))
						//{
						//	//Boiler.DrawGauge(this.actuator_state.speed_current, 0.00f, this.actuator.speed_max);
						//	GUI.DrawGauge(MathF.Abs(this.axle_state.angular_velocity), 0.00f, this.actuator.speed_max);

						//	GUI.SameLine();

						//	using (GUI.Group.New(new(GUI.RmX, 96)))
						//	{
						//		GUI.DrawInventoryDock(Inventory.Type.Essence, new(48, 48));

						//		using (GUI.Group.New(size: new(GUI.RmX, 0.00f), padding: new(4, 4)))
						//		{
						//			//GUI.Text($"{MathF.Abs(this.axle_state.angular_velocity):0.00}/{this.actuator.speed_max:0.00} rad/s");
						//			GUI.Text($"{MathF.Abs(this.axle_state.angular_velocity):0.00} rad/s");
						//			GUI.Text($"{(this.axle_state.sum_torque * 0.001f):0.00} kNm/s");
						//			GUI.Text($"{(this.axle_state.sum_torque * MathF.Abs(this.axle_state.angular_velocity) * 0.001f):0.00} kW");
						//		}
						//	}
						//}

						//if (GUI.SliderFloat("Target Speed", ref this.actuator.speed_target, 0.00f, this.actuator.speed_max, size: new Vector2(160, 32)))
						//if (GUI.SliderFloatLerp("Target Speed"u8, ref this.actuator_state.speed_target_ratio, 0.00f, this.actuator.speed_max, size: new Vector2(160, 32)))
						//{
						//	var rpc = new Actuator.ConfigureRPC()
						//	{
						//		speed_target_ratio = this.actuator_state.speed_target_ratio
						//	};
						//	rpc.Send(this.ent_actuator);
						//}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity ent_interactable, Entity ent_actuator,
		[Source.Owned] in Actuator.Data actuator,
		[Source.Owned, Pair.Component<Essence.Container.Data>] ref Inventory1.Data inventory,
		[Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.IsActive())
			{
				var gui = new ActuatorGUI()
				{
					ent_interactable = ent_interactable,
					ent_actuator = ent_actuator,

					actuator = actuator,

					inventory = inventory
					//axle = axle,
					//axle_state = axle_state,
				};
				gui.Submit();
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateRenderer(ISystem.Info info, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Motor.Data motor, [Source.Owned] ref Motor.State motor_state,
		[Source.Owned] in Actuator.Data actuator,
		[Source.Owned] ref Essence.Container.Data container,
		[Source.Owned, Pair.Component<Actuator.Data>, Optional(true)] ref Light.Data light)
		{
			//var ratio_a = Maths.Normalize(motor_state.force_applied, motor_state.force_available);
			//var ratio_b = Maths.Normalize(motor_state.speed_current * 1.80f, motor.speed_max);
			//var ratio = Maths.Avg(motor_state.utilization_ratio + motor_state.power_target_ratio, motor_state.power_target_ratio) * motor_state.power_target_ratio; //Maths.Min(ratio_a, ratio_b);
			//var ratio = Maths.Max(motor_state.power_target_ratio, Maths.Avg(motor_state.utilization_ratio + motor_state.power_target_ratio, motor_state.power_target_ratio) * motor_state.power_target_ratio);

			ref var essence_data = ref container.h_essence.GetData();
			if (essence_data.IsNotNull())
			{
				//var dir = -transform.GetDirection();
				//var pos = transform.LocalToWorld(Vector2.Zero);

				//var intensity = Maths.Clamp(ratio * 1.50f, 0.00f, motor_state.power_target_ratio * 2);

				var intensity = Maths.Clamp(motor_state.intensity_current * 1.50f, 0.00f, 2.00f);
				var color_a = ColorBGRA.Lerp(essence_data.color_emit, ColorBGRA.White, 0.50f);
				var color_b = essence_data.color_emit.WithColorMult(0.20f).WithAlphaMult(0.00f);

				if (light.IsNotNull())
				{
					light.color = color_a.WithAlphaMult(intensity * random.NextFloatRange(0.90f, 1.20f));
					light.scale = new Vector2(8, 8) * random.NextFloatRange(0.90f, 1.10f);
					light.rotation = random.NextFloatRange(-0.02f, 0.02f);
					light.offset = actuator.offset + random.NextVector2Range(new Vector2(-0.08f, +0.08f), new Vector2(-0.02f, +0.02f));
				}

				//for (var i = 0u; i < 1u; i++)
				//{
				//	Particle.Spawn(ref region, new Particle.Data()
				//	{
				//		texture = Light.tex_light_circle_00,
				//		lifetime = 0.20f,
				//		pos = pos,
				//		//vel = dir * 30.00f,
				//		drag = 0.10f,
				//		frame_count = 1,
				//		frame_count_total = 1,
				//		frame_offset = 0,
				//		scale = 0.70f,
				//		//stretch = new Vector2(1.00f, 0.30f),
				//		//face_dir_ratio = 1.00f,
				//		growth = 100.00f,
				//		color_a = color_a,
				//		color_b = color_b,
				//		glow = 4.00f * intensity
				//	});
				//}
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSound(ISystem.Info info,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Motor.Data motor, [Source.Owned] ref Motor.State motor_state,
		[Source.Owned] in Actuator.Data actuator,
		[Source.Owned, Pair.Component<Actuator.Data>] ref Sound.Emitter sound_emitter)
		{
			//var ratio = Maths.Normalize(motor_state.force_applied, motor_state.force_available);
			//var ratio = Maths.Normalize(motor_state.force_applied, motor_state.force_available * App.fixed_update_interval_s);
			//var ratio = Maths.Normalize(motor_state.speed_current * 2.00f, motor.speed_max);

			//var ratio_a = Maths.Normalize(motor_state.force_applied, motor_state.force_available);
			//var ratio_b = Maths.Normalize(motor_state.speed_current * 1.80f, motor.speed_max);
			//var ratio = Maths.Min(ratio_a, ratio_b);

			//var ratio = Maths.Max(motor_state.power_target_ratio, Maths.Avg(motor_state.utilization_ratio + motor_state.power_target_ratio, motor_state.power_target_ratio) * motor_state.power_target_ratio);
			//var intensity = Maths.Clamp(ratio * 1.50f, 0.00f, motor_state.power_target_ratio * 2);
			var intensity = motor_state.intensity_current;
			sound_emitter.volume_mult.MoveTowardsDamped(Maths.Lerp01(0.00f, 1.20f, intensity * 0.50f), 0.15f, 0.02f);
			sound_emitter.pitch_mult.MoveTowardsDamped(Maths.Lerp01(0.25f, 1.20f, intensity * 0.35f), 0.08f, 0.03f);
		}
#endif
	}
}
