namespace TC2.Base.Components
{
	public static partial class Gun
	{
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";
		public static readonly Texture.Handle texture_muzzle_flash = "MuzzleFlash";

		public static readonly Sound.Handle sound_gun_break = "gun_break";

		public enum Stage: uint
		{
			Ready,
			Fired,
			Cycling,
			Reloading,
			Jammed
		}

		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Automatic = 1u << 0,
			Full_Reload = 1u << 1,

			Cycle_On_Shoot = 1u << 2,
			Manual_Cycle = 1u << 3,

			No_Particles = 1u << 4,

			Child_Projectiles = 1u << 5
		}

		public enum Type: byte
		{
			Undefined = 0,

			Handgun,
			Shotgun,
			SMG,
			Rifle,
			MachineGun,
			Cannon,
			AutoCannon,
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
			Front
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
			Crank
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
		public enum Hints: uint
		{
			None = 0,

			//Can_Reload = 1 << 0,
			//Can_Shoot = 1 << 1,
			Cycled = 1 << 2,
			Loaded = 1 << 3,
			Wants_Reload = 1 << 4
		}

		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Gun.Data>]
		public partial struct Animation: IComponent
		{
			public uint frame_ready;
			public uint frame_ready_loaded;
			public uint frame_fired;
			public uint frame_cycling;
			public uint frame_reloading;
			public uint frame_jammed;
		}

		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Gun.State>]
		public partial struct Data: IComponent
		{
			public static readonly Sound.Handle sound_jam_default = "gun_jam";

			public Vector2 muzzle_offset = default;
			public Vector2 particle_offset = default;
			public float particle_rotation = default;

			public Sound.Handle sound_shoot = default;
			public Sound.Handle sound_cycle = default;
			public Sound.Handle sound_reload = default;
			public Sound.Handle sound_empty = default;
			public Sound.Handle sound_jam = sound_jam_default;

			[Statistics.Info("Damage", description: "Damage dealt by the fired projectile.", format: "{0:0.##}x", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float damage_multiplier = default;

			[Statistics.Info("Muzzle Velocity", description: "Velocity of the fired projectile.", format: "{0:0.##} m/s", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float velocity_multiplier = default;

			[Statistics.Info("Spread", description: "Spread of the fired projectiles.", format: "{0:0.##}x", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.High)]
			public float jitter_multiplier = default;

			[Statistics.Info("Recoil", description: "Force applied after firing the weapon.", format: "{0:0.##}x", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Medium)]
			public float recoil_multiplier = default;

			[Statistics.Info("Reload Speed", description: "Time to reload the weapon.", format: "{0:0.##}s", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Medium)]
			public float reload_interval = default;

			[Statistics.Info("Cycle Speed", description: "Rate of fire.", format: "{0:0.##}s", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.High)]
			public float cycle_interval = default;

			[Statistics.Info("Stability", description: "Reliability, may result in a catastrophic failure if too low.", format: "{0:P2}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Low)]
			public float stability = 1.00f;

			[Statistics.Info("Failure Rate", description: "Chance of malfunction, such as jamming after being fired.", format: "{0:P2}", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Low)]
			public float failure_rate = 0.00f;

			[Statistics.Info("Maximum Ammunition", description: "Ammo capacity.", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float max_ammo = default;

			[Statistics.Info("Ammunition Usage", description: "Ammo used per shot.", format: "{0:0}", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Medium)]
			public float ammo_per_shot = 1.00f;

			[Statistics.Info("Barrel Count", description: "Number of barrels.", format: "{0:0}", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Medium)]
			public int barrel_count = 1;

			[Statistics.Info("Loudness", description: "Loudness of the shot.", format: "{0:0.##}x", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Low)]
			public float sound_volume = 1.25f;

			public float sound_size = 1.00f;
			public float sound_dist_multiplier = 4.00f;

			public float sound_pitch = 1.00f;

			public float flash_size = 1.00f;

			public float smoke_size = 1.00f;
			public float smoke_amount = 1.00f;

			public float shake_amount = 0.20f;

			public float heuristic_range = 30.00f;

			[Statistics.Info("Projectile Count", description: "Number of projectiles fired per shot.", format: "{0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public int projectile_count = 1;

			public Gun.Flags flags = default;

			[Statistics.Info("Ammo", description: "Ammunition type.", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
			public Material.Flags ammo_filter = default;

			[Statistics.Info("Operation", description: "Operation mode of the weapon.", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			public Gun.Action action = default;

			[Statistics.Info("Type", description: "Type of the weapon.", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			public Gun.Type type = default;

			[Statistics.Info("Feed", description: "Method of loading ammunition.", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			public Gun.Feed feed = default;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct State: IComponent
		{
			public Gun.Stage stage;
			public Gun.Hints hints;

			[Save.Ignore, Net.Ignore] public float next_cycle;
			[Save.Ignore, Net.Ignore] public float next_reload;
		}

#if CLIENT
		public struct HoldGUI: IGUICommand
		{
			public Transform.Data transform;
			public Vector2 world_position_target;

			public Gun.Data gun;
			public Inventory1.Data inventory;
			public Gun.State state;

			public void Draw()
			{
				var dir_a = (this.world_position_target - this.transform.GetInterpolatedPosition()).GetNormalized();
				var dir_b = this.transform.GetInterpolatedDirection();

				ref var region = ref Client.GetRegion();

				using (var window = GUI.Window.HUD("Crosshair", GUI.WorldToCanvas(this.world_position_target), size: new(100, 100)))
				{
					if (window.show)
					{
						if (this.state.stage == Gun.Stage.Reloading)
						{
							GUI.TitleCentered($"Reloading\n{MathF.Max(this.state.next_reload - region.GetFixedTime(), 0.00f):0.00}", pivot: new(0.50f));
						}
					}
				}

				//GUI.DrawCrosshair(this.transform.GetInterpolatedPosition(), this.world_position_target, this.transform.GetInterpolatedDirection(), this.gun.jitter_multiplier, this.inventory[0].quantity, this.gun.max_ammo);
				GUI.DrawCrosshair(this.transform.GetInterpolatedPosition(), this.world_position_target, Vector2.Lerp(dir_a, dir_b, 0.25f), this.gun.jitter_multiplier, this.inventory[0].quantity, this.gun.max_ammo);
			}
		}

		[ISystem.GUI(ISystem.Mode.Single)]
		public static void OnGUI(ISystem.Info info,
		[Source.Owned] in Gun.Data gun, [Source.Owned] in Gun.State state, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned, Pair.Of<Gun.Data>] ref Inventory1.Data inventory,
		[Source.Parent] in Interactor.Data interactor, [Source.Parent] in Player.Data player)
		{
			if (player.IsLocal())
			{
				var gui = new HoldGUI()
				{
					transform = transform,
					world_position_target = control.mouse.position,
					state = state,
					inventory = inventory,
					gun = gun
				};
				gui.Submit();
			}
		}
#endif

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

			renderer.sprite.frame.Y = frame;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateLight1([Source.Owned] in Gun.State gun_state, [Source.Owned, Pair.Of<Gun.Data>] ref Light.Data light)
		{
			if (gun_state.stage == Gun.Stage.Fired)
			{
				light.intensity = 4.00f;
			}
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateLight2([Source.Owned, Pair.Of<Gun.Data>] ref Light.Data light)
		{
			light.intensity = Maths.Lerp(light.intensity, 0.00f, 0.50f);
		}

		[ISystem.Update(ISystem.Mode.Single)]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void UpdateReload<T>(ISystem.Info info, Entity entity,
		[Source.Owned] ref Gun.Data gun, [Source.Owned] ref Gun.State gun_state,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control,
		[Source.Owned, Pair.Of<Gun.Data>] ref Inventory1.Data inventory_magazine, [Source.Any, Pair.Of<Storage.Data>] ref T inventory,
		[Source.Parent, Optional] in Specialization.Gunslinger.Data gunslinger) where T : unmanaged, IInventory
		{
#if SERVER
			if (gun_state.hints.HasAny(Gun.Hints.Wants_Reload))
			{
				gun_state.hints.SetFlag(Gun.Hints.Wants_Reload, false);
				gun_state.stage = Gun.Stage.Reloading;
				gun_state.Sync(entity);

				return;
			}
#endif

			if (gun_state.stage == Gun.Stage.Reloading)
			{
				var time = info.WorldTime;
				if (time < gun_state.next_reload) return;
				ref var region = ref info.GetRegion();

				if (inventory_magazine.resource.quantity >= gun.max_ammo)
				{
					gun_state.stage = Gun.Stage.Cycling;
				}
				else if (!gun.flags.HasAll(Gun.Flags.Full_Reload) && control.mouse.GetKeyDown(Mouse.Key.Left) && gun_state.hints.HasAll(Gun.Hints.Loaded))
				{
#if SERVER
					gun_state.stage = Gun.Stage.Cycling;
					gun_state.Sync(entity);

					return;
#endif
				}
				else
				{
					gun_state.next_reload = info.WorldTime + gunslinger.ApplyReloadSpeed(gun.reload_interval);
					if (inventory_magazine.resource.material.id != 0 && !inventory_magazine.resource.material.GetDefinition().flags.HasAll(gun.ammo_filter)) inventory_magazine.resource.material = default;

					if (inventory_magazine.resource.material.id == 0 || inventory_magazine.resource.quantity <= float.Epsilon)
					{
						var count = inventory.Length;
						for (var i = 0; i < count; i++)
						{
							ref var resource = ref inventory[i];
							if (resource.material.GetDefinition().flags.HasAll(gun.ammo_filter))
							{
								inventory_magazine.resource.material = resource.material;
								break;
							}
						}
					}

					if (inventory_magazine.resource.material != 0)
					{
#if SERVER
						gun_state.hints.SetFlag(Gun.Hints.Cycled, false);

						var amount = Maths.Clamp(MathF.Min(gun.max_ammo - inventory_magazine.resource.quantity, gun.flags.HasAll(Gun.Flags.Full_Reload) ? gun.max_ammo : gunslinger.ApplyBulkReload(1.00f)), 0.00f, gun.max_ammo);
						//App.WriteLine(amount);

						var done = true;

						if (Resource.Withdraw(ref inventory, ref inventory_magazine.resource, ref amount))
						{
							done = false;
							inventory_magazine.flags.SetFlag(Inventory.Flags.Dirty, true);

							Sound.Play(ref region, gun.sound_reload, transform.position);
						}

						if (done)
						{
							gun_state.stage = Gun.Stage.Cycling;
							gun_state.Sync(entity);

							return;
						}
#endif
					}
					else
					{
						gun_state.next_reload = info.WorldTime + 0.10f;
#if SERVER
						gun_state.stage = Gun.Stage.Ready;
						gun_state.Sync(entity);
#endif
						return;
					}
				}
			}
		}

		[ISystem.Update(ISystem.Mode.Single)]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void OnUpdate(ISystem.Info info, Entity entity,
		[Source.Owned] ref Gun.Data gun, [Source.Owned] ref Gun.State gun_state, [Source.Owned] ref Body.Data body,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control,
		[Source.Owned, Pair.Of<Gun.Data>] ref Inventory1.Data inventory_magazine,
		[Source.Parent, Optional] in Specialization.Gunslinger.Data gunslinger, [Source.Parent, Optional] in Faction.Data faction, [Source.Owned, Optional(true)] ref Overheat.Data overheat)
		{
			var time = info.WorldTime;
			ref var region = ref info.GetRegion();
			if (gun_state.stage == Gun.Stage.Fired)
			{
				var pos_w_offset = transform.LocalToWorld(gun.muzzle_offset);
				var pos_w_offset_particle = transform.LocalToWorld(gun.muzzle_offset + gun.particle_offset);
				var dir = transform.GetDirection();
				var dir_particle = dir.RotateByRad(gun.particle_rotation);
				var random = XorRandom.New();
				var base_vel = body.GetVelocity();

				body.AddImpulseWorld(-dir * 70.00f * gun.recoil_multiplier, pos_w_offset);

				var failure_rate = gun.failure_rate;
				var stability = gun.stability;

				var force_jammed = false;

#if CLIENT
				Shake.Emit(ref region, transform.position, gun.shake_amount, gun.shake_amount * 1.25f, 16.00f);
#endif

#if SERVER
				ref var material = ref inventory_magazine.resource.material.GetDefinition();
				if (material.projectile_prefab.id != 0)
				{
					var loaded_ammo = new Resource.Data()
					{
						material = inventory_magazine.resource.material,
						quantity = 0.00f
					};

					var amount = gun.ammo_per_shot;
					Resource.Withdraw(ref inventory_magazine, ref loaded_ammo, ref amount);

					var count = (material.projectile_count * gun.projectile_count) * (loaded_ammo.quantity / gun.ammo_per_shot);

					var velocity_jitter = 1.00f - (Maths.Clamp(gun.jitter_multiplier * 0.20f, 0.00f, 1.00f) * 0.50f);
					var angle_jitter = Maths.Clamp(gun.jitter_multiplier, 0.00f, 25.00f);

					if (!overheat.IsNull())
					{
						if (overheat.heat_critical > 0.00f && material.projectile_heat > 0.00f)
						{
							var heat = ((gun.ammo_per_shot - amount) * material.projectile_heat) / MathF.Max(body.GetMass() * 0.10f, 1.00f);
							overheat.heat_current += heat;

							var heat_excess = MathF.Max(overheat.heat_current - overheat.heat_critical, 0.00f);
							if (heat_excess > 0.00f)
							{
								failure_rate = Maths.Clamp(failure_rate + (heat_excess * 0.01f), 0.00f, 1.00f);
								stability = Maths.Clamp(stability - (heat_excess * 0.005f), 0.00f, 1.00f);

								angle_jitter *= 1.00f + Maths.Clamp01(heat_excess * 0.01f);
								velocity_jitter *= 1.00f + Maths.Clamp01(heat_excess * 0.005f);
							}

							overheat.Sync(entity);
						}
					}

					{
						for (var i = 0; i < count; i++)
						{
							var random_multiplier = random.NextFloatRange(0.90f * velocity_jitter, 1.10f);

							var args =
							(
								damage_mult: gun.damage_multiplier * random_multiplier,
								vel: dir.RotateByDeg(random.NextFloat(angle_jitter * 0.50f * material.projectile_spread_mult)) * gun.velocity_multiplier * material.projectile_speed_mult * random_multiplier,
								ent_owner: body.GetParent(),
								ent_gun: entity,
								faction_id: faction.id,
								gun_flags: gun.flags
							);

							if (args.vel.LengthSquared() < (material.projectile_velocity_min * material.projectile_velocity_min))
							{
								force_jammed = true;
								break;
							}
							else
							{
								region.SpawnPrefab(material.projectile_prefab, pos_w_offset, rotation: args.vel.GetAngleRadians(), velocity: args.vel, angular_velocity: dir.GetParity()).ContinueWith(ent =>
								{
									ref var projectile = ref ent.GetComponent<Projectile.Data>();
									if (!projectile.IsNull())
									{
										projectile.damage_base *= args.damage_mult;
										projectile.damage_bonus *= args.damage_mult;
										projectile.velocity = args.vel;
										projectile.ent_owner = args.ent_owner;
										projectile.faction_id = args.faction_id;

										ent.SyncComponent(ref projectile);
									}

									ref var explosive = ref ent.GetComponent<Explosive.Data>();
									if (!explosive.IsNull())
									{
										explosive.ent_owner = args.ent_owner;

										ent.SyncComponent(ref explosive);
									}

									if (args.gun_flags.HasAll(Gun.Flags.Child_Projectiles))
									{
										ent.AddRelation(args.ent_gun, Relation.Type.Child);
									}
								});
							}
						}
					}

					if (stability < 1.00f && random.NextBool(failure_rate) && random.NextBool(1.00f - stability))
					{
						var explosion_data = new Explosion.Data()
						{
							power = 1.50f, // + (count * 0.80f),
							radius = 5.50f, // + (count * 2.50f),
							damage_entity = (gun.damage_multiplier * (1.00f + ((count - 1) * 3.80f))) * 200.00f,
							damage_terrain = (gun.damage_multiplier * (1.00f + (count * 0.50f))) * 130.00f,
							smoke_amount = 0.30f,
							sparks_amount = 2.00f,
							ent_owner = body.GetParent()
						};

						region.SpawnPrefab("explosion", transform.position).ContinueWith(x =>
						{
							ref var explosion = ref x.GetComponent<Explosion.Data>();
							if (!explosion.IsNull())
							{
								explosion.power = explosion_data.power;
								explosion.radius = explosion_data.radius;
								explosion.damage_entity = explosion_data.damage_entity;
								explosion.damage_terrain = explosion_data.damage_terrain;
								explosion.ent_owner = explosion_data.ent_owner;
								explosion.smoke_amount = explosion_data.smoke_amount;

								explosion.Sync(x);
							}
						});

						Sound.Play(ref region, sound_gun_break, pos_w_offset, volume: 1.50f, pitch: 1.10f, size: 1.50f);

						entity.Delete();
					}
				}
#endif

				if (force_jammed || gun.flags.HasAll(Gun.Flags.Cycle_On_Shoot))
				{
					gun_state.stage = Gun.Stage.Cycling;

#if SERVER
					if (force_jammed || random.NextBool(failure_rate))
					{
						//App.WriteLine("jammed");

						gun_state.stage = Gun.Stage.Jammed;
						entity.SyncComponent(ref gun_state);

						Sound.Play(ref region, gun.sound_jam, pos_w_offset, volume: 1.10f, pitch: 1.00f, size: 1.50f);
						WorldNotification.Push(ref region, "* Jammed *", 0xffff0000, transform.position, lifetime: 1.00f);
					}
#endif
				}
				else
				{
					gun_state.stage = Gun.Stage.Ready;
				}

#if CLIENT
				if (!gun.flags.HasAll(Gun.Flags.No_Particles))
				{
					{
						var particle = Particle.New(texture_muzzle_flash, transform.LocalToWorld(gun.muzzle_offset + gun.particle_offset + new Vector2(1.50f * gun.flash_size, 0.00f).RotateByRad(gun.particle_rotation)), 0.25f);
						particle.fps = 24;
						particle.frame_count = 6;
						particle.frame_count_total = 6;
						particle.scale = gun.flash_size;
						particle.lit = 1.00f;
						particle.rotation = transform.rotation + gun.particle_rotation + (transform.scale.X < 0.00f ? MathF.PI : 0);
						particle.vel = base_vel;

						Particle.Spawn(ref region, particle);
					}

					var smoke_count = (int)gun.smoke_amount;
					for (var i = 0; i < smoke_count; i++)
					{
						var particle = Particle.New(texture_smoke, pos_w_offset_particle + (dir_particle * i * 0.50f), random.NextFloatRange(2.00f, 8.00f));
						particle.fps = random.NextByteRange(12, 16);
						particle.frame_count = 64;
						particle.frame_count_total = 64;
						particle.frame_offset = random.NextByteRange(0, 64);
						particle.scale = random.NextFloatRange(0.05f, 0.10f) * gun.smoke_size;
						particle.angular_velocity = random.NextFloatRange(-0.10f, 0.10f);
						particle.vel = base_vel + (dir_particle * random.NextFloatRange(1.00f, 1.50f));
						particle.force = new Vector2(0, -random.NextFloatRange(0.00f, 0.20f)) + (dir_particle * random.NextFloatRange(0.05f, 0.20f));
						particle.rotation = random.NextFloat(10.00f);
						particle.growth = random.NextFloatRange(0.15f, 0.30f);
						particle.drag = random.NextFloatRange(0.01f, 0.02f);
						particle.color_a = random.NextColor32Range(new Color32BGRA(255, 240, 240, 240), new Color32BGRA(255, 220, 220, 220)).WithAlphaMult(Maths.Clamp(0.40f + ((smoke_count - i) * 0.04f), 0.20f, 1.00f));
						particle.color_b = new Color32BGRA(000, 150, 150, 150);

						Particle.Spawn(ref region, particle);
					}
				}

				Sound.Play(gun.sound_shoot, pos_w_offset, volume: gun.sound_volume, pitch: gun.sound_pitch, size: gun.sound_size, priority: 0.70f, dist_multiplier: gun.sound_dist_multiplier);
#endif
			}

			switch (gun_state.stage)
			{
				case Gun.Stage.Cycling:
				{
					if (time < gun_state.next_cycle) break;

					if (gun_state.hints.HasAll(Gun.Hints.Cycled))
					{
						gun_state.stage = Gun.Stage.Ready;
					}
					else
					{
						var cycle_interval = gun.cycle_interval;
						if (!gun.flags.HasAll(Gun.Flags.Automatic)) cycle_interval = gunslinger.ApplyShootSpeed(cycle_interval);

						gun_state.next_cycle = info.WorldTime + cycle_interval;
						gun_state.hints.SetFlag(Gun.Hints.Cycled, true);

#if SERVER
						Sound.Play(ref region, gun.sound_cycle, transform.position, volume: 0.50f);
#endif
					}
				}
				break;
			}
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void OnReady(ISystem.Info info, Entity entity,
		[Source.Owned] ref Gun.Data gun, [Source.Owned] ref Gun.State gun_state,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] ref Body.Data body,
		[Source.Owned, Pair.Of<Gun.Data>] ref Inventory1.Data inventory_magazine)
		{
			gun_state.hints.SetFlag(Gun.Hints.Loaded, inventory_magazine.resource.quantity > float.Epsilon && inventory_magazine.resource.material.id != 0);

			if (gun_state.stage == Gun.Stage.Ready)
			{
				if (control.mouse.GetKeyDown(Mouse.Key.Left) && !gun_state.hints.HasAll(Gun.Hints.Cycled))
				{
#if SERVER
					gun_state.stage = Gun.Stage.Cycling;
					entity.SyncComponent(ref gun_state);
#endif
					return;
				}

				if (control.keyboard.GetKeyDown(Keyboard.Key.Reload) || (control.mouse.GetKeyDown(Mouse.Key.Left) && !gun_state.hints.HasAll(Gun.Hints.Loaded)))
				{
#if SERVER

					//gun_state.stage = Gun.Stage.Reloading;
					gun_state.hints.SetFlag(Gun.Hints.Wants_Reload, true);
					entity.SyncComponent(ref gun_state);
#endif
					return;
				}

				if (gun_state.hints.HasAll(Gun.Hints.Cycled) && (control.mouse.GetKeyDown(Mouse.Key.Left) || (control.mouse.GetKey(Mouse.Key.Left) && gun.flags.HasAll(Gun.Flags.Automatic))))
				{
					if (inventory_magazine.resource.quantity > float.Epsilon && inventory_magazine.resource.material.id != 0)
					{
#if SERVER
						gun_state.stage = Gun.Stage.Fired;
						gun_state.hints.SetFlag(Gun.Hints.Cycled, false);
						entity.SyncComponent(ref gun_state);
#endif
					}
					else
					{
						gun_state.stage = gun.flags.HasAll(Flags.Automatic) ? Gun.Stage.Ready : Gun.Stage.Cycling;
						gun_state.hints.SetFlag(Gun.Hints.Cycled, false);

#if SERVER
						ref var region = ref info.GetRegion();
						Sound.Play(ref region, gun.sound_empty, transform.position, volume: 0.50f);
						WorldNotification.Push(ref region, "* No ammo *", 0xffff0000, transform.position);
#endif
					}

					return;
				}
			}
			else if (gun_state.stage == Gun.Stage.Jammed)
			{
				if (control.mouse.GetKeyDown(Mouse.Key.Left) || control.keyboard.GetKeyDown(Keyboard.Key.Reload))
				{
					ref var region = ref info.GetRegion();
					var random = XorRandom.New();

					body.AddImpulse(transform.GetDirection().RotateByDeg(90.00f + random.NextFloatRange(-20.00f, 20.00f)) * MathF.Min(500, body.GetMass() * random.NextFloatRange(7.50f, 15.00f)));

#if SERVER
					if (random.NextBool(MathF.Max(0.10f, 1.00f - (0.40f + (gun.failure_rate * 5.00f)))))
					{
						//App.WriteLine("unjammed");

						//gun_state.stage = Gun.Stage.Reloading;

						gun_state.hints.SetFlag(Gun.Hints.Wants_Reload, true);
						gun_state.next_cycle = info.WorldTime + gun.reload_interval;

						entity.SyncComponent(ref gun_state);

						Sound.Play(ref region, gun.sound_jam, transform.position, volume: 0.10f, pitch: random.NextFloatRange(0.70f, 0.80f), size: 1.10f);
						WorldNotification.Push(ref region, "* Unjammed *", 0xff00ff00, transform.position);
					}
					else
					{
						gun_state.stage = Gun.Stage.Jammed;
						entity.SyncComponent(ref gun_state);

						Sound.Play(ref region, gun.sound_jam, transform.position, volume: 0.10f, pitch: random.NextFloatRange(0.70f, 0.80f), size: 1.10f);
						WorldNotification.Push(ref region, "* Jammed *", 0xffff0000, transform.position);
					}
#endif
				}
			}
		}
	}
}
