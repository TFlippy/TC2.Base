﻿
using System.Diagnostics.CodeAnalysis;

namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		public static partial class Mode
		{
			public static partial class Conveyors
			{
				[IComponent.Data(Net.SendType.Reliable, name: "Wrench (Conveyors)")]
				public partial struct Data: IComponent, Wrench.IMode, Wrench.ILinkerMode<Conveyors.TargetInfo, Conveyor.Data>
				{
					[Save.Ignore] public Entity ent_src;
					[Save.Ignore] public Entity ent_dst;

					[Save.Ignore] public ulong inventory_id_src;
					[Save.Ignore] public ulong inventory_id_dst;

					public IRecipe.Handle selected_recipe;
					//public Belt.Flags flags;

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 0, 1, 24, 24, 1, 0);
					public static string Name { get; } = "Conveyors";

					public Crafting.Recipe.Tags RecipeTags => Crafting.Recipe.Tags.Conveyor;
					public Physics.Layer LayerMask => Physics.Layer.Conveyor;
					public Color32BGRA ColorOk => Color32BGRA.Green;
					public Color32BGRA ColorError => Color32BGRA.Red;
					public Color32BGRA ColorNew => Color32BGRA.Yellow;

					[UnscopedRef] public ref Entity EntitySrc => ref this.ent_src;
					[UnscopedRef] public ref Entity EntityDst => ref this.ent_dst;
					[UnscopedRef] public ref IRecipe.Handle SelectedRecipe => ref this.selected_recipe;

					public TargetInfo CreateTargetInfo(Entity entity, bool is_src)
					{
						return new TargetInfo(entity, is_src ? this.inventory_id_src : this.inventory_id_dst, is_src);
					}

#if CLIENT
					public void SendSetTargetRPC(Entity ent_wrench, Entity ent_src, Entity ent_dst)
					{
						var rpc = new SetTargetRPC
						{
							ent_src = ent_src,
							ent_dst = ent_dst
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
							using (GUI.Group.New(size: new(GUI.GetRemainingWidth(), 28), padding: new(4, 2)))
							{
								GUI.TitleCentered(recipe.name, size: 24, pivot: new Vector2(0.00f, 0.50f));
							}

							GUI.SeparatorThick();

							using (GUI.Group.New(size: GUI.GetRemainingSpace(), padding: new(4, 6)))
							{
								using (GUI.Wrap.Push(GUI.GetRemainingWidth()))
								{
									GUI.TextShaded(recipe.desc, color: GUI.font_color_desc);

									GUI.NewLine();

									if (recipe.products[0].type == Crafting.Product.Type.Prefab && recipe.products[0].prefab.TryGetPrefab(out var prefab))
									{
										var root = prefab.Root;
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
							ref var region = ref Client.GetRegion();
							var placement = recipe.placement.Value;
							var pos = ((info_src.pos + info_dst.pos) * 0.50f);

							//using (var hud = GUI.Window.Standalone("Wrench.HUD", position: (info_src.pos - new Vector2(0.00f, info_src.radius + 0.25f)).WorldToCanvas(), size: new(168, 100), pivot: new(0.50f, 1.00f)))
							//using (var hud = GUI.Window.Standalone("Wrench.HUD", position: (info_dst.pos - new Vector2(0.00f, info_dst.radius + 0.25f)).WorldToCanvas(), size: new(168, 100), pivot: new(0.50f, 1.00f)))
							using (var hud = GUI.Window.Standalone("Wrench.HUD", position: ((info_src.pos + info_dst.pos) * 0.50f).WorldToCanvas(), size: new(300, 0), pivot: new(0.50f, 0.50f), force_position: false))
							{
								if (hud.show)
								{
									//GUI.DrawBackground(GUI.tex_window, hud.group.GetOuterRect(), padding: new(4));
									GUI.DrawWindowBackground(GUI.tex_window, padding: new Vector4(4));

									var hud_rect = hud.group.GetOuterRect();

									var inventories_src = info_src.entity.GetInventories();
									var inventories_dst = info_dst.entity.GetInventories();

									var sync = false;

									var inventory_filter = new Inventory.Filter();

									if (recipe.products[0].type == Crafting.Product.Type.Prefab && recipe.products[0].prefab.TryGetPrefab(out var prefab))
									{
										var root = prefab.Root;
										if (root != null)
										{
											if (root.TryGetComponentData<Conveyor.Data>(out var duct_data, true))
											{
												inventory_filter.flags = duct_data.filter_flags;
												inventory_filter.type = duct_data.filter_type;
												inventory_filter.material = duct_data.filter_material;
											}
										}
									}

									if (this.ent_src.TryGetInventory(this.inventory_id_src, out var h_inv_src))
									{
										//var h_inventory = new Inventory.Handle()
										using (var window = GUI.Window.Standalone($"inv_src", position: new(hud_rect.a.X, hud_rect.a.Y), pivot: new(1.00f, 0.00f), size: h_inv_src.GetPreferedFrameSize() + new Vector2(0, 0)))
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
										using (var window = GUI.Window.Standalone($"inv_dst", position: new(hud_rect.b.X, hud_rect.a.Y), pivot: new(0.00f, 0.00f), size: h_inv_dst.GetPreferedFrameSize() + new Vector2(0, 0)))
										{
											if (window.show)
											{
												GUI.DrawInventory(h_inv_dst, is_readonly: true, filter: inventory_filter);
											}
										}
									}

									using (GUI.Group.New(size: new(GUI.GetRemainingWidth(), 24 * 6), padding: new(4)))
									{
										//GUI.LabelShaded("Distance:", distance, $"{{0:0.00}}/{placement.length_max:0.00} m");

										var w = GUI.GetRemainingWidth();

										using (GUI.ID.Push(this.ent_src))
										{
											using (var group_col = GUI.Group.New(size: new Vector2(w * 0.50f, GUI.GetRemainingHeight()), padding: new(4)))
											{
												GUI.DrawBackground(GUI.tex_panel, group_col.GetOuterRect(), new Vector4(4));

												using (var group_row = GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), 24), padding: new(4)))
												{
													GUI.TitleCentered($"From: {this.ent_src.GetName()}", size: 16, pivot: new(0.50f, 0.50f));
												}

												GUI.SeparatorThick();

												DrawInventories(ref inventories_src, ref this.inventory_id_src, ref sync);
											}
										}

										GUI.SameLine();

										using (GUI.ID.Push(this.ent_dst))
										{
											using (var group_col = GUI.Group.New(size: new Vector2(w * 0.50f, GUI.GetRemainingHeight()), padding: new(4)))
											{
												GUI.DrawBackground(GUI.tex_panel, group_col.GetOuterRect(), new Vector4(4));

												using (var group_row = GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), 24), padding: new(4)))
												{
													GUI.TitleCentered($"To: {this.ent_dst.GetName()}", size: 16, pivot: new(0.50f, 0.50f));
												}

												GUI.SeparatorThick();

												DrawInventories(ref inventories_dst, ref this.inventory_id_dst, ref sync);
											}
										}
									}

									if (sync)
									{
										var rpc = new SetTargetRPC
										{
											ent_src = this.ent_src,
											ent_dst = this.ent_dst,

											component_id_src = this.inventory_id_src,
											component_id_dst = this.inventory_id_dst,
										};
										rpc.Send(ent_wrench);
									}

									var errors = errors_src | errors_dst;

									using (GUI.Group.New(size: GUI.GetRemainingSpace(), padding: new(4, 4)))
									{
										using (GUI.Group.New2(size: new(GUI.GetRemainingWidth(), GUI.GetRemainingHeight() - 40), padding: new(6, 0, 6, 4)))
										{
											GUI.Title("Requires");
											GUI.SeparatorThick();
											GUI.NewLine(4);

											var has_reqs = GUI.DrawRequirements(ref region, ent_wrench, ref Client.GetPlayer(), world_position: pos, requirements: recipe.requirements.AsSpan(), amount_multiplier: 1.00f + MathF.Ceiling(distance));
											if (!has_reqs) errors |= Build.Errors.RequirementsNotMet;
										}

										using (GUI.Group.Centered(outer_size: GUI.GetRemainingSpace(), inner_size: new(100, 40)))
										{
											if (GUI.DrawButton("Create", new Vector2(100, 40), enabled: info_src.valid && info_dst.valid, error: errors != Build.Errors.None, color: GUI.col_button_ok))
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

													using (GUI.Wrap.Push(GUI.GetRemainingWidth()))
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

									//using (GUI.Group.Centered(outer_size: new(GUI.GetRemainingWidth(), 40), inner_size: new(100, 40)))
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

									//			using (GUI.Wrap.Push(GUI.GetRemainingWidth()))
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
									using (var group_row = GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), 32), padding: new(4)))
									{
										GUI.DrawBackground(GUI.tex_panel, group_row.GetOuterRect(), new Vector4(4));

										GUI.TitleCentered(h_inventory.Name, pivot: new(0.50f, 0.50f));

										var is_selected = selected_inventory_id == h_inventory.ID;
										if (GUI.Selectable3(h_inventory.Name, group_row.GetOuterRect(), is_selected))
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
						if (info_src.IsValid)
						{
							if (!info_new.IsValid && !info_dst.IsValid)
							{
								var dir = (info_src.Position - wpos_mouse).GetNormalizedFast();

								GUI.DrawLine2((info_src.Position).WorldToCanvas(), (wpos_mouse).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.00f), 8.00f, 8.00f);
							}

							if (info_new.IsValid)
							{
								var dir = (info_src.Position - info_new.Position).GetNormalizedFast();

								GUI.DrawLine2((info_src.Position).WorldToCanvas(), (info_new.Position).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.50f), 8.00f, 8.00f);
							}

							if (info_dst.IsValid)
							{
								var dir = (info_src.Position - info_dst.Position).GetNormalizedFast();

								GUI.DrawLine2((info_src.Position).WorldToCanvas(), (info_dst.Position).WorldToCanvas(), color_src, color_dst, 8.00f, 8.00f);
							}
						}

						if (info_src.IsValid)
						{
							//GUI.DrawCircle(info_src.Position.WorldToCanvas(), info_src.Radius * GUI.GetWorldToCanvasScale(), color_src, 2.00f);
							GUI.DrawEntity(info_src.Entity, color_src.WithAlphaMult(0.50f));
						}

						if (info_dst.IsValid)
						{
							//GUI.DrawCircle(info_dst.Position.WorldToCanvas(), info_dst.Radius * GUI.GetWorldToCanvasScale(), color_dst, 2.00f);
							GUI.DrawEntity(info_dst.Entity, color_dst.WithAlphaMult(0.50f));
						}

						if (info_new.IsValid)
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
					public ulong inventory_id;

					public Transform.Data transform;

					public float radius;
					public Vector2 pos;

					public bool is_src;
					public bool alive;
					public bool valid;

					public Entity Entity => this.entity;
					public ulong ComponentID => this.inventory_id;
					public Vector2 Position => this.pos;
					public float Radius => this.radius;
					public bool IsSource => this.is_src;
					public bool IsAlive => this.alive;
					public bool IsValid => this.valid;

					public TargetInfo(Entity entity, ulong inventory_id, bool is_src)
					{
						this.entity = entity;
						this.is_src = is_src;
						this.alive = this.entity.IsAlive();

						if (this.alive)
						{
							this.valid = true;

							this.valid &= this.entity.GetComponent<Transform.Data>().TryGetValue(out this.transform);

							var has_inventory = false;

							var inventories = this.entity.GetInventories();
							foreach (var h_inventory in inventories)
							{
								if (h_inventory.Flags.HasAny(Inventory.Flags.Allow_Ducts) && (inventory_id == 0 || h_inventory.ID == inventory_id))
								{
									has_inventory = true;
									this.inventory_id = h_inventory.ID;
									this.pos = this.transform.LocalToWorld(h_inventory.Offset);

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

					public ulong component_id_src;
					public ulong component_id_dst;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Conveyors.Data data)
					{
						App.WriteLine($"{this.ent_src} == {data.ent_src}; {this.ent_dst} == {data.ent_dst}");

						ref var region = ref entity.GetRegion();

						var info_src = new TargetInfo(this.ent_src, this.component_id_src, true);
						var info_dst = new TargetInfo(this.ent_dst, this.component_id_dst, false);

						data.ent_src = info_src.entity;
						data.ent_dst = info_dst.entity;

						data.inventory_id_src = info_src.inventory_id;
						data.inventory_id_dst = info_dst.inventory_id;

						data.Sync(entity);
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
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Conveyors.Data data)
					{
						if (this.recipe.HasValue)
						{
							data.selected_recipe = this.recipe.Value;
						}

						data.Sync(entity);
					}
#endif
				}

				public struct ConfirmRPC: Net.IRPC<Wrench.Mode.Conveyors.Data>
				{
#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Conveyors.Data data)
					{
						ref var region = ref entity.GetRegion();
						ref var player = ref connection.GetPlayer();
						ref var recipe = ref data.selected_recipe.GetData();

						if (!region.IsNull() && !player.IsNull() && !recipe.IsNull())
						{
							var errors = Build.Errors.None;

							var info_src = new TargetInfo(data.ent_src, data.inventory_id_src, true);
							var info_dst = new TargetInfo(data.ent_dst, data.inventory_id_dst, false);

							if (info_src.valid && info_dst.valid)
							{
								var pos_mid = info_src.pos; // (info_src.pos + info_dst.pos) * 0.50f;
								var dir = (info_dst.pos - info_src.pos).GetNormalized(out var distance);

								errors |= data.EvaluateNodePair<Wrench.Mode.Conveyors.Data, Wrench.Mode.Conveyors.TargetInfo, Conveyor.Data>(ref region, ref info_src, ref info_dst, ref recipe, out _, player.faction_id);
								if (!Crafting.Evaluate(entity, ref player, pos_mid, ref recipe.requirements, amount_multiplier: 1.00f + MathF.Ceiling(distance))) errors |= Build.Errors.RequirementsNotMet;

								if (errors == Build.Errors.None)
								{
									Crafting.Consume(entity, pos_mid, ref recipe.requirements, amount_multiplier: 1.00f + MathF.Ceiling(distance), sync: true);

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

							data.Sync(entity);
						}
					}
#endif
				}
			}
		}
	}
}
