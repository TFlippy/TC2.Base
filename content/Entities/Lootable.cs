
namespace TC2.Base.Components
{
	public static class Breakable
	{
		internal const bool debug_log = false;

		[Flags]
		public enum Flags: ushort
		{
			None = 0,

			No_Damage = 1 << 0,
			No_Offset = 1 << 1,
			No_Mass_Conversion = 1 << 2,
			Splittable = 1 << 3,
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region, sync_table_capacity: 256)]
		public struct Data(): IComponent
		{
			//public IMaterial.Handle h_material;
			public Breakable.Flags flags;

			//public IMaterial.Conversion.Type conversion_type;
			//public IMaterial.Conversion.Flags conversion_flags;
			public Resource.SpawnFlags spawn_flags;
			public Resource.SpawnFlags spawn_flags_rem;
			public float merge_radius = 4.00f;
			//public Material.Type material_type;

			//public Material.Flags material_flags;
		}

		[Shitcode]
		[ISystem.Event<Health.DamageEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnDamage(ref Region.Data region, ISystem.Info info, Entity entity, ref Health.DamageEvent ev, ref XorRandom random,
		[Source.Owned] in Health.Data health, [Source.Owned] ref Resource.Data resource, [Source.Owned] ref Breakable.Data breakable, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body)
		{
			if (ev.flags.HasAny(Damage.Flags.No_Loot_Drop | Damage.Flags.No_Damage) || ev.yield <= 0.01f) return;

			//var yield = data.yield;
			//if (yield >= 0.01f)


			if (resource.material.HasConversion(ev.damage_type))
			{
				//var damage = ev.damage_integrity;
				//var amount_multiplier = damage * health.GetMaxHealth().RcpFast01();

				//var ent_attacker = data.ent_attacker;

				//resource.material.GetConversion()

				//ref var material = ref resource.material.GetData();
				//if (material.IsNotNull() && material.conversions != null && material.conversions.TryGetValue(ev.damage_type, out var conv) && random.NextBool(conv.chance))
#if SERVER
				ref var conv = ref resource.material.GetConversion(ev.damage_type);
				if (conv.IsNotNull() && random.NextBool(conv.chance))
				{
					var ent_owner = ev.ent_owner;
					var spawn_flags = breakable.spawn_flags;

					var has_no_offset = breakable.flags.HasAny(Breakable.Flags.No_Offset) | conv.spawn_flags.HasAny(Resource.SpawnFlags.No_Offset);
					var has_no_mass_conversion = breakable.flags.HasAny(Breakable.Flags.No_Mass_Conversion) | conv.flags.HasAny(IMaterial.Conversion.Flags.No_Mass_Conversion);

					var yield = Maths.ClampMin(conv.yield * ev.yield, 0.00f);
					var damage = ev.damage_integrity * yield;
					var amount_multiplier = damage * health.GetMaxHealthInvFast();

					var amount = Maths.Min(resource.quantity, Maths.Ceil(resource.quantity * amount_multiplier));
					var amount_rem = amount;

					//var yield = Maths.Clamp01(conv.yield * ev.yield);

					var conv_ratio = random.NextFloatExtra(conv.ratio, conv.ratio_extra).Clamp01();
					var waste_ratio = random.NextFloatExtra(conv.ratio_waste, conv.ratio_waste_extra).Clamp01();

					//App.WriteLine($"amount: {amount}; conv_ratio: {conv_ratio}");
					var amount_converted = amount_rem.SplitRef(conv_ratio);
					var amount_wasted = amount_rem.SplitRef(waste_ratio);

					//amount_taken.Split(data.yield * conv.yield, out var amount_wasted, out var amount_converted);
					//App.WriteLine($"taken: {amount_taken}; wasted: {amount_wasted}; converted: {amount_converted}; rem: {amount_rem}; yield: {yield}");

					//var amount_wasted = amount_rem.MultDiff(conv.yield);
					//App.WriteLine($"amount_wasted: {amount_wasted}; {amount_rem}");

					//amount_taken -= amount_converted;
					//amount_taken *= conv.yield;

					//resource.quantity -= amount_converted;
					//var amount_converted_corrected = amount_converted * (material.mass_per_unit / material_conv.mass_per_unit);

					if (amount_converted >= Resource.epsilon)
					{
						ref var material_conv = ref conv.h_material.GetData();
						if (material_conv.IsNotNull())
						{
							var spawn_flags_conv = (spawn_flags | conv.spawn_flags).WithAddRemFlags(ev.spawn_flags);
							Resource.Spawn(region: ref region,
							material: conv.h_material,
							world_position: has_no_offset ? transform.position : ev.world_position,
							amount: has_no_mass_conversion ? amount_converted : Resource.GetConvertedQuantity(resource.material, conv.h_material, amount_converted),
							max_distance: breakable.merge_radius * conv.merge_radius_mult,
							flags: spawn_flags_conv,
							ent_owner: ent_owner,
							angular_velocity: body.GetAngularVelocity(),
							velocity: has_no_offset ? body.GetVelocity() : body.GetVelocity() + (random.NextUnitVector2Range(0, 4) * conv.velocity_mult));
						}
					}

					if (amount_wasted >= Resource.epsilon)
					{
						ref var material_waste = ref conv.h_material_waste.GetData();
						if (material_waste.IsNotNull())
						{
							var spawn_flags_conv = (spawn_flags | conv.spawn_flags_waste).WithAddRemFlags(ev.spawn_flags);
							Resource.Spawn(region: ref region,
							material: conv.h_material_waste,
							world_position: has_no_offset ? transform.position : ev.world_position,
							amount: has_no_mass_conversion ? amount_wasted : Resource.GetConvertedQuantity(resource.material, conv.h_material_waste, amount_wasted),
							max_distance: breakable.merge_radius * conv.merge_radius_mult,
							flags: spawn_flags_conv,
							ent_owner: ent_owner,
							angular_velocity: body.GetAngularVelocity(),
							velocity: has_no_offset ? body.GetVelocity() : body.GetVelocity() + (random.NextUnitVector2Range(0, 4) * conv.velocity_mult));
						}
					}

					if (conv.h_sound)
					{
						Sound.Play(ref region, conv.h_sound, ev.world_position, volume: conv.sound_volume * random.NextFloatExtra(0.90f, 0.20f), pitch: conv.sound_pitch * random.NextFloatExtra(0.95f, 0.10f));
					}

					var amount_taken = amount - amount_rem;
					resource.quantity -= amount_taken;
					resource.Modified(entity, true);

					//ev.flags.AddFlag(Damage.Flags.No_Damage, breakable.flags.HasAny(Breakable.Flags.No_Damage));
				}
#endif

				ev.flags.AddFlag(Damage.Flags.No_Damage, breakable.flags.HasAny(Breakable.Flags.No_Damage));
				ev.knockback *= 0.10f;
			}

			//App.WriteValue(ev.size);

			if (breakable.flags.HasAny(Flags.Splittable) && ev.flags.HasNone(Damage.Flags.No_Damage | Damage.Flags.No_Loot_Drop) && ev.size >= 1.00f && resource.GetQuantityNormalized() >= 0.20f && ev.random.NextBool((ev.size * 0.50f) + Maths.Normalize01Fast(ev.damage_integrity, health.integrity)))
			{
				//var split_amount = resource.TakeSplit(ev.random.NextFloatRange(0.15f, 0.40f));

				//var vel = ev.normal * random.NextFloatExtra(3.00f, 4.00f);
				//var vel = ev.random.NextFloatExtra(5.00f, 4.00f);
				//body.AddVelocity(ev.normal.RotateByRad(random.NextFloatExtra(0.50f, -1.00f)) * random.NextFloatExtra(3.00f, 4.00f));
				//body.AddVelocity((ev.normal) * vel);


#if SERVER
				//var vel = ev.normal * random.NextFloatExtra(3.00f, 4.00f);
				//body.AddVelocity(ev.normal.RotateByRad(random.NextFloatExtra(0.50f, -1.00f)) * random.NextFloatExtra(3.00f, 4.00f));



				//var vel = ev.normal.RotateByRad(random.NextFloat(1)) * random.NextFloatExtra(1.00f, 4.00f);
				var vel = (ev.normal.RotateByRad(random.NextFloat(0.50f))) * random.NextFloatExtra(4.00f, 4.00f);

				var split_amount = resource.TakeSplit(ev.random.NextFloatRange(0.15f, 0.40f));

				resource.Modified(entity, sync: true);
				Resource.Spawn(region: ref region,
				material: resource.material,
				world_position: transform.position + ((body.Up + ev.normal) * 0.50f),
				amount: split_amount,
				max_distance: 0.00f,
				flags: Resource.SpawnFlags.No_Offset,
				//velocity: (ev.normal + body.Right) * vel);
				velocity: vel);

				//App.WriteValue(ev.normal);

				//region.DrawDebugDir(ev.world_position, ev.normal, color: Color32BGRA.Magenta, 4);
#endif
				ev.flags.AddFlag(Damage.Flags.No_Damage, breakable.flags.HasAny(Breakable.Flags.No_Damage));
			}

			//data.flags.AddFlag(Damage.Flags.No_Damage, breakable.flags.HasAny(Breakable.Flags.No_Damage));
		}
	}

	public static partial class Lootable
	{
		public partial struct Item()
		{
			[Flags]
			public enum Flags: byte
			{
				None = 0,
			}

			public IMaterial.Handle material;
			public Chance chance = Chance.Max;
			public Lootable.Item.Flags flags;

			public float min;
			public float max;
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region, sync_table_capacity: 512)]
		public partial struct Data(): IComponent
		{
			public float merge_radius = 2.50f;
			public float spawn_radius = 1.50f;

			[Save.TrimEmpty]
			public FixedArray4<Lootable.Item> items;
		}

#if SERVER
		[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("initialized", true, Source.Modifier.Owned)]
		public static void OnRemove(ref Region.Data region, ref XorRandom random,
		[Source.Owned] ref Lootable.Data lootable,
		[Source.Owned] in Health.Data health, [Source.Owned] in Body.Data body)
		{
			//App.WriteLine("drop loot");
			var yield = Constants.Harvestable.global_yield_modifier * Constants.Materials.global_yield_modifier;
			if (yield > 0.01f && health.integrity <= 0.00f)
			{
				ref var items = ref lootable.items;
				var pos = body.GetPosition();

				for (var i = 0; i < items.Length; i++)
				{
					ref var item = ref items[i];
					if (item.material && (item.chance.m_value == 0 || random.NextBool(item.chance)))
					{
						var amount = random.NextFloatRange(item.min, item.max) * yield;
						if (amount >= Resource.epsilon)
						{
							Resource.Spawn(region: ref region,
								material: item.material,
								world_position: pos, // + random.NextUnitVector2Range(lootable.spawn_radius * 0.50f, lootable.spawn_radius),
								amount: amount,
								max_distance: lootable.merge_radius,
								flags: Resource.SpawnFlags.Merge,
								velocity: (body.GetVelocity() * 0.20f) + random.NextUnitVector2Range(0.50f, 6.50f),
								angular_velocity: body.GetAngularVelocity() + random.NextFloatRange(-2.50f, 2.50f));
						}
						item = default;
					}
				}
			}
		}
#endif
	}

}
