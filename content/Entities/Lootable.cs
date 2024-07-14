
namespace TC2.Base.Components
{
	public static class Breakable
	{
		internal const bool debug_log = false;

		[Flags]
		public enum Flags: ushort
		{
			None = 0,

			No_Damage = 1 << 0
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true, sync_table_capacity: 256)]
		public struct Data: IComponent
		{
			[Save.Force] public IMaterial.Handle h_material;

			[Save.NewLine]
			[Save.Force] public Breakable.Flags flags;
			[Save.Force] public IMaterial.Conversion.Type conversion_type;
			[Save.Force] public IMaterial.Conversion.Flags conversion_flags;
			[Save.Force] public Resource.SpawnFlags spawn_flags;

			[Save.NewLine]
			[Save.Force] public Material.Type material_type;
			[Save.Force] public Material.Flags material_flags;

			public Data()
			{

			}
		}

		[ISystem.Event<Health.DamageEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnDamage(ref Region.Data region, ISystem.Info info, Entity entity, ref Health.DamageEvent data, ref XorRandom random,
		[Source.Owned] in Health.Data health, [Source.Owned] ref Resource.Data resource, [Source.Owned] ref Breakable.Data breakable, [Source.Owned] in Transform.Data transform, [Source.Owned] in Body.Data body)
		{
			if (data.flags.HasAny(Damage.Flags.No_Loot_Drop | Damage.Flags.No_Damage)) return;

#if SERVER
			var yield = data.yield;
			if (yield >= 0.01f)
			{
				var damage = data.damage_integrity;
				var amount_multiplier = damage * Maths.Clamp01(health.GetMaxHealthInv()) * yield;

				var ent_attacker = data.ent_attacker;
				var ent_owner = data.ent_owner;

				var spawn_flags = breakable.spawn_flags;

				ref var material = ref resource.material.GetData();
				if (material.IsNotNull())
				{
					var amount = Maths.Min(resource.quantity, MathF.Ceiling(resource.quantity * amount_multiplier));
					if (amount >= 1.00f)
					{
						if (material.conversions != null && material.conversions.TryGetValue(data.damage_type, out var conv))
						{
							if (random.NextBool(conv.chance))
							{
								ref var material_conv = ref conv.h_material.GetData();
								if (material_conv.IsNotNull())
								{
									var conv_ratio = random.NextFloatExtra(conv.ratio, conv.ratio_extra);
									var amount_converted = amount *= conv_ratio;

									amount -= amount_converted;
									amount *= conv.yield;

									resource.quantity -= amount_converted;
									var amount_converted_corrected = amount_converted * (material.mass_per_unit / material_conv.mass_per_unit);

									var spawn_flags_conv = spawn_flags | conv.spawn_flags;
									Resource.Spawn(ref region, conv.h_material, data.world_position, amount_converted_corrected, 4.00f, flags: spawn_flags_conv, ent_target: ent_attacker, ent_owner: ent_owner,
									angular_velocity: body.GetAngularVelocity(), velocity: body.GetVelocity() + random.NextUnitVector2Range(0, 3));

									resource.Modified(entity, true);
								}
							}
						}
					}
				}
			}
#endif

			data.flags.AddFlag(Damage.Flags.No_Damage, breakable.flags.HasAny(Breakable.Flags.No_Damage));
		}
	}

	public static partial class Lootable
	{
		[Serializable]
		public partial struct Item
		{
			public IMaterial.Handle material;

			public float min;
			public float max;
		}

		[Flags]
		public enum Flags: uint
		{
			None = 0,
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true, sync_table_capacity: 512)]
		public partial struct Data: IComponent
		{
			public float merge_radius = 2.00f;
			public float spawn_radius = 0.00f;

			[Save.TrimEmpty]
			public FixedArray8<Lootable.Item> items;

			public Data()
			{

			}
		}

#if SERVER
		[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnRemove(ref Region.Data region, ISystem.Info info, Entity entity, ref XorRandom random, [Source.Owned] ref Lootable.Data lootable, [Source.Owned] in Health.Data health, [Source.Owned] in Transform.Data transform, [Source.Owned] in Body.Data body)
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
					if (item.material.id != 0)
					{
						var amount = random.NextFloatRange(item.min, item.max) * yield;
						if (amount >= Resource.epsilon)
						{
							Resource.Spawn(ref region, item.material, pos + random.NextUnitVector2Range(0.00f, lootable.spawn_radius), amount, lootable.merge_radius, flags: Resource.SpawnFlags.Merge, velocity: (body.GetVelocity() * 0.20f) + random.NextUnitVector2Range(0.50f, 6.50f), angular_velocity: body.GetAngularVelocity() + random.NextFloatRange(-2.50f, 2.50f));
						}
						item = default;
					}
				}
			}
		}
#endif
	}

}
