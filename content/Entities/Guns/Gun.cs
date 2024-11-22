namespace TC2.Base.Components
{
	public static partial class Gun
	{
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";
		public static readonly Texture.Handle texture_muzzle_flash = "MuzzleFlash";

		public static readonly Sound.Handle sound_gun_break = "gun_break";

		public enum Stage: byte
		{
			Ready = 0,

			Fired,
			Cycling,
			Reloading,
			Jammed
		}

		[Flags]
		public enum Flags: ushort
		{
			None = 0,

			[Name("Automatic")]
			Automatic = 1 << 0,

			[Name("Full Reload")]
			Full_Reload = 1 << 1,

			[Name("Cycle on Shoot")]
			Cycle_On_Shoot = 1 << 2,

			[Name("Manual Cycle")]
			Manual_Cycle = 1 << 3,

			No_Particles = 1 << 4,

			Child_Projectiles = 1 << 5,
			Rope_Projectiles = 1 << 6,
			No_LMB_Cycle = 1 << 7,
			Cycled_When_Reloaded = 1 << 8,

			[Name("Reset Bursts")]
			Reset_Bursts = 1 << 9,

			[Name("Eject on Shoot")]
			Eject_On_Shoot = 1 << 10,

			[Name("Eject from Muzzle")]
			Eject_From_Muzzle = 1 << 11,
		}

		public enum Type: byte
		{
			Undefined = 0,

			[Name("Handgun")]
			Handgun,
			[Name("Shotgun")]
			Shotgun,
			[Name("SMG")]
			SMG,
			[Name("Rifle")]
			Rifle,
			[Name("Machine Gun")]
			MachineGun,
			[Name("Cannon")]
			Cannon,
			[Name("Autocannon")]
			AutoCannon,
			[Name("Launcher")]
			Launcher
		}

		public enum Feed: byte
		{
			Undefined = 0,

			Single,
			Cylinder,
			Drum,
			Clip,
			Magazine,
			Belt,
			Breech,
			Front,
			Funnel
		}

		public enum Action: byte
		{
			Undefined = 0,

			Revolver,
			Manual,
			Bolt,
			Lever,
			Pump,
			Blowback,
			Gas,
			Matchlock,
			Flintlock,
			Crank,
			Motor
		}

		//public enum Operation: byte
		//{
		//	Undefined = 0,

		//	Revolver,
		//	Bolt,
		//	Lever,
		//	Pump,
		//	Blowback,
		//	Gas,
		//	Crank
		//}

		[Flags]
		public enum Hints: ushort
		{
			None = 0,

			//Can_Reload = 1 << 0,
			Cycled = 1 << 2,
			Loaded = 1 << 3,
			Wants_Reload = 1 << 4,
			Artillery = 1 << 5,
			Hold_RMB = 1 << 6,
			No_Ammo = 1 << 7,
			Supressive_Fire = 1 << 8,
			Long_Range = 1 << 9,
			Close_Range = 1 << 10,

			[Net.Ignore] Eject_Pending = 1 << 15,
		}

		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Animation: IComponent
		{
			public byte frame_ready;
			public byte frame_ready_loaded;
			public byte frame_fired;
			public byte frame_cycling;

			public byte frame_reloading;
			public byte frame_jammed;
		}

		[IComponent.Data(Net.SendType.Reliable, region_only: true), IComponent.With<Gun.State>]
		public partial struct Data(): IComponent
		{
			public static readonly Sound.Handle sound_jam_default = "gun.jam.00";

			[Editor.Picker.Position(true, true)]
			public Vector2 muzzle_offset;
			[Editor.Picker.Position(true, true)]
			public Vector2 receiver_offset;

			[Save.NewLine]
			[Editor.Picker.Position(true, true)]
			public Vector2 particle_offset;

			[Save.NewLine]
			[Editor.Picker.Direction(true, false, scale: 1.00f)]
			public Vector2 eject_direction;
			public float eject_angular_velocity = 0.10f;
			//public float eject_scale = 1.00f;


			public float particle_rotation;
			public float flash_size = 1.00f;


			[Save.NewLine]
			public float smoke_size = 1.00f;
			public float smoke_amount = 1.00f;

			[Save.NewLine]
			public float shake_amount = 0.20f;
			public float shake_radius = 16.00f;


			[Save.NewLine]
			[Statistics.Info("Loudness", description: "Loudness of the shot.", format: "{0:0.##}x", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Low)]
			public float sound_volume = 1.25f;
			public float sound_size = 1.00f;
			public float sound_dist_multiplier = 2.00f;
			public float sound_pitch = 1.00f;


			[Save.NewLine]
			public Sound.Handle sound_shoot;
			public Sound.Handle sound_cycle;

			public Sound.Handle sound_reload;
			public Sound.Handle sound_empty;

			public Sound.Handle sound_eject;
			public Sound.Handle sound_jam = sound_jam_default;


			[Save.NewLine]
			[Statistics.Info("Damage", description: "Damage dealt by the fired projectile.", format: "{0:0.##}x", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float damage_multiplier;
			[Statistics.Info("Muzzle Velocity", description: "Velocity of the fired projectile.", format: "{0:0.##} m/s", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float velocity_multiplier;
			[Statistics.Info("Spread", description: "Spread of the fired projectiles.", format: "{0:0.##}x", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.High)]
			public float jitter_multiplier;
			[Statistics.Info("Recoil", description: "Force applied after firing the weapon.", format: "{0:0.##}x", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Medium)]
			public float recoil_multiplier;
			public float velocity_max = 700.00f;

			[Statistics.Info("Reload Speed", description: "Time to reload the weapon.", format: "{0:0.##}s", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Medium)]
			public float reload_interval;
			[Statistics.Info("Cycle Speed", description: "Rate of fire.", format: "{0:0.##}s", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.High)]
			public float cycle_interval;
			[Statistics.Info("Stability", description: "Reliability, may result in a catastrophic failure if too low.", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float stability = 100.00f;
			[Statistics.Info("Failure Rate", description: "Chance of malfunction, such as jamming after being fired.", format: "{0:P2}", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Low)]
			public float failure_rate = 0.00f;

			[Save.NewLine]
			[Statistics.Info("Ammo", description: "Ammunition type.", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
			public Material.Flags ammo_filter;

			[Statistics.Info("Ammunition Usage", description: "Ammo used per shot.", format: "{0:0}", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Medium)]
			public float ammo_per_shot = 1.00f;
			[Statistics.Info("Maximum Ammunition", description: "Ammo capacity.", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float max_ammo;

			[Statistics.Info("Barrel Count", description: "Number of barrels.", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public byte barrel_count = 1;
			[Statistics.Info("Projectile Count", description: "Number of projectiles fired per shot.", format: "{0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public byte projectile_count = 1;
			[Statistics.Info("Burst Count", description: "Number of shots per burst.", format: "{0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public byte burst_count = 1;

			[Save.NewLine]
			[Statistics.Info("Operation", description: "Operation mode of the weapon.", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			public Gun.Action action;
			[Statistics.Info("Type", description: "Type of the weapon.", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			public Gun.Type type;
			[Statistics.Info("Feed", description: "Method of loading ammunition.", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			public Gun.Feed feed;

			[Save.NewLine]
			public Gun.Flags flags;

			public float heuristic_range = 30.00f;
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State(): IComponent
		{
			public Gun.Hints hints;

			public Gun.Stage stage;
			public byte burst_rem;
			public byte eject_rem;
			public byte unused;

			public Resource.Data resource_ammo;

			//public IMaterial.Handle h_last_ammo;

			[Save.Ignore, Net.Ignore] public Vector2 last_recoil;
			[Save.Ignore, Net.Ignore] public Entity ent_projectile_next;
			[Save.Ignore, Net.Ignore] public float angle_jitter;
			[Save.Ignore, Net.Ignore] public float muzzle_velocity;
			[Save.Ignore, Net.Ignore] public float next_cycle;
			[Save.Ignore, Net.Ignore] public float next_reload;
		}

		public static bool TryCalculateTrajectory(Vector2 pos_muzzle, Vector2 pos_target, float speed, float gravity, out float? angle_shallow, out float? angle_steep)
		{
			angle_shallow = null;
			angle_steep = null;

			//ref var material = ref material_ammo.GetData();
			//if (material.IsNotNull() && ammo.prefab.TryGetPrefab(out var prefab_projectile))
			{
				//var pos_w_offset = transform.LocalToWorld(gun.muzzle_offset);
				//var dir = transform.GetDirection();

				//var velocity_jitter = 1.00f - (Maths.Clamp(gun.jitter_multiplier * 0.20f, 0.00f, 1.00f) * 0.50f);

				//if (prefab_projectile.Root.TryGetComponentData<Projectile.Data>(out var projectile, true))
				{
					//var random_multiplier = random.NextFloatRange(0.90f * velocity_jitter, 1.10f);


					//var random_multiplier = ((0.90f * velocity_jitter) + 1.10f) * 0.50f;

					var p = pos_target - pos_muzzle;
					p.Y *= -1;

					var v = speed; // velocity_multiplier * ammo.speed_mult;
					var g = gravity; /// region.GetGravity().Y * projectile.gravity;
					//var d = projectile.damp;

					var sqrt = MathF.Sqrt((v * v * v * v) - (g * (g * (p.X * p.X) + (2.00f * p.Y * (v * v)))));

					var a = MathF.Atan(((v * v) - sqrt) / (g * p.X));
					var b = MathF.Atan(((v * v) + sqrt) / (g * p.X));

					if (!a.IsNaN())
					{
						angle_shallow = a;

						if (!b.IsNaN())
						{
							if (a.Abs() < b.Abs())
							{
								angle_steep = b;
							}
							else
							{
								angle_steep = a;
								angle_shallow = b;
							}
						}
					}
					else if (!b.IsNaN())
					{
						angle_shallow = b;
					}
				}
			}

			return angle_shallow.HasValue || angle_steep.HasValue;
		}

#if CLIENT
		internal static Vector2 last_trajectory_gizmo_pos;

		public static void DrawTrajectory(ref Region.Data region, IMaterial.Handle material_ammo, in Gun.Data gun, in Transform.Data transform)
		{
			var ts = Timestamp.Now();

			ref var material = ref material_ammo.GetData();
			if (material.IsNotNull())
			{
				ref var ammo = ref material.ammo.GetRefOrNull();
				if (ammo.IsNotNull() && ammo.prefab.TryGetPrefab(out var prefab_projectile))
				{
					var pos_w_offset = transform.LocalToWorld(gun.muzzle_offset);
					var canvas_scale = region.GetWorldToCanvasScale();

					var color = GUI.font_color_red;

					if (prefab_projectile.root.TryGetComponentData<Projectile.Data>(out var projectile, true))
					{
						var vel = transform.GetDirection() * Maths.Min(gun.velocity_max, gun.velocity_multiplier * ammo.speed_mult); // * random_multiplier;

						var pos_a = pos_w_offset;
						var pos_b = pos_w_offset;

						//GUI.Text($"{vel}");

						var iter_count = 800;
						var iter_count_inv = 1.00f / iter_count;

						var pos_last = pos_a;
						var dist_delta = 0.00f;

						var line_len = 4.00f;
						var line_gap = 2.00f;

						var alpha = 1.00f;

						for (var i = 0; i < iter_count; i++)
						{
							var pos = pos_b;
							alpha = Maths.Clamp(1.00f - (i * iter_count_inv), 0.10f, 0.50f);

							//vel *= projectile.damp;
							if (vel.LengthSquared() > 40.00f.Pow2()) vel = vel.GetNormalized(out var vel_len) * Maths.Max(40.00f, vel_len * projectile.damp);
							vel += Region.gravity * App.fixed_update_interval_s * projectile.gravity;

							var step = vel * App.fixed_update_interval_s;
							pos += step;

							pos_a = pos_b;
							pos_b = pos;

							dist_delta += Vector2.Distance(pos_a, pos_b);

							if (dist_delta >= line_len)
							{
								var dir = (pos - pos_last).GetNormalized(out var len);
								//len -= line_gap;

								var pos_line_a = pos_last;
								var pos_line_b = pos_last + (dir * len);

								pos_last = pos;

								if (!region.IsInLineOfSight(pos_line_a, pos_line_b, out pos_last, radius: projectile.size, query_flags: Physics.QueryFlag.Static, mask: Physics.Layer.World | Physics.Layer.Solid, exclude: Physics.Layer.Essence | Physics.Layer.Ignore_Bullet | Physics.Layer.Gas | Physics.Layer.Water | Physics.Layer.Fire))
								{
									//var pos_line_mid = (pos_line_a + pos_line_b) * 0.50f;
									//var dir_perp = dir.GetPerpendicular() * 10;

									//GUI.DrawLine((pos_line_mid - (dir_perp)).WorldToCanvas(), (pos_line_mid + (dir_perp)).WorldToCanvas(), GUI.col_button_yellow.WithAlphaMult(0.50f), 4.00f);


									break;
								}

								//GUI.DrawCircleFilled(pos_line_a.WorldToCanvas(), 4, GUI.col_button_yellow.WithAlphaMult(0.50f), segments: 4, layer: GUI.Layer.Background);
								//GUI.DrawCircleFilled(pos_line_b.WorldToCanvas(), 4, GUI.col_button_yellow.WithAlphaMult(0.50f), segments: 4, layer: GUI.Layer.Background);

								GUI.DrawLine(region.WorldToCanvas(pos_line_a + (dir * line_gap)), region.WorldToCanvas(pos_line_b), color.WithAlphaMult(alpha), 4.00f, layer: GUI.Layer.Background);
								dist_delta = 0;
							}
						}

						var canvas_rect = GUI.GetCanvasRect();
						var c_pos_last = region.WorldToCanvas(pos_last);
						var c_pos_last_clipped = c_pos_last;
						var c_radius = (1.00f + (gun.jitter_multiplier * ammo.spread_mult)) * canvas_scale * 1.00f;

						GUI.DrawLine(c_pos_last - new Vector2(c_radius * 1.50f, 0.00f), c_pos_last + new Vector2(c_radius * 1.50f, 0.00f), color: color.WithAlphaMult(0.250f), thickness: 1.00f, layer: GUI.Layer.Background);
						GUI.DrawLine(c_pos_last - new Vector2(0.00f, c_radius * 1.50f), c_pos_last + new Vector2(0.00f, c_radius * 1.50f), color: color.WithAlphaMult(0.250f), thickness: 1.00f, layer: GUI.Layer.Background);
						GUI.DrawCircleFilled(c_pos_last, projectile.size * canvas_scale, color.WithAlphaMult(1.00f), segments: 12, layer: GUI.Layer.Background);
						GUI.DrawCircle(c_pos_last, c_radius, color.WithAlphaMult(alpha), thickness: 2.00f, segments: 32, layer: GUI.Layer.Background);

						var canvas_rect_inner = canvas_rect.Pad(new Vector4(200));
						//if (!canvas_rect_inner.ContainsPoint(c_pos_last))
						{
							var c_pos_edge = canvas_rect_inner.ClipPoint(c_pos_last);
							canvas_rect_inner.ClipLine(ref c_pos_edge, ref c_pos_last_clipped);

							GUI.DrawLine(Gun.last_trajectory_gizmo_pos, c_pos_last, color: color.WithAlphaMult(0.50f), thickness: 2.00f, layer: GUI.Layer.Background);
							GUI.DrawCircleFilled(Gun.last_trajectory_gizmo_pos, (projectile.size * canvas_scale * 0.75f), color.WithAlphaMult(1.00f), segments: 4, layer: GUI.Layer.Background);
						}

						GUI.DrawTextCentered($"[{pos_last.X:0}, {pos_last.Y:0}]\n{Vector2.Distance(pos_w_offset, pos_last):0.00}m\n{(Maths.NormalizeAngle(transform.GetInterpolatedRotation()) * Maths.rad2deg):0.00}°", Gun.last_trajectory_gizmo_pos + new Vector2(0, 4 + (12 * 2)), color: GUI.font_color_default, layer: GUI.Layer.Background);
						Gun.last_trajectory_gizmo_pos = Vector2.Lerp(Gun.last_trajectory_gizmo_pos, c_pos_last_clipped, 0.03f);
					}
				}
			}

			var elapsed_ms = ts.GetMilliseconds();
			//GUI.Text($"trace in {elapsed_ms:0.0000} ms");
		}

		public struct HoldGUI: IGUICommand
		{
			public Transform.Data transform;
			public Vector2 world_position_target;

			public Gun.Data gun;
			public Gun.State gun_state;
			public Inventory1.Data inventory;

			public void Draw()
			{
				var pos_muzzle = this.transform.LocalToWorldInterpolated(this.gun.muzzle_offset);

				var dir_a = (this.world_position_target - pos_muzzle).GetNormalized();
				var dir_b = this.transform.GetInterpolatedDirection();

				ref var region = ref Client.GetRegion();

				using (var window = GUI.Window.HUD("Crosshair"u8, region.WorldToCanvas(this.world_position_target) + new Vector2(0, -64), size: new(80, 32), pivot: new(0.50f, 0.00f)))
				{
					if (window.show)
					{
						GUI.DisableChat();

						if (this.gun_state.stage == Gun.Stage.Reloading)
						{
							GUI.TitleCentered("Reloading"u8, pivot: new(0.50f, 0.00f));
							GUI.TitleCentered(Maths.Max(this.gun_state.next_reload - region.GetWorldTime(), 0.00f), format: "0.00", pivot: new(0.50f, 1.00f));
						}
					}
				}

				//GUI.DrawLine((transform.position).WorldToCanvas(), (transform.position + (transform.GetDirection() * 10.00f)).WorldToCanvas(), Color32BGRA.Red);

				//GUI.DrawCrosshair(this.transform.GetInterpolatedPosition(), this.world_position_target, this.transform.GetInterpolatedDirection(), this.gun.jitter_multiplier, this.inventory[0].quantity, this.gun.max_ammo);

				var pos_a = pos_muzzle;
				var pos_b = this.world_position_target;

				//var dist = Vector2.Distance(pos_a, pos_b);

				//GUI.DrawArc(region.WorldToCanvas(pos_a), dir_a, dir_b, radius: dist * region.GetWorldToCanvasScale(), thickness: 8, color: GUI.font_color_red.WithAlpha(50));

				//Gun.DrawTrajectory(ref region, gun_state.resource_ammo.material, in gun, in transform);
				//Gun.DrawCrosshair(ref region, ref this.gun, ref this.gun_state, pos_a, pos_b, Vector2.Lerp(dir_a, dir_b, 0.00f), this.gun.jitter_multiplier, this.inventory[0].quantity, this.gun.max_ammo);
				//Gun.DrawCrosshair(ref region, ref this.gun, ref this.gun_state, pos_a, pos_b, Maths.Slerp(dir_a, dir_b, 0.75f), this.gun.jitter_multiplier, this.inventory[0].quantity, this.gun.max_ammo);
				Gun.DrawCrosshair(ref region, ref this.gun, ref this.gun_state, pos_a, pos_b, dir_a, dir_b, this.gun.jitter_multiplier, this.inventory[0].quantity, this.gun.max_ammo);
			}
		}

		[Shitcode] // WIP shitcode
		public static void DrawCrosshair(ref Region.Data region, ref Gun.Data gun, ref Gun.State gun_state, Vector2 position_a, Vector2 position_b, Vector2 dir_a, Vector2 dir_b, float radius, float ammo_count, float ammo_count_max)
		{
			var dist = Vector2.Distance(position_a, position_b);
			//var dist = Maths.ProjectedDistance(position_a, dir_b, position_b);
			//var dist_offset = Vector2.Distance(position_b, position_a + (dir * dist));

			//var dir_smooth = Maths.Slerp(dir_a, dir_b, 0.90f);

			var position_actual = position_a + (dir_b * dist);
			var is_los = region.IsInLineOfSight(position_a, position_actual, out var position_actual_los, out var hit_normal, radius: 0.00f, mask: Physics.Layer.World | Physics.Layer.Solid, exclude: Physics.Layer.Ignore_Bullet | Physics.Layer.Creature | Physics.Layer.Background | Physics.Layer.Item | Physics.Layer.Organic | Physics.Layer.Ignore_Hover);

			var dist_los = Vector2.Distance(position_a, position_actual_los);


			//var position_actual_smooth = Maths.Slerp(position_b, position_actual, 0.5f);

			var cpos_muzzle = region.WorldToCanvas(position_a);
			var cpos_cursor = region.WorldToCanvas(position_b);
			var cpos_target = region.WorldToCanvas(position_actual);
			var cpos_target_los = region.WorldToCanvas(position_actual_los);

			//GUI.DrawArc(region.WorldToCanvas(position_a), )

			//if (!GUI.IsHovered) GUI.SetCursor(App.CursorType.Hidden, 100);

			radius = Maths.Min(radius, 20.00f);
			radius *= MathF.Sqrt(dist);
			var line_length = Maths.Clamp(radius * 2.00f, 28.00f, 64.00f);

			var color = new Color32BGRA(0xffff0000);

			if (ammo_count == 0 || gun_state.stage == Gun.Stage.Reloading)
			{
				color = 0xffffff00;
			}
			else
			{
				//if (!is_los)
				//{
				//	color = 0x80808080;
				//}
			}

			var color_circle = color.WithAlphaMult(0.30f);
			var color_crosshair = color;

			//var color_line_a = Color32BGRA.Yellow;
			//var color_line_b = Color32BGRA.Orange;

			var color_line = Color32BGRA.Orange;

			if (!is_los)
			{
				//color_line = Color32BGRA.Gray;
				//color_crosshair = Color32BGRA.Gray;
			}

			var color_line_a = color_line;
			var color_line_b = color_line;

			//var line_thickness_ammo = 2.00f;
			var line_thickness_ammo = Maths.Clamp(16.00f / gun.max_ammo, 2, 8);
			var r_outer = 8.00f;
			var r_inner = 3.00f;


			var angle_a = dir_a.GetAngleRadians();
			var angle_b = dir_b.GetAngleRadians();
			var angle_c = angle_a + Maths.DeltaAngle(angle_b, angle_a);

			//GUI.DrawArc(region.WorldToCanvas(position_a), dir_a, dir_b, radius: dist * region.GetWorldToCanvasScale(), thickness: 8, color: color.WithAlpha(50));
			GUI.DrawArc(region.WorldToCanvas(position_a), Maths.Min(angle_a, angle_c), Maths.Max(angle_a, angle_c), radius: dist * region.GetWorldToCanvasScale(), thickness: 8, color: color_line.WithAlpha(50));

			//GUI.DrawTextCentered($"{angle_a}; {angle_b}", cpos_target);

			//GUI.DrawCircle(cpos_target + new Vector2(0.50f), radius, color_circle);

			//GUI.DrawLine2(cpos_target - (dir_b * Maths.Clamp((dist * 0.35f) - 0.25f, 3.00f, 6.00f) * region.GetWorldToCanvasScale()), cpos_target + (dir_b * Maths.Clamp(dist * 0.50f, 6, 12) * region.GetWorldToCanvasScale()), color_line_a.WithAlpha(150), color_line_b.WithAlpha(250), thickness_a: 1.00f, thickness_b: 1.00f);

			//GUI.DrawLine2(a: cpos_target - (dir_b * Maths.Clamp((dist * 0.35f) - 0.25f, 3.00f, 6.00f) * region.GetWorldToCanvasScale()),
			//	b: cpos_target + (dir_b * Maths.Clamp(dist * 0.50f, 6, 12) * region.GetWorldToCanvasScale()),
			//	color_a: color_line_a.WithAlpha(50),
			//	color_b: color_line_b.WithAlpha(250),
			//	thickness_a: 1.00f,
			//	thickness_b: 1.00f);

			GUI.DrawLine2(a: cpos_muzzle,
			b: cpos_target + (dir_b * Maths.Clamp(dist * 0.50f, 6, 12) * region.GetWorldToCanvasScale()),
			color_a: color_line_a.WithAlpha(30),
			color_b: color_line_b.WithAlpha(50),
			thickness_a: 1.00f,
			thickness_b: 1.00f);



			if (is_los)
			{
				//GUI.DrawLine2(cpos_target - (dir_b * Maths.Clamp((dist * 0.35f) - 0.25f, 3.00f, 6.00f) * region.GetWorldToCanvasScale()), cpos_target + (dir_b * Maths.Clamp(dist * 0.50f, 6, 12) * region.GetWorldToCanvasScale()), color_line_a.WithAlpha(150), color_line_b.WithAlpha(250), thickness_a: 1.00f, thickness_b: 1.00f);

				//GUI.DrawLine2(cpos_target - (dir_b * Maths.Clamp((dist * 0.35f) - 0.25f, 3.00f, 6.00f) * region.GetWorldToCanvasScale()), cpos_target + (dir_b * Maths.Clamp(dist * 0.50f, 6, 12) * region.GetWorldToCanvasScale()), color_line_a.WithAlpha(150), color_line_b.WithAlpha(250), thickness_a: 1.00f, thickness_b: 1.00f);
				//GUI.DrawCircleFilled(cpos_target + new Vector2(0.50f), 3.00f, color, segments: 4);
			}
			else
			{
				//GUI.DrawLine2(cpos_target_los - (dir_b * ((dist_los * 0.35f) - 0.25f).Clamp0X() * region.GetWorldToCanvasScale()), cpos_target_los + (dir_b * Maths.Clamp(dist_los * 0.50f, 6, 12) * region.GetWorldToCanvasScale()), color_line_a.WithAlpha(150), color_line_b.WithAlpha(240), thickness_a: 1.00f, thickness_b: 1.00f);

				GUI.DrawLine2(
					a: cpos_target_los,
					b: cpos_target_los + (dir_b * Maths.Clamp(dist * 0.50f, 6, 12) * region.GetWorldToCanvasScale()),
					color_a: Color32BGRA.Gray.WithAlpha(250),
					color_b: Color32BGRA.Gray.WithAlpha(50),
					thickness_a: 1.00f,
					thickness_b: 1.00f);

				GUI.DrawLine2(
					a: cpos_target_los - (dir_b * Maths.Clamp((dist * 0.35f) - 0.25f, 3.00f, dist_los) * region.GetWorldToCanvasScale()),
					b: cpos_target_los,
					color_a: Color32BGRA.Gray.WithAlpha(50),
					color_b: Color32BGRA.Gray.WithAlpha(250),
					thickness_a: 1.00f,
					thickness_b: 1.00f);

				//GUI.DrawLine2(cpos_target - (dir_b * Maths.Clamp((dist * 0.35f) - 0.25f, 3.00f, 6.00f) * region.GetWorldToCanvasScale()), cpos_target + (dir_b * Maths.Clamp(dist * 0.50f, 6, 12) * region.GetWorldToCanvasScale()), color_line_a.WithAlpha(50), color_line_b.WithAlpha(150), thickness_a: 1.00f, thickness_b: 1.00f);
				//GUI.DrawLine2(cpos_target_los, cpos_target + (dir_b * Maths.Clamp(dist * 0.50f, 6, 12) * region.GetWorldToCanvasScale()), color_line_a.WithAlpha(50), color_line_b.WithAlpha(150), thickness_a: 1.00f, thickness_b: 1.00f);

				//GUI.DrawLine2(cpos_target_los - (dir_b * ((dist_los * 0.35f) - 0.25f).Clamp0X() * region.GetWorldToCanvasScale()), cpos_target_los, color_line_a.WithAlpha(150), color_line_b.WithAlpha(240), thickness_a: 1.00f, thickness_b: 1.00f);
				GUI.DrawCircleFilled(a: cpos_target_los + new Vector2(0.50f), radius: 4.00f, color: Color32BGRA.Gray, segments: 4);
			}

			GUI.DrawCircleFilled(cpos_target + new Vector2(0.50f), 3.00f, color, segments: 4);
			GUI.DrawCircleFilled(cpos_cursor + new Vector2(0.50f), 3.00f, color_line, segments: 4);


			//GUI.DrawLine(cpos_target + new Vector2(-line_length, 0), cpos_target + new Vector2(+line_length, 0), color_line, 1.00f);
			//GUI.DrawLine(cpos_target + new Vector2(0, -line_length), cpos_target + new Vector2(0, +line_length), color_line, 1.00f);



			GUI.DrawLine(cpos_target + new Vector2(+line_length, 0), cpos_target + new Vector2(+r_outer, 0), color_crosshair, 1.00f);
			GUI.DrawLine(cpos_target + new Vector2(-line_length, 0), cpos_target + new Vector2(-r_outer, 0), color_crosshair, 1.00f);

			GUI.DrawLine(cpos_target + new Vector2(0, +line_length), cpos_target + new Vector2(0, +r_outer), color_crosshair, 1.00f);
			GUI.DrawLine(cpos_target + new Vector2(0, -line_length), cpos_target + new Vector2(0, -r_outer), color_crosshair, 1.00f);

			if (ammo_count_max > Resource.epsilon)
			{
				var step = MathF.Tau / ammo_count_max;
				var count = (int)ammo_count_max;

				for (var i = 0; i < count; i++)
				{
					var (sin, cos) = MathF.SinCos(i * -step);

					var is_empty = i >= ammo_count;

					var col = color;
					col = col.WithColorMult(is_empty ? 0.10f : 0.80f);
					if (is_empty) col = col.WithAlphaMult(0.50f);


					//GUI.DrawCircleFilled(cpos_target + (new Vector2(cos, sin) * (is_empty ? 18 : 24)), r, col, segments: 4);
					GUI.DrawLine(cpos_target + (new Vector2(cos, sin) * (is_empty ? 8 : 14)), cpos_target + (new Vector2(cos, sin) * (is_empty ? 16 : 22)), thickness: line_thickness_ammo, color: col);
					//GUI.DrawLine(cpos_target + (new Vector2(cos, sin) * (radius + (is_empty ? -2 : -2))), cpos_target + (new Vector2(cos, sin) * (radius + (is_empty ? 4 : 8))), thickness: line_thickness_ammo, color: col);
				}
			}
		}

		//[ISystem.Render(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void RenderLaserExample(ISystem.Info info, Entity entity,
		//[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] in Gun.Data gun, [Source.Owned] in Animated.Renderer.Data renderer)
		//{
		//	var rmb = control.mouse.GetKey(Mouse.Key.Right);
		//	if (true || rmb)
		//	{
		//		ref var region = ref info.GetRegion();

		//		var aim_dir = transform.GetInterpolatedDirection();
		//		var pos_a = transform.LocalToWorldInterpolated(renderer.offset + gun.muzzle_offset); // + (aim_dir * 0.25f);
		//		var pos_b = pos_a + (aim_dir * 50.00f);

		//		var hit_pos = pos_b;

		//		Span<LinecastResult> results = stackalloc LinecastResult[16];
		//		if (region.TryLinecastAll(pos_a, pos_b, 0.00f, ref results, mask: Physics.Layer.Solid | Physics.Layer.World, exclude: Physics.Layer.Essence | Physics.Layer.Ignore_Bullet))
		//		{
		//			results.SortByDistance();

		//			foreach (ref var result in results)
		//			{
		//				if (result.material_type == Material.Type.Glass) continue;
		//				if (result.alpha <= 0.00f && !result.layer.HasAny(Physics.Layer.Static | Physics.Layer.World)) continue;

		//				hit_pos = result.world_position;

		//				break;
		//			}
		//		}

		//		var dist = Vector2.Distance(pos_a, hit_pos);

		//		var transform_tmp = transform;
		//		transform_tmp.SetPosition(hit_pos);

		//		Projectile.Renderer.Draw(transform_tmp, new()
		//		{
		//			color_a = 0x80ff0000,
		//			color_b = 0x80ff0000,
		//			length = dist
		//		});
		//	}
		//}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent)]
		public static void OnGUI(ISystem.Info info,
		[Source.Owned] in Gun.Data gun, [Source.Owned] in Gun.State state, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned, Pair.Component<Gun.Data>] ref Inventory1.Data inventory,
		[Source.Parent] in Interactor.Data interactor, [Source.Parent] in Player.Data player)
		{
			var gui = new HoldGUI()
			{
				transform = transform,
				//world_position_target = control.mouse.position, // control.mouse.GetInterpolatedPosition(),
				world_position_target = control.mouse.GetInterpolatedPosition(),
				gun_state = state,
				inventory = inventory,
				gun = gun
			};
			gui.Submit();
		}

		// TODO: Shithack
		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUIVehicle(ISystem.Info info,
		[Source.Owned] in Gun.Data gun, [Source.Owned] in Gun.State state, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned, Pair.Component<Gun.Data>] ref Inventory1.Data inventory,
		[Source.Parent] in Vehicle.Data vehicle)
		{
			if (vehicle.ent_seat_driver.IsValid() && vehicle.ent_seat_driver == Client.GetControlledEntity())
			{
				var gui = new HoldGUI()
				{
					transform = transform,
					//world_position_target = control.mouse.position, //control.mouse.GetInterpolatedPosition(),
					world_position_target = control.mouse.GetInterpolatedPosition(),
					gun_state = state,
					inventory = inventory,
					gun = gun
				};
				gui.Submit();
			}
		}
#endif

		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnAddVehicle(ISystem.Info info, Entity ent_gun,
		[Source.Owned] in Gun.Data gun, [Source.Parent] in Joint.Base joint, [Source.Parent] ref Vehicle.Data vehicle)
		{
			if (!vehicle.ent_gun.IsAlive()) vehicle.ent_gun = ent_gun;
		}

		[ISystem.Remove(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnRemVehicle(ISystem.Info info, Entity ent_gun,
		[Source.Owned] in Gun.Data gun, [Source.Parent] in Joint.Base joint, [Source.Parent] ref Vehicle.Data vehicle)
		{
			if (vehicle.ent_gun == ent_gun) vehicle.ent_gun = default;
		}

#if SERVER
		[ISystem.Event<EssenceNode.FailureEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnFailure(ref Region.Data region, ISystem.Info info, Entity entity, ref XorRandom random, ref EssenceNode.FailureEvent data, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Gun.Data gun, [Source.Owned] ref Gun.State gun_state)
		{
			gun_state.stage = Gun.Stage.Jammed;
			gun_state.Sync(entity, true);

			gun.jitter_multiplier += random.NextFloatRange(0.50f, 3.00f);
			gun.stability *= random.NextFloatRange(0.70f, 0.98f);
			gun.failure_rate += random.NextFloatRange(0.02f, 0.08f);
			gun.failure_rate *= random.NextFloatRange(1.10f, 1.80f);
			gun.failure_rate = gun.failure_rate.Clamp01();
			gun.Sync(entity, true);
		}
#endif

#if CLIENT
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateAnimation([Source.Owned] in Gun.State gun_state, [Source.Owned] in Gun.Animation animation, [Source.Owned] ref Animated.Renderer.Data renderer)
		{
			var frame = 0u;

			switch (gun_state.stage)
			{
				case Gun.Stage.Ready:
				{
					frame = gun_state.hints.HasAll(Gun.Hints.Loaded) ? animation.frame_ready_loaded : animation.frame_ready;
				}
				break;

				case Gun.Stage.Fired:
				{
					frame = animation.frame_fired;
				}
				break;

				case Gun.Stage.Cycling:
				{
					frame = animation.frame_cycling;
				}
				break;

				case Gun.Stage.Reloading:
				{
					frame = animation.frame_reloading;
				}
				break;

				case Gun.Stage.Jammed:
				{
					frame = animation.frame_jammed;
				}
				break;
			}

			renderer.sprite.frame.y = frame;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateLight1([Source.Owned] in Gun.State gun_state, [Source.Owned, Pair.Component<Gun.Data>] ref Light.Data light)
		{
			if (gun_state.stage == Gun.Stage.Fired)
			{
				light.intensity = 4.00f;
			}
		}

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateLight2([Source.Owned, Pair.Component<Gun.Data>] ref Light.Data light)
		{
			light.intensity = Maths.Lerp(light.intensity, 0.00f, 0.50f);
		}
#endif

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateReload(ref Region.Data region, ISystem.Info info, Entity entity,
		[Source.Owned] ref Gun.Data gun, [Source.Owned] ref Gun.State gun_state,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control,
		[Source.Owned, Pair.Component<Gun.Data>] ref Inventory1.Data inventory_magazine, [Source.Any] ref Storage.Data storage)
		{
			gun_state.resource_ammo.quantity = inventory_magazine.resource.quantity;

			var time = info.WorldTime;
			if (time < gun_state.next_reload) return;

#if SERVER
			if (gun_state.hints.HasAny(Gun.Hints.Wants_Reload))
			{
				gun_state.burst_rem = gun.burst_count;
				gun_state.hints.RemoveFlag(Gun.Hints.Wants_Reload);
				gun_state.stage = Gun.Stage.Reloading;
				gun_state.Sync(entity, true);

				return;
			}
#endif

			if (gun_state.stage == Gun.Stage.Reloading)
			{
				if ((inventory_magazine.resource.quantity >= gun.max_ammo)) // Fully done reloading
				{
					if (gun.flags.HasAny(Gun.Flags.Cycled_When_Reloaded))
					{
						if (gun_state.hints.TryAddFlag(Gun.Hints.Cycled))
						{

						}
						gun_state.stage = Gun.Stage.Ready;
					}
					else
					{
						gun_state.stage = Gun.Stage.Cycling;
					}

#if SERVER
					gun_state.Sync(entity, true);
#endif
				}
				else if (gun.flags.HasNone(Gun.Flags.Full_Reload) && control.mouse.GetKeyDown(Mouse.Key.Left) && gun_state.hints.HasAny(Gun.Hints.Loaded)) // Wants to shoot mid-reloading (e.g. shotgun)
				{
					gun_state.stage = Gun.Stage.Cycling;

#if SERVER
					gun_state.Sync(entity, true);
#endif
				}
				else if (storage.inv_storage.TryGetHandle(out var h_inventory))  // Reloading
				{
					gun_state.next_reload = info.WorldTime + gun.reload_interval;

					ref var material_ammo = ref inventory_magazine.resource.material.GetData();
					if (material_ammo.IsNotNull() && material_ammo.flags.HasNone(gun.ammo_filter)) inventory_magazine.resource.material = default;

					if (inventory_magazine.resource.material.id == 0 || inventory_magazine.resource.quantity <= Resource.epsilon)
					{
						var count = h_inventory.Length;
						var inventory_span = h_inventory.GetReadOnlySpan();

						for (var i = 0; i < count; i++)
						{
							var resource = inventory_span[i];

							ref var material = ref resource.material.GetData();
							if (material.IsNotNull() && material.flags.HasAny(gun.ammo_filter) && material.ammo.HasValue)
							{
								ref var ammo = ref material.ammo.GetRefOrNull();

								inventory_magazine.resource.material = resource.material;
								gun_state.hints.SetFlag(Gun.Hints.Artillery, material.flags.HasAny(Material.Flags.Explosive));
								gun_state.hints.RemoveFlag(Gun.Hints.No_Ammo);
								gun_state.muzzle_velocity = Maths.Min(gun.velocity_max, gun.velocity_multiplier * ammo.speed_mult);
								gun_state.angle_jitter = Maths.Clamp(gun.jitter_multiplier, 0.00f, 25.00f) * ammo.spread_mult * 0.50f;
								gun_state.resource_ammo = resource;

								break;
							}
						}
					}

					if (inventory_magazine.resource.material != 0) // If magazine knows that ammo it wants to use, withdraw it from parent inventory
					{
#if SERVER
						gun_state.hints.RemoveFlag(Gun.Hints.Cycled);

						var amount = Maths.Clamp(Maths.Min(gun.max_ammo - inventory_magazine.resource.quantity, gun.flags.HasAny(Gun.Flags.Full_Reload) ? gun.max_ammo : 1.00f), 0.00f, gun.max_ammo);
						//App.WriteLine(amount);

						var done_reloading = true;

						if (h_inventory.Withdraw(ref inventory_magazine.resource, ref amount, unlimited: Constants.Requirements.unlimited_ammo))
						{
							done_reloading = false; // Successfully withdrawn, therefore there's still some ammo left to load
							inventory_magazine.flags.AddFlag(Inventory.Flags.Dirty);

							Sound.Play(ref region, gun.sound_reload, transform.position);
						}

						if (done_reloading)
						{
							if (gun.flags.HasAny(Gun.Flags.Cycled_When_Reloaded))
							{
								gun_state.hints.AddFlag(Gun.Hints.Cycled);
								gun_state.stage = Gun.Stage.Ready;
							}
							else
							{
								gun_state.stage = Gun.Stage.Cycling;
							}

							//gun_state.next_reload = 0.00f; // Skip reload timer, so next reload update executes instantly and switches state

							//gun_state.stage = gun.flags.HasAny(Gun.Flags.Cycled_When_Reloaded) ? Gun.Stage.Ready : Gun.Stage.Cycling;
							gun_state.Sync(entity, true);
						}
#endif
					}
					else
					{
						gun_state.next_reload = info.WorldTime + 0.10f;
						gun_state.hints.AddFlag(Gun.Hints.No_Ammo);
#if SERVER
						gun_state.stage = Gun.Stage.Ready;
						gun_state.Sync(entity, true);
#endif
					}
				}
			}
		}

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void OnUpdate(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] ref Gun.Data gun, [Source.Owned] ref Gun.State gun_state, [Source.Owned] ref Body.Data body,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control,
		[Source.Owned, Pair.Component<Gun.Data>] ref Inventory1.Data inventory_magazine,
		[Source.Owned, Optional(true)] ref Heat.Data heat, [Source.Owned, Optional(true)] ref Heat.State heat_state)
		{
			var time = info.WorldTime;
			if (gun_state.stage == Gun.Stage.Fired)
			{
				var pos_w_offset = transform.LocalToWorld(gun.muzzle_offset);
				var pos_w_offset_particle = transform.LocalToWorld(gun.muzzle_offset + gun.particle_offset);
				var dir = transform.GetDirection();
				var dir_particle = dir.RotateByRad(gun.particle_rotation);
				var vel_base = body.GetVelocity();

				var failure_rate = gun.failure_rate;
				var stability_ratio = 1.00f;
				var stability = gun.stability;

#if SERVER
				var force_jammed = false;
#endif

#if CLIENT
				Shake.Emit(ref region, transform.position, gun.shake_amount, gun.shake_amount * 1.25f, gun.shake_radius);
#endif

				ref var material = ref inventory_magazine.resource.material.GetData();
				if (material.IsNotNull() && material.ammo.HasValue)
				{
					ref var ammo = ref material.ammo.GetRefOrNull();

					var velocity_jitter = Maths.Clamp01(gun.jitter_multiplier * 0.20f) * 0.50f;
					var angle_jitter = Maths.Clamp(gun.jitter_multiplier, 0.00f, 25.00f);

					var recoil_mass = ammo.mass * gun.ammo_per_shot;
					var recoil_speed = Maths.Min(gun.velocity_max, gun.velocity_multiplier * ammo.speed_mult);
					var recoil_force = -dir * ((recoil_mass * recoil_speed) * gun.recoil_multiplier * ammo.recoil_mult * App.tickrate * 20.00f);

					//recoil_force = Physics.LimitForce(ref body, recoil_force, new Vector2(50, 50));

					//App.WriteLine($"{body.GetMass() * gun.recoil_multiplier * App.tickrate * 150.00f}; {recoil_force.Length()}");

					//body.AddForceWorld(-dir * body.GetMass() * gun.recoil_multiplier * App.tickrate * 150.00f, pos_w_offset);
					//body.AddForce(recoil_force); //, pos_w_offset);

					failure_rate = Maths.Clamp01((failure_rate + ammo.failure_base) * ammo.failure_mult);
					//stability_ratio = Maths.Clamp01((stability_ratio + ammo.stability_base) * ammo.stability_mult);

					stability *= ammo.stability_mult;
					var stability_req = ammo.stability_base * (1.00f + ((gun.projectile_count - 1) * 0.50f));
					stability_ratio = Maths.Normalize01(stability * Maths.Mulpo(failure_rate * failure_rate, -0.10f), stability_req);

					failure_rate = Maths.Clamp01(failure_rate + ((1.00f - stability_ratio) * 0.10f)); // Maths.Lerp01(failure_rate, 1.00f - stability_ratio, )

					body.AddForceWorld(recoil_force, pos_w_offset); // TODO: use impulse instead
					gun_state.last_recoil = recoil_force;

					//App.WriteLine($"ratio: {stability_ratio}; failure: {failure_rate}; stability: {stability}/{stability_req}");



#if SERVER
					var loaded_ammo = new Resource.Data()
					{
						material = inventory_magazine.resource.material,
						quantity = 0.00f
					};

					var amount = gun.ammo_per_shot;
					Resource.Withdraw(ref inventory_magazine, ref loaded_ammo, ref amount);

					var count = (ammo.count * gun.projectile_count) * (uint)Maths.Normalize(loaded_ammo.quantity, gun.ammo_per_shot);

					if (heat.IsNotNull() && heat_state.IsNotNull())
					{
						if (heat.temperature_operating > Maths.epsilon && ammo.heat > Maths.epsilon)
						{
							//var heat = ((gun.ammo_per_shot - amount) * ammo.heat) / Maths.Max(heat.heat_capacity_extra + (body.GetMass() * 0.10f), 1.00f);
							var heat_amount = ((gun.ammo_per_shot - amount) * Energy.J(ammo.heat)); // / Maths.Max(heat.heat_capacity_extra + (body.GetMass() * 0.10f), 1.00f);
							heat_state.AddEnergy(heat_amount);

							var heat_excess = Maths.Max(heat_state.temperature_current - heat.temperature_high, 0.00f);
							if (heat_excess > Maths.epsilon)
							{
								failure_rate = Maths.Clamp01(failure_rate + (heat_excess * 0.01f));
								stability_ratio = Maths.Clamp01(stability_ratio - (heat_excess * 0.005f));

								angle_jitter *= 1.00f + Maths.Clamp01(heat_excess * 0.01f);
								velocity_jitter *= 1.00f + Maths.Clamp01(heat_excess * 0.005f);
							}

							heat_state.Sync(entity, true);
						}
					}

					//var faction_id = default(IFaction.Handle);

					//if (body_parent.IsNotNull())
					//{

					//}

					var stability_ratio_sub = 1.00f - stability_ratio;
					var explode_chance = Maths.Lerp(failure_rate, Maths.Pow2(stability_ratio_sub), 0.50f);
					//App.WriteLine(explode_chance);

					if (stability_ratio < 1.00f && random.NextBool(explode_chance))
					{
						var explosion_data = new Explosion.Data()
						{
							power = 3.50f + (MathF.Pow(ammo.stability_base * (1.00f + ((gun.projectile_count - 1) * 0.35f)), 0.45f) * 0.05f), // + (count * 0.80f),
							radius = (5.00f + (MathF.Pow(ammo.stability_base * (1.00f + ((gun.projectile_count - 1) * 0.40f)), 0.55f) * 0.05f)) * stability_ratio_sub, // + (count * 2.50f),
							damage_entity = ammo.stability_base * ((1.00f + ((gun.projectile_count - 1) * 1.10f))) * stability_ratio_sub * 2.50f,
							damage_terrain = ammo.stability_base * (1.00f + (gun.projectile_count * 0.80f)) * stability_ratio_sub,
							smoke_amount = 2.10f,
							sparks_amount = 3.00f,
							pitch = 1.50f,
							flash_duration_multiplier = 1.20f,
							flash_intensity_multiplier = 2.70f,
							ent_owner = body.GetParent()
						};

						region.SpawnPrefab("explosion", pos_w_offset).ContinueWith(ent =>
						{
							ref var explosion = ref ent.GetComponent<Explosion.Data>();
							if (explosion.IsNotNull())
							{
								explosion.damage_type = Damage.Type.Shrapnel;
								explosion.damage_type_secondary = Damage.Type.Shrapnel;
								explosion.power = explosion_data.power;
								explosion.radius = explosion_data.radius;
								explosion.damage_entity = explosion_data.damage_entity;
								explosion.damage_terrain = explosion_data.damage_terrain;
								explosion.ent_owner = explosion_data.ent_owner;
								explosion.smoke_amount = explosion_data.smoke_amount;
								explosion.sparks_amount = explosion_data.sparks_amount;
								explosion.pitch = explosion_data.pitch;
								explosion.flash_duration_multiplier = explosion_data.flash_duration_multiplier;
								explosion.flash_intensity_multiplier = explosion_data.flash_intensity_multiplier;

								explosion.Sync(ent, true);
							}
						});

						Sound.Play(ref region, sound_gun_break, pos_w_offset, volume: 1.50f, pitch: 1.10f, size: 1.50f);
						force_jammed = true;

						if (random.NextBool(stability_ratio_sub))
						{
							entity.Delete();
						}
					}
					else
					{
						//var vel_base = body.GetVelocity();
						var vel_min_sq = ammo.velocity_min.Pow2();
						var h_faction = body.GetFaction();
						var ent_owner = body.GetParent();
						var ent_projectile_next = !gun_state.ent_projectile_next.IsAlive() ? gun_state.ent_projectile_next : default;

						var step = Maths.RcpFast(count);
						var step_current = 0.00f;

						for (var i = 0u; i < count; i++)
						{
							step_current += step;

							//var random_multiplier = random.NextFloatRange(0.90f * velocity_jitter, 1.10f);
							var random_multiplier = 1.00f + Maths.Clamp(random.NextFloat(velocity_jitter * step_current), -0.50f, 0.50f);

							var vel = dir.RotateByDeg(random.NextFloat(angle_jitter * 0.50f * ammo.spread_mult)) * Maths.Min(gun.velocity_max, (ammo.speed_base + gun.velocity_multiplier) * random.NextFloatExtra(ammo.speed_mult, -ammo.speed_jitter * step_current) * random_multiplier);
							if (vel.LengthSquared() < vel_min_sq)
							{
								force_jammed = true;
								continue;
							}

							//App.WriteLine(random_multiplier);

							var args =
							(
								damage_mult: gun.damage_multiplier * random_multiplier,
								vel: vel_base + vel,
								ang_vel: random.NextFloatRange(-0.05f, 0.05f) * (angle_jitter + ammo.spin_base) * ammo.spin_mult * random_multiplier,
								ent_owner: ent_owner,
								ent_gun: entity,
								faction_id: h_faction,
								gun_flags: gun.flags,
								drag_jitter: random.NextFloatExtra(1.00f, -ammo.speed_jitter * 0.04f * step_current)
							);

							if (gun.type == Gun.Type.Launcher)
							{
								args.ang_vel += random.NextFloatRange(-30, 30) * failure_rate;
							}

							region.SpawnPrefab(ammo.prefab, pos_w_offset, rotation: args.vel.GetAngleRadians(), velocity: args.vel, angular_velocity: args.ang_vel, entity: ent_projectile_next, faction_id: h_faction).ContinueWith(ent =>
							{
								if (ent.TryGetRecord(out var record))
								{
									ref var projectile = ref ent.GetComponent<Projectile.Data>();
									if (projectile.IsNotNull())
									{
										projectile.damage_base *= args.damage_mult;
										projectile.damage_bonus *= args.damage_mult;
										projectile.velocity = args.vel;
										projectile.angular_velocity = args.ang_vel;
										projectile.ent_owner = args.ent_owner;
										projectile.faction_id = args.faction_id;
										projectile.damp *= args.drag_jitter;
										projectile.Sync(ent, true);
									}

									ref var explosive = ref ent.GetComponent<Explosive.Data>();
									if (explosive.IsNotNull())
									{
										explosive.ent_owner = args.ent_owner;
										explosive.Sync(ent, true);
									}

									if (args.gun_flags.HasAny(Gun.Flags.Child_Projectiles))
									{
										ent.AddRelation(args.ent_gun, Relation.Type.Child);
									}

									if (args.gun_flags.HasAny(Gun.Flags.Rope_Projectiles))
									{
										var ent_child = args.ent_gun.GetChild(Relation.Type.Rope);
										if (ent_child.IsAlive())
										{
											ent_child.RemoveRelation(entity, Relation.Type.Rope);
											ent_child.Delete();
										}
										ent.AddRelation(args.ent_gun, Relation.Type.Rope);
									}
								}
							});

							if (ent_projectile_next != 0)
							{
								ent_projectile_next = default;
								//gun_state.ent_projectile_next = default;
							}
						}

						if (gun.shake_amount > 0.50f)
						{
							var shockwave_radius = Maths.Clamp(((gun.shake_amount * gun.shake_radius) * 0.12f), 0.00f, 24.00f);
							if (shockwave_radius >= 4.00f)
							{
								var shake_amount = gun.shake_amount * 0.50f;
								//App.WriteLine(shockwave_radius);
								Explosion.Spawn(ref region, pos_w_offset + (dir * 1.75f), (Entity ent_explosion, ref Explosion.Data explosion) =>
								{
									explosion.power = 2.00f;
									explosion.radius = shockwave_radius;
									explosion.damage_entity = 10.00f * shockwave_radius;
									explosion.damage_terrain = 10.00f * shockwave_radius;
									explosion.damage_type = Damage.Type.Shockwave;
									explosion.damage_type_secondary = Damage.Type.Shockwave;
									explosion.ent_owner = entity;
									explosion.fire_amount = 0.00f;
									explosion.smoke_amount = 0.00f;
									explosion.smoke_lifetime_multiplier = 1.00f;
									explosion.smoke_velocity_multiplier = 1.00f;
									explosion.flash_duration_multiplier = 0.00f;
									explosion.flash_intensity_multiplier = 0.00f;
									explosion.sparks_amount = 0.00f;
									explosion.volume = 0.00f;
									explosion.pitch = 0.00f;
									explosion.stun_multiplier = 1.40f;
									explosion.shake_multiplier = shake_amount;
									explosion.force_multiplier = 0.20f;
									explosion.flags |= Explosion.Flags.No_Split;
									explosion.ent_ignored = entity;

									explosion.Sync(ent_explosion, true);
								});
							}
						}
					}
#endif
				}

				gun_state.eject_rem += (byte)Maths.Min(gun.ammo_per_shot, gun_state.resource_ammo.quantity);
				gun_state.hints.AddFlag(Hints.Eject_Pending);

				if (gun.flags.HasAny(Gun.Flags.Cycle_On_Shoot))
				{
					gun_state.stage = Gun.Stage.Cycling;
				}
				else
				{
					gun_state.stage = Gun.Stage.Ready;
				}

				gun_state.next_reload = time + Maths.Lerp(gun.cycle_interval * 3.00f, gun.reload_interval * 0.50f, 0.50f);

#if SERVER
				if (force_jammed || random.NextBool(failure_rate))
				{
					//App.WriteLine("jammed");

					gun_state.stage = Gun.Stage.Jammed;
					gun_state.Sync(entity, true);

					Sound.Play(ref region, gun.sound_jam, pos_w_offset, volume: 1.10f, pitch: 1.00f, size: 1.50f);
					WorldNotification.Push(ref region, "* Jammed *", 0xffff0000, transform.position, lifetime: 1.00f);
				}
#endif

#if CLIENT
				if (gun.flags.HasNone(Gun.Flags.No_Particles))
				{
					if (gun.flash_size > Maths.epsilon)
					{
						Particle.Spawn(ref region, new Particle.Data()
						{
							texture = texture_muzzle_flash,
							pos = transform.LocalToWorld(gun.muzzle_offset + gun.particle_offset + new Vector2(1.50f * gun.flash_size, 0.00f).RotateByRad(gun.particle_rotation)),
							lifetime = 0.25f,
							fps = 24,
							frame_count = 6,
							frame_count_total = 6,
							scale = gun.flash_size,
							lit = 1.00f,
							rotation = transform.rotation + gun.particle_rotation + (transform.scale.X.IsNegative() ? MathF.PI : 0),
							vel = vel_base
						});
					}

					if (gun.smoke_size > Maths.epsilon && gun.smoke_amount > Maths.epsilon)
					{
						var smoke_count = (int)gun.smoke_amount;
						for (var i = 0; i < smoke_count; i++)
						{
							Particle.Spawn(ref region, new Particle.Data()
							{
								texture = texture_smoke,
								pos = pos_w_offset_particle + (dir_particle * i * 0.50f),
								lifetime = random.NextFloatRange(2.00f, 8.00f),
								fps = random.NextByteRange(12, 16),
								frame_count = 64,
								frame_count_total = 64,
								frame_offset = random.NextByteRange(0, 64),
								scale = random.NextFloatRange(0.05f, 0.10f) * gun.smoke_size,
								angular_velocity = random.NextFloatRange(-0.10f, 0.10f),
								vel = vel_base + (dir_particle * random.NextFloatRange(1.00f, 1.50f)),
								force = new Vector2(0, -random.NextFloatRange(0.00f, 0.20f)) + (dir_particle * random.NextFloatRange(0.05f, 0.20f)),
								rotation = random.NextFloat(10.00f),
								growth = random.NextFloatRange(0.15f, 0.30f),
								drag = random.NextFloatRange(0.07f, 0.10f),
								color_a = random.NextColor32Range(new Color32BGRA(255, 240, 240, 240), new Color32BGRA(255, 220, 220, 220)).WithAlphaMult(Maths.Clamp(0.40f + ((smoke_count - i) * 0.04f), 0.20f, 1.00f)),
								color_b = new Color32BGRA(000, 150, 150, 150)
							});
						}
					}
				}

				var pitch_modifier = 1.00f;
				if (heat.IsNotNull() && heat_state.IsNotNull())
				{
					pitch_modifier += Maths.InvLerp01(heat.temperature_medium, heat.temperature_high, heat_state.temperature_current) * 0.08f;
				}
				pitch_modifier *= random.NextFloatRange(0.98f, 1.02f);

				Sound.Play(gun.sound_shoot, pos_w_offset, volume: gun.sound_volume, pitch: gun.sound_pitch * pitch_modifier, size: gun.sound_size, priority: 0.70f, dist_multiplier: gun.sound_dist_multiplier);
#endif
			}

			if (gun_state.stage == Stage.Reloading || gun.flags.HasAny(Gun.Flags.Eject_On_Shoot) || (gun.action != Action.Revolver && gun_state.hints.HasAny(Hints.Cycled)))
			{
				if (gun_state.eject_rem > 0 && gun_state.hints.TryRemoveFlag(Hints.Eject_Pending))
				{
#if CLIENT
					ref var material = ref gun_state.resource_ammo.material.GetData();
					if (material.IsNotNull())
					{
						ref var ammo = ref material.ammo.GetRefOrNull();
						if (ammo.IsNotNull() && ammo.sprite_casing.texture.id != 0)
						{
							var pos_eject = transform.LocalToWorld(gun.flags.HasAny(Flags.Eject_From_Muzzle) ? gun.muzzle_offset : gun.receiver_offset);
							var dir_eject = transform.LocalToWorldDirection(gun.eject_direction);

							var casing_count = (uint)gun_state.eject_rem; // gun_state.resource_ammo.quantity
							for (var i = 0; i < casing_count; i++)
							{
								Particle.Spawn(ref region, new Particle.Data()
								{
									texture = ammo.sprite_casing.texture,
									pos = pos_eject,
									lifetime = random.NextFloatRange(0.90f, 1.20f) * ammo.casing_scale,
									fps = 0,
									frame_count = 1,
									frame_count_total = 32,
									frame_offset = (byte)ammo.sprite_casing.frame.x,
									scale = ammo.casing_scale,
									rotation = transform.rotation,
									angular_velocity = random.NextFloatRange(-1.00f, 1.00f) * gun.eject_angular_velocity,
									vel = (dir_eject * random.NextFloatRange(1.00f, 1.50f)) + random.NextUnitVector2Range(0.20f, 1.20f),
									force = new Vector2(0, random.NextFloatRange(25.00f, 30.00f)),
									growth = -random.NextFloatRange(0.65f, 0.70f),
									drag = random.NextFloatRange(0.004f, 0.010f),
									color_a = ColorBGRA.White,
									color_b = ColorBGRA.White,
								});
							}

							Sound.Play(ref region, gun.sound_eject, transform.position, volume: 0.80f);
						}
					}

					//Sound.Play(ref region, gun.sound_cycle, transform.position, volume: 0.50f);
#endif

					gun_state.eject_rem = 0;
				}
			}

			switch (gun_state.stage)
			{
				case Gun.Stage.Cycling:
				{
					if (time < gun_state.next_cycle) break;

					if (gun_state.hints.HasAny(Gun.Hints.Cycled))
					{
						gun_state.stage = Gun.Stage.Ready;
					}
					else
					{
						var cycle_interval = gun.cycle_interval;
						//if (!gun.flags.HasAll(Gun.Flags.Automatic)) cycle_interval = gunslinger.ApplyShootSpeed(cycle_interval);

						gun_state.next_cycle = info.WorldTime + cycle_interval;
						if (gun_state.hints.TryAddFlag(Gun.Hints.Cycled))
						{
#if CLIENT
							//							ref var material = ref inventory_magazine.resource.material.GetData();
							//							if (material.IsNotNull())
							//							{
							//								ref var ammo = ref material.ammo.GetRefOrNull();
							//								if (ammo.IsNotNull() && ammo.sprite_casing.texture.id != 0) 
							//								{
							//									Particle.Spawn(ref region, new Particle.Data()
							//									{
							//										texture = ammo.sprite_casing.texture,
							//										pos = transform.LocalToWorld(gun.receiver_offset),
							//										lifetime = random.NextFloatRange(1.10f, 1.30f),
							//										fps = 0,
							//										frame_count = 1,
							//										frame_count_total = 8,
							//										frame_offset = (byte)ammo.sprite_casing.frame.X,
							//										scale = random.NextFloatRange(0.85f, 1.00f),
							//										angular_velocity = random.NextFloatRange(-1.00f, 1.00f) * gun.eject_angular_velocity,
							//										vel = transform.LocalToWorldDirection(gun.eject_direction) * random.NextFloatRange(1.00f, 1.50f),
							//										force = new Vector2(0, random.NextFloatRange(25.00f, 30.00f)),
							//										growth = -random.NextFloatRange(0.75f, 0.90f),
							//										drag = random.NextFloatRange(0.008f, 0.012f),
							//										color_a = ColorBGRA.White,
							//										color_b = ColorBGRA.White,
							//									});
							//								}
							//							}

							//Sound.Play(ref region, gun.sound_cycle, transform.position, volume: 0.50f);
#endif
						}

#if SERVER
						Sound.Play(ref region, gun.sound_cycle, transform.LocalToWorld(gun.receiver_offset), volume: 0.50f);
#endif
					}
				}
				break;
			}
		}

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.50f)]
		public static void UpdateAimable([Source.Owned] in Gun.Data gun, [Source.Owned] ref Aimable.Data aimable)
		{
			aimable.offset = gun.muzzle_offset; // new Vector2(0.00f, gun.muzzle_offset.Y);
			aimable.deadzone = gun.muzzle_offset.Length();
		}

		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region)]
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.50f)]
		public static void UpdateHoldable([Source.Owned] in Gun.Data gun, [Source.Owned] in Gun.State gun_state, [Source.Owned] ref Holdable.Data holdable, [Source.Owned] ref Body.Data body)
		{
			holdable.hints.AddFlag(NPC.ItemHints.Weapon | NPC.ItemHints.Gun);
			holdable.hints.SetFlag(NPC.ItemHints.Short_Range, gun_state.hints.HasAny(Gun.Hints.Close_Range));
			holdable.hints.SetFlag(NPC.ItemHints.Long_Range, gun_state.hints.HasAny(Gun.Hints.Long_Range));
			holdable.hints.SetFlag(NPC.ItemHints.Usable, !gun_state.hints.HasAny(Gun.Hints.No_Ammo));
			holdable.hints.SetFlag(NPC.ItemHints.Heavy, body.GetMass() >= 30.00f);
			holdable.hints.SetFlag(NPC.ItemHints.Inaccurate, gun.jitter_multiplier >= 1.50f);
			holdable.hints.SetFlag(NPC.ItemHints.Junk, gun.failure_rate >= 0.10f || gun.jitter_multiplier >= 5.00f);
		}

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void OnReady(ISystem.Info info, ref Region.Data region, Entity entity, ref XorRandom random,
		[Source.Owned] ref Gun.Data gun, [Source.Owned] ref Gun.State gun_state,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] ref Body.Data body,
		[Source.Owned, Pair.Component<Gun.Data>] ref Inventory1.Data inventory_magazine)
		{
			gun_state.hints.SetFlag(Gun.Hints.Loaded, inventory_magazine.resource.quantity > Resource.epsilon && inventory_magazine.resource.material.id != 0);
			gun_state.hints.SetFlag(Gun.Hints.Supressive_Fire, gun.max_ammo >= 10.00f && (gun.cycle_interval <= 0.10f || (gun.cycle_interval <= 0.20f && gun.flags.HasAny(Gun.Flags.Automatic))));
			gun_state.hints.SetFlag(Gun.Hints.Close_Range, gun.type == Gun.Type.Shotgun || (gun.heuristic_range <= 15.00f && gun.reload_interval <= 0.50f));
			gun_state.hints.SetFlag(Gun.Hints.Long_Range, gun.heuristic_range > 15.00f && gun.jitter_multiplier <= 0.05f);

			if (gun_state.stage == Gun.Stage.Ready)
			{
				if (control.keyboard.GetKeyDown(Keyboard.Key.Reload) || (control.mouse.GetKeyDown(Mouse.Key.Left) && gun_state.hints.HasNone(Gun.Hints.Loaded)))
				{
#if SERVER

					//gun_state.stage = Gun.Stage.Reloading;
					gun_state.hints.AddFlag(Gun.Hints.Wants_Reload);
					gun_state.Sync(entity, true);
#endif
					return;
				}

				if (control.mouse.GetKeyDown(Mouse.Key.Left) && gun.flags.HasNone(Gun.Flags.No_LMB_Cycle) && gun_state.hints.HasNone(Gun.Hints.Cycled))
				{
#if SERVER
					gun_state.stage = Gun.Stage.Cycling;
					gun_state.Sync(entity, true);
#endif
					return;
				}

				if (gun_state.hints.HasAny(Gun.Hints.Cycled) && (control.mouse.GetKeyDown(Mouse.Key.Left) || (gun.flags.HasAny(Gun.Flags.Automatic) && (control.mouse.GetKey(Mouse.Key.Left) && (gun.burst_count <= 1 || gun_state.burst_rem > 0)))))
				{
					if (inventory_magazine.resource.quantity > Resource.epsilon && inventory_magazine.resource.material.id != 0)
					{
						if (control.mouse.GetKeyDown(Mouse.Key.Left) && (gun.flags.HasAny(Gun.Flags.Reset_Bursts) || gun_state.burst_rem == 0))
						{
							gun_state.burst_rem = gun.burst_count;
						}

#if SERVER


						if (gun.burst_count <= 1 || gun_state.burst_rem > 0)
						{
							gun_state.stage = Gun.Stage.Fired;
							gun_state.hints.RemoveFlag(Gun.Hints.Cycled);
							gun_state.burst_rem = (byte)Maths.Clamp(gun_state.burst_rem - 1, 0, gun.burst_count);
						}

						gun_state.Sync(entity, true);
#endif
					}
					else
					{
						gun_state.stage = gun.flags.HasAny(Gun.Flags.Automatic) ? Gun.Stage.Ready : Gun.Stage.Cycling;
						gun_state.hints.RemoveFlag(Gun.Hints.Cycled);

#if SERVER
						Sound.Play(ref region, gun.sound_empty, transform.position, volume: 0.50f);
						WorldNotification.Push(ref region, "* No ammo *", 0xffff0000, transform.position);
#endif
					}

					return;
				}
			}
			else if (gun_state.stage == Gun.Stage.Jammed)
			{
				if (info.WorldTime >= gun_state.next_cycle && (control.mouse.GetKeyDown(Mouse.Key.Left) || control.keyboard.GetKeyDown(Keyboard.Key.Reload)))
				{
					body.AddImpulse(transform.Down.RotateByDeg(random.NextFloatRange(-20.00f, 20.00f)) * Maths.Min(500, body.GetMass() * random.NextFloatRange(7.50f, 15.00f)));

#if SERVER
					if (random.NextBool(Maths.Max(0.10f, 1.00f - (0.40f + (gun.failure_rate * 5.00f)))))
					{
						//App.WriteLine("unjammed");

						//gun_state.stage = Gun.Stage.Reloading;

						gun_state.hints.AddFlag(Gun.Hints.Wants_Reload);
						gun_state.next_cycle = info.WorldTime + gun.reload_interval;
						gun_state.Sync(entity, true);

						Sound.Play(ref region, gun.sound_jam, transform.position, volume: 0.50f, pitch: random.NextFloatRange(0.70f, 0.95f), size: 1.10f);
						WorldNotification.Push(ref region, "* Unjammed *", 0xff00ff00, transform.position);
					}
					else
					{
						gun_state.stage = Gun.Stage.Jammed;
						gun_state.next_cycle = info.WorldTime + gun.cycle_interval;
						gun_state.Sync(entity, true);

						Sound.Play(ref region, gun.sound_jam, transform.position, volume: 0.50f, pitch: random.NextFloatRange(0.70f, 0.95f), size: 1.10f);
						WorldNotification.Push(ref region, "* Jammed *", 0xffff0000, transform.position);
					}
#endif
				}
			}
		}
	}
}
