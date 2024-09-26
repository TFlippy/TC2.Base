
namespace TC2.Base.Components
{
	[Asset.Hjson(prefix: "essence.", capacity_world: 0, capacity_region: 0, capacity_local: 0, order: -30)]
	public interface IEssence: IAsset2<IEssence, IEssence.Data>
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,
		}

		[Serializable]
		public struct Data
		{
			[Save.Force] public string name;
			[Save.Force, Save.MultiLine] public string desc;

			[Save.NewLine]
			[Save.Force] public IEssence.Flags flags;
			[Save.Force] public Essence.Type type_tmp;

			[Save.NewLine]
			[Save.Force] public ColorBGRA color_emit = new ColorBGRA(1.00f, 1.00f, 1.00f, 1.00f);

			//[Save.NewLine]
			//[Save.Force, Obsolete] public float force_emit;
			//[Save.Force, Obsolete] public float heat_emit;

			[Save.Force] public float emit_force;
			[Save.Force] public Power emit_power_thermal;
			[Save.Force] public Power emit_power_kinetic;
			[Save.Force] public Power emit_power_radiant;
			[Save.Force] public Power emit_power_electric;
			[Save.Force] public Power emit_power_magnetic;

			[Save.NewLine]
			[Save.Force] public Sound.Handle sound_emit_loop;
			[Save.Force] public Sound.Handle sound_emit_pulser_loop;
			[Save.Force] public Sound.Handle sound_emit_stressor_loop;
			[Save.Force] public Sound.Handle sound_emit_impactor_loop;
			[Save.Force] public Sound.Handle sound_emit_cycler_loop;
			[Save.Force] public Sound.Handle sound_emit_oscillator_loop;
			[Save.Force] public Sound.Handle sound_emit_ambient_loop;
			[Save.Force] public Sound.Handle sound_drain_loop;
			[Save.Force] public Sound.Handle sound_collapse;
			[Save.Force] public Sound.Handle sound_zap;
			[Save.Force] public Sound.Handle sound_blast;
			[Save.Force] public Sound.Handle sound_impulse;
			[Save.Force] public Sound.Handle sound_impact;

			[Save.NewLine]
			[Save.Force] public Prefab.Handle h_prefab_node;
			[Save.Force] public IMaterial.Handle h_material_pellet;

			public Data()
			{

			}
		}
	}

	public static partial class Essence
	{
		public static Power GetThermalPower(this Essence.Container.Data container)
		{
			ref var essence_data = ref container.h_essence.GetData();
			if (essence_data.IsNotNull())
			{
				return container.GetEmittedEssenceAmount() * essence_data.emit_power_thermal;
			}
			else
			{
				return default;
			}
		}

		public static float GetForce(this Essence.Container.Data container)
		{
			ref var essence_data = ref container.h_essence.GetData();
			if (essence_data.IsNotNull())
			{
				return container.GetEmittedEssenceAmount() * essence_data.emit_force;
			}
			else
			{
				return default;
			}
		}

		public static Power GetMagneticPower(this Essence.Container.Data container)
		{
			ref var essence_data = ref container.h_essence.GetData();
			if (essence_data.IsNotNull())
			{
				return container.GetEmittedEssenceAmount() * essence_data.emit_power_magnetic;
			}
			else
			{
				return default;
			}
		}

		public static Power GetElectricPower(this Essence.Container.Data container)
		{
			ref var essence_data = ref container.h_essence.GetData();
			if (essence_data.IsNotNull())
			{
				return container.GetEmittedEssenceAmount() * essence_data.emit_power_electric;
			}
			else
			{
				return default;
			}
		}

		public static float GetEmittedEssenceAmount(this Essence.Container.Data container)
		{
			return container.available * container.rate_current;
		}

		public static partial class Container
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Show_GUI = 1u << 0,
				Allow_Edit_Rate = 1u << 1,
				Allow_Edit_Frequency = 1u << 2
			}

			[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
			public partial struct Data: IComponent
			{
				[Statistics.Info("Type", description: "Essence type.", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
				public IEssence.Handle h_essence;

				[Statistics.Info("Amount", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
				public float available;

				public float rate;
				public float rate_speed = 0.10f;

				[Statistics.Info("Stability", format: "{0:P2}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
				public float stability = 1.00f;

				public float health_threshold = 0.20f;
				public float glow_modifier = 1.00f;
				public float heat_modifier = 1.00f;
				public float frequency;

				public Essence.Container.Flags flags;
				public Essence.Emitter.Type emit_type;

				[Asset.Ignore] public float rate_current;
				[Asset.Ignore] public float noise_current;
				[Asset.Ignore, Save.Ignore, Net.Ignore] public float t_next_noise;
				[Asset.Ignore, Save.Ignore, Net.Ignore] public float t_next_damage;
				[Asset.Ignore, Save.Ignore, Net.Ignore] public float t_next_collapse;

				public Data()
				{

				}
			}

			[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdate(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
			[Source.Owned] ref Essence.Container.Data container, [Source.Owned, Optional] in Health.Data health)
			{
				var time = info.WorldTime;
				if (time >= container.t_next_noise)
				{
					var health_modifier = health.GetHealthNormalizedAvg();

					//App.WriteLine(health_modifier);
					container.t_next_noise = time + random.NextFloatExtra(0.05f, 0.25f * container.stability);
					container.noise_current = random.NextFloat((1.00f - (container.stability * health_modifier)).Pow2());
				}

				container.rate_current.LerpFMARef(Maths.Clamp01(container.rate + container.noise_current), container.rate_speed);
			}

			[ISystem.Update.D(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdateHeat(ISystem.Info info, Entity entity, ref Region.Data region,
			[Source.Owned] in Essence.Container.Data container,
			[Source.Owned] in Heat.Data heat, [Source.Owned] ref Heat.State heat_state)
			{
				heat_state.AddPower(container.GetThermalPower() * container.heat_modifier, info.DeltaTime);
			}

			[ISystem.Modified(ISystem.Mode.Single, ISystem.Scope.Region, order: -100)]
			public static void OnResourceModified(Entity entity,
			[Source.Owned] ref Resource.Data resource, [Source.Owned] ref Essence.Container.Data container)
			{
				container.h_essence = resource.GetEssenceType();
				container.available = resource.quantity * Essence.essence_per_pellet;
				App.WriteLine($"OnResourceModified essence container {container.h_essence}");
			}

			[ISystem.Modified(ISystem.Mode.Single, ISystem.Scope.Region, order: -100), HasComponent<Resource.Data>(Source.Modifier.Owned, false)]
			public static void OnInventoryModified(Entity entity,
			[Source.Owned, Pair.Component<Essence.Container.Data>] ref Inventory1.Data inventory, [Source.Owned] ref Essence.Container.Data container)
			{
				container.h_essence = inventory.resource.GetEssenceType();
				container.available = inventory.resource.quantity * Essence.essence_per_pellet;
				App.WriteLine($"OnInventoryModified essence container {container.h_essence}");
			}

			[ISystem.Modified(ISystem.Mode.Single, ISystem.Scope.Region, order: -10)]
			public static void OnModified(Entity entity,
			[Source.Owned] ref Essence.Container.Data container,
			[Source.Owned, Pair.Component<Essence.Container.Data>, Optional(true)] ref Sound.Emitter sound_emitter,
			[Source.Owned, Pair.Component<Essence.Container.Data>, Optional(true)] ref Light.Data light)
			{
				ref var essence_data = ref container.h_essence.GetData();
				App.WriteLine($"OnModified essence container {container.h_essence}");

				if (sound_emitter.IsNotNull())
				{
					var h_sound = default(Sound.Handle);
					if (essence_data.IsNotNull())
					{
						h_sound = container.emit_type switch
						{
							Essence.Emitter.Type.Undefined => essence_data.sound_emit_loop,
							Essence.Emitter.Type.Impactor => essence_data.sound_emit_impactor_loop,
							Essence.Emitter.Type.Cycler => essence_data.sound_emit_cycler_loop,
							Essence.Emitter.Type.Pulser => essence_data.sound_emit_pulser_loop,
							Essence.Emitter.Type.Stressor => essence_data.sound_emit_stressor_loop,
							Essence.Emitter.Type.Oscillator => essence_data.sound_emit_oscillator_loop,
							Essence.Emitter.Type.Ambient => essence_data.sound_emit_ambient_loop,
							_ => essence_data.sound_emit_loop
						};
						//entity.MarkModified<Essence.Container.Data, Sound.Emitter>();
					}

					var refresh = h_sound != sound_emitter.file;
					sound_emitter.file = h_sound;

#if CLIENT
					if (refresh) sound_emitter.Refresh();
#endif
				}

				if (light.IsNotNull())
				{
					if (essence_data.IsNotNull())
					{
						var color = essence_data.color_emit;
						//color.a *= Maths.Max(1.00f - container.stability, container.rate) * container.available * 0.01f * container.glow_modifier;
						//color.a *= container.available * 0.01f * container.glow_modifier;

						light.color = color;
					}
					else
					{
						light.color = default;
					}
				}

				//levitator.force_multiplier = Essence.force_per_motion_essence * levitator.EssenceAvailable;
			}

			[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void UpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random,
			[Source.Owned] in Transform.Data transform,
			[Source.Owned] ref Essence.Container.Data container,
			[Source.Owned, Pair.Component<Essence.Container.Data>, Optional(true)] ref Sound.Emitter sound_emitter,
			[Source.Owned, Pair.Component<Essence.Container.Data>, Optional(true)] ref Light.Data light)
			{
				var intensity = container.available * 0.01f * container.rate_current; //.glow_modifier;
				var noise = 1.00f + container.noise_current; // (1.00f - (float.Sin((random.NextFloat(container.stability.Inv()) * 2) + (info.WorldTime * intensity * 5.00f)) * 0.55f));

				//ref var essence_data = ref container.h_essence.GetData();
				//if (essence_data.IsNotNull())
				//var color_a = ColorBGRA.Lerp(essence_data.color_emit, new ColorBGRA(1, 1, 1, 1), 0.50f);
				//var color_b = essence_data.color_emit.WithColorMult(0.20f).WithAlphaMult(0.00f);

				if (light.IsNotNull())
				{
					//light.color = color_a.WithAlphaMult(intensity * random.NextFloatRange(0.90f, 1.20f));
					light.intensity.LerpFMARef(intensity * container.glow_modifier * noise, 0.05f);
					//light.scale = new Vector2(8, 8) * random.NextFloatRange(0.90f, 1.10f);
					light.rotation = random.NextFloat(0.02f);
					//light.offset = actuator.offset + random.NextVector2Range(new Vector2(-0.08f, +0.08f), new Vector2(-0.02f, +0.02f));
				}

				//for (var i = 0u; i < 1u; i++)
				//{
				//	Particle.Spawn(ref region, new Particle.Data()
				//	{
				//		texture = Light.tex_light_circle_00,
				//		lifetime = 0.20f,
				//		pos = pos,
				//		//vel = dir * 30.00f,
				//		drag = 0.10f,
				//		frame_count = 1,
				//		frame_count_total = 1,
				//		frame_offset = 0,
				//		scale = 0.70f,
				//		//stretch = new Vector2(1.00f, 0.30f),
				//		//face_dir_ratio = 1.00f,
				//		growth = 100.00f,
				//		color_a = color_a,
				//		color_b = color_b,
				//		glow = 4.00f * intensity
				//	});
				//}

				if (sound_emitter.IsNotNull())
				{
					sound_emitter.volume_mult.MoveTowardsDamped(Maths.Lerp01(0.00f, 1.40f, intensity * 0.70f), 0.15f, 0.02f);
					sound_emitter.pitch_mult.MoveTowardsDamped(Maths.Lerp01(0.10f, 1.20f, intensity * 0.35f) * noise, 0.05f, 0.01f);
				}
			}

#if SERVER
			[ISystem.Event<Health.PostDamageEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnPostDamage(ISystem.Info info, Entity entity, ref XorRandom random, ref Region.Data region, ref Health.PostDamageEvent data,
			[Source.Owned] in Health.Data health, [Source.Owned] ref Essence.Container.Data container, [Source.Owned] in Transform.Data transform)
			{
				var health_norm = health.GetHealthNormalized();
				if (health_norm <= container.health_threshold && info.WorldTime >= container.t_next_collapse)
				{
					var modifier = container.stability <= 0.00f ? 1.00f : MathF.Pow(Maths.Clamp01(Maths.Max(1.00f - (health_norm * container.stability), Maths.Lerp01(0.50f, container.rate_current * 0.30f, container.stability))), 1.50f);
					//App.WriteLine($"modifier: {modifier}");

					if (modifier >= 0.25f)
					{
						if (random.NextBool(modifier * 0.50f))
						{
							container.t_next_collapse = info.WorldTime + random.NextFloatRange(0.50f, 1.00f);
							var full_collapse = (1.00f - health_norm) >= container.stability;

							Essence.SpawnCollapsingNode(ref region, container.h_essence, container.available * Maths.Max(container.rate_current, modifier * modifier) * modifier, transform.position, full_collapse: full_collapse);
							if (full_collapse)
							{
								entity.Delete();
							}
						}
					}
				}
			}

			//[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region)]
			//public static void OnExplosiveRemove(ISystem.Info info, Entity entity, ref Region.Data region, [Source.Owned] in Transform.Data transform, [Source.Owned] in Explosive.Data explosive, [Source.Owned] ref Essence.Container.Data container)
			//{
			//	if (explosive.flags.HasAny(Explosive.Flags.Primed))
			//	{
			//		container.next_collapse = info.WorldTime + 1.00f;
			//		Essence.Explode(ref region, container.h_essence, container.available, transform.position);
			//	}
			//}

			//[ISystem.Event<Health.PostDamageEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
			//public static void OnPostDamage(ISystem.Info info, Entity entity, ref Health.PostDamageEvent data, [Source.Owned] ref Health.Data health, [Source.Owned] ref Essence.Container.Data essence_container)
			//{
			//	if (health.integrity <= essence_container.health_threshold)
			//	{
			//		if (!essence_container.flags.HasAny(Essence.Container.Flags.Primed))
			//		{
			//			essence_container.flags |= Essence.Container.Flags.Primed;
			//			entity.RemoveComponent<Essence.Container.Data>();
			//		}
			//	}
			//}

			//[ISystem.Modified<Resource.Data>(ISystem.Mode.Single, ISystem.Scope.Region)]
			//public static void OnResourceModified(ISystem.Info info, [Source.Owned] in Resource.Data resource, [Source.Owned] ref Essence.Container.Data essence_container)
			//{
			//	//essence_container.modifier = MathF.Sqrt(resource.quantity / resource.material.GetDefinition().quantity_max);
			//}

			//[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region)]
			//public static void OnRemove(ISystem.Info info, Entity entity, [Source.Owned] in Transform.Data transform, [Source.Owned] in Essence.Container.Data essence_container)
			//{
			//	if (essence_container.flags.HasAny(Essence.Container.Flags.Primed))
			//	{
			//		ref var region = ref info.GetRegion();
			//		var prefab = Essence.GetEssencePrefab(essence_container.type);
			//		if (prefab.id != 0)
			//		{
			//			var essence_container_tmp = essence_container;

			//			region.SpawnPrefab(prefab, transform.position).ContinueWith(x =>
			//			{
			//				ref var essence_node = ref x.GetComponent<EssenceNode.Data>();
			//				if (!essence_node.IsNull())
			//				{
			//					essence_node.type = essence_container_tmp.type;
			//					essence_node.amount = essence_container_tmp.amount;
			//					essence_node.stability = essence_container_tmp.stability;
			//					essence_node.volatility = essence_container_tmp.volatility;

			//					essence_node.Sync(x);
			//				}
			//			});
			//		}
			//	}
			//}
#endif

			[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region), HasComponent<Explosive.Data>(Source.Modifier.Owned, false)]
			public static void OnProjectileRemove(ISystem.Info info, Entity entity, ref Region.Data region,
			[Source.Owned] in Transform.Data transform, [Source.Owned] ref Projectile.Data projectile, [Source.Owned] ref Essence.Container.Data container)
			{
				container.t_next_collapse = info.WorldTime + 10.00f;
				EssenceNode.Collapse(ref region, ref projectile.random, container.h_essence, entity, transform.position, container.available);
				//Essence.Explode(ref region, container.h_essence, container.available, transform.position);
			}

			[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnExplosiveRemove(ISystem.Info info, Entity entity, ref Region.Data region,
			[Source.Owned] in Transform.Data transform, [Source.Owned] in Explosive.Data explosive, [Source.Owned] ref Essence.Container.Data container)
			{
				if (explosive.flags.HasAny(Explosive.Flags.Primed))
				{
					var random = XorRandom.New(entity);

					container.t_next_collapse = info.WorldTime + 10.00f;
					EssenceNode.Collapse(ref region, ref random, container.h_essence, entity, transform.position, container.available);
					//Essence.Explode(ref region, container.h_essence, container.available, transform.position);
				}
			}

			public struct ConfigureRPC: Net.IRPC<Essence.Container.Data>
			{
				public float? rate_target;
				public float? freq_target;

#if SERVER
				public void Invoke(Net.IRPC.Context rpc, ref Essence.Container.Data data)
				{
					var sync = false;
					sync |= this.rate_target.TryCopyToClamped01(ref data.rate);
					//sync |= this.power_target_ratio.TryCopyToClamped01(ref data.power_target_ratio);

					if (sync)
					{
						data.Sync(rpc.entity, true);
					}
				}
#endif
			}

#if CLIENT
			public partial struct ContainerGUI: IGUICommand
			{
				public Entity ent_interactable;
				public Entity ent_essence_container;

				public Inventory1.Data inventory;
				public Essence.Container.Data essence_container;

				public void Draw()
				{
					using (var window = GUI.Window.InteractionMisc("Essence"u8, this.ent_interactable, size: new Vector2(0, 48), min_width: 96, min_height: 48))
					{
						//App.WriteLine(this.ent_interactable);

						this.StoreCurrentWindowTypeID(100);
						//using (var embed_modules = GUI.Embed.New("motor.embed"u8, size: GUI.GetRemainingSpace()))
						{
							if (window.show)
							{
								//GUI.DrawRect(GUI.GetAvailableRect(), layer: GUI.Layer.Foreground);

								//GUI.DrawInventoryDock(Inventory.Type.Essence, new Vector2(GUI.RmY));
								GUI.DrawInventory(this.inventory.GetHandle());

								GUI.SameLine();

								using (var group = GUI.Group.New(size: GUI.Rm))
								{
									if (this.essence_container.flags.HasAny(Essence.Container.Flags.Allow_Edit_Rate))
									{
										if (GUI.SliderFloat("Rate"u8, ref this.essence_container.rate, 0.00f, 1.00f, snap: 0.001f, size: new Vector2(GUI.RmX, 24)))
										{
											var rpc = new Essence.Container.ConfigureRPC()
											{
												rate_target = this.essence_container.rate
											};
											rpc.Send(this.ent_essence_container);
										}

										//GUI.NewLine(0);

										var essence_color = Color32BGRA.Neutral;

										ref var essence_data = ref this.essence_container.h_essence.GetData();
										if (essence_data.IsNotNull())
										{
											essence_color = essence_data.color_emit;
										}

										//GUI.Text($"{essence_color}");

										GUI.DrawHorizontalGauge(this.essence_container.GetEmittedEssenceAmount(), 100 * Essence.essence_per_pellet * this.inventory.stack_size_multiplier, color: essence_color, size: GUI.Rm);
										//if (essence_data.IsNotNull())
										{
											GUI.DrawHoverTooltip(in this, draw: static (x) =>
											{
												var max_width = 192.00f;
												var h_essence = x.arg.essence_container.h_essence;
												ref var essence_data = ref h_essence.GetData();
												if (essence_data.IsNotNull())
												{
													//GUI.LabelShaded("Name:"u8, essence_data.name, width: max_width, font_a: GUI.Font.Superstar, font_b: GUI.Font.Superstar, size_a: 20.00f, size_b: 20.00f);
													//GUI.SeparatorThick(spacing: 6);

													var emitted_amount = x.arg.essence_container.GetEmittedEssenceAmount();

													GUI.LabelShaded("Name:"u8, essence_data.name, width: max_width);
													GUI.NewLine(4);

													GUI.LabelShaded("Amount:"u8, emitted_amount, format: "0.00' Ef'", width: max_width);
													GUI.LabelShaded("Thermal:"u8, essence_data.emit_power_thermal * emitted_amount, format: "0.00' kW'", width: max_width);
													GUI.LabelShaded("Electric:"u8, essence_data.emit_power_electric * emitted_amount, format: "0.00' kW'", width: max_width);
													GUI.LabelShaded("Force:"u8, essence_data.emit_force * emitted_amount * 0.001f, format: "0.00' kN'", width: max_width);
												}
											});
										}
										GUI.FocusableAsset(this.essence_container.h_essence);
									}
								}
							}
						}
					}
				}
			}

			[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnGUI(ISystem.Info info, ref Region.Data region,
			Entity ent_interactable, Entity ent_essence_container,
			[Source.Owned, Pair.Component<Essence.Container.Data>] ref Inventory1.Data inventory,
			[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Essence.Container.Data essence_container)
			{
				if (interactable.IsActive() && essence_container.flags.HasAny(Essence.Container.Flags.Show_GUI))
				{
					//App.WriteLine(ent_interactable);

					var gui = new ContainerGUI()
					{
						ent_interactable = ent_interactable,
						ent_essence_container = ent_essence_container,
						inventory = inventory,
						essence_container = essence_container,
					};
					gui.Submit();
				}
			}
#endif
		}

		public static class Emitter
		{
			public enum Type: byte
			{
				Undefined,

				[Name("Ambient", desc: "Passive emitter activated by surrounding environment.")]
				Ambient,
				[Name("Impactor", desc: "Simple discrete emitter activated by kinetic impulses.")]
				Impactor,
				[Name("Cycler", desc: "Reciprocating emitter activated by a pair of pellets interacting with eachother.")]
				Cycler,
				[Name("Pulser", desc: "Adjustable discrete emitter activated by short electrical pulses.")]
				Pulser,
				[Name("Stressor", desc: "Adjustable continuous emitter activated by compressive force.")]
				Stressor,
				[Name("Oscillator", desc: "Variable-frequency emitter activated by alternating electrical current.")]
				Oscillator,
				[Name("Fragmenter", desc: "Powerful single-use emitter activated by shattering the pellet.")]
				Fragmenter,
				[Name("Projector", desc: "Experimental emitter array capable of projecting effects of essences onto distant surfaces.")]
				Projector,

			}

			[Flags]
			public enum Flags: ushort
			{
				None = 0,

				Show_GUI = 1 << 0,
				Allow_Edit_Rate = 1 << 1,
				Allow_Edit_Frequency = 1 << 2
			}

			[IComponent.Data(Net.SendType.Reliable, region_only: true)]
			public partial struct Data: IComponent
			{
				public Essence.Emitter.Type type;
				public Essence.Emitter.Flags flags;

				public float frequency = 0.00f;

				[Editor.Slider.Clamped(-float.Tau, +float.Tau, float.Pi / 16.00f)]
				public float rotation;
				[Editor.Picker.Position(true)]
				public Vector2 offset;

				public float rate_speed = 0.10f;
				public float rate_max;
				public float rate_target;
				[Asset.Ignore] public float rate_current;

				public Data()
				{

				}
			}
		}

		//public interface IPowered: IComponent
		//{
		//	public float EssenceAvailable { get; set; }
		//	public float EssenceRate { get; set; }

		//	public static void UpdateInventory<T>(ref T powered, ref Inventory1.Data inventory) where T : unmanaged, Essence.IPowered
		//	{
		//		powered.EssenceAvailable = inventory.resource.quantity * essence_per_pellet;
		//	}
		//}

#if SERVER
		[ChatCommand.Region("essenceboom", "", admin: true)]
		public static void EssenceBoomCommand(ref ChatCommand.Context context, string identifier, float amount)
		{
			ref var region = ref context.GetRegion();
			if (region.IsNotNull())
			{
				var random = XorRandom.New(true);
				//EssenceNode.Collapse(ref region, ref random, identifier, default, context.GetPlayer().control.mouse.position, amount);
				Essence.SpawnCollapsingNode(ref region, identifier, amount, context.GetPlayerData().control.mouse.position, full_collapse: true);
			}
		}

		public static void SpawnCollapsingNode(ref Region.Data region, IEssence.Handle h_essence, float amount, Vector2 position, bool full_collapse = true)
		{
			ref var essence_data = ref h_essence.GetData();
			if (essence_data.IsNotNull())
			{
				var prefab = essence_data.h_prefab_node;
				if (prefab.id != 0)
				{
					if (amount >= 1.00f)
					{
						region.SpawnPrefab(prefab, position).ContinueWith(x =>
						{
							ref var essence_node = ref x.GetComponent<EssenceNode.Data>();
							if (essence_node.IsNotNull())
							{
								essence_node.h_essence = h_essence;
								essence_node.amount = amount;
								essence_node.stability = 0.00f;
								essence_node.volatility = 1.00f;
								essence_node.flags.AddFlag(EssenceNode.Flags.Full_Collapse, full_collapse);

								essence_node.Sync(x, true);
							}
						});
					}
				}
			}
		}
#endif

		[Query(ISystem.Scope.Region)]
		public delegate void GetAllNodesQuery(ISystem.Info info, Entity entity, [Source.Owned] ref EssenceNode.Data essence_node, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body);

		public enum Type: uint
		{
			Undefined,

			Heat,
			Motion,
			Life,
			Radiance,
			Cognition,
			Electricity,
			Failure
		}

		public static readonly Dictionary<IMaterial.Handle, IEssence.Handle> material_to_essence = new();
		public static readonly Dictionary<IEssence.Handle, IMaterial.Handle> essence_to_material = new();

		public const float essence_per_pellet = 10.00f;
		//public const float force_per_motion_essence = 2000.00f;

		//public static float GetEssenceForce(Essence.Type type) => type switch
		//{
		//	Essence.Type.Heat => 1400.00f,
		//	Essence.Type.Motion => 2000.00f,
		//	Essence.Type.Life => 400.00f,
		//	Essence.Type.Radiance => 800.00f,
		//	Essence.Type.Cognition => 200.00f,
		//	Essence.Type.Electricity => -800.00f,
		//	Essence.Type.Failure => -200.00f,

		//	_ => default
		//};

		//public static float GetEssenceHeat(Essence.Type type) => type switch
		//{
		//	Essence.Type.Heat => 20.00f,
		//	Essence.Type.Motion => 4.00f,
		//	Essence.Type.Life => 1.00f,
		//	Essence.Type.Radiance => 10.00f,
		//	Essence.Type.Cognition => -2.00f,
		//	Essence.Type.Electricity => 8.00f,
		//	Essence.Type.Failure => -5.00f,

		//	_ => default
		//};

		//public static Sound.Handle GetEssenceEmitSound(Essence.Type type) => type switch
		//{
		//	Essence.Type.Heat => "essence.emit.heat.loop.00",
		//	Essence.Type.Motion => "essence.emit.motion.loop.00",
		//	Essence.Type.Life => "essence.emit.life.loop.00",
		//	Essence.Type.Radiance => "essence.emit.cognition.loop.00",
		//	Essence.Type.Cognition => "essence.emit.cognition.loop.00",
		//	Essence.Type.Electricity => "essence.emit.cognition.loop.00",
		//	Essence.Type.Failure => "essence.emit.cognition.loop.00",

		//	_ => default
		//};

		//public static void Init()
		//{
		//	var materials = NetRegistry<Material.Definition>.GetAll();
		//	material_to_essence = new Essence.Type[materials.Length];

		//	for (var i = 0; i < material_to_essence.Length; i++)
		//	{
		//		ref var material = ref materials[i];
		//		if (material.flags.HasAny(Material.Flags.Essence))
		//		{
		//			var identifier = material.identifier.ToString();

		//			var char_index = identifier.LastIndexOf('.');
		//			if (char_index != -1)
		//			{
		//				var enum_name = identifier.Substring(char_index + 1);
		//				if (Enum.TryParse<Essence.Type>(enum_name, ignoreCase: true, out var essence_type))
		//				{
		//					material_to_essence[i] = essence_type;
		//				}
		//			}
		//		}
		//	}
		//}

		public static IEssence.Handle GetEssenceType(in this Resource.Data resource) => Essence.GetEssenceType(resource.material.id);
		public static IEssence.Handle GetEssenceType(this IMaterial.Handle material)
		{
			material_to_essence.TryGetValue(material, out var essence_type);
			return essence_type;

			//return material.id < material_to_essence.Length ? material_to_essence[material.id] : default;
		}

		//public static Prefab.Handle GetEssencePrefab(Essence.Type type) => type switch
		//{
		//	Essence.Type.Heat => "essence_node.heat",
		//	Essence.Type.Motion => "essence_node.motion",
		//	Essence.Type.Life => "essence_node.life",
		//	Essence.Type.Radiance => "essence_node.radiance",
		//	Essence.Type.Cognition => "essence_node.cognition",
		//	Essence.Type.Electricity => "essence_node.electricity",
		//	Essence.Type.Failure => "essence_node.failure",

		//	_ => default
		//};

		//public static IMaterial.Handle GetEssenceMaterial(Essence.Type type)
		//{
		//	essence_to_material.TryGetValue(type, out var material);
		//	return material;
		//}

		//// TODO: Just for debugging (until essences get implemented properly)
		//public static ColorBGRA GetColor(Essence.Type type) => type switch
		//{
		//	Essence.Type.Undefined => new Color32BGRA(255, 255, 255, 255),

		//	Essence.Type.Heat => new Color32BGRA(255, 255, 180, 0),
		//	Essence.Type.Motion => new Color32BGRA(255, 0, 255, 255),
		//	Essence.Type.Life => new Color32BGRA(255, 50, 255, 0),
		//	Essence.Type.Radiance => new Color32BGRA(255, 255, 255, 255),
		//	Essence.Type.Cognition => new Color32BGRA(255, 130, 0, 255),
		//	Essence.Type.Electricity => new Color32BGRA(255, 165, 205, 255),
		//	Essence.Type.Failure => new Color32BGRA(255, 165, 130, 90),

		//	_ => new Color32BGRA(255, 255, 255, 255)
		//};


		//[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateInventory<T>(ISystem.Info info, Entity entity,
		//[Source.Owned] ref T powered, [Source.Owned, Pair.Component<>] ref Inventory1.Data inventory) where T : unmanaged, IComponent, Essence.IPowered
		//{

		//}
	}
}
