namespace TC2.Base.Components
{
	public static class RopeTest
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public struct Data: IComponent
		{
			//public Vector2 offset_a;
			//public Vector2 offset_b;

			public float width_a;
			public float width_b;

			public Texture.Handle texture;

			public float thickness;
			public float scale;
		}

#if CLIENT
		[ISystem.Render(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateRopes(ISystem.Info info, [Source.Owned] in RopeTest.Data rope, [Source.Shared] in Transform.Data transform_parent, [Source.Owned, Override] in Joint.DualRope joint)
		{
			if (joint.TryGetPositionsA(out var a_0, out var a_1) && joint.TryGetPositionsB(out var b_0, out var b_1))
			{
				a_0 = transform_parent.WorldToLocal(a_0);
				a_1 = transform_parent.WorldToLocal(a_1);
				b_0 = transform_parent.WorldToLocal(b_0);
				b_1 = transform_parent.WorldToLocal(b_1);

				Rope.Draw(transform_parent, new()
				{
					p0 = a_0,
					p1 = a_0,
					p2 = b_0,
					p3 = b_0,

					texture = rope.texture,
					scale = rope.scale,
					thickness = rope.thickness,
					z = -75.000f
				});

				Rope.Draw(transform_parent, new()
				{
					p0 = a_1,
					p1 = a_1,
					p2 = b_1,
					p3 = b_1,

					texture = rope.texture,
					scale = rope.scale,
					thickness = rope.thickness,
					z = -75.000f
				});
			}
		}
#endif
	}

	// Reference: https://www.kubicekballoons.eu/envelopes/model-series/model-z
	public static partial class Balloon
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true), IComponent.With<Balloon.State>]
		public partial struct Data: IComponent
		{
			public float release_max = 1.00f;
			public float release_rate = 1.00f;

			public float acc = 1.00f;
			public float speed_max = 20.00f;

			public float force_modifier = 100.00f;
			public float temperature_max = Maths.CelsiusToKelvin(140.00f);

			public float envelope_volume = 4200.00f; // cubic meters
			public float envelope_mass = 210.00f;
			public float envelope_thickness = Units.cm(0.20f);
			public float envelope_thermal_conductivity = 0.188f; // W/m*K

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State: IComponent
		{
			public float speed_current;
			public float speed_target;

			public float altitude;

			public float lift_modifier;
			public float altitude_modifier;

			public float current_radius;
			public float current_surface_area;
			public float current_temperature_air = Region.ambient_temperature;
			//public float current_temperature_envelope;
			public float current_volume;

			public State()
			{

			}
		}

#if CLIENT
		public partial struct BalloonGUI: IGUICommand
		{
			public Entity ent_balloon;
			public Entity ent_interactable;
			public Balloon.Data balloon;
			public Balloon.State balloon_state;
			public Transform.Data transform;
			public Burner.Data burner;
			public Burner.State burner_state;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Balloon", this.ent_interactable, new Vector2(192, 96), no_mouse_close: true))
				{
					//this.StoreCurrentWindowTypeID(order: -1000);
					if (window.show)
					{
						ref var region = ref this.ent_balloon.GetRegion();

						using (GUI.Group.New(size: GUI.Rm, padding: new(2, 2)))
						{
							//GUI.Text($"{this.balloon.altitude:0.00} m {this.balloon.lift_modifier:P2}");

							GUI.DrawTemperatureRange(this.burner_state.temperature_exhaust, this.burner_state.temperature_exhaust, 1000, size: new Vector2(24, GUI.RmY));
							//GUI.SameLine();
							//GUI.DrawVerticalGauge(this.balloon_state.current_lift, this.balloon_state.target_lift, this.balloon.lift_max, size: new Vector2(24, GUI.RmY), color_a: GUI.col_button_yellow, color_b: GUI.col_button_yellow);
							//GUI.DrawHoverTooltip($"Lift:\n{this.balloon_state.current_lift:0}/{this.balloon.lift_max:0}");
							GUI.SameLine();
							GUI.DrawVerticalGauge(this.balloon_state.altitude, this.balloon_state.altitude, 300.00f, size: new Vector2(24, GUI.RmY));
							GUI.DrawHoverTooltip($"Altitude:\n{this.balloon_state.altitude:0}/{300.00f:0}m");

							//GUI.DrawTemperatureRange(this.burner_state.current_temperature, this.burner_state.current_temperature, 1000, size: new Vector2(24, GUI.RmY));
							//GUI.SameLine();
							//GUI.DrawWorkV(0.50f, size: new Vector2(24, GUI.RmY));

							//GUI.DrawGauge(this.burner_state.modifier, 0.00f, 1.00f);

							GUI.SameLine();


							using (GUI.Group.New(new(GUI.RmX, 48)))
							{
								GUI.DrawInventoryDock(Inventory.Type.Fuel, new(48, 48));

							}
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity, Entity ent_interactable,
		[Source.Owned] in Balloon.Data balloon, [Source.Owned] in Balloon.State balloon_state, [Source.Owned] in Transform.Data transform, [Source.Parent] in Interactable.Data interactable,
		[Source.Parent] in Burner.Data burner, [Source.Parent] in Burner.State burner_state)
		{
			if (interactable.show)
			{
				var gui = new BalloonGUI()
				{
					ent_balloon = entity,
					ent_interactable = ent_interactable,
					balloon = balloon,
					balloon_state = balloon_state,
					transform = transform,
					burner = burner,
					burner_state = burner_state
				};
				gui.Submit();
			}
		}
#endif

		public static float base_altitude = 100.00f;

		public static float GetAltitude(ref Region.Data region, float y)
		{
			ref var terrain = ref region.GetTerrain();
			var map_height = terrain.GetHeight();

			return base_altitude + (map_height * 0.50f) - y;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateScale(ISystem.Info info, ref Region.Data region,
		[Source.Owned] in Balloon.Data balloon, [Source.Owned] in Balloon.State balloon_state, 
		[Source.Owned] ref Transform.Data transform)
		{
			//var size = MathF.Pow(Maths.Normalize01(balloon_state.current_lift, balloon.lift_max) * balloon_state.altitude_modifier, 0.50f) * 0.30f;
			//var sign = transform.GetScaleOld().GetParity();

			//transform.scale = Vector2.Lerp(transform.scale, new Vector2((0.90f + (size * 0.60f)) * sign, 0.90f + (size * 0.70f)), 0.05f);
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateNoRotate(ISystem.Info info, ref Region.Data region,
		[Source.Owned] in Balloon.Data balloon, [Source.Owned] in Balloon.State balloon_state, 
		[Source.Parent] ref Burner.State burner_state, [Source.Owned, Override] ref NoRotate.Data no_rotate)
		{
			//var temperature = Maths.KelvinToCelsius(burner_state.exhaust_temperature);
			//var modifier = Maths.Normalize01(balloon_state.current_lift, balloon.lift_max);

			//no_rotate.multiplier *= Maths.Normalize01(temperature - 100, 200.00f);
			//no_rotate.mass_multiplier *= Maths.Min(modifier * 2.00f, Maths.Normalize01(temperature - 100, 200.00f));
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateShape(ISystem.Info info, ref Region.Data region,
		[Source.Owned] in Balloon.Data balloon, [Source.Owned] in Balloon.State balloon_state, 
		[Source.Owned, Pair.Of<Body.Data>] ref Shape.Circle circle)
		{
			//var modifier = Maths.Normalize01(balloon_state.current_lift * 10.00f, balloon.lift_max);
			//circle.rigidity_dynamic = Maths.Lerp(0.20f, 1.00f, modifier);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ref Region.Data region, ISystem.Info info, Entity entity, Entity ent_transform_parent,
		[Source.Parent] ref Transform.Data transform_parent, [Source.Parent] in Control.Data control,
		[Source.Parent] ref Burner.Data burner, [Source.Parent] ref Burner.State burner_state,
		[Source.Owned] ref Body.Data body, [Source.Owned] ref Transform.Data transform,
		[Source.Owned] ref Balloon.Data balloon, [Source.Owned] ref Balloon.State balloon_state,
		[Source.Owned] in Health.Data health)
		{
			var speed = 0.00f;
			var fuel_modifier_target = burner_state.modifier_fuel_target;
			var mass = body.GetMass();
			var fuel_rate_step = 0.80f;

			//var temperature = Maths.KelvinToCelsius(burner_state.exhaust_temperature);
			//balloon_state.current_lift = Maths.Lerp(balloon_state.current_lift, temperature * 8.00f, 0.50f);

			var temperature_ambient = Region.ambient_temperature;
			var wind_speed = 4.00f;

			var htc_air = Phys.GetAirConvectionHTC(wind_speed);
			var htc_envelope = Phys.GetConvectionHTC(balloon.envelope_thermal_conductivity, balloon.envelope_thickness);

			balloon_state.current_temperature_air = Maths.SumWeighted(balloon_state.current_temperature_air, burner_state.temperature_exhaust, balloon.envelope_volume * Phys.air_density_kordel, burner_state.exhaust_output);

			//var temperature_envelope = 

			var air_density_ambient = Phys.GetAirDensity(Phys.atmospheric_pressure_kordel, temperature_ambient);
			//var air_density_balloon = Phys.GetAirDensity(Phys.reference_atmospheric_pressure_bar, balloon_state.current_temperature_air);
			Phys.CharlesLaw(balloon.envelope_volume, temperature_ambient, out var envelope_volume_hot, balloon_state.current_temperature_air);
			Phys.Buoyancy(air_density_ambient, envelope_volume_hot, out var buoyant_force);

			balloon_state.current_volume = Maths.Lerp(Maths.Max(balloon_state.current_volume, balloon.envelope_volume), envelope_volume_hot, 0.10f);
			//balloon_state.current_temperature_air = Maths.Lerp(Maths.Max(balloon_state.current_temperature_air, temperature_ambient), envelope_volume_hot, 0.10f);
			balloon_state.lift_modifier = Maths.Normalize(balloon_state.current_volume, balloon.envelope_volume);

			balloon_state.speed_current = htc_air;
			balloon_state.speed_target = htc_envelope;


			//if (control.keyboard.GetKey(Keyboard.Key.MoveRight)) speed += balloon_state.speed * info.DeltaTime;
			//if (control.keyboard.GetKey(Keyboard.Key.MoveLeft)) speed -= balloon_state.speed * info.DeltaTime;
			if (control.keyboard.GetKey(Keyboard.Key.MoveUp)) fuel_modifier_target += fuel_rate_step * info.DeltaTime;
			else if (control.keyboard.GetKey(Keyboard.Key.MoveDown)) fuel_modifier_target -= fuel_rate_step * info.DeltaTime;
			else
			{
				fuel_modifier_target = Maths.Lerp(fuel_modifier_target, 0.50f, 0.10f);
			}

			burner_state.modifier_fuel_target = fuel_modifier_target.Clamp01();

			////var heat_transfer_surface = Phys.calc (heat_coeff * boiler.area_surface * (balloon_state.current_temperature_air - temperature_ambient); // * (1.00f - boiler.insulation);
			//var dt = info.DeltaTime;

			//balloon_state.current_temperature_air -= Maths.Normalize((heat_transfer_surface - heat_transfer_self) * dt * heat_transfer_speed_mult, boiler.mass_tank * substance_surface.specific_heat);

			//var m = ((1.00f / balloon.lift_max * 1.00f) * (balloon_state.target_lift - balloon_state.current_lift));
			////burner_state.air_modifier_target = (burner_state.air_modifier_target + (m * 0.70f)).Clamp01();
			////burner._loss_multiplier = Maths.Max(1.00f, MathF.Pow(balloon.altitude * 0.02f, 1.50f));

			var modifier = MathF.Pow(MathF.Sin(Maths.Min(health.integrity, health.durability) * MathF.PI * 0.50f), 1.20f);
			var gravity_modifier = Maths.Step(modifier, 0.20f);

			////App.WriteLine(gravity_modifier);

			//var fuel_rate_step = 0.10f;

			//if (control.keyboard.GetKey(Keyboard.Key.MoveRight)) speed += balloon_state.speed * info.DeltaTime;
			//if (control.keyboard.GetKey(Keyboard.Key.MoveLeft)) speed -= balloon_state.speed * info.DeltaTime;
			//if (control.keyboard.GetKey(Keyboard.Key.MoveUp)) fuel_modifier_target += fuel_rate_step * info.DeltaTime;
			//if (control.keyboard.GetKey(Keyboard.Key.MoveDown)) fuel_modifier_target -= fuel_rate_step * info.DeltaTime;

			//balloon_state.altitude = GetAltitude(ref region, transform.position.Y + body.GetVelocity().Y);
			//balloon_state.lift_modifier = MathF.Pow(0.90f, balloon_state.altitude / 10.00f);

			//burner_state.air_modifier_target = Maths.Lerp01(burner_state.air_modifier_target, balloon_state.lift_modifier, 0.10f);
			//burner_state.fuel_modifier_target = fuel_modifier_target.Clamp01();

			////sound_emitter.volume = Maths.Lerp(sound_emitter.volume, lift <= 0.00f ? 0.00f : 1.00f, 0.05f);
			////sound_emitter.pitch = Maths.Lerp(sound_emitter.pitch, lift <= 0.00f ? 0.50f : 1.00f, 0.02f);

			//var lift = 0.00f;
			//balloon_state.target_lift = Maths.Clamp(balloon_state.target_lift + lift - (MathF.Pow(Maths.Max(balloon_state.altitude - 100, 10), 2.00f) * 0.000f), 0.00f, balloon.lift_max * modifier);
			//balloon_state.speed_target = Maths.Clamp(speed != 0.00f ? (balloon_state.speed_target + speed) : 0.00f, -balloon.speed_max, balloon.speed_max);

			////balloon.current_lift = balloon.target_lift; // Maths.Lerp(balloon.current_lift, balloon.target_lift, 0.20f);
			balloon_state.speed_current = 0; // balloon_state.speed_target; // Maths.Lerp(balloon.current_speed, balloon.target_speed, 0.50f);

			//balloon_state.altitude_modifier = ((1 * balloon_state.lift_modifier) / 200.00f);
			////if (balloon.altitude_modifier < 1.00f) balloon.altitude_modifier *= balloon.altitude_modifier;

			var lift_mult = 1.50f;
			var force = new Vector2(balloon_state.speed_current * balloon.force_modifier, Maths.Min(-(buoyant_force * balloon_state.lift_modifier * lift_mult * info.DeltaTime) * region.GetGravity().Y * gravity_modifier, 0));
			body.AddForce(force);
			//body.AddVelocity(-body.GetVelocity() * 0.035f);

			//#if SERVER
			//			if (control.keyboard.GetKeyDown(Keyboard.Key.Spacebar))
			//			{


			//				transform_parent.scale.X *= -1.00f;
			//				transform_parent.Modified(ent_transform_parent, true);
			//			}
			//#endif

			//#if SERVER
			//			if (control.keyboard.GetKeyDown(Keyboard.Key.Spacebar))
			//			{
			//				transform_parent.scale.X *= -1.00f;
			//				transform_parent.Sync(ent_transform_parent);
			//			}
			//#endif
		}
	}
}
