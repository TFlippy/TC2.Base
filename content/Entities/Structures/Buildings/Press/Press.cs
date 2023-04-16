namespace TC2.Base.Components
{
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

		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Press.State>]
		public partial struct Data: IComponent
		{
			public Vector2 slider_offset;
			public float slider_length;
			public float speed;
			public float load_multiplier;

			[Net.Ignore, Save.Ignore]
			public int current_sound_index;
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct State: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Smashed = 1 << 0,
				Success = 1 << 1
			}

			public Press.State.Flags flags;
		}

#if CLIENT
		private static readonly Texture.Handle bigger_smoke_light = "BiggerSmoke_Light";
		private static readonly Texture.Handle metal_spark_01 = "metal_spark.01";

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Press.Data press, [Source.Owned] ref Press.State press_state, [Source.Owned, Pair.Of<Press.Data>] ref Light.Data light, [Source.Owned] ref Crafter.State state)
		{
			if (press_state.flags.HasAny(Press.State.Flags.Smashed))
			{
				ref var region = ref info.GetRegion();

				var random = XorRandom.New();

				if (press_state.flags.HasAny(Press.State.Flags.Success))
				{
					//Sound.Play(snd_smash[(press.current_sound_index++) % snd_smash.Length], transform.position, volume: 1.00f, pitch: random.NextFloatRange(0.70f, 0.80f), size: 0.80f, priority: 0.60f, dist_multiplier: 0.70f);
					Sound.Play(snd_stamp[(press.current_sound_index++) % snd_stamp.Length], transform.position, volume: 1.00f, pitch: random.NextFloatRange(0.95f, 1.05f), size: 1.00f, priority: 0.60f, dist_multiplier: 0.70f);
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
					Sound.Play(snd_fail[(press.current_sound_index++) % snd_fail.Length], transform.position, volume: 1.20f, pitch: random.NextFloatRange(0.70f, 0.90f), size: 0.80f, priority: 0.60f, dist_multiplier: 0.80f);
					Shake.Emit(ref region, transform.position, 0.30f, 0.30f, radius: 16.00f);
				}

				press_state.flags.SetFlag(Press.State.Flags.Smashed | Press.State.Flags.Success, false);
			}
			else
			{
				light.intensity = Maths.Lerp(light.intensity, 0.00f, 0.10f);
			}
		}
#endif

		public static readonly Gradient<float> gradient_work = new Gradient<float>(0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 1.00f, 1.00f);

		[ISystem.Update.Modify(ISystem.Mode.Single)]
		public static void UpdateWheel(ISystem.Info info, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Press.Data press, [Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{
			//var t = MathF.Pow((MathF.Cos(axle_state.rotation) + 1.00f) * 0.50f, 1.50f);
			//if (MathF.Abs(axle_state.rotation) <= 1.00f)
			//{
			//	t = 1;
			//}

			var t = Maths.NormalizeClamp(MathF.Abs(axle_state.rotation), MathF.Tau);
			var val = gradient_work.GetValue(t);

			axle_state.new_tmp_load += val * press.load_multiplier * 200.00f;
		}

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Press.Data press, [Source.Owned] ref Press.State press_state, [Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state,
		[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{
			//if (crafter.recipe.id != 0)
			//{
			//	if (press.recipe_cached.id != crafter.recipe.id)
			//	{
			//		ref var recipe = ref crafter.GetCurrentRecipe();
			//		if (!recipe.IsNull())
			//		{
			//			var load = 0.00f;

			//			foreach (ref var requirement in recipe.requirements.AsSpan())
			//			{
			//				if (requirement.type == Crafting.Requirement.Type.Work && requirement.work == Work.Type.Pressing)
			//				{
			//					load += requirement.difficulty;
			//				}
			//			}

			//			press.load_multiplier = load;
			//		}

			//		press.recipe_cached = crafter.recipe;
			//	}
			//}

			switch (crafter_state.current_work_type)
			{
				case Work.Type.Pressing:
				{
					press.load_multiplier = crafter_state.current_work_difficulty;

					press_state.flags.SetFlag(Press.State.Flags.Success, false);

					if (axle_state.flags.HasAny(Axle.State.Flags.Revolved))
					{
						if (MathF.Abs(axle_state.angular_velocity) >= 1.00f)
						{
							crafter_state.work += 1.00f;
							press_state.flags.SetFlag(Press.State.Flags.Success, true);
						}
						else
						{
							crafter_state.work = 0.00f;
						}

						press_state.flags.SetFlag(Press.State.Flags.Smashed, true);

						axle_state.Sync(entity);
						//crafter_state.Sync(entity);
						press_state.Sync(entity);
					}
				}
				break;
			}
		}
#endif

#if CLIENT
		public struct PressGUI: IGUICommand
		{
			public Entity ent_press;
			public Transform.Data transform;

			public Press.Data press;
			public Press.State press_state;

			public Crafter.Data crafter;
			public Crafter.State crafter_state;

			public Axle.Data axle;
			public Axle.State axle_state;

			public static int load_graph_index;
			public static float[] load_graph = new float[App.tickrate * 3];

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Press", this.ent_press))
				{
					this.StoreCurrentWindowTypeID(order: -100);

					if (window.show)
					{
						ref var player = ref Client.GetPlayer();
						ref var region = ref Client.GetRegion();

						Crafting.Context.NewFromPlayer(ref Client.GetRegion(), ref player, ent_this: this.ent_press, out var context);

						var w_right = (48 * 4) + 24;

						using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth() - w_right, GUI.GetRemainingHeight())))
						{
							using (GUI.Group.New(size: GUI.GetRemainingSpace()))
							{
								GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

								using (GUI.Group.New(size: GUI.GetRemainingSpace(), padding: new(12, 12)))
								{
									//CrafterExt.DrawRecipe(ref region, ref this.crafter, ref this.crafter_state);
									CrafterExt.DrawRecipe(ref context, ref this.crafter, ref this.crafter_state);

									//ref var recipe = ref this.crafter.GetCurrentRecipe();
									//if (!recipe.IsNull())
									//{
									//	//ref var inventory_data = ref this.ent_press.GetTrait<Crafter.State, Inventory8.Data>();

									//	//GUI.DrawShopRecipe(ref region, this.crafter.recipe, this.ent_press, player.ent_controlled, this.transform.position, default, default, inventory_data.GetHandle(), draw_button: false, draw_title: false, draw_description: false, search_radius: 0.00f);
									//}

									using (GUI.Group.New(size: GUI.GetRemainingSpace(), padding: new(12, 12)))
									{
										GUI.DrawFillBackground(GUI.tex_panel, new(8, 8, 8, 8), margin: new(-12, 0, -12, -12));

										load_graph[load_graph_index] = this.axle_state.new_tmp_load;
										GUI.LineGraph("##load", load_graph, ref load_graph_index, size: GUI.GetRemainingSpace(), scale_min: 0.00f, scale_max: 10000.00f);
									}
								}
							}
						}

						GUI.SameLine();

						using (GUI.Group.New(size: new Vector2(w_right, GUI.GetRemainingHeight())))
						{
							using (GUI.Group.New(size: new Vector2(48 * 4, 48 * 2) + new Vector2(24, 24)))
							{
								GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

								using (GUI.Group.New(padding: new(12, 12)))
								{
									GUI.DrawInventoryDock(Inventory.Type.Output, size: new(48 * 4, 48 * 2));
								}
							}

							using (GUI.Group.New(size: new Vector2(48 * 4, GUI.GetRemainingHeight()) + new Vector2(24, 0)))
							{
								GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

								using (GUI.Group.New(padding: new(12, 12)))
								{
									GUI.DrawInventoryDock(Inventory.Type.Input, size: new(48 * 4, 48 * 2));
									//GUI.DrawWorkH(Maths.Normalize(this.crafter_state.work, this.crafter.required_work), size: GUI.GetRemainingSpace() with { Y = 32 } - new Vector2(48, 0));
								}
							}
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUI(Entity entity, [Source.Owned] in Transform.Data transform, 
		[Source.Owned] in Press.Data press, [Source.Owned] in Press.State press_state, 
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state,
		[Source.Owned] in Axle.Data axle, [Source.Owned] in Axle.State axle_state,
		[Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
			{
				var gui = new PressGUI()
				{
					ent_press = entity,

					transform = transform,

					press = press,
					press_state = press_state,

					crafter = crafter,
					crafter_state = crafter_state,

					axle = axle,
					axle_state = axle_state
				};
				gui.Submit();
			}
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Press.Data press, [Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state,
		[Source.Owned, Pair.Of<Press.Data>] ref Animated.Renderer.Data renderer_slider)
		{
			renderer_slider.offset = press.slider_offset + new Vector2(0.00f, MathF.Pow((MathF.Cos(axle_state.rotation) + 1.00f) * 0.50f, press.speed) * press.slider_length);
		}
#endif
	}
}
