using TC2.Base.Components;

namespace TC2.Base
{
	public static partial class SideMenu
	{
#if CLIENT
		[FixedAddressValueType] public static bool show_help = true;
		[FixedAddressValueType] public static bool show_pins = false;
		[FixedAddressValueType] public static bool show_interact = true;
		[FixedAddressValueType] public static bool show_pickups = true;

		[Net.MsgPack]
		public struct PinnedItem
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0u,

				Done = 1 << 0,

			}

			[Net.Key(00), Save.Force] public PinnedItem.Flags flags;

			[Save.NewLine]
			[Net.Key(01), Save.Force] public Shipment.Item2 item;
			[Net.Key(02), Save.Force] public float amount_req;
		}

		static partial void Ext_DrawScrollbox_Top(ref SideMenuGUI gui, ref readonly GUI.Window window, ref readonly GUI.Scrollbox scrollbox);
		static partial void Ext_DrawScrollbox_Bottom(ref SideMenuGUI gui, ref readonly GUI.Window window, ref readonly GUI.Scrollbox scrollbox);

		// WIP/experimental stuff
		public partial struct SideMenuGUI: IGUICommand
		{
			//public Entity ent_player;

			public static readonly HashSet<IHelp.Handle> hs_selected_handles = new(32);

			public static IHelp.Handle edit_h_selected;
			public static string edit_help_filter;
			[FixedAddressValueType] public static PinnedList<PinnedItem> pinned_items = new(32);

			public static Vec2f pos_cached_interact;
			public static Vec2f pos_cached_pickup;

			public void Draw()
			{
				ref var region_common = ref Client.GetRegionCommon();
				if (region_common.IsNull()) return;

				ref var player_data = ref Client.GetPlayerData();
				if (player_data.IsNull()) return;

				var h_character = Client.GetCharacterHandle();
				//ref var character = ref region.GetCharacter(h_character);
				//if (character.IsNotNull())
				{
					var window_size_base = new Vec2f((24 * 9) + 14, 48 * 10);

					using (var window = GUI.Window.Standalone("sidemenu"u8, position: new(GUI.CanvasSize.X - 8, 36),
					pivot: new(1.00f, 0.00f), size: window_size_base, size_min: window_size_base.WithY(136), size_max: window_size_base.WithY(512), 
					padding: new(8), flags: GUI.Window.Flags.Resizable))
					//pivot: new(1.00f, 0.00f), size: window_size_base, padding: new(8), flags: GUI.Window.Flags.Resizable))
					{
						if (window.show)
						{
							var hs_selected_handles = SideMenuGUI.hs_selected_handles;

							//GUI.DrawWindowBackground(GUI.tex_window_popup_l, 4, color: GUI.col_button);
							GUI.DrawWindowBackground(GUI.tex_window_sidebar, 4);

							//using (var group_top = GUI.Group.New(size: new(GUI.RmX, 32)))
							//{
							//	if (GUI.DrawIconButton("button.a"u8, Sprite.Empty, size: new(GUI.RmY)))
							//	{

							//	}
							//}

							//GUI.SeparatorThick();

							ref var region = ref region_common.AsRegion();
							ref var character = ref region.GetCharacter(h_character, out var ent_character);
							if (character.IsNotNull())
							{
								pos_cached_interact.UpdateCachedPosition(character.world_position, 0.50f);
								pos_cached_pickup.UpdateCachedPosition(character.world_position, 0.50f);
							}

							//if (region.IsNotNull())
							//{
							//	GUI.DrawPoint(region.WorldToCanvas(pos_cached_interact), color: Color32BGRA.Cyan, layer: GUI.Layer.Foreground);
							//	GUI.DrawPoint(region.WorldToCanvas(pos_cached_pickup), color: Color32BGRA.Yellow, layer: GUI.Layer.Foreground);
							//}

							using (var scrollbox = GUI.Scrollbox.New("sidemenu.scroll"u8, size: GUI.Rm))
							{
								Ext_DrawScrollbox_Top(ref this, in window, in scrollbox);

								//if (App.IsModEditing)
								if (SideMenu.show_help)
								{
									using (var collapsible = GUI.Collapsible2.New("col.help"u8, size: new(GUI.RmX, 32), default_open: true))
									{
										GUI.TitleCentered("Tutorial"u8, size: 24, pivot: new(0.00f, 0.50f));

										if (collapsible.Inner())
										{
											//using (var group_help = GUI.Group.New(size: new(GUI.RmX, 0)))
											{
												//if (GUI.TextInput("help.search"u8, "<search>"u8, ref edit_help_filter, size: new(GUI.RmX, 32), max_length: 32))
												//{

												//}
												//GUI.FocusOnCtrlF();

												var categories = IHelp.Database.GetAssets().GroupBy(x => x.data.category, StringComparer.Ordinal).OrderBy(x => x.Key);
												foreach (var pair in categories)
												{
													var category = pair.Key;

													using (var push_id_category = GUI.ID<SideMenuGUI>.Push(category))
													using (var collapsible_category = GUI.Collapsible2.New("col.category"u8, size: new(GUI.RmX, 32), default_open: false))
													{
														GUI.TitleCentered(category, size: 16, pivot: new(0.00f, 0.50f));

														if (collapsible_category.Inner())
														{
															foreach (var asset in pair.OrderBy(x => x.data.order))
															//foreach (var asset in assets)
															{
																ref var help_data = ref asset.GetData(out var h_help);
																if (help_data.flags.HasAny(IHelp.Flags.Hidden)) continue;
																//if (help_data.flags.HasAny(IHelp.Flags.Disabled)) continue;

																//if (help_data.IsNull()) continue;

																var is_visible = false;
																var is_selected = h_help == edit_h_selected; // hs_selected_handles.Contains(h_help);

																using (var push_id = GUI.ID<SideMenuGUI, IHelp.Data>.Push(h_help))
																using (var group_row = GUI.Group.New(size: new(GUI.RmX, 28)))
																{
																	is_visible = group_row.IsVisible();
																	if (is_visible)
																	{
																		//group_row.DrawBackground(GUI.tex_panel);
																		//GUI.TitleCentered(ent_result.GetName(), pivot: new(0.00f, 0.50f), offset: new(6, 0));

																		using (var group_icon = GUI.Group.New(size: new(GUI.RmY)))
																		{
																			group_icon.DrawBackground(GUI.tex_slot_filled);
																		}

																		GUI.SameLine(-4);

																		using (var group_text = GUI.Group.New(size: GUI.Rm))
																		{
																			group_text.DrawBackground(GUI.tex_window_popup_r, color: GUI.col_button);

																			GUI.TitleCentered(help_data.name, pivot: new(0.00f, 0.50f), offset: new(10, 0));
																		}

																		var row_rect = group_row.GetOuterRect();
																		if (GUI.Selectable3(id: push_id.hash, rect: row_rect, selected: is_selected))
																		{
																			edit_h_selected.Toggle(h_help);
																		}
																	}
																}

																if (is_visible)
																{
																	//var row_rect = GUI.GetLastItemRect(out var row_hash);
																	//var is_selected = hs_selected_handles.Contains(h_help);

																	//if (GUI.Selectable3(id: row_hash, rect: row_rect, selected: is_selected))
																	//{
																	//	//hs_selected_handles.Toggle(h_help);
																	//}


																	//if (!GUI.IsAnyTooltipVisible() && (is_selected || GUI.IsItemHovered()))
																	if ((edit_h_selected == 0 && GUI.IsItemHovered()) || (edit_h_selected == h_help))
																	{
																		//using (var tooltip = GUI.Tooltip.New(pos: GUI.GetLastItemRect().tl, override_prev: true,
																		//pivot: new(1.00f, 0.00f), size: help_data.size.ToVec2f()))

																		using (var tooltip = GUI.Window.Standalone("tutorial"u8, position: GUI.GetLastItemRect().tl,
																		pivot: new(1.00f, 0.00f), size: help_data.size.ToVec2f(), padding: new(8), force_position: true,
																		flags: GUI.Window.Flags.No_Input))
																		{
																			tooltip.BringToFront();

																			if (tooltip.show)
																			{
																				GUI.DrawWindowBackground();
																				using (var group_top = GUI.Group.New(size: new(GUI.RmX, 128)))
																				{
																					ref var sprite = ref help_data.sprite;
																					if (sprite.texture)
																					{
																						var sprite_scale = help_data.sprite_scale;
																						using (var group_sprite = GUI.Group.New(size: sprite.size.ToVec2f() * sprite_scale))
																						{
																							var rect_sprite = GUI.GetRemainingRect(); // AABB.Simple(GUI.GetCursorScreenPos(), size: Maths.ScaleToWidth(GUI.Rm, width: GUI.RmX));
																							GUI.DrawSpriteCentered(sprite: sprite, rect: rect_sprite, constrain: false,
																							layer: GUI.Layer.Window, dummy: false, scale: sprite_scale);

																							GUI.Draw9Slice(texture: GUI.tex_frame, padding: new(4), rect: rect_sprite);

																							//GUI.DrawRect(rect_sprite, layer: GUI.Layer.Foreground);

																							//GUI.SeparatorThick();
																						}

																						GUI.SameLine();

																						using (var group_title = GUI.Group.New(size: GUI.Rm, padding: new(6, 2)))
																						using (GUI.Wrap.Push(GUI.RmX))
																						{
																							GUI.Title(text: help_data.title.OrDefault(help_data.name), size: help_data.title_size);

																							GUI.SeparatorThick(spacing: 2);

																							GUI.NewLine(4);
																							GUI.TextShaded(help_data.desc);
																						}
																					}
																				}

																				using (GUI.Wrap.Push(GUI.RmX))
																				{
																					//GUI.Title(help_data.name);

																					//GUI.SeparatorThick(spacing: 2);

																					//GUI.NewLine(4);
																					//GUI.TextShaded(help_data.desc);




																					var entries = help_data.entries.AsSpan();
																					if (!entries.IsEmpty)
																					{
																						GUI.NewLine(4);

																						var entry_index = 0;
																						foreach (ref var entry in entries)
																						{
																							var entry_flags = entry.flags;
																							if (entry_index != 0)
																							{
																								GUI.NewLine(4);
																							}
																							//else if (entry_flags.HasAny(IHelp.Entry.Flags.Newline))
																							//{
																							//	GUI.NewLine(4);
																							//}

																							if (entry_flags.HasAny(IHelp.Entry.Flags.Bullet))
																							{
																								if (entry_flags.HasAny(IHelp.Entry.Flags.Nested))
																								{
																									GUI.SameLine(10);
																								}

																								GUI.Text("-"u8, color: entry.color);
																								GUI.SameLine(2);
																							}
																							//GUI.Text("- "u8);
																							//GUI.SameLine();

																							if (entry_flags.HasAny(IHelp.Entry.Flags.Bold))
																							{
																								GUI.TextShaded(text: entry.text, color: entry.color, font: GUI.Font.Superstar, size: 16);
																							}
																							else
																							{
																								GUI.TextShaded(text: entry.text, color: entry.color);
																							}

																							entry_index++;
																						}
																					}

																					GUI.NewLine(4);
																					//GUI.DrawHoverTooltip(help_data.desc);
																				}
																			}
																		}

																		GUI.FocusableAsset(asset);
																	}
																}
															}
														}
													}
												}

												//var ts_elapsed = ts.GetMilliseconds();
												//GUI.Text(ts_elapsed, "0.0000'ms'");
											}
										}
									}
								}

								if (SideMenu.show_pins)
								{
									using (var collapsible = GUI.Collapsible2.New("col.pins"u8, size: new(GUI.RmX, 32), default_open: true))
									{
										GUI.TitleCentered("Pins"u8, size: 24, pivot: new(0.00f, 0.50f));

										if (collapsible.Inner())
										{

										}
									}
								}

								if (SideMenu.show_interact)
								{
									using (var collapsible = GUI.Collapsible2.New("col.interact"u8, size: new(GUI.RmX, 32), default_open: true))
									{
										GUI.TitleCentered("Interactions"u8, size: 24, pivot: new(0.00f, 0.50f));

										if (collapsible.Inner())
										{
											using (var group_interactables = GUI.Group.New(size: new(GUI.RmX, 0)))
											{
												//ref var region = ref region_common.AsRegion();
												if (region.IsNotNull())
												{
													//ref var character = ref region.GetCharacter(h_character, out var ent_character);
													if (character.IsNotNull())
													{
														var results_span = FixedArray.CreateSpan32NoInit<OverlapResult>(out var results_buffer);
														if (region.TryOverlapPointAll(world_position: pos_cached_interact, radius: Interactor.c_max_distance,
														hits: ref results_span, mask: Physics.Layer.Interactable, exclude: Physics.Layer.Ignore_Hover))
														{
															results_span.Sort(static (a, b) => a.entity.lower.CompareToFast(b.entity.lower));
															//results_span.SortByDistance();
															var ent_controlled = character.ent_controlled;
															var ent_prev = Entity.None;

															foreach (ref var result in results_span)
															{
																//ref var faction = ref ent_squad.GetComponent<Faction.Data>();
																//if (faction.IsNotNull() && faction.id == player.faction_id)
																//{

																var ent_result = result.entity;
																if (ent_result == ent_controlled) continue;
																if (ent_result == ent_prev) continue;

																const float slot_size = 56.00f;

																GUI.TrySameLine(slot_size);

																using (var push_id = GUI.ID<Interactable.Data, Selection.Data>.Push(ent_result))
																//using (var group_row = GUI.Group.New(new Vector2(GUI.RmX, 24)))
																using (var group_row = GUI.Group.New(new Vector2(slot_size)))
																{
																	//group_row.DrawBackground(GUI.tex_panel);
																	//GUI.TitleCentered(ent_result.GetName(), pivot: new(0.00f, 0.50f), offset: new(6, 0));

																	var selected = ent_result == Interactable.GetCurrentTarget();
																	group_row.DrawBackground(selected ? GUI.tex_slot_white_hover : GUI.tex_slot_white, color: GUI.col_frame);
																	//group_row.DrawBackground(GUI.tex_slot_simple);
																	GUI.DrawEntityIcon(ent_result, draw_tooltip: false);

																	if (GUI.Selectable3("select"u8, group_row.GetOuterRect(), selected: selected))
																	{
																		if (selected)
																		{
																			Interactable.Close();
																		}
																		else
																		{
																			Interactable.Open(ent_result);
																		}

																		//var rpc = new Selection.SelectSquadRPC()
																		//{
																		//	ent_squad = ent_squad
																		//};
																		//rpc.Send(this.ent_selection);

																		//dropdown.Close();
																	}
																}

																if (GUI.IsItemHovered(out var rect_hovered))
																{
																	using (var tooltip = GUI.Tooltip.New(rect: rect_hovered, anchor: GUI.Anchor.Left, pivot: new(0.50f, 0.50f), size: new(0, 0), offset: new(0, 0)))
																	{
																		var has_inventories = false;
																		var inventories = ent_result.GetInventories();
																		using (var group_inventories = GUI.Group.New(size: new(0, 0)))
																		{
																			foreach (var h_inventory in inventories)
																			{
																				if (!h_inventory.Flags.HasAny(Inventory.Flags.Hidden))
																				{
																					if (has_inventories) GUI.NewLine(8);
																					has_inventories = true;

																					GUI.DrawSmallInventory(h_inventory, is_readonly: true, text_color: GUI.font_color_default, icon_color: Color32BGRA.White);
																				}
																			}
																		}

																		if (has_inventories)
																		{
																			GUI.SameLine(8);
																		}

																		using (var group_info = GUI.Group.New(size: new(48 * 4, 0)))
																		using (GUI.Wrap.Push(GUI.RmX))
																		{
																			if (ent_result.TryGetPrefab(out var prefab))
																			{
																				var root = prefab.root;
																				if (root is not null)
																				{
																					GUI.TextShaded(root.Name.OrDefault(prefab.GetName));
																					GUI.TextShaded(root.Description, color: GUI.font_color_default.WithColorMult(0.75f));
																				}
																			}

																			GUI.HighlightEntity(ent_result, color: GUI.col_button_yellow.WithAlpha(128), layer: GUI.Layer.Background);
																			GUI.DrawEntityMarker(ent_result);
																		}


																		//GUI.Title(ent_result.GetName());
																	}
																}

																// to avoid duplicates on entities that have more than 1 shape
																ent_prev = ent_result;
																//}
															}
														}
													}
												}
											}
										}
									}
								}

								if (SideMenu.show_pickups)
								{
									using (var collapsible = GUI.Collapsible2.New("col.pickups"u8, size: new(GUI.RmX, 32), default_open: true))
									{
										GUI.TitleCentered("Nearby Items"u8, size: 24, pivot: new(0.00f, 0.50f));

										if (collapsible.Inner())
										{
											using (var group_pickups = GUI.Group.New(size: new(GUI.RmX, 0)))
											{
												//ref var region = ref region_common.AsRegion();
												if (region.IsNotNull())
												{
													//ref var character = ref region.GetCharacter(h_character, out var ent_character);
													if (character.IsNotNull())
													{
														var max_pickup_distance = 2.50f;
														var results_span = FixedArray.CreateSpan32NoInit<OverlapResult>(out var results_buffer);
														if (region.TryOverlapPointAll(world_position: pos_cached_pickup, radius: max_pickup_distance,
														hits: ref results_span, require: Physics.Layer.Dynamic | Physics.Layer.Holdable,
														mask: Physics.Layer.Item | Physics.Layer.Resource | Physics.Layer.Furniture | Physics.Layer.Attachable | Physics.Layer.Wheel | Physics.Layer.Crate | Physics.Layer.Shipment | Physics.Layer.Decoration | Physics.Layer.Vehicle | Physics.Layer.Background,
														exclude: Physics.Layer.Ignore_Hover | Physics.Layer.Stored | Physics.Layer.Static | Physics.Layer.World | Physics.Layer.Attached | Physics.Layer.Zone | Physics.Layer.Bounds | Physics.Layer.Gas | Physics.Layer.Fire | Physics.Layer.Water | Physics.Layer.Essence))
														{
															results_span.Sort(static (a, b) => a.entity.lower.CompareToFast(b.entity.lower));
															//results_span.SortByDistance();
															var ent_controlled = character.ent_controlled;
															var ent_prev = Entity.None;
															var ent_pickup = character.ent_pickup;
															var ent_pickup_target = character.ent_pickup_target;

															foreach (ref var result in results_span)
															{
																//ref var faction = ref ent_squad.GetComponent<Faction.Data>();
																//if (faction.IsNotNull() && faction.id == player.faction_id)
																//{

																var ent_result = result.entity;
																if (ent_result == ent_controlled) continue;
																if (ent_result == ent_prev) continue;

																var ent_result_parent = result.entity_parent;
																if (ent_result_parent != 0 && ent_result != ent_pickup_target) continue;

																const float slot_size = 48.00f;

																GUI.TrySameLine(slot_size);

																using (var push_id = GUI.ID<Holdable.Data, Selection.Data>.Push(ent_result))
																//using (var group_row = GUI.Group.New(new Vector2(GUI.RmX, 24)))
																using (var group_row = GUI.Group.New(new Vector2(slot_size)))
																{
																	//group_row.DrawBackground(GUI.tex_panel);
																	//GUI.TitleCentered(ent_result.GetName(), pivot: new(0.00f, 0.50f), offset: new(6, 0));

																	var h_material = result.GetMaterialHandle();
																	ref var material_data = ref h_material.GetData();

																	var selected = ent_result == ent_pickup_target; // Interactable.GetCurrentTarget();

																	//group_row.DrawBackground(selected ? GUI.tex_slot_white_hover : GUI.tex_slot_white, color: selected ? GUI.col_button_ok : GUI.col_frame);
																	group_row.DrawBackground(selected ? GUI.tex_slot_white_hover : GUI.tex_slot_white, color: GUI.col_frame);
																	GUI.DrawEntityIcon(ent_result, draw_tooltip: false);

																	if (result.layer.HasAny(Physics.Layer.Destructible))
																	{
																		ref var health = ref ent_result.GetComponent<Health.Data>();
																		if (health.IsNotNull())
																		{
																			//var rect_health = group_row.GetOuterRect().Pad(l: 6, u: 6, r: 6);
																			//rect_health.

																			var rect_health = group_row.GetInnerRect().GetPaddedRect(inner_size: new(40, 6), pivot: new(0.50f, 1.00f));
																			//rect_health.bottom -= 6;

																			var health_val = health.GetHealthNormalized();

																			rect_health.width *= health_val;
																			GUI.Draw9Slice(GUI.tex_bar_solid_01, padding: new(4), rect: rect_health, color: Color32BGRA.RedGreen(health_val.Pow2()));

																			//GUI.DrawRect(rect_health, color: Color32BGRA.Red, layer: GUI.Layer.Window);
																		}
																	}

																	if (GUI.Selectable3("select"u8, group_row.GetOuterRect(), selected: selected))
																	{
																		if (selected)
																		{
																			if (h_material && GUI.GetKeyboard().GetKey(Keyboard.Key.LeftShift))
																			{
																				var h_inventory = character.GetInventory();
																				Inventory.SendDeposit(index: null, resource_ent: ent_result, storage_dst_ent: h_inventory.Entity, storage_dst_id: h_inventory.ID, amount: 50000.00f);
																			}
																			else
																			{
																				Arm.SendDrop(ent_parent: ent_pickup, direction: default);
																			}
																		}
																		else
																		{
																			if (ent_pickup_target != 0)
																			{
																				Arm.SendDrop(ent_parent: ent_pickup, ent_target: ent_result, direction: default);
																				//Inventory.send
																				//Arm.SendPickup(ent_parent: ent_pickup, ent_target: ent_result, local_position: default, drop_held: true);

																				//Arm.SendDrop(ent_parent: ent_pickup, ent_target: ent_result, direction: default)
																				//	.ContinueWith(() => Arm.SendPickup(ent_parent: ent_pickup, ent_target: ent_result, local_position: default, drop_held: true));
																			}
																			else
																			{
																				if (h_material && GUI.GetKeyboard().GetKey(Keyboard.Key.LeftShift))
																				{
																					var h_inventory = character.GetInventory();
																					Inventory.SendDeposit(index: null, resource_ent: ent_result, storage_dst_ent: h_inventory.Entity, storage_dst_id: h_inventory.ID, amount: 50000.00f);
																				}
																				else
																				{
																					Arm.SendPickup(ent_parent: ent_pickup, ent_target: ent_result, local_position: default, drop_held: true);
																				}
																			}
																		}

																		//var rpc = new Selection.SelectSquadRPC()
																		//{
																		//	ent_squad = ent_squad
																		//};
																		//rpc.Send(this.ent_selection);

																		//dropdown.Close();
																	}
																}

																if (GUI.IsItemHovered(out var rect_hovered))
																{
																	using (var tooltip = GUI.Tooltip.New(rect: rect_hovered, anchor: GUI.Anchor.Left, pivot: new(0.50f, 0.50f), size: new(0, 0), offset: new(0, 0)))
																	{
																		var has_inventories = false;
																		if (result.layer.HasNone(Physics.Layer.Resource))
																		{
																			var inventories = ent_result.GetInventories();
																			using (var group_inventories = GUI.Group.New(size: new(0, 0)))
																			{
																				foreach (var h_inventory in inventories)
																				{
																					if (!h_inventory.Flags.HasAny(Inventory.Flags.Hidden))
																					{
																						if (has_inventories) GUI.NewLine(8);
																						has_inventories = true;

																						GUI.DrawSmallInventory(h_inventory, is_readonly: true, text_color: GUI.font_color_default, icon_color: Color32BGRA.White);
																					}
																				}
																			}

																			if (has_inventories)
																			{
																				GUI.SameLine(8);
																			}
																		}

																		using (var group_info = GUI.Group.New(size: new(48 * 4, 0)))
																		using (GUI.Wrap.Push(GUI.RmX))
																		{
																			if (ent_result.TryGetPrefab(out var prefab))
																			{
																				var root = prefab.root;
																				if (root is not null)
																				{
																					GUI.TextShaded(root.Name.OrDefault(prefab.GetName));
																					GUI.TextShaded(root.Description, color: GUI.font_color_default.WithColorMult(0.75f));
																				}
																			}

																			GUI.HighlightEntity(ent_result, color: GUI.col_button_yellow.WithAlpha(128), layer: GUI.Layer.Background);
																			GUI.DrawEntityMarker(ent_result);
																		}


																		//GUI.Title(ent_result.GetName());
																	}
																}

																// to avoid duplicates on entities that have more than 1 shape
																ent_prev = ent_result;
																//}
															}
														}
													}
												}
											}
										}
									}
								}

								Ext_DrawScrollbox_Bottom(ref this, in window, in scrollbox);
							}
						}
					}
				}

			}
		}

		//[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Global | ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Owned)]
		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Global)]
		public static void OnGUI_SideMenu(ISystem.Info.Global info, ref Region.Data.Global region, [Source.Owned] ref World.Global world)
		{
			if (true)
			{
				var gui = new SideMenuGUI()
				{
					//ent_player = ent_player,
				};
				gui.Submit();
			}
		}
#endif
	}
}

