namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		public static partial class Mode
		{
			public static partial class Conveyors
			{
				[IComponent.Data(Net.SendType.Reliable, name: "Wrench (Conveyors)", region_only: true)]
				public partial struct Data: IComponent, Wrench.IMode, Wrench.ILinkerMode<Conveyors.TargetInfo, Conveyor.Data>
				{
					[Save.Ignore] public Entity ent_src;
					[Save.Ignore] public Entity ent_dst;

					[Save.Ignore] public IComponent.Handle inventory_id_src;
					[Save.Ignore] public IComponent.Handle inventory_id_dst;

					[Asset.Ignore] public IRecipe.Handle selected_recipe;
					//public Belt.Flags flags;

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 24, 24, 1, 0);
					public static string Name { get; } = "Conveyors";

					public Crafting.Recipe.Type RecipeType => Crafting.Recipe.Type.Conveyor;
					public Physics.Layer LayerMask => Physics.Layer.Conveyor;
					public Color32BGRA ColorOk => Color32BGRA.Green;
					public Color32BGRA ColorError => Color32BGRA.Red;
					public Color32BGRA ColorNew => Color32BGRA.Yellow;

					public Entity EntitySrc => this.ent_src;
					public Entity EntityDst => this.ent_dst;

					public readonly IComponent.Handle ComponentSrc => this.inventory_id_src;
					public readonly IComponent.Handle ComponentDst => this.inventory_id_dst;
					public readonly IRecipe.Handle SelectedRecipe => this.selected_recipe;

					public TargetInfo CreateTargetInfo(ref Region.Data.Common region, Entity entity, IComponent.Handle h_component, Vector2 pos, bool is_src)
					{
						return new TargetInfo(entity, is_src ? this.inventory_id_src : this.inventory_id_dst, is_src);
					}

#if CLIENT
					public void SendSetTargetRPC(Entity ent_wrench, Entity ent_src, IComponent.Handle h_component_src, Entity ent_dst, IComponent.Handle h_component_dst)
					{
						var rpc = new SetTargetRPC
						{
							ent_src = ent_src,
							ent_dst = ent_dst,

							h_component_src = h_component_src,
							h_component_dst = h_component_dst,
						};
						rpc.Send(ent_wrench);
					}

					public void SendSetRecipeRPC(Entity ent_wrench, IRecipe.Handle recipe)
					{
						var rpc = new EditRPC
						{
							recipe = recipe
						};
						rpc.Send(ent_wrench);
					}

					public void DrawInfo(Entity ent_wrench, ref TargetInfo info_src, ref TargetInfo info_dst, Build.Errors errors_src, Build.Errors errors_dst, float distance)
					{
						ref var recipe = ref this.selected_recipe.GetData();
						if (!recipe.IsNull() && recipe.placement.HasValue)
						{
							using (GUI.Group.New(size: new(GUI.RmX, 28), padding: new(4, 2)))
							{
								GUI.TitleCentered(recipe.GetName(), size: 24, pivot: new Vector2(0.00f, 0.50f));
							}

							GUI.SeparatorThick();

							using (GUI.Group.New(size: GUI.Rm, padding: new(4, 6)))
							{
								using (GUI.Wrap.Push(GUI.RmX))
								{
									GUI.TextShaded(recipe.GetDescription().OrDefault(recipe.GetDescriptionFallback()), color: GUI.font_color_desc);

									GUI.NewLine();

									if (recipe.products[0].type == Crafting.Product.Type.Prefab && recipe.products[0].prefab.TryGetPrefab(out var prefab))
									{
										var root = prefab.root;
										if (root != null)
										{
											GUI.DrawStats(root, priority_min: Statistics.Priority.Low);
										}
									}
								}
							}
						}
					}

					public void DrawHUD(Entity ent_wrench, ref TargetInfo info_src, ref TargetInfo info_dst, Build.Errors errors_src, Build.Errors errors_dst, float distance)
					{
						ref var recipe = ref this.selected_recipe.GetData();
						if (!recipe.IsNull() && recipe.placement.HasValue)
						{
							ref var region = ref Client.GetRegionCommon();
							var placement = recipe.placement.Value;
							var pos = ((info_src.pos + info_dst.pos) * 0.50f);

							//using (var hud = GUI.Window.Standalone("Wrench.HUD", position: (info_src.pos - new Vector2(0.00f, info_src.radius + 0.25f)).WorldToCanvas(), size: new(168, 100), pivot: new(0.50f, 1.00f)))
							//using (var hud = GUI.Window.Standalone("Wrench.HUD", position: (info_dst.pos - new Vector2(0.00f, info_dst.radius + 0.25f)).WorldToCanvas(), size: new(168, 100), pivot: new(0.50f, 1.00f)))
							using (var hud = GUI.Window.Standalone("Wrench.HUD"u8, position: ((info_src.pos + info_dst.pos) * 0.50f).WorldToCanvas(), size: new(300, 0), pivot: new(0.50f, 0.50f), force_position: false))
							{
								if (hud.show)
								{
									Crafting.Context.NewFromCharacter(region: ref region, h_character: Client.GetCharacterHandle(), ent_producer: ent_wrench, context: out var context);

									//GUI.DrawBackground(GUI.tex_window, hud.group.GetOuterRect(), padding: new(4));
									GUI.DrawWindowBackground(GUI.tex_window, padding: new Vector4(4));

									var hud_rect = hud.group.GetOuterRect();

									var inventories_src = info_src.entity.GetInventories();
									var inventories_dst = info_dst.entity.GetInventories();

									var sync = false;

									var inventory_filter = new Filter.Mask<Material.Flags>();

									if (recipe.products[0].type == Crafting.Product.Type.Prefab && recipe.products[0].prefab.TryGetPrefab(out var prefab))
									{
										var root = prefab.root;
										if (root != null)
										{
											if (root.TryGetComponentData<Conveyor.Data>(out var duct_data, true))
											{
												inventory_filter = duct_data.filter;
											}
										}
									}

									if (this.ent_src.TryGetInventory(this.inventory_id_src, out var h_inv_src))
									{
										//var h_inventory = new Inventory.Handle()
										using (var window = GUI.Window.Standalone("inv_src"u8, position: new(hud_rect.a.X, hud_rect.a.Y), pivot: new(1.00f, 0.00f), size: h_inv_src.GetPreferedFrameSize() + new Vector2(0, 0)))
										{
											if (window.show)
											{
												GUI.DrawInventory(h_inv_src, is_readonly: true, filter: inventory_filter);
											}
										}
									}

									if (this.ent_dst.TryGetInventory(this.inventory_id_dst, out var h_inv_dst))
									{
										//var h_inventory = new Inventory.Handle()
										using (var window = GUI.Window.Standalone("inv_dst"u8, position: new(hud_rect.b.X, hud_rect.a.Y), pivot: new(0.00f, 0.00f), size: h_inv_dst.GetPreferedFrameSize() + new Vector2(0, 0)))
										{
											if (window.show)
											{
												GUI.DrawInventory(h_inv_dst, is_readonly: true);
											}
										}
									}

									using (GUI.Group.New(size: new(GUI.RmX, 24 * 6), padding: new(4)))
									{
										//GUI.LabelShaded("Distance:", distance, $"{{0:0.00}}/{placement.length_max:0.00} m");

										var w = GUI.RmX;

										using (GUI.ID.Push(this.ent_src))
										{
											using (var group_col = GUI.Group.New(size: new Vector2(w * 0.50f, GUI.RmY), padding: new(4)))
											{
												GUI.DrawBackground(GUI.tex_panel, group_col.GetOuterRect(), new Vector4(4));

												using (var group_row = GUI.Group.New(size: new Vector2(GUI.RmX, 24), padding: new(4)))
												{
													GUI.TitleCentered($"From: {this.ent_src.GetName()}", size: 16, pivot: new(0.50f, 0.50f));
												}

												GUI.SeparatorThick();

												DrawInventories(ref inventories_src, ref this.inventory_id_src.AsU64(), ref sync);
											}
										}

										GUI.SameLine();

										using (GUI.ID.Push(this.ent_dst))
										{
											using (var group_col = GUI.Group.New(size: new Vector2(w * 0.50f, GUI.RmY), padding: new(4)))
											{
												GUI.DrawBackground(GUI.tex_panel, group_col.GetOuterRect(), new Vector4(4));

												using (var group_row = GUI.Group.New(size: new Vector2(GUI.RmX, 24), padding: new(4)))
												{
													GUI.TitleCentered($"To: {this.ent_dst.GetName()}", size: 16, pivot: new(0.50f, 0.50f));
												}

												GUI.SeparatorThick();

												DrawInventories(ref inventories_dst, ref this.inventory_id_dst.AsU64(), ref sync);
											}
										}
									}

									if (sync)
									{
										var rpc = new SetTargetRPC
										{
											ent_src = this.ent_src,
											ent_dst = this.ent_dst,

											h_component_src = this.inventory_id_src,
											h_component_dst = this.inventory_id_dst,
										};
										rpc.Send(ent_wrench);
									}

									var errors = errors_src | errors_dst;

									using (GUI.Group.New(size: GUI.Rm, padding: new(4, 4)))
									{
										using (GUI.Group.New2(size: new(GUI.RmX, GUI.RmY - 40), padding: new(6, 0, 6, 4)))
										{
											GUI.Title("Requires"u8);
											GUI.SeparatorThick();
											GUI.NewLine(4);

											var has_reqs = GUI.DrawRequirements(context: ref context, requirements: recipe.requirements.AsSpan(), amount_multiplier: 1.00f + MathF.Ceiling(distance));
											//var has_reqs = GUI.DrawRequirements(ref region, ent_wrench, ref Client.GetPlayer(), world_position: pos, requirements: recipe.requirements.AsSpan(), amount_multiplier: 1.00f + MathF.Ceiling(distance));
											if (!has_reqs) errors |= Build.Errors.RequirementsNotMet;
										}

										using (GUI.Group.Centered(outer_size: GUI.Rm, inner_size: new(100, 40)))
										{
											if (GUI.DrawButton("Create"u8, new Vector2(100, 40), enabled: info_src.valid && info_dst.valid, error: errors != Build.Errors.None, color: GUI.col_button_ok))
											{
												var rpc = new Wrench.Mode.Conveyors.ConfirmRPC()
												{

												};
												rpc.Send(ent_wrench);
											}
											if (GUI.IsItemHovered())
											{
												using (GUI.Tooltip.New(size: new(244, 0)))
												{
													//GUI.Title("Requires");
													//GUI.SeparatorThick();
													//GUI.NewLine(4);
													//var has_reqs = GUI.DrawRequirements(ref region, ent_wrench, ref Client.GetPlayer(), world_position: pos, requirements: recipe.requirements.AsSpan(), amount_multiplier: 1.00f + distance);
													//if (!has_reqs) errors |= Build.Errors.RequirementsNotMet;

													using (GUI.Wrap.Push(GUI.RmX))
													{
														if (errors != Build.Errors.None)
														{
															GUI.TextShaded(errors.ToFormattedString("* {0}!", "\n"), color: GUI.font_color_error);
														}
													}
												}
											}
										}
									}

									//using (GUI.Group.Centered(outer_size: new(GUI.RmX, 40), inner_size: new(100, 40)))
									//{
									//	if (GUI.DrawButton("Create", new Vector2(100, 40), enabled: info_src.valid && info_dst.valid, error: errors != Build.Errors.None, color: GUI.col_button_ok))
									//	{
									//		var rpc = new Wrench.Mode.Conveyors.ConfirmRPC()
									//		{

									//		};
									//		rpc.Send(ent_wrench);
									//	}
									//	if (GUI.IsItemHovered())
									//	{
									//		using (GUI.Tooltip.New(size: new(244, 0)))
									//		{
									//			GUI.Title("Requires");
									//			GUI.SeparatorThick();
									//			GUI.NewLine(4);
									//			var has_reqs = GUI.DrawRequirements(ref region, ent_wrench, ref Client.GetPlayer(), world_position: pos, requirements: recipe.requirements.AsSpan(), amount_multiplier: 1.00f + MathF.Ceiling(distance));
									//			if (!has_reqs) errors |= Build.Errors.RequirementsNotMet;

									//			using (GUI.Wrap.Push(GUI.RmX))
									//			{
									//				if (errors != Build.Errors.None)
									//				{
									//					GUI.TextShaded(errors.ToFormattedString("* {0}!", "\n"), color: GUI.font_color_error);
									//				}
									//			}
									//		}
									//	}

									//	//GUI.DrawHoverTooltip("Create a conveyor connection.");
									//}

									//if (errors != Build.Errors.None)
									//	{
									//		GUI.TextShaded(errors.ToFormattedString("* {0}!", "\n"), color: color_error);
									//	}
								}
							}
						}
					}

					private static void DrawInventories(scoped ref Inventory.Handle.List inventories, scoped ref ulong selected_inventory_id, scoped ref bool sync)
					{
						foreach (var h_inventory in inventories)
						{
							if (h_inventory.Flags.HasAny(Inventory.Flags.Allow_Ducts))
							{
								using (GUI.ID.Push(h_inventory.ID))
								{
									using (var group_row = GUI.Group.New(size: new Vector2(GUI.RmX, 32), padding: new(4)))
									{
										GUI.DrawBackground(GUI.tex_panel, group_row.GetOuterRect(), new Vector4(4));

										GUI.TitleCentered(h_inventory.Type.GetEnumName(), pivot: new(0.50f, 0.50f));

										var is_selected = selected_inventory_id == h_inventory.ID;
										if (GUI.Selectable3(h_inventory.Type.GetEnumName(), group_row.GetOuterRect(), is_selected))
										{
											selected_inventory_id = is_selected ? 0 : h_inventory.ID;
											sync = true;
										}
									}

									//if (GUI.IsItemHovered())
									//{
									//	using (GUI.Tooltip.New(size: h_inventory.GetPreferedFrameSize() + new Vector2(16, 16)))
									//	{
									//		GUI.DrawInventory(h_inventory);
									//	}
									//}
								}
							}
						}
					}

					void Wrench.ILinkerMode<Conveyors.TargetInfo, Conveyor.Data>.DrawGizmos(Entity ent_wrench, ref Vector2 wpos_mouse, ref TargetInfo info_src, ref TargetInfo info_dst, ref TargetInfo info_new, ref Color32BGRA color_src, ref Color32BGRA color_dst, ref Color32BGRA color_new)
					{
						if (info_src.IsSelectable)
						{
							if (!info_new.IsSelectable && !info_dst.IsSelectable)
							{
								var dir = (info_src.Position - wpos_mouse).GetNormalizedFast();

								GUI.DrawLine2((info_src.Position).WorldToCanvas(), (wpos_mouse).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.00f), 8.00f, 8.00f);
							}

							if (info_new.IsSelectable)
							{
								var dir = (info_src.Position - info_new.Position).GetNormalizedFast();

								GUI.DrawLine2((info_src.Position).WorldToCanvas(), (info_new.Position).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.50f), 8.00f, 8.00f);
							}

							if (info_dst.IsSelectable)
							{
								var dir = (info_src.Position - info_dst.Position).GetNormalizedFast();

								GUI.DrawLine2((info_src.Position).WorldToCanvas(), (info_dst.Position).WorldToCanvas(), color_src, color_dst, 8.00f, 8.00f);
							}
						}

						if (info_src.IsSelectable)
						{
							//GUI.DrawCircle(info_src.Position.WorldToCanvas(), info_src.Radius * GUI.GetWorldToCanvasScale(), color_src, 2.00f);
							GUI.DrawEntity(info_src.Entity, color_src.WithAlphaMult(0.50f));
						}

						if (info_dst.IsSelectable)
						{
							//GUI.DrawCircle(info_dst.Position.WorldToCanvas(), info_dst.Radius * GUI.GetWorldToCanvasScale(), color_dst, 2.00f);
							GUI.DrawEntity(info_dst.Entity, color_dst.WithAlphaMult(0.50f));
						}

						if (info_new.IsSelectable)
						{
							//GUI.DrawCircle(info_new.Position.WorldToCanvas(), info_new.Radius * GUI.GetWorldToCanvasScale(), color_new.WithAlphaMult(0.50f), 2.00f);
							GUI.DrawEntity(info_new.Entity, color_new.WithAlphaMult(0.50f));
						}
					}
#endif
				}

				public struct TargetInfo: ITargetInfo
				{
					public Entity entity;
					public IComponent.Handle inventory_id;

					public float radius;
					public Vector2 pos;

					public bool is_src;
					public bool alive;
					public bool valid;
					public bool selectable;

					public readonly Entity Entity => this.entity;
					public readonly IComponent.Handle ComponentID => this.inventory_id;
					public readonly Vector2 Position => this.pos;
					public readonly float Radius => this.radius;
					public readonly bool IsSource => this.is_src;
					public readonly bool IsSelectable => this.valid;
					public readonly bool IsAlive => this.alive;
					public readonly bool IsValid => this.valid;

					public TargetInfo(Entity entity, ulong inventory_id, bool is_src)
					{
						this.entity = entity;
						this.is_src = is_src;
						this.alive = this.entity.IsAlive();

						if (this.alive)
						{
							this.valid = true;

							this.valid &= this.entity.GetComponent<Transform.Data>().TryGetValueFromRef(out var transform);

							var has_inventory = false;

							var inventories = this.entity.GetInventories();
							foreach (var h_inventory in inventories)
							{
								if (h_inventory.Flags.HasAny(Inventory.Flags.Allow_Ducts) && (inventory_id == 0 || h_inventory.ID == inventory_id))
								{
									has_inventory = true;
									this.inventory_id = h_inventory.ID;
									this.pos = transform.LocalToWorld(h_inventory.Offset);

									break;
								}
							}

							this.valid &= has_inventory;

							if (this.valid)
							{
								this.radius = 1.00f;
								//this.pos = this.transform.LocalToWorld(invento;
							}
						}
					}
				}

				public struct SetTargetRPC: Net.IRPC<Wrench.Mode.Conveyors.Data>
				{
					public Entity ent_src;
					public Entity ent_dst;

					public IComponent.Handle h_component_src;
					public IComponent.Handle h_component_dst;

#if SERVER
					public void Invoke(Net.IRPC.Context rpc, ref Wrench.Mode.Conveyors.Data data)
					{
						//App.WriteLine($"{this.ent_src} == {data.ent_src}; {this.ent_dst} == {data.ent_dst}");

						ref var region = ref rpc.entity.GetRegion();

						var info_src = new TargetInfo(this.ent_src, this.h_component_src, true);
						var info_dst = new TargetInfo(this.ent_dst, this.h_component_dst, false);

						data.ent_src = info_src.entity;
						data.ent_dst = info_dst.entity;

						data.inventory_id_src = info_src.inventory_id;
						data.inventory_id_dst = info_dst.inventory_id;

						data.Sync(rpc.entity);
					}
#endif
				}

				public struct EditRPC: Net.IRPC<Wrench.Mode.Conveyors.Data>
				{
					public IRecipe.Handle? recipe;

					public float? throughput_min;
					public float? throughput_max;
					public float? interval;

					public Material.Flags? filter_flags;
					public Material.Type? filter_type;
					public IMaterial.Handle? filter_material;


#if SERVER
					public void Invoke(Net.IRPC.Context rpc, ref Wrench.Mode.Conveyors.Data data)
					{
						if (this.recipe.HasValue)
						{
							data.selected_recipe = this.recipe.Value;
						}

						data.Sync(rpc.entity);
					}
#endif
				}

				public struct ConfirmRPC: Net.IRPC<Wrench.Mode.Conveyors.Data>
				{
#if SERVER
					public void Invoke(Net.IRPC.Context rpc, ref Wrench.Mode.Conveyors.Data data)
					{
						ref var region = ref rpc.entity.GetRegion();
						ref var character = ref rpc.connection.GetCharacter();
						ref var recipe = ref data.selected_recipe.GetData();

						if (!region.IsNull() && !character.IsNull() && !recipe.IsNull())
						{
							var errors = Build.Errors.None;

							var info_src = new TargetInfo(data.ent_src, data.inventory_id_src, true);
							var info_dst = new TargetInfo(data.ent_dst, data.inventory_id_dst, false);

							if (info_src.valid && info_dst.valid)
							{
								var pos_mid = info_src.pos; // (info_src.pos + info_dst.pos) * 0.50f;
								var dir = (info_dst.pos - info_src.pos).GetNormalized(out var distance);

								Crafting.Context.NewFromCharacter(ref region.AsCommon(), rpc.connection.GetCharacterHandle(), rpc.entity, out var context);

								errors |= data.EvaluateNodePair<Wrench.Mode.Conveyors.Data, Wrench.Mode.Conveyors.TargetInfo, Conveyor.Data>(ref region, ref info_src, ref info_dst, ref recipe, out _, character.faction);
								//if (!Crafting.Evaluate(entity, ref player, pos_mid, ref recipe.requirements, amount_multiplier: 1.00f + MathF.Ceiling(distance))) errors |= Build.Errors.RequirementsNotMet;
								if (!context.Evaluate(recipe.requirements.AsSpan(), amount_multiplier: 1.00f + MathF.Ceiling(distance))) errors |= Build.Errors.RequirementsNotMet;

								if (errors == Build.Errors.None)
								{
									//Crafting.Consume(entity, pos_mid, ref recipe.requirements, amount_multiplier: 1.00f + MathF.Ceiling(distance), sync: true);
									context.Consume(recipe.requirements.AsSpan(), amount_multiplier: 1.00f + MathF.Ceiling(distance));

									var arg = (ent_src: info_src.entity, ent_dst: info_dst.entity, inventory_id_src: info_src.inventory_id, inventory_id_dst: info_dst.inventory_id);

									region.SpawnPrefab(recipe.products[0].prefab, pos_mid).ContinueWith(ent =>
									{
										ref var conveyor = ref ent.GetComponent<Conveyor.Data>();
										if (!conveyor.IsNull())
										{
											conveyor.a.Set(arg.ent_src, arg.inventory_id_src);
											conveyor.b.Set(arg.ent_dst, arg.inventory_id_dst);

											ent.MarkModified<Conveyor.Data>(sync: true);

											//conveyor.a.Set(arg.ent_src);
											//conveyor.b.Set(arg.ent_dst);

											//conveyor.a_state.Set(arg.ent_src);
											//conveyor.b_state.Set(arg.ent_dst);

											//conveyor.flags = arg.flags;

											//ent.MarkModified<Conveyor.Data>(sync: true);
										}

										//ent.AddRel2<Conveyor.Link>(arg.ent_src, default, false, false, false);
										//ent.AddRel2<Conveyor.Link>(arg.ent_dst, default, false, false, false);
									});

									data.ent_src = default;
									data.ent_dst = default;

									data.inventory_id_src = default;
									data.inventory_id_dst = default;
								}
							}

							data.Sync(rpc.entity);
						}
					}
#endif
				}
			}
		}
	}
}
