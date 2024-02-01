
using Keg.Engine.Game;

namespace TC2.Base.Components
{
	public static partial class Tractor
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{
			//[Statistics.Info("Max Speed", description: "", format: "{0:0.##} m/s", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float speed = 10.00f;

			[Statistics.Info("Brake", description: "", format: "{0:0.##} rad/s", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Medium)]
			public float brake_step = 15.00f;

			[Statistics.Info("Acceleration", description: "", format: "{0:0.##} rad/s", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float speed_step = 10.00f;

			//public int gear = default;
			//public float gear_mod = default;

			[Editor.Picker.Position(relative: true)]
			public Vector2 smoke_offset;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State: IComponent
		{
			[Save.Ignore, Net.Ignore]
			public float current_motor_speed;

			[Save.Ignore, Net.Ignore]
			public float current_motor_force;

			[Save.Ignore, Net.Ignore]
			public float target_wheel_speed;

			public State()
			{

			}
		}

		public static readonly Sound.Handle[] clutch_sounds = new Sound.Handle[]
		{
			"tractor.clutch.00",
			"tractor.clutch.01",
			"tractor.clutch.02"
		};

		//[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateWheels(ISystem.Info info, [Source.Shared] in Tractor.Data tractor, [Source.Shared] in Tractor.State tractor_state, [Source.Owned] ref Wheel.Slot wheel_slot, [Source.Owned, Override] ref Joint.Wheel joint, [Source.Owned] ref Joint.Base joint_base)
		//{
		//	joint.speed = tractor_state.current_motor_speed;
		//	joint.force = tractor_state.current_motor_force * wheel_slot.force_multiplier;
		//}


		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateWheels(ISystem.Info info, [Source.Shared] in Tractor.Data tractor, [Source.Shared] in Tractor.State tractor_state, 
		[Source.Owned] ref Wheel.Slot wheel_slot, [Source.Owned, Override] ref Joint.Wheel joint, [Source.Owned] ref Joint.Base joint_base)
		{
			if (wheel_slot.flags.HasAll(Wheel.Flags.Has_Wheel))
			{
				joint.speed = tractor_state.current_motor_speed;
				joint.force = tractor_state.current_motor_force * wheel_slot.force_multiplier;
				joint.brake = 0.00f;
				if (joint.speed.Abs() > 0.01f) joint_base.state |= Joint.State.Roused;
			}
			else
			{
				joint.speed = 0.00f;
				joint.force = 0.00f;
				//joint.brake = joint_base.max_torque;
			}

			//App.WriteLine("w");
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("wrecked", false, Source.Modifier.Owned)]
		public static void UpdateControls(ISystem.Info info, Entity entity, ref Region.Data region, [Source.Owned] ref Transform.Data transform, 
		[Source.Owned] ref Tractor.Data tractor, [Source.Owned] ref Tractor.State tractor_state, [Source.Owned] in Control.Data control)
		{
			var speed = 0.00f;

			if (control.keyboard.GetKey(Keyboard.Key.MoveRight)) speed -= tractor.speed;
			if (control.keyboard.GetKey(Keyboard.Key.MoveLeft)) speed += tractor.speed;

			//if (control.keyboard.GetKeyDown(Keyboard.Key.Q)) tractor.gear = Maths.Clamp(tractor.gear - 1, 0, 4);
			//if (control.keyboard.GetKeyDown(Keyboard.Key.E)) tractor.gear = Maths.Clamp(tractor.gear + 1, 0, 4);

			var ratio = 1.00f; // * MathF.CopySign(1.00f, transform.scale.X); // + (tractor.gear * tractor.gear_mod);
			//if (control.keyboard.GetKey(Keyboard.Key.LeftShift)) speed *= 2.00f;

			tractor_state.target_wheel_speed = (speed != 0.00f ? speed : 0.00f) * ratio;
			//tractor_state.target_motor_force = (speed != 0.00f ? tractor.force : tractor.brake) / ratio;

#if SERVER
			if (control.keyboard.GetKeyDown(Keyboard.Key.Spacebar))
			{
				transform.scale.X *= -1.00f;
				transform.Modified(entity, true);
				//transform.Sync(entity, true);

				//Span<Entity> children = stackalloc Entity[64];
				//entity.GetAllChildren(ref children);
				//children.Reverse();

				//foreach (var ent_child in children)
				//{
				//	//ref var transform_child = ref ent_child.GetComponent<Transform.Data>();

				//	if (!ent_child.HasComponent<Joint.Base>())
				//	{
				//		ref var transform_child = ref ent_child.GetComponent<Transform.Data>();
				//		if (transform_child.IsNotNull())
				//		{
				//			App.WriteLine($"{ent_child}; {ent_child.GetName()}");

				//			transform_child.scale.X *= -1.00f;
				//			transform_child.Sync(ent_child, true);
				//		}
				//	}
				//}

			}
#endif
		}

		//[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateMotors(ISystem.Info info, [Source.Owned] ref Tractor.Data tractor, [Source.Owned] ref Tractor.State tractor_state, [Source.Owned] in Control.Data control)
		//{
		//	tractor_state.current_motor_force = Maths.MoveTowards(tractor_state.current_motor_force, tractor_state.target_motor_force, tractor.force_step);
		//	tractor_state.current_motor_speed = Maths.MoveTowards(tractor_state.current_motor_speed, tractor_state.target_motor_speed, tractor.speed_step);
		//}

		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateEngine1(ISystem.Info info, [Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Tractor.Data tractor, [Source.Owned] ref Tractor.State tractor_state,
		[Source.Owned] in Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{
			tractor_state.current_motor_force = axle_state.sum_torque;
			tractor_state.current_motor_speed = Maths.Clamp(Maths.MoveTowards(tractor_state.current_motor_speed, tractor_state.target_wheel_speed, ((Maths.SignEquals(tractor_state.current_motor_speed, tractor_state.target_wheel_speed) || MathF.Abs(tractor_state.target_wheel_speed) < 0.01f) ? tractor.speed_step : tractor.brake_step) * info.DeltaTime), -MathF.Abs(axle_state.angular_velocity), MathF.Abs(axle_state.angular_velocity));
		}

		//[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("wrecked", false, Source.Modifier.Owned)]
		//public static void UpdateEngine2(ISystem.Info info,
		//[Source.Owned] ref Tractor.Data tractor, [Source.Owned] ref Tractor.State tractor_state,
		//[Source.Owned] ref SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state, [Source.Owned] in Control.Data control)
		//{
		//	if (control.keyboard.GetKey(Keyboard.Key.MoveUp))
		//	{
		//		steam_engine.speed_target = Maths.MoveTowards(steam_engine.speed_target, steam_engine.speed_max, 10.00f * info.DeltaTime);
		//	}
		//	else if (control.keyboard.GetKey(Keyboard.Key.MoveDown))
		//	{
		//		steam_engine.speed_target = Maths.MoveTowards(steam_engine.speed_target, 0.00f, 10.00f * info.DeltaTime); 
		//	}

		//	//steam_engine.speed_target = Maths.Lerp(steam_engine.speed_target, MathF.Abs(tractor_state.target_motor_speed), 0.20f);
		//	//steam_engine.speed_target = Maths.MoveTowards(steam_engine.speed_target, steam_engine.speed_max, tractor.speed); // steam_engine.speed_target + speed, steam_engine.speed_max)
		//}

		//[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("wrecked", false, Source.Modifier.Parent)]
		//public static void UpdateEngine2Parent(ISystem.Info info,
		//[Source.Parent] ref Tractor.Data tractor, [Source.Parent] ref Tractor.State tractor_state,
		//[Source.Owned] ref SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state, [Source.Parent] in Control.Data control) => UpdateEngine2(info, ref tractor, ref tractor_state, ref steam_engine, ref steam_engine_state, in control);


#if CLIENT
		public static readonly Sound.Handle sound_stall = "Truck_Engine_Stall";
		public static readonly Texture.Handle texture_smoke = "SmallSmoke";

		//[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateSound(ISystem.Info info, [Source.Owned] ref Tractor.Data tractor, [Source.Owned] ref Tractor.State tractor_state, 
		//[Source.Owned, Pair.Of<Tractor.Data>] ref Sound.Emitter sound_emitter, [Source.Owned] in Transform.Data transform)
		//{
		//	var ratio = 1.00f + (tractor.gear * tractor.gear_mod);

		//	sound_emitter.volume = Maths.Lerp(sound_emitter.volume, Maths.Clamp(MathF.Abs(tractor_state.target_motor_speed), 0.40f, 1.00f), 0.02f);
		//	sound_emitter.pitch = 0.80f + (MathF.Abs(tractor_state.current_motor_speed / tractor.speed / ratio) * 0.20f);

		//	if (info.WorldTime > tractor_state.next_smoke)
		//	{
		//		var random = XorRandom.New();
		//		ref var region = ref info.GetRegion();

		//		tractor_state.next_smoke = info.WorldTime + 0.20f - MathF.Abs(tractor_state.current_motor_speed * 0.02f);

		//		Particle.Spawn(ref region, new Particle.Data()
		//		{
		//			texture = texture_smoke,
		//			lifetime = 0.50f + random.NextFloatRange(0.00f, 0.30f),
		//			pos = transform.LocalToWorld(tractor.smoke_offset) + random.NextVector2(0.20f),
		//			vel = new Vector2(0.00f, -2.00f) + (-transform.GetDirection() * (3.00f + MathF.Abs(tractor_state.current_motor_speed * 0.50f))),
		//			fps = 16,
		//			frame_count = 6,
		//			frame_offset = random.NextByteRange(0, 8),
		//			frame_count_total = 6,
		//			scale = 1.00f,
		//			rotation = random.NextFloat(MathF.PI),
		//			angular_velocity = random.NextFloat(MathF.PI),
		//			growth = 1.50f,
		//			color_a = new Color32BGRA(255, 80, 70, 50),
		//			color_b = new Color32BGRA(0, 80, 70, 50),
		//			drag = 0.10f
		//		});
		//	}
		//}
#endif

//#if CLIENT
//		public struct TractorGUI: IGUICommand
//		{
//			public Entity ent_tractor;

//			public Transform.Data transform;

//			public void Draw()
//			{
//				using (var window = GUI.Window.Interaction("Tractor", this.ent_tractor))
//				{
//					this.StoreCurrentWindowTypeID(order: -100);

//					ref var player = ref Client.GetPlayer();
//					ref var region = ref Client.GetRegion();

//					using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
//					{

//					}
//				}
//			}
//		}

//		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
//		public static void OnGUI(Entity entity, [Source.Owned] in Transform.Data transform, [Source.Owned] in Tractor.Data tractor, [Source.Owned] in Interactable.Data interactable)
//		{
//			if (interactable.show)
//			{
//				var gui = new TractorGUI()
//				{
//					ent_tractor = entity,

//					transform = transform
//				};
//				gui.Submit();
//			}
//		}
//#endif
	}
}
