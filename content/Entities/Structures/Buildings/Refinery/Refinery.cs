namespace TC2.Base.Components
{
//	public static partial class Heater
//	{
//		[ITrait.Data(Net.SendType.Unreliable)]
//		public struct Data: ITrait
//		{
//			[Flags]
//			public enum Flags: uint
//			{
//				None = 0,

//				Use_Misc = 1 << 0
//			}

//			public Heater.Data.Flags flags;

//			public Temperature temperature_target;
//			public Temperature temperature_current;

//			[Save.Ignore, Net.Ignore] public float t_next_update;

//			public Data()
//			{

//			}
//		}

//		//[IComponent.Data(Net.SendType.Unreliable)]
//		//public struct State: IComponent
//		//{
//		//	public Temperature temperature_target;
//		//	public Temperature temperature_current;

//		//	[Save.Ignore, Net.Ignore] public float t_next_update;
//		//}

////		public struct ConfigureRPC: Net.IRPC<Heater.State>
////		{
////#if SERVER
////			public void Invoke(ref NetConnection connection, Entity entity, ref Heater.State data)
////			{
////				//ref var region = ref entity.GetRegion();
////				//if (region.GetWorldTime() >= data.t_next_edit)
////				//{
////				//	data.t_next_edit = region.GetWorldTime() + 0.10f;

////				//	var sync = false;

////				//	if (this.burner_modifier.TryGetValue(out var v_burner_modifier))
////				//	{
////				//		ref var burner_state = ref entity.GetComponent<Burner.State>();
////				//		if (burner_state.IsNotNull())
////				//		{
////				//			if (burner_state.modifier_intake_target.TrySet(v_burner_modifier.Clamp01()))
////				//			{
////				//				burner_state.Sync(entity, true);
////				//			}
////				//		}
////				//	}

////				//	if (sync)
////				//	{
////				//		data.Sync(entity, true);
////				//	}
////				//}
////			}
////#endif
////		}

//		public const float update_interval = 0.20f;
//		public const float update_interval_failed = 3.00f;

//		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

//		//[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
//		//public static void Update(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
//		//[Source.Owned] in Transform.Data transform,
//		//[Source.Owned, Pair.Wildcard] ref Heater.Data heater, IComponent.Handle h_heater)
//		//{
//		//	var time = info.WorldTime;
//		//	if (time >= heater.t_next_update)
//		//	{
//		//		heater.t_next_update = time + update_interval;
//		//		//heater.temperature_target.m_value.MoveTowardsDamped(region.GetAmbientTemperature(transform.position), 10.00f, 0.10f);
//		//		Maths.Diffuse(ref heater.temperature_target.m_value, region.GetAmbientTemperature(transform.position), 5.00f, info.DeltaTime);
//		//	}

//		//	heater.temperature_current.m_value.MoveTowardsDamped(heater.temperature_target.m_value, 2.00f, 0.10f);
//		//}

//		//[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
//		//public static void UpdateEssence(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
//		//[Source.Owned] in Transform.Data transform, [Source.Owned] ref EssenceContainer.Data essence_container,
//		//[Source.Owned, Pair.Component<EssenceContainer.Data>] ref Heater.Data heater, IComponent.Handle h_heater)
//		//{
//		//	var time = info.WorldTime;
//		//	if (time >= heater.t_next_update)
//		//	{
//		//		ref var essence_data = ref essence_container.h_essence.GetData();
//		//		if (essence_data.IsNotNull())
//		//		{
//		//			heater.temperature_target += (essence_data.heat_emit * essence_container.rate * essence_container.available) * info.DeltaTime;
//		//		}
//		//		//App.WriteLine("a");

//		//		//heater.t_next_update = time + update_interval;
//		//	}
//		//}

//#if CLIENT
//		public struct HeaterGUI: IGUICommand
//		{
//			public Entity ent_heater;

//			public Transform.Data transform;

//			public Heater.Data heater;
//			public IComponent.Handle h_heater;

//			public void Draw()
//			{
//				using (var window = GUI.Window.InteractionMisc("Heater"u8, this.ent_heater, size: new(24, 96 * 1)))
//				{
//					this.StoreCurrentWindowTypeID(order: -100);
//					if (window.show)
//					{
//						using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
//						{
//							GUI.DrawTemperatureRange(this.heater.temperature_current, this.heater.temperature_target, 2000, size: GUI.Rm);
//						}
//					}
//				}
//			}
//		}

//		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
//		public static void OnGUI(Entity entity,
//		[Source.Owned, Pair.Wildcard] in Heater.Data heater, IComponent.Handle h_heater,
//		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform)
//		{
//			if (interactable.IsActive())
//			{
//				var gui = new HeaterGUI()
//				{
//					ent_heater = entity,

//					transform = transform,

//					heater = heater,
//					h_heater = h_heater
//				};
//				gui.Submit();
//			}
//		}
//#endif

////#if CLIENT
////		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
////		public static void UpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random,
////		[Source.Owned] in Transform.Data transform,
////		[Source.Owned] in Heater.Data heater, [Source.Owned] ref Heater.State heater_state,
////		[Source.Owned, Pair.Component<Heater.State>, Optional(true)] ref Sound.Emitter sound_emitter,
////		[Source.Owned, Pair.Component<Heater.State>, Optional(true)] ref Light.Data light)
////		{

////		}
////#endif
//	}


	public static partial class Refinery
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Refinery.State>]
		public struct Data: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Use_Misc = 1 << 0
			}

			public Refinery.Data.Flags flags;

			//public Vector2 smoke_offset;
			//public float tank_volume = 1.00f;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public struct State: IComponent
		{
			//public float amount;
			//public float mass;
			//public double specific_heat;

			//public double gas_amount;
			//public float gas_temperature;

			//public float temperature_current;
			//public double pressure_current;

			//public float temperature_target;
			//public double pressure_target;

			//[Save.Ignore, Net.Ignore] public int current_sound_index;
			[Save.Ignore, Net.Ignore] public float t_next_smoke;
			[Save.Ignore, Net.Ignore] public float t_next_update;
			//[Save.Ignore, Net.Ignore] public float t_next_edit;
		}

		public struct ConfigureRPC: Net.IRPC<Refinery.State>
		{
#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Refinery.State data)
			{
				//ref var region = ref entity.GetRegion();
				//if (region.GetWorldTime() >= data.t_next_edit)
				//{
				//	data.t_next_edit = region.GetWorldTime() + 0.10f;

				//	var sync = false;

				//	if (this.burner_modifier.TryGetValue(out var v_burner_modifier))
				//	{
				//		ref var burner_state = ref entity.GetComponent<Burner.State>();
				//		if (burner_state.IsNotNull())
				//		{
				//			if (burner_state.modifier_intake_target.TrySet(v_burner_modifier.Clamp01()))
				//			{
				//				burner_state.Sync(entity, true);
				//			}
				//		}
				//	}

				//	if (sync)
				//	{
				//		data.Sync(entity, true);
				//	}
				//}
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

		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region | ISystem.Scope.Global)]
		public static void Update(ISystem.Info.Common info, ref Region.Data.Common region_common, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] in Refinery.Data refinery, [Source.Owned] ref Refinery.State refinery_state)
		{
			var time = info.WorldTime;
			if (time >= refinery_state.t_next_update)
			{
				refinery_state.t_next_update = time + update_interval;
			}
		}

#if CLIENT
		public struct RefineryGUI: IGUICommand
		{
			public Entity ent_refinery;

			public Transform.Data transform;

			//public Heater.Data heater;
			//public IComponent.Handle h_heater;

			public Heat.Data heat;
			public Heat.State heat_state;

			public Refinery.Data refinery;
			public Refinery.State refinery_state;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Refinery"u8, this.ent_refinery))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
						{
							using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
							{
								//GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

								//using (var scrollbox = GUI.Scrollbox.New("recipes", size: GUI.Rm, padding: new(8, 8)))
								//{
								//	GUI.DrawBackground(GUI.tex_frame, scrollbox.group_frame.GetInnerRect(), new(8, 8, 8, 8));

								//	if (this.crafter.h_recipe.id != 0)
								//	{
								//		CrafterExt.DrawRecipe(ref context, ref this.crafter, ref this.crafter_state);
								//	}
								//	else
								//	{
								//		GUI.TitleCentered("<no recipe selected>", size: 24);
								//	}
								//	//CrafterExt.DrawRecipe(ref region, ref this.crafter, ref this.crafter_state);
								//	//CrafterExt.DrawRecipe(context: ref context, recipe: ref this.crafter.GetCurrentRecipe(), current_index: this.crafter_state.current_index, progress: this.crafter_state.progress.AsSpan(), amount_multiplier: this.crafter.amount_multiplier);


								//	//ref var recipe = ref this.crafter.GetCurrentRecipe();
								//	//if (!recipe.IsNull())
								//	//{
								//	//	ref var inventory_data = ref this.ent_refinery.GetTrait<Crafter.State, Inventory8.Data>();
								//	//	GUI.DrawShopRecipe(ref region, this.crafter.recipe, this.ent_refinery, player.ent_controlled, default, default, default, inventory_data.GetHandle(), draw_button: false, draw_title: false, draw_description: false, search_radius: 0.00f);
								//	//}
								//}
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

						//using (var window_child = window.BeginChildWindow("refinery.settings.sub"u8, GUI.AlignX.Right, GUI.AlignY.Center, size: new(248, window.group.size.Y - 32), padding: new(8), open: true, flags: GUI.Window.Flags.No_Click_Focus | GUI.Window.Flags.No_Appear_Focus))
						//{
						//	if (window_child.show)
						//	{
						//		using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
						//		{
						//			using (var group_output = GUI.Group.New(size: new Vector2(Inventory.slot_size.X * 4, Inventory.slot_size.Y * 4), padding: new Vector2(4, 4), add_padding_to_size: true))
						//			{
						//				group_output.DrawBackground(GUI.tex_frame);

						//				//using (GUI.Group.New(padding: new(12, 12)))
						//				{
						//					GUI.DrawInventoryDock(Inventory.Type.Output, size: new(Inventory.slot_size.X * 4, Inventory.slot_size.Y * 4));
						//				}
						//			}

						//			using (var group_input = GUI.Group.New(size: new Vector2(Inventory.slot_size.X * 4, Inventory.slot_size.Y * 3) + new Vector2(24, 0), padding: new Vector2(4, 4), add_padding_to_size: true))
						//			{
						//				group_input.DrawBackground(GUI.tex_frame);

						//				//using (GUI.Group.New(padding: new(12, 12)))
						//				//{
						//				//	using (GUI.Group.New(size: new Vector2(Inventory.slot_size.X * 4, Inventory.slot_size.Y * 2)))
						//				//	{
						//				//		GUI.DrawTemperatureRange(300, 300, max_temperature, new Vector2(24, GUI.RmY)); // TODO: update this to use vents
						//				//		GUI.SameLine();
						//				//		GUI.DrawInventoryDock(Inventory.Type.Input, size: new(Inventory.slot_size.X * 4, Inventory.slot_size.Y * 2));

						//				//		//GUI.DrawWorkH(Maths.Normalize(this.crafter_state.work, this.crafter.), size: GUI.Rm with { Y = 32 } - new Vector2(48, 0));
						//				//		//GUI.SameLine();
						//				//		//GUI.DrawInventoryDock(Inventory.Type.Fuel, new Vector2(48, 48));
						//				//	}

						//				//	using (var group_burner = GUI.Group.New(size: new Vector2(GUI.RmX, Inventory.slot_size.Y * 1)))
						//				//	{
						//				//		//group_burner.DrawRect(layer: GUI.Layer.Foreground);

						//				//		GUI.DrawInventoryDock(Inventory.Type.Fuel, Inventory.GetFrameSize(1, 1));

						//				//		GUI.SameLine();

						//				//		using (var group_burner_slider = GUI.Group.New(size: GUI.Rm))
						//				//		{
						//				//			if (GUI.SliderFloat("Burner Intake", ref this.burner_state.modifier_intake_target, 0.00f, 1.00f, size: new(GUI.RmX, 24), color_frame: GUI.col_output))
						//				//			{
						//				//				var rpc = new Refinery.ConfigureRPC()
						//				//				{
						//				//					burner_modifier = this.burner_state.modifier_intake_target
						//				//				};
						//				//				rpc.Send(this.ent_refinery);
						//				//			}

						//				//			GUI.TitleCentered($"{(this.burner_state.available_power * 0.001):0} KW", pivot: new(0.00f, 0.50f), offset: new(4, 0));
						//				//			//GUI.TitleCentered($"{this.burner_state.fuel_quantity:0.00}", pivot: new(1.00f, 0.50f), offset: new(-4, 0));
						//				//		}
						//				//	}

						//				//	using (var group_notes = GUI.Group.New(size: GUI.Rm))
						//				//	{
						//				//		//if (!this.burner_state.cached_fuel_material.IsValid())
						//				//		//{
						//				//		//	GUI.Title("Burner has no fuel!", color: GUI.font_color_red);
						//				//		//}
						//				//	}
						//				//}
						//			}
						//		}
						//	}
						//}
					
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region | ISystem.Scope.Global)]
		public static void OnGUI(Entity entity, // IComponent.Handle h_heater,
		[Source.Owned] in Refinery.Data refinery, [Source.Owned] in Refinery.State refinery_state, 
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned, Optional] in Heat.Data heat, [Source.Owned, Optional] in Heat.State heat_state)
		//[Source.Owned, Pair.First, Optional] in Heater.Data heater)
		{
			if (interactable.IsActive())
			{
				var gui = new RefineryGUI()
				{
					ent_refinery = entity,

					transform = transform,

					heat = heat,
					heat_state = heat_state,

					//heater = heater,
					//h_heater = h_heater,

					refinery = refinery,
					refinery_state = refinery_state,
				};
				gui.Submit();
			}
		}
#endif

#if CLIENT
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSound(ISystem.Info info, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] in Refinery.Data refinery, [Source.Owned] ref Refinery.State refinery_state,
		[Source.Owned, Pair.Component<Refinery.State>] ref Sound.Emitter sound_emitter)
		{
			//var axle_speed = MathF.Abs(axle_state.angular_velocity);

			//sound_emitter.volume_mult = Maths.Clamp(axle_speed * 0.50f, 0.00f, 0.50f);
			//sound_emitter.pitch_mult = Maths.Clamp(axle_speed * 0.80f, 0.50f, 1.00f);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateParticles(ISystem.Info info, ref Region.Data region, ref XorRandom random, 
		[Source.Owned] in Transform.Data transform, 
		[Source.Owned] in Refinery.Data refinery, [Source.Owned] ref Refinery.State refinery_state)
		{
			//var axle_speed = MathF.Abs(axle_state.angular_velocity);
			//if (axle_speed > 1.00f && info.WorldTime >= state.t_next_smoke)
			//{
			//	state.t_next_smoke = info.WorldTime + 0.50f;

			//	Particle.Spawn(ref region, new Particle.Data()
			//	{
			//		texture = texture_smoke,
			//		lifetime = random.NextFloatRange(8.00f, 10.00f),
			//		pos = transform.LocalToWorldNoRotation(refinery.smoke_offset) + random.NextVector2(0.20f),
			//		vel = new Vector2(0.70f, -1.10f) * 1.50f,
			//		force = new Vector2(0.14f, 0.20f) * random.NextFloatRange(0.90f, 1.10f),
			//		fps = (byte)random.NextFloatRange(8, 10),
			//		frame_count = 64,
			//		frame_count_total = 64,
			//		frame_offset = (byte)random.NextFloatRange(0, 64),
			//		scale = random.NextFloatRange(0.20f, 0.40f),
			//		rotation = random.NextFloat(10.00f),
			//		angular_velocity = random.NextFloat(0.50f),
			//		growth = 0.30f,
			//		color_a = new Color32BGRA(180, 197, 217, 160),
			//		color_b = new Color32BGRA(0, 240, 240, 240)
			//	});
			//}
		}
#endif
	}
}
