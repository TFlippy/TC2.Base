
namespace TC2.Base.Components
{
	public static partial class Essence
	{
		public static partial class Container
		{
			//public enum Type: byte
			//{
			//	Undefined = 0,

			//	Pile,
			//	Single,
			//	Stack,
			//	Array,
			//	Capacitor
			//}

			public enum Type: byte
			{
				Undefined = 0,

				Pellet_Magazine,
				Conduit,
				Director,


				//[Name("Thumper", desc: "")]
				//Thumper,
			}

			// TODO: rework this
			public enum EmitType: byte
			{
				Undefined = 0,

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
				[Obsolete("Moved to Emitter.Type"), Name("Projector", desc: "Experimental emitter array capable of projecting effects of essences onto distant surfaces.")]
				Projector,
				[Name("Explosive", desc: "Single-use emitter activated by an explosive charge.")]
				Explosive,

				//[Name("Thumper", desc: "")]
				//Thumper,
			}

			[Flags]
			public enum Flags: ushort
			{
				None = 0,

				Show_GUI = 1 << 0,
				Allow_Edit_Rate = 1 << 1,
				Allow_Edit_Frequency = 1 << 2,
				Ignore_Resource_Essence = 1 << 3,

				Constant_Glow = 1 << 4,
				Constant_Heat = 1 << 5,
				No_Noise = 1 << 6,

				Faction = 1 << 7
			}

			[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Global | IComponent.Scope.Region)]
			public partial struct Data(): IComponent
			{
				[Statistics.Info("Amount", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
				[Net.Segment.A] public float available;
				[Net.Segment.A] public float rate;
				[Net.Segment.A, Asset.Ignore] public float noise_current;
				[Net.Segment.A, Asset.Ignore] public float rate_current;

				[Save.NewLine]
				[Statistics.Info("Type", description: "Essence type.", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
				[Net.Segment.B] public IEssence.Handle h_essence;
				[Net.Segment.B] public Essence.Container.Flags flags;

				[Save.NewLine]
				[Net.Segment.B] public required Essence.Container.Type type;
				[Net.Segment.B] public required Essence.Container.EmitType emit_type;
				[Net.Segment.B] public ushort unused_01;

				[Save.NewLine]
				[Statistics.Info("Stability", format: "{0:P2}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
				[Net.Segment.B] public float stability = 1.00f;
				[Net.Segment.B] public float rate_speed = 0.10f;
				//[Net.Segment.B] public required float capacity;

				[Save.NewLine]
				[Net.Segment.C] public float glow_modifier = 1.00f;
				[Net.Segment.C] public float heat_modifier = 1.00f;
				[Net.Segment.C] public float available_modifier = 1.00f;
				[Net.Segment.C] public float health_threshold = 0.20f;

				[Save.NewLine]
				[Asset.Ignore, Save.Ignore, Net.Ignore] public float t_next_noise;
				[Asset.Ignore, Save.Ignore, Net.Ignore] public float t_next_collapse;
				[Asset.Ignore, Save.Ignore, Net.Ignore] public float available_ratio;
				[Asset.Ignore, Save.Ignore, Net.Ignore] public float unused_02;
			}

			[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdate(ISystem.Info info, ref XorRandom random,
			[Source.Owned] ref Essence.Container.Data container, [Source.Owned, Optional] in Health.Data health)
			{
				if (!container.h_essence) return;

				if (container.flags.HasNone(Essence.Container.Flags.No_Noise))
				{
					var time = info.WorldTime;
					if (time >= container.t_next_noise)
					{
						var health_modifier = health.GetHealthNormalizedAvg();

						//App.WriteLine(health_modifier);
						container.t_next_noise = time + random.NextFloatExtra(0.05f, 0.25f * container.stability);
						container.noise_current = random.NextFloat((1.00f - (container.stability * health_modifier)).Pow2());
					}
				}

				container.rate_current.LerpFMARef(Maths.ClampMin(0.00f, container.rate + container.noise_current.Abs()), container.rate_speed);
			}

			[ISystem.Update.D(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdateHeat(ISystem.Info info,
			[Source.Owned] in Essence.Container.Data container,
			/*[Source.Owned] in Heat.Data heat, */[Source.Owned] ref Heat.State heat_state)
			{
				if (!container.h_essence) return;

				heat_state.AddPower(container.GetThermalPower() * container.heat_modifier, info.DeltaTime);
			}

			[ISystem.Modified.Component<Resource.Data>(ISystem.Mode.Single, ISystem.Scope.Region, order: -200)]
			public static void OnResourceModified(Entity entity,
			[Source.Owned] ref Resource.Data resource, [Source.Owned] ref Essence.Container.Data container)
			{
				container.available = (resource.quantity * container.available_modifier) * Essence.essence_per_pellet;
#if SERVER
				if (container.flags.HasNone(Essence.Container.Flags.Ignore_Resource_Essence))
				{
					var sync = container.h_essence.TrySet(resource.GetEssenceType());
					//App.WriteLine($"OnResourceModified essence container {container.h_essence}");

					if (sync)
					{
						container.Modified(entity, true);
					}
				}
#endif
			}

			[HasComponent<Resource.Data>(Source.Modifier.Owned, false)]
			[ISystem.Modified.Pair<Essence.Container.Data, Inventory1.Data>(ISystem.Mode.Single, ISystem.Scope.Region, order: -100)]
			public static void OnInventoryModified(Entity entity,
			[Source.Owned, Pair.Component<Essence.Container.Data>] ref Inventory1.Data inventory, [Source.Owned] ref Essence.Container.Data container)
			{
				container.available = (inventory.resource.quantity * container.available_modifier) * Essence.essence_per_pellet;
#if SERVER
				var sync = container.h_essence.TrySet(inventory.resource.GetEssenceType());
				//App.WriteLine($"OnInventoryModified essence container {container.h_essence}");

				if (sync)
				{
					container.Modified(entity, true);
				}
#endif
			}

			//public static int[] test = new int[]
			//{
			//	[0] = 7, 4, 4, 4
			//};

			[Shitcode]
			[ISystem.Modified(ISystem.Mode.Single, ISystem.Scope.Region, order: 50)]
			public static void OnModified_Sound([Source.Owned] ref Essence.Container.Data container,
			[Source.Owned, Pair.Component<Essence.Container.Data>] ref Sound.Emitter sound_emitter)
			{
				var h_sound = default(Sound.Handle);

				ref var essence_data = ref container.h_essence.GetData();
				if (essence_data.IsNotNull())
				{
					h_sound = container.emit_type switch
					{
						Essence.Container.EmitType.Undefined => essence_data.sound_emit_ambient_loop,
						Essence.Container.EmitType.Ambient => essence_data.sound_emit_ambient_loop,
						Essence.Container.EmitType.Impactor => essence_data.sound_emit_impactor_loop,
						Essence.Container.EmitType.Cycler => essence_data.sound_emit_cycler_loop,
						Essence.Container.EmitType.Pulser => essence_data.sound_emit_pulser_loop,
						Essence.Container.EmitType.Stressor => essence_data.sound_emit_stressor_loop,
						Essence.Container.EmitType.Oscillator => essence_data.sound_emit_oscillator_loop,
						Essence.Container.EmitType.Fragmenter => essence_data.sound_emit_ambient_loop,
						Essence.Container.EmitType.Projector => essence_data.sound_emit_projector_loop,
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

			[ISystem.Modified(ISystem.Mode.Single, ISystem.Scope.Region, order: 50)]
			public static void OnModified_Light([Source.Owned] ref Essence.Container.Data container,
			[Source.Owned, Pair.Component<Essence.Container.Data>] ref Light.Data light)
			{
				light.color = container.h_essence.GetEmitColor();
			}

			[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void UpdateEffects_Sound([Source.Owned] ref Essence.Container.Data container,
			[Source.Owned, Pair.Component<Essence.Container.Data>] ref Sound.Emitter sound_emitter)
			{
				if (container.available > 0.00f)
				{
					var intensity = container.available * container.rate_current;
					var noise = 1.00f + container.noise_current;

					sound_emitter.volume_mult.MoveTowardsDamped(Maths.Lerp01(0.00f, 1.40f, intensity * 0.0070f), 0.15f, 0.02f);
					sound_emitter.pitch_mult.MoveTowardsDamped(Maths.Lerp01(0.10f, 1.20f, intensity * 0.0035f) * noise, 0.05f, 0.01f);
				}
				else
				{
					sound_emitter.volume_mult = 0.00f;
				}
			}

			[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void UpdateEffects_Light(ref XorRandom random, [Source.Owned] ref Essence.Container.Data container,
			[Source.Owned, Pair.Component<Essence.Container.Data>] ref Light.Data light)
			{
				if (container.available > 0.00f)
				{
					var intensity = container.glow_modifier;
					//var intensity = container.available * container.rate_current * 0.01f;
					//var noise = 1.00f + container.noise_current;

					if (container.flags.HasNone(Essence.Container.Flags.Constant_Glow))
					{
						intensity *= container.available * container.rate_current * 0.01f;
					}
					else
					{

					}

					if (container.flags.HasNone(Essence.Container.Flags.No_Noise))
					{
						intensity *= 1.00f + container.noise_current;
					}

					//light.color = color_a.WithAlphaMult(intensity * random.NextFloatRange(0.90f, 1.20f));
					//light.intensity.LerpFMARef(intensity * container.glow_modifier * noise, 0.05f);
					light.intensity.LerpFMARef(intensity, 0.05f);
					//light.scale = new Vector2(8, 8) * random.NextFloatRange(0.90f, 1.10f);
					light.rotation = random.NextFloat(0.02f);
					//light.offset = actuator.offset + random.NextVector2Range(new Vector2(-0.08f, +0.08f), new Vector2(-0.02f, +0.02f));
				}
				else
				{
					light.intensity = 0.00f;
				}
			}

#if SERVER
			[ISystem.Event<Health.PostDamageEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnPostDamage(ISystem.Info info, Entity entity, ref XorRandom random, ref Region.Data region, ref Health.PostDamageEvent data,
			[Source.Owned] in Health.Data health, [Source.Owned] ref Essence.Container.Data container, [Source.Owned] in Transform.Data transform)
			{
				var health_norm = health.GetHealthNormalized();
				if (health_norm <= 0.00f || (health_norm <= container.health_threshold && (info.WorldTime >= container.t_next_collapse)))
				{
					var modifier = (container.stability * health_norm) <= 0.00f ? 1.00f : Maths.PowFast(Maths.Clamp01(Maths.Max(1.00f - (health_norm * container.stability), Maths.Lerp01(0.50f, container.rate_current * 0.30f, container.stability))), 1.50f);
					//App.WriteLine($"modifier: {modifier}");

					if (modifier >= 0.25f && (health_norm <= 0.00f || random.NextBool(modifier * 0.50f)))
					{
						container.t_next_collapse = info.WorldTime + random.NextFloatRange(0.50f, 1.00f);
						var full_collapse = modifier >= 1.00f; // (1.00f - health_norm) >= container.stability;

						Essence.SpawnCollapsingNode(region: ref region,
						h_essence: container.h_essence,
						amount: container.available * Maths.Max(container.rate_current, modifier.Pow2()) * modifier,
						position: transform.position,
						full_collapse: full_collapse);

						if (full_collapse)
						{
							entity.Delete();
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
				EssenceNode.Collapse(region: ref region,
					random: ref projectile.random,
					h_essence: container.h_essence,
					entity: entity,
					world_position: transform.position,
					amount: container.available);
				//Essence.Explode(ref region, container.h_essence, container.available, transform.position);
			}

			[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnExplosiveRemove(ISystem.Info info, Entity entity, ref Region.Data region,
			[Source.Owned] in Transform.Data transform, [Source.Owned] in Explosive.Data explosive, [Source.Owned] ref Essence.Container.Data container,
			[HasTag("no_gib", true, Source.Modifier.Owned)] bool no_gib)
			{
				if (no_gib) return;

				if (explosive.flags.HasAny(Explosive.Flags.Primed))
				{
					container.t_next_collapse = info.WorldTime + 10.00f;

					var random = XorRandom.New(entity.lower, container.h_essence);
					EssenceNode.Collapse(region: ref region,
						random: ref random,
						h_essence: container.h_essence,
						entity: entity,
						world_position: transform.position,
						amount: container.available);
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
					if (data.flags.HasAny(Essence.Container.Flags.Faction))
					{
						var h_faction_client = rpc.connection.GetFactionHandle();
						var h_faction = rpc.record.GetFactionHandle();

						Assert.Check(h_faction.IsAccessible(h_faction_client));
					}

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

				public IFaction.Handle h_faction;

				public void Draw()
				{
					using (var window = GUI.Window.InteractionMisc("Essence"u8, this.ent_interactable, size: new(0, 48), min_width: 96, min_height: 48))
					{
						//App.WriteLine(this.ent_interactable);

						this.StoreCurrentWindowTypeID(100);
						//using (var embed_modules = GUI.Embed.New("motor.embed"u8, size: GUI.GetRemainingSpace()))
						{
							if (window.show)
							{
								var h_faction_client = Client.GetFactionHandle();
								var is_accessible = this.h_faction.IsAccessible(h_faction_client);

								//GUI.DrawRect(GUI.GetAvailableRect(), layer: GUI.Layer.Foreground);

								//GUI.DrawInventoryDock(Inventory.Type.Essence, new Vector2(GUI.RmY));

								//using (var group_outer = GUI.Group.New(size: new(0, 0)))
								{
									GUI.DrawInventory(this.inventory.GetHandle());

									var rect_inventory = GUI.GetLastItemRect();
									//GUI.Help.Draw("help.essence"u8, GUI.Anchor.Left, "Insert EC-Pellet"u8, 
									//	desc: "This device is powered by essence pellets.\n"u8 +
									//	"You can control the emission rate using\n"u8 +
									//	"the slider next to the slot.\n\n"u8 +
									//	"Heaters typically use EC-TH pellets, which can be obtained from Limelights,\nwhile motors and propulsion-related devices use EC-MT pellets."u8);
									
									//GUI.DrawRect(GUI.GetLastItemRect(), layer: GUI.Layer.Foreground);


									GUI.SameLine();

									using (var group = GUI.Group.New(size: GUI.Rm))
									{
										if (this.essence_container.flags.HasAny(Essence.Container.Flags.Allow_Edit_Rate))
										{
											if (GUI.SliderFloat(label: "Rate"u8,
											value: ref this.essence_container.rate,
											min: 0.00f,
											max: 1.00f,
											snap: 0.001f,
											enabled: is_accessible,
											size: new Vector2(GUI.RmX, 24)))
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

											GUI.DrawHorizontalGauge(current: this.essence_container.GetEmittedEssenceAmount(),
												max: 250 * Essence.essence_per_pellet * this.inventory.stack_size_multiplier,
												color: essence_color,
												size: GUI.Rm);
											//if (essence_data.IsNotNull())
											{
												GUI.DrawHoverTooltip(arg: in this, draw: static (x) =>
												{
													var max_width = 192.00f;
													var h_essence = x.arg.essence_container.h_essence;
													ref var essence_data = ref h_essence.GetData();
													if (essence_data.IsNotNull())
													{
														//GUI.LabelShaded("Name:"u8, essence_data.name, width: max_width, font_a: GUI.Font.Superstar, font_b: GUI.Font.Superstar, size_a: 20.00f, size_b: 20.00f);
														//GUI.SeparatorThick(spacing: 6);

														var emitted_amount = x.arg.essence_container.GetEmittedEssenceAmount();

														//GUI.LabelShaded("Name:"u8, essence_data.name, width: max_width);
														GUI.Title($"{essence_data.name} Essence");
														GUI.NewLine(4);

														GUI.LabelShaded("Amount:"u8, emitted_amount, format: "0.00' Ef'", width: max_width);
														GUI.LabelShaded("Thermal:"u8, essence_data.emit_power_thermal * emitted_amount, format: "0.00' kW'", width: max_width);
														GUI.LabelShaded("Electric:"u8, essence_data.emit_power_electric * emitted_amount, format: "0.00' kW'", width: max_width);
														GUI.LabelShaded("Force:"u8, essence_data.emit_force * emitted_amount * 0.001f, format: "0.00' kN'", width: max_width);
													}
													else
													{
														GUI.Title("<No Essence>"u8);
													}
												});
											}
											GUI.FocusableAsset(this.essence_container.h_essence);
										}
									}
								}
								//GUI.DrawRect(GUI.GetLastItemRect(), layer: GUI.Layer.Foreground);
							}
						}
					}
				}
			}

			[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnGUI(/*ISystem.Info info, ref Region.Data region,*/
			Entity ent_interactable, Entity ent_essence_container,
			[Source.Owned, Pair.Component<Essence.Container.Data>] in Inventory1.Data inventory,
			[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Essence.Container.Data essence_container,
			[Source.Owned, Optional] in Faction.Data faction)
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
						h_faction = faction.id
					};
					gui.Submit();
				}
			}
#endif
		}









	}
}
