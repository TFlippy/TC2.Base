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
			[Net.Segment.D, Asset.Ignore] public float current_speed;
			[Net.Segment.D, Asset.Ignore] public Pressure current_pressure;
			[Net.Segment.D, Asset.Ignore] public Energy current_kinetic_energy;
		}

		[ISystem.PostUpdate.E(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateRenderer(ISystem.Info info,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Piston.Data piston,
		[Source.Owned, Pair.Component<Piston.Data>] ref Animated.Renderer.Data renderer)
		{
			renderer.offset = piston.offset.AddY(piston.current_distance);
			//renderer_slider.offset = press.slider_offset + new Vector2(0.00f, MathF.Pow((MathF.Cos(axle_state.rotation) + 1.00f) * 0.50f, press.speed) * press.slider_length);
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
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
			var distance_tmp = distance_old;

			piston.current_kinetic_energy = 0.00f;

			if (piston.status == Status.Impacted)
			{
				piston.status = Status.Retracting;
				piston.flags.RemoveFlag(Flags.Impacted);
			}
			else if (distance_old > piston.length)
			{
				var energy_impact = Energy.GetKineticEnergy(piston.mass, piston.current_speed);
				
				//App.WriteValue(energy_impact);
				piston.current_speed *= -0.10f;
				piston.current_kinetic_energy += energy_impact;
				//piston.current_distance = piston.length;
				distance_tmp = piston.length;

				piston.flags.AddFlag(Flags.Impacted);
				piston.status = Status.Impacted;
			}
			else if (distance_old < 0.00f)
			{
				piston.current_speed *= -0.30f; // -0.50f;
												//piston.current_distance = 0.00f;
				distance_tmp = 0.00f;
			}
			else
			{
				piston.current_speed -= piston.current_distance; // * info.DeltaTime;
				piston.status = Status.Idle;
				//piston.current_kinetic_energy = 0.00f;
			}

			//crafter_state.flags.AddFlag(Crafter.State.Flags.Cycled);

			var distance_new = Maths.FMA(piston.current_speed, App.fixed_update_interval_s_f32, distance_tmp);
			piston.current_distance = distance_new;

			piston.current_speed *= 0.92f;
		}

		[ISystem.Event<EssenceNode.CollapseEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnEssencePulseEvent(ref Region.Data region, ISystem.Info info, Entity ent_piston, ref Essence.PulseEvent ev,
		[Source.Owned] in Transform.Data transform, /*[Source.Owned] ref Control.Data control,*/
		[Source.Owned] ref Piston.Data piston, [Source.Owned, Pair.Component<Piston.Data>] ref Essence.Emitter.Data essence_emitter
		/*[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state*/)
		{
			App.WriteLine("essence pulse event", color: App.Color.Magenta);
		}

		[ISystem.PreUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Signal(ISystem.Info info, ref XorRandom random,
		IComponent.Handle<Essence.Emitter.Data> h_essence_emitter, Entity ent_essence_emitter,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Analog.Relay.Data analog_relay,
		[Source.Owned, Pair.Wildcard] ref Essence.Emitter.Data essence_emitter)
		{
			var signal_value = analog_relay.signal_current[essence_emitter.channel_emit];
			essence_emitter.flags.SetFlag(Essence.Emitter.Flags.Pulsed, signal_value > 0.10f);
		}

		[ISystem.PostUpdate.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Essence(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_piston,
		[Source.Owned] in Transform.Data transform, /*[Source.Owned] ref Control.Data control,*/
		[Source.Owned] ref Piston.Data piston, [Source.Owned, Pair.Component<Piston.Data>] ref Essence.Emitter.Data essence_emitter
		/*[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state*/)
		{
			//if (control.mouse.GetKey(Mouse.Key.Left))

			if (essence_emitter.flags.HasAny(Essence.Emitter.Flags.Pulsed))
			{
				//App.WriteLine("press pressed", color: App.Color.Magenta);


#if SERVER
				piston.current_speed = 25.00f;
				piston.Sync(ent_piston, true);
			
#endif

#if CLIENT
				var h_essence = essence_emitter.h_essence; // new IEssence.Handle("motion");
				ref var essence_data = ref h_essence.GetData();
				if (essence_data.IsNotNull())
				{
					var pos = transform.LocalToWorld(essence_emitter.offset);
					var dir = transform.LocalToWorldDirection(essence_emitter.direction);

					var intensity = 1.00f;
					var color_a = ColorBGRA.Lerp(essence_data.color_emit, ColorBGRA.White, 0.50f);
					var color_b = essence_data.color_emit.WithColorMult(0.20f).WithAlphaMult(0.00f);

					//App.WriteLine("essence");

					//Sound.Play(region: ref region, sound: essence_emitter.h_sound_emit, world_position: pos, volume: 1.00f, pitch: 1.00f, size: 0.35f, dist_multiplier: 0.65f);
					Sound.Play(region: ref region, h_soundmix: essence_emitter.h_soundmix_test, random: ref random, pos: pos); //, volume: 1.00f, pitch: 1.00f, size: 0.35f, dist_multiplier: 0.65f);
					Shake.Emit(region: ref region, world_position: pos, trauma: 0.60f, max: 0.60f, radius: 10.00f);

					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = Light.tex_light_circle_00,
						lifetime = 0.20f,
						pos = pos - dir,
						vel = dir * 20.00f,
						drag = 0.20f,
						frame_count = 1,
						frame_count_total = 1,
						frame_offset = 0,
						scale = 1.00f,
						stretch = new Vector2(1.00f, 0.50f),
						face_dir_ratio = 1.00f,
						growth = 50.00f,
						color_a = color_a,
						color_b = color_b,
						glow = 20.00f * intensity
					});

					//Particle.Spawn(ref region, new Particle.Data()
					//{
					//	texture = Light.tex_light_circle_04,
					//	lifetime = random.NextFloatRange(1.00f, 1.25f),
					//	pos = data.world_position + (dir * 0.50f),
					//	scale = random.NextFloatRange(1.00f, 1.50f),
					//	growth = random.NextFloatRange(1.50f, 2.00f),
					//	stretch = new Vector2(0.90f, 0.60f),
					//	rotation = dir.GetAngleRadiansFast(),
					//	face_dir_ratio = 1.00f,
					//	color_a = new Vector4(1.00f, 0.70f, 0.40f, 30.00f),
					//	color_b = new Vector4(0.20f, 0.00f, 0.00f, 0.00f),
					//	glow = 1.00f
					//});
				}
#endif
			}
		}

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

			[Net.Segment.A] public float slider_length;
			[Net.Segment.A] public float speed;
			[Net.Segment.A] public float load_multiplier;

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
		public static void OnUpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Press.Data press, [Source.Owned] ref Press.State press_state,
		[Source.Owned, Pair.Component<Press.Data>] ref Light.Data light, [Source.Owned] ref Crafter.State state)
		{
			if (press_state.flags.HasAny(Press.State.Flags.Smashed))
			{
				if (press_state.flags.HasAny(Press.State.Flags.Success))
				{
					//Sound.Play(snd_smash[(press.current_sound_index++) % snd_smash.Length], transform.position, volume: 1.00f, pitch: random.NextFloatRange(0.70f, 0.80f), size: 0.80f, priority: 0.60f, dist_multiplier: 0.70f);
					Sound.Play(snd_stamp.GetNext(ref press_state.current_sound_index), transform.position, volume: 1.00f, pitch: random.NextFloatRange(0.95f, 1.05f), size: 1.00f, priority: 0.60f, dist_multiplier: 0.70f);
					Shake.Emit(ref region, transform.position, 0.40f, 0.40f, radius: 24.00f);

					light.intensity = 1.00f;

					var pos = transform.LocalToWorld(light.offset);
					var dir = transform.GetDirection();

					{
						Particle.Spawn(ref region, new Particle.Data()
						{
							texture = bigger_smoke_light,
							lifetime = random.NextFloatRange(0.50f, 1.00f),
							pos = pos + random.NextVector2(0.50f),
							vel = random.NextVector2Range(-1.00f, 1.00f),
							fps = random.NextByteRange(5, 10),
							frame_count = 64,
							frame_count_total = 64,
							frame_offset = random.NextByteRange(0, 64),
							scale = random.NextFloatRange(0.50f, 0.80f),
							rotation = random.NextFloat(10.00f),
							angular_velocity = random.NextFloatRange(-1.00f, 1.00f),
							growth = random.NextFloatRange(0.40f, 0.60f),
							drag = random.NextFloatRange(0.01f, 0.03f),
							force = new Vector2(0.00f, random.NextFloatRange(0, -2)),
							color_a = random.NextColor32Range(0x80ffffff, 0xa0ffffff),
							color_b = random.NextColor32Range(0x00ffffff, 0x00808080)
						});
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
					Sound.Play(snd_fail.GetNext(ref press_state.current_sound_index), transform.position, volume: 1.20f, pitch: random.NextFloatRange(0.70f, 0.90f), size: 0.80f, priority: 0.60f, dist_multiplier: 0.80f);
					Shake.Emit(ref region, transform.position, 0.30f, 0.30f, radius: 16.00f);
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

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Piston(/*ISystem.Info info, ref Region.Data region, ref XorRandom random, */Entity ent_press,
		//[Source.Owned] in Transform.Data transform, [Source.Owned] ref Control.Data control,
		[Source.Owned] ref Press.Data press, [Source.Owned] ref Press.State press_state, [Source.Owned] ref Piston.Data piston,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{
			if (piston.flags.HasAny(Piston.Flags.Impacted))
			{
#if SERVER
				crafter_state.flags.AddFlag(Crafter.State.Flags.In_Contact | Crafter.State.Flags.Ready);

				press_state.flags.AddFlag(State.Flags.Smashed | State.Flags.Success);
				press_state.Sync(ent_press, true);
				//App.WriteLine("smash");
#endif
			}
			else
			{
				crafter_state.flags.RemoveFlag(Crafter.State.Flags.In_Contact | Crafter.State.Flags.Ready);
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

						//ref var player = ref Client.GetPlayer();
						//ref var region = ref Client.GetRegion();
						//ref var character = ref Client.GetCharacter(out var character_asset);

						//Crafting.Context.NewFromCharacter(ref region.AsCommon(), character_asset, ent_producer: this.ent_press, out var context);

						//var w_right = (48 * 4) + 24;

						//using (GUI.Group.New(size: new Vector2(GUI.RmX - w_right, GUI.RmY)))
						//{
						//	using (GUI.Group.New(size: GUI.Rm))
						//	{
						//		GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

						//		using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
						//		{
						//			//CrafterExt.DrawRecipe(ref region, ref this.crafter, ref this.crafter_state);
						//			//CrafterExt.DrawRecipe(ref context, ref this.crafter, ref this.crafter_state);

						//			//ref var recipe = ref this.crafter.GetCurrentRecipe();
						//			//if (!recipe.IsNull())
						//			//{
						//			//	//ref var inventory_data = ref this.ent_press.GetTrait<Crafter.State, Inventory8.Data>();

						//			//	//GUI.DrawShopRecipe(ref region, this.crafter.recipe, this.ent_press, player.ent_controlled, this.transform.position, default, default, inventory_data.GetHandle(), draw_button: false, draw_title: false, draw_description: false, search_radius: 0.00f);
						//			//}

						//			//using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
						//			//{
						//			//	GUI.DrawFillBackground(GUI.tex_panel, new(8, 8, 8, 8), margin: new(-12, 0, -12, -12));

						//			//	load_graph[load_graph_index] = this.axle_state.force_load_old;
						//			//	GUI.LineGraph("##load", load_graph, ref load_graph_index, size: GUI.Rm, scale_min: 0.00f, scale_max: 10000.00f);
						//			//}
						//		}
						//	}
						//}

						//GUI.SameLine();

						//using (GUI.Group.New(size: new Vector2(w_right, GUI.RmY)))
						//{
						//	using (GUI.Group.New(size: new Vector2(48 * 4, 48 * 2) + new Vector2(24, 24), padding: new(12)))
						//	{
						//		GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

						//		ref var shipment = ref this.ent_press.GetComponent<Shipment.Data>();
						//		if (shipment.IsNotNull())
						//		{
						//			var item_context = GUI.ItemContext.Begin(false);
						//			GUI.DrawShipment(ref item_context, this.ent_press, ref shipment, new Vector2(96, 48));
						//		}
						//	}

						//	using (GUI.Group.New(size: new Vector2(48 * 4, 48 * 2) + new Vector2(24, 24)))
						//	{
						//		GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

						//		using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
						//		{
						//			GUI.DrawInventoryDock(Inventory.Type.Output, size: new(48 * 4, 48 * 2));
						//		}
						//	}

						//	using (GUI.Group.New(size: new Vector2(48 * 4, GUI.RmY) + new Vector2(24, 0)))
						//	{
						//		GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

						//		using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
						//		{
						//			GUI.DrawInventoryDock(Inventory.Type.Input, size: new(48 * 4, 48 * 2));
						//			//GUI.DrawWorkH(Maths.Normalize(this.crafter_state.work, this.crafter.required_work), size: GUI.Rm with { Y = 32 } - new Vector2(48, 0));
						//		}
						//	}
						//}

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

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Press.Data press, // [Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state,
		[Source.Owned, Pair.Component<Press.Data>] ref Animated.Renderer.Data renderer_slider)
		{
			//renderer_slider.offset = press.slider_offset + new Vector2(0.00f, MathF.Pow((MathF.Cos(axle_state.rotation) + 1.00f) * 0.50f, press.speed) * press.slider_length);
		}
#endif
	}
}
