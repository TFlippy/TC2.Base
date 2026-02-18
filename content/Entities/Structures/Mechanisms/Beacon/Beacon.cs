
namespace TC2.Base.Components
{
	public static partial class Beacon
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0u,

			Pending = 1u << 0
		}

		public enum Type: byte
		{
			Undefined = 0,

			Trade,
			Cargo,
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Global | IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			public Beacon.Type type;
			public byte unused_00;
			public IWarrant.Handle h_warrant;

			public Beacon.Flags flags;

			public float strength;
			public float price_estimate;
		}

		public struct EditRPC: Net.IRPC<Beacon.Data>
		{
			public Beacon.Type? type;
			public Beacon.Flags? flags;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Beacon.Data data)
			{
				var sync = false;

				if (sync)
				{
					data.Sync(rpc.entity, true);
				}
			}
#endif
		}

		public struct ActionRPC: Net.IRPC<Beacon.Data>
		{


#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Beacon.Data data)
			{
				var sync = false;

				if (sync)
				{
					data.Sync(rpc.entity, true);
				}
			}
#endif
		}

#if CLIENT
		public partial struct BeaconGUI: IGUICommand
		{
			public Entity ent_beacon;
			public Beacon.Data beacon;
			public Transform.Data transform;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Beacon"u8, this.ent_beacon))
				{
					this.StoreCurrentWindowTypeID(order: 7);
					if (window.show)
					{
						var ent_attached = this.ent_beacon.GetParent<Sticky.Rel>();

						using (var group_icon = GUI.Group.New(size: new(192, 128)))
						{
							group_icon.DrawBackground(GUI.tex_window);
							GUI.DrawEntityIcon(ent_attached, scale: 4);
						}

						GUI.SameLine();

						using (var group_info = GUI.Group.New(size: new(GUI.RmX, 128), padding: new(4)))
						{
							//GUI.TitleCentered(ent_attached.GetName(), pivot: new(0.00f, 0.00f), size: 20);
							GUI.Title(ent_attached.GetName(), size: 20);

							var price_estimate = 0.00f;

							if (ent_attached.IsAlive())
							{
								foreach (var h_inventory in ent_attached.GetInventories())
								{
									GUI.Title(h_inventory.Type.ToStringUtf8());
									var span_items = h_inventory.GetReadOnlySpan();
									foreach (var item in span_items)
									{
										if (item)
										{
											ref var material = ref item.GetMaterial();
											if (material.IsNotNull())
											{
												ref var commodity = ref material.commodity.GetRefOrNull();
												if (commodity.IsNotNull())
												{
													var item_price = commodity.market_price * item.quantity;
													price_estimate += item_price;
													GUI.LabelShaded(item.GetShortName(), item_price, format: "0' Đk'");
												}
												else
												{
													GUI.LabelShaded(item.GetShortName(), "N/A"u8, color_b: GUI.font_color_desc);
													//GUI.TextShaded(item.GetShortName(), color: GUI.font_color_default.WithAlpha((byte)(commodity.IsNotNull() ? 255 : 100)));
												}
											}
										}
									}
								}
							}

							GUI.Separator(spacing: 4);
							GUI.LabelShaded("Est.Value"u8, price_estimate, format: "0' Đk'");
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI([Source.Owned] in Interactable.Data interactable,
		Entity ent_beacon, [Source.Owned] in Beacon.Data beacon, [Source.Owned] in Transform.Data transform)
		{
			if (interactable.IsActive())
			{
				var gui = new BeaconGUI()
				{
					ent_beacon = ent_beacon,
					beacon = beacon,
					transform = transform,
				};
				gui.Submit();
			}
		}
#endif

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Beacon.Data beacon, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body)
		{

		}
	}
}
