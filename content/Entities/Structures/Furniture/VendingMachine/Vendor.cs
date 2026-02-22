
namespace TC2.Base.Components
{
	[Asset.Hjson(prefix: "catalogue.", capacity_world: 512, capacity_region: 16, capacity_local: 8, flags_world: Asset.Flags.Recycle, flags_local: Asset.Flags.Recycle)]
	public interface ICatalogue: IAsset2<ICatalogue, ICatalogue.Data>
	{
		[Net.MsgPack]
		public struct Data(): IName
		{
			[Net.Key(00), Save.Force] public string name;

			//[Net.Key(00)]
			[Save.NewLine]
			[Net.Key(02), Save.Force] public ICompany.Handle h_company;

			[Save.NewLine]
			[Net.Key(04), Save.Force] public ImperialDateTime date_created;

			[Save.NewLine]
			[Net.Key(05), Save.Force, Save.Inline] public Shipment.Item2[] items;
			//[Net.Key(06), Save.Force, Save.Inline] public Shipment.Item2[] items_trading;

			readonly ReadOnlySpan<char> IName.GetName() => this.name;
			readonly ReadOnlySpan<char> IName.GetShortName() => this.name;
		}
	}

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
			[Save.Force] public required Vendor.Type type;
			[Save.Force] public required byte item_count_max = 8;
			[Save.Force] public Vendor.Flags flags;

			[Save.NewLine]
			[Asset.Ignore] public IStore.Handle h_store;
			//public IStockpile.Handle h_stockpile;
			[Asset.Ignore] public IWarrant.Handle h_warrant_current;
			public ICatalogue.Handle h_catalogue;
			public ushort unused_01;

			public ISoundMix.Handle h_soundmix_signal;
			public ISoundMix.Handle h_soundmix_order;

			[Save.NewLine]
			[Net.Segment.C] public float fee_base;
			[Net.Segment.C] public float fee_per_kg;
			[Net.Segment.C] public float unused_03;
			[Net.Segment.C, Save.Force] public required float weight_max = 500.00f;

			[Save.NewLine]
			[Net.Segment.D, Asset.Ignore] public FixedArray16<Shipment.Item2> edit_selected_items;
		}

		public struct EditOrderRPC: Net.IRPC<Vendor.Data>
		{
			public Shipment.Item2? item;
			public IStockpile.Handle? h_stockpile;
			public IStore.Handle? h_store;
			public ICatalogue.Handle? h_catalogue;
			public byte? index;

			//public Vendor.Type? type;
			//public Vendor.Flags? flags;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Vendor.Data data)
			{
				var sync = false;

				sync |= data.h_store.TrySet(this.h_store);

				if (this.item.TryGetValueUnsafe(out var item_tmp))
				{
					if (item_tmp.quantity <= 0.00f)
					{
						sync |= data.edit_selected_items.AsSpan(data.item_count_max).TryRemove(item_tmp); // .TryRemove(in item_tmp);
					}
					else
					{
						sync |= data.edit_selected_items.AsSpan(data.item_count_max).TryAddOrSet(item_tmp);
					}

					data.edit_selected_items.Compact();

					//var item_index = data.edit_selected_items.indexof(this.item.GetValueOrDefault());
				}

				if (sync)
				{
					data.Sync(rpc.entity, true);
				}
			}
#endif
		}

		public struct SubmitRPC: Net.IRPC<Vendor.Data>
		{


#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Vendor.Data data)
			{
				//Assert.IsDevMode();
				//Assert.IsAdmin(ref rpc.connection);

				ref var region_common = ref rpc.GetRegionCommon();

				var sync = false;

				ref var inventory_money = ref rpc.entity.GetTrait<Vendor.Data, Inventory1.Data>();
				Assert.IsNotNull(ref inventory_money);

				ref var resource_money = ref inventory_money.resource;
				var credit = resource_money.GetMarketPrice();

				var span_items = data.edit_selected_items.AsSpan(data.item_count_max);
				//var total_cost = span_items.GetMarketPrice();
				//Assert.Check(total_cost >= 1.00f);

				//var has_enough_money = credit >= total_cost;

				//App.WriteValue((inventory_money.resource, credit, total_cost, has_enough_money));

				//if (has_enough_money)
				{
					//var ok = inventory_money.Remove(resource_money.material, total_cost);
					//if (Assert.IsTrue(ok))
					{

						//Crafting.Product


						var total_cost = 0.00f;
						var total_mass = (Mass)0.00f;

						var span_products = FixedArray.CreateSpanList8<Crafting.Product>(out var buffer_products);
						foreach (ref var item in span_items)
						{
							if (item.header == 0) continue;

							var item_price = item.GetMarketPrice();
							Assert.Check(item_price > 0.00f);

							total_cost += item_price;
							total_mass += item.GetMass();

							switch (item.type)
							{
								case Shipment.Item.Type.Resource:
								{
									span_products.Add(Crafting.Product.Resource(item.h_material, item.quantity));
								}
								break;

								case Shipment.Item.Type.Prefab:
								{
									span_products.Add(Crafting.Product.Prefab(item.h_prefab, (int)item.quantity));
								}
								break;

								default:
								{
									Assert.Throw("Cannot spawn vendor item!", item, level: Assert.Level.Warn);
								}
								break;
							}
							//span_products.Add(item);
						}

						//var fee_vat = total_cost * 0.21f;

						total_cost.CeilRef();
						var fee_trader = (total_cost * 0.09f).Ceil();
						var fee_mass = (total_mass.m_value * data.fee_per_kg).Ceil();
						var fee_base = data.fee_base.Ceil();
						var final_cost = (total_cost + fee_trader + fee_mass + fee_base).Ceil();

						var has_enough_money = credit >= total_cost;
						App.WriteValue((inventory_money.resource, credit, final_cost, has_enough_money));

						Crafting.Context.NewFromSelf(ref region_common, rpc.entity, out var context, search_radius: 6.00f);

						Assert.Check(inventory_money.Remove(resource_money.material, final_cost));
						context.Produce(span_products, spawn_flags: Resource.SpawnFlags.No_Discard | Resource.SpawnFlags.Allow_Encumbered | Resource.SpawnFlags.Show_Notification | Resource.SpawnFlags.Merge | Resource.SpawnFlags.Pickup);
					}
				}

				//ref readonly var rec_vendor = ref rpc.GetRecord();

//ref var inventory_money = ref rpc.

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
						var h_location_region = region_common.GetLocationHandle();

						var pos = this.transform.position;

						ref var store_data = ref this.vendor.h_store.GetData();

						var ent_trader_g = Entity.None;

						ref var target_airship_g = ref TC2.Conquest.WorldMap.Airship.Data.Null;
						ref var target_trader_g = ref TC2.Conquest.WorldMap.Trader.Data.Null;
						ref var target_transform_g = ref Transform.Data.Null;

						if (store_data.IsNotNull())
						{
							ent_trader_g = store_data.h_location.GetGlobalEntity();
						}

						var is_alive_ent_trader_g = ent_trader_g.IsAlive();
						if (is_alive_ent_trader_g)
						{
							target_airship_g = ref ent_trader_g.GetComponent<TC2.Conquest.WorldMap.Airship.Data>();
							target_trader_g = ref ent_trader_g.GetComponent<TC2.Conquest.WorldMap.Trader.Data>();
							target_transform_g = ref ent_trader_g.GetComponent<Transform.Data>();
						}

						var can_signal = false;

						if (target_airship_g.IsNotNull())
						{
							can_signal = h_location_region != target_airship_g.h_location_docked && !target_airship_g.dock_request_locations.Contains(h_location_region);
						}

						var credit = this.inventory_money.resource.GetMarketPrice();

						using (var group_left = GUI.Group.New(size: new(224, GUI.RmY)))
						{
							using (var group_store = GUI.Group.New(size: new(GUI.RmX, 192)))
							{
								var h_store_edit = this.vendor.h_store;
								if (GUI.AssetInput2("edit.store"u8, ref h_store_edit, size: new(GUI.RmX, 40), show_label: false, show_null: true))
								{
									var rpc = new EditOrderRPC
									{
										h_store = h_store_edit
									};
									rpc.Send(this.ent_vendor);
								}

								using (var group_info = GUI.Group.New(size: GUI.Rm, padding: new(10, 8)))
								{
									group_info.DrawBackground(GUI.tex_window);

									if (store_data.IsNotNull())
									{
										//GUI.Title(store_data.h_company.GetName(), size: 20);

										GUI.LabelShaded("Name:"u8, store_data.h_location.GetName());
										GUI.FocusableAsset(store_data.h_location);

										GUI.LabelShaded("Company:"u8, store_data.h_company.GetName());
										GUI.FocusableAsset(store_data.h_company);
									}

									if (target_airship_g.IsNotNull())
									{
										//can_signal = h_location_region != target_airship_g.h_location_docked && !target_airship_g.dock_request_locations.Contains(h_location_region);

										GUI.NewLine(8);

										if (target_transform_g.IsNotNull())
										{
											GUI.LabelShaded("Distance:"u8, TC2.Conquest.WorldMap.km_per_unit * Vec2f.Distance(target_transform_g.position, TC2.Conquest.WorldMap.GetPosition(h_location_region)), format: "0.0' km'");
										}

										GUI.NewLine(8);


										GUI.LabelShaded("Current:"u8, target_airship_g.h_location_docked.GetName());
										GUI.FocusableAsset(target_airship_g.h_location_docked);

										GUI.LabelShaded("Next:"u8, target_airship_g.h_location_target.GetName());
										GUI.FocusableAsset(target_airship_g.h_location_target);

										GUI.NewLine(8);

										if (target_airship_g.h_location_docked == h_location_region)
										{
											GUI.TextShaded("Docked"u8, color: GUI.font_color_green_b);
										}
										else if (target_airship_g.dock_request_locations.ContainsHandle(h_location_region))
										{
											GUI.TextShaded("Request Accepted"u8, color: GUI.font_color_yellow_b);
										}
										else
										{
											GUI.TextShaded("Request Available"u8, color: GUI.font_color_default);
										}
									}
								}
							}

							if (GUI.DrawButton("Call Airship"u8, size: new(128, 40), color: GUI.col_button_yellow, enabled: can_signal))
							{
								var rpc = new TC2.Conquest.WorldMap.Airship.SignalRPC
								{
									h_location = h_location_region
								};
								rpc.Send(ent_trader_g);
							}
							GUI.DrawHoverTooltip("Request the selected airship to stop by this region."u8);
						}

						GUI.SameLine();

						using (var group_right = GUI.Group.New(size: GUI.Rm, padding: new(8)))
						{
							group_right.DrawBackground(GUI.tex_window);

							var final_cost = 0.00f;
							var total_cost = 0.00f;
							var total_mass = (Mass)0.00f;
							var item_count = 0;

							using (var group_catalogue = GUI.Group.New(size: new(GUI.RmX, (48 * 3) + 24 + 2)))
							{
								if (target_trader_g.IsNotNull())
								{
									var h_catalogue = target_trader_g.h_catalogue;
									ref var catalogue_data = ref h_catalogue.GetData();
									if (catalogue_data.IsNotNull())
									{
										GUI.Title(catalogue_data.GetName(), size: 24);
										GUI.FocusableAsset(h_catalogue);

										GUI.SeparatorThick();

										using (var group_items = GUI.Group.New(size: GUI.Rm))
										{
											group_items.DrawBackground(GUI.tex_panel);

											var catalogue_items_span = catalogue_data.items.AsSpan();
											for (var i = 0; i < catalogue_items_span.Length; i++)
											{
												var item = catalogue_items_span[i];
												if (item.header == 0) continue;

												using (var hash = GUI.ID<Vendor.Data, ICatalogue.Data>.Push(i))
												{
													var item_index = this.vendor.edit_selected_items.IndexOf(in item);
													var is_selected = item_index >= 0;

													const float item_h = 48.00f;

													if (i != 0) GUI.TrySameLine(item_h);
													using (var group_item = GUI.Group.New(size: new(item_h), padding: new(4)))
													{
														group_item.DrawBackground(GUI.tex_slot_simple);

														GUI.DrawItem(item, size: GUI.Rm, is_readonly: true, show_quantity: false);
														var rect_item = GUI.GetLastItemRect();

														if (is_selected)
														{
															GUI.DrawRect(rect_item.Pad(1), color: GUI.col_button_ok.WithAlpha(150), layer: GUI.Layer.Window);
														}

														if (GUI.Selectable3(hash, rect_item, selected: is_selected))
														{
															//App.WriteLine($"click [{i:00}] {item}");

															if (is_selected)
															{
																var rpc = new Vendor.EditOrderRPC
																{
																	item = item.WithQuantity(0.00f)
																};
																rpc.Send(this.ent_vendor);
															}
															else
															{
																var rpc = new Vendor.EditOrderRPC
																{
																	item = item.WithQuantity(1.00f)
																};
																rpc.Send(this.ent_vendor);
															}
														}
													}
												}
											}
										}
									}
								}
							}

							GUI.SeparatorThick();
							GUI.NewLine(2);

							using (var group_total = GUI.Group.New(size: GUI.Rm.SubY(32 + 10), padding: new(6)))
							{
								group_total.DrawBackground(GUI.tex_window);

								using (var group_items_title = GUI.Group.New(size: new(GUI.RmX, 24)))
								{
									group_items_title.DrawBackground(GUI.tex_panel);

									GUI.TitleCentered("Purchase Goods"u8, size: 24, pivot: new(0.00f, 0.50f), offset: new(4, 0));
								}

								GUI.SeparatorThick();

								//using (var group_cost = GUI.Group.New(size: new(GUI.RmX, 96), padding: new(8)))
								using (var group_items = GUI.Group.New(size: new(GUI.RmX, 0), padding: new(0)))
								{
									var selected_items_span = this.vendor.edit_selected_items.AsSpan();

									for (var i = 0; i < selected_items_span.Length; i++)
									{
										var item = selected_items_span[i];
										if (item.header == 0) continue;

										var item_price = item.GetMarketPrice();
										var item_mass = item.GetMass();

										total_cost += item_price;
										total_mass += item_mass;

										var h_material = item.h_material;

										using (var hash = GUI.ID<Vendor.Data, Shipment.Item2>.Push(i))
										using (var item_row = GUI.Group.New(size: new(GUI.RmX, 20)))
										{
											var amount_tmp = (int)item.quantity;
											var amount_old = amount_tmp;
											var amount_min = 1;
											var amount_max = 1;
											var step_size = 1;

											ref var material_data = ref h_material.GetData();
											if (material_data.IsNotNull())
											{
												step_size.ClampMinRef((int)material_data.snapping);
												amount_max.ClampMinRef((int)material_data.quantity_max);
											}

											var is_hovered = false;

											using (var button = GUI.CustomButton.New("bt.rem"u8, size: new(GUI.RmY), sound: GUI.sound_select, sound_volume: 0.10f, enabled: true, set_cursor: true))
											{
												var tex = button.hovered ? GUI.tex_button_sub_hover : GUI.tex_button_sub;

												GUI.Draw9Slice(tex, new Vector4(2), button.bb, color: Color32BGRA.White.WithAlphaMult(true ? 1.00f : 0.50f));

												if (button.pressed)
												{
													var rpc = new Vendor.EditOrderRPC
													{
														item = item.WithQuantity(0)
													};
													rpc.Send(this.ent_vendor);
												}
											}

											GUI.SameLine(-2);

											using (var group_item_text = GUI.Group.New(size: GUI.Rm.SubX(48)))
											{
												group_item_text.DrawBackground(GUI.tex_window_sidebar_b);

												GUI.TextShadedCentered(item.h_material.GetName(), pivot: new(0.00f, 0.50f), offset: new(8, 0));
												GUI.FocusableAsset(item.h_material);

												is_hovered |= GUI.IsItemHovered();
												if (is_hovered)
												{
													using (var tooltip = GUI.Tooltip.New(size: new(224, 0)))
													{
														//GUI.DrawMaterialSmall(item.h_material, size: new(32));

														//GUI.SameLine(8);

														using (GUI.Wrap.Push(GUI.RmX))
														{
															//GUI.DrawMaterialSmall(item.h_material, size: new(32));

															GUI.DrawMaterialInfo(item.h_material, amount: item.quantity);
														}
													}
												}

												//GUI.TextShadedCentered(item.quantity, format: "0'x'", pivot: new(0.00f, 0.50f), offset: new(144 + 8, 0));
												//GUI.TextShadedCentered(item_mass, format: "0.##' kg'", pivot: new(0.00f, 0.50f), offset: new(144 + 8, 0), color: GUI.font_color_desc);
												GUI.TextShadedCentered(item_mass, format: "0.##' kg'", pivot: new(0.00f, 0.50f), offset: new(160 + 8, 0), color: GUI.font_color_desc);
												//GUI.TextShadedCentered(-item_price, format: "0' Đk'", pivot: new(0.00f, 0.50f), offset: new(144 + 80, 0), color: GUI.font_color_red_b);
												GUI.TextShadedCentered(item_price, format: "0' Đk'", pivot: new(0.00f, 0.50f), offset: new(160 + 80, 0));
											}

											GUI.SameLine();

											if (GUI.ScalarInput("input"u8, ref amount_tmp, size: GUI.Rm, format: Maths.NumberFormat.Int, show_frame: false)
											|| GUI.ScrollInput(GUI.GetLastItemRect().Pad(1), value: ref amount_tmp, step: step_size, min: amount_min, max: amount_max, id: hash))
											{
												amount_tmp.ClampRef(amount_min, amount_max);
												if (amount_tmp != amount_old)
												{
													var rpc = new Vendor.EditOrderRPC
													{
														item = item.WithQuantity(amount_tmp)
													};
													rpc.Send(this.ent_vendor);
												}
											}
										}

										item_count++;
									}
								}

								GUI.SeparatorThick();

								total_cost.CeilRef();
								var fee_trader = (total_cost * 0.09f).Ceil();
								var fee_mass = (total_mass.m_value * this.vendor.fee_per_kg).Ceil();
								var fee_base = this.vendor.fee_base.Ceil();
								final_cost = (total_cost + fee_trader + fee_mass + fee_base).Ceil();

								using (var group_cost = GUI.Group.New(size: new(GUI.RmX, 0), padding: new(4)))
								{
									//var fee_vat = total_cost * 0.21f;
									//var fee_trader = total_cost * 0.09f;
									//var fee_mass = total_mass * this.vendor.fee_per_kg;
									//var fee_base = this.vendor.fee_base;

									//var fee_vat = total_cost * 0.21f;

									//GUI.TextShadedCentered(total_cost, format: "0' Đk'", pivot: new(0.00f, 0.50f), offset: new(144 + 80 + 48, 0));
									//GUI.TextShadedCentered(total_cost, format: "0' Đk'", pivot: new(0.00f, 0.50f), offset: new(128 + 80 + 48, 0));
									//GUI.NewLine();
									//GUI.TextShadedCentered(fee_vat, format: "'+'0' Đk'", pivot: new(0.00f, 0.50f), offset: new(144 + 80 + 48 - 8, 0), color: GUI.font_color_red_b);
									//GUI.TextShadedCentered(fee_vat, format: "'+'0' Đk'", pivot: new(0.00f, 0.50f), offset: new(144 + 80 + 48 - 8, 0), color: GUI.font_color_red_b);

									//GUI.TextShadedCentered(fee_trader, format: "'+'0' Đk'", pivot: new(0.00f, 0.50f), offset: new(128 + 80 + 48, 0), color: GUI.font_color_red_b);
									//GUI.NewLine();

									GUI.TitleCentered(total_cost, format: "0' Đk'", pivot: new(0.00f, 0.50f), offset: new(128 + 80 + 48, 0));
									if (fee_base > 0.00f)
									{
										GUI.NewLine();
										GUI.TextShadedCentered(fee_base, format: "'+'0' Đk'", pivot: new(0.00f, 0.50f), offset: new(128 + 80 + 48, 0), color: GUI.font_color_red_b);
									}

									if (fee_trader > 0.00f)
									{
										GUI.NewLine();
										GUI.TextShadedCentered(fee_trader, format: "'+'0' Đk'", pivot: new(0.00f, 0.50f), offset: new(128 + 80 + 48, 0), color: GUI.font_color_red_b);
									}

									if (fee_mass > 0.00f)
									{
										GUI.NewLine();
										GUI.TextShadedCentered(fee_mass, format: "'+'0' Đk'", pivot: new(0.00f, 0.50f), offset: new(128 + 80 + 48, 0), color: GUI.font_color_red_b);
									}
									//GUI.NewLine();

									//GUI.Separator(thickness: 1, spacing: 6);
									//GUI.NewLine(2);

									//GUI.TextShadedCentered(total_cost, format: "0' Đk'", pivot: new(0.00f, 0.50f), offset: new(128 + 80 + 48, 0));
									//GUI.NewLine();

									//GUI.TextShadedCentered(total_cost, format: "0' Đk'", pivot: new(0.00f, 0.50f), offset: new(144 + 80 + 48, 0));

									//GUI.LabelShaded("Total"u8, total_cost, format: "0' Đk'");
								}
							}

							GUI.SeparatorThick();

							using (var group_buttons = GUI.Group.New(size: GUI.Rm))
							{
								var has_enough_money = credit >= final_cost;
								using (var group_funds = GUI.Group.New(size: new(144, GUI.RmY), padding: new(6)))
								{
									GUI.LabelShaded("Funds:"u8, credit, format: "0' Đk'");
									//GUI.NewLine(4);
									GUI.LabelShaded("Cost:"u8, final_cost, format: "0' Đk'", color_b: has_enough_money ? GUI.font_color_green_b : GUI.font_color_red_b);
								}

								GUI.SameLine();

								using (var group_funds = GUI.Group.New(size: GUI.Rm.SubX(160), padding: new(6)))
								{

								}

								GUI.SameLine();


								var can_submit = has_enough_money && item_count > 0 && final_cost >= 1.00f;
								if (GUI.DrawButton("Submit Mail Order"u8, size: new(160, GUI.RmY), color: GUI.col_button_ok, error: !can_submit))
								{
									var rpc = new Vendor.SubmitRPC
									{

									};
									rpc.Send(this.ent_vendor);
								}

								if (GUI.IsItemHovered())
								{
									using (var tooltip = GUI.Tooltip.New(size: new(0, 0)))
									{
										GUI.TextShaded("Confirm this trade request.\nThe goods will be delivered by the specified zeppelin."u8);

										if (!has_enough_money) GUI.TextShaded("* Not enough funds!"u8, color: GUI.font_color_red_b);
									}
								}
							}
						}

						//var is_los_sky = true;

						//ref var region = ref region_common.AsRegion();
						//if (region.IsNotNull())
						//{
						//	is_los_sky = region.IsLOS(pos, pos.WithY(0), threshold_solid: 0.00f, allow_inside: false);
						//	if (is_los_sky)
						//	{
						//		is_los_sky &= region.IsInLineOfSight(pos, pos.WithY(0), radius: 3.00f, layer: Physics.Layer.Solid, mask: Physics.Layer.None, exclude: Physics.Layer.World | Physics.Layer.Bounds | Physics.Layer.Water | Physics.Layer.Gas | Physics.Layer.Liquid | Physics.Layer.Fire | Physics.Layer.Essence, skip_inside: false);
						//	}
						//}

						//using (var group_buttons = GUI.Group.New(size: GUI.Rm))
						//{

						//}

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
