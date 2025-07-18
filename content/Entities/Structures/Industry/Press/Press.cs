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
			[Net.Segment.A, Save.Force] public Volume cylinder_volume;
			//public Sound.Handle h_sound;

			[Net.Segment.C, Asset.Ignore] public Piston.Status status;
			[Net.Segment.C, Asset.Ignore] private byte unused_c_00;
			[Net.Segment.C] public Piston.Flags flags;
			[Net.Segment.C, Asset.Ignore] public float current_distance;
			[Net.Segment.C, Asset.Ignore] private float unused_c_01;
			[Net.Segment.C, Asset.Ignore] private float unused_c_02;

			[Net.Segment.D, Asset.Ignore] public float current_force;
			[Net.Segment.D, Asset.Ignore] public float prev_speed;
			[Net.Segment.D, Asset.Ignore] public float current_speed;
			[Net.Segment.D, Asset.Ignore] public Energy current_kinetic_energy;
		}

		[ISystem.PostUpdate.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateRenderer(ISystem.Info info,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Piston.Data piston,
		[Source.Owned, Pair.Component<Piston.Data>] ref Animated.Renderer.Data renderer)
		{
			renderer.offset = piston.offset.AddY(piston.current_distance);
			//renderer_slider.offset = press.slider_offset + new Vector2(0.00f, MathF.Pow((MathF.Cos(axle_state.rotation) + 1.00f) * 0.50f, press.speed) * press.slider_length);
		}

		[ISystem.PostUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_piston,
		[Source.Owned] in Transform.Data transform, /*[Source.Owned] ref Control.Data control,*/
		[Source.Owned] ref Piston.Data piston/*, [Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state*/)
		{
			//App.WriteLine("press");

			//region.Schedule((ref region) =>
			//{
			//	ref var control = ref ent_piston.GetComponent<Control.Data>();
			//	if (control.mouse.GetKeyDown(Mouse.Key.Left))
			//	{
			//		App.WriteLine("press pressed", color: App.Color.Magenta);
			//	}
			//});

			var distance_old = piston.current_distance;
			var distance_new = Maths.FMA(piston.current_speed, App.fixed_update_interval_s_f32, distance_old);

			piston.current_kinetic_energy = 0.00f;

			piston.prev_speed = piston.current_speed;


			if (piston.status == Status.Impacted)
			{
				piston.status = Status.Retracting;
				piston.flags.RemoveFlag(Flags.Impacted);
			}
			else if (distance_new > piston.length)
			{
				var energy_impact = Energy.GetKineticEnergy(piston.mass, piston.current_speed);
				//App.WriteValue(piston.current_speed);

				//App.WriteValue(energy_impact);
				piston.current_speed *= -0.10f;
				piston.current_kinetic_energy += energy_impact;
				//piston.current_distance = piston.length;
				distance_new = piston.length;

				piston.flags.AddFlag(Flags.Impacted);
				piston.status = Status.Impacted;
			}
			else if (distance_new < 0.00f)
			{
				piston.current_speed *= -0.10f; // -0.50f;
												//piston.current_distance = 0.00f;
				distance_new = 0.00f;
			}
			else
			{
				piston.current_speed *= piston.damping;
				piston.current_speed -= piston.current_distance; // * info.DeltaTime;
				piston.status = Status.Idle;
				//piston.current_kinetic_energy = 0.00f;
			}

			//crafter_state.flags.AddFlag(Crafter.State.Flags.Cycled);

			//var distance_new = Maths.FMA(piston.current_speed, App.fixed_update_interval_s_f32, distance_tmp);

			piston.current_distance = distance_new;

			//piston.current_speed *= 0.92f;
		}

		[ISystem.Event<Essence.PulseEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnEssencePulseEvent(ref Region.Data region, ISystem.Info info, Entity ent_piston, ref Essence.PulseEvent ev,
		[Source.Owned] in Transform.Data transform, /*[Source.Owned] ref Control.Data control,*/
		[Source.Owned] ref Piston.Data piston, [Source.Owned, Pair.Component<Piston.Data>] ref Essence.Emitter.Data essence_emitter
		/*[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state*/)
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

		[ISystem.PreUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void PostUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_piston,
		[Source.Owned] in Transform.Data transform, /*[Source.Owned] ref Control.Data control,*/
		[Source.Owned] ref Piston.Data piston,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{
			//#if SERVER
			//			if (control.mouse.GetKey(Mouse.Key.Left))
			//			{
			//				App.WriteLine("press pressed", color: App.Color.Magenta);


			//				piston.current_speed = 25.00f;

			//				piston.Sync(ent_piston, true);
			//			}
			//#endif

			//App.WriteLine("press");
		}
	}

	public static partial class Press
	{
		public static readonly Sound.Handle[] snd_stamp =
		{
			"press.stamp.00",
			"press.stamp.01",
			"press.stamp.02"
		};

		public static readonly Sound.Handle[] snd_smash =
		{
			"press.smash.00",
			"press.smash.01",
			"press.smash.02"
		};

		public static readonly Sound.Handle[] snd_fail =
		{
			"press.fail.00",
			"press.fail.01",
			"press.fail.02"
		};

		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region), IComponent.With<Press.State>]
		public partial struct Data(): IComponent
		{
			[Net.Segment.A, Editor.Picker.Position(relative: true)] public Vec2f slider_offset;

			//[Net.Segment.A] public float slider_length;
			//[Net.Segment.A] public float speed;
			[Net.Segment.A] public float load_multiplier;

			[Net.Segment.A] public ISoundMix.Handle h_soundmix_stamp;
			[Net.Segment.A] public ISoundMix.Handle h_soundmix_fail;

			//[Net.Ignore, Save.Ignore] public uint current_sound_index;
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct State(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Smashed = 1 << 0,
				Success = 1 << 1
			}

			public Press.State.Flags flags;
			[Net.Ignore, Save.Ignore] public uint current_sound_index;
		}

#if CLIENT
		private static readonly Texture.Handle bigger_smoke_light = "BiggerSmoke_Light";
		private static readonly Texture.Handle metal_spark_01 = "metal_spark.01";

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateEffects(ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Press.Data press, [Source.Owned] ref Press.State press_state,
		[Source.Owned, Pair.Component<Press.Data>] ref Light.Data light, [Source.Owned] ref Crafter.State crafter_state)
		{
			if (press_state.flags.HasAny(Press.State.Flags.Smashed))
			{
				if (press_state.flags.HasAny(Press.State.Flags.Success))
				{
					//Sound.Play(snd_smash[(press.current_sound_index++) % snd_smash.Length], transform.position, volume: 1.00f, pitch: random.NextFloatRange(0.70f, 0.80f), size: 0.80f, priority: 0.60f, dist_multiplier: 0.70f);
					Sound.Play(sound: snd_stamp.GetNext(ref press_state.current_sound_index), world_position: transform.position, volume: 0.60f, pitch: random.NextFloatExtra(0.95f, 0.10f), size: 4.00f, priority: 0.20f, dist_multiplier: 0.55f);
					Shake.Emit(region: ref region, world_position: transform.position, trauma: 0.30f, max: 0.35f, radius: 14.00f);

					light.intensity = 1.00f;

					var pos = transform.LocalToWorld(light.offset);
					var dir = transform.GetDirection();

					{
						ref var particle = ref Particle.Reserve(ref region);

						particle.texture = bigger_smoke_light;
						particle.lifetime = random.NextFloatRange(0.50f, 1.00f);
						particle.pos = pos + random.NextVector2(0.50f);
						particle.vel = random.NextVector2Range(-1.00f, 1.00f);
						particle.fps = random.NextByteRange(5, 10);
						particle.frame_count = 64;
						particle.frame_count_total = 64;
						particle.frame_offset = random.NextByteRange(0, 64);
						particle.scale = random.NextFloatRange(0.50f, 0.80f);
						particle.rotation = random.NextFloat(10.00f);
						particle.angular_velocity = random.NextFloatRange(-1.00f, 1.00f);
						particle.growth = random.NextFloatRange(0.40f, 0.60f);
						particle.drag = random.NextFloatRange(0.01f, 0.03f);
						particle.force = new Vector2(0.00f, random.NextFloatRange(0, -2));
						particle.color_a = random.NextColor32Range(0x80ffffff, 0xa0ffffff);
						particle.color_b = random.NextColor32Range(0x00ffffff, 0x00808080);

					}

					//for (var i = 0; i < 10; i++)
					//{
					//	Particle.Spawn(ref region, new Particle.Data()
					//	{
					//		texture = metal_spark_01,
					//		lifetime = random.NextFloatRange(0.20f, 0.80f),
					//		pos = transform.LocalToWorld(light.offset + new Vector2(random.NextFloatRange(-0.50f, 0.50f), 0)),
					//		vel = (dir * random.NextFloatRange(0, 25) * (i % 2 == 0 ? 1.00f : -1.00f)).RotateByRad(random.NextFloatRange(-0.10f, 0.10f)) + random.NextVector2(5),
					//		fps = 0,
					//		frame_count = 1,
					//		frame_offset = random.NextByteRange(0, 4),
					//		frame_count_total = 4,
					//		scale = random.NextFloatRange(0.30f, 0.40f),
					//		growth = -random.NextFloatRange(0.60f, 1.00f),
					//		force = new Vector2(0.00f, random.NextFloatRange(20, 60)),
					//		drag = random.NextFloatRange(0.01f, 0.05f),
					//		stretch = new Vector2(random.NextFloatRange(2.00f, 3.00f), 0.75f),
					//		color_a = random.NextColor32Range(0xffffffff, 0xffffc0b0),
					//		color_b = random.NextColor32Range(0xffffc0b0, 0xffffa090),
					//		lit = 1.00f,
					//		face_dir_ratio = 1.00f
					//	});
					//}
				}
				else
				{
					Sound.Play(sound: snd_fail.GetNext(ref press_state.current_sound_index), world_position: transform.position, volume: 1.20f, pitch: random.NextFloatRange(0.70f, 0.90f), size: 0.80f, priority: 0.60f, dist_multiplier: 0.80f);
					Shake.Emit(region: ref region, world_position: transform.position, trauma: 0.30f, max: 0.30f, radius: 16.00f);
				}

				press_state.flags.RemoveFlag(Press.State.Flags.Smashed | Press.State.Flags.Success);
			}
			else
			{
				light.intensity = Maths.Lerp(light.intensity, 0.00f, 0.15f);
			}
		}
#endif

		//public static readonly Gradient<float> gradient_work = new(0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 1.00f, 1.00f);

		//[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateWheel(ISystem.Info info, Entity entity,
		//[Source.Owned] in Transform.Data transform, [Source.Owned] ref Press.Data press, [Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		//{
		//	//var t = MathF.Pow((MathF.Cos(axle_state.rotation) + 1.00f) * 0.50f, 1.50f);
		//	//if (MathF.Abs(axle_state.rotation) <= 1.00f)
		//	//{
		//	//	t = 1;
		//	//}

		//	var t = Maths.NormalizeClamp(MathF.Abs(axle_state.rotation), MathF.Tau);
		//	var val = gradient_work.GetValue(t);

		//	axle_state.force_load_new += val * press.load_multiplier * 200.00f;
		//}

		//public static float CalculateVelocity(Energy kinetic_energy, Mass mass)
		//{
		//	var velocity = Maths.Sqrt(2.00f * kinetic_energy / mass);
		//	return velocity;
		//}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Piston(/*ISystem.Info info, ref Region.Data region, ref XorRandom random, */Entity ent_press,
		//[Source.Owned] in Transform.Data transform, [Source.Owned] ref Control.Data control,
		[Source.Owned] ref Press.Data press, [Source.Owned] ref Press.State press_state, [Source.Owned] ref Piston.Data piston,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{
			if (piston.flags.HasAny(Piston.Flags.Impacted))
			{
#if SERVER
				//var amount = piston.current_kinetic_energy;
				////amount = Maths.NormalizeFast(amount, 16);
				////amount *= App.fixed_update_interval_s_f32;
				//amount *= 2;
				//amount /= piston.mass;
				//amount = Maths.Sqrt(amount);


				//var energy = piston.current_kinetic_energy;
				//var mass = piston.mass;
				//var velocity = Maths.Sqrt(2.00f * energy / mass);

				var velocity = Energy.GetVelocity(piston.current_kinetic_energy, piston.mass);
				var amount = velocity * piston.mass;
				//var mass = Energy.GetMass(piston.current_kinetic_energy, 25);
				//App.WriteValue(velocity);

				scoped var ev = new Crafter.WorkEvent(amount: amount, 
				calculate_work: static (ref Region.Data.Common region, Vec2f pos, ref Crafter.WorkEvent ev, 
				ref ICrafter.ModeInfo mode, ref Crafting.Order order, ref Crafting.Requirement req, ref IWork.Data work) =>
				{
					return Maths.NormalizeFast(ev.amount, req.difficulty);
				});

				//var ts = Timestamp.Now();
				ev.Trigger(ent_press, h_component: IComponent.Handle.FromComponent<Crafter.Data>());
				//crafter_state.flags.AddFlag(Crafter.State.Flags.In_Contact | Crafter.State.Flags.Ready);
				//var ts_elapsed = ts.GetMilliseconds();

				//App.WriteLine($"{ts_elapsed:0.00000} ms");

				//press_state.flags.AddFlag(State.Flags.Smashed | State.Flags.Success);
				//press_state.Sync(ent_press, true);
				//App.WriteLine("smash");
#endif
			}
			else
			{
				//crafter_state.flags.RemoveFlag(Crafter.State.Flags.In_Contact | Crafter.State.Flags.Ready);
			}
		}

#if CLIENT
		public struct PressGUI: IGUICommand
		{
			public Entity ent_press;
			public Transform.Data transform;

			public Press.Data press;
			public Press.State press_state;

			public Crafter.Data crafter;
			public Crafter.State crafter_state;

			//public Axle.Data axle;
			//public Axle.State axle_state;

			public static uint load_graph_index;
			public static float[] load_graph = new float[App.tickrate * 3];

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc(title: "Press"u8, entity: this.ent_press, size: new(48 * 3, 96), show_misc: true))
				{
					this.StoreCurrentWindowTypeID(order: 100);
					if (window.show)
					{
						GUI.DrawFillBackground(GUI.tex_frame, new(8));
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity ent_press, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Press.Data press, [Source.Owned] in Press.State press_state,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state,
		[Source.Owned] in Interactable.Data interactable)
		{
			return;

			if (interactable.IsActive())
			{
				var gui = new PressGUI()
				{
					ent_press = ent_press,

					transform = transform,

					press = press,
					press_state = press_state,

					crafter = crafter,
					crafter_state = crafter_state,

					//axle = axle,
					//axle_state = axle_state
				};
				gui.Submit();
			}
		}

		//[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void OnUpdate(ISystem.Info info,
		//[Source.Owned] in Transform.Data transform,
		//[Source.Owned] ref Press.Data press, // [Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state,
		//[Source.Owned, Pair.Component<Press.Data>] ref Animated.Renderer.Data renderer_slider)
		//{
		//	//renderer_slider.offset = press.slider_offset + new Vector2(0.00f, MathF.Pow((MathF.Cos(axle_state.rotation) + 1.00f) * 0.50f, press.speed) * press.slider_length);
		//}
#endif
	}
}
