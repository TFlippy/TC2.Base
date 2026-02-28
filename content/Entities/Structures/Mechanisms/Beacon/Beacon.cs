
namespace TC2.Base.Components
{
	public static partial class Beacon
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0u,

			Active = 1u << 0,

			[Asset.Ignore] Obstructed = 1 << 16,
		}

		public enum Type: byte
		{
			Undefined = 0,

			Trade,
			Cargo,
		}

		public const Beacon.Flags flags_editable_mask = Beacon.Flags.Active;

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

				sync |= data.flags.TrySetFlagMasked(this.flags, Beacon.flags_editable_mask);

				if (sync)
				{
					data.Sync(rpc.entity, true);
				}
			}
#endif
		}

		public struct DEV_SellRPC: Net.IRPC<Beacon.Data>
		{


#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Beacon.Data data)
			{
				Assert.IsDevMode();
				Assert.IsAdmin(ref rpc.connection);

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

					var fee_mass = total_mass.m_value * data.fee_per_kg;
					var fee_base = price_estimate * data.fee_base;

					var price_w_fees = price_estimate - fee_base - fee_mass;
					var tax = Maths.Abs(price_w_fees) * 0.21f;
					var price_w_tax = price_w_fees - tax;

					ent_attached.AddTag("no_gib");
					rpc.entity.AddTag("no_gib");

					ent_attached.Delete();
					rpc.entity.Delete();

					Crafting.Context.NewFromCharacter(ref rpc.GetRegionCommon(), rpc.GetSenderCharacterHandle(), rpc.entity, out var context, search_radius: 4.00f);

					context.Produce([Crafting.Product.Money(price_w_tax)], spawn_flags: Resource.SpawnFlags.Pickup | Resource.SpawnFlags.Show_Notification | Resource.SpawnFlags.Allow_Encumbered | Resource.SpawnFlags.No_Discard | Resource.SpawnFlags.No_Offset);
				}

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
				using (var window = GUI.Window.Interaction("Trade Beacon"u8, this.ent_beacon))
				{
					this.StoreCurrentWindowTypeID(order: 7);
					if (window.show)
					{
						ref var region_common = ref this.ent_beacon.GetRegionCommon();
						var ent_attached = this.ent_beacon.GetParent<Sticky.Rel>();
						var pos = this.transform.position;

						var price_estimate = 0.00f;
						var total_mass = (Mass)0.00f;
						var reward = 0.00f;

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

								//total_mass += this.ent_beacon.

								//using (var group_items = GUI.Group.New(size: GUI.Rm.SubY((14 * 6) + 4 + 4)))
								using (var group_items = GUI.Scrollbox.New("sb.items"u8, size: GUI.Rm.SubY((14 * 6) + 4 + 4), padding: new(4)))
								{
									if (ent_attached.IsAlive())
									{
										ref var body = ref ent_attached.GetComponent<Body.Data>();
										if (body.IsNotNull())
										{
											total_mass += body.GetMass() - (body.inventory_weight * body.inventory_weight_multiplier);
											pos = body.GetPosition();
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

									//GUI.NewLine(2);
									//GUI.Separator(spacing: 4, thickness: 1.00f);
								}

								GUI.SeparatorThick();
								GUI.NewLine(2);

								//GUI.Separator(spacing: 4);

								using (var group_total = GUI.Group.New(size: GUI.Rm))
								{
									var fee_mass = total_mass.m_value * this.beacon.fee_per_kg;
									var fee_base = price_estimate * this.beacon.fee_base;

									var price_w_fees = price_estimate - fee_base - fee_mass;
									var tax = Maths.Abs(price_w_fees) * 0.21f;
									var price_w_tax = price_w_fees - tax;

									reward = price_w_tax;

									GUI.LabelShaded("Value"u8, price_estimate, format: "0' Đk'");
									GUI.LabelShaded("Service Fee"u8, -fee_base, format: "0' Đk'", color_b: GUI.font_color_red_b);
									GUI.LabelShaded("Weight Fee"u8, -fee_mass, format: "0' Đk'", color_b: GUI.font_color_red_b);
									GUI.LabelShaded("Tax"u8, -tax, format: "0' Đk'", color_b: GUI.font_color_red_b);
									GUI.Separator(spacing: 4, thickness: 1.00f);
									GUI.LabelShaded("Reward"u8, reward, format: "0' Đk'", color_b: reward < 0.00f ? GUI.font_color_red_b : GUI.font_color_green_b);
									GUI.LabelShaded("Mass"u8, total_mass, format: "0.##' kg'");
								}
							}

							var is_los_sky = true;

							ref var region = ref region_common.AsRegion();
							if (region.IsNotNull())
							{
								//var ts = Timestamp.Now();
								is_los_sky = region.IsLOS(pos, pos.WithY(0), threshold_solid: 0.00f, allow_inside: false);
								if (is_los_sky)
								{
									//region.TryOverlapBB()
									is_los_sky &= region.IsInLineOfSight(pos, pos.WithY(0), radius: 3.00f, layer: Physics.Layer.Solid, mask: Physics.Layer.None, exclude: Physics.Layer.World | Physics.Layer.Bounds | Physics.Layer.Water | Physics.Layer.Gas | Physics.Layer.Liquid | Physics.Layer.Fire | Physics.Layer.Essence, skip_inside: false);
								}
								//var ts_elapsed_ms = ts.GetMilliseconds();
								//GUI.Text($"{ts_elapsed_ms:0.0000} ms");

								//GUI.DrawLine(region.WorldToCanvas(pos), region.WorldToCanvas(pos with { Y = 0 }), color: is_los_sky.GetColor32(), thickness: region.GetWorldToCanvasScale(), layer: GUI.Layer.Foreground);

							}

							using (var group_buttons = GUI.Group.New(size: GUI.Rm))
							{
								group_buttons.DrawBackground(GUI.tex_window);

								if (false)
								{
									if (this.beacon.flags.HasAny(Beacon.Flags.Active))
									{
										if (GUI.DrawButton("Deactivate"u8, size: new(96, GUI.RmY), color: GUI.col_button_error))
										{
											var rpc = new Beacon.EditRPC
											{
												flags = this.beacon.flags.WithRemoved(Beacon.Flags.Active)
											};
											rpc.Send(this.ent_beacon);
										}
									}
									else
									{
										if (GUI.DrawButton("Activate"u8, size: new(96, GUI.RmY), color: GUI.col_button_ok, error: !(is_los_sky && ent_attached.IsAlive())))
										{
											var rpc = new Beacon.EditRPC
											{
												flags = this.beacon.flags.WithAdded(Beacon.Flags.Active)
											};
											rpc.Send(this.ent_beacon);
										}
										GUI.DrawHoverTooltip("Activate the beacon, allowing zeppelins to pick up this object."u8);
									}
								}

								if (GUI.ShowDebugGUI)
								{
									//GUI.SameLine();

									if (GUI.DrawButton("DEV: Sell"u8, size: new(96, GUI.RmY), color: GUI.col_button_debug, error: !(is_los_sky && reward >= 1.00f && ent_attached.IsAlive())))
									{
										var rpc = new Beacon.DEV_SellRPC
										{
										};
										rpc.Send(this.ent_beacon);
									}
								}
								else
								{
									GUI.TitleCentered(">> coming soon (sorry) <<"u8, pivot: new(0.50f), size: 24, color: GUI.col_button_yellow);
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
			//if (beacon.flags.HasAny(Flags.Active))
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
		[Source.Owned] ref Beacon.Data beacon, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body)
		{
			//App.WriteLine("draw");
			//if (beacon.flags.HasAny(Flags.Active))
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
