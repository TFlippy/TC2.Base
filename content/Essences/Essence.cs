
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

		[FixedAddressValueType] public static FixedArray256<IEssence.Handle> material_to_essence;
		[FixedAddressValueType] public static FixedArray256<IMaterial.Handle> essence_to_material;

		[FixedAddressValueType] public static FixedArray256<ColorBGRA> essence_to_color_emit;

		[FixedAddressValueType] public static FixedArray256<float> essence_to_emit_force;
		[FixedAddressValueType] public static FixedArray256<Power> essence_to_emit_power_thermal;
		[FixedAddressValueType] public static FixedArray256<Power> essence_to_emit_power_kinetic;
		[FixedAddressValueType] public static FixedArray256<Power> essence_to_emit_power_radiant;
		[FixedAddressValueType] public static FixedArray256<Power> essence_to_emit_power_electric;
		[FixedAddressValueType] public static FixedArray256<Power> essence_to_emit_power_magnetic;

		static void IAsset2<IEssence, IEssence.Data>.OnRefresh(IEssence.Definition definition)
		{
			ref var data = ref definition.GetData();

			var identifier = definition.identifier;
			var h_essence = definition.GetHandle();
			var index = (nuint)h_essence.id;

			var identifier_material = $"pellet.{identifier}";
			if (Assert.Check(IMaterial.Handle.TryParse(identifier_material, out var h_material), Assert.Level.Warn))
			{
				essence_to_material[index] = h_material;
				material_to_essence[h_material.id] = h_essence;
			}

			IEssence.essence_to_color_emit[index] = data.color_emit;

			IEssence.essence_to_emit_force[index] = data.emit_force;
			IEssence.essence_to_emit_power_thermal[index] = data.emit_power_thermal;
			IEssence.essence_to_emit_power_kinetic[index] = data.emit_power_kinetic;
			IEssence.essence_to_emit_power_radiant[index] = data.emit_power_radiant;
			IEssence.essence_to_emit_power_electric[index] = data.emit_power_electric;
			IEssence.essence_to_emit_power_magnetic[index] = data.emit_power_magnetic;
		}

		public struct Data(): IName, IDescription
		{
			[Save.Force] public string name;
			[Save.Force, Save.MultiLine] public string desc;

			[Save.NewLine]
			[Save.Force] public IEssence.Flags flags;
			[Obsolete] public Essence.Type type_tmp;

			[Save.NewLine]
			[Save.Force] public ColorBGRA color_emit;

			//[Save.NewLine]
			//[Save.Force, Obsolete] public float force_emit;
			//[Save.Force, Obsolete] public float heat_emit;

			[Save.NewLine]
			[Save.Force] public float emit_force;
			[Save.Force] public Power emit_power_thermal;
			[Save.Force] public Power emit_power_kinetic;
			[Save.Force] public Power emit_power_radiant;
			[Save.Force] public Power emit_power_electric;
			[Save.Force] public Power emit_power_magnetic;

			[Save.NewLine]
			[Obsolete] public Sound.Handle sound_emit_loop;
			[Obsolete] public Sound.Handle sound_drain_loop;

			[Save.NewLine]
			[Save.Force] public Sound.Handle sound_emit_pulser_loop;
			[Save.Force] public Sound.Handle sound_emit_stressor_loop;
			[Save.Force] public Sound.Handle sound_emit_impactor_loop;
			[Save.Force] public Sound.Handle sound_emit_cycler_loop;
			[Save.Force] public Sound.Handle sound_emit_oscillator_loop;
			[Save.Force] public Sound.Handle sound_emit_ambient_loop;
			[Save.Force] public Sound.Handle sound_emit_projector_loop;

			[Save.NewLine]
			[Save.Force] public Sound.Handle sound_collapse;
			[Save.Force] public Sound.Handle sound_zap;
			[Save.Force] public Sound.Handle sound_blast;
			[Save.Force] public Sound.Handle sound_impulse;
			[Save.Force] public Sound.Handle sound_impact;
			[Save.Force] public Sound.Handle sound_shatter;

			[Save.NewLine]
			[Save.Force] public Prefab.Handle h_prefab_node;
			[Save.Force] public IMaterial.Handle h_material_pellet;

			[Save.NewLine]
			[Save.Force] public IEvent.Info on_collapse;
			[Save.Force] public IEvent.Info on_failure;

			readonly ReadOnlySpan<char> IName.GetName() => this.name;
			readonly ReadOnlySpan<char> IName.GetShortName() => this.name;
			readonly ReadOnlySpan<char> IDescription.GetDescription() => this.desc;
		}
	}

	public static partial class Essence
	{
		public static float GetForce(this IEssence.Handle h_essence) => IEssence.essence_to_emit_force[h_essence.id];
		public static Power GetThermalPower(this IEssence.Handle h_essence) => IEssence.essence_to_emit_power_thermal[h_essence.id];
		public static Power GetKineticPower(this IEssence.Handle h_essence) => IEssence.essence_to_emit_power_kinetic[h_essence.id];
		public static Power GetRadiantPower(this IEssence.Handle h_essence) => IEssence.essence_to_emit_power_radiant[h_essence.id];
		public static Power GetElectricPower(this IEssence.Handle h_essence) => IEssence.essence_to_emit_power_electric[h_essence.id];
		public static Power GetMagneticPower(this IEssence.Handle h_essence) => IEssence.essence_to_emit_power_magnetic[h_essence.id];
		public static ColorBGRA GetEmitColor(this IEssence.Handle h_essence) => IEssence.essence_to_color_emit[h_essence.id];


		public static float GetForce(this Essence.Container.Data container) => container.GetEmittedEssenceAmount() * container.h_essence.GetForce();
		public static Power GetThermalPower(this Essence.Container.Data container) => container.GetEmittedEssenceAmount() * container.h_essence.GetThermalPower();
		public static Power GetKineticPower(this Essence.Container.Data container) => container.GetEmittedEssenceAmount() * container.h_essence.GetKineticPower();
		public static Power GetRadiantPower(this Essence.Container.Data container) => container.GetEmittedEssenceAmount() * container.h_essence.GetRadiantPower();
		public static Power GetElectricPower(this Essence.Container.Data container) => container.GetEmittedEssenceAmount() * container.h_essence.GetElectricPower();
		public static Power GetMagneticPower(this Essence.Container.Data container) => container.GetEmittedEssenceAmount() * container.h_essence.GetMagneticPower();

		public static float GetEmittedEssenceAmount(this Essence.Container.Data container)
		{
			return container.available * container.rate_current;
		}

		public static partial class Container
		{
			[Flags]
			public enum Flags: byte
			{
				None = 0,

				Show_GUI = 1 << 0,
				Allow_Edit_Rate = 1 << 1,
				Allow_Edit_Frequency = 1 << 2,
				Ignore_Resource_Essence = 1 << 3,

				Constant_Glow = 1 << 4,
				Constant_Heat = 1 << 5,
				No_Noise = 1 << 6,
			}

			[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Global | IComponent.Scope.Region)]
			public partial struct Data(): IComponent
			{
				[Statistics.Info("Amount", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
				[Net.Segment.A] public float available;
				[Net.Segment.A] public float rate;
				[Net.Segment.A, Asset.Ignore] public float rate_current;
				[Net.Segment.A, Asset.Ignore] public float noise_current;

				[Statistics.Info("Stability", format: "{0:P2}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
				[Net.Segment.B] public float stability = 1.00f;
				[Net.Segment.B] public float available_modifier = 1.00f;
				[Net.Segment.B] public float frequency;

				[Statistics.Info("Type", description: "Essence type.", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
				[Net.Segment.B] public IEssence.Handle h_essence;
				[Net.Segment.B] public Essence.Emitter.Type emit_type;
				[Net.Segment.B] public Essence.Container.Flags flags;

				[Net.Segment.C] public float glow_modifier = 1.00f;
				[Net.Segment.C] public float heat_modifier = 1.00f;
				[Net.Segment.C] public float rate_speed = 0.10f;
				[Net.Segment.C] public float health_threshold = 0.20f;

				[Asset.Ignore, Save.Ignore, Net.Ignore] public float t_next_noise;
				[Asset.Ignore, Save.Ignore, Net.Ignore] public float t_next_damage;
				[Asset.Ignore, Save.Ignore, Net.Ignore] public float t_next_collapse;
				[Asset.Ignore, Save.Ignore, Net.Ignore] private float unused;
			}

			[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdate(ISystem.Info info, ref XorRandom random,
			[Source.Owned] ref Essence.Container.Data container, [Source.Owned, Optional] in Health.Data health)
			{
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

				container.rate_current.LerpFMARef(Maths.ClampMin(0.00f, container.rate + container.noise_current), container.rate_speed);
			}

			[ISystem.Update.D(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdateHeat(ISystem.Info info,
			[Source.Owned] in Essence.Container.Data container,
			[Source.Owned] in Heat.Data heat, [Source.Owned] ref Heat.State heat_state)
			{
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

			[ISystem.Modified.Pair<Essence.Container.Data, Inventory1.Data>(ISystem.Mode.Single, ISystem.Scope.Region, order: -100), HasComponent<Resource.Data>(Source.Modifier.Owned, false)]
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
						Essence.Emitter.Type.Undefined => essence_data.sound_emit_ambient_loop,
						Essence.Emitter.Type.Ambient => essence_data.sound_emit_ambient_loop,
						Essence.Emitter.Type.Impactor => essence_data.sound_emit_impactor_loop,
						Essence.Emitter.Type.Cycler => essence_data.sound_emit_cycler_loop,
						Essence.Emitter.Type.Pulser => essence_data.sound_emit_pulser_loop,
						Essence.Emitter.Type.Stressor => essence_data.sound_emit_stressor_loop,
						Essence.Emitter.Type.Oscillator => essence_data.sound_emit_oscillator_loop,
						Essence.Emitter.Type.Fragmenter => essence_data.sound_emit_ambient_loop,
						Essence.Emitter.Type.Projector => essence_data.sound_emit_projector_loop,
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
					var modifier = (container.stability * health_norm) <= 0.00f ? 1.00f : MathF.Pow(Maths.Clamp01(Maths.Max(1.00f - (health_norm * container.stability), Maths.Lerp01(0.50f, container.rate_current * 0.30f, container.stability))), 1.50f);
					//App.WriteLine($"modifier: {modifier}");

					if (modifier >= 0.25f && (health_norm <= 0.00f || random.NextBool(modifier * 0.50f)))
					{
						container.t_next_collapse = info.WorldTime + random.NextFloatRange(0.50f, 1.00f);
						var full_collapse = modifier >= 1.00f; // (1.00f - health_norm) >= container.stability;

						Essence.SpawnCollapsingNode(ref region, container.h_essence, container.available * Maths.Max(container.rate_current, modifier.Pow2()) * modifier, transform.position, full_collapse: full_collapse);
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
				EssenceNode.Collapse(ref region, ref projectile.random, container.h_essence, entity, transform.position, container.available);
				//Essence.Explode(ref region, container.h_essence, container.available, transform.position);
			}

			[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnExplosiveRemove(ISystem.Info info, Entity entity, ref Region.Data region,
			[Source.Owned] in Transform.Data transform, [Source.Owned] in Explosive.Data explosive, [Source.Owned] ref Essence.Container.Data container)
			{
				if (explosive.flags.HasAny(Explosive.Flags.Primed))
				{
					container.t_next_collapse = info.WorldTime + 10.00f;

					var random = XorRandom.New(entity.lower, container.h_essence);
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
										if (GUI.SliderFloat(label: "Rate"u8, value: ref this.essence_container.rate, min: 0.00f, max: 1.00f, snap: 0.001f, size: new Vector2(GUI.RmX, 24)))
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

										GUI.DrawHorizontalGauge(current: this.essence_container.GetEmittedEssenceAmount(), max: 100 * Essence.essence_per_pellet * this.inventory.stack_size_multiplier, color: essence_color, size: GUI.Rm);
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

		public static partial class Charge
		{
			[Flags]
			public enum Flags: byte
			{
				None = 0,


			}

			[ITrait.Data(Net.SendType.Unreliable, IComponent.Scope.Global | IComponent.Scope.Region)]
			public partial struct Data(): ITrait
			{
				public IEssence.Handle h_essence;
				public Essence.Charge.Flags flags;
				private byte unused_00;

				private uint unused_01;

				public float amount;
				public float capacity;
			}
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
				[Name("Explosive", desc: "Single-use emitter activated by an explosive charge.")]
				Explosive,

				//[Name("Thumper", desc: "")]
				//Thumper,
			}

			[Flags]
			public enum Flags: ushort
			{
				None = 0,

				Internal = 1 << 0,
				External = 1 << 1,

				Self_Discharging = 1 << 4,

				Enable_Signal_Read = 1 << 7,
				Enable_Pulse_Event = 1 << 8,

				//Show_GUI = 1 << 0,
				//Allow_Edit_Rate = 1 << 1,
				//Allow_Edit_Frequency = 1 << 2
			}

			[Flags]
			public enum StateFlags: ushort
			{
				None = 0,

				Charging = 1 << 0,

				Pulse = 1 << 1,
			}

			[ITrait.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
			public partial struct Data(): ITrait
			{
				[Net.Segment.A, Editor.Picker.Position(relative: true)] public required Vec2f offset;
				[Net.Segment.A, Editor.Picker.Direction(normalize: true)] public required Vec2f direction = Vec2f.Down;

				[Net.Segment.A] public required Essence.Emitter.Type type;
				[Net.Segment.A] private byte unused_a_00;
				[Net.Segment.A] public Essence.Emitter.Flags flags;
				[Net.Segment.A, Save.Force, Editor.Slider.Clamped(0.00f, 10000.00f, snap: 1.00f)] public required float charge_capacity;
				[Net.Segment.A, Save.Force, Editor.Slider.Clamped(0.00f, 1.00f, snap: 0.001f)] public required float charge_loss = 0.03f;
				[Net.Segment.A, Save.Force, Editor.Slider.Clamped(0.00f, 1.00f, snap: 0.001f)] public required float efficiency = 1.00f;

				[Net.Segment.B] public IEssence.Handle h_essence;
				[Net.Segment.B] public Sound.Handle h_sound_emit;
				[Net.Segment.B] public Sound.Handle h_sound_discharge;
				[Net.Segment.B] public ISoundMix.Handle h_soundmix_test;

				[Net.Segment.C] public Essence.Emitter.StateFlags state_flags;
				[Net.Segment.C] public IEssence.Handle h_essence_charge;
				[Net.Segment.C] public Signal.Channel channel_emit;
				[Net.Segment.C] private byte unused_c_02;
				[Net.Segment.C] private ushort unused_c_03;

				[Net.Segment.D, Asset.Ignore] public float current_charge;
				[Net.Segment.D, Asset.Ignore] public float current_charge_ratio;
				[Net.Segment.D, Asset.Ignore] public float current_emit;
				[Net.Segment.D, Asset.Ignore] public float current_instability;




				//public float frequency;


				//public float rate_speed = 0.10f;
				//public float rate_max;
				//public float rate_target;
				//[Asset.Ignore] public float rate_current;
			}

			[ISystem.Update.D(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdate_Charge(ISystem.Info info, ref XorRandom random,
			//IComponent.Handle<Essence.Emitter.Data> h_essence_emitter, Entity ent_essence_emitter,
			IComponent.Handle<Essence.Emitter.Data> h_essence_emitter, Entity ent_essence_emitter,
			[Source.Owned] in Transform.Data transform, [Source.Owned] ref Essence.Container.Data essence_container,
			[Source.Owned] ref Body.Data body,
			//[Source.Owned, Pair.Wildcard] ref Essence.Emitter.Data essence_emitter)
			[Source.Owned, Pair.Component<Piston.Data>] ref Essence.Emitter.Data essence_emitter)
			{
				if (essence_emitter.flags.HasAny(Essence.Emitter.Flags.Self_Discharging))
				{
					if (essence_emitter.current_charge_ratio >= 1.00f)
					{
						var amount_taken = essence_emitter.current_charge.MultSub(0.98f);
#if SERVER
						essence_emitter.current_emit += amount_taken;
						if (essence_emitter.state_flags.TrySetFlag(Essence.Emitter.StateFlags.Pulse))
						{
							//App.WriteLine(h_essence_emitter);
							essence_emitter.Sync(ent_essence_emitter, IComponent.Handle.FromComponentPair<Piston.Data, Essence.Emitter.Data>());
						}
#endif
					}
					else
					{
						essence_emitter.current_emit = 0.00f;
						essence_emitter.state_flags.RemoveFlag(Essence.Emitter.StateFlags.Pulse);
					}
				}

				essence_emitter.h_essence_charge = essence_container.h_essence;
				essence_emitter.current_charge_ratio = Maths.NormalizeFastUnsafe(essence_emitter.current_charge, essence_emitter.charge_capacity);
				var rate = essence_container.rate_current * essence_emitter.efficiency;
				//essence_container.rate_current *= 1.00f - rate; // * App.fixed_update_interval_s_f32;

				essence_emitter.current_charge += (rate * essence_container.available) * App.fixed_update_interval_s_f32;
				essence_emitter.current_charge -= essence_emitter.current_charge * essence_emitter.charge_loss;
			}

			[ISystem.PreUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdate_Signal(ISystem.Info info, ref XorRandom random, Entity entity,
			IComponent.Handle<Essence.Emitter.Data> h_essence_emitter, Entity ent_essence_emitter,
			[Source.Owned] in Transform.Data transform, [Source.Owned] ref Analog.Relay.Data analog_relay,
			[Source.Owned, Pair.Wildcard] ref Essence.Emitter.Data essence_emitter)
			{
				return;
				if (essence_emitter.flags.HasNone(Flags.Enable_Signal_Read)) return;

				var signal_value = analog_relay.signal_current[essence_emitter.channel_emit];
				//essence_emitter.flags.SetFlag(Essence.Emitter.Flags.Pulsed, signal_value > 0.10f);

				if (essence_emitter.state_flags.TrySetFlag(Essence.Emitter.StateFlags.Pulse, signal_value > 0.10f))
				{
#if SERVER
					//var rec = entity.GetRecord();



					//static void Func(ref PulseEvent ev)
					//{
					//	ev.amount *= Maths.Atan2Fast(ev.dir.y, ev.dir.x); // (ev.amount); // -2, 2, ev.amount); // MathF.IEEERemainder(ev.amount, ev.dir.x);
					//}

					//var ts = Timestamp.Now();
					////Func(ref ev);
					//var ev = new PulseEvent()
					//{
					//	h_essence = essence_emitter.h_essence,
					//	emitter_type = essence_emitter.type,
					//	amount = signal_value,
					//	pos = transform.position,
					//	dir = essence_emitter.direction
					//};
					//ev.Trigger(entity);
					////ev.Trigger(rec, IComponent.Handle.FromComponent<Piston.Data>());
					////ev.TriggerDeferred(entity);
					//var ts_elapsed = ts.GetMilliseconds();

					//App.WriteLine($"{ts_elapsed:0.0000} ms");
#endif
				}
			}
		}

		[IEvent.Data]
		public partial struct PulseEvent(): IEvent
		{
			public required IEssence.Handle h_essence;
			public required Essence.Emitter.Type emitter_type;

			public required float amount;

			public required Vec2f pos;
			public required Vec2f dir;



		}

		//[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void OnUpdate(ISystem.Info info, ref XorRandom random,
		//IComponent.Handle<Essence.Emitter.Data> h_essence_emitter, Entity ent_essence_emitter,
		//[Source.Owned] in Transform.Data transform,
		//[Source.Owned, Pair.Wildcard] ref Essence.Emitter.Data essence_emitter)
		//{

		//}



		//[ISystem.PreUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void OnUpdateSignal(ISystem.Info info, ref XorRandom random,
		//IComponent.Handle<Essence.Emitter.Data> h_essence_emitter, Entity ent_essence_emitter,
		//[Source.Owned] in Transform.Data transform, [Source.Owned] in Analog.Relay.Data analog_relay,
		//[Source.Owned, Pair.Wildcard] ref Essence.Emitter.Data essence_emitter)
		//{
		//	var signal_value = analog_relay.signal_current[essence_emitter.channel_emit];
		//	essence_emitter.flags.SetFlag(Emitter.Flags.Pulsed, signal_value > 0.10f);
		//}

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
		public delegate void GetAllNodesQuery(ISystem.Info info, Entity entity,
			[Source.Owned] ref EssenceNode.Data essence_node, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body);

		// TODO: temporary, will be removed
		public enum Type: uint
		{
			Undefined,

			Heat,
			Motion,
			Life,
			Radiance,
			Cognition,
			Electricity,
			Failure,
			Anentropy
		}

		//[Obsolete] public static readonly Dictionary<IMaterial.Handle, IEssence.Handle> material_to_essence = new();
		//[Obsolete] public static readonly Dictionary<IEssence.Handle, IMaterial.Handle> essence_to_material = new();

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

		public static IEssence.Handle GetEssenceType(this Resource.Data resource) => Essence.GetEssenceType(resource.material);
		public static IEssence.Handle GetEssenceType(this IMaterial.Handle material) => IEssence.material_to_essence[(byte)material.id];

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
