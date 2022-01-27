using Keg.Engine;

namespace TC2.Base.Components
{
	public static partial class Supplier
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data: IComponent
		{
			public float interval = 10.00f;
			public float amount_multiplier = 1.00f;
			public int shipment_counter;

			[Save.Ignore] public float next_update;
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public struct State: IComponent
		{
			public float work_current;
			public float work_required = 100.00f;
		}

		public struct Product
		{
			public Shipment.Item item;
			public float step;

			public Product(Shipment.Item item, float step = 1.00f)
			{
				this.item = item;
				this.step = step;
			}
		}

		public static readonly WeightedList<Supplier.Product> products = new()
		{
			new(60.00f, new(Shipment.Item.Resource("wood", 100.00f, 700.00f), step: 50.00f)),
			new(40.00f, new(Shipment.Item.Resource("iron_plate", 10.00f, 20.00f), step: 10.00f))
		};

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void OnLateUpdate(ISystem.Info info, Entity entity,
		[Source.Owned] ref Supplier.Data supplier, [Source.Owned] ref Supplier.State supplier_state, [Source.Owned] ref Stockpile.Data stockpile)
		{
			if (info.WorldTime > supplier.next_update)
			{
				supplier.next_update = info.WorldTime + supplier.interval;
				supplier_state.work_current += 1.00f * supplier.interval;

				if (supplier_state.work_current >= supplier_state.work_required)
				{
					supplier_state.work_current = 0.00f;
					ref var shipment = ref stockpile.shipments[Maths.Repeat(supplier.shipment_counter, stockpile.shipments.Length)];

					shipment = new();
					shipment.name = $"Goods #{supplier.shipment_counter + 1} ({entity.GetName()})";

					var items = shipment.items.AsSpan();

					var random = XorRandom.New();

					for (int i = 0; i < 4; i++)
					{
						var product = products.GetRandom(ref random);
						var item = product.item;
						item.quantity = (random.NextFloatRange(item.quantity, item.max) * supplier.amount_multiplier).ToNearestMultiple(product.step);

						items.Add(item);
					}

					items.Sort((x, y) => -(x.quantity.CompareTo(y.quantity)));

					shipment.mass = items.CalculateMass();

					supplier.shipment_counter++;
					stockpile.Sync(entity);
				}

				supplier_state.Sync(entity);
			}
		}
#endif
	}
}