
using Keg.Engine;
using System.Diagnostics.CodeAnalysis;

namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		public static partial class Mode
		{
			public static partial class Build
			{
				[IComponent.Data(Net.SendType.Reliable, name: "Wrench (Build)", region_only: true)]
				public partial struct Data(): IComponent, Wrench.IMode
				{
					public float placement_range = 4.00f;

					[Asset.Ignore] public IRecipe.Handle recipe;
					[Asset.Ignore] public Wrench.Mode.Build.Flags flags;

					[Asset.Ignore, Save.Ignore, Net.Ignore] public float next_place;

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 24, 24, 3, 0);
					public static string Name { get; } = "Build";

					public readonly Recipe.Type RecipeType => Recipe.Type.Undefined;
					public readonly Color32BGRA ColorOk => Color32BGRA.Green;
					public readonly Color32BGRA ColorError => Color32BGRA.Red;
					public readonly Color32BGRA ColorNew => Color32BGRA.Yellow;

					public readonly IRecipe.Handle SelectedRecipe => this.recipe;

#if CLIENT

					//public Entity ent_build;
					//public Wrench.Mode.Build.Data build;
					////public Control.Data control;
					//public Transform.Data transform;


					public static readonly string[] category_names = Enum.GetNames<Wrench.Mode.Build.Category>();
					public static readonly Wrench.Mode.Build.Category[] category_values = Enum.GetValues<Wrench.Mode.Build.Category>();
					public static FixedString64 edit_name_filter;

					public static Recipe.Type edit_type_filter = Recipe.Type.Architecture;
					public static List<(uint index, float rank)> recipe_indices = new(64);

					[Region.Local] public static Vector2 pos_a_placeholder;
					[Region.Local] public static float next_place_local;

					[Region.Local] public static Vector2? normal_cached;
					[Region.Local] public static Vector2? pos_a_raw;
					[Region.Local] public static Vector2? pos_b_raw;

					public static bool show_zones;
					public static bool enable_grid;

					public static Sprite ui_icons_crafting_mini = new Sprite("ui_icons_crafting.mini", 16, 16, 0, 0);
					public static Sprite ui_icons_builder_categories = new Sprite("ui_icons_builder_categories", 16, 16, 0, 0);

					public void Draw(GUI.Window window, Entity ent_wrench, ref Wrench.Data wrench)
					{
						ref var region = ref Client.GetRegion();
						ref var character = ref Client.GetCharacter(out var character_asset);

						ref readonly var kb = ref Control.GetKeyboard();
						ref readonly var mouse = ref Control.GetMouse();

						var search_filter = edit_name_filter.ToString();
						var is_searching = !string.IsNullOrEmpty(search_filter);

						Arm.HoverGUI.Hide();

						ref var wrench_transform = ref ent_wrench.GetComponent<Transform.Data>();

						{
							GUI.DrawWindowBackground(GUI.tex_window_menu, new Vector4(8, 8, 8, 8));

							//if (Control.GetKeyboard().GetKeyDown(Keyboard.Key.Reload))
							//{
							//	//this.builder.category = (Builder.Category)Maths.Repeat((int)this.builder.category + 1, category_values.Length);
							//	//Sound.PlayGUI(GUI.sound_select, volume: 0.07f);
							//}

							//Crafting.Context.NewFromCharacter(ref region.AsCommon(), character_asset, ent_wrench, out var context);
							Crafting.Context.NewFromCurrentCharacter(ent_wrench, out var context);

							// Top bar
							using (GUI.Group.New(size: new(GUI.RmX, 40)))
							{
								using (GUI.Group.New(size: new(category_values.Length * 40, GUI.RmY)))
								{
									for (var i = 0; i < category_values.Length; i++)
									{
										using (GUI.ID<Wrench.Mode.Build.Category>.Push(i + 1))
										{
											var button_type_filter = Recipe.Type.Undefined; //.None;

											switch ((Wrench.Mode.Build.Category)i)
											{
												case Wrench.Mode.Build.Category.Architecture:
												{
													button_type_filter = Recipe.Type.Architecture;
												}
												break;

												//case Wrench.Mode.Build.Category.Furniture:
												//{
												//	button_tags_filter = Recipe.Tags.Furniture;
												//}
												//break;

												case Wrench.Mode.Build.Category.Industry:
												{
													button_type_filter = Recipe.Type.Industry;
												}
												break;

												case Wrench.Mode.Build.Category.Machinery:
												{
													button_type_filter = Recipe.Type.Machinery;
												}
												break;

												case Wrench.Mode.Build.Category.Buildings:
												{
													button_type_filter = Recipe.Type.Buildings;
												}
												break;

												//case Wrench.Mode.Build.Category.Infrastructure:
												//{
												//	button_tags_filter = Recipe.Tags.Infrastructure;
												//}
												//break;

												//case Wrench.Mode.Build.Category.Misc:
												//{
												//	button_tags_filter = Recipe.Tags.Misc;
												//}
												//break;
											}

											if (GUI.DrawIconButton("build.category"u8, ui_icons_builder_categories.WithFrame((uint)i, 0), new(40, 40), color: !is_searching && edit_type_filter == button_type_filter ? GUI.col_button_highlight : GUI.col_button))
											{
												edit_type_filter = button_type_filter;
											}

											if (GUI.IsItemHovered())
											{
												using (GUI.Tooltip.New())
												{
													GUI.Title(category_names[i]);
												}
											}

											GUI.SameLine();
										}
									}
								}

								GUI.SameLine();

								if (GUI.TextInput("##search"u8, "search (Ctrl+F)"u8, ref edit_name_filter, new Vector2(GUI.RmX - 40, GUI.RmY), show_label: false))
								{

								}
								GUI.FocusOnCtrlF();

								GUI.SameLine();

								using (GUI.Group.Centered(outer_size: GUI.Rm, inner_size: new Vector2(24, 24)))
								{
									GUI.Checkbox("Zones"u8, ref show_zones, new Vector2(GUI.RmY), show_text: false);
									GUI.DrawHoverTooltip("Show Zones"u8);
								}
							}

							GUI.SeparatorThick();

							using (GUI.Scrollbox.New("Build.Recipes"u8, GUI.Rm))
							{
								using (var grid = GUI.Grid.New(size: GUI.Rm, new Vector2(48, 48)))
								{
									var scale = 2.00f;

									//var recipes = Shop.GetAllRecipes();

									//var ts = Timestamp.Now();

									recipe_indices.Clear();

									var recipes = IRecipe.Database.GetAssetsSpan();
									foreach (var d_recipe in recipes)
									{
										ref var recipe = ref d_recipe.GetData();
										if (recipe.type == edit_type_filter && recipe.flags.HasNone(Recipe.Flags.Hidden | Recipe.Flags.Custom))
										{
											//var size = (Vector2)recipe.icon.GetFrameSize();
											var size = recipe.frame_size.OrDefault(recipe.icon.GetFrameSize(scale));
											size = size.ScaleToNearestMultiple(new Vector2(48, 48));

											//var rank = recipe.rank;
											//rank += d_recipe.id * 0.000001f;


											var rank = recipe.rank;
											rank += d_recipe.id * 0.0001f;
											rank -= (size.X * size.Y) * 0.001f;

											if (recipe.products[0].type == Crafting.Product.Type.Block) rank -= 5000.00f;
											//rank += (size.X) * 1.00f;
											rank -= (size.Y) * 10.00f;
											////rank -= size.Y * 0.10f;

											recipe_indices.Add((d_recipe.id, rank));
										}
									}

									recipe_indices.Sort(SortFunc);
									static int SortFunc((uint index, float rank) a, (uint index, float rank) b)
									{
										return a.rank.CompareTo(b.rank);
									}

									//GUI.Text($"{ts.GetMilliseconds():0.0000} ms");

									var h_recipe_selected = this.recipe;
									var h_recipe_prev = IRecipe.Handle.None;
									var recipe_switch_dir = 0;

									if (mouse.GetKeyDown(Mouse.Key.Back)) recipe_switch_dir = 1;
									else if (mouse.GetKeyDown(Mouse.Key.Forward)) recipe_switch_dir = -1;

									foreach (var pair in recipe_indices)
									{
										//GUI.Text($"{pair.rank}");

										var h_recipe = (IRecipe.Handle)pair.index;

										ref var recipe = ref h_recipe.GetData(out var recipe_asset);
										if (recipe.IsNotNull() && (is_searching || recipe.type == edit_type_filter))
										{
											if (!is_searching || recipe.name.Contains(search_filter, StringComparison.OrdinalIgnoreCase))
											{
												//var frame_size = recipe.icon.GetFrameSize(scale);
												//frame_size += new Vector2(8, 8);

												var frame_size = recipe.frame_size.OrDefault(recipe.icon.GetFrameSize(scale));
												//frame_size += new Vector2(2);
												frame_size = frame_size.ScaleToNearestMultiple(new Vector2(48, 48));

												using (var cell = grid.Push(frame_size, id: (uint)recipe_asset.GetHashCode()))
												{
													if (cell.group.IsVisible())
													{
														var h_recipe_selected_tmp = h_recipe_selected;
														if (GUI.DrawRecipeButton(context: ref context, rect: cell.group.GetOuterRect(), h_recipe: h_recipe, h_recipe_selected: ref h_recipe_selected_tmp,
														evaluation_flags: Crafting.EvaluateFlags.None,
														flags_add: GUI.DrawRecipeFlags.None,
														flags_rem: GUI.DrawRecipeFlags.Send_RPC | GUI.DrawRecipeFlags.Products | GUI.DrawRecipeFlags.Highlight | GUI.DrawRecipeFlags.Button | GUI.DrawRecipeFlags.Counter))
														{
															var rpc = new Wrench.Mode.Build.ConfigureRPC
															{
																recipe = h_recipe
															};
															rpc.Send(ent_wrench);
														}
													}
												}

												if (recipe.flags.HasNone(Recipe.Flags.Disabled | Recipe.Flags.Hidden))
												{
													if (recipe_switch_dir < 0)
													{
														if (h_recipe == h_recipe_selected && h_recipe_prev != 0)
														{
															Sound.PlayGUI(GUI.sound_select, volume: 0.08f, pitch: 0.95f);

															var rpc = new Wrench.Mode.Build.ConfigureRPC
															{
																recipe = h_recipe_prev
															};
															rpc.Send(ent_wrench);
															recipe_switch_dir = 0;
														}
													}
													else if (recipe_switch_dir > 0)
													{
														if (h_recipe_prev == h_recipe_selected)
														{
															Sound.PlayGUI(GUI.sound_select, volume: 0.08f, pitch: 1.05f);

															var rpc = new Wrench.Mode.Build.ConfigureRPC
															{
																recipe = h_recipe
															};
															rpc.Send(ent_wrench);
															recipe_switch_dir = 0;
														}
													}

													h_recipe_prev = h_recipe;
												}
											}
										}
									}
								}
							}
						}


						//if (!GUI.IsHovered)

						if (!Editor.IsActive)
						{
							using (var window_hud = GUI.Window.HUD("Build.HUD"u8, position: region.WorldToCanvas(mouse.GetInterpolatedPosition() + new Vector2(1.50f, 1.50f)), size: new(192, 0), padding: new(8, 8), pivot: new(0.00f, 0.00f)))
							{
								if (window_hud.show)
								{
									var errors = Wrench.Mode.Build.Errors.None;
									var skip_support = false;
									var support = 0.00f;
									var clearance = 0.00f;
									var amount_multiplier = 1.00f;

									ref var recipe = ref this.recipe.GetData();
									if (recipe.IsNotNull() && character.IsNotNull())
									{
										//Crafting.Context.NewFromPlayer(ref region, ref player, ent_wrench, out var context);
										Crafting.Context.NewFromCurrentCharacter(ent_wrench, out var context);

										if (recipe.type != Recipe.Type.Undefined)
										{
											ref var product = ref recipe.products[0];
											if (recipe.placement.TryGetValue(out var placement))
											{
												var pos_raw = mouse.position;
												var pos_origin = wrench_transform.position;

												var placement_flags = placement.flags;

												var snapping_flags = placement.snapping_flags;
												var snapping_filter = placement.snapping_filter;

												var h_faction = character.faction;

												//var rot_offset = 0.00f;
												var normal_surface = normal_cached ?? new Vector2(0.00f, -1.00f);
												var normal_distance = 0.00f;

												var placement_size_max = Maths.Max(placement.length_min, placement.size.GetMax());

												var snapping_radius = placement.snapping_radius;
												if (snapping_flags.HasAny(Placement.SnappingFlags.Add_Size_To_Snap_Radius)) snapping_radius += placement_size_max * 0.50f;

												var no_snap = snapping_flags.HasAny(pos_a_raw.HasValue ? Placement.SnappingFlags.No_End_Snap : Placement.SnappingFlags.No_Start_Snap);

												if (!no_snap && !kb.GetKey(Keyboard.Key.LeftShift) && ((snapping_flags.HasAny(Placement.SnappingFlags.Force) || kb.GetKey(Keyboard.Key.LeftControl) != snapping_flags.HasAny(Placement.SnappingFlags.Snap_To_Surface)) && !snapping_filter.IsEmpty()))
												{
													if (region.TryOverlapPoint(pos_raw, radius: snapping_radius, out var overlap_result, mask: snapping_filter.include, require: snapping_filter.require, exclude: snapping_filter.exclude | Physics.Layer.Dynamic))
													{
														//var normal_offset = 0.00f; // Terrain.shape_thickness * 0.50f;

														var shape_radius = overlap_result.GetShapeRadius();

														var is_raw_collider = snapping_flags.HasAny(Placement.SnappingFlags.Use_Raw_Collider) || overlap_result.layer.HasAny(Physics.Layer.World) || shape_radius <= 0.00f;
														pos_raw = is_raw_collider ? overlap_result.world_position_raw : overlap_result.world_position;
														var normal_tmp = is_raw_collider ? overlap_result.normal_raw : (overlap_result.world_position - overlap_result.world_position_raw).GetNormalized();

														GUI.DrawCross(region.WorldToCanvas(pos_raw.Snap(0.125f)).Round(), color: GUI.font_color_green.WithAlpha(150), radius: (region.GetWorldToCanvasScale() * 0.500f).Ceil(), thickness: (0.125f * region.GetWorldToCanvasScale()).Ceil());

														var is_at_base = placement.type == Placement.Type.Line && pos_a_raw.HasValue && pos_raw.IsInDistance(pos_a_raw.Value, snapping_radius * 2.00f);
														if (is_at_base)
														{
															pos_raw = pos_a_raw.Value + (normal_surface * Maths.Max(placement_size_max, placement.length_step));
														}

														if (snapping_flags.HasAny(Placement.SnappingFlags.Align_To_Surface))
														{
															if (snapping_flags.HasAny(Placement.SnappingFlags.Align_Parallel))
															{
																normal_tmp = normal_tmp.GetPerpendicular(normal_tmp.X.SignEquals((pos_raw.Y - pos_origin.Y)));
															}

															if (!pos_a_raw.HasValue) normal_surface = normal_tmp;
														}

														if (!is_at_base)
														{
															if (overlap_result.layer.HasAny(Physics.Layer.World))
															{
																if (snapping_flags.HasAny(Placement.SnappingFlags.Use_Collider_Radius))
																{
																	pos_raw += normal_tmp * Terrain.shape_thickness * 0.50f;
																	if (snapping_flags.HasAny(Placement.SnappingFlags.Inset_Terrain)) pos_raw -= normal_tmp * placement_size_max * 0.50f;
																}
															}
															else
															{
																if (is_raw_collider && snapping_flags.HasAny(Placement.SnappingFlags.Use_Collider_Radius))
																{
																	pos_raw += normal_surface * shape_radius;
																}
															}

															//if (product.type == Crafting.Product.Type.Block)
															//{
															//	pos_raw += normal_tmp * Terrain.shape_thickness * 0.50f;
															//}

															if (snapping_flags.HasAny(Placement.SnappingFlags.Align_To_Surface) && !pos_a_raw.HasValue) normal_distance += placement_size_max;
															if (snapping_flags.HasAny(Placement.SnappingFlags.Add_Size_To_Snap_Offset)) pos_raw += normal_tmp * placement_size_max * 0.50f;
														}

														//if (snapping_flags.HasAny(Placement.SnappingFlags.Add_Placement_Offset))
														//{
														//	pos_raw += placement.offset.RotateByDir(overlap_result.normal_raw.GetPerpendicular(true));
														//}

														pos_raw += normal_tmp * placement.snapping_depth;

													}
												}

												if (!pos_a_raw.HasValue && (mouse.GetKeyDown(Mouse.Key.Left) || (placement.type == Placement.Type.Simple && mouse.GetKeyDown(Mouse.Key.Right))))
												{
													pos_a_raw = pos_raw;
													normal_cached = normal_surface;
												}
												else if (pos_a_raw.HasValue && mouse.GetKeyDown(Mouse.Key.Left) && placement.type == Placement.Type.Line)
												{
													pos_b_raw = pos_raw;
												}

												//if (pos_a_raw.HasValue && !pos_b_raw.HasValue && placement.type == Placement.Type.Line)
												//{
												//	pos_b_raw = pos_raw + normal_surface;
												//}

												//if (pos_a_raw.HasValue) // && control.mouse.GetKeyUp(Mouse.Key.Left))


												var flags = this.flags;
												flags.SetFlag(Wrench.Mode.Build.Flags.Snap, !kb.GetKey(Keyboard.Key.LeftShift));

												var scale = new Vector2(1, 1);
												if (placement.flags.HasAny(Placement.Flags.Allow_Mirror_X)) scale.X.ToggleSign(pos_raw.X < pos_origin.X);

												Wrench.Mode.Build.GetPlacementInfo(placement: ref placement,
													flags: flags,
													pos_raw: pos_raw,
													pos_a_raw: pos_a_raw,
													pos_b_raw: pos_b_raw,
													scale: scale,
													normal: normal_surface,
													normal_distance: normal_distance,
													pos: out var pos,
													pos_a: out var pos_a,
													pos_b: out var pos_b,
													pos_final: out var pos_final,
													rot_final: out var rot_final,
													bb: out var bb);

												//region.DrawDebugDir(pos, normal_surface * 8, color: Color32BGRA.Yellow);

												//rot_final += rot_offset;

												var transform = new Transform.Data(pos_final, rot_final, scale);
												var matrix = transform.GetMatrix3x2();

												var ent_parent = ent_wrench.GetParent(Relation.Type.Child);

												Color32BGRA color_ok = 0xff00ff00;
												Color32BGRA color_error = 0xffff0000;
												Color32BGRA color_gray = 0xff404040;
												Color32BGRA color_yellow = 0xffffff00;

												Color32BGRA color_dummy;

												if (true) color_dummy = color_ok;
												else color_dummy = color_error;

												var color_dummy_bg = color_dummy.WithAlphaMult(0.10f);
												var color_dummy_fg = color_dummy.WithAlphaMult(0.30f);

												var color_gray_bg = color_gray.WithAlphaMult(0.10f);
												var color_gray_fg = color_gray.WithAlphaMult(0.60f);

												var color_ok_bg = color_ok.WithAlphaMult(0.10f);
												var color_ok_fg = color_ok.WithAlphaMult(0.30f);

												var color_error_bg = color_error.WithAlphaMult(0.10f);
												var color_error_fg = color_error.WithAlphaMult(0.30f);

												var color_yellow_bg = color_yellow.WithAlphaMult(0.10f);
												var color_yellow_fg = color_yellow.WithAlphaMult(0.30f);

												GUI.DrawChunkRect(ref region, pos_raw);
												//GUI.DrawTerrainOutline(ref region, pos_raw, radius: 2.00f, thickness: 1.00f, color: GUI.col_white);

												switch (product.type)
												{
													case Crafting.Product.Type.Block:
													{
														ref var block = ref product.block.GetData();
														if (block.IsNotNull())
														{
															var tile_flags = block.tile_flags | product.tile_flags;

															ref var terrain = ref region.GetTerrain();

															Wrench.Mode.Build.EvaluateBlock(region: ref region, placement: in placement, errors: ref errors, skip_support: ref skip_support, placed_block_count: out var placed_block_count, bb: bb, block: product.block, tile_flags: tile_flags, pos: pos, pos_a: pos_a, pos_b: pos_b);
															amount_multiplier = placed_block_count;
															Wrench.Mode.Build.Evaluate(region: ref region, entity: ent_wrench, placement: in placement, errors: ref errors, skip_support: ref skip_support, support: out support, clearance: out clearance, bb: bb, transform: in transform, placement_range: this.placement_range, pos: pos, pos_a: pos_a, pos_b: pos_b, faction_id: character.faction);

															if (show_zones || placement.flags.HasAny(Placement.Flags.Require_Claimed) || errors.HasAny(Wrench.Mode.Build.Errors.Claimed))
															{
																Claim.DrawFullOverlay(ref region, show_zones, true);
																Claim.DrawClaimerOverlay(ref region, show_zones, true);
															}

															//if (!Crafting.Evaluate(ent_wrench, ent_parent, this_transform.position, ref recipe.requirements, inventory: inventory, amount_multiplier: amount_multiplier, h_faction: h_faction)) errors |= Errors.RequirementsNotMet;
															if (!context.Evaluate(requirements: recipe.requirements.AsSpan(), amount_multiplier: amount_multiplier)) errors |= Errors.RequirementsNotMet;

															if (errors != Wrench.Mode.Build.Errors.None)
															{
																color_dummy_fg = color_error_fg;
																color_dummy_bg = color_error_bg;

																//GUI.DrawOverlapBB(ref region, bb, Physics.Layer.Solid | Physics.Layer.Building);
															}

															var pixel_size = new Vector2(App.pixels_per_unit_inv) * region.GetWorldToCanvasScale();

															//GUI.DrawCross(region.WorldToCanvas(pos_a), radius: 16, layer: GUI.Layer.Background, color: color_dummy_fg);
															GUI.DrawRect(region.WorldToCanvas(bb), layer: GUI.Layer.Background, color: color_dummy_fg);

															var pos_a_centered = pos_a;
															var pos_b_centered = pos_b;

															if (placement.type == Placement.Type.Line)
															{
																var dir = (pos_a - pos_b).GetNormalized(out var len);
																pos_a_centered -= dir * placement.size * 0.50f;
																pos_b_centered += dir * placement.size * 0.50f;

																pos_a_centered = pos_a_centered.Snap(0.125f);
																pos_b_centered = pos_b_centered.Snap(0.125f);
															}

															GUI.DrawCross(region.WorldToCanvas(pos_a_centered), color: GUI.font_color_default.WithAlpha(180), radius: region.GetWorldToCanvasScale() * 4.00f);

															var args = new DrawTileArgs(offset: region.WorldToCanvas(bb.a), pixel_size: pixel_size, color: color_dummy_fg, tile_flags: block.tile_flags | product.tile_flags, block: product.block, max_health: block.max_health, mappings_replace: placement.mappings_replace);
															switch (placement.type)
															{
																case Placement.Type.Rectangle:
																{
																	terrain.IterateRect(world_position: pos, size: placement.size * App.pixels_per_unit, argument: ref args,
																		func: placement.flags.HasAny(Placement.Flags.Replace) ? DrawTileFuncReplace : DrawTileFunc,
																		iteration_flags: Terrain.IterationFlags.Iterate_Empty);
																}
																break;

																case Placement.Type.Circle:
																{
																	terrain.IterateCircle(world_position: pos, radius: placement.radius * App.pixels_per_unit, argument: ref args,
																		func: placement.flags.HasAny(Placement.Flags.Replace) ? DrawTileFuncReplace : DrawTileFunc,
																		iteration_flags: Terrain.IterationFlags.Iterate_Empty);
																}
																break;

																case Placement.Type.Line:
																{
																	if (pos_a_raw.HasValue) GUI.DrawCross(region.WorldToCanvas(pos_b_centered), color: GUI.font_color_default.WithAlpha(180), radius: region.GetWorldToCanvasScale() * 2.00f);

																	//if (pos_a_raw.HasValue) App.WriteLine($"{pos}; {pos_a}; {pos_b}");
																	terrain.IterateSquareLine(world_position_a: pos_a, world_position_b: pos_b, thickness: placement.size.X, argument: ref args,
																		func: placement.flags.HasAny(Placement.Flags.Replace) ? DrawTileFuncReplace : DrawTileFunc,
																		iteration_flags: Terrain.IterationFlags.Iterate_Empty);
																
																
																}
																break;
															}
														}
													}
													break;

													case Crafting.Product.Type.Prefab:
													case Crafting.Product.Type.Resource:
													{
														var h_prefab = default(Prefab.Handle);
														if (product.type == Crafting.Product.Type.Prefab) h_prefab = product.prefab;
														else if (product.type == Crafting.Product.Type.Resource) h_prefab = product.material.GetPrefabHandle();

														if (h_prefab.TryGetPrefab(out var prefab))
														{
															//var prefab_handle = product.prefab;

															//var scale = new Vector2(1, 1);
															//scale.X.ToggleSign(placement.flags.HasAny(Placement.Flags.Allow_Mirror_X) & pos_raw.X < wrench_transform.position.X);

															Wrench.Mode.Build.EvaluatePrefab(region: ref region, placement: in placement, errors: ref errors, skip_support: ref skip_support, h_prefab: h_prefab, matrix: in matrix, pos_a: pos_a, pos_b: pos_b);
															amount_multiplier = 1.00f;
															Wrench.Mode.Build.Evaluate(region: ref region, entity: ent_wrench, placement: in placement, errors: ref errors, skip_support: ref skip_support, support: out support, clearance: out clearance, bb: bb, transform: in transform, placement_range: this.placement_range, pos: pos_final, pos_a: pos_a, pos_b: pos_b, faction_id: character.faction);

															if (show_zones || placement.flags.HasAny(Placement.Flags.Require_Claimed) || errors.HasAny(Wrench.Mode.Build.Errors.Claimed))
															{
																Claim.DrawFullOverlay(ref region, show_zones || placement.flags.HasAny(Placement.Flags.Require_Claimed), true);
																Claim.DrawClaimerOverlay(ref region, show_zones || placement.flags.HasAny(Placement.Flags.Require_Claimed), true);
															}

															if (placement.type == Placement.Type.Line)
															{
																amount_multiplier += Vector2.Distance(pos_a, pos_b);
															}

															if (recipe.construction.HasValue)
															{
																var construction = recipe.construction.Value;

																//if (!Crafting.Evaluate(ent_wrench, ent_parent, this_transform.position, ref construction.requirements, inventory: inventory, amount_multiplier: amount_multiplier, h_faction: h_faction)) errors |= Errors.RequirementsNotMet;
																if (!context.Evaluate(requirements: construction.requirements, amount_multiplier: amount_multiplier)) errors |= Errors.RequirementsNotMet;
															}
															else
															{
																//if (!Crafting.Evaluate(ent_wrench, ent_parent, this_transform.position, ref recipe.requirements, inventory: inventory, amount_multiplier: amount_multiplier, h_faction: h_faction)) errors |= Errors.RequirementsNotMet;
																if (!context.Evaluate(requirements: recipe.requirements.AsSpan(), amount_multiplier: amount_multiplier)) errors |= Errors.RequirementsNotMet;
															}

															if (errors != Wrench.Mode.Build.Errors.None)
															{
																color_dummy_fg = color_error_fg;
																color_dummy_bg = color_error_bg;

																//GUI.DrawOverlapBB(ref region, bb, Physics.Layer.Solid | Physics.Layer.Building);
															}

															switch (placement.type)
															{
																case Placement.Type.Line:
																{
																	if (prefab.root.TryGetComponentData<Resizable.Data>(out var resizable, initialized: true) && prefab.root.TryGetComponentData<Animated.Renderer.Data>(out var renderer, initialized: true))
																	{
																		resizable.a = Vector2.Zero;
																		resizable.b = pos_b - pos_a;

																		var sprite_size = renderer.sprite.texture.Value.Size / 8.00f;

																		var dir = (resizable.b - resizable.a).GetNormalized(out var len);
																		var mid = (resizable.a + resizable.b) * 0.50f;

																		var angle = dir.GetAngleRadians();

																		renderer.offset = mid - new Vector2((len - sprite_size.X) * 0.50f, resizable.offset_y).RotateByRad(angle);
																		renderer.rotation = -angle;
																		renderer.rect.Z = len / sprite_size.X;

																		var normalized = (resizable.b - resizable.a).GetNormalized(out var length);

																		//var transform = new Transform.Data(pos_final, rot_final, scale);

																		var renderer_a = new Animated.Renderer.Data
																		{
																			sprite = resizable.cap_a,
																			rotation = renderer.rotation,
																			scale = new Vector2(resizable.flags.HasAny(Resizable.Flags.Mirror_Cap_A) ? -1.00f : 1.00f, 1.00f),
																			offset = resizable.a - (normalized * resizable.cap_offset) - new Vector2(0.00f, resizable.offset_y).RotateByDir(normalized)
																		};

																		var renderer_b = new Animated.Renderer.Data
																		{
																			sprite = resizable.cap_b,
																			rotation = renderer.rotation,
																			scale = new Vector2(resizable.flags.HasAny(Resizable.Flags.Mirror_Cap_B) ? -1.00f : 1.00f, 1.00f),
																			offset = resizable.b + (normalized * resizable.cap_offset) - new Vector2(0.00f, resizable.offset_y).RotateByDir(normalized)
																		};

																		GUI.DrawRenderer(in transform, in renderer, color_dummy_fg);
																		GUI.DrawRenderer(in transform, in renderer_a, color_dummy_fg);
																		GUI.DrawRenderer(in transform, in renderer_b, color_dummy_fg);
																	}
																}
																break;

																default:
																case Placement.Type.Simple:
																{
																	if (prefab.root.TryGetComponentData<Animated.Renderer.Data>(out var renderer, initialized: true))
																	{
																		//var transform = new Transform.Data(pos_final, rot_final, scale);
																		//var offset = transform.LocalToWorld(placement.offset);


																		var sprite = recipe.icon;
																		if (sprite.texture.id != 0)
																		{
																			renderer.sprite = sprite;
																		}

																		//renderer.offset -= placement.offset;


																		if (recipe.construction.HasValue)
																		{
																			var construction = recipe.construction.Value;
																			var construction_prefab = construction.prefab.GetPrefab();

																			GUI.DrawRenderer(in transform, in renderer, color_dummy_fg);

																			if (construction_prefab != null)
																			{
																				if (construction_prefab.root.TryGetComponentData<Animated.Renderer.Data>(out var renderer_construction, initialized: true))
																				{
																					GUI.DrawRenderer(in transform, in renderer_construction, color_dummy_fg);
																				}
																			}
																		}
																		else
																		{
																			GUI.DrawRenderer(in transform, in renderer, color_dummy_fg);
																		}

																		ref var rect_foundation = ref placement.rect_foundation.GetRefOrNull();
																		if (rect_foundation.IsNotNull())
																		{
																			ref var terrain = ref region.GetTerrain();

																			var quad_world = transform.LocalToWorld(rect_foundation - placement.offset);
																			var quad_world_aabb = quad_world.GetAABB().SnapCeil(0.125f);
																			//var quad_canvas = region.WorldToCanvas(quad_world);
																			var quad_canvas_aabb = region.WorldToCanvas(quad_world_aabb);


																			var pixel_size = new Vector2(App.pixels_per_unit_inv) * region.GetWorldToCanvasScale();

																			var args = new DrawFoundationArgs(offset: quad_canvas_aabb.a,
																				pixel_size: pixel_size,
																				tile_flags: placement.tileflags_foundation,
																				max_health: 1000,
																				color: color_ok_fg.WithAlpha(100));

																			terrain.IterateRectRotated(quad_world, argument: ref args,
																				func: DrawFoundationFunc,
																				iteration_flags: Terrain.IterationFlags.Iterate_Empty);

																			GUI.DrawQuad(region.WorldToCanvas(quad_world), color_dummy_fg);
																			//GUI.DrawRect(quad_canvas_aabb, GUI.font_color_yellow);
																		}

																		ref var rect_clearance = ref placement.rect_clearance.GetRefOrNull();
																		if (rect_clearance.IsNotNull())
																		{
																			ref var terrain = ref region.GetTerrain();

																			var quad_world = transform.LocalToWorld(rect_clearance - placement.offset);
																			var quad_world_aabb = quad_world.GetAABB().SnapCeil(0.125f);
																			//var quad_canvas = region.WorldToCanvas(quad_world);
																			var quad_canvas_aabb = region.WorldToCanvas(quad_world_aabb);


																			var pixel_size = new Vector2(App.pixels_per_unit_inv) * region.GetWorldToCanvasScale();

																			var args = new DrawFoundationArgs(offset: quad_canvas_aabb.a,
																				pixel_size: pixel_size,
																				tile_flags: placement.tileflags_clearance,
																				max_health: 1000,
																				color: color_error_fg);

																			terrain.IterateRectRotated(quad_world, argument: ref args,
																				func: DrawFoundationFunc,
																				iteration_flags: Terrain.IterationFlags.Iterate_Empty);

																			GUI.DrawQuad(region.WorldToCanvas(quad_world), color_dummy_fg.WithAlphaMult(0.50f));
																			//GUI.DrawRect(quad_canvas_aabb, GUI.font_color_yellow);
																		}
																		else
																		{
																			GUI.DrawQuad(region.WorldToCanvas(bb.RotateByRad(transform.rotation)), color_dummy_fg.WithAlphaMult(0.50f));
																		}


																		//GUI.DrawQuad(region.WorldToCanvas(bb.RotateByRad(transform.rotation)), color_dummy_fg);
																		GUI.DrawCircleFilled(region.WorldToCanvas(transform.LocalToWorld(-placement.offset)), 2.00f, 0xffffffff, segments: 4);
																	}
																	//GUI.DrawRect

																	//var bb_canvas = AABB.Centered(pos_final, placement.size);
																	//GUI.DrawRect(GUI.WorldToCanvas(bb_canvas), color_dummy_fg);
																	//GUI.DrawRect(sprite, GUI.WorldToCanvas(bb), color_dummy_fg, clip: false);
																}
																break;
															}
														}
													}
													break;
												}

												if (!GUI.IsHovered)
												{
													GUI.Title(recipe.name);

													GUI.Separator();

													ref var construction = ref recipe.construction.GetRefOrNull();
													if (construction.IsNotNull())
													{
														GUI.DrawRequirements(ref context, construction.requirements, Crafting.EvaluateFlags.None, amount_multiplier: amount_multiplier);

														GUI.NewLine(6);
														GUI.Separator();

														GUI.DrawRequirements(ref context, recipe.requirements, Crafting.EvaluateFlags.Display_Only, amount_multiplier: amount_multiplier, highlight: true);
													}
													else
													{
														GUI.DrawRequirements(ref context, recipe.requirements, Crafting.EvaluateFlags.None, amount_multiplier: amount_multiplier, highlight: true);
													}

													GUI.NewLine(4);

													using (GUI.Wrap.Push())
													{
														if (placement.min_support > Maths.epsilon)
														{
															GUI.TextShaded($"Support: {(support * 100.00f):0}%/{(placement.min_support * 100.00f):0}%", color: support >= placement.min_support ? color_ok : color_error);
														}

														if (placement.min_clearance > Maths.epsilon)
														{
															GUI.TextShaded($"Clearance: {(clearance * 100.00f):0}%/{(placement.min_clearance * 100.00f):0}%", color: clearance >= placement.min_clearance ? color_ok : color_error);
														}

														if (errors != Wrench.Mode.Build.Errors.None)
														{
															GUI.TextShaded(errors.ToFormattedString("* {0}!", "\n"), color: color_error);
														}
													}

													{
														var reset = false;

														//reset |= (pos_a_raw.HasValue && !mouse.GetKey(Mouse.Key.Left | Mouse.Key.Right));

														var place = false;
														if (placement.type == Placement.Type.Line)
														{
															place = pos_a_raw.HasValue & pos_b_raw.HasValue; // && mouse.GetKeyUp(Mouse.Key.Left);
															reset |= mouse.GetKeyDown(Mouse.Key.Right);
														}
														else
														{
															if (placement.flags.HasAny(Placement.Flags.Continuous))
															{
																place = mouse.GetKeyDown(Mouse.Key.Left);
																place |= mouse.GetKey(Mouse.Key.Left);

																reset |= mouse.GetKeyDown(Mouse.Key.Right) || !mouse.GetKey(Mouse.Key.Left);
															}
															else
															{
																place = mouse.GetKeyDown(Mouse.Key.Left);
																reset |= !mouse.GetKey(Mouse.Key.Right);
															}
														}

														var time = region.GetFixedTime();

														place &= time >= next_place_local;
														if (place)
														{
															if (errors.HasNone(Wrench.Mode.Build.Errors.ZeroCount))
															{
																var rpc = new Wrench.Mode.Build.PlaceRPC
																{
																	pos_origin = pos_origin,
																	pos_raw = pos_raw,
																	pos_a_raw = pos_a_raw,
																	pos_b_raw = pos_b_raw,
																	normal = normal_surface,
																	flags = flags,
																};
																rpc.Send(ent_wrench);
															}

															if (placement.flags.HasNone(Placement.Flags.Continuous))
															{
																reset = true;
															}
															else
															{
																if (placement.type == Placement.Type.Line)
																{
																	//pos_a_raw = pos_b - ((pos_b - pos_a).GetNormalized() * placement.length_step);
																	pos_a_raw = (pos_b - ((pos_b - pos_a).GetNormalized() * placement.size * 0.50f).Snap(0.125f)).Snap(0.125f);
																	pos_b_raw = null;
																	normal_cached = null;
																}
															}

															next_place_local = time + placement.cooldown;
														}

														if (reset)
														{
															pos_a_raw = null;
															pos_b_raw = null;
															normal_cached = null;
														}
													}
												}
											}
										}
									}
								}
							}

						}
					}
#endif
				}

				public enum Category: uint
				{
					Architecture = 0,
					//Furniture,
					Industry,
					Buildings,
					Machinery
					//Misc
				}

				public partial struct ConfigureRPC: Net.IRPC<Wrench.Mode.Build.Data>
				{
					public IRecipe.Handle recipe;

#if SERVER
					public void Invoke(Net.IRPC.Context rpc, ref Wrench.Mode.Build.Data build)
					{
						build.recipe = this.recipe;
						build.Sync(rpc.entity);
					}
#endif
				}

				public static void Evaluate(ref Region.Data region, Entity entity, in Placement placement, ref Wrench.Mode.Build.Errors errors, ref bool skip_support, out float support, out float clearance, AABB bb, in Transform.Data transform, float placement_range, Vector2 pos, Vector2? pos_a = default, Vector2? pos_b = default, IFaction.Handle faction_id = default)
				{
					placement_range.ClampMaxRef(placement.max_range);

					if (placement.min_claim > Maths.epsilon)
					{
						var claim_ratio = Claim.GetOverlapRatio(region: ref region,
							bb: bb,
							faction_id: faction_id,
							allow_unclaimed: placement.flags.HasNone(Placement.Flags.Require_Claimed));
						if (claim_ratio < placement.min_claim) errors |= Errors.Claimed;
					}

					support = 1.00f;
					clearance = 1.00f;

					var check_support = placement.min_support > Maths.epsilon && !skip_support;
					var check_clearance = placement.min_clearance > Maths.epsilon;

					if (check_support | check_clearance)
					{
						Wrench.Mode.Build.CalculateSupport(region: ref region,
							placement: in placement,
							transform: in transform,
							support_count: out var support_count,
							blocked_count: out var blocked_count,
							clear_count: out var clear_count,
							total_count_support: out var total_count_support,
							total_count_clearance: out var total_count_clearance,
							pos: pos,
							pos_a: pos_a,
							pos_b: pos_b);

						//Build.CalculateSupport(ref region, in placement, out var support_count, out var blocked_count, out var total_count, pos, pos_a, pos_b);

						if (check_support)
						{
							support = Maths.Normalize01(placement.flags.HasAny(Placement.Flags.No_Solid_Support) ? support_count - blocked_count : support_count, total_count_support);
							errors.AddFlag(Errors.Unsupported, support < placement.min_support);
							errors.RemoveFlag(Errors.NoTerrain, placement.rect_foundation.HasValue && support >= placement.min_support);
						}

						if (check_clearance)
						{
							clearance = Maths.Normalize01(clear_count, total_count_clearance);
							errors.AddFlag(Errors.Obstructed, clearance < placement.min_clearance);
						}
					}

					if (placement.type == Placement.Type.Line)
					{
						var a = pos_a ?? pos;
						var b = pos_b ?? pos;

						var len = Vector2.Distance(a, b);

						if (placement.length_min > Maths.epsilon && len < placement.length_min)
						{
							errors |= Wrench.Mode.Build.Errors.MinLength;
						}

						if (placement.length_max > Maths.epsilon && len > (placement.length_max + placement.size.GetMax()) + 0.125f)
						{
							errors |= Wrench.Mode.Build.Errors.MaxLength;
						}
					}

					ref var transform_entity = ref entity.GetComponent<Transform.Data>();
					if (transform_entity.IsNotNull())
					{
						var placement_range_sq = placement_range.Pow2(); // * placement_range;
						var dist_sq = float.PositiveInfinity;

						switch (placement.type)
						{
							case Placement.Type.Simple:
							{
								dist_sq = Vector2.DistanceSquared(transform_entity.position, bb.ClipPoint(transform_entity.position));

								//#if CLIENT
								//GUI.DrawCircleFilled(GUI.WorldToCanvas(bb.ClipPoint(transform.position)), 4.00f, 0xffffffff);
								//#endif
							}
							break;

							case Placement.Type.Rectangle:
							{
								dist_sq = Vector2.DistanceSquared(transform_entity.position, bb.ClipPoint(transform_entity.position));

								//#if CLIENT
								//GUI.DrawCircleFilled(GUI.WorldToCanvas(bb.ClipPoint(transform.position)), 4.00f, 0xffffffff);
								//#endif
							}
							break;

							case Placement.Type.Circle:
							{
								dist_sq = Vector2.DistanceSquared(transform_entity.position, pos) - placement.radius.Pow2();
							}
							break;

							case Placement.Type.Line:
							{
								var line = new Line(pos_a ?? pos, pos_b ?? pos);
								dist_sq = Vector2.DistanceSquared(transform_entity.position, line.GetClosestPoint(transform_entity.position));

								//#if CLIENT
								//GUI.DrawCircleFilled(GUI.WorldToCanvas(line.GetClosestPoint(transform.position)), 4.00f, 0xffffffff);
								//#endif
							}
							break;
						}

						if (dist_sq > placement_range_sq)
						{
							errors |= Wrench.Mode.Build.Errors.OutOfRange;
						}

						//var placement_range_sq = placement_range * placement_range;
						//if (Vector2.DistanceSquared(pos, transform.position) >= placement_range_sq) errors |= Errors.OutOfRange;
					}
				}

				public static void EvaluateBlock(ref Region.Data region, in Placement placement, ref Wrench.Mode.Build.Errors errors, ref bool skip_support, out int placed_block_count, AABB bb, IBlock.Handle block, TileFlags tile_flags, Vector2 pos, Vector2? pos_a = default, Vector2? pos_b = default)
				{
					placed_block_count = Wrench.Mode.Build.CalculateBlockCount(ref region, in placement, block, tile_flags, pos, pos_a, pos_b);
					if (placed_block_count <= 0) errors |= Wrench.Mode.Build.Errors.ZeroCount;

					if (tile_flags.HasAny(TileFlags.Solid) && placement.flags.HasNone(Placement.Flags.Ignore_Obstructed))
					{
						//if (placement.type == Placement.Type.Line && (((pos_a ?? pos) - (pos_b ?? pos)).GetProduct() != 0.00f))
						if (placement.type == Placement.Type.Line && !(pos_a ?? pos).IsSameAxis(pos_b ?? pos))
						{
							var radius = (placement.size.GetMax() * 0.50f);
							var dir = ((pos_a ?? pos) - (pos_b ?? pos)).GetNormalized(out var len);

							Span<LinecastResult> hits = FixedArray.CreateSpan16<LinecastResult>(out var buffer);
							if (region.TryLinecastAll((pos_a ?? pos) - (dir * (radius)), (pos_b ?? pos) + (dir * (radius)), radius, ref hits, mask: Physics.Layer.Solid | Physics.Layer.Building, exclude: Physics.Layer.Ignore_Hover | Physics.Layer.Stored))
							{
								errors |= Errors.Obstructed;
							}
						}
						else
						{
							//Span<OverlapBBResult> hits = stackalloc OverlapBBResult[16];
							//if (region.TryOverlapBBAll(pos, bb.GetSize() - new Vector2(0.25f), ref hits, mask: Physics.Layer.Solid | Physics.Layer.Building)) errors |= Errors.Obstructed;

							Span<ShapeOverlapResult> hits = FixedArray.CreateSpan32<ShapeOverlapResult>(out var buffer);
							if (region.TryOverlapRectAll(bb.Pad(new Vector4(0.0625f)), ref hits, mask: Physics.Layer.Solid | Physics.Layer.Building, exclude: Physics.Layer.Ignore_Hover | Physics.Layer.Stored))
							{
								errors |= Errors.Obstructed;
							}
						}
					}
				}

				public static void EvaluatePrefab(ref Region.Data region, in Placement placement, ref Wrench.Mode.Build.Errors errors, ref bool skip_support, Prefab.Handle h_prefab, in Matrix3x2 matrix, Vector2? pos_a = default, Vector2? pos_b = default)
				{
					//bb = new AABB(bb.a + new Vector2(0.50f), bb.b - new Vector2(0.50f));

					//var bb_size = bb.GetSize();
					//var bb_size_inner = bb_size;

					//bb_size_inner.X -= Maths.Clamp(bb_size.X - 0.50f, 0.00f, 0.50f);
					//bb_size_inner.Y -= Maths.Clamp(bb_size.Y - 0.50f, 0.00f, 0.50f);

					//bb = bb.GetPaddedRect(bb_size_inner);

					//var mask = Physics.Layer.Building | Physics.Layer.Support | Physics.Layer.No_Overlapped_Placement | Physics.Layer.World;

					var (layer, mask, exclude) = h_prefab.GetShapesFilter();
					var physics_filter = placement.physics_filter;
					var has_physics_filter = !physics_filter.IsEmpty();

					var require = physics_filter.require;
					mask.AddFlag(physics_filter.include);
					exclude.AddFlag(physics_filter.exclude | Physics.Layer.Stored);

					if (placement.flags.HasAny(Placement.Flags.Require_Terrain))
					{
						errors.AddFlag(Wrench.Mode.Build.Errors.NoTerrain);
					}

					if (placement.type == Placement.Type.Line)
					{
						Span<LinecastResult> results = FixedArray.CreateSpan32<LinecastResult>(out var buffer);
						//if (region.TryLinecastAll(pos_a.Value, pos_b.Value, placement.size.GetMax() * 0.50f, ref results, mask: mask, query_flags: Physics.QueryFlag.Static))
						if (region.TryLinecastAll(pos_a.Value, pos_b.Value, placement.size.GetMax() * 0.50f, ref results, layer: layer, mask: mask, exclude: exclude, require: require, query_flags: Physics.QueryFlag.Static | Physics.QueryFlag.Dynamic))
						{
							foreach (ref var result in results)
							{
								var result_layer = result.layer;
								var result_material_type = result.material_type;
								if (result_material_type == Material.Type.None) continue;

								//if (has_physics_filter && !physics_filter.Evaluate(result.layer))
								//{
								//	errors |= Errors.Obstructed;
								//}

								//if (has_physics_filter)
								//{
								//	if (physics_filter.exclude.HasAny(result_layer))
								//	{
								//		errors |= Errors.Obstructed;
								//		continue;
								//	}
								//}

								if (result_layer.HasAny(Physics.Layer.World))
								{
									if (!placement.rect_foundation.HasValue && placement.flags.HasAny(Placement.Flags.Require_Terrain | Placement.Flags.Terrain_Is_Support))
									{
										errors.RemoveFlag(Wrench.Mode.Build.Errors.NoTerrain);
										if (placement.flags.HasAny(Placement.Flags.Terrain_Is_Support)) skip_support = true;
									}
								}
								else if (placement.flags.HasAny(Placement.Flags.Allow_Placement_Over_Buildings))
								{
									if (result_layer.HasAny(Physics.Layer.No_Overlapped_Placement))
									{
										skip_support = false;
										if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;

										//break;
									}
									else if (!placement.rect_foundation.HasValue && result_layer.HasAny(Physics.Layer.Support))
									{
										skip_support = true;
									}
									else if (result_layer.HasAny(Physics.Layer.Building))
									{

									}
									else
									{
										if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;
									}
								}
								else
								{
									if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;
								}

								//App.WriteLine(result.alpha);
								//if (placement.flags.HasAny(Placement.Flags.Require_Terrain | Placement.Flags.Terrain_Is_Support) && result.layer.HasAny(Physics.Layer.World) && (result.alpha <= 0.10f || result.alpha >= 0.90f))
								//if (result.layer.HasAny(Physics.Layer.World))
								//{
								//	if (placement.flags.HasAny(Placement.Flags.Require_Terrain | Placement.Flags.Terrain_Is_Support) && (result.alpha <= 0.10f || result.alpha >= 0.90f))
								//	{
								//		errors.RemoveFlag(Wrench.Mode.Build.Errors.NoTerrain);
								//		if (placement.flags.HasAny(Placement.Flags.Terrain_Is_Support)) skip_support = true;
								//	}
								//}
								//else if (placement.flags.HasAny(Placement.Flags.Allow_Placement_Over_Buildings))
								//{
								//	if (result.layer.HasAny(Physics.Layer.No_Overlapped_Placement))
								//	{
								//		skip_support = false;
								//		if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;

								//		break;
								//	}
								//	else if (result.layer.HasAny(Physics.Layer.Support))
								//	{
								//		skip_support = true;
								//	}
								//	else if (result.layer.HasAny(Physics.Layer.Building))
								//	{

								//	}
								//	else
								//	{
								//		if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;
								//	}
								//}
								//else
								//{
								//	if (result.layer.HasAny(Physics.Layer.No_Overlapped_Placement))
								//	{

								//	}
								//	else
								//	{
								//		if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;
								//	}
								//}
							}
						}
					}
					else
					{
						//Span<OverlapBBResult> results = stackalloc OverlapBBResult[32];
						//if (region.TryOverlapBBAll(bb, ref results, mask: mask, query_flags: Physics.QueryFlag.Static))

						//var ts = Timestamp.Now();
						Span<ShapeOverlapResult> results = FixedArray.CreateSpan32<ShapeOverlapResult>(out var buffer);
						//if (region.TryOverlapRectAll(bb, ref results, mask: mask, query_flags: Physics.QueryFlag.Static))

						//if (region.TryOverlapPrefabAll(h_prefab: h_prefab, matrix: in matrix, results: ref results, mask: mask, query_flags: Physics.QueryFlag.Static))
						if (region.TryOverlapPrefabAll(h_prefab: h_prefab, matrix: in matrix, results: ref results, layer: layer, mask: mask, exclude: exclude, require: require, query_flags: Physics.QueryFlag.Static | Physics.QueryFlag.Dynamic))
						{
							//App.WriteLine(results.Length);
							foreach (ref var result in results)
							{
								var result_layer = result.layer;
								var result_material_type = result.material_type;
								if (result_material_type == Material.Type.None) continue;

								//if (has_physics_filter && !physics_filter.Evaluate(result.layer))
								//{
								//	errors |= Errors.Obstructed;
								//}

								//if (has_physics_filter)
								//{
								//	if (physics_filter.exclude.HasAny(result_layer))
								//	{
								//		errors |= Errors.Obstructed;
								//		continue;
								//	}
								//}

								if (result_layer.HasAny(Physics.Layer.World))
								{
									if (!placement.rect_foundation.HasValue && placement.flags.HasAny(Placement.Flags.Require_Terrain | Placement.Flags.Terrain_Is_Support))
									{
										errors.RemoveFlag(Wrench.Mode.Build.Errors.NoTerrain);
										if (placement.flags.HasAny(Placement.Flags.Terrain_Is_Support)) skip_support = true;
									}
								}
								else if (placement.flags.HasAny(Placement.Flags.Allow_Placement_Over_Buildings))
								{
									if (result_layer.HasAny(Physics.Layer.No_Overlapped_Placement))
									{
										skip_support = false;
										if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;

										//break;
									}
									else if (!placement.rect_foundation.HasValue && result_layer.HasAny(Physics.Layer.Support))
									{
										skip_support = true;
									}
									else if (result_layer.HasAny(Physics.Layer.Building))
									{

									}
									else
									{
										if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;
									}
								}
								else
								{
									if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;
								}
							}
						}
						//App.WriteLine($"{ts.GetMilliseconds():0.000} ms");
					}
				}

				public static void GetPlacementInfo(ref Placement placement, Wrench.Mode.Build.Flags flags, Vector2 pos_raw, Vector2? pos_a_raw, Vector2? pos_b_raw, Vector2 scale, Vector2 normal, float normal_distance, out Vector2 pos, out Vector2 pos_a, out Vector2 pos_b, out Vector2 pos_final, out float rot_final, out AABB bb)
				{
					var snap = new Vector2(0.125f);
					if (placement.flags.HasNone(Placement.Flags.No_Snapping) && flags.HasAny(Wrench.Mode.Build.Flags.Snap))
					{
						snap = placement.snap ?? placement.size;
					}

					//var pos_a_raw = build.pos_a ?? pos_raw;
					//var pos_b_raw = build.pos_b ?? pos_raw;

					pos = Wrench.Mode.Build.ConstrainPosition(in placement, pos_raw, pos_a_raw, pos_b_raw, snap: snap);
					//pos_a = Build.ConstrainPosition(in placement, pos_a_raw ?? pos + new Vector2(0, placement.length_step), pos_a_raw, pos_b_raw, snap: snap);
					pos_a = pos_a_raw.HasValue ? Wrench.Mode.Build.ConstrainPosition(in placement, pos_a_raw.Value, pos_a_raw, pos_b_raw, snap: snap) : pos;
					pos_b = pos_b_raw.HasValue ? Wrench.Mode.Build.ConstrainPosition(in placement, pos_b_raw.Value, pos_a_raw, pos_b_raw, snap: snap) : pos + (normal * normal_distance);
					//pos += placement.offset;
					//pos_a += placement.offset;
					//pos_b += placement.offset;

					var offset = placement.offset * scale;

					pos_final = pos;
					rot_final = 0.00f;

					switch (placement.type)
					{
						case Placement.Type.Line:
						{
							var dir = (pos_a - pos_b).GetNormalized(out var len);
							pos_a += dir * placement.size * 0.50f;
							pos_b -= dir * placement.size * 0.50f;

							pos_a = pos_a.Snap(0.125f);
							pos_b = pos_b.Snap(0.125f);

							if (pos_a_raw.HasValue && placement.snapping_flags.HasAll(Placement.SnappingFlags.Align_To_Surface | Placement.SnappingFlags.Rotate_Line_Normal))
							{
								var cross = Maths.Cross(dir, normal);
								if (cross < 0.00f)
								{
									(pos_a, pos_b) = (pos_b, pos_a);
								}
								//App.WriteLine($"{dir}; {normal}; {Maths.Cross(dir, normal)}");
							}

							pos_final = pos_a;
						}
						break;

						case Placement.Type.Simple:
						{
							if (placement.rotation_max > Maths.epsilon)
							{
								var dir = (pos_raw - (pos_a_raw ?? pos_raw)).GetNormalized(out var len);
								if (placement.rotation_offset != 0.00f) dir = dir.RotateByRad(placement.rotation_offset);

								dir = Vector2.Lerp(normal, dir, Maths.Clamp((len - 0.25f) * 2.00f, 0.00f, 1.00f));

								var rot_max = Maths.Snap(MathF.Abs(placement.rotation_max), placement.rotation_step);
								var rot = Maths.Clamp(Maths.Snap(dir.GetAngleRadians() + (MathF.PI * 0.50f), placement.rotation_step), -rot_max, +rot_max);

								pos_final = pos_a + (offset.RotateByRad(rot)); // - offset;
								rot_final = rot;
							}
						}
						break;
					}

					bb = placement.type switch
					{
						Placement.Type.Rectangle => AABB.Centered(pos, placement.size),
						Placement.Type.Circle => AABB.Circle(pos, placement.radius),
						Placement.Type.Line => AABB.Line(pos_a, pos_b, placement.size.X),
						Placement.Type.Simple => AABB.Centered(pos_final, placement.size),
						_ => AABB.Centered(pos, placement.size)
					};
				}

				public partial struct PlaceRPC: Net.IRPC<Wrench.Mode.Build.Data>
				{
					public Wrench.Mode.Build.Flags flags;
					public Vector2 pos_origin;
					public Vector2 pos_raw;
					public Vector2? pos_a_raw;
					public Vector2? pos_b_raw;
					public Vector2? normal;

#if SERVER
					public void Invoke(Net.IRPC.Context rpc, ref Wrench.Mode.Build.Data build)
					{
						Assert.IsRealNumber(this.pos_origin);
						Assert.IsRealNumber(this.pos_raw);

						Assert.IsRealNumberOrNull(this.pos_a_raw);
						Assert.IsRealNumberOrNull(this.pos_b_raw);
						Assert.IsRealNumberOrNull(this.normal);

						ref var region = ref rpc.entity.GetRegion();

						var errors = Wrench.Mode.Build.Errors.None;

						var success = false;
						var skip_support = false;
						var support = 0.00f;
						var clearance = 0.00f;
						var amount_multiplier = 1.00f;

						//ref var player = ref connection.GetPlayerData();
						ref var transform_entity = ref rpc.entity.GetComponent<Transform.Data>();
						ref var recipe = ref build.recipe.GetData();
						ref var character_data = ref rpc.connection.GetCharacter(out var h_character);

						if (recipe.IsNotNull() && transform_entity.IsNotNull() && character_data.IsNotNull())
						{
							//Crafting.Context.NewFromPlayer(ref region, ref player, entity, out var context);
							Crafting.Context.NewFromCharacter(ref region.AsCommon(), h_character, rpc.entity, out var context);
							//var ent_character = connection.enti

							if (recipe.type == Recipe.Type.Architecture || recipe.type == Recipe.Type.Industry || recipe.type == Recipe.Type.Buildings || recipe.type == Recipe.Type.Machinery)
							{
								ref var product = ref recipe.products[0];
								if (recipe.placement.TryGetValue(out var placement))
								{
									var scale = new Vector2(1, 1);
									var normal_surface = this.normal.GetNormalized(out var normal_distance) ?? new Vector2(0.00f, -1.00f);

									if (placement.flags.HasAny(Placement.Flags.Allow_Mirror_X)) scale.X.ToggleSign(this.pos_raw.X < this.pos_origin.X);

									var random = XorRandom.New(true);

									Wrench.Mode.Build.GetPlacementInfo(placement: ref placement,
										flags: this.flags,
										pos_raw: this.pos_raw,
										pos_a_raw: this.pos_a_raw,
										pos_b_raw: this.pos_b_raw,
										scale: scale,
										normal: normal_surface,
										normal_distance: normal_distance,
										pos: out var pos,
										pos_a: out var pos_a,
										pos_b: out var pos_b,
										pos_final: out var pos_final,
										rot_final: out var rot_final,
										bb: out var bb);

									var transform = new Transform.Data(pos_final, rot_final, scale);
									var matrix = transform.GetMatrix3x2();

									var ent_parent = rpc.entity.GetParent(Relation.Type.Child);
									//var inventory = player.GetInventory();
									var h_faction = character_data.faction;

									if (product.type == Crafting.Product.Type.Block)
									{
										ref var block = ref product.block.GetData();
										if (block.IsNotNull())
										{
											var tile_flags = block.tile_flags | product.tile_flags;

											ref var terrain = ref region.GetTerrain();

											Wrench.Mode.Build.EvaluateBlock(region: ref region, placement: in placement, errors: ref errors, skip_support: ref skip_support, placed_block_count: out var placed_block_count, bb: bb, block: product.block, tile_flags: tile_flags, pos: pos, pos_a: pos_a, pos_b: pos_b);
											amount_multiplier = placed_block_count;
											Wrench.Mode.Build.Evaluate(region: ref region, entity: rpc.entity, placement: in placement, errors: ref errors, skip_support: ref skip_support, support: out support, clearance: out clearance, bb: bb, transform: in transform, placement_range: build.placement_range, pos: pos, pos_a: pos_a, pos_b: pos_b, faction_id: h_faction);

											//if (!Crafting.Evaluate(entity, ent_parent, transform.position, ref recipe.requirements, inventory: inventory, amount_multiplier: amount_multiplier, h_faction: h_faction)) errors |= Errors.RequirementsNotMet;
											if (!context.Evaluate(requirements: recipe.requirements.AsSpan(), amount_multiplier: amount_multiplier)) errors |= Errors.RequirementsNotMet;

											if (errors == Wrench.Mode.Build.Errors.None)
											{
												//Crafting.Consume(ent_parent, transform.position, ref recipe.requirements, inventory: inventory, amount_multiplier: amount_multiplier, sync: true);
												context.Consume(requirements: recipe.requirements.AsSpan(), amount_multiplier: amount_multiplier);

												var tile_meta = 0u;

												// TODO: WIP/experimental, looks kinda ugly atm in most cases
												if (placement.flags.HasAny(Placement.Flags.Align_Background))
												{
													var offset_top_left = ((pos_a + (placement.size * 0.50f)) * App.pixels_per_unit).Floor();
													offset_top_left.X %= placement.size.X;
													offset_top_left.Y %= placement.size.Y;

													var tile_offset_x = ((uint)offset_top_left.X) & 0xf; // (((uint)(16 - offset_top_left.X)) & 0xf);
													var tile_offset_y = ((uint)offset_top_left.Y) & 0xf; // (((uint)(16 - offset_top_left.Y)) & 0xf);

													//App.WriteLine($"{tile_offset_x}; {tile_offset_y} {offset_top_left}");

													tile_meta |= tile_offset_x;
													tile_meta |= tile_offset_y << 4;
												}

												var args = new SetTileFuncArgs(block: product.block, tile_flags: tile_flags, count: 0, max_health: block.max_health, meta: (byte)tile_meta, mappings_replace: placement.mappings_replace);
												switch (placement.type)
												{
													case Placement.Type.Rectangle:
													{
														terrain.IterateRect(world_position: pos, size: placement.size * App.pixels_per_unit, argument: ref args,
															func: placement.flags.HasAny(Placement.Flags.Replace) ? SetTileFuncReplace : SetTileFunc,
															dirty_flags: Chunk.DirtyFlags.Sync | Chunk.DirtyFlags.Neighbours | Chunk.DirtyFlags.Collider,
															iteration_flags: Terrain.IterationFlags.Create_If_Empty);
													}
													break;

													case Placement.Type.Circle:
													{
														terrain.IterateCircle(world_position: pos, radius: placement.radius * App.pixels_per_unit, argument: ref args,
															func: placement.flags.HasAny(Placement.Flags.Replace) ? SetTileFuncReplace : SetTileFunc,
															dirty_flags: Chunk.DirtyFlags.Sync | Chunk.DirtyFlags.Neighbours | Chunk.DirtyFlags.Collider,
															iteration_flags: Terrain.IterationFlags.Create_If_Empty);
													}
													break;

													case Placement.Type.Line:
													{
														//App.WriteLine($"{pos}; {pos_a}; {pos_b}");
														terrain.IterateSquareLine(world_position_a: pos_a, world_position_b: pos_b, thickness: placement.size.X, argument: ref args,
															func: placement.flags.HasAny(Placement.Flags.Replace) ? SetTileFuncReplace : SetTileFunc,
															dirty_flags: Chunk.DirtyFlags.Sync | Chunk.DirtyFlags.Neighbours | Chunk.DirtyFlags.Collider,
															iteration_flags: Terrain.IterationFlags.Create_If_Empty);
													}
													break;
												}

												//var ent_character = player.GetControlledCharacter().entity;
												//if (ent_character.IsValid())
												//{
												//	//App.WriteLine($"char {ent_character}");

												//	var ev = new Character.BuildEvent()
												//	{
												//		recipe = build.recipe,
												//		block = product.block,
												//		amount = args.count
												//	};
												//	ent_character.TriggerEvent(ref ev);
												//}

												success = true;
											}
										}
									}
									else if (product.type == Crafting.Product.Type.Prefab || product.type == Crafting.Product.Type.Resource)
									{
										var h_prefab = default(Prefab.Handle);
										if (product.type == Crafting.Product.Type.Prefab) h_prefab = product.prefab;
										else if (product.type == Crafting.Product.Type.Resource) h_prefab = product.material.GetPrefabHandle();

										if (h_prefab.TryGetPrefab(out var prefab))
										{
											//var prefab_handle = product.prefab;

											//var scale = new Vector2(1, 1);

											//if (placement.flags.HasAny(Placement.Flags.Allow_Mirror_X) && this.pos_raw.X < transform.position.X)
											//{
											//	scale.X *= -1.00f;
											//}

											Wrench.Mode.Build.EvaluatePrefab(region: ref region, placement: in placement, errors: ref errors, skip_support: ref skip_support, h_prefab: h_prefab, matrix: in matrix, pos_a: pos_a, pos_b: pos_b);
											amount_multiplier = 1.00f;
											Wrench.Mode.Build.Evaluate(region: ref region,  entity: rpc.entity, placement: in placement, errors: ref errors, skip_support: ref skip_support, support: out support, clearance: out clearance, bb: bb, transform: in transform, placement_range: build.placement_range, pos: pos_final, pos_a: pos_a, pos_b: pos_b, faction_id: h_faction);

											if (placement.type == Placement.Type.Line)
											{
												amount_multiplier += Vector2.Distance(pos_a, pos_b);
											}

											if (recipe.construction.HasValue)
											{
												var construction = recipe.construction.Value;

												//if (!Crafting.Evaluate(entity, ent_parent, transform.position, ref construction.requirements, inventory: inventory, amount_multiplier: amount_multiplier, h_faction: h_faction)) errors |= Errors.RequirementsNotMet;
												if (!context.Evaluate(requirements: construction.requirements.AsSpan(), amount_multiplier: amount_multiplier)) errors |= Errors.RequirementsNotMet;

												if (errors == Wrench.Mode.Build.Errors.None)
												{
													//Crafting.Consume(ent_parent, transform.position, ref construction.requirements, inventory: inventory, amount_multiplier: amount_multiplier, sync: true);
													context.Consume(requirements: construction.requirements.AsSpan(), amount_multiplier: amount_multiplier);

													var dismantlable_tmp = new Dismantlable.Data();
													dismantlable_tmp.yield = 0.90f;
													dismantlable_tmp.required_work = 5.00f;
													var dismantlable_tmp_items = dismantlable_tmp.items.AsSpan();

													var requirements_construction = construction.requirements;
													for (var i = 0; i < requirements_construction.Length; i++)
													{
														ref var requirement = ref requirements_construction[i];
														switch (requirement.type)
														{
															case Crafting.Requirement.Type.Resource:
															{
																dismantlable_tmp_items.Add(Shipment.Item.Resource(requirement.material, requirement.amount));
															}
															break;
														}
													}

													//var order_tmp = new Crafting.Order();
													//order_tmp.amount_multiplier = 1.00f;
													//order_tmp.flags |= Crafting.Order.Flags.InProgress;
													//order_tmp.h_recipe = build.recipe;

													//var shipment_tmp = new Shipment.Data();
													//shipment_tmp.flags |= Shipment.Flags.Keep_Items | Shipment.Flags.No_GUI;

													//var work_count = 0;
													//var item_count = 0;

													//var quantity = product.amount;

													//var requirements = recipe.requirements;
													//for (var i = 0; i < requirements.Length; i++)
													//{
													//	ref var requirement = ref requirements[i];
													//	switch (requirement.type)
													//	{
													//		case Crafting.Requirement.Type.Work:
													//		{
													//			order_tmp.work[work_count++] = new(requirement.work, Work.Amount.Flags.Pending, requirement.group, requirement.difficulty, (byte)i, requirement.amount * Constants.Construction.work_requirement_multiplier);
													//		}
													//		break;

													//		case Crafting.Requirement.Type.Resource:
													//		{
													//			shipment_tmp.items[item_count++] = Shipment.Item.Resource(requirement.material, 0.00f, requirement.amount * Constants.Construction.resource_requirement_multiplier);
													//		}
													//		break;
													//	}
													//}

													var h_recipe = build.recipe;

													region.SpawnPrefab(prefab: construction.prefab, position: pos_final, rotation: rot_final, scale: scale, faction_id: h_faction).ContinueWith((ent) =>
													{
														ref var dismantlable = ref ent.GetOrAddComponent<Dismantlable.Data>(sync: true, ignore_mask: true);
														if (dismantlable.IsNotNull())
														{
															dismantlable = dismantlable_tmp;
														}

														ref var construction = ref ent.GetComponent<Construction.Data>();
														if (construction.IsNotNull())
														{
															construction.h_recipe = h_recipe;

															//construction.prefab = h_prefab;
															//construction.order = order;
															//construction.stage = Construction.Stage.Materials;
															//construction.quantity = quantity;

															ref var order = ref ent.GetOrAddPair<Construction.Data, Crafting.Order>(sync: true, ignore_mask: true, sync_if_added: true);
															if (order.IsNotNull())
															{
																//order = new(); // order_tmp;
															}
														}

														ref var shipment = ref ent.GetOrAddComponent<Shipment.Data>(sync: true, ignore_mask: true);
														if (shipment.IsNotNull())
														{
															//shipment = shipment_tmp;
														}
													});

													success = true;
												}
											}
											else
											{
												//if (!Crafting.Evaluate(entity, ent_parent, transform.position, ref recipe.requirements, inventory: inventory, amount_multiplier: amount_multiplier, h_faction: h_faction)) errors |= Errors.RequirementsNotMet;
												if (!context.Evaluate(requirements: recipe.requirements.AsSpan(), amount_multiplier: amount_multiplier)) errors |= Errors.RequirementsNotMet;

												if (errors == Wrench.Mode.Build.Errors.None)
												{
													//Crafting.Consume(ent_parent, transform.position, ref recipe.requirements, inventory: inventory, amount_multiplier: amount_multiplier, sync: true);
													context.Consume(requirements: recipe.requirements.AsSpan(), amount_multiplier: amount_multiplier);

													var dismantlable_tmp = new Dismantlable.Data();
													dismantlable_tmp.yield = recipe.dismantle_yield;
													dismantlable_tmp.required_work = recipe.dismantle_work;
													var dismantlable_tmp_items = dismantlable_tmp.items.AsSpan();

													var requirements = recipe.requirements;
													for (var i = 0; i < requirements.Length; i++)
													{
														ref var requirement = ref requirements[i];
														switch (requirement.type)
														{
															case Crafting.Requirement.Type.Resource:
															{
																dismantlable_tmp_items.Add(Shipment.Item.Resource(requirement.material, requirement.amount));
																//dismantlable_tmp.resources[i] = new Resource.Data(requirement.material, requirement.amount);
															}
															break;
														}
													}

													if (placement.type == Placement.Type.Line && placement.flags.HasAny(Placement.Flags.Place_Line_Rotated))
													{
														var dir = (pos_b - pos_a).GetNormalized(out var len);
														rot_final = dir.GetAngleRadians();
													}

													region.SpawnPrefab(prefab: h_prefab, position: pos_final, rotation: rot_final, scale: scale, faction_id: h_faction).ContinueWith((ent) =>
													{
														ref var dismantlable = ref ent.GetOrAddComponent<Dismantlable.Data>(sync: true, ignore_mask: true);
														if (!dismantlable.IsNull())
														{
															dismantlable = dismantlable_tmp;
														}

														ref var resizable = ref ent.GetComponent<Resizable.Data>();
														if (!resizable.IsNull())
														{
															resizable.a = Vector2.Zero;

															if (placement.type == Placement.Type.Line && placement.flags.HasAny(Placement.Flags.Place_Line_Rotated))
															{
																resizable.b = new Vector2((pos_b - pos_a).Length(), 0.00f);
															}
															else
															{
																resizable.b = pos_b - pos_a;
															}

															resizable.Modified(ent, sync: true);
														}
													});

													//var ent_character = player.GetControlledCharacter().entity;
													//if (ent_character.IsValid())
													//{
													//	var ev = new Character.BuildEvent()
													//	{
													//		recipe = build.recipe,
													//		prefab = prefab_handle,
													//		amount = 1
													//	};
													//	ent_character.TriggerEvent(ref ev);
													//}

													success = true;
												}
											}
										}
									}

									if (success)
									{
										if (placement.sound.id != 0) Sound.Play(ref region, placement.sound, pos, volume: 1.00f, pitch: random.NextFloatRange(0.80f, 1.00f), priority: 0.35f);
										Shake.Emit(ref region, pos, 0.20f, 0.20f);
									}
									else
									{
										Notification.Push(ref rpc.connection, $"Cannot place: {errors.ToFormattedString()}", Color32BGRA.Red, sound: "error", volume: 0.60f);
									}

									build.next_place = region.GetFixedTime() + placement.cooldown;
								}
							}
						}
					}
#endif
				}

				[Flags]
				public enum Errors: uint
				{
					None = 0,

					[Name("Out of reach")]
					OutOfRange = 1 << 0,

					[Name("Requirements not met")]
					RequirementsNotMet = 1 << 1,

					[Name("Empty")]
					ZeroCount = 1 << 2,

					[Name("Obstructed")]
					Obstructed = 1 << 3,

					[Name("Not enough support")]
					Unsupported = 1 << 4,

					[Name("Outside territory")]
					Claimed = 1 << 5,

					[Name("Too short")]
					MinLength = 1 << 6,

					[Name("Too long")]
					MaxLength = 1 << 7,

					[Name("Invalid")]
					Invalid = 1 << 8,

					[Name("Requires ground")]
					NoTerrain = 1 << 9,
				}

				[Flags]
				public enum Flags: uint
				{
					None = 0,

					Ruler = 1 << 0,
					Zones = 1 << 1,
					Snap = 1 << 2
				}

				public static Vector2 GetSnappedPosition(Vector2 world_position, Vector2? pivot, Vector2 step)
				{
					var pos = world_position;
					var offset = default(Vector2);
					var grid = new Vector2(Chunk.unit_width, Chunk.unit_height) / (Vector2.Max(step, new Vector2(0.125f, 0.125f)) * 8);

					if (pivot.HasValue) offset = pivot.Value - (step);

					pos -= offset;
					pos *= grid;
					pos.X = MathF.Round(pos.X);
					pos.Y = MathF.Round(pos.Y);
					pos /= grid;
					pos += offset;

					//pos.X = (int)pos.X;
					//pos.Y = (int)pos.Y;

					return pos;
				}

				#region CalculateBlockCount
				private record struct CalculateBlockCountArgs(IBlock.Handle block, TileFlags tile_flags, int count, float max_health, Dictionary<IBlock.Handle, Block.Mapping> mappings_replace = null);
				private static void CountTileFunc(ref Tile tile, int x, int y, byte mask, ref CalculateBlockCountArgs args)
				{
					if ((tile.BlockID == 0 || args.tile_flags.HasAny(TileFlags.Solid)) && tile.Flags.HasNone(TileFlags.Solid))
					{
						args.count++;
					}
				}

				private static void CountTileFuncReplace(ref Tile tile, int x, int y, byte mask, ref CalculateBlockCountArgs args)
				{
					//if (tile.BlockID != 0 && !tile.Flags.HasAll(TileFlags.Solid) && tile.BlockID != arg.block.id)
					//if (tile.BlockID != 0 && tile.BlockID != args.block.id && !tile.Flags.HasAny(TileFlags.No_Replace) && (tile.Flags.HasAll(TileFlags.Solid) == args.tile_flags.HasAll(TileFlags.Solid) && tile.ScaledHealth <= args.max_health))
					//{
					//	args.count++;
					//}

					//if (args.mappings_replace.ContainsKey(tile.BlockID))
					//{
					if (args.mappings_replace.TryGetValue(tile.GetBlockHandle(), out var mapping) && tile.Flags.HasAny(TileFlags.Solid) == mapping.flags.HasAny(TileFlags.Solid))
					{
						args.count++;
					}
				}

				public static int CalculateBlockCount(ref Region.Data region, in Placement placement, IBlock.Handle block, TileFlags tile_flags, Vector2 pos, Vector2? pos_a = default, Vector2? pos_b = default)
				{
					ref var terrain = ref region.GetTerrain();

					var args = new CalculateBlockCountArgs(block: block, tile_flags: tile_flags, count: 0, max_health: block.GetData().max_health, mappings_replace: placement.mappings_replace);
					switch (placement.type)
					{
						case Placement.Type.Rectangle:
						{
							terrain.IterateRect(world_position: pos, size: placement.size * App.pixels_per_unit, argument: ref args,
								func: placement.flags.HasAny(Placement.Flags.Replace) ? CountTileFuncReplace : CountTileFunc,
								iteration_flags: Terrain.IterationFlags.Iterate_Empty);
						}
						break;

						case Placement.Type.Circle:
						{
							terrain.IterateCircle(world_position: pos, radius: placement.radius * App.pixels_per_unit, argument: ref args,
								func: placement.flags.HasAny(Placement.Flags.Replace) ? CountTileFuncReplace : CountTileFunc,
								iteration_flags: Terrain.IterationFlags.Iterate_Empty);
						}
						break;

						case Placement.Type.Line:
						{
							terrain.IterateSquareLine(world_position_a: pos_a ?? pos, world_position_b: pos_b ?? pos, thickness: placement.size.X, argument: ref args,
								func: placement.flags.HasAny(Placement.Flags.Replace) ? CountTileFuncReplace : CountTileFunc,
								iteration_flags: Terrain.IterationFlags.Iterate_Empty);
						}
						break;
					}

					return args.count;
				}
				#endregion

#if CLIENT
				#region DrawTile
				private record struct DrawTileArgs(Vector2 offset, Vector2 pixel_size, TileFlags tile_flags, IBlock.Handle block, float max_health, Color32BGRA color, Dictionary<IBlock.Handle, Block.Mapping> mappings_replace = null);
				static void DrawTileFunc(ref Tile tile, int x, int y, byte mask, ref DrawTileArgs args)
				{
					var pos = args.offset + new Vector2(args.pixel_size.X * x, args.pixel_size.Y * y);
					if ((tile.BlockID == 0 || args.tile_flags.HasAny(TileFlags.Solid)) && tile.Flags.HasNone(TileFlags.Solid))
					{
						GUI.DrawRectFilled(pos, pos + args.pixel_size, args.color);
					}
					else
					{
						GUI.DrawRectFilled(pos, pos + args.pixel_size, color_gray.WithAlpha(100));
					}
				}

				static void DrawTileFuncReplace(ref Tile tile, int x, int y, byte mask, ref DrawTileArgs args)
				{
					var pos = args.offset + new Vector2(args.pixel_size.X * x, args.pixel_size.Y * y);
					//if (tile.BlockID != 0 && !tile.Flags.HasAll(TileFlags.Solid) && tile.BlockID != args.block.id)
					//if (tile.BlockID != 0 && tile.BlockID != args.block.id && !tile.Flags.HasAny(TileFlags.No_Replace) && (tile.Flags.HasAll(TileFlags.Solid) == args.tile_flags.HasAll(TileFlags.Solid) && tile.ScaledHealth <= args.max_health))
					if (args.mappings_replace.TryGetValue(tile.GetBlockHandle(), out var mapping) && tile.Flags.HasAny(TileFlags.Solid) == mapping.flags.HasAny(TileFlags.Solid))
					{
						GUI.DrawRectFilled(pos, pos + args.pixel_size, args.color);
					}
					else
					{
						//GUI.DrawRectFilled(pos, pos + args.rect_size, args.color_gray);
						GUI.DrawRectFilled(pos, pos + args.pixel_size, color_gray.WithAlpha(100));
					}
				}

				private record struct DrawFoundationArgs(Vector2 offset, Vector2 pixel_size, TileFlags tile_flags, float max_health, Color32BGRA color);
				static void DrawFoundationFunc(ref Tile tile, int x, int y, byte mask, ref DrawFoundationArgs args)
				{
					var pos = args.offset + new Vector2(args.pixel_size.X * x, args.pixel_size.Y * y);
					if (tile.Flags.HasAny(args.tile_flags))
					{
						GUI.DrawRectFilled(pos, pos + args.pixel_size, args.color);
					}
					else
					{
						GUI.DrawRectFilled(pos, pos + args.pixel_size, color_gray.WithAlpha(100));
					}
				}
				#endregion
#endif

				public static readonly Color32BGRA color_ok = 0xff00ff00;
				public static readonly Color32BGRA color_error = 0xffff0000;
				public static readonly Color32BGRA color_gray = 0xff404040;
				public static readonly Color32BGRA color_yellow = 0xffffff00;

#if SERVER
				#region SetTile
				private record struct SetTileFuncArgs(IBlock.Handle block, TileFlags tile_flags, int count, float max_health, byte meta, Dictionary<IBlock.Handle, Block.Mapping> mappings_replace = null);
				static void SetTileFunc(ref Tile tile, int x, int y, byte mask, ref SetTileFuncArgs args)
				{
					if ((tile.BlockID == 0 || args.tile_flags.HasAny(TileFlags.Solid)) && tile.Flags.HasNone(TileFlags.Solid))
					{
						tile.Reset();

						tile.Health = 255;
						tile.BlockID = (byte)args.block.id;
						tile.Flags |= args.tile_flags;
						tile.Meta = args.meta;

						args.count++;
					}
				}

				static void SetTileFuncReplace(ref Tile tile, int x, int y, byte mask, ref SetTileFuncArgs args)
				{
					//if (tile.BlockID != 0 && !tile.Flags.HasAll(TileFlags.Solid) && tile.BlockID != arg.block.id)
					//if (tile.BlockID != 0 && tile.BlockID != args.block.id && !tile.Flags.HasAny(TileFlags.No_Replace) && (tile.Flags.HasAll(TileFlags.Solid) == args.tile_flags.HasAll(TileFlags.Solid) && tile.ScaledHealth <= args.max_health))
					//{
					//	tile.Reset();

					//	tile.Health = 255;
					//	tile.BlockID = (byte)args.block.id;
					//	tile.Flags |= args.tile_flags;

					//	args.count++;
					//}

					if (args.mappings_replace.TryGetValue(tile.GetBlockHandle(), out var mapping) && tile.Flags.HasAny(TileFlags.Solid) == mapping.flags.HasAny(TileFlags.Solid))
					{
						tile.Set(mapping);
						args.count++;
					}
				}
				#endregion
#endif

				#region CalculateSupport
				private record struct CalculateSupportArgs(int support_count, int blocked_count, int total_count, Dictionary<IBlock.Handle, Block.Mapping> mappings_replace = null);
				private static void CalculateSupportFunc(ref Tile tile, int x, int y, byte mask, ref CalculateSupportArgs args)
				{
					if ((tile.BlockID != 0 || tile.Flags.HasAny(TileFlags.Ground))) // && !tile.Flags.HasAll(TileFlags.Solid))
					{
						args.support_count++;
					}

					if (tile.Flags.HasAny(TileFlags.Solid))
					{
						args.blocked_count++;
					}

					args.total_count++;
				}

				private static void CalculateSupportFuncReplace(ref Tile tile, int x, int y, byte mask, ref CalculateSupportArgs args)
				{
					if ((tile.BlockID != 0 || tile.Flags.HasAny(TileFlags.Ground))) // && !tile.Flags.HasAll(TileFlags.Solid))
					{
						args.support_count++;
					}

					if (tile.Flags.HasAny(TileFlags.Solid))
					{
						args.blocked_count++;
					}

					args.total_count++;
				}

				private record struct CalculateFoundationSupportArgs(int support_count, int blocked_count, int total_count, TileFlags tile_flags, float max_health);
				private static void CalculateFoundationSupportFunc(ref Tile tile, int x, int y, byte mask, ref CalculateFoundationSupportArgs args)
				{
					if (tile.Flags.HasAny(args.tile_flags))
					{
						args.support_count++;
					}
					else
					{

					}

					args.total_count++;
				}

				private record struct CalculateFoundationClearanceArgs(int clear_count, int blocked_count, int total_count, TileFlags tile_flags);
				private static void CalculateFoundationClearanceFunc(ref Tile tile, int x, int y, byte mask, ref CalculateFoundationClearanceArgs args)
				{
					if (tile.Flags.HasAny(args.tile_flags))
					{
						args.blocked_count++;
					}
					else
					{
						args.clear_count++;
					}

					args.total_count++;
				}

				public static void CalculateSupport(ref Region.Data region, in Placement placement, in Transform.Data transform, out int support_count, out int blocked_count, out int clear_count, out int total_count_support, out int total_count_clearance, Vector2 pos, Vector2? pos_a = default, Vector2? pos_b = default)
				{
					ref var terrain = ref region.GetTerrain();

					//var args = new CalculateSupportArgs(valid_count: 0, total_count: 0);
					//terrain.IterateRect(pos, (placement.size * App.pixels_per_unit) + new Vector2(2, 2), ref args, CalculateSupportFunc);

					//support_count = 0;
					//blocked_count = 0;
					//clear_count = 0;

					//total_count_support = 0;
					//total_count_clearance = 0;


					if (placement.rect_foundation.TryGetValue(out var rect_foundation))
					{
						var tile_flags = placement.tileflags_foundation;

						var quad_world = transform.LocalToWorld(rect_foundation - placement.offset).Snap(0.125f);
						var quad_world_aabb = quad_world.GetAABB();

						var args = new CalculateFoundationSupportArgs(support_count: 0, blocked_count: 0, total_count: 0, tile_flags: tile_flags, max_health: 10000.00f);

						//terrain.IterateRect(quad_world_aabb, argument: ref args,
						//	func: DrawFoundationFunc,
						//	iteration_flags: Terrain.IterationFlags.Iterate_Empty);

						terrain.IterateRectRotated(quad_world, argument: ref args,
							func: CalculateFoundationSupportFunc,
							iteration_flags: Terrain.IterationFlags.Iterate_Empty);

						support_count = args.support_count;
						blocked_count = args.blocked_count;
						total_count_support = args.total_count;
					}
					else
					{
						var args = new CalculateSupportArgs(support_count: 0, blocked_count: 0, total_count: 0, mappings_replace: placement.mappings_replace);
						switch (placement.type)
						{
							case Placement.Type.Simple:
							case Placement.Type.Rectangle:
							{
								terrain.IterateRect(world_position: pos, size: (placement.size * App.pixels_per_unit) + new Vector2(2.00f), argument: ref args,
									func: placement.flags.HasAny(Placement.Flags.Replace) ? CalculateSupportFuncReplace : CalculateSupportFunc,
									iteration_flags: Terrain.IterationFlags.Iterate_Empty);
							}
							break;

							case Placement.Type.Circle:
							{
								terrain.IterateCircle(world_position: pos, radius: (placement.radius * App.pixels_per_unit) + 2.00f, argument: ref args,
									func: placement.flags.HasAny(Placement.Flags.Replace) ? CalculateSupportFuncReplace : CalculateSupportFunc,
									iteration_flags: Terrain.IterationFlags.Iterate_Empty);
							}
							break;

							case Placement.Type.Line:
							{
								var dir = ((pos_b ?? pos) - (pos_a ?? pos)).GetNormalized(out var len);

								terrain.IterateSquareLine(world_position_a: (pos_a ?? pos) - (dir * 0.125f), world_position_b: (pos_b ?? pos) + (dir * 0.125f),
									thickness: placement.size.X, argument: ref args,
									func: placement.flags.HasAny(Placement.Flags.Replace) ? CalculateSupportFuncReplace : CalculateSupportFunc,
									iteration_flags: Terrain.IterationFlags.Iterate_Empty);
							}
							break;
						}

						//return args.total_count > 0 ? args.valid_count / (float)args.total_count : 0.00f;

						support_count = args.support_count;
						blocked_count = args.blocked_count;
						total_count_support = args.total_count;
					}

					if (placement.rect_clearance.TryGetValue(out var rect_clearance))
					{
						var tile_flags = placement.tileflags_clearance;

						var quad_world = transform.LocalToWorld(rect_clearance - placement.offset).Snap(0.125f);
						var quad_world_aabb = quad_world.GetAABB();

						var args = new CalculateFoundationClearanceArgs(clear_count: 0, blocked_count: 0, total_count: 0, tile_flags: tile_flags);
						terrain.IterateRectRotated(quad_world, argument: ref args,
							func: CalculateFoundationClearanceFunc,
							iteration_flags: Terrain.IterationFlags.Iterate_Empty);

						total_count_clearance = args.total_count;
						clear_count = args.clear_count;
					}
					else
					{
						total_count_clearance = total_count_support;
						clear_count = total_count_clearance - blocked_count;
					}
				}
				#endregion

				public static Vector2 ConstrainPosition(in Placement placement, Vector2 pos, Vector2? pos_a = null, Vector2? pos_b = null, Vector2? snap = null)
				{
					if (pos_a.HasValue)
					{
						var pivot = Wrench.Mode.Build.GetSnappedPosition(pos_a.Value, null, new Vector2(0.125f));
						pos = Wrench.Mode.Build.GetSnappedPosition(pos, pivot, snap ?? new Vector2(0.125f));

						if (placement.flags.HasAny(Placement.Flags.Snap_Axis)) pos = pos.SnapAxis(pivot);
					}
					else
					{
						pos = Wrench.Mode.Build.GetSnappedPosition(pos, null, new Vector2(0.125f));
					}

					if (placement.type == Placement.Type.Line)
					{
						//if (pos_a.HasValue)
						//{
						//	if (placement.length_min > 0.00f || placement.length_max > 0.00f)
						//	{
						//		var min = placement.length_min > 0.00f ? placement.length_min : 0.250f;
						//		var max = placement.length_max > 0.00f ? placement.length_max : 100.00f;

						//		pos = Maths.ClampRadiusSnapped2(pos, pos_a.Value, min, max, placement.length_step);
						//	}
						//}

						if (pos_a.HasValue && placement.length_max > Maths.epsilon)
						{
							var pivot = Wrench.Mode.Build.GetSnappedPosition(pos_a.Value, null, new Vector2(0.125f));
							pos = Maths.ClampRadiusSnapped2(pos, pivot, placement.length_min, placement.length_max, placement.length_step);
						}

						if (placement.flags.HasAny(Placement.Flags.Lock_X))
						{
							if (pos_a.HasValue)
							{
								pos.X = pos_a.Value.X;
							}
						}

						if (placement.flags.HasAny(Placement.Flags.Lock_Y))
						{
							if (pos_a.HasValue)
							{
								pos.Y = pos_a.Value.Y;
							}
						}
					}

					return pos;
				}
			}
		}
	}
}
