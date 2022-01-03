
namespace TC2.Base.Components
{
	public static partial class Gun
	{
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";
		public static readonly Texture.Handle texture_muzzle_flash = "MuzzleFlash";

		public static readonly Sound.Handle sound_gun_jam = "gun_jam";
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

			No_Particles = 1u << 4
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
			Cycled = 1 << 2
		}

		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public Vector2 muzzle_offset;

			public Sound.Handle sound_shoot;
			public Sound.Handle sound_cycle;
			public Sound.Handle sound_reload;
			public Sound.Handle sound_empty;

			[Statistics.Info("Muzzle Velocity", description: "Velocity of the fired projectile.", format: "{0:0.##} m/s", comparison: Statistics.Comparison.Higher)]
			public float velocity_multiplier;

			[Statistics.Info("Spread", description: "Spread of the fired projectiles.", format: "{0:0.##}x", comparison: Statistics.Comparison.Lower)]
			public float jitter_multiplier;

			[Statistics.Info("Damage", description: "Damage dealt by the fired projectile.", format: "{0:0.##}x", comparison: Statistics.Comparison.Higher)]
			public float damage_multiplier;

			[Statistics.Info("Recoil", description: "Force applied after firing the weapon.", format: "{0:0.##}x", comparison: Statistics.Comparison.Lower)]
			public float recoil_multiplier;

			[Statistics.Info("Reload Speed", description: "Time to reload the weapon.", format: "{0:0.##}s", comparison: Statistics.Comparison.Lower)]
			public float reload_interval;

			[Statistics.Info("Cycle Speed", description: "Rate of fire.", format: "{0:0.##}s", comparison: Statistics.Comparison.Lower)]
			public float cycle_interval;

			[Statistics.Info("Stability", description: "Reliability, may result in a catastrophic failure if too low.", format: "{0:P2}", comparison: Statistics.Comparison.Higher)]
			public float stability = 1.00f;

			[Statistics.Info("Failure Rate", description: "Chance of malfunction, such as jamming after being fired.", format: "{0:P2}", comparison: Statistics.Comparison.Lower)]
			public float failure_rate = 0.00f;

			[Statistics.Info("Maximum Ammunition", description: "Ammo capacity.", format: "{0:0}", comparison: Statistics.Comparison.Higher)]
			public float max_ammo;

			[Statistics.Info("Ammunition Usage", description: "Ammo used per shot.", format: "{0:0}", comparison: Statistics.Comparison.Lower)]
			public float ammo_per_shot = 1.00f;

			[Statistics.Info("Loudness", description: "Loudness of the shot.", format: "{0:0.##}x", comparison: Statistics.Comparison.Lower)]
			public float sound_volume = 1.25f;

			public float sound_size = 2.00f;

			public float sound_pitch = 1.00f;

			public float flash_size = 1.00f;

			public float smoke_size = 1.00f;
			public int smoke_amount = 1;

			[Statistics.Info("Projectile Count", description: "Number of projectiles fired per shot.", format: "{0}", comparison: Statistics.Comparison.Higher)]
			public int projectile_count = 1;

			public Gun.Flags flags;

			[Statistics.Info("Ammo", description: "Ammunition type.", comparison: Statistics.Comparison.None)]
			public Material.Flags ammo_filter;

			[Statistics.Info("Operation", description: "Operation mode of the weapon.", comparison: Statistics.Comparison.None)]
			public Gun.Action action;

			[Statistics.Info("Type", description: "Type of the weapon.", comparison: Statistics.Comparison.None)]
			public Gun.Type type;

			[Statistics.Info("Feed", description: "Method of loading ammunition.", comparison: Statistics.Comparison.None)]
			public Gun.Feed feed;
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct State: IComponent
		{
			[Save.Ignore] public Stage stage;
			[Save.Ignore] public Hints hints;
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

				using (var window = GUI.Window.HUD("Crosshair", GUI.WorldToCanvas(world_position_target), size: new(100, 100)))
				{
					if (window.show)
					{
						if (this.state.stage == Stage.Reloading)
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
		[Source.Owned] in Gun.Data gun, [Source.Owned] in Gun.State state, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Trait.Of<Gun.Data>] ref Inventory1.Data inventory,
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

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateLight1([Source.Owned] in Gun.State gun_state, [Trait.Of<Gun.Data>] ref Light.Data light)
		{
			if (gun_state.stage == Stage.Fired)
			{
				light.intensity = 4.00f;
			}
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateLight2([Trait.Of<Gun.Data>] ref Light.Data light)
		{
			light.intensity = Maths.Lerp(light.intensity, 0.00f, 0.50f);
		}

		[ISystem.Update(ISystem.Mode.Single)]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void UpdateReload<T>(ISystem.Info info, Entity entity,
		[Source.Owned] ref Gun.Data gun, [Source.Owned] ref Gun.State gun_state,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control,
		[Source.Owned, Trait.Of<Gun.Data>] ref Inventory1.Data inventory_magazine, [Source.Parent, Trait.Of<Storage.Data>] ref T inventory,
		[Source.Parent, Optional] in Specialization.Gunslinger.Data gunslinger) where T : unmanaged, IInventory
		{
			if (gun_state.stage == Stage.Reloading)
			{
				var time = info.WorldTime;
				if (time < gun_state.next_reload) return;
				ref var region = ref info.GetRegion();

				if (inventory_magazine.resource.quantity >= gun.max_ammo)
				{
					gun_state.stage = Stage.Cycling;
				}
				else if (!gun.flags.HasFlag(Flags.Full_Reload) && control.mouse.GetKey(Mouse.Key.Left))
				{
#if SERVER
					gun_state.stage = Stage.Cycling;
					entity.SyncComponent(ref gun_state);
#endif
				}
				else
				{
					gun_state.next_reload = info.WorldTime + gunslinger.ApplyReloadSpeed(gun.reload_interval);
					if (inventory_magazine.resource.material.id != 0 && !inventory_magazine.resource.material.GetDefinition().flags.HasFlag(gun.ammo_filter)) inventory_magazine.resource.material = default;

					if (inventory_magazine.resource.material.id == 0 || inventory_magazine.resource.quantity <= float.Epsilon)
					{
						var count = inventory.Length;
						for (var i = 0; i < count; i++)
						{
							ref var resource = ref inventory[i];
							if (resource.material.GetDefinition().flags.HasFlag(gun.ammo_filter))
							{
								inventory_magazine.resource.material = resource.material;
								break;
							}
						}
					}

					if (inventory_magazine.resource.material != 0)
					{
#if SERVER
						gun_state.hints &= ~Hints.Cycled;

						var amount = Maths.Clamp(MathF.Min(gun.max_ammo - inventory_magazine.resource.quantity, gun.flags.HasFlag(Flags.Full_Reload) ? gun.max_ammo : gunslinger.ApplyBulkReload(1.00f)), 0.00f, gun.max_ammo);
						//App.WriteLine(amount);

						var done = true;

						if (Resource.Withdraw(ref inventory, ref inventory_magazine.resource, ref amount))
						{
							done = false;
							inventory_magazine.flags |= Inventory.Flags.Dirty;

							Sound.Play(ref region, gun.sound_reload, transform.position);
						}

						if (done)
						{
							gun_state.stage = Stage.Cycling;
							entity.SyncComponent(ref gun_state);
						}
#endif
					}
					else
					{
						gun_state.stage = Stage.Ready;
					}
				}
			}
		}

		[ISystem.Update(ISystem.Mode.Single)]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void OnUpdate(ISystem.Info info, Entity entity,
		[Source.Owned] ref Gun.Data gun, [Source.Owned] ref Gun.State gun_state, [Source.Owned] ref Body.Data body,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control,
		[Source.Owned, Trait.Of<Gun.Data>] ref Inventory1.Data inventory_magazine,
		[Source.Parent, Optional] in Specialization.Gunslinger.Data gunslinger, [Source.Parent, Optional] in Faction.Data faction, [Source.Owned, Optional] ref Overheat.Data overheat)
		{
			var time = info.WorldTime;
			ref var region = ref info.GetRegion();
			if (gun_state.stage == Stage.Fired)
			{
				var pos_w_offset = transform.LocalToWorld(gun.muzzle_offset);
				var dir = transform.GetDirection();
				var random = XorRandom.New();

				body.AddImpulseWorld(-dir * 70.00f * gun.recoil_multiplier, pos_w_offset);

				var failure_rate = gun.failure_rate;
				var stability = gun.stability;

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

					if (overheat.heat_critical > 0.00f && material.projectile_heat > 0.00f)
					{
						var heat = ((gun.ammo_per_shot - amount) * material.projectile_heat) / MathF.Max(body.GetMass() * 0.10f, 1.00f);
						overheat.heat_current += heat;

						var heat_excess = MathF.Max(overheat.heat_current - overheat.heat_critical, 0.00f);
						if (heat_excess > 0.00f)
						{
							failure_rate = Maths.Clamp(failure_rate + (heat_excess * 0.01f), 0.00f, 1.00f);
							stability = Maths.Clamp(stability - (heat_excess * 0.005f), 0.00f, 1.00f);
						}

						overheat.Sync(entity);
					}

					{
						for (var i = 0; i < count; i++)
						{
							var projectile_init =
							(
								damage_mult: gun.damage_multiplier,
								vel: dir.RotateByDeg(random.NextFloat(gun.jitter_multiplier * 0.50f * material.projectile_spread_mult)) * gun.velocity_multiplier * material.projectile_speed_mult * random.NextFloatRange(0.90f, 1.10f),
								owner: body.GetParent(),
								faction_id: faction.id
							);

							region.SpawnPrefab(material.projectile_prefab, pos_w_offset).ContinueWith(ent =>
							{
								ref var projectile = ref ent.GetComponent<Projectile.Data>();
								if (!projectile.IsNull())
								{
									projectile.damage_base *= projectile_init.damage_mult;
									projectile.damage_bonus *= projectile_init.damage_mult;
									projectile.velocity = projectile_init.vel;
									projectile.ent_owner = projectile_init.owner;
									projectile.faction_id = projectile_init.faction_id;

									ent.SyncComponent(ref projectile);
								}

								ref var explosive = ref ent.GetComponent<Explosive.Data>();
								if (!explosive.IsNull())
								{
									explosive.owner_entity = projectile_init.owner;
									ent.SyncComponent(ref explosive);
								}
							});
						}
					}

					if (stability < 1.00f && random.NextBool(failure_rate) && random.NextBool(1.00f - stability))
					{
						var explosion_data = new Explosion.Data()
						{
							power = 1.50f + (count * 0.80f),
							radius = 5.50f + (count * 2.50f),
							damage_entity = (gun.damage_multiplier * (1.00f + ((count - 1) * 3.80f))) * 200.00f,
							damage_terrain = (gun.damage_multiplier * (1.00f + (count * 0.80f))) * 130.00f,
							smoke_amount = 0.30f,
							sparks_amount = 2.00f,
							owner_entity = body.GetParent()
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
								explosion.owner_entity = explosion_data.owner_entity;
								explosion.smoke_amount = explosion_data.smoke_amount;

								explosion.Sync(x);
							}
						});

						Sound.Play(ref region, sound_gun_break, pos_w_offset, volume: 1.50f, pitch: 1.10f, size: 1.50f);

						entity.Delete();
					}
				}
#endif

				if (gun.flags.HasFlag(Flags.Cycle_On_Shoot))
				{
					gun_state.stage = Stage.Cycling;

#if SERVER
					if (random.NextBool(failure_rate))
					{
						//App.WriteLine("jammed");

						gun_state.stage = Stage.Jammed;
						entity.SyncComponent(ref gun_state);

						Sound.Play(ref region, sound_gun_jam, pos_w_offset, volume: 1.10f, pitch: 1.00f, size: 1.50f);
					}
#endif
				}
				else
				{
					gun_state.stage = Stage.Ready;
				}

#if CLIENT
				if (!gun.flags.HasFlag(Gun.Flags.No_Particles))
				{
					{
						var particle = Particle.New(texture_muzzle_flash, transform.LocalToWorld(gun.muzzle_offset + new Vector2(1.50f * gun.flash_size, 0.00f)), 0.25f);
						particle.fps = 24;
						particle.frame_count = 6;
						particle.frame_count_total = 6;
						particle.scale = gun.flash_size;
						particle.rotation = transform.rotation + (transform.scale.X < 0.00f ? MathF.PI : 0);

						Particle.Spawn(ref region, particle);
					}

					for (var i = 0; i < gun.smoke_amount; i++)
					{
						var particle = Particle.New(texture_smoke, pos_w_offset + (dir * i * 0.50f), random.NextFloatRange(3.00f, 12.00f));
						particle.fps = (byte)random.NextFloatRange(8, 10);
						particle.frame_count = 64;
						particle.frame_count_total = 64;
						particle.frame_offset = (byte)random.NextFloatRange(0, 64);
						particle.scale = random.NextFloatRange(0.05f, 0.10f) * gun.smoke_size;
						particle.angular_velocity = random.NextFloatRange(-0.10f, 0.10f);
						particle.vel = dir * random.NextFloatRange(1.00f, 1.50f);
						particle.force = new Vector2(0, -random.NextFloatRange(0.00f, 0.20f)) + (dir * random.NextFloatRange(0.05f, 0.20f));
						particle.rotation = random.NextFloat(10.00f);
						particle.growth = random.NextFloatRange(0.15f, 0.30f);
						particle.drag = random.NextFloatRange(0.00f, 0.01f);
						particle.color_a = new Color32BGRA(100, 220, 220, 220);
						particle.color_b = new Color32BGRA(000, 150, 150, 150);

						Particle.Spawn(ref region, particle);
					}
				}

				Sound.Play(gun.sound_shoot, pos_w_offset, volume: gun.sound_volume, pitch: gun.sound_pitch, size: gun.sound_size);
#endif
			}

			switch (gun_state.stage)
			{
				case Stage.Cycling:
				{
					if (time < gun_state.next_cycle) break;

					if (gun_state.hints.HasFlag(Hints.Cycled))
					{
						gun_state.stage = Stage.Ready;
					}
					else
					{
						var cycle_interval = gun.cycle_interval;
						if (!gun.flags.HasFlag(Flags.Automatic)) cycle_interval = gunslinger.ApplyShootSpeed(cycle_interval);

						gun_state.next_cycle = info.WorldTime + cycle_interval;
						gun_state.hints |= Hints.Cycled;

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
		[Source.Owned, Trait.Of<Gun.Data>] ref Inventory1.Data inventory_magazine)
		{
			if (gun_state.stage == Stage.Ready)
			{
				if (control.mouse.GetKeyDown(Mouse.Key.Left) && !gun_state.hints.HasFlag(Hints.Cycled))
				{
#if SERVER
					gun_state.stage = Stage.Cycling;
					entity.SyncComponent(ref gun_state);
#endif
					return;
				}

				if (control.keyboard.GetKeyDown(Keyboard.Key.Reload))
				{
#if SERVER
					gun_state.stage = Stage.Reloading;
					entity.SyncComponent(ref gun_state);
#endif
					return;
				}

				if (gun_state.hints.HasFlag(Hints.Cycled) && (control.mouse.GetKeyDown(Mouse.Key.Left) || (control.mouse.GetKey(Mouse.Key.Left) && gun.flags.HasFlag(Flags.Automatic))))
				{
					if (inventory_magazine.resource.quantity > float.Epsilon && inventory_magazine.resource.material.id != 0)
					{
#if SERVER
						gun_state.stage = Stage.Fired;
						gun_state.hints &= ~Hints.Cycled;
						entity.SyncComponent(ref gun_state);
#endif
					}
					else
					{
						gun_state.stage = gun.flags.HasFlag(Flags.Automatic) ? Stage.Ready : Stage.Cycling;
						gun_state.hints &= ~Hints.Cycled;
#if SERVER
						Sound.Play(ref info.GetRegion(), gun.sound_empty, transform.position, volume: 0.50f);
#endif
					}

					return;
				}
			}
			else if (gun_state.stage == Stage.Jammed)
			{
				if (control.mouse.GetKeyDown(Mouse.Key.Left) || control.keyboard.GetKeyDown(Keyboard.Key.Reload))
				{
					var random = XorRandom.New();

					body.AddImpulse(transform.GetDirection().RotateByDeg(90.00f + random.NextFloatRange(-20.00f, 20.00f)) * MathF.Min(500, body.GetMass() * random.NextFloatRange(7.50f, 15.00f)));

#if SERVER
					if (random.NextBool(MathF.Max(0.10f, 1.00f - (0.40f + (gun.failure_rate * 5.00f)))))
					{
						//App.WriteLine("unjammed");

						gun_state.stage = Stage.Reloading;
						gun_state.next_cycle = info.WorldTime + gun.reload_interval;

						entity.SyncComponent(ref gun_state);

						Sound.Play(ref info.GetRegion(), sound_gun_jam, transform.position, volume: 0.10f, pitch: random.NextFloatRange(0.70f, 0.80f), size: 1.10f);
					}
					else
					{
						//App.WriteLine("jammed");

						gun_state.stage = Stage.Jammed;
						entity.SyncComponent(ref gun_state);

						Sound.Play(ref info.GetRegion(), sound_gun_jam, transform.position, volume: 0.10f, pitch: random.NextFloatRange(0.70f, 0.80f), size: 1.10f);
					}
#endif
				}
			}
		}
	}
}
