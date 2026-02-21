
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
			public Vendor.Type type;
			public byte unused_00;
			public Vendor.Flags flags;

			[Save.NewLine]
			public IStore.Handle h_store;
			public IStockpile.Handle h_stockpile;
			public IWarrant.Handle h_warrant_current;
			public ICatalogue.Handle h_catalogue;
			public uint unused_02;

			[Save.NewLine]
			[Asset.Ignore] public FixedArray16<Shipment.Item2> edit_selected_items;
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
						sync |= data.edit_selected_items.AsSpan(data.unused_00).TryRemove(item_tmp); // .TryRemove(in item_tmp);
					}
					else
					{
						sync |= data.edit_selected_items.AsSpan(data.unused_00).TryAddOrSet(item_tmp);
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
								//group_store.DrawBackground(GUI.tex_window);

								var h_store_edit = this.vendor.h_store;
								if (GUI.AssetInput2("edit.store"u8, ref h_store_edit, size: new(GUI.RmX, 40), show_label: false, show_null: true))
								{
									var rpc = new EditOrderRPC
									{
										h_store = h_store_edit
									};
									rpc.Send(this.ent_vendor);
									//if (h_store_edit != this.vendor.h_store)

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

									// TODO: temporary shithack
									if (is_alive_ent_trader_g)
									{
										//ref var unit_airship = ref ent_trader_g.GetComponent<TC2.Conquest.WorldMap.Airship.Data>();
										if (target_airship_g.IsNotNull())
										{
											//can_signal = h_location_region != target_airship_g.h_location_docked && !target_airship_g.dock_request_locations.Contains(h_location_region);

											GUI.NewLine(8);

											//GUI.LabelShaded("Status:"u8, unit_airship.state);

											//ref var transform_trader_g = ref ent_trader_g.GetTransform();
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
												GUI.TextShaded("In Queue"u8, color: GUI.font_color_yellow_b);
											}
											else
											{
												GUI.TextShaded("Available"u8, color: GUI.font_color_default);
											}
											//if (GUI.DrawButton("Signal"u8, size: new(96, 40), color: GUI.col_button_yellow, enabled: can_signal))
											//{
											//	var rpc = new TC2.Conquest.WorldMap.Airship.SignalRPC
											//	{
											//		h_location = h_location_region
											//	};
											//	rpc.Send(ent_trader_g);
											//}
											//GUI.DrawHoverTooltip("Signal the airship to stop by this region."u8);
										}
									}
								}
								//GUI.DrawEntityIcon(ent_attached, scale: 4);
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

						using (var group_right = GUI.Group.New(size: GUI.Rm))
						{
							using (var group_info = GUI.Group.New(size: GUI.Rm.SubY(40), padding: new(8)))
							{
								group_info.DrawBackground(GUI.tex_window);

								//GUI.TitleCentered(ent_attached.GetName(), pivot: new(0.00f, 0.00f), size: 20);
								//GUI.Title(ent_attached.GetName(), size: 20);

								var total_cost = 0.00f;
								var total_mass = (Mass)0.00f;

								//total_mass += this.ent_vendor.

								//using (var group_items = GUI.Group.New(size: GUI.Rm.SubY((14 * 6) + 4 + 4)))
								//using (var group_items = GUI.Scrollbox.New("sb.items"u8, size: GUI.Rm.SubY((14 * 6) + 4 + 4), padding: new(4)))
								using (var group_catalogue = GUI.Group.New(size: new(GUI.RmX, (48 * 3) + 24)))
								{
									//group_catalogue.DrawBackground(GUI.tex_panel);

									//ref var unit_trader = ref ent_trader_g.GetComponent<TC2.Conquest.WorldMap.Trader.Data>();
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

														//item.quantity = 0;

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
																App.WriteLine($"click [{i:00}] {item}");

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

														//using (var item_row = GUI.Group.New(size: new(GUI.RmX, 32)))
														//{
														//	//using (var item_row = GUI.Group.New(size: new(GUI.RmX, 32)))
														//	{
														//		GUI.DrawItem(item, size: new(GUI.RmY), is_readonly: true);
														//	}
														//}
													}
												}
											}
										}
									}
								}

								GUI.SeparatorThick();
								GUI.NewLine(2);

								//GUI.Separator(spacing: 4);

								using (var group_total = GUI.Group.New(size: GUI.Rm, padding: new(6)))
								{
									group_total.DrawBackground(GUI.tex_window);
									//var total_cost = 0.00f;

									//using (var group_items = GUI.Group.New(size: new(GUI.RmX, 96), padding: new(4)))
									using (var group_items = GUI.Group.New(size: new(GUI.RmX, 0), padding: new(4)))
									{
										group_items.DrawBackground(GUI.tex_panel);

										GUI.Title("Ordered Goods"u8, size: 24);

										GUI.NewLine(4);

										var item_width = GUI.RmX / 8;

										var selected_items_span = this.vendor.edit_selected_items.AsSpan();
										for (var i = 0; i < selected_items_span.Length; i++)
										{
											var item = selected_items_span[i];
											if (item.header == 0) continue;

											var item_price = item.GetMarketPrice();
											var item_mass = item.GetMass();

											total_cost += item_price;
											total_mass += item_mass;

											if (false)
											{
												using (var hash = GUI.ID<Vendor.Data, Shipment.Item2>.Push(i))
												{
													if (i != 0) GUI.TrySameLine(item_width);
													//using (var group_item = GUI.Group.New(size: new(item_h), padding: new(4)))
													//using (var group_item = GUI.Group.New(size: new(item_width, item_width + 16)))
													using (var group_item = GUI.Group.New(size: new(item_width)))
													{
														//group_item.DrawBackground(GUI.tex_slot_simple);

														GUI.DrawItem(item, size: new(item_width), is_readonly: true);
														var rect_item = GUI.GetLastItemRect();

														//if (is_selected)
														//{
														//	GUI.DrawRect(rect_item.Pad(1), color: GUI.col_button_ok.WithAlpha(150), layer: GUI.Layer.Window);
														//}

														//if (GUI.ButtonBehavior(hash.hash.id, rect_item, out var hovered, out var held, allow_overlap: false, keys: GUI.ButtonKeys.Left | GUI.ButtonKeys.Right))
														//{
														//	App.WriteLine($"click [{i:00}] {item}");

														//}
														if (GUI.Selectable3(hash, rect_item, selected: false))
														{
															App.WriteLine($"click [{i:00}] {item}");

															//if (is_selected)
															//{
															//	var rpc = new Vendor.EditOrderRPC
															//	{
															//		item = item.WithQuantity(0.00f)
															//	};
															//	rpc.Send(this.ent_vendor);
															//}
															//else
															//{
															//	var rpc = new Vendor.EditOrderRPC
															//	{
															//		item = item.WithQuantity(1.00f)
															//	};
															//	rpc.Send(this.ent_vendor);
															//}
														}

														var amount_tmp = (int)item.quantity;
														if (GUI.ScrollInput(rect_item, value: ref amount_tmp, step: 1, min: 1, max: 100, id: hash))
														{
															var rpc = new Vendor.EditOrderRPC
															{
																item = item.WithQuantity(amount_tmp)
															};
															rpc.Send(this.ent_vendor);
														}

														//GUI.TextShadedCentered(item_price, format: "0' Đk", pivot: new(0.00f, 0.00f), size: 12, color: GUI.font_color_red_b);

														//var amount_tmp = (int)item.quantity;
														//if (GUI.HoverInputInt(item.header.ToString(), rect_item, ref amount_tmp, 1, 100))
														//{

														//}
													}
												}
											}
											//using (var item_row = GUI.Group.New(size: new(GUI.RmX, 32)))
											//{
											//	//using (var item_row = GUI.Group.New(size: new(GUI.RmX, 32)))
											//	{
											//		GUI.DrawItem(item, size: new(GUI.RmY), is_readonly: true);
											//	}
											//}

										}
									}

									GUI.SeparatorThick();

									//using (var group_cost = GUI.Group.New(size: new(GUI.RmX, 96), padding: new(8)))
									using (var group_cost = GUI.Group.New(size: new(GUI.RmX, 0), padding: new(0)))
									{
										var selected_items_span = this.vendor.edit_selected_items.AsSpan();

										for (var i = 0; i < selected_items_span.Length; i++)
										{
											var item = selected_items_span[i];
											if (item.header == 0) continue;

											//if (i != 0) GUI.

											var item_price = item.GetMarketPrice();
											var item_mass = item.GetMass();

											using (var hash = GUI.ID<Vendor.Data, Shipment.Item2>.Push(i))
											using (var item_row = GUI.Group.New(size: new(GUI.RmX, 20)))
											{
												var amount_tmp = (int)item.quantity;
												var amount_old = amount_tmp;

												var is_hovered = false;

												if (GUI.ScalarInput("input"u8, ref amount_tmp, size: new(48, GUI.RmY), format: Maths.NumberFormat.Int, show_frame: false)
												|| GUI.ScrollInput(GUI.GetLastItemRect().Pad(2), value: ref amount_tmp, step: 1, min: 1, max: 100, id: hash))
												{
													amount_tmp.ClampRef(1, 100);
													if (amount_tmp != amount_old)
													{
														var rpc = new Vendor.EditOrderRPC
														{
															item = item.WithQuantity(amount_tmp)
														};
														rpc.Send(this.ent_vendor);
													}
												}
												is_hovered |= GUI.IsItemHovered();

												GUI.SameLine(4);

												using (var group_item_text = GUI.Group.New(size: GUI.Rm.SubX(20)))
												{
													//group_item_text.DrawBackground(GUI.tex_panel_white, color: GUI.col_black.WithAlpha(80));
													group_item_text.DrawBackground(GUI.tex_window_sidebar);

													GUI.TextShadedCentered(item.h_material.GetName(), pivot: new(0.00f, 0.50f));
													GUI.FocusableAsset(item.h_material);

													is_hovered |= GUI.IsItemHovered();
													if (is_hovered)
													{
														using (var tooltip = GUI.Tooltip.New(size: new(224, 0)))
														using (GUI.Wrap.Push(GUI.RmX))
														{
															GUI.DrawMaterialInfo(item.h_material, amount: item.quantity);
														}
													}

													//GUI.item

													//GUI.TextShadedCentered(item.quantity, format: "0'x'", pivot: new(0.00f, 0.50f), offset: new(144 + 8, 0));
													GUI.TextShadedCentered(item_mass, format: "0.##' kg'", pivot: new(0.00f, 0.50f), offset: new(144 + 8, 0), color: GUI.font_color_desc);
													GUI.TextShadedCentered(item_price, format: "0' Đk'", pivot: new(0.00f, 0.50f), offset: new(144 + 80, 0));
												}

												GUI.SameLine();

												using (var button = GUI.CustomButton.New("bt.rem"u8, size: GUI.Rm, sound: GUI.sound_select, sound_volume: 0.10f, enabled: true, set_cursor: true))
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

													//if (pressed)
													//{
													//	value += step;
													//}
												}

												//if (GUI.DrawStepButton("step"u8, size: GUI.Rm, value: ref amount_tmp, step: 1, min: 1, max: 10, add: true, enabled: true))
												//{

												//}
											}
										}

										//using (var group_l = group_cost.Split(size: new(192, 64), align_x: GUI.AlignX.Left, align_y: GUI.AlignY.Top))
										//{
										//	//var selected_items_span = this.vendor.edit_selected_items.AsSpan();
										//	//selected_items_span.get

										//	GUI.LabelShaded("Credit:"u8, credit, format: "0' Đk'");
										//	GUI.LabelShaded("Price:"u8, total_cost, format: "0' Đk'");

										//	//var selected_items_span = this.vendor.edit_selected_items.AsSpan();
											
										//}
									}
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
