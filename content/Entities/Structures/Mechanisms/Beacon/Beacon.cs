
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

			public float fee_base;
			public float fee_per_kg;
		}

		public struct EditRPC: Net.IRPC<Beacon.Data>
		{
			public Beacon.Type? type;
			public Beacon.Flags? flags;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Beacon.Data data)
			{
				var sync = false;

				sync |= data.flags.TrySetFlagMasked(this.flags, Beacon.Flags.Pending);

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

				var ent_attached = rpc.entity.GetParent<Sticky.Rel>();

				var price_estimate = 0.00f;
				var total_mass = (Mass)0.00f;

				if (ent_attached.IsAlive())
				{
					ref var body = ref ent_attached.GetComponent<Body.Data>();
					if (body.IsNotNull())
					{
						total_mass += body.GetMass() - (body.inventory_weight * body.inventory_weight_multiplier);
					}

					foreach (var h_inventory in ent_attached.GetInventories())
					{
						var span_items = h_inventory.GetReadOnlySpan();
						foreach (var item in span_items)
						{
							if (item)
							{
								total_mass += item.GetMass();
								price_estimate += item.GetMarketPrice();
							}
						}
					}

					ref var harvestable = ref ent_attached.GetComponent<Harvestable.Data>();
					if (harvestable.IsNotNull())
					{
						ref var harvestable_state = ref ent_attached.GetComponent<Harvestable.State>();
						if (harvestable_state.IsNotNull())
						{
							var items = harvestable.resources;
							for (var i = 0; i < items.Length; i++)
							{
								var item = items[i];
								if (item)
								{
									item.quantity *= (1.00f - harvestable_state.pct_spawned[i].Value);
									price_estimate += item.GetMarketPrice();
								}
							}
						}
					}
				}

				var fee_mass = total_mass.m_value * data.fee_per_kg;
				var fee_base = price_estimate * data.fee_base;

				var price_w_fees = price_estimate - fee_base - fee_mass;
				var tax = Maths.Abs(price_w_fees) * 0.21f;
				var price_w_tax = price_w_fees - tax;



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

						using (var group_left = GUI.Group.New(size: new(192, GUI.RmY)))
						{
							using (var group_icon = GUI.Group.New(size: new(GUI.RmX, 128)))
							{
								group_icon.DrawBackground(GUI.tex_window);
								GUI.DrawEntityIcon(ent_attached, scale: 4);
							}
						}

						GUI.SameLine();

						using (var group_right = GUI.Group.New(size: GUI.Rm))
						{
							using (var group_info = GUI.Group.New(size: GUI.Rm.SubY(40), padding: new(10)))
							{
								group_info.DrawBackground(GUI.tex_window);

								//GUI.TitleCentered(ent_attached.GetName(), pivot: new(0.00f, 0.00f), size: 20);
								GUI.Title(ent_attached.GetName(), size: 20);

								var price_estimate = 0.00f;
								var total_mass = (Mass)0.00f;

								//total_mass += this.ent_beacon.

								using (var group_items = GUI.Group.New(size: GUI.Rm.SubY((14 * 6) + 4 + 4)))
								{
									if (ent_attached.IsAlive())
									{
										ref var body = ref ent_attached.GetComponent<Body.Data>();
										if (body.IsNotNull())
										{
											total_mass += body.GetMass() - (body.inventory_weight * body.inventory_weight_multiplier);
										}

										foreach (var h_inventory in ent_attached.GetInventories())
										{
											//GUI.Title(h_inventory.Type.ToStringUtf8());
											var span_items = h_inventory.GetReadOnlySpan();
											foreach (var item in span_items)
											{
												if (item)
												{
													ref var material = ref item.GetMaterial();
													if (material.IsNotNull())
													{
														total_mass += item.GetMass();

														ref var commodity = ref material.commodity.GetRefOrNull();
														if (commodity.IsNotNull())
														{
															var item_price = commodity.market_price * item.quantity;
															price_estimate += item_price;

															GUI.TextShaded(item.quantity, format: "0'x'");
															GUI.OffsetLine(48);
															GUI.LabelShaded(item.GetName(), item_price, format: "0' Đk'");
															//GUI.SameLine(8);
															//GUI.TextShaded(item.GetMass(), format: "0' kg'", size: 12);

															//if (GUI.IsItemHovered())
															//{
															//	using (var tooltip = GUI.Tooltip.New())
															//	{

															//	}
															//}
														}
														else
														{
															GUI.TextShaded(item.quantity, format: "0'x'", color: GUI.font_color_desc);
															GUI.OffsetLine(48);
															GUI.LabelShaded(item.GetName(), "N/A"u8, color_a: GUI.font_color_desc, color_b: GUI.font_color_desc);
															//GUI.TextShaded(item.GetShortName(), color: GUI.font_color_default.WithAlpha((byte)(commodity.IsNotNull() ? 255 : 100)));
														}

														if (GUI.IsItemHovered())
														{
															using (var tooltip = GUI.Tooltip.New(size: new(192, 0)))
															{
																GUI.LabelShaded("Mass"u8, item.GetMass(), format: "0.##' kg'");
															}
														}

														GUI.FocusableAsset(item.material);
													}
												}
											}
										}

										ref var harvestable = ref ent_attached.GetComponent<Harvestable.Data>();
										if (harvestable.IsNotNull())
										{
											ref var harvestable_state = ref ent_attached.GetComponent<Harvestable.State>();
											if (harvestable_state.IsNotNull())
											{
												GUI.NewLine(2);
												GUI.Separator(spacing: 4, thickness: 1.00f);


												GUI.Title("Harvestable"u8, color: GUI.font_color_yellow_b);
												var items = harvestable.resources;
												for (var i = 0; i < items.Length; i++)
												{
													var item = items[i];
													if (item)
													{
														item.quantity *= (1.00f - harvestable_state.pct_spawned[i].Value);

														ref var material = ref item.GetMaterial();
														if (material.IsNotNull())
														{
															//total_mass += item.GetMass();

															ref var commodity = ref material.commodity.GetRefOrNull();
															if (commodity.IsNotNull())
															{
																var item_price = commodity.market_price * item.quantity;
																price_estimate += item_price;

																GUI.TextShaded(item.quantity, format: "'~'0'x'", color: GUI.font_color_yellow_b);
																GUI.OffsetLine(48);
																GUI.LabelShaded(item.GetName(), item_price, format: "0' Đk'", color_a: GUI.font_color_yellow_b, color_b: GUI.font_color_yellow_b);
																GUI.FocusableAsset(item.material);

																//if (GUI.IsItemHovered())
																//{
																//	using (var tooltip = GUI.Tooltip.New())
																//	{

																//	}
																//}
															}
															else
															{
																GUI.TextShaded(item.quantity, format: "'~'0'x'", color: GUI.font_color_yellow_b);
																GUI.OffsetLine(48);
																GUI.LabelShaded(item.GetName(), "N/A"u8, color_a: GUI.font_color_desc, color_b: GUI.font_color_desc);
																//GUI.TextShaded(item.GetShortName(), color: GUI.font_color_default.WithAlpha((byte)(commodity.IsNotNull() ? 255 : 100)));
															}

															if (GUI.IsItemHovered())
															{
																using (var tooltip = GUI.Tooltip.New(size: new(192, 0)))
																{
																	GUI.LabelShaded("Mass"u8, item.GetMass(), format: "0.##' kg'");
																}
															}

															GUI.FocusableAsset(item.material);
														}
													}
												}
											}
										}
									}

									GUI.NewLine(2);
									GUI.Separator(spacing: 4, thickness: 1.00f);
								}

								//GUI.Separator(spacing: 4);

								using (var group_total = GUI.Group.New(size: GUI.Rm))
								{
									var fee_mass = total_mass.m_value * this.beacon.fee_per_kg;
									var fee_base = price_estimate * this.beacon.fee_base;

									var price_w_fees = price_estimate - fee_base - fee_mass;
									var tax = Maths.Abs(price_w_fees) * 0.21f;
									var price_w_tax = price_w_fees - tax;

									GUI.LabelShaded("Value"u8, price_estimate, format: "0' Đk'");
									GUI.LabelShaded("Fee (Base)"u8, -fee_base, format: "0' Đk'", color_b: GUI.font_color_red_b);
									GUI.LabelShaded("Fee (Mass)"u8, -fee_mass, format: "0' Đk'", color_b: GUI.font_color_red_b);
									GUI.LabelShaded("Sales Tax"u8, -tax, format: "0' Đk'", color_b: GUI.font_color_red_b);
									GUI.Separator(spacing: 4, thickness: 1.00f);
									GUI.LabelShaded("Reward"u8, price_w_tax, format: "0' Đk'", color_b: price_w_tax < 0.00f ? GUI.font_color_red_b : GUI.font_color_green_b);
									GUI.LabelShaded("Mass"u8, total_mass, format: "0.##' kg'");
								}
							}

							using (var group_buttons = GUI.Group.New(size: GUI.Rm))
							{
								//group_buttons.DrawBackground(GUI.tex_window);

								if (this.beacon.flags.HasAny(Beacon.Flags.Pending))
								{
									if (GUI.DrawButton("Deactivate"u8, size: new(96, GUI.RmY), color: GUI.col_button_error))
									{
										var rpc = new Beacon.EditRPC
										{
											flags = this.beacon.flags.WithRemoved(Beacon.Flags.Pending)
										};
										rpc.Send(this.ent_beacon);
									}
								}
								else
								{
									if (GUI.DrawButton("Activate"u8, size: new(96, GUI.RmY), color: GUI.col_button_ok, enabled: ent_attached.IsAlive()))
									{
										var rpc = new Beacon.EditRPC
										{
											flags = this.beacon.flags.WithAdded(Beacon.Flags.Pending)
										};
										rpc.Send(this.ent_beacon);
									}
									GUI.DrawHoverTooltip("Activate the beacon, allowing zeppelins to pick up this object."u8);
								}

								if (GUI.ShowDebugGUI)
								{
									GUI.SameLine();

									if (GUI.DrawButton("DEV: Sell"u8, size: new(96, GUI.RmY), color: GUI.col_button_debug))
									{
										var rpc = new Beacon.ActionRPC
										{
										};
										rpc.Send(this.ent_beacon);
									}
								}
							}
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
