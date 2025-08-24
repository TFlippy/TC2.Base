
namespace TC2.Base.Components
{
	public static partial class Heat
	{
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[IComponent.Data(Net.SendType.Reliable, name: "Heatable", region_only: true, sync_table_capacity: 256), IComponent.With<Heat.State>()]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Exclude_Body_Mass = 1u << 0,
				No_Smoke = 1u << 1,
				No_Sparks = 1u << 2,

				[Obsolete] No_Ignite = 1u << 4,

				No_GUI = 1u << 5,
				No_Self_Damage = 1u << 6,
				No_Held_Damage = 1u << 7,
				No_Convection_Damage = 1u << 8,

				Meltable = 1u << 10,
				Formless = 1u << 11,
			}

			public Temperature temperature_medium = Temperature.Celsius(150.00f);
			public Temperature temperature_high = Temperature.Celsius(525.00f);
			public Temperature temperature_ignite;
			[Statistics.Info("Temperature (Max)", description: "Maximum operating temperature.", format: "{0:0.##} K", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public Temperature temperature_operating = Temperature.Celsius(620.00f);
			public Temperature temperature_melt;
			public Temperature temperature_breakdown = Temperature.Celsius(2000.00f);

			[Statistics.Info("Dissipation", description: "Cooling rate.", format: "{0:0.##} kW", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public Power cool_rate = 1.00f;

			[Statistics.Info("Heat Capacity (Extra)", description: "Extra heat capacity.", format: "{0:0.##} kJ", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public Energy heat_capacity_extra;

			public float heat_capacity_mult = 1.00f;
			public float heat_damage_mult = 1.00f;
			public float cool_rate_mult = 1.00f;

			public float smoke_size_mult = 1.00f;
			public float smoke_speed_mult = 1.00f;
			public float smoke_rise_mult = 1.00f;

			public Heat.Data.Flags flags;
			public Fire.Flags fire_flags = Fire.Flags.No_Radius_Ignite;

			[Editor.Picker.Position(true)]
			public Vector2 offset;

			[Editor.Picker.Position(true)]
			public Vector2 smoke_offset;

			public Vector2 size = new(0.50f, 0.25f);

			public Color32BGRA flame_tint;
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true, sync_table_capacity: 256)]
		public partial struct State(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,

				Overheated = 1 << 0,
				Melting = 1 << 1,
				[Save.Ignore, Asset.Ignore] Cached = 1 << 15,
			}

			public Temperature temperature_current = Temperature.Ambient;
			public Energy heat_capacity = 1.00f;
			public float modifier = 1.00f;
			public Heat.State.Flags flags;

			[Asset.Ignore, Save.Ignore] public float overheat_ratio_current;
			[Asset.Ignore, Save.Ignore] public Energy heat_capacity_inv = 1.00f;

			[Asset.Ignore, Net.Ignore, Save.Ignore] public Temperature temperature_ambient = Temperature.Ambient;
			[Asset.Ignore, Net.Ignore, Save.Ignore] public float mass_cached;

			[Asset.Ignore, Net.Ignore, Save.Ignore] public float t_next_ambient;
			[Asset.Ignore, Net.Ignore, Save.Ignore] public float t_next_properties;
			[Asset.Ignore, Net.Ignore, Save.Ignore] public float t_next_effect;
			[Asset.Ignore, Net.Ignore, Save.Ignore] public float t_next_convection;
		}


		[IEvent.Data]
		public partial struct MeltEvent(): IEvent, IEvent<Crafting.ConfiguredRecipe>
		{
			public Temperature temperature;

			void IEvent<Crafting.ConfiguredRecipe>.Bind(ref Crafting.ConfiguredRecipe data)
			{

			}
		}

		public static void SetupFromSubstance(ref this Heat.Data heat, ISubstance.Handle h_substance)
		{
			ref var substance_data = ref h_substance.GetData();
			if (substance_data.IsNotNull())
			{
				heat.temperature_ignite = 0.00f;
				
				heat.temperature_operating = Maths.Lerp(substance_data.sintering_temperature_lower, substance_data.sintering_temperature_upper, 0.50f);
				heat.temperature_medium = Temperature.Celsius(Maths.Lerp(80.00f, substance_data.sintering_temperature_lower.Celsius(), 0.25f));
				heat.temperature_high = Maths.Lerp(substance_data.forging_temperature_lower, substance_data.melting_point, 0.80f);
				if (substance_data.melting_point != 0.00f) heat.temperature_melt = substance_data.melting_point;
				if (substance_data.autoignition_temperature != 0.00f) heat.temperature_ignite = substance_data.autoignition_temperature;
				heat.temperature_breakdown = substance_data.boiling_point;
				if (substance_data.color_flame != default) heat.flame_tint = substance_data.color_flame;

				if (heat.flags.HasAny(Data.Flags.Meltable))
				{
					heat.temperature_high = Maths.Lerp(substance_data.melting_point, substance_data.boiling_point, 0.65f);
				}

				Maths.MinMax(ref heat.temperature_operating, ref heat.temperature_high);
				Maths.MinMax(ref heat.temperature_medium, ref heat.temperature_high);
				if (heat.flags.HasAny(Data.Flags.Meltable))
				{
					//Maths.MinMax(ref heat.temperature_melt, ref heat.temperature_high);
				}
				else
				{
					//Maths.MinMax(ref heat.temperature_high, ref heat.temperature_melt);
				}
				Maths.MinMax(ref heat.temperature_high, ref heat.temperature_breakdown);
			}
		}


#if CLIENT
		public struct HeatGUI: IGUICommand
		{
			public Entity ent_heat;

			public Transform.Data transform;

			public Heat.Data heat;
			public Heat.State heat_state;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Heat"u8, this.ent_heat, size: new(48, 96 * 1), min_width: 48))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (GUI.Group.New(size: GUI.Rm))
						{
							//GUI.SameLine(24);
							using (var group_left = GUI.Group.New(size: new(GUI.RmX * 0.50f, GUI.RmY)))
							{
								group_left.DrawBackground(GUI.tex_slot_filled);
							}
							GUI.SameLine();
							GUI.DrawTemperatureRange(this.heat_state.temperature_current, this.heat.temperature_operating, 2000, size: new(24, GUI.RmY), label_b: "Current"u8, label_a: "Max"u8);
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region, order: -100)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Heat.Data heat, [Source.Owned] in Heat.State heat_state)
		{
			if (heat.flags.HasNone(Data.Flags.No_GUI) && interactable.IsActive())
			{
				var gui = new HeatGUI()
				{
					ent_heat = entity,

					transform = transform,

					heat = heat,
					heat_state = heat_state
				};
				gui.Submit();
			}
		}
#endif
		//public static void EqualizeTemperature(ref Temperature temperature_a, ref Temperature temperature_b, Mass mass, Energy specific_heat)
		//{

		//}

		public static void AddEnergy(ref this Heat.State heat_state, Energy energy)
		{
			//var heat = amount / Maths.Max(heat_state.heat_capacity_extra + (mass * 0.10f), 1.00f);
			heat_state.temperature_current.m_value.FMARef(energy.m_value, heat_state.heat_capacity_inv.m_value);
		}

		public static void AddPower(ref this Heat.State heat_state, Power power, float dt)
		{
			//var heat = amount / Maths.Max(heat_state.heat_capacity_extra + (mass * 0.10f), 1.00f);

			power.m_value *= dt;
			heat_state.temperature_current.m_value.FMARef(power.m_value, heat_state.heat_capacity_inv.m_value);
		}

		[ISystem.Monitor(ISystem.Mode.Single, ISystem.Scope.Region), HasComponent<Heat.Data>(Source.Modifier.Owned, true)]
		public static void OnAddRem(ISystem.Info info, [Source.Owned] ref Body.Data body)
		{
			switch (info.EventType)
			{
				case ISystem.EventType.Add:
				{
					body.override_shape_layer.AddFlag(Physics.Layer.Heatable);
					body.MarkDirty();
				}
				break;

				case ISystem.EventType.Remove:
				{
					body.override_shape_layer.RemoveFlag(Physics.Layer.Heatable);
					body.MarkDirty();
				}
				break;
			}
		}

		public const float interval_ambient = 0.23f;
		public const float interval_properties = 0.77f;

		public static Sound.Handle h_sound_sizzle = "effect.sizzle.sparky.00";

		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnAttach(ISystem.Info info, ref Region.Data region,
		Entity ent_transform, Entity ent_transform_parent,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned] in Transform.Data transform, [Source.Parent] in Transform.Data transform_parent,
		[Source.Parent] in Joint.Base joint_base)
		{
			//App.WriteLine("add heat attached");
		}

		public static readonly Temperature temperature_holdable_max = Temperature.Celsius(110.00f);

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.273f), HasTag("initialized", true, Source.Modifier.Owned)]
		public static void OnUpdateAttached(ISystem.Info info, ref Region.Data region, ref XorRandom random,
		Entity ent_body, Entity ent_body_parent, Entity ent_joint_base,
		[Source.Owned] in Body.Data body, [Source.Parent] in Body.Data body_parent, [Source.Parent] in Joint.Base joint_base,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state)
		{
#if SERVER
			// TODO: hardcoded
			if (heat.flags.HasNone(Heat.Data.Flags.No_Held_Damage) && joint_base.material_type == Material.Type.Flesh)
			{
				var temperature_celsius = heat_state.temperature_current.Celsius();
				if (temperature_celsius >= 50.00f && random.NextBool(temperature_celsius * 0.005f))
				{
					var damage_type = Damage.Type.Steam;
					if (temperature_celsius >= 1337.00f) damage_type = Damage.Type.Phlogiston;
					else if (temperature_celsius >= 550.00f) damage_type = Damage.Type.Fire;
					else if (temperature_celsius >= 120.00f) damage_type = Damage.Type.Heat;

					var damage = temperature_celsius * random.NextFloatRange(0.40f, 0.80f);
					Damage.Hit(ent_attacker: ent_body,
						ent_owner: default,
						ent_target: ent_body_parent,
						position: body_parent.GetPosition(),
						velocity: new Vector2(0.00f, -0.25f),
						normal: new(0.00f, -1.00f),
						damage_integrity: damage,
						damage_durability: damage,
						damage_terrain: damage,
						target_material_type: body_parent.GetMaterial(),
						damage_type: damage_type,
						yield: 0.90f,
						xp_modifier: 0.00f,
						impulse: 0.00f,
						flags: Damage.Flags.No_Loot_Pickup | Damage.Flags.No_Loot_Notification);

					if (temperature_celsius >= temperature_holdable_max && random.NextBool(temperature_celsius * 0.005f))
					{
						Arm.Drop(ent_joint_base, direction: body_parent.Right.RotateByRad(random.NextFloat(0.10f)) * 3);
						Sound.Play(region: ref region,
							sound: h_sound_sizzle,
							world_position: body.GetPosition(),
							volume: 0.90f,
							pitch: random.NextFloatExtra(1.60f, 0.30f),
							size: 1.50f,
							priority: 0.25f,
							dist_multiplier: 1.10f);
					}
				}
			}
#endif
		}

		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("initialized", true, Source.Modifier.Owned)]
		[ISystem.Modified(ISystem.Mode.Single, ISystem.Scope.Region, order: 1000)]
		public static void OnModified(ISystem.Info info, Entity entity,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned] ref Body.Data body, [Source.Owned, Optional] in Resource.Data resource)
		{
#if SERVER
			heat_state.flags.RemoveFlag(Heat.State.Flags.Cached);

			var modifier = 1.00f;
			if (resource.material != 0)
			{
				modifier = resource.GetQuantityNormalized();
			}
			heat_state.modifier = modifier;
#endif
			//#if SERVER
			//			var modifier = 1.00f;
			//			if (resource.material.id != 0)
			//			{
			//				modifier = resource.GetQuantityNormalized();
			//			}

			//			if (heat.flags.HasAny(Heat.Data.Flags.Exclude_Body_Mass))
			//			{
			//				heat_state.heat_capacity.m_value = Maths.Max(heat.heat_capacity_extra.m_value * modifier, 0.10f);
			//			}
			//			else
			//			{
			//				heat_state.heat_capacity.m_value = Maths.Max(Maths.FMA(body.GetMass().ClampMin(1.00f), heat.heat_capacity_mult, heat.heat_capacity_extra.m_value * modifier), 0.10f);
			//			}

			//			heat_state.modifier = modifier;
			//			heat_state.heat_capacity_inv.m_value = 1.00f / heat_state.heat_capacity.m_value;

			//			App.WriteLine($"modified {heat_state.modifier}; {heat_state.heat_capacity}; {heat_state.heat_capacity_inv}; {resource.quantity}");

			//			//heat.Sync(entity, true);
			//			heat_state.Sync(entity, true);
			//#endif
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("initialized", true, Source.Modifier.Owned)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body)
		{
			var modifier = heat_state.modifier;

#if SERVER
			var is_body_dirty = false;
			var mass = body.GetMass().ClampMin(1.00f);
			if (mass != heat_state.mass_cached) // && heat_state.flags.TryAddFlag(Heat.State.Flags.Cached))
			{
				if (heat.flags.HasAny(Heat.Data.Flags.Exclude_Body_Mass))
				{
					heat_state.heat_capacity.m_value = Maths.Max(heat.heat_capacity_extra.m_value * modifier, 0.10f);
				}
				else
				{
					heat_state.heat_capacity.m_value = Maths.Max(Maths.FMA(mass, heat.heat_capacity_mult, heat.heat_capacity_extra.m_value * modifier), 0.10f);
				}

				heat_state.modifier = modifier;
				heat_state.heat_capacity_inv.m_value = 1.00f / heat_state.heat_capacity.m_value;

				//App.WriteLine($"modified {heat_state.modifier}; {heat_state.heat_capacity}; {heat_state.heat_capacity_inv}");

				heat_state.mass_cached = mass;
				heat_state.Sync(entity, true);
			}
#endif

			var temperature_ambient = heat_state.temperature_ambient;
			var temperature_current = heat_state.temperature_current;

			var time = info.WorldTime;
			if (time >= heat_state.t_next_ambient)
			{
				heat_state.t_next_ambient = time + interval_ambient;
				heat_state.temperature_ambient = temperature_ambient = region.GetAmbientTemperature(transform.LocalToWorld(heat.offset));
			}

			Phys.TransferHeatAmbientSimpleFast(temperature: ref temperature_current, temperature_ambient: temperature_ambient,
				heat_capacity_inv: heat_state.heat_capacity_inv, rate: heat.cool_rate * heat.cool_rate_mult * heat_state.modifier,
				dt: info.DeltaTime);

			if (!heat_state.flags.TryAddFlag(Heat.State.Flags.Overheated, temperature_current >= heat.temperature_operating))
			{
				//if (heat.flags.HasAny(Data.Flags. )				
				heat_state.flags.RemoveFlag(Heat.State.Flags.Overheated, temperature_current <= heat.temperature_medium);
			}


#if SERVER
			if (time >= heat_state.t_next_properties)
			{
				heat_state.t_next_properties = time + interval_properties;

				is_body_dirty |= body.override_shape_layer.TrySetFlag(Physics.Layer.Hot, temperature_current >= Temperature.Celsius(100.00f));
				if (heat.temperature_melt != 0.00f)
				{
					is_body_dirty |= body.override_shape_layer.TrySetFlag(Physics.Layer.Liquid, temperature_current >= heat.temperature_melt);
					is_body_dirty |= body.override_shape_mask.TrySetFlag(Physics.Layer.Liquid | Physics.Layer.Platform | Physics.Layer.Bulk | Physics.Layer.Item | Physics.Layer.Wheel | Physics.Layer.Creature, temperature_current >= heat.temperature_melt);
				}
			}

			if (is_body_dirty)
			{
				body.MarkDirty();
			}
#endif

			heat_state.temperature_current = temperature_current;

			//if (heat.flags.HasAny(Heat.Flags.Heated))
			//{
			//	if (temperature_current <= heat.temperature_medium)
			//	{
			//		heat.flags &= ~Heat.Flags.Heated;
			//	}
			//}

			//#if SERVER
			//			if (info.WorldTime >= heat.next_sync && heat.heat_current > ambient_temperature)
			//			{
			//				heat.Sync(entity);
			//				heat.next_sync = info.WorldTime + 3.00f;
			//			}
			//#endif
		}

#if SERVER
		[ChatCommand.Region("heat", "", creative: true)]
		public static void HeatCommand(ref ChatCommand.Context context, float energy)
		{
			ref var region = ref context.GetRegion();
			Assert.IsNotNull(ref region);

			var ent_target = context.GetTargetEntity();
			if (!ent_target.IsAlive())
			{
				var pos = context.GetTargetPosition();
				App.WriteLine(pos);

				if (region.TryOverlapPoint(pos, 0.50f, out var result, mask: Physics.Layer.Heatable, exclude: Physics.Layer.World))
				{
					ent_target = result.entity;
				}
			}

			ref var heat = ref ent_target.GetComponent<Heat.Data>();
			ref var heat_state = ref ent_target.GetComponent<Heat.State>();
			if (heat.IsNotNull() && heat_state.IsNotNull())
			{
				heat_state.AddEnergy(energy);
				heat_state.Sync(ent_target);
			}
		}

		[ChatCommand.Region("temperature", "", creative: true)]
		public static void TemperatureCommand(ref ChatCommand.Context context, float temperature)
		{
			ref var region = ref context.GetRegion();
			Assert.IsNotNull(ref region);

			var ent_target = context.GetTargetEntity();

			App.WriteLine(ent_target);
			App.WriteLine(ent_target.IsAlive());

			if (!ent_target.IsAlive())
			{
				var pos = context.GetTargetPosition();
				App.WriteLine(pos);

				if (region.TryOverlapPoint(pos, 0.50f, out var result, mask: Physics.Layer.Heatable, exclude: Physics.Layer.World))
				{
					ent_target = result.entity;
				}
			}

			ref var heat = ref ent_target.GetComponent<Heat.Data>();
			ref var heat_state = ref ent_target.GetComponent<Heat.State>();
			if (heat.IsNotNull() && heat_state.IsNotNull())
			{
				heat_state.temperature_current = temperature;
				heat_state.Sync(ent_target);
			}
		}
#endif

#if SERVER
		[ISystem.Event<Crafting.SpawnEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnSpawnEvent(ISystem.Info info, ref Region.Data region, Entity entity, ref Crafting.SpawnEvent ev,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state)
		{
			//App.WriteLine($"spawn {data.temperature} {data.product.flags}");
			if (ev.product.flags.HasAny(Crafting.Product.Flags.Use_Temperature))
			{
				heat_state.temperature_current = ev.temperature;
				heat_state.flags.RemoveFlag(Heat.State.Flags.Cached);
				heat_state.Sync(entity, true);
			}
		}

		[ISystem.Event<Resource.SpawnEvent>(ISystem.Mode.Single, ISystem.Scope.Region, order: 5)]
		public static void OnSpawnResource(ISystem.Info info, ref Region.Data region, Entity entity, ref Resource.SpawnEvent ev,
		[Source.Owned] in Resource.Data resource, [Source.Owned] ref Heat.State heat_state)
		{
			heat_state.temperature_current.TrySet(ev.temperature);
			heat_state.flags.RemoveFlag(Heat.State.Flags.Cached);
			heat_state.Sync(entity, true);
		}

		[ISystem.Event<Resource.MergeEvent>(ISystem.Mode.Single, ISystem.Scope.Region, order: 10)]
		public static void OnMergeResource(ISystem.Info info, ref Region.Data region, Entity entity, ref Resource.MergeEvent ev,
		[Source.Owned] in Resource.Data resource, [Source.Owned] ref Heat.State heat_state)
		{
			var amount_rem = Maths.Min(resource.GetMaxQuantity() - resource.quantity, ev.resource.quantity);
			if (amount_rem > 0)
			{
				var mass_per_unit = resource.GetMassPerUnit();
				var temperature_new = Temperature.Merge(heat_state.temperature_current, ev.temperature, resource.quantity * mass_per_unit, amount_rem * mass_per_unit);
				//App.WriteLine($"OnMergeResource(): add temperature: {ev.temperature} to {heat_state.temperature_current} => {temperature_new}; amount_rem: {amount_rem}");

				heat_state.temperature_current = temperature_new;
				heat_state.flags.RemoveFlag(Heat.State.Flags.Cached);
				heat_state.Sync(entity, true);
			}
		}

		[ISystem.PostUpdate.F(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.38f), HasTag("initialized", true, Source.Modifier.Owned)]
		public static void OnUpdate_HeatDamage(ISystem.Info info, Entity entity, ref XorRandom random,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Health.Data health,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state, [Source.Owned] ref Body.Data body)
		{
			var temperature_current = heat_state.temperature_current;
			if (heat.temperature_ignite != 0.00f && temperature_current > heat.temperature_ignite)
			{
				var excess = Maths.Max(0.00f, temperature_current - heat.temperature_ignite);
				if (random.NextBool(excess * 0.01f))
				{
					Fire.Ignite(ent_target: entity, intensity: temperature_current * 0.019f, flags: heat.fire_flags, temperature: temperature_current, tint: heat.flame_tint.OrNullable());
				} 
			}

			if (heat.flags.HasNone(Heat.Data.Flags.No_Self_Damage) && temperature_current > heat.temperature_high)
			{
				var modifier = Maths.InvLerp(heat.temperature_high, heat.temperature_breakdown, temperature_current).Pow2();
				if (random.NextBool(modifier))
				{
					var damage = modifier * health.GetMaxHealth() * random.NextFloatExtra(0.35f, 0.65f) * heat.heat_damage_mult;
					Damage.Hit(ent_attacker: entity,
						ent_owner: default,
						ent_target: entity,
						position: transform.LocalToWorld(heat.offset),
						velocity: random.NextUnitVector2Range(1, 1),
						normal: random.NextUnitVector2Range(1, 1),
						damage_integrity: damage,
						damage_durability: damage,
						damage_terrain: damage,
						target_material_type: body.GetMaterial(),
						damage_type: Damage.Type.Heat,
						yield: 0.90f,
						xp_modifier: 0.00f,
						impulse: 0.00f,
						flags: Damage.Flags.No_Loot_Pickup | Damage.Flags.No_Loot_Notification);
				}
			}
		}
#endif

#if CLIENT
		[ISystem.Event<Interactor.HoverEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnHover(ref Region.Data region, ISystem.Info info, Entity ent_heatable, [Source.Owned] ref Interactor.HoverEvent ev,
		[Source.Owned] in Heat.State heat_state)
		{
			if (heat_state.temperature_current >= temperature_holdable_max)
			{
				GUI.Title("! HOT !"u8, color: GUI.font_color_red_b.WithAlphaMult(ev.alpha));
			}
		}
#endif

#if CLIENT
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned, Pair.Component<Heat.Data>, Optional(true)] ref Light.Data light,
		[Source.Owned, Pair.Component<Heat.Data>, Optional(true)] ref Sound.Emitter sound_emitter)
		{
			//light.offset = heat.offset;

			var time = info.WorldTime;
			var temperature_current = heat_state.temperature_current;

			if (light.IsNotNull())
			{
				light.color.LerpRef(Temperature.GetGlowColor(temperature_current), 0.07f); // = Maths.Max(heat.temperature_current - 150.00f, 0.00f) / 250.00f;
				light.size_modifier.LerpFMARef(Maths.Max(heat_state.modifier * ((temperature_current.m_value * 0.0006f).Pow2()), 0.750f), 0.02f);
				light.jitter *= 0.97f;
			}

			if (sound_emitter.IsNotNull())
			{
				//sound_emitter.offset = heat.offset;
				sound_emitter.volume_mult = Maths.Clamp(Maths.InvLerp(heat.temperature_medium * 0.95f, heat.temperature_high * 1.45f, temperature_current).Mul(0.85f).ClampMin(0.00f).Pow2(), 0.00f, 0.40f);
				sound_emitter.pitch_mult = 0.50f + Maths.Clamp(Maths.InvLerp(heat.temperature_medium * 1.25f, heat.temperature_high * 1.60f, temperature_current).Mul(0.75f).ClampMin(0.00f).Pow2(), 0.05f, 0.50f);
			}

			if (heat.flags.HasNone(Heat.Data.Flags.No_Smoke) && temperature_current >= Temperature.Celsius(75.00f) && time >= heat_state.t_next_effect)
			{
				heat_state.t_next_effect = time + 0.04f;

				//var dir = transform.GetDirection();
				var intensity = Maths.Clamp(Maths.Max(temperature_current - Temperature.Celsius(50.00f), 0.00f) * 0.003f, 0.00f, 0.75f).Pow2();
				var pos = transform.LocalToWorld(heat.smoke_offset + (new Vector2(random.NextFloat(heat.size.X), random.NextFloat(heat.size.Y)) * 0.85f));

				if (light.IsNotNull())
				{
					light.jitter = random.NextUnitVector2(intensity * 0.10f);
					//light.size_modifier *= intensity;
				}

				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = texture_smoke,
					lifetime = random.NextFloatExtra(0.50f, 0.80f),
					pos = pos, // + (dir * random.NextFloatRange(0.00f, 1.00f))),
					vel = (new Vector2(random.NextFloat(intensity), random.NextFloat01() * -intensity * 4) * heat.smoke_speed_mult) + new Vector2(random.NextFloat01(3), -random.NextFloat01(2)), // (-dir * random.NextFloatRange(3.00f, 6.00f) * intensity) + new Vector2(0, -2),
					force = new Vector2(0.10f, -0.40f) * heat.smoke_speed_mult,
					fps = random.NextByteRange(15, 20),
					frame_count = 64,
					frame_count_total = 64,
					frame_offset = random.NextByteRange(0, 64),
					scale = random.NextFloatRange(0.30f, 0.50f) * heat.smoke_size_mult,
					rotation = random.NextFloat(float.Pi),
					angular_velocity = random.NextFloatExtra(1.00f, 2.00f) * heat.smoke_speed_mult,
					growth = random.NextFloatExtra(0.20f, 0.30f),
					color_a = new Color32BGRA(0xffffffff).WithAlphaMult(intensity * 0.50f),
					color_b = new Color32BGRA(0x00ffffff),
				});

				if (temperature_current >= 1450.00f)
				{
					var intensity_spark = Maths.InvLerp(1200, 2500, temperature_current).Clamp01();
	
					var pos_spark = transform.LocalToWorld(heat.offset + (new Vector2(random.NextFloat(heat.size.X), 0.00f)));
					var dir_up = transform.Up;

					var glow_color = Temperature.GetGlowColor(temperature_current);
					var glow_color_clamped = glow_color.WithAlpha(1.00f);

					for (var i = 0; i < 2; i++)
					{
						var lit = random.NextBool(0.60f);
						Particle.Spawn(ref region, new Particle.Data()
						{
							texture = Welder.metal_spark_01,
							lifetime = random.NextFloatRange(0.20f, 0.40f),
							pos = pos_spark + random.NextVector2(0.50f),
							vel = (dir_up.RotateByRad(random.NextFloat(2.00f)) * random.NextFloatExtra(1, 5) * 3 * intensity_spark) + new Vector2(random.NextFloat(75.00f * intensity), random.NextFloatExtra(2.00f, -15.00f)),
							fps = 8,
							frame_count = 4,
							frame_offset = random.NextByteRange(0, 4),
							frame_count_total = 4,
							scale = random.NextFloatRange(0.70f, 1.10f) * intensity_spark * (lit ? 0.75f : 1.20f),
							growth = -random.NextFloatRange(0.30f, 0.60f),
							force = new Vector2(0, random.NextFloatRange(15, 80)),
							drag = random.NextFloatRange(0.01f, 0.04f),
							stretch = new Vector2(random.NextFloatRange(0.70f, 1.10f), 0.65f),
							color_a = lit ? glow_color_clamped : glow_color,
							color_b = (lit ? glow_color_clamped : glow_color).WithAlpha(0),
							lit = lit ? 1.00f : 0.00f,
							glow = lit ? 0.00f : 4.00f + (intensity_spark * 5.00f),
							face_dir_ratio = 1.00f,
						});
					}

					Shake.Emit(ref region, pos_spark, 0.10f, 0.25f * intensity_spark.Clamp01(), radius: 24.00f * intensity_spark);
					if (random.NextBool(0.07f))
					{
						//Sound.Play(h_sound_sizzle, pos_spark, volume: random.NextFloatExtra(0.30f, 0.50f) * intensity_spark, pitch: random.NextFloatExtra(0.30f, 0.50f) * intensity_spark, size: 1.00f, dist_multiplier: 0.50f, priority: 0.10f);
					}

					//for (var i = 0; i < 10; i++)
					//{
					//	var lit = random.NextBool(0.50f);

					//	Particle.Spawn(ref region, new Particle.Data()
					//	{
					//		texture = Welder.metal_spark_01,
					//		lifetime = random.NextFloatRange(0.60f, 1.80f) * (lit ? 1.00f : 0.50f),
					//		pos = pos_spark + random.NextVector2(0.20f),
					//		vel = (dir_up * random.NextFloatRange(0, 40)).RotateByRad(random.NextFloatRange(-1.00f, 1.00f)) + random.NextVector2(15),
					//		fps = 0,
					//		frame_count = 1,
					//		frame_offset = random.NextByteRange(0, 4),
					//		frame_count_total = 4,
					//		scale = random.NextFloatRange(0.70f, 1.00f) * (lit ? 1.00f : 0.80f),
					//		growth = -random.NextFloatRange(0.50f, 0.70f),
					//		force = new Vector2(0.00f, random.NextFloatRange(-20, 40)),
					//		drag = random.NextFloatRange(0.01f, 0.05f),
					//		stretch = new Vector2(random.NextFloatRange(1.50f, 2.00f), 0.75f),
					//		color_a = random.NextColor32Range(0xffffffff, 0xffffc0b0),
					//		color_b = random.NextColor32Range(0xffffc0b0, 0xffffa090),
					//		lit = lit ? 1.00f : 0.00f,
					//		glow = lit ? 0.00f : 5.00f,
					//		face_dir_ratio = 1.00f,
					//	});
					//}
				}
			}
		}

		//[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateSound(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity, [Source.Owned] in Transform.Data transform,
		//[Source.Owned] in Heat.Data heat, [Source.Owned] ref Heat.State heat_state, [Source.Owned, Pair.Component<Heat.Data>] ref Sound.Emitter sound_emitter)
		//{
		//	sound_emitter.offset = heat.offset;
		//	sound_emitter.volume_mult = Maths.Clamp(Maths.Max(heat.temperature_current - heat.temperature_medium, 0.00f) / 4000.00f, 0.00f, 0.20f);
		//	sound_emitter.pitch_mult = 0.50f + Maths.Clamp(Maths.Max(heat.temperature_current - heat.temperature_medium, 0.00f) / 4000.00f, 0.00f, 0.40f);

		//	if (heat.temperature_current >= 100.00f && info.WorldTime >= heat.t_next_steam)
		//	{
		//		var dir = transform.GetDirection();
		//		var intensity = Maths.Clamp(Maths.Max(heat.temperature_current - heat.temperature_medium, 0.00f) / 50.00f, 0.00f, 0.30f);

		//		heat.t_next_steam = info.WorldTime + 0.04f;
		//		Particle.Spawn(ref region, new Particle.Data()
		//		{
		//			texture = texture_smoke,
		//			lifetime = random.NextFloatRange(0.50f, 1.00f),
		//			pos = transform.LocalToWorld(heat.offset + (new Vector2(random.NextFloatRange(-heat.size.X, heat.size.X), random.NextFloatRange(-heat.size.Y, heat.size.Y)) * 0.50f)), // + (dir * random.NextFloatRange(0.00f, 1.00f))),
		//			vel = new Vector2(random.NextFloatRange(-intensity, intensity), random.NextFloatRange(0.00f, -intensity * 10)), // (-dir * random.NextFloatRange(3.00f, 6.00f) * intensity) + new Vector2(0, -2),
		//			force = new Vector2(0.10f, -0.40f),
		//			fps = random.NextByteRange(15, 20),
		//			frame_count = 64,
		//			frame_count_total = 64,
		//			frame_offset = random.NextByteRange(0, 64),
		//			scale = random.NextFloatRange(0.30f, 0.50f),
		//			rotation = random.NextFloat(10.00f),
		//			angular_velocity = random.NextFloatRange(1.00f, 3.00f),
		//			growth = 0.40f,
		//			color_a = new Color32BGRA(0xc0ffffff).WithAlphaMult(intensity),
		//			color_b = new Color32BGRA(0x00ffffff),
		//		});
		//	}
		//}
#endif
	}
}
