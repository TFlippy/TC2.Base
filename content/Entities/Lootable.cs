
namespace TC2.Base.Components
{
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

		[IComponent.Data(Net.SendType.Unreliable, sync_table_capacity: 512)]
		public partial struct Data: IComponent
		{
			public float merge_radius = 2.00f;
			public float spawn_radius = 0.00f;

			[Save.TrimEmpty]
			public FixedArray8<Lootable.Item> items = default;

			public Data()
			{

			}
		}

#if SERVER
		[ISystem.RemoveLast(ISystem.Mode.Single)]
		public static void OnRemove(ref Region.Data region, ISystem.Info info, Entity entity, ref XorRandom random, [Source.Owned] ref Lootable.Data lootable, [Source.Owned] in Health.Data health, [Source.Owned] in Transform.Data transform)
		{
			//App.WriteLine("drop loot");
			var yield = Constants.Harvestable.global_yield_modifier * Constants.Materials.global_yield_modifier * health.yield;
			if (yield > 0.01f && health.integrity <= 0.00f)
			{
				ref var items = ref lootable.items;

				for (var i = 0; i < items.Length; i++)
				{
					ref var item = ref items[i];
					if (item.material.id != 0)
					{
						var amount = random.NextFloatRange(item.min, item.max) * yield;
						if (amount > Resource.epsilon)
						{
							Resource.Spawn(ref region, item.material, transform.position + random.NextUnitVector2Range(0.00f, lootable.spawn_radius), amount, lootable.merge_radius, lootable.merge_radius > 0.00f);
							item = default;
						}
					}
				}
			}
		}
#endif
	}

}
