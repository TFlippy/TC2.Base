
namespace TC2.Base.Components
{
	public static partial class Vendor
	{
		[Flags]
		public enum Flags: ushort
		{
			None = 0,

		}

		public enum Type: byte
		{
			Undefined = 0,

			Vending_Machine,
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Global | IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			public Vendor.Type type;
			public byte unused_00;
			public Vendor.Flags flags;

			[Save.NewLine]
			public IStore.Handle h_store;
			public IStockpile.Handle h_stockpile;
			public IWarrant.Handle h_warrant_current;
			public ushort unused_01;
			public uint unused_02;

			[Save.NewLine]
			[Asset.Ignore] public FixedArray16<Shipment.Item2> edit_selected_items;
		}

		public struct EditOrderRPC: Net.IRPC<Vendor.Data>
		{
			public Shipment.Item2? item;
			public IStockpile.Handle? h_stockpile;
			public IStore.Handle? h_store;
			public byte? index;

			//public Vendor.Type? type;
			//public Vendor.Flags? flags;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Vendor.Data data)
			{
				var sync = false;

				//sync |= data.flags.TrySetFlagMasked(this.flags, Vendor.flags_editable_mask);

				if (sync)
				{
					data.Sync(rpc.entity, true);
				}
			}
#endif
		}

		public struct DEV_TestRPC: Net.IRPC<Vendor.Data>
		{


#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Vendor.Data data)
			{
				Assert.IsDevMode();
				Assert.IsAdmin(ref rpc.connection);

				var sync = false;

				if (sync)
				{
					data.Sync(rpc.entity, true);
				}
			}
#endif
		}

#if CLIENT
		public partial struct VendorGUI: IGUICommand
		{
			public Entity ent_vendor;
			public Vendor.Data vendor;
			public Transform.Data transform;
			public Inventory1.Data inventory_money;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Vending Machine"u8, this.ent_vendor, tooltip_tab: "Various goods can be ordered here."))
				{
					this.StoreCurrentWindowTypeID(order: -10);
					if (window.show)
					{
						ref var region_common = ref this.ent_vendor.GetRegionCommon();
						var pos = this.transform.position;

						using (var group_left = GUI.Group.New(size: new(192, GUI.RmY)))
						{
							using (var group_icon = GUI.Group.New(size: new(GUI.RmX, 128)))
							{
								group_icon.DrawBackground(GUI.tex_window);

								var h_store_edit = this.vendor.h_store;
								if (GUI.AssetInput2("edit.store"u8, ref h_store_edit, size: new(GUI.RmX, 32), show_label: false, show_null: true))
								{

								}

								//GUI.DrawEntityIcon(ent_attached, scale: 4);
							}
						}

						GUI.SameLine();

						using (var group_right = GUI.Group.New(size: GUI.Rm))
						{
							using (var group_info = GUI.Group.New(size: GUI.Rm.SubY(40), padding: new(10)))
							{
								group_info.DrawBackground(GUI.tex_window);

								//GUI.TitleCentered(ent_attached.GetName(), pivot: new(0.00f, 0.00f), size: 20);
								//GUI.Title(ent_attached.GetName(), size: 20);

								var price_estimate = 0.00f;
								var total_mass = (Mass)0.00f;

								//total_mass += this.ent_vendor.

								//using (var group_items = GUI.Group.New(size: GUI.Rm.SubY((14 * 6) + 4 + 4)))
								using (var group_items = GUI.Scrollbox.New("sb.items"u8, size: GUI.Rm.SubY((14 * 6) + 4 + 4), padding: new(4)))
								{
									//if (ent_attached.IsAlive())
									//{
									//	ref var body = ref ent_attached.GetComponent<Body.Data>();
									//	if (body.IsNotNull())
									//	{
									//		total_mass += body.GetMass() - (body.inventory_weight * body.inventory_weight_multiplier);
									//		pos = body.GetPosition();
									//	}

									//	foreach (var h_inventory in ent_attached.GetInventories())
									//	{
									//		//GUI.Title(h_inventory.Type.ToStringUtf8());
									//		var span_items = h_inventory.GetReadOnlySpan();
									//		foreach (var item in span_items)
									//		{
									//			if (item)
									//			{
									//				ref var material = ref item.GetMaterial();
									//				if (material.IsNotNull())
									//				{
									//					total_mass += item.GetMass();

									//					ref var commodity = ref material.commodity.GetRefOrNull();
									//					if (commodity.IsNotNull())
									//					{
									//						var item_price = commodity.market_price * item.quantity;
									//						price_estimate += item_price;

									//						GUI.TextShaded(item.quantity, format: "0'x'");
									//						GUI.OffsetLine(48);
									//						GUI.LabelShaded(item.GetName(), item_price, format: "0' Đk'");
									//						//GUI.SameLine(8);
									//						//GUI.TextShaded(item.GetMass(), format: "0' kg'", size: 12);

									//						//if (GUI.IsItemHovered())
									//						//{
									//						//	using (var tooltip = GUI.Tooltip.New())
									//						//	{

									//						//	}
									//						//}
									//					}
									//					else
									//					{
									//						GUI.TextShaded(item.quantity, format: "0'x'", color: GUI.font_color_desc);
									//						GUI.OffsetLine(48);
									//						GUI.LabelShaded(item.GetName(), "N/A"u8, color_a: GUI.font_color_desc, color_b: GUI.font_color_desc);
									//						//GUI.TextShaded(item.GetShortName(), color: GUI.font_color_default.WithAlpha((byte)(commodity.IsNotNull() ? 255 : 100)));
									//					}

									//					if (GUI.IsItemHovered())
									//					{
									//						using (var tooltip = GUI.Tooltip.New(size: new(192, 0)))
									//						{
									//							GUI.LabelShaded("Mass"u8, item.GetMass(), format: "0.##' kg'");
									//						}
									//					}

									//					GUI.FocusableAsset(item.material);
									//				}
									//			}
									//		}
									//	}

									//	ref var harvestable = ref ent_attached.GetComponent<Harvestable.Data>();
									//	if (harvestable.IsNotNull())
									//	{
									//		ref var harvestable_state = ref ent_attached.GetComponent<Harvestable.State>();
									//		if (harvestable_state.IsNotNull())
									//		{
									//			GUI.NewLine(2);
									//			GUI.Separator(spacing: 4, thickness: 1.00f);

									//			GUI.Title("Harvestable"u8, color: GUI.font_color_yellow_b);
									//			var items = harvestable.resources;
									//			for (var i = 0; i < items.Length; i++)
									//			{
									//				var item = items[i];
									//				if (item)
									//				{
									//					item.quantity *= (1.00f - harvestable_state.pct_spawned[i].Value);

									//					ref var material = ref item.GetMaterial();
									//					if (material.IsNotNull())
									//					{
									//						//total_mass += item.GetMass();

									//						ref var commodity = ref material.commodity.GetRefOrNull();
									//						if (commodity.IsNotNull())
									//						{
									//							var item_price = commodity.market_price * item.quantity;
									//							price_estimate += item_price;

									//							GUI.TextShaded(item.quantity, format: "'~'0'x'", color: GUI.font_color_yellow_b);
									//							GUI.OffsetLine(48);
									//							GUI.LabelShaded(item.GetName(), item_price, format: "0' Đk'", color_a: GUI.font_color_yellow_b, color_b: GUI.font_color_yellow_b);

									//							//if (GUI.IsItemHovered())
									//							//{
									//							//	using (var tooltip = GUI.Tooltip.New())
									//							//	{

									//							//	}
									//							//}
									//						}
									//						else
									//						{
									//							GUI.TextShaded(item.quantity, format: "'~'0'x'", color: GUI.font_color_yellow_b);
									//							GUI.OffsetLine(48);
									//							GUI.LabelShaded(item.GetName(), "N/A"u8, color_a: GUI.font_color_desc, color_b: GUI.font_color_desc);
									//							//GUI.TextShaded(item.GetShortName(), color: GUI.font_color_default.WithAlpha((byte)(commodity.IsNotNull() ? 255 : 100)));
									//						}

									//						if (GUI.IsItemHovered())
									//						{
									//							using (var tooltip = GUI.Tooltip.New(size: new(192, 0)))
									//							{
									//								GUI.LabelShaded("Mass"u8, item.GetMass(), format: "0.##' kg'");
									//							}
									//						}

									//						GUI.FocusableAsset(item.material);
									//					}
									//				}
									//			}
									//		}
									//	}
									//}

									//GUI.NewLine(2);
									//GUI.Separator(spacing: 4, thickness: 1.00f);
								}

								GUI.SeparatorThick();
								GUI.NewLine(2);

								//GUI.Separator(spacing: 4);

								using (var group_total = GUI.Group.New(size: GUI.Rm))
								{
									//var fee_mass = total_mass.m_value * this.vendor.fee_per_kg;
									//var fee_base = price_estimate * this.vendor.fee_base;

									//var price_w_fees = price_estimate - fee_base - fee_mass;
									//var tax = Maths.Abs(price_w_fees) * 0.21f;
									//var price_w_tax = price_w_fees - tax;

									//GUI.LabelShaded("Value"u8, price_estimate, format: "0' Đk'");
									//GUI.LabelShaded("Fee (Base)"u8, -fee_base, format: "0' Đk'", color_b: GUI.font_color_red_b);
									//GUI.LabelShaded("Fee (Mass)"u8, -fee_mass, format: "0' Đk'", color_b: GUI.font_color_red_b);
									//GUI.LabelShaded("VAT"u8, -tax, format: "0' Đk'", color_b: GUI.font_color_red_b);
									//GUI.Separator(spacing: 4, thickness: 1.00f);
									//GUI.LabelShaded("Reward"u8, price_w_tax, format: "0' Đk'", color_b: price_w_tax < 0.00f ? GUI.font_color_red_b : GUI.font_color_green_b);
									//GUI.LabelShaded("Mass"u8, total_mass, format: "0.##' kg'");
								}
							}

							var is_los_sky = true;

							ref var region = ref region_common.AsRegion();
							if (region.IsNotNull())
							{
								//var ts = Timestamp.Now();
								is_los_sky = region.IsLOS(pos, pos with { Y = 0 }, threshold_solid: 0.00f, allow_inside: false);
								if (is_los_sky)
								{
									//region.TryOverlapBB()
									is_los_sky &= region.IsInLineOfSight(pos, pos with { Y = 0 }, radius: 3.00f, layer: Physics.Layer.Solid, mask: Physics.Layer.None, exclude: Physics.Layer.World | Physics.Layer.Bounds | Physics.Layer.Water | Physics.Layer.Gas | Physics.Layer.Liquid | Physics.Layer.Fire | Physics.Layer.Essence, skip_inside: false);
								}
								//var ts_elapsed_ms = ts.GetMilliseconds();
								//GUI.Text($"{ts_elapsed_ms:0.0000} ms");

								//GUI.DrawLine(region.WorldToCanvas(pos), region.WorldToCanvas(pos with { Y = 0 }), color: is_los_sky.GetColor32(), thickness: region.GetWorldToCanvasScale(), layer: GUI.Layer.Foreground);

							}

							using (var group_buttons = GUI.Group.New(size: GUI.Rm))
							{
								//group_buttons.DrawBackground(GUI.tex_window);

								//if (this.vendor.flags.HasAny(Vendor.Flags.Active))
								//{
								//	if (GUI.DrawButton("Deactivate"u8, size: new(96, GUI.RmY), color: GUI.col_button_error))
								//	{
								//		var rpc = new Vendor.EditRPC
								//		{
								//			flags = this.vendor.flags.WithRemoved(Vendor.Flags.Active)
								//		};
								//		rpc.Send(this.ent_vendor);
								//	}
								//}
								//else
								//{
								//	if (GUI.DrawButton("Activate"u8, size: new(96, GUI.RmY), color: GUI.col_button_ok, enabled: is_los_sky && ent_attached.IsAlive()))
								//	{
								//		var rpc = new Vendor.EditRPC
								//		{
								//			flags = this.vendor.flags.WithAdded(Vendor.Flags.Active)
								//		};
								//		rpc.Send(this.ent_vendor);
								//	}
								//	GUI.DrawHoverTooltip("Activate the vendor, allowing zeppelins to pick up this object."u8);
								//}

								//if (GUI.ShowDebugGUI)
								//{
								//	GUI.SameLine();

								//	if (GUI.DrawButton("DEV: Sell"u8, size: new(96, GUI.RmY), color: GUI.col_button_debug, enabled: is_los_sky && ent_attached.IsAlive()))
								//	{
								//		var rpc = new Vendor.DEV_SellRPC
								//		{
								//		};
								//		rpc.Send(this.ent_vendor);
								//	}
								//}
							}
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI([Source.Owned] in Interactable.Data interactable,
		Entity ent_vendor, [Source.Owned] in Vendor.Data vendor, [Source.Owned] in Transform.Data transform, 
		[Source.Owned, Pair.Component<Vendor.Data>] in Inventory1.Data inventory_money)
		{
			if (interactable.IsActive())
			{
				var gui = new VendorGUI()
				{
					ent_vendor = ent_vendor,
					vendor = vendor,
					transform = transform,
					inventory_money = inventory_money
				};
				gui.Submit();
			}
		}
#endif

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Vendor.Data vendor, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body)
		{
			//if (vendor.flags.HasAny(Flags.Active))
			//{
			//	if (body.type != Body.Type.Kinematic)
			//	{
			//		body.type = Body.Type.Kinematic;
			//		body.MarkDirty();
			//	}
			//	else
			//	{
			//		body.SetVelocity(new(0.00f, -2.50f));
			//	}
			//}
		}

#if CLIENT
		[ISystem.Render(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnRender(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Vendor.Data vendor, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body)
		{
			//App.WriteLine("draw");
			//if (vendor.flags.HasAny(Flags.Active))
			//{
			//	var pos_a = Vector2.Zero; // transform.WorldToLocal(transform.position);
			//	var pos_b = transform.WorldToLocal(transform.position.WithY(0.00f));

			//	Rope.Draw(transform, new()
			//	{
			//		thickness = 0.60f,
			//		texture = "skyhook.00.cable",
			//		scale = Vec2f.Distance(pos_a, pos_b) * 0.125f,
			//		z = -75.000f,
			//		//transform_index = 0,

			//		p0 = pos_a,
			//		p1 = pos_a,
			//		p2 = pos_b,
			//		p3 = pos_b,
			//	});
			//}
		}
#endif
	}
}
