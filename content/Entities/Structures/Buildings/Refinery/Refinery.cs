namespace TC2.Base.Components
{
	public static partial class Refinery
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Refinery.State>]
		public struct Data: IComponent
		{
			public Vector2 smoke_offset;
			public float tank_volume = 1.00f;
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public struct State: IComponent
		{
			[Net.Ignore, Save.Ignore] public int current_sound_index;
			[Net.Ignore, Save.Ignore] public float next_smoke;

			public float amount;
			public float mass;
			public double specific_heat;

			public double gas_amount;
			public float gas_temperature;

			public float temperature_current;
			public double pressure_current;

			public float temperature_target;
			public double pressure_target;
		}

		public struct ConfigureRPC: Net.IRPC<Refinery.State>
		{
			public float? temperature_target;
			public double? pressure_target;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Refinery.State data)
			{
				if (this.temperature_target.HasValue) data.temperature_target = this.temperature_target.Value;
				if (this.pressure_target.HasValue) data.pressure_target = this.pressure_target.Value;

				data.Sync(entity);
			}
#endif
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateInventory<T>(ISystem.Info info, Entity entity,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state,
		[Source.Owned] in Refinery.Data refinery, [Source.Owned] ref Refinery.State refinery_state,
		[Source.Owned, Trait.Of<Crafter.State>] ref T inventory) where T : unmanaged, IInventory
		{
			var amount_total = 0.00f;
			var mass_total = 0.00f;
			var specific_heat_total = 0.00;
			var gas_amount_total = 0.00;

			ref var recipe = ref crafter.GetCurrentRecipe();

			foreach (ref var requirement in recipe.requirements.AsSpan())
			{
				if (requirement.type == Crafting.Requirement.Type.Resource)
				{
					ref var material = ref requirement.material.GetDefinition();
					if (material.id != 0)
					{
						var quantity = requirement.amount;
						var mass = quantity * material.mass_per_unit;

						amount_total += quantity;
						mass_total += mass;
						specific_heat_total += quantity * material.specific_heat;
						if (material.molar_mass > 0) gas_amount_total += (mass * 1000.00) / material.molar_mass;
					}
				}
			}

			//foreach (ref var resource in inventory.GetResources())
			//{
			//	ref var material = ref resource.GetMaterial();
			//	if (material.id != 0)
			//	{
			//		var mass = resource.quantity * material.mass_per_unit;

			//		amount_total += resource.quantity;
			//		mass_total += mass;
			//		specific_heat_total += resource.quantity * material.specific_heat;
			//		//gas_amount_total += (mass * 1000.00) / material.molar_mass;
			//	}
			//}

			refinery_state.amount = amount_total;
			refinery_state.mass = mass_total;
			refinery_state.specific_heat = amount_total > 0.00 ? specific_heat_total / amount_total : 0.00;
			refinery_state.gas_amount = gas_amount_total;
		}

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

#if SERVER
		[ISystem.Update(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] in Refinery.Data refinery, [Source.Owned] ref Refinery.State refinery_state,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state,
		[Source.Owned] in Burner.Data burner, [Source.Owned] ref Burner.State burner_state,
		[Source.Owned] ref Wheel.Data wheel)
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
					refinery_state.temperature_current = Maths.MoveTowards(refinery_state.temperature_current, burner_state.current_temperature, (float)((burner_state.available_energy * Maths.Clamp(1.00f, 0.00f, 1.00f)) / joule_per_kelvin) * Refinery.update_interval);
				}

				//refinery_state.pressure_current = CalculateAirPressure(refinery_state.temperature_current);
				//refinery_state.pressure_current = CalculateGasPressure(refinery.tank_volume, Math.Max(refinery.tank_volume * air_moles_per_cubic_meter, refinery_state.gas_amount), refinery_state.temperature_current);
				refinery_state.pressure_current = CalculateGasPressure(refinery.tank_volume, Math.Max(refinery.tank_volume * air_moles_per_cubic_meter, refinery_state.gas_amount), refinery_state.temperature_current);

				entity.SyncComponent(ref refinery_state);

				if (crafter.recipe.id != 0)
				{
					wheel.rotation %= MathF.Tau;

					if (MathF.Abs(wheel.angular_velocity) > 1.00f)
					{
						crafter_state.current_work += 1.00f * update_interval;
					}
					else
					{
						crafter_state.current_work = 0.00f;
					}

					entity.SyncComponent(ref wheel);
					entity.SyncComponent(ref crafter_state);

					//App.WriteLine($"refinery: {ts.GetMilliseconds():0.0000} ms");					
				}
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

					var w_right = (48 * 4) + 24;

					using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth() - w_right, GUI.GetRemainingHeight())))
					{
						using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight() - 96 - 12)))
						{
							GUI.DrawFillBackground("ui_frame", new(8, 8, 8, 8));

							using (GUI.Group.New(size: GUI.GetRemainingSpace(), padding: new(12, 12)))
							{
								ref var recipe = ref this.crafter.GetCurrentRecipe();
								if (!recipe.IsNull())
								{
									ref var inventory_data = ref this.ent_refinery.GetTrait<Crafter.State, Inventory8.Data>();
									GUI.DrawShopRecipe(ref region, ref recipe, this.ent_refinery, player.ent_controlled, default, default, default, inventory_data.GetHandle(), draw_button: false, draw_title: false, draw_description: false, search_radius: 0.00f);
								}
							}
						}

						using (GUI.Group.New(size: GUI.GetRemainingSpace()))
						{
							GUI.DrawFillBackground("ui_frame", new(8, 8, 8, 8));

							using (GUI.Group.New(size: GUI.GetRemainingSpace(), padding: new(12, 12)))
							{
								using (GUI.Group.New(size: new(48, GUI.GetRemainingHeight())))
								{
									GUI.DrawTemperatureRange(this.refinery_state.temperature_current, this.refinery_state.temperature_target, max_temperature, new Vector2(24, GUI.GetRemainingHeight()));

									GUI.SameLine();

									GUI.DrawPressureRange(this.refinery_state.pressure_current, this.refinery_state.pressure_target, max_pressure, new Vector2(24, GUI.GetRemainingHeight()));
								}

								GUI.SameLine();

								using (GUI.Group.New(size: GUI.GetRemainingSpace()))
								{
									var changed = false;

									changed |= GUI.SliderFloat("Temperature", ref this.refinery_state.temperature_target, 300.00f, max_temperature, "%.2f", new Vector2(80, 24));
									GUI.SameLine();
									changed |= GUI.SliderDouble("Pressure", ref this.refinery_state.pressure_target, atmospheric_pressure, max_pressure, "%.2f", new Vector2(80, 24));
									//GUI.DrawWorkH(Maths.Normalize(this.crafter_state.current_work, this.crafter.required_work), new Vector2(160, 24));

									if (changed)
									{
										var rpc = new Refinery.ConfigureRPC()
										{
											pressure_target = this.refinery_state.pressure_target,
											temperature_target = this.refinery_state.temperature_target
										};
										rpc.Send(this.ent_refinery);
									}
								}
							}
						}
					}

					GUI.SameLine();

					using (GUI.Group.New(size: new Vector2(w_right, GUI.GetRemainingHeight())))
					{
						using (GUI.Group.New(size: new Vector2(48 * 4, 48 * 4) + new Vector2(24, 24)))
						{
							GUI.DrawFillBackground("ui_frame", new(8, 8, 8, 8));

							using (GUI.Group.New(padding: new(12, 12)))
							{
								GUI.DrawInventoryDock(Inventory.Type.Output, size: new(48 * 4, 48 * 4));
							}
						}

						using (GUI.Group.New(size: new Vector2(48 * 4, GUI.GetRemainingHeight()) + new Vector2(24, 0)))
						{
							GUI.DrawFillBackground("ui_frame", new(8, 8, 8, 8));

							using (GUI.Group.New(padding: new(12, 12)))
							{
								using (GUI.Group.New(size: new Vector2(48 * 4, GUI.GetRemainingHeight())))
								{
									GUI.DrawInventoryDock(Inventory.Type.Input, size: new(48 * 4, 48 * 2));

									GUI.DrawWorkH(Maths.Normalize(this.crafter_state.current_work, this.crafter.required_work), size: GUI.GetRemainingSpace() with { Y = 32 } - new Vector2(48, 0));
									GUI.SameLine();
									GUI.DrawInventoryDock(Inventory.Type.Fuel, new Vector2(48, 48));
								}
							}
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUI(Entity entity, [Source.Owned] in Refinery.Data refinery, [Source.Owned] in Refinery.State refinery_state, [Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State state, [Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
			{
				var gui = new RefineryGUI()
				{
					ent_refinery = entity,

					refinery = refinery,
					refinery_state = refinery_state,

					crafter = crafter,
					crafter_state = state
				};
				gui.Submit();
			}
		}
#endif

#if CLIENT
		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateSound(ISystem.Info info,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Refinery.Data refinery, [Source.Owned] ref Wheel.Data wheel, [Source.Owned] ref Sound.Emitter sound_emitter)
		{
			var wheel_speed = MathF.Abs(wheel.angular_velocity);

			sound_emitter.volume = Maths.Clamp(wheel_speed * 0.50f, 0.00f, 0.50f);
			sound_emitter.pitch = Maths.Clamp(wheel_speed * 0.80f, 0.50f, 1.00f);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateParticles(ISystem.Info info, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Wheel.Data wheel, [Source.Owned] ref Refinery.Data refinery, [Source.Owned] ref Refinery.State state)
		{
			var wheel_speed = MathF.Abs(wheel.angular_velocity);
			if (wheel_speed > 1.00f && info.WorldTime >= state.next_smoke)
			{
				state.next_smoke = info.WorldTime + 0.50f;

				var random = XorRandom.New();
				Particle.Spawn(ref info.GetRegion(), new Particle.Data()
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
