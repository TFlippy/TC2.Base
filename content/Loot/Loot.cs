
namespace TC2.Base.Components
{
	public static partial class Loot
	{
		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Global | IComponent.Scope.Region), ITrait.Data(Net.SendType.Unreliable, IComponent.Scope.Global | IComponent.Scope.Region)]
		public partial struct Container(): IComponent, ITrait
		{
			public struct Entry
			{
				public ILoot.Handle h_loot;
			}

			[Flags]
			public enum Flags: byte
			{
				None = 0,

				[Asset.Ignore] Initialized = 1 << 0,
				No_Duplicates = 1 << 1,
				Shuffle_Inventory = 1 << 2,
			}

	
			//public float rarity_weight = 1.00f;

			[Save.Force] public required float rarity_min = 1.00f;
			[Save.Force] public required float rarity_mid = 3.00f;
			[Save.Force] public required float rarity_max = 10.00f;
			public float rarity_falloff;

			[Save.NewLine]
			[Save.Force] public float weight_multiplier = 1.00f;
			[Save.Force] public float amount_multiplier = 1.00f;
			[Save.Force] public float amount_extra_multiplier = 1.00f;

			[Save.NewLine]
			[Save.Force] public Loot.Container.Flags flags;
			[Save.Force] public ILoot.MergeFlags merge_flags;

			[Save.NewLine]
			[Save.Force] public required byte rolls = 1;
			[Save.Force] public required byte rolls_extra = 2;

			//public static readonly FixedArray4<float> tables_weight_mult_default = new(true, 1.00f);
			//public static readonly FixedArray4<float> tables_amount_mult_default = new(true, 1.00f);

			[Save.NewLine]
			[Save.Force, Save.Inline] public required FixedArray4<ILoot.Handle> tables;
			//[Save.Force, Save.Inline] public FixedArray4<float> tables_weight_mult = tables_weight_mult_default;
			//[Save.Force, Save.Inline] public FixedArray4<float> tables_amount_mult = tables_amount_mult_default;
		}

#if SERVER
		//public static void MergeNodes(WeightedList<ILoot.Node> nodes, HashSet<ILoot.Handle> visited, ILoot.Handle handle, float weight_modifier, float rarity_min, float rarity_max)
		//{
		//	if (handle.id != 0 && !visited.Contains(handle))
		//	{
		//		ref var loot = ref handle.GetData();
		//		if (!loot.IsNull())
		//		{
		//			if (loot.nodes != null)
		//			{
		//				foreach (var node in loot.nodes)
		//				{
		//					if (node.rarity >= rarity_min && node.rarity <= rarity_max)
		//					{
		//						nodes.Add(new(node.weight * weight_modifier, node), false);
		//					}
		//				}
		//			}

		//			if (loot.includes != null)
		//			{
		//				foreach (var include in loot.includes)
		//				{
		//					MergeNodes(nodes, visited, include.handle, weight_modifier * include.weight, rarity_min, rarity_max);
		//				}
		//			}
		//		}
		//	}
		//}

		[ThreadStatic] internal static WeightedList<Shipment.Item2> tls_nodes_item;
		[ThreadStatic] internal static WeightedList<Resource.Data> tls_nodes_resource;
		[ThreadStatic] internal static HashSet<ILoot.Handle> tls_visited;

		[ISystem.AddFirst(ISystem.Mode.Single, ISystem.Scope.Global | ISystem.Scope.Region, order: 500), HasTag("initialized", true, Source.Modifier.Owned)]
		public static void OnAddShipment(Entity entity, ref XorRandom random,
		[Source.Owned] ref Loot.Container container, [Source.Owned] ref Shipment.Data shipment)
		{
			const bool debug_show = false;
			const bool debug_show_items = false;
			const bool debug_show_time = false;

			if (container.flags.TryAddFlag(Container.Flags.Initialized))
			{
				var ts_loot_gen = Timestamp.Now();

				// TODO: shitcode, GC
				var nodes = tls_nodes_item ??= new(16);
				nodes.Clear();

				var visited = tls_visited ??= new(4);
				visited.Clear();

				foreach (var h_loot in container.tables)
				{
					if (h_loot)
					{
						ILoot.MergeItemNodes(random: ref random,
						nodes: nodes,
						visited: visited,
						h_loot: h_loot,
						weight_multiplier: container.weight_multiplier,
						amount_multiplier: container.amount_multiplier,
						rarity_min: container.rarity_min,
						rarity_mid: container.rarity_mid,
						rarity_max: container.rarity_max,
						rarity_falloff: container.rarity_falloff,
						merge_flags: container.merge_flags);

						//MergeNodes(nodes, visited, container.tables[i], 1.00f, container.rarity_min, container.rarity_max);
					}
				}

				if ((uint)nodes.Count != 0)
				{
					//App.WriteValue(nodes.WeightSum);
					//App.WriteLine(nodes.AsSpan().ToArrayString());

					if (debug_show_items) App.WriteLine(nodes.AsSpan().ToArrayString());

					nodes.Rebuild();

					//App.WriteValue(nodes.WeightSum);

					//App.WriteLine(nodes.ToFormattedString());




					var rolls_count = random.NextUIntExtra(container.rolls, container.rolls_extra);
					//App.WriteValue(rolls_count);

					var items_list = stackalloc Shipment.Item2[(int)rolls_count].AsSpanList();
					for (var i = 0u; i < rolls_count; i++)
					{
						var item = nodes.GetRandom(random: ref random, remove: container.flags.HasAny(Container.Flags.No_Duplicates));
						if (debug_show_items) App.WriteValue(item, color: App.Color.Green);
						items_list.TryMergeAdd(item);

						if ((uint)nodes.Count == 0) break;
					}

					//App.WriteLine(items_list.GetSpan().ToArrayString());

					if (Assert.Check(items_list.Count != 0, message: "No loot generated!", level: Assert.Level.Warn))
					{
						var shipment_items_span = shipment.items.AsSpan();
						for (var i = 0u; i < items_list.Count; i++)
						{
							if (shipment_items_span.TryMergeAdd(items_list[i]))
							{

							}
						}

						shipment.flags.RemoveFlag(Shipment.Flags.Cached);
						shipment.Modified(entity, sync: true);
					}

					//App.WriteTimestamp(ts_loot_gen); //,  $"loot in {ts.GetMilliseconds():0.000} ms");
				}

				if (debug_show_time) App.WriteTimestamp(ts_loot_gen, "Loot Generator (Shipment)");
			}
		}

		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Global | ISystem.Scope.Region, order: 500), HasTag("initialized", true, Source.Modifier.Owned)]
		public static void OnAddStorage(Entity entity, ref XorRandom random,
		[Source.Owned, Pair.Component<Storage.Data>] ref Loot.Container container, [Source.Owned] ref Storage.Data storage)
		{
			const bool debug_show = false;
			const bool debug_show_items = false;
			const bool debug_show_time = false;
			const bool debug_show_asserts = false;

			//var inventories = entity.GetInventories2();
			//if (storage.inv_storage.TryGetHandle(out var h_inventory) && container.flags.TryAddFlag(Container.Flags.Initialized))
			if (container.flags.TryAddFlag(Container.Flags.Initialized))
			{
				var ts_loot_gen = Timestamp.Now();

				//// TODO: shitcode, GC
				//var nodes = new WeightedList<Resource.Data>(16);
				//var visited = new HashSet<ILoot.Handle>(4);

				// TODO: shitcode, GC
				var nodes = tls_nodes_resource ??= new(16);
				nodes.Clear();

				var visited = tls_visited ??= new(4);
				visited.Clear();

				foreach (var h_loot in container.tables)
				{
					if (h_loot)
					{
						//var weight_mult = container.tables_weight_mult[]

						ILoot.MergeResourceNodes(random: ref random,
						nodes: nodes,
						visited: visited,
						h_loot: h_loot,
						weight_multiplier: container.weight_multiplier,
						amount_multiplier: container.amount_multiplier,
						amount_extra_multiplier: container.amount_extra_multiplier,
						rarity_min: container.rarity_min,
						rarity_mid: container.rarity_mid,
						rarity_max: container.rarity_max,
						rarity_falloff: container.rarity_falloff,
						merge_flags: container.merge_flags);
						//MergeNodes(nodes, visited, container.tables[i], 1.00f, container.rarity_min, container.rarity_max);
					}
				}

				if ((uint)nodes.Count != 0)
				{
					if (debug_show_items) App.WriteLine(nodes.AsSpan().ToArrayString());

					nodes.Rebuild();

					if (debug_show_items) App.WriteValue(nodes.WeightSum);

					//App.WriteLine(nodes.AsSpan().ToArrayString());

					var rolls_count = random.NextUIntExtra(container.rolls, container.rolls_extra);

					var items_list = stackalloc Shipment.Item2[(int)rolls_count].AsSpanList();
					for (var i = 0u; i < rolls_count; i++)
					{
						var item = nodes.GetRandom(random: ref random, remove: container.flags.HasAny(Container.Flags.No_Duplicates));
						if (debug_show_items) App.WriteValue(item, color: App.Color.Green);

						items_list.TryMergeAdd(item);

						if ((uint)nodes.Count == 0) break;
					}

					if (debug_show) App.WriteLine(items_list.GetSpan().ToArrayString());

					if (Assert.Check(items_list.Count != 0, message: "No loot generated!", level: Assert.Level.Warn))
					{
						var inventories = entity.GetInventoryList();
						//inventories.

						foreach (var h_inventory in inventories)
						{
							var inventory_dirty = false;

							{
								var i = 0u;
								if (debug_show) App.WriteValue((i, items_list.Count, h_inventory.Type, h_inventory.Length));
								
								while (i < items_list.Count)
								{
									ref var item = ref items_list[i];
									inventory_dirty |= h_inventory.Deposit(resource: ref item.AsResource(), amount: item.quantity, done: out var remove);
									if (remove)
									{
										items_list.RemoveAtSwapback(i, clear: false);
									}
									else
									{
										i++;
									}
								}
							}

							if (inventory_dirty)
							{
								if (container.flags.HasAny(Container.Flags.Shuffle_Inventory))
								{
									var inventory_items = h_inventory.GetSpan();
									inventory_items.Shuffle(ref random);
								}
								//var n = inventory_items.Length;
								//for (var i = 0; i < n - 1; i++)
								//{
								//	int j = random.NextIntRange(i, n);
								//	if (j != i)
								//	{
								//		var tmp = inventory_items[i];
								//		inventory_items[i] = inventory_items[j];
								//		inventory_items[j] = tmp;
								//	}
								//}

								h_inventory.Sync();
							}
							if (items_list.IsEmpty) break;
						}

						if (debug_show_asserts) Assert.Check(items_list.Count == 0, message: "Not enough space for all of loot!", level: Assert.Level.Warn);
					}
				}
				//var ts_elapsed = ts.GetMilliseconds();
				if (debug_show_time) App.WriteTimestamp(ts_loot_gen, "Loot Generator (Inventory)");
			}
		}
#endif
	}
}
