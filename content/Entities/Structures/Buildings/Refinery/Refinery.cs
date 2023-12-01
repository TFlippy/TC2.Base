namespace TC2.Base.Components
{
	public static partial class Refinery
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true), IComponent.With<Refinery.State>]
		public struct Data: IComponent
		{
			public Vector2 smoke_offset = default;
			public float tank_volume = 1.00f;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public struct State: IComponent
		{
			public float amount;
			public float mass;
			public double specific_heat;

			public double gas_amount;
			public float gas_temperature;

			public float temperature_current;
			public double pressure_current;

			public float temperature_target;
			public double pressure_target;

			[Save.Ignore, Net.Ignore] public int current_sound_index;
			[Save.Ignore, Net.Ignore] public float t_next_smoke;
			[Save.Ignore, Net.Ignore] public float t_next_edit;
		}

		public struct ConfigureRPC: Net.IRPC<Refinery.State>
		{
			public float? burner_modifier;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Refinery.State data)
			{
				ref var region = ref entity.GetRegion();
				if (region.GetWorldTime() >= data.t_next_edit)
				{
					data.t_next_edit = region.GetWorldTime() + 0.10f;

					var sync = false;

					if (this.burner_modifier.TryGetValue(out var v_burner_modifier))
					{
						ref var burner_state = ref entity.GetComponent<Burner.State>();
						if (burner_state.IsNotNull())
						{
							if (burner_state.air_modifier.TrySet(v_burner_modifier.Clamp01()))
							{
								burner_state.Sync(entity, true);
							}
						}
					}

					if (sync)
					{
						data.Sync(entity, true);
					}
				}
			}
#endif
		}

		//[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateInventory<T>(ISystem.Info info, Entity entity,
		//[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state,
		//[Source.Owned] in Refinery.Data refinery, [Source.Owned] ref Refinery.State refinery_state,
		//[Source.Owned, Pair.Of<Crafter.State>] ref T inventory) where T : unmanaged, IInventory
		//{
		//	var amount_total = 0.00f;
		//	var mass_total = 0.00f;
		//	var specific_heat_total = 0.00;
		//	var gas_amount_total = 0.00;

		//	ref var recipe = ref crafter.GetCurrentRecipe();

		//	//foreach (ref var requirement in recipe.requirements.AsSpan())
		//	//{
		//	//	if (requirement.type == Crafting.Requirement.Type.Resource)
		//	//	{
		//	//		ref var material = ref requirement.material.GetDefinition();
		//	//		if (material.id != 0)
		//	//		{
		//	//			var quantity = requirement.amount;
		//	//			var mass = quantity * material.mass_per_unit;

		//	//			amount_total += quantity;
		//	//			mass_total += mass;
		//	//			specific_heat_total += quantity * material.specific_heat;
		//	//			if (material.molar_mass > 0) gas_amount_total += (mass * 1000.00) / material.molar_mass;
		//	//		}
		//	//	}
		//	//}

		//	//foreach (ref var resource in inventory.GetResources())
		//	//{
		//	//	ref var material = ref resource.GetMaterial();
		//	//	if (material.id != 0)
		//	//	{
		//	//		var mass = resource.quantity * material.mass_per_unit;

		//	//		amount_total += resource.quantity;
		//	//		mass_total += mass;
		//	//		specific_heat_total += resource.quantity * material.specific_heat;
		//	//		if (material.molar_mass > 0) gas_amount_total += (mass * 1000.00) / material.molar_mass;
		//	//	}
		//	//}

		//	refinery_state.amount = amount_total;
		//	refinery_state.mass = mass_total;
		//	refinery_state.specific_heat = amount_total > 0.00 ? specific_heat_total / amount_total : 0.00;
		//	refinery_state.gas_amount = gas_amount_total;
		//}

		public const float update_interval = 0.20f;
		public const float update_interval_failed = 3.00f;

		internal const float tau_inv = 1.00f / MathF.Tau;

		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		public const double atmospheric_pressure = 100_000.00;
		public const double ambient_temperature = 300.00;

		public const double default_molar_mass = 30.000;
		public const double default_heat_capacity = 20.000;

		public const double gas_constant = 8.3144598;
		public const double air_moles_per_cubic_meter = 40.000;

		public static double CalculateAirPressure(float temperature)
		{
			return (atmospheric_pressure / ambient_temperature) * (double)temperature;
		}

		public static double CalculateGasPressure(float volume, double moles, float temperature)
		{
			return (moles * gas_constant * (double)temperature) / (double)volume;
		}

		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateWheel(ISystem.Info info, Entity entity, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Refinery.Data refinery, [Source.Owned] in Refinery.State refinery_state,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state,
		[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{
			switch (crafter_state.current_work_type)
			{
				case Work.Type.Mixing:
				{
					axle_state.force_load_new += crafter_state.current_work_difficulty * 100.00f;
					crafter_state.work += axle_state.angular_velocity * info.DeltaTime;
				}
				break;
			}
		}

#if SERVER
		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.20f)]
		public static void UpdateCrafter(ISystem.Info info, Entity entity,
		[Source.Owned] in Refinery.Data refinery, [Source.Owned] in Refinery.State refinery_state,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state, 
		[Source.Owned] in Burner.Data burner, [Source.Owned] in Burner.State burner_state)
		//[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{
			switch (crafter_state.current_work_type)
			{
				case Work.Type.Cooking:
				case Work.Type.Heating:
				{
					var power = burner_state._available_power;
					var work_amount = (float)((power / MathF.Max(crafter_state.current_work_difficulty, 1.00f)) * info.DeltaTime * 0.001f);
					crafter_state.work += work_amount;
				}
				break;

				case Work.Type.Refining:
				{
					var power = burner_state._available_power;
					var work_amount = (float)((power / MathF.Max(crafter_state.current_work_difficulty, 1.00f)) * info.DeltaTime * 0.001f);
					crafter_state.work += work_amount;
				}
				break;
			}
		}

		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] in Refinery.Data refinery, [Source.Owned] ref Refinery.State refinery_state,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state,
		[Source.Owned] in Burner.Data burner, [Source.Owned] ref Burner.State burner_state,
		[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{
			if (info.WorldTime >= crafter_state.next_tick)
			{
				crafter_state.next_tick = info.WorldTime + update_interval;

				var input_amount = refinery_state.amount;
				var joule_per_kelvin = 1000.00 + (refinery_state.specific_heat * refinery_state.mass);

				if (refinery_state.temperature_target <= refinery_state.temperature_current)
				{
					refinery_state.temperature_current = Maths.MoveTowards(refinery_state.temperature_current, refinery_state.temperature_target, 100.00f * Refinery.update_interval);
				}
				else
				{
					refinery_state.temperature_current = Maths.MoveTowards(refinery_state.temperature_current, burner_state.current_temperature, (float)((burner_state._available_power * Maths.Clamp(1.00f, 0.00f, 1.00f)) / joule_per_kelvin) * Refinery.update_interval);
				}

				////refinery_state.pressure_current = CalculateAirPressure(refinery_state.temperature_current);
				////refinery_state.pressure_current = CalculateGasPressure(refinery.tank_volume, Math.Max(refinery.tank_volume * air_moles_per_cubic_meter, refinery_state.gas_amount), refinery_state.temperature_current);
				//refinery_state.pressure_current = CalculateGasPressure(refinery.tank_volume, Math.Max(refinery.tank_volume * air_moles_per_cubic_meter, refinery_state.gas_amount), refinery_state.temperature_current);

				//entity.SyncComponent(ref refinery_state);

				//if (crafter.recipe.id != 0)
				//{
				//	if (MathF.Abs(axle_state.angular_velocity) > 1.00f)
				//	{
				//		crafter_state.work += 1.00f * update_interval;
				//	}
				//	else
				//	{
				//		crafter_state.work = 0.00f;
				//	}

				//	entity.SyncComponent(ref axle_state);
				//	entity.SyncComponent(ref crafter_state);

				//	//App.WriteLine($"refinery: {ts.GetMilliseconds():0.0000} ms");					
				//}
			}
		}
#endif

#if CLIENT
		public struct RefineryGUI: IGUICommand
		{
			public Entity ent_refinery;

			public Refinery.Data refinery;
			public Refinery.State refinery_state;

			public Crafter.Data crafter;
			public Crafter.State crafter_state;

			public Burner.Data burner;
			public Burner.State burner_state;

			public void Draw()
			{
				var max_temperature = 2000.00f;
				// var default_pressure = 100.00;
				var max_pressure = 5_000_000.00;

				using (var window = GUI.Window.Interaction("Refinery", this.ent_refinery))
				{
					this.StoreCurrentWindowTypeID(order: -100);

					ref var player = ref Client.GetPlayer();
					ref var region = ref Client.GetRegion();

					Crafting.Context.NewFromPlayer(ref region, ref player, ent_producer: this.ent_refinery, out var context);

					var w_right = (48 * 4) + 24;

					using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
					{
						using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
						{
							//GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

							using (var scrollbox = GUI.Scrollbox.New("recipes", size: GUI.Rm, padding: new(8, 8)))
							{
								GUI.DrawBackground(GUI.tex_frame, scrollbox.group_frame.GetInnerRect(), new(8, 8, 8, 8));

								if (this.crafter.recipe.id != 0)
								{
									CrafterExt.DrawRecipe(ref context, ref this.crafter, ref this.crafter_state);
								}
								else
								{
									GUI.TitleCentered("<no recipe selected>", size: 24);
								}
									//CrafterExt.DrawRecipe(ref region, ref this.crafter, ref this.crafter_state);
								//CrafterExt.DrawRecipe(context: ref context, recipe: ref this.crafter.GetCurrentRecipe(), current_index: this.crafter_state.current_index, progress: this.crafter_state.progress.AsSpan(), amount_multiplier: this.crafter.amount_multiplier);


								//ref var recipe = ref this.crafter.GetCurrentRecipe();
								//if (!recipe.IsNull())
								//{
								//	ref var inventory_data = ref this.ent_refinery.GetTrait<Crafter.State, Inventory8.Data>();
								//	GUI.DrawShopRecipe(ref region, this.crafter.recipe, this.ent_refinery, player.ent_controlled, default, default, default, inventory_data.GetHandle(), draw_button: false, draw_title: false, draw_description: false, search_radius: 0.00f);
								//}
							}
						}

						//using (GUI.Group.New(size: GUI.Rm))
						//{
						//	GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

						//	using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
						//	{
						//		using (GUI.Group.New(size: new(48, GUI.RmY)))
						//		{
						//			GUI.DrawTemperatureRange(this.refinery_state.temperature_current, this.refinery_state.temperature_target, max_temperature, new Vector2(24, GUI.RmY));

						//			GUI.SameLine();

						//			GUI.DrawPressureRange(this.refinery_state.pressure_current, this.refinery_state.pressure_target, max_pressure, new Vector2(24, GUI.RmY));
						//		}

						//		GUI.SameLine();

						//		using (GUI.Group.New(size: GUI.Rm))
						//		{
						//			if (GUI.SliderFloat("Value", ref this.burner_state.modifier, 0.00f, 1.00f, size: new(GUI.RmX, 24)))
						//			{
						//				var rpc = new Refinery.ConfigureRPC()
						//				{
						//					burner_modifier = this.burner_state.modifier
						//				};
						//				rpc.Send(this.ent_refinery);
						//			}

						//			//var changed = false;

						//			//changed |= GUI.SliderFloat("Temperature", ref this.refinery_state.temperature_target, 300.00f, max_temperature, "%.2f", new Vector2(80, 24));
						//			//GUI.SameLine();
						//			//changed |= GUI.SliderDouble("Pressure", ref this.refinery_state.pressure_target, atmospheric_pressure, max_pressure, "%.2f", new Vector2(80, 24));
						//			////GUI.DrawWorkH(Maths.Normalize(this.crafter_state.current_work, this.crafter.required_work), new Vector2(160, 24));

						//			//if (changed)
						//			//{
						//			//	var rpc = new Refinery.ConfigureRPC()
						//			//	{
						//			//		pressure_target = this.refinery_state.pressure_target,
						//			//		temperature_target = this.refinery_state.temperature_target
						//			//	};
						//			//	rpc.Send(this.ent_refinery);
						//			//}
						//		}
						//	}
						//}
					}

					//GUI.SameLine();

					using (var window_child = window.BeginChildWindow("refinery.settings.sub", GUI.AlignX.Right, GUI.AlignY.Center, size: new(248, window.group.size.Y - 32), padding: new(8), open: true, flags: GUI.Window.Flags.No_Click_Focus | GUI.Window.Flags.No_Appear_Focus))
					{
						if (window_child.show)
						{
							using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
							{
								using (var group_output = GUI.Group.New(size: new Vector2(Inventory.slot_size.X * 4, Inventory.slot_size.Y * 4), padding: new Vector2(4, 4), add_padding_to_size: true))
								{
									group_output.DrawBackground(GUI.tex_frame);

									//using (GUI.Group.New(padding: new(12, 12)))
									{
										GUI.DrawInventoryDock(Inventory.Type.Output, size: new(Inventory.slot_size.X * 4, Inventory.slot_size.Y * 4));
									}
								}

								using (var group_input = GUI.Group.New(size: new Vector2(Inventory.slot_size.X * 4, Inventory.slot_size.Y * 3) + new Vector2(24, 0), padding: new Vector2(4, 4), add_padding_to_size: true))
								{
									group_input.DrawBackground(GUI.tex_frame);

									//using (GUI.Group.New(padding: new(12, 12)))
									{
										using (GUI.Group.New(size: new Vector2(Inventory.slot_size.X * 4, Inventory.slot_size.Y * 2)))
										{
											GUI.DrawTemperatureRange(this.burner_state.current_temperature, this.burner_state.current_temperature, max_temperature, new Vector2(24, GUI.RmY));
											GUI.SameLine();
											GUI.DrawInventoryDock(Inventory.Type.Input, size: new(Inventory.slot_size.X * 4, Inventory.slot_size.Y * 2));

											//GUI.DrawWorkH(Maths.Normalize(this.crafter_state.work, this.crafter.), size: GUI.Rm with { Y = 32 } - new Vector2(48, 0));
											//GUI.SameLine();
											//GUI.DrawInventoryDock(Inventory.Type.Fuel, new Vector2(48, 48));
										}

										using (var group_burner = GUI.Group.New(size: new Vector2(GUI.RmX, Inventory.slot_size.Y * 1)))
										{
											//group_burner.DrawRect(layer: GUI.Layer.Foreground);

											GUI.DrawInventoryDock(Inventory.Type.Fuel, Inventory.GetFrameSize(1, 1));

											GUI.SameLine();

											using (var group_burner_slider = GUI.Group.New(size: GUI.Rm))
											{
												if (GUI.SliderFloat("Burner Intake", ref this.burner_state.air_modifier, 0.00f, 1.00f, size: new(GUI.RmX, 24), color_frame: GUI.col_output))
												{
													var rpc = new Refinery.ConfigureRPC()
													{
														burner_modifier = this.burner_state.air_modifier
													};
													rpc.Send(this.ent_refinery);
												}

												GUI.TitleCentered($"{(this.burner_state._available_power * 0.001):0} KW", pivot: new(0.00f, 0.50f), offset: new(4, 0));
												GUI.TitleCentered($"{this.burner_state.fuel_quantity:0.00}", pivot: new(1.00f, 0.50f), offset: new(-4, 0));
											}
										}

										using (var group_notes = GUI.Group.New(size: GUI.Rm))
										{
											if (!this.burner_state.cached_fuel_material.IsValid())
											{
												GUI.Title("Burner has no fuel!", color: GUI.font_color_red);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity, 
		[Source.Owned] in Refinery.Data refinery, [Source.Owned] in Refinery.State refinery_state, 
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state,
		[Source.Owned] in Burner.Data burner, [Source.Owned] in Burner.State burner_state,
		[Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
			{
				var gui = new RefineryGUI()
				{
					ent_refinery = entity,

					refinery = refinery,
					refinery_state = refinery_state,

					crafter = crafter,
					crafter_state = crafter_state,

					burner = burner,
					burner_state = burner_state,
				};
				gui.Submit();
			}
		}
#endif

#if CLIENT
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSound(ISystem.Info info,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Refinery.Data refinery, [Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state, [Source.Owned] ref Sound.Emitter sound_emitter)
		{
			var axle_speed = MathF.Abs(axle_state.angular_velocity);

			sound_emitter.volume = Maths.Clamp(axle_speed * 0.50f, 0.00f, 0.50f);
			sound_emitter.pitch = Maths.Clamp(axle_speed * 0.80f, 0.50f, 1.00f);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateParticles(ISystem.Info info, ref Region.Data region, ref XorRandom random, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state, [Source.Owned] ref Refinery.Data refinery, [Source.Owned] ref Refinery.State state)
		{
			var axle_speed = MathF.Abs(axle_state.angular_velocity);
			if (axle_speed > 1.00f && info.WorldTime >= state.t_next_smoke)
			{
				state.t_next_smoke = info.WorldTime + 0.50f;

				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = texture_smoke,
					lifetime = random.NextFloatRange(8.00f, 10.00f),
					pos = transform.LocalToWorldNoRotation(refinery.smoke_offset) + random.NextVector2(0.20f),
					vel = new Vector2(0.70f, -1.10f) * 1.50f,
					force = new Vector2(0.14f, 0.20f) * random.NextFloatRange(0.90f, 1.10f),
					fps = (byte)random.NextFloatRange(8, 10),
					frame_count = 64,
					frame_count_total = 64,
					frame_offset = (byte)random.NextFloatRange(0, 64),
					scale = random.NextFloatRange(0.20f, 0.40f),
					rotation = random.NextFloat(10.00f),
					angular_velocity = random.NextFloat(0.50f),
					growth = 0.30f,
					color_a = new Color32BGRA(180, 197, 217, 160),
					color_b = new Color32BGRA(0, 240, 240, 240)
				});
			}
		}
#endif
	}
}
