namespace TC2.Base.Components
{
	public static partial class Logistics
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Global | IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,


			}

			public required Logistics.Data.Flags flags;
			[Save.Force] public required byte linked_capacity;

			[Save.NewLine]
			[Save.Force] public required BitField<Inventory.Type> filter_inventory_types;
			public Filter.Mask2x<Inventory.Flags> filter_inventory_flags;
			//public Filter.Mask2x<Material.Flags> filter_material_flags;

			[Save.NewLine]
			[Save.Force, Editor.Picker.Box] public required AABB search_rect;

			[Save.NewLine, Asset.Ignore]
			public FixedArray16<Entity> linked_ents;
		}

		public struct EditRPC: Net.IRPC<Logistics.Data>
		{
			public Filter.Mask2x<Inventory.Flags>? edit_filter_inventory_flags;
			//public Filter.Mask2x<Material.Flags>? edit_filter_material_flags;


#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Logistics.Data data)
			{
				var sync = false;

				if (sync)
				{
					rpc.Sync(ref data, true);
				}
			}
#endif
		}

		public struct LinkRPC: Net.IRPC<Logistics.Data>
		{
			public required Entity ent_inventory;
			public required byte index_link;

			//public IComponent.Handle h_inventory;


#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Logistics.Data data)
			{
				Assert.IsIndexInRange(this.index_link, data.linked_capacity);

				var sync = false;

				var span_linked = data.linked_ents.AsSpan(data.linked_capacity);

				if (this.ent_inventory)
				{
					var index_old = span_linked.IndexOf(this.ent_inventory);
					if (index_old >= 0)
					{
						(span_linked[index_old], span_linked[this.index_link]) = (span_linked[this.index_link], span_linked[index_old]);
						sync |= true;
					}
					else
					{
						sync |= span_linked[this.index_link].TrySet(this.ent_inventory);
					}
					//Assert.Check(!span_linked.Contains(this.ent_inventory));
					//sync |= span_linked[this.index_link].TrySet(this.ent_inventory);
				}
				else
				{
					sync |= span_linked[this.index_link].TryReset();
				}

				if (sync)
				{
					rpc.Sync(ref data, true);
				}
			}
#endif
		}

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_logistics,
		[Source.Owned] ref Logistics.Data logistics,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Transform.Data transform,
		[Source.Owned, Optional] in Faction.Data faction, [Source.Owned, Optional] in Company.Data company)
		{

		}

#if CLIENT
		public struct LogisticsGUI: IGUICommand
		{
			public Entity ent_logistics;

			public Logistics.Data logistics;
			public Transform.Data transform;

			public IFaction.Handle h_faction;
			public ICompany.Handle h_company;

			public static Entity ent_storage_hovered;
			public static Entity ent_storage_hovered_cached;

			public static Entity ent_storage_selected;
			public static Entity ent_storage_selected_cached;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction(identifier: "Logistics Station"u8, entity: this.ent_logistics,
				tooltip_tab: "Manage linked storages."))
				{
					this.StoreCurrentWindowTypeID(order: -1000);
					if (window.show)
					{
						ref var region_common = ref this.ent_logistics.GetRegionCommon();
						var span_linked = this.logistics.linked_ents.Slice(this.logistics.linked_capacity);
						var h_character_client = Client.GetCharacterHandle();

						//ref var body_self = ref this.ent_logistics.gettra

						//var rect_body_self = this.ent_logistics.rect

						(ent_storage_hovered, ent_storage_hovered_cached) = (Entity.Empty, ent_storage_hovered);

						using (var group_left = GUI.Group.New(size: new(298 - 48, GUI.RmY), padding: new(6)))
						{
							group_left.DrawBackground(GUI.tex_window);

							//var count = this.logistics.linked_capacity;

							for (var i = 0; i < span_linked.Length; i++)
							{
								using (var id = GUI.ID<Logistics.Data, Entity>.Push(i))
								using (var group_row = GUI.Group.New(size: new(GUI.RmX, 32)))
								{
									if (group_row.IsVisible())
									{
										var ent_storage = span_linked[i];
										var ent_storage_tmp = ent_storage;
										var is_alive = ent_storage.IsAlive();

										if (ent_storage != 0 && (ent_storage == ent_storage_hovered_cached || ent_storage == ent_storage_selected_cached))
										{
											//GUI.DrawRect(rect, color: GUI.font_color_orange, layer: GUI.Layer.Window);
											GUI.DrawRectFilled(group_row.GetOuterRect(), color: GUI.col_default, layer: GUI.Layer.Window);
										}

										if (GUI.EntityPicker(identifier: "picker.logi"u8, name: "Link"u8, size: new(52, GUI.RmY),
										region_id: region_common.GetID(), entity: ref ent_storage_tmp, color: is_alive ? GUI.col_button_yellow : GUI.col_button,
										layer_require: Physics.Layer.Static, 
										layer_include: Physics.Layer.Storage | Physics.Layer.Workshop | Physics.Layer.Crafter, 
										layer_exclude: Physics.Layer.Resource | Physics.Layer.World | Physics.Layer.Ignore_Hover | Physics.Layer.Zone | Physics.Layer.Conveyor | 
										Physics.Layer.Belt | Physics.Layer.Pipe | Physics.Layer.Construction | Physics.Layer.Dynamic))
										{
											App.WriteValue(ent_storage_tmp);

											var rpc = new Logistics.LinkRPC
											{
												ent_inventory = ent_storage_tmp,
												index_link = (byte)i
											};
											rpc.Send(this.ent_logistics);
										}

										GUI.SameLine();

										using (var group_info = GUI.Group.New(size: GUI.Rm.SubX(GUI.RmY)))
										using (GUI.Wrap.Push(GUI.RmX))
										{
											var color = Color32BGRA.GUI;
											group_info.DrawBackground(GUI.tex_slot_white, color: color.WithAlpha(192).WithColorDiv(Maths.Factor.x2));

											//GUI.DrawBackground(GUI.tex_slot, rect: GUI.GetRemainingRect(), padding: new(4));

											if (is_alive)
											{
												//var name_a = is_alive ? ent_picker_tmp.GetName() : "<none>";
												//var name_b = is_alive ? ent_picker_tmp.GetPrefabName() : null;

												var name_a = ent_storage.GetName();
												var name_b = ent_storage.GetPrefabName();
												if (string.Equals(name_a, name_b, StringComparison.Ordinal)) name_b = null;

												GUI.TitleCentered(name_a, pivot: new(0.00f, 0.00f), offset: new(6, 2));
												if (name_b is not null) GUI.TextShadedCentered(name_b, pivot: new(0.00f, 1.00f), offset: new(6, -2), size: 14);
											}
										}
										
										if (GUI.Selectable3(id, rect: GUI.GetLastItemRect(), selected: ent_storage != 0 && ent_storage == ent_storage_selected_cached))
										{
											ent_storage_selected_cached.Toggle(ent_storage);
										}

										GUI.SameLine();

										if (GUI.DrawIconButton("btn.rem"u8, sprite: GUI.tex_button_sub, size: new(GUI.RmY), error: !is_alive))
										{
											var rpc = new Logistics.LinkRPC
											{
												ent_inventory = Entity.Empty,
												index_link = (byte)i
											};
											rpc.Send(this.ent_logistics);
										}
									}
								}

								if (GUI.IsItemHovered(out var rect))
								{
									ent_storage_hovered = span_linked[i];
									//GUI.DrawRect(rect, color: GUI.font_color_orange, layer: GUI.Layer.Window);
								}
							}
						}

						GUI.SameLine();

						var ts = Timestamp.Default;
						var ts_elapsed = 0.00;
						var total_inventories = 0;

						using (var group_right = GUI.Group.New(size: GUI.Rm))
						{
							using (var group_top = GUI.Group.New(size: GUI.Rm.SubY(48)))
							{
								using (var scroll = GUI.Scrollbox.New("scroll.inventories"u8, size: GUI.Rm.SubX(96), padding: new(4)))
								{
									//group_right.DrawBackground(GUI.tex_window);
									//scroll.group_frame.DrawBackground(GUI.tex_window);

									//var count = this.logistics.linked_capacity;

									ts = Timestamp.Now();
									for (var i = 0; i < span_linked.Length; i++)
									{
										var ent_storage = span_linked[i];
										if (ent_storage.TryGetRecord(out var rec))
										{
											//var draw_separator = false;

											foreach (var inventory in ent_storage.GetInventories())
											{
												//if (inventory.IsAccessible(h_character_client))

												if (this.logistics.filter_inventory_types.Has(inventory.Type) && inventory.Flags.Evaluate(this.logistics.filter_inventory_flags)) //.HasAnyExcept(Inventory.Flags.Public, Inventory.Flags.Hidden))
												{
													var frame_size_pref = inventory.GetPreferedFrameSize();

													if (total_inventories != 0) GUI.TrySameLine(frame_size_pref.X);
													//GUI.DrawRect(AABB.Simple(GUI.GetCursorScreenPos(), frame_size_pref), layer: GUI.Layer.Foreground);

													using (var group_inventory = GUI.Group.New(size: frame_size_pref))
													{
														if (group_inventory.IsVisible())
														{
															if (ent_storage != 0 && (ent_storage == ent_storage_hovered_cached || ent_storage == ent_storage_selected_cached))
															{
																//GUI.DrawRect(rect, color: GUI.font_color_orange, layer: GUI.Layer.Window);
																GUI.DrawRectFilled(group_inventory.GetOuterRect(), color: GUI.col_default.WithAlpha(100), layer: GUI.Layer.Window);
															}

															GUI.DrawInventory(inventory);
														}
													}

													//var last_rect = GUI.GetLastItemRect();
													if (GUI.IsItemHovered(out var rect))
													{
														ent_storage_hovered = ent_storage;
														//GUI.DrawRect(rect, color: GUI.font_color_orange, layer: GUI.Layer.Window);
													}

													//if (ent_linked == ent_storage_hovered_cached)
													//{
													//	//GUI.DrawRect(rect, color: GUI.font_color_orange, layer: GUI.Layer.Window);
													//	GUI.DrawRectFilled(rect, color: GUI.font_color_yellow_b.WithAlpha(16), layer: GUI.Layer.Window);
													//}

													//draw_separator = true;
													total_inventories++;
												}
											}

											//if (draw_separator) GUI.SeparatorThick();
										}
									}
									ts_elapsed = ts.GetMilliseconds();
								}

								GUI.SameLine();

								using (var group_side = GUI.Group.New(size: GUI.Rm))
								{
									if (this.ent_logistics.TryGetInventory2(Inventory.Type.Buffer, out var inventory_buffer))
									{
										GUI.DrawInventory(inventory_buffer);
									}
								}
							}

							GUI.SeparatorThick();

							GUI.TextShaded($"{total_inventories} inventories in {ts_elapsed:0.000} ms");
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Global | ISystem.Scope.Region)]
		public static void OnGUI([Source.Owned] in Interactable.Data interactable, Entity ent_logistics,
		[Source.Owned] in Logistics.Data logistics, [Source.Owned] in Transform.Data transform,
		[Source.Owned, Optional] in Faction.Data faction, [Source.Owned, Optional] in Company.Data company)
		{
			if (interactable.IsActive())
			{
				var gui = new LogisticsGUI()
				{
					ent_logistics = ent_logistics,

					logistics = logistics,
					transform = transform,

					h_faction = faction.id,
					h_company = company.h_company,
				};
				gui.Submit();
			}
		}
#endif
	}
}
