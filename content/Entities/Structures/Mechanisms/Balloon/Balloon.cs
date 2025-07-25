﻿namespace TC2.Base.Components
{
	public static class RopeTest
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
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
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region), IComponent.With<Balloon.State>]
		public partial struct Data(): IComponent
		{
			//public float release_max = 1.00f;
			//public float release_rate = 1.00f;

			//public float acc = 1.00f;
			public float speed_max = 8.00f;

			//public float force_modifier = 100.00f;
			public float temperature_max = Temperature.Celsius(180.00f); // Maths.CelsiusToKelvin(180.00f);

			public Radius envelope_radius_mid;
			public Radius envelope_radius_bottom;
			public Height envelope_height_bottom;
			public Width envelope_thickness = Units.mm(0.40f);
			public Power envelope_thermal_conductivity = Power.W(0.021f); // kW/m*K
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct State(): IComponent
		{
			public Temperature current_temperature_air = Region.ambient_temperature;
			[Asset.Ignore] public float speed_current;
			[Asset.Ignore] public Volume current_volume;

			[Net.Ignore, Save.Ignore] public float lift_modifier;
			[Net.Ignore, Save.Ignore] public float altitude_modifier;

			[Net.Ignore, Save.Ignore] public Volume envelope_volume;
			[Net.Ignore, Save.Ignore] public Area envelope_surface_area;
			[Net.Ignore, Save.Ignore] public Mass envelope_mass;
			
			[Net.Ignore, Save.Ignore] public Mass air_mass;
			[Net.Ignore, Save.Ignore] public Density air_density;

			[Net.Ignore, Save.Ignore] public float altitude;
			[Net.Ignore, Save.Ignore] public float buoyant_force;
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
				using (var window = GUI.Window.InteractionMisc("Balloon"u8, this.ent_interactable, new Vector2(96 + 48, 96), no_mouse_close: true))
				{
					//this.StoreCurrentWindowTypeID(order: -1000);
					if (window.show)
					{
						ref var region = ref this.ent_balloon.GetRegion();

						//using (GUI.Group.New(size: GUI.Rm, padding: new(2, 2)))
						{
							//GUI.Text($"{this.balloon.altitude:0.00} m {this.balloon.lift_modifier:P2}");

							//GUI.DrawTemperatureRange(this.balloon_state.current_temperature_air, region.GetAmbientTemperature(transform.position), balloon.temperature_max, size: new Vector2(24, GUI.RmY));
							//GUI.SameLine();
							//GUI.DrawVerticalGauge(this.balloon_state.current_lift, this.balloon_state.target_lift, this.balloon.lift_max, size: new Vector2(24, GUI.RmY), color_a: GUI.col_button_yellow, color_b: GUI.col_button_yellow);
							//GUI.DrawHoverTooltip($"Lift:\n{this.balloon_state.current_lift:0}/{this.balloon.lift_max:0}");
							//GUI.SameLine();
							GUI.DrawGauge2(this.balloon_state.altitude, 0.00f, 300.00f, size: new(GUI.RmY));
							//GUI.DrawVerticalGauge(this.balloon_state.altitude, this.balloon_state.altitude, 300.00f, size: new Vector2(24, GUI.RmY));
							GUI.DrawHoverTooltip($"Altitude:\n{this.balloon_state.altitude:0}/{300.00f:0}m");

							//GUI.DrawTemperatureRange(this.burner_state.current_temperature, this.burner_state.current_temperature, 1000, size: new Vector2(24, GUI.RmY));
							//GUI.SameLine();
							//GUI.DrawWorkV(0.50f, size: new Vector2(24, GUI.RmY));

							//GUI.DrawGauge(this.burner_state.modifier, 0.00f, 1.00f);

							GUI.SameLine();

							using (GUI.Group.New(new(GUI.RmX, 48)))
							{
								GUI.DrawInventoryDock(Inventory.Type.Fuel, GUI.Rm);
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
			if (interactable.IsActive())
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

		//public static float base_altitude = 100.00f;

		//public static float GetAltitude(ref Region.Data region, float y)
		//{
		//	ref var terrain = ref region.GetTerrain();
		//	var map_height = terrain.GetHeight();

		//	return base_altitude + (map_height * 0.50f) - y;
		//}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateScale(ISystem.Info info, ref Region.Data region,
		[Source.Owned] in Balloon.Data balloon, [Source.Owned] in Balloon.State balloon_state, 
		[Source.Owned] ref Transform.Data transform)
		{
			var size = MathF.Pow(Maths.Max(balloon_state.lift_modifier - 0.80f, 0.10f) * 2.00f, 0.50f) * 0.30f;
			var sign = transform.GetScaleOld().GetParity();

			transform.scale = Vector2.Lerp(transform.scale, new Vector2((0.90f + (size * 0.60f)) * sign, 0.80f + (size * 0.70f)), 0.05f);
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateNoRotate(ISystem.Info info, ref Region.Data region,
		[Source.Owned] in Balloon.Data balloon, [Source.Owned] in Balloon.State balloon_state, 
		[Source.Parent] ref Burner.State burner_state, [Source.Owned, Override] ref NoRotate.Data no_rotate)
		{
			//var temperature = Maths.KelvinToCelsius(burner_state.exhaust_temperature);
			//var modifier = Maths.Normalize01(balloon_state.current_lift, balloon.lift_max);



			no_rotate.multiplier *= Maths.Max(0.00f, (balloon_state.lift_modifier - 1.00f).Pow2() * 10.00f);
			no_rotate.mass_multiplier *= 1.00f + Maths.Max(0.00f, (balloon_state.lift_modifier - 1.00f) * 0.50f);
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateShape(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] in Balloon.Data balloon, [Source.Owned] ref Balloon.State balloon_state,
		[Source.Owned] ref Body.Data body, [Source.Owned, Pair.Component<Body.Data>] ref Shape.Circle circle)
		{
			var density_linen = Density.kgm3(300.00f);
			var circle_mass_old = circle.mass;

			balloon_state.envelope_volume = Volume.Balloon(balloon.envelope_radius_mid, balloon.envelope_radius_bottom, balloon.envelope_height_bottom);
			balloon_state.envelope_surface_area = Area.Balloon(balloon.envelope_radius_mid, balloon.envelope_radius_bottom, balloon.envelope_height_bottom);
			balloon_state.envelope_mass = (Mass)(balloon_state.envelope_surface_area.m_value * density_linen * balloon.envelope_thickness);
			balloon_state.air_mass = (Mass)(balloon_state.envelope_volume.m_value * Phys.air_density_kordel.m_value);

			circle.mass = Maths.Max(1.00f, balloon_state.envelope_mass + balloon_state.air_mass);
			//circle.mass = Maths.Max(1.00f, balloon_state.air_mass);

#if SERVER
			if (circle.mass != circle_mass_old)
			{
				body.Modified(entity, true);
			}
#endif

			//var modifier = Maths.Normalize01(balloon_state.current_lift * 10.00f, balloon.lift_max);
			//circle.rigidity_dynamic = Maths.Lerp(0.20f, 1.00f, modifier);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ref Region.Data region, ISystem.Info info, Entity entity, Entity ent_transform_parent,
		[Source.Parent] ref Transform.Data transform_parent, [Source.Parent] in Control.Data control,
		[Source.Parent] ref Burner.Data burner, [Source.Parent] ref Burner.State burner_state,
		[Source.Owned] ref Body.Data body, [Source.Owned] ref Transform.Data transform,
		[Source.Owned] ref Balloon.Data balloon, [Source.Owned] ref Balloon.State balloon_state,
		[Source.Parent, Pair.Tag("exhaust")] ref Air.Vent.Data vent_exhaust,
		[Source.Owned] in Health.Data health)
		{
			var show_debug = false;
			var dt = info.DeltaTime;
			var wind_speed = 4.00f;
			var mass = body.GetMass();
			var fuel_rate_step = 0.15f;

			var temperature_speed = 20.00f;

			balloon_state.altitude = region.GetAltitude(transform.position.Y + (body.GetVelocity().Y * App.fixed_update_interval_s)); // // GetAltitude(ref region, transform.position.Y + body.GetVelocity().Y);
			var fuel_modifier_target = burner_state.modifier_fluid_target;

			var temperature_ambient = Region.ambient_temperature - (balloon_state.altitude * 0.02f);
			var atmospheric_pressure_ambient = Phys.CalculateAtmosphericPressure(temperature_ambient, balloon_state.altitude);

			var air_density_ambient = Phys.GetAirDensity(atmospheric_pressure_ambient, Region.ambient_temperature);

			var htc_air = Phys.GetAirConvectionHTC(wind_speed + body.GetVelocity().Length());
			var htc_envelope = Phys.GetConvectionHTC(balloon.envelope_thermal_conductivity, balloon.envelope_thickness) * (1.00f + (balloon_state.altitude * 0.004f)).Pow2();

			balloon_state.current_temperature_air = Maths.SumWeighted(balloon_state.current_temperature_air, vent_exhaust.blob.temperature, balloon_state.envelope_volume, vent_exhaust.flow_rate_old.m_value.Abs() * temperature_speed * 2.00f);

			Phys.CharlesLaw(balloon_state.envelope_volume, Phys.ambient_temperature, out var envelope_volume_hot, balloon_state.current_temperature_air);
			balloon_state.current_volume = Maths.Lerp(Maths.Max(balloon_state.current_volume, balloon_state.envelope_volume), envelope_volume_hot, 0.10f);

			Phys.Buoyancy(air_density_ambient, balloon_state.current_volume, region.GetGravity().Y, out var buoyant_force);

			balloon_state.air_density = balloon_state.air_mass / envelope_volume_hot;

			var test = balloon_state.current_temperature_air;

			//App.WriteLine(temperature_ambient);
			Phys.TransferHeatAmbient(ref balloon_state.current_temperature_air, temperature_ambient, balloon_state.air_mass, Phys.air_specific_heat, balloon_state.envelope_surface_area, htc_envelope + htc_air, dt * temperature_speed);

			//Phys.TransferHeatAmbient2(ref test, temperature_ambient, balloon_state.air_mass, Phys.air_specific_heat, balloon_state.envelope_surface_area, htc_envelope, dt);


			var health_modifier = MathF.Pow(MathF.Sin(Maths.Min(health.integrity, health.durability) * MathF.PI * 0.50f), 1.20f);

			balloon_state.buoyant_force = buoyant_force;
			balloon_state.lift_modifier = Maths.Normalize(balloon_state.current_volume, balloon_state.envelope_volume) * health_modifier;

			//balloon_state.speed_current = htc_air;
			//balloon_state.speed_target = htc_envelope;

			var speed = 0.00f;
			var speed_step = 2.00f;

			if (control.keyboard.GetKey(Keyboard.Key.MoveRight)) speed += balloon.speed_max;
			if (control.keyboard.GetKey(Keyboard.Key.MoveLeft)) speed -= balloon.speed_max;

			if (control.keyboard.GetKey(Keyboard.Key.MoveUp)) fuel_modifier_target.MoveTowards(1.00f, fuel_rate_step * dt);
			else if (control.keyboard.GetKey(Keyboard.Key.MoveDown)) fuel_modifier_target.MoveTowards(0.00f, fuel_rate_step * dt);
			else
			{
				//fuel_modifier_target.MoveTowards(0.10f, fuel_rate_step, dt);
			}

			burner_state.modifier_fluid_target = fuel_modifier_target.Clamp01();

			//var gravity_modifier = Maths.Step(modifier, 0.20f);

			balloon_state.speed_current.MoveTowards(speed, speed_step * dt); // balloon_state.speed_target; // Maths.Lerp(balloon.current_speed, balloon.target_speed, 0.50f);

			var force = new Vector2(balloon_state.speed_current * mass, 0.00f);
			force.Y -= buoyant_force * 1.00f * health_modifier;

			body.AddForce(force);
			var mass_force = mass * region.GetGravity().Y;



#if CLIENT
			if (show_debug)
			{
				region.DrawDebugText(transform.position + new Vector2(0.00f, 0.00f),
				$"altitude: {balloon_state.altitude:0.0000}\n" +
				$"test: {test:0.0000}\n" +
				$"htc_air: {htc_air:0.0000}\n" +
				$"htc_envelope: {htc_envelope:0.0000}\n" +
				$"temperature_ambient: {temperature_ambient:0.0000}\n" +
				$"temperature_balloon: {balloon_state.current_temperature_air:0.0000}\n" +
				$"atmospheric_pressure_kordel: {Phys.atmospheric_pressure_kordel:0.0000}\n" +
				$"atmospheric_pressure_ambient: {atmospheric_pressure_ambient:0.0000}\n" +
				$"air_density_kordel: {Phys.air_density_kordel:0.0000}\n" +
				$"air_density_ambient: {air_density_ambient:0.0000}\n" +
				$"air_density_hot: {balloon_state.air_density:0.0000}\n" +
				$"envelope_mass: {balloon_state.envelope_mass:0.0000}\n" +
				$"air_mass: {balloon_state.air_mass:0.0000}\n" +
				$"mass: {mass:0.0000}\n" +
				$"buoyant_force: {buoyant_force:0.0000}\n" +
				$"mass_force: {mass_force:0.0000}\n" +
				"", Color32BGRA.Yellow);
			}
#endif

#if SERVER
			// TODO: sync it properly
			if (region.GetCurrentTick() % 64 == 0)
			{
				balloon_state.Sync(entity);
			}
#endif

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
