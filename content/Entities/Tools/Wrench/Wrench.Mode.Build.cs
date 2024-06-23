
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
				public partial struct Data: IComponent, Wrench.IMode
				{
					[Asset.Ignore] public IRecipe.Handle recipe;
					[Asset.Ignore] public Build.Flags flags;

					[Asset.Ignore, Save.Ignore, Net.Ignore] public float next_place;

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 24, 24, 3, 0);
					public static string Name { get; } = "Build";

					public readonly Crafting.Recipe.Tags RecipeTags => Crafting.Recipe.Tags.Construction;
					public readonly Color32BGRA ColorOk => Color32BGRA.Green;
					public readonly Color32BGRA ColorError => Color32BGRA.Red;
					public readonly Color32BGRA ColorNew => Color32BGRA.Yellow;

					public readonly IRecipe.Handle SelectedRecipe => this.recipe;

#if CLIENT

					//public Entity ent_build;
					//public Build.Data build;
					////public Control.Data control;
					//public Transform.Data transform;


					public static readonly string[] category_names = Enum.GetNames<Build.Category>();
					public static readonly Build.Category[] category_values = Enum.GetValues<Build.Category>();
					public static FixedString64 edit_name_filter;

					public static Crafting.Recipe.Tags edit_tags_filter = Crafting.Recipe.Tags.Architecture;
					public static List<(uint index, float rank)> recipe_indices = new(64);

					[Region.Local] public static Vector2 pos_a_placeholder;
					[Region.Local] public static float next_place_local;

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
										using (GUI.ID<Build.Category>.Push(i + 1))
										{
											var button_tags_filter = Crafting.Recipe.Tags.None;

											switch ((Build.Category)i)
											{
												case Build.Category.Architecture:
												{
													button_tags_filter = Crafting.Recipe.Tags.Architecture;
												}
												break;

												case Build.Category.Construction:
												{
													button_tags_filter = Crafting.Recipe.Tags.Construction;
												}
												break;

												case Build.Category.Industry:
												{
													button_tags_filter = Crafting.Recipe.Tags.Industry;
												}
												break;

												case Build.Category.Buildings:
												{
													button_tags_filter = Crafting.Recipe.Tags.Buildings;
												}
												break;

												case Build.Category.Mechanisms:
												{
													button_tags_filter = Crafting.Recipe.Tags.Mechanisms;
												}
												break;

												case Build.Category.Misc:
												{
													button_tags_filter = Crafting.Recipe.Tags.Misc;
												}
												break;
											}

											if (GUI.DrawIconButton("build.category"u8, ui_icons_builder_categories.WithFrame((uint)i, 0), new(40, 40), color: !is_searching && edit_tags_filter == button_tags_filter ? GUI.col_button_highlight : GUI.col_button))
											{
												edit_tags_filter = button_tags_filter;
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

									var recipes = IRecipe.Database.GetAssets();
									foreach (var d_recipe in recipes)
									{
										ref var recipe = ref d_recipe.GetData();
										if (recipe.type == Crafting.Recipe.Type.Build)
										{
											var size = (Vector2)recipe.icon.GetFrameSize();

											var rank = recipe.rank;
											rank += d_recipe.id * 0.000001f;

											if (recipe.products[0].type == Crafting.Product.Type.Block) rank -= 10000.00f;
											rank += (size.X) * 1.00f;
											rank -= (size.Y) * 10.00f;
											//rank -= size.Y * 0.10f;

											recipe_indices.Add((d_recipe.id, rank));
										}
									}

									recipe_indices.Sort(SortFunc);
									static int SortFunc((uint index, float rank) a, (uint index, float rank) b)
									{
										return a.rank.CompareTo(b.rank);
									}

									//GUI.Text($"{ts.GetMilliseconds():0.0000} ms");

									foreach (var pair in recipe_indices)
									{
										//GUI.Text($"{pair.rank}");

										var h_recipe = (IRecipe.Handle)pair.index;

										ref var recipe = ref h_recipe.GetData(out var recipe_asset);
										if (recipe.IsNotNull() && (is_searching || recipe.tags.HasAny(edit_tags_filter)))
										{
											if (!is_searching || recipe.name.Contains(search_filter, StringComparison.OrdinalIgnoreCase))
											{
												var frame_size = recipe.icon.GetFrameSize(scale);
												frame_size += new Vector2(8, 8);
												frame_size = frame_size.ScaleToNearestMultiple(new Vector2(48, 48));

												using (var cell = grid.Push(frame_size, id: (uint)recipe_asset.GetHashCode()))
												{
													if (cell.group.IsVisible())
													{
														if (GUI.DrawRecipeButton(context: ref context, rect: cell.group.GetOuterRect(), h_recipe: h_recipe, h_recipe_selected: ref this.recipe,
														evaluation_flags: Crafting.EvaluateFlags.None,
														flags_add: GUI.DrawRecipeFlags.None,
														flags_rem: GUI.DrawRecipeFlags.Send_RPC | GUI.DrawRecipeFlags.Products | GUI.DrawRecipeFlags.Highlight | GUI.DrawRecipeFlags.Button | GUI.DrawRecipeFlags.Counter))
														{
															var rpc = new Build.ConfigureRPC
															{
																recipe = h_recipe
															};
															rpc.Send(ent_wrench);
														}

														//var selected = this.recipe.id == pair.index;
														//using (var button = GUI.CustomButton.New(h_recipe, frame_size, sound: GUI.sound_select, sound_volume: 0.10f, enabled: recipe.flags.HasNone(Crafting.Recipe.Flags.Disabled)))
														//{
														//	GUI.Draw9Slice((selected | button.hovered) ? GUI.tex_slot_white_hover : GUI.tex_slot_white, new Vector4(4), button.bb, color: recipe.color_button);
														//	//GUI.DrawSpriteCentered(recipe.icon, button.bb, layer: GUI.Layer.Window, scale: scale);

														//	//var icon_extra = recipe.icon_extra;
														//	//if (icon_extra.texture.id != 0)
														//	//{
														//	//	var icon_extra_size = icon_extra.GetFrameSize();
														//	//	GUI.DrawSpriteCentered(icon_extra, AABB.Centered(button.bb.b - icon_extra_size + recipe.icon_extra_offset, icon_extra_size + recipe.icon_extra_offset), layer: GUI.Layer.Window, scale: 2 * recipe.icon_extra_scale);
														//	//}

														//	//if (recipe.flags.HasAny(Crafting.Recipe.Flags.Custom))
														//	//{
														//	//	var icon_blueprint = ui_icons_crafting_mini.WithFrame((frame_size.X > 48 & frame_size.Y > 48) ? 3u : 2u, 0);
														//	//	GUI.DrawSpriteCentered(icon_blueprint, button.bb.Pad(u: 4, r: 4), layer: GUI.Layer.Window, pivot: new(1.00f, 0.00f), scale: 2.00f, color: Color32BGRA.White.WithAlpha(200));
														//	//}

														//	//if (recipe.flags.HasAny(Crafting.Recipe.Flags.WIP))
														//	//{
														//	//	GUI.TextShadedCenteredRect("WIP"u8, pivot: new(0.50f, 0.50f), rect: button.bb, font: GUI.Font.Superstar, size: 24, color: GUI.font_color_yellow_b, box_shadow: false);
														//	//}

														//	GUI.DrawRecipeIcon(h_recipe, button.bb);

														//	//if (recipe.h_company.TryGetDefinition(out var company_asset))
														//	//{
														//	//	ref var company_data = ref company_asset.GetData();
														//	//	GUI.DrawSpriteCentered(company_data.GetLogo(ICompany.LogoSize.Sign_2x1), button.bb.Pad(u: 4, l: 4), layer: GUI.Layer.Window, pivot: new(0.00f, 0.00f), scale: 2.00f, color: Color32BGRA.White.WithAlpha(200));
														//	//}

														//	if (button.pressed)
														//	{
														//		var rpc = new Build.ConfigureRPC
														//		{
														//			recipe = new IRecipe.Handle(pair.index)
														//		};
														//		rpc.Send(ent_wrench);
														//	}
														//}
														//if (GUI.IsItemHovered())
														//{
														//	using (GUI.Tooltip.New(size: new(300, 0)))
														//	{
														//		using (GUI.Wrap.Push(GUI.RmX))
														//		{
														//			GUI.DrawShopRecipe(ref context, h_recipe,
														//				flags_rem: GUI.DrawRecipeFlags.Products | GUI.DrawRecipeFlags.Scrollbox | GUI.DrawRecipeFlags.Button | GUI.DrawRecipeFlags.Background | GUI.DrawRecipeFlags.Send_RPC | GUI.DrawRecipeFlags.Tooltip);

														//			//using (var row = GUI.Group.New(size: new(GUI.RmX, 0)))
														//			//{
														//			//	GUI.Title(recipe.GetName());

														//			//	GUI.SameLine();
														//			//	var mass = recipe.products.AsSpan().GetTotalMass().GetSum();
														//			//	GUI.TextShadedCentered(mass, format: "0.00 kg", pivot: new(1.00f, 0.50f));
														//			//}

														//			//GUI.TextShaded(recipe.GetDescription().OrDefault(recipe.GetDescriptionFallback()), color: GUI.font_color_default);

														//			//GUI.SeparatorThick(spacing: 16);

														//			//ref var construction = ref recipe.construction.GetRefOrNull();
														//			//if (construction.IsNotNull())
														//			//{
														//			//	GUI.DrawRequirements(ref context, construction.requirements.AsSpan());

														//			//	GUI.SeparatorThick(spacing: 16);
														//			//}

														//			//GUI.DrawRequirements(ref context, recipe.requirements.AsSpan(), highlight: true);
														//		}
														//	}
														//}
														//GUI.FocusableAsset(h_recipe);
													}
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
							using (var window_hud = GUI.Window.HUD("Build.HUD"u8, position: region.WorldToCanvas(mouse.GetInterpolatedPosition() + new Vector2(1, -1)), size: new(256, 0), padding: new(8, 8), pivot: new(0.00f, 0.00f)))
							{
								if (window_hud.show)
								{
									var errors = Build.Errors.None;
									var skip_support = false;
									var support = 0.00f;
									var amount_multiplier = 1.00f;

									ref var recipe = ref this.recipe.GetData();
									if (recipe.IsNotNull() && character.IsNotNull())
									{
										//Crafting.Context.NewFromPlayer(ref region, ref player, ent_wrench, out var context);
										Crafting.Context.NewFromCurrentCharacter(ent_wrench, out var context);

										if (recipe.type == Crafting.Recipe.Type.Build)
										{
											ref var product = ref recipe.products[0];
											if (recipe.placement.TryGetValue(out var placement))
											{
												var pos_raw = mouse.position;
												var pos_origin = wrench_transform.position;

												var h_faction = character.faction;

												if (!pos_a_raw.HasValue && (mouse.GetKeyDown(Mouse.Key.Left) || (placement.type == Placement.Type.Simple && mouse.GetKeyDown(Mouse.Key.Right))))
												{
													pos_a_raw = pos_raw;
												}
												else if (pos_a_raw.HasValue && mouse.GetKeyDown(Mouse.Key.Left) && placement.type == Placement.Type.Line)
												{
													pos_b_raw = pos_raw;
												}

												//if (pos_a_raw.HasValue) // && control.mouse.GetKeyUp(Mouse.Key.Left))


												var flags = this.flags;
												flags.SetFlag(Build.Flags.Snap, !kb.GetKey(Keyboard.Key.LeftShift));

												var scale = new Vector2(1, 1);
												if (placement.flags.HasAny(Placement.Flags.Allow_Mirror_X)) scale.X.ToggleSign(pos_raw.X < pos_origin.X);

												Build.GetPlacementInfo(placement: ref placement,
													flags: flags,
													pos_raw: pos_raw,
													pos_a_raw: pos_a_raw,
													pos_b_raw: pos_b_raw,
													scale: scale,
													pos: out var pos,
													pos_a: out var pos_a,
													pos_b: out var pos_b,
													pos_final: out var pos_final,
													rot_final: out var rot_final,
													bb: out var bb);
												
												var transform = new Transform.Data(pos_final, rot_final, scale);
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

												GUI.DrawTerrainOutline(ref region, pos_raw, 2.00f);

												switch (product.type)
												{
													case Crafting.Product.Type.Block:
													{
														ref var block = ref product.block.GetData();
														if (block.IsNotNull())
														{
															var tile_flags = block.tile_flags | product.tile_flags;

															ref var terrain = ref region.GetTerrain();

															errors |= Build.EvaluateBlock(region: ref region, placement: in placement, skip_support: ref skip_support, placed_block_count: out var placed_block_count, bb: bb, block: product.block, tile_flags: tile_flags, pos: pos, pos_a: pos_a, pos_b: pos_b);
															amount_multiplier = placed_block_count;
															errors |= Build.Evaluate(entity: ent_wrench, placement: in placement, skip_support: ref skip_support, support: out support, bb: bb, transform: in transform, pos: pos, pos_a: pos_a, pos_b: pos_b, faction_id: character.faction);

															if (show_zones || placement.flags.HasAny(Placement.Flags.Require_Claimed) || errors.HasAny(Build.Errors.Claimed))
															{
																Claim.DrawFullOverlay(ref region, show_zones, true);
																Claim.DrawClaimerOverlay(ref region, show_zones, true);
															}

															//if (!Crafting.Evaluate(ent_wrench, ent_parent, this_transform.position, ref recipe.requirements, inventory: inventory, amount_multiplier: amount_multiplier, h_faction: h_faction)) errors |= Errors.RequirementsNotMet;
															if (!context.Evaluate(requirements: recipe.requirements.AsSpan(), amount_multiplier: amount_multiplier)) errors |= Errors.RequirementsNotMet;

															if (errors != Build.Errors.None)
															{
																color_dummy_fg = color_error_fg;
																color_dummy_bg = color_error_bg;

																//GUI.DrawOverlapBB(ref region, bb, Physics.Layer.Solid | Physics.Layer.Building);
															}

															var pixel_size = new Vector2(App.pixels_per_unit_inv) * region.GetWorldToCanvasScale();

															//GUI.DrawCross(region.WorldToCanvas(pos_a), radius: 16, layer: GUI.Layer.Background, color: color_dummy_fg);
															GUI.DrawRect(region.WorldToCanvas(bb), layer: GUI.Layer.Background, color: color_dummy_fg);
															GUI.DrawCross(region.WorldToCanvas(pos_a), color: GUI.font_color_default.WithAlpha(180), radius: region.GetWorldToCanvasScale() * 3.00f);

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
														var prefab_handle = default(Prefab.Handle);
														if (product.type == Crafting.Product.Type.Prefab) prefab_handle = product.prefab;
														else if (product.type == Crafting.Product.Type.Resource)
														{
															ref var material = ref product.material.GetData();
															if (material.IsNotNull())
															{
																prefab_handle = material.prefab;
															}
														}

														if (prefab_handle.TryGetPrefab(out var prefab))
														{
															//var prefab_handle = product.prefab;

															//var scale = new Vector2(1, 1);
															//scale.X.ToggleSign(placement.flags.HasAny(Placement.Flags.Allow_Mirror_X) & pos_raw.X < wrench_transform.position.X);

															errors |= Build.EvaluatePrefab(region: ref region, placement: in placement, skip_support: ref skip_support, bb: bb, pos: pos_final, pos_a: pos_a, pos_b: pos_b);
															amount_multiplier = 1.00f;
															errors |= Build.Evaluate(entity: ent_wrench, placement: in placement, skip_support: ref skip_support, support: out support, bb: bb, transform: in transform, pos: pos_final, pos_a: pos_a, pos_b: pos_b, faction_id: character.faction);

															if (show_zones || placement.flags.HasAny(Placement.Flags.Require_Claimed) || errors.HasAny(Build.Errors.Claimed))
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

															if (errors != Build.Errors.None)
															{
																color_dummy_fg = color_error_fg;
																color_dummy_bg = color_error_bg;

																//GUI.DrawOverlapBB(ref region, bb, Physics.Layer.Solid | Physics.Layer.Building);
															}

															switch (placement.type)
															{
																case Placement.Type.Line:
																{
																	if (prefab.Root.TryGetComponentData<Resizable.Data>(out var resizable, initialized: true) && prefab.Root.TryGetComponentData<Animated.Renderer.Data>(out var renderer, initialized: true))
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
																	if (prefab.Root.TryGetComponentData<Animated.Renderer.Data>(out var renderer, initialized: true))
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

																			GUI.DrawRenderer(in transform, in renderer, Color32BGRA.White.WithAlphaMult(0.60f));

																			if (construction_prefab != null)
																			{
																				if (construction_prefab.Root.TryGetComponentData<Animated.Renderer.Data>(out var renderer_construction, initialized: true))
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
																			//var rect_offset = new AABB(transform.LocalToWorld(rect_foundation.a - placement.offset), transform.LocalToWorld(rect_foundation.b - placement.offset));
																			//var rect_offset = new AABB(transform.LocalToWorld(rect_foundation.a, rotation: false), transform.LocalToWorld(rect_foundation.b, rotation: false));
																			//var rect_offset = transform.LocalToWorld(rect_foundation); // new AABB(transform.LocalToWorld(rect_foundation.a - placement.offset), transform.LocalToWorld(rect_foundation.b - placement.offset));
																			//var rect_offset = transform.LocalToWorld(rect_foundation); // new AABB(transform.LocalToWorld(rect_foundation.a - placement.offset), transform.LocalToWorld(rect_foundation.b - placement.offset));
																			//GUI.DrawTerrainOutline(ref region, rect_offset.GetPosition(), 12.00f);

																			var tile_flags = placement.tileflags_foundation;

																			ref var terrain = ref region.GetTerrain();

																			var quad_world = transform.LocalToWorld(rect_foundation - placement.offset).Snap(0.125f); // + transform.position;
																			var quad_world_aabb = quad_world.GetAABB(); //.Snap(0.125f);
																														//quad_world_aabb.a = quad_world_aabb.a.SnapFloor(0.125f);
																														//quad_world_aabb.b = quad_world_aabb.b.SnapCeil(0.125f);

																			var quad_canvas = region.WorldToCanvas(quad_world);
																			var quad_canvas_aabb = region.WorldToCanvas(quad_world_aabb);


																			var pixel_size = new Vector2(App.pixels_per_unit_inv) * region.GetWorldToCanvasScale();

																			var args = new DrawFoundationArgs(offset: quad_canvas_aabb.a,
																				pixel_size: pixel_size,
																				tile_flags: tile_flags,
																				max_health: 1000,
																				color: color_ok_fg);

																			//terrain.IterateRect(quad_world_aabb, argument: ref args,
																			//	func: DrawFoundationFunc,
																			//	iteration_flags: Terrain.IterationFlags.Iterate_Empty);

																			terrain.IterateRectRotated(quad_world, argument: ref args,
																				func: DrawFoundationFunc,
																				iteration_flags: Terrain.IterationFlags.Iterate_Empty);

																			//var args = new DrawTileArgs(offset: quad_canvas_aabb.a, pixel_size: pixel_size, color: color_dummy_fg, tile_flags: tile_flags, block: default, max_health: 1000);
																			//terrain.IterateRectRotated(quad_world, argument: ref args,
																			//	func: DrawTileFunc,
																			//	iteration_flags: Terrain.IterationFlags.Iterate_Empty);


																			//GUI.DrawQuadFilled(region.WorldToCanvas(quad_world), color_dummy_fg);
																			//GUI.DrawRect(quad_canvas_aabb, GUI.font_color_yellow);
																		}

																		//GUI.DrawRect(region.WorldToCanvas(bb), color_dummy_fg, layer: GUI.Layer.Background);
																		//GUI.DrawQuad(region.WorldToCanvas(bb.Rotate(transform.rotation)), color_dummy_fg);
																		GUI.DrawQuad(region.WorldToCanvas(bb.RotateByRad(transform.rotation)), color_dummy_fg);
																		//GUI.DrawRect(region.WorldToCanvas(bb.RotateByRad(transform.rotation)).GetAABB(), color: GUI.font_color_yellow);


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

													GUI.NewLine();

													using (GUI.Wrap.Push())
													{
														if (placement.min_support > Maths.epsilon)
														{
															GUI.TextShaded($"Support: {(support * 100.00f):0}%/{(placement.min_support * 100.00f):0}%", color: support >= placement.min_support ? color_ok : color_error);
														}

														if (errors != Build.Errors.None)
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
															if (errors.HasNone(Build.Errors.ZeroCount))
															{
																var rpc = new Build.PlaceRPC
																{
																	pos_origin = pos_origin,
																	pos_raw = pos_raw,
																	pos_a_raw = pos_a_raw,
																	pos_b_raw = pos_b_raw,
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
																	pos_a_raw = (pos_b - ((pos_b - pos_a).GetNormalized() * placement.size * 0.50f)).SnapFloor(0.125f);
																	pos_b_raw = null;
																}
															}

															next_place_local = time + placement.cooldown;
														}

														if (reset)
														{
															pos_a_raw = null;
															pos_b_raw = null;
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
					Construction,
					Industry,
					Buildings,
					Mechanisms,
					Misc,
					//Factions,
					//Logistics
				}

				public partial struct ConfigureRPC: Net.IRPC<Build.Data>
				{
					public IRecipe.Handle recipe;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Build.Data build)
					{
						build.recipe = this.recipe;
						build.Sync(entity);
					}
#endif
				}

				public static Build.Errors Evaluate(Entity entity, in Placement placement, ref bool skip_support, out float support, AABB bb, in Transform.Data transform, Vector2 pos, Vector2? pos_a = default, Vector2? pos_b = default, IFaction.Handle faction_id = default, float placement_range = 4.00f)
				{
					ref var region = ref entity.GetRegion();

					var errors = Build.Errors.None;

					if (placement.min_claim > Maths.epsilon)
					{
						var claim_ratio = Claim.GetOverlapRatio(region: ref region,
							bb: bb,
							faction_id: faction_id,
							allow_unclaimed: placement.flags.HasNone(Placement.Flags.Require_Claimed));
						if (claim_ratio < placement.min_claim) errors |= Errors.Claimed;
					}

					support = 1.00f;
					var clearance = 1.00f;

					var check_support = placement.min_support > Maths.epsilon && !skip_support;
					var check_clearance = placement.min_clearance > Maths.epsilon;

					if (check_support || check_clearance)
					{
						Build.CalculateSupport(region: ref region,
							placement: in placement,
							transform: in transform,
							support_count: out var support_count,
							blocked_count: out var blocked_count,
							total_count: out var total_count,
							pos: pos,
							pos_a: pos_a,
							pos_b: pos_b);

						//Build.CalculateSupport(ref region, in placement, out var support_count, out var blocked_count, out var total_count, pos, pos_a, pos_b);

						if (check_support)
						{
							support = Maths.NormalizeClamp(placement.flags.HasAny(Placement.Flags.No_Solid_Support) ? support_count - blocked_count : support_count, total_count);
							if (support < placement.min_support)
							{
								errors |= Errors.Unsupported;
							}
						}

						if (check_clearance)
						{
							clearance = 1.00f - Maths.NormalizeClamp(blocked_count, total_count);
							if (clearance < placement.min_clearance)
							{
								errors |= Errors.Obstructed;
							}
						}
					}

					if (placement.type == Placement.Type.Line)
					{
						var a = pos_a ?? pos;
						var b = pos_b ?? pos;

						var len = Vector2.Distance(a, b);

						if (placement.length_min > Maths.epsilon && len < placement.length_min)
						{
							errors |= Build.Errors.MinLength;
						}

						if (placement.length_max > Maths.epsilon && len > placement.length_max + placement.length_step)
						{
							errors |= Build.Errors.MaxLength;
						}
					}

					ref var transform_entity = ref entity.GetComponent<Transform.Data>();
					if (transform_entity.IsNotNull())
					{
						var placement_range_sq = placement_range * placement_range;
						var dist_sq = float.PositiveInfinity;

						switch (placement.type)
						{
							case Placement.Type.Simple:
							{
								dist_sq = Vector2.DistanceSquared(transform_entity.position, bb.ClipPoint(transform_entity.position));

								//#if CLIENT
								//						GUI.DrawCircleFilled(GUI.WorldToCanvas(bb.ClipPoint(transform.position)), 4.00f, 0xffffffff);
								//#endif
							}
							break;

							case Placement.Type.Rectangle:
							{
								dist_sq = Vector2.DistanceSquared(transform_entity.position, bb.ClipPoint(transform_entity.position));

								//#if CLIENT
								//						GUI.DrawCircleFilled(GUI.WorldToCanvas(bb.ClipPoint(transform.position)), 4.00f, 0xffffffff);
								//#endif
							}
							break;

							case Placement.Type.Circle:
							{
								dist_sq = Vector2.DistanceSquared(transform_entity.position, pos) - (placement.radius * placement.radius);
							}
							break;

							case Placement.Type.Line:
							{
								var line = new Line(pos_a ?? pos, pos_b ?? pos);
								dist_sq = Vector2.DistanceSquared(line.GetClosestPoint(transform_entity.position), transform_entity.position);

								//#if CLIENT
								//						GUI.DrawCircleFilled(GUI.WorldToCanvas(line.GetClosestPoint(transform.position)), 4.00f, 0xffffffff);
								//#endif
							}
							break;
						}

						if (dist_sq > placement_range_sq)
						{
							errors |= Build.Errors.OutOfRange;
						}

						//var placement_range_sq = placement_range * placement_range;
						//if (Vector2.DistanceSquared(pos, transform.position) >= placement_range_sq) errors |= Errors.OutOfRange;
					}

					return errors;
				}

				public static Build.Errors EvaluateBlock(ref Region.Data region, in Placement placement, ref bool skip_support, out int placed_block_count, AABB bb, IBlock.Handle block, TileFlags tile_flags, Vector2 pos, Vector2? pos_a = default, Vector2? pos_b = default)
				{
					var errors = Build.Errors.None;

					placed_block_count = Build.CalculateBlockCount(ref region, in placement, block, tile_flags, pos, pos_a, pos_b);
					if (placed_block_count <= 0) errors |= Build.Errors.ZeroCount;

					if (tile_flags.HasAny(TileFlags.Solid) && placement.flags.HasNone(Placement.Flags.Ignore_Obstructed))
					{
						if (placement.type == Placement.Type.Line)
						{
							var radius = placement.size.GetMax() * 0.125f;
							var dir = ((pos_a ?? pos) - (pos_b ?? pos)).GetNormalized(out var len);

							Span<LinecastResult> hits = FixedArray.CreateSpan16<LinecastResult>(out var buffer);
							if (region.TryLinecastAll((pos_a ?? pos) - (dir * (radius + 0.25f)), (pos_b ?? pos) + (dir * (radius + 0.25f)), radius, ref hits, mask: Physics.Layer.Solid | Physics.Layer.Building))
							{
								errors |= Errors.Obstructed;
							}
						}
						else
						{
							//Span<OverlapBBResult> hits = stackalloc OverlapBBResult[16];
							//if (region.TryOverlapBBAll(pos, bb.GetSize() - new Vector2(0.25f), ref hits, mask: Physics.Layer.Solid | Physics.Layer.Building)) errors |= Errors.Obstructed;

							Span<ShapeOverlapResult> hits = FixedArray.CreateSpan32<ShapeOverlapResult>(out var buffer);
							if (region.TryOverlapRectAll(bb, ref hits, mask: Physics.Layer.Solid | Physics.Layer.Building))
							{
								errors |= Errors.Obstructed;
							}
						}
					}

					return errors;
				}

				public static Build.Errors EvaluatePrefab(ref Region.Data region, in Placement placement, ref bool skip_support, AABB bb, Vector2 pos, Vector2? pos_a = default, Vector2? pos_b = default)
				{
					var errors = Build.Errors.None;

					//bb = new AABB(bb.a + new Vector2(0.50f), bb.b - new Vector2(0.50f));

					var bb_size = bb.GetSize();
					var bb_size_inner = bb_size;

					bb_size_inner.X -= Maths.Clamp(bb_size.X - 0.50f, 0.00f, 0.50f);
					bb_size_inner.Y -= Maths.Clamp(bb_size.Y - 0.50f, 0.00f, 0.50f);

					bb = bb.GetPaddedRect(bb_size_inner);

					var mask = Physics.Layer.Building | Physics.Layer.Support | Physics.Layer.No_Overlapped_Placement | Physics.Layer.World;

					if (placement.flags.HasAny(Placement.Flags.Require_Terrain))
					{
						errors.AddFlag(Build.Errors.NoTerrain);
					}

					if (placement.type == Placement.Type.Line)
					{
						Span<LinecastResult> results = FixedArray.CreateSpan32<LinecastResult>(out var buffer);
						if (region.TryLinecastAll(pos_a.Value, pos_b.Value, placement.size.GetMax() * 0.50f, ref results, mask: mask, query_flags: Physics.QueryFlag.Static))
						{
							foreach (ref var result in results)
							{
								//App.WriteLine(result.alpha);
								//if (placement.flags.HasAny(Placement.Flags.Require_Terrain | Placement.Flags.Terrain_Is_Support) && result.layer.HasAny(Physics.Layer.World) && (result.alpha <= 0.10f || result.alpha >= 0.90f))
								if (result.layer.HasAny(Physics.Layer.World))
								{
									if (placement.flags.HasAny(Placement.Flags.Require_Terrain | Placement.Flags.Terrain_Is_Support) && (result.alpha <= 0.10f || result.alpha >= 0.90f))
									{
										errors.RemoveFlag(Build.Errors.NoTerrain);
										if (placement.flags.HasAny(Placement.Flags.Terrain_Is_Support)) skip_support = true;
									}
								}
								else if (placement.flags.HasAny(Placement.Flags.Allow_Placement_Over_Buildings))
								{
									if (result.layer.HasAny(Physics.Layer.No_Overlapped_Placement))
									{
										skip_support = false;
										if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;

										break;
									}
									else if (result.layer.HasAny(Physics.Layer.Support))
									{
										skip_support = true;
									}
									else if (result.layer.HasAny(Physics.Layer.Building))
									{

									}
									else
									{
										if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;
									}
								}
								else
								{
									if (result.layer.HasAny(Physics.Layer.No_Overlapped_Placement))
									{

									}
									else
									{
										if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;
									}
								}
							}
						}
					}
					else
					{
						//Span<OverlapBBResult> results = stackalloc OverlapBBResult[32];
						//if (region.TryOverlapBBAll(bb, ref results, mask: mask, query_flags: Physics.QueryFlag.Static))

						//var ts = Timestamp.Now();
						Span<ShapeOverlapResult> results = FixedArray.CreateSpan32<ShapeOverlapResult>(out var buffer);
						if (region.TryOverlapRectAll(bb, ref results, mask: mask, query_flags: Physics.QueryFlag.Static))
						{
							//App.WriteLine(results.Length);
							foreach (ref var result in results)
							{
								if (result.layer.HasAny(Physics.Layer.World))
								{
									if (placement.flags.HasAny(Placement.Flags.Require_Terrain | Placement.Flags.Terrain_Is_Support))
									{
										errors.SetFlag(Build.Errors.NoTerrain, false);
										if (placement.flags.HasAny(Placement.Flags.Terrain_Is_Support)) skip_support = true;
									}
								}
								else if (placement.flags.HasAny(Placement.Flags.Allow_Placement_Over_Buildings))
								{
									if (result.layer.HasAny(Physics.Layer.No_Overlapped_Placement))
									{
										skip_support = false;
										if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;

										break;
									}
									else if (result.layer.HasAny(Physics.Layer.Support))
									{
										skip_support = true;
									}
									else if (result.layer.HasAny(Physics.Layer.Building))
									{

									}
									else
									{
										if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;
									}
								}
								else
								{
									if (result.layer.HasAny(Physics.Layer.No_Overlapped_Placement))
									{

									}
									else
									{
										if (placement.flags.HasNone(Placement.Flags.Ignore_Obstructed)) errors |= Errors.Obstructed;
									}
								}
							}
						}
						//App.WriteLine($"{ts.GetMilliseconds():0.000} ms");
					}

					return errors;
				}

				public static void GetPlacementInfo(ref Placement placement, Build.Flags flags, Vector2 pos_raw, Vector2? pos_a_raw, Vector2? pos_b_raw, Vector2 scale, out Vector2 pos, out Vector2 pos_a, out Vector2 pos_b, out Vector2 pos_final, out float rot_final, out AABB bb)
				{
					var snap = new Vector2(0.125f);
					if (placement.flags.HasNone(Placement.Flags.No_Snapping) && flags.HasAny(Build.Flags.Snap))
					{
						snap = placement.snap ?? placement.size;
					}

					//var pos_a_raw = build.pos_a ?? pos_raw;
					//var pos_b_raw = build.pos_b ?? pos_raw;

					pos = Build.ConstrainPosition(in placement, pos_raw, pos_a_raw, pos_b_raw, snap: snap);
					//pos_a = Build.ConstrainPosition(in placement, pos_a_raw ?? pos + new Vector2(0, placement.length_step), pos_a_raw, pos_b_raw, snap: snap);
					pos_a = Build.ConstrainPosition(in placement, pos_a_raw ?? pos, pos_a_raw, pos_b_raw, snap: snap);
					pos_b = Build.ConstrainPosition(in placement, pos_b_raw ?? pos, pos_a_raw, pos_b_raw, snap: snap);
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
							pos_final = pos_a;
						}
						break;

						case Placement.Type.Simple:
						{
							if (placement.rotation_max > Maths.epsilon)
							{
								var dir = (pos_raw - (pos_a_raw ?? pos_raw)).GetNormalized(out var len);
								if (placement.rotation_offset != 0.00f) dir = dir.RotateByRad(placement.rotation_offset);

								dir = Vector2.Lerp(new Vector2(0, -1), dir, Maths.Clamp((len - 0.25f) * 2.00f, 0.00f, 1.00f));

								var rot_max = Maths.Snap(MathF.Abs(placement.rotation_max), placement.rotation_step);
								var rot = Maths.Clamp(Maths.Snap(dir.GetAngleRadians() + (MathF.PI * 0.50f), placement.rotation_step), -rot_max, +rot_max);

								pos_final = pos_a + (offset.RotateByRad(rot)); // - offset;
								rot_final = rot;
							}
						}
						break;
					}

					//var mat = Maths.TRS3x2(pos_final, 0.00f, scale);
					//pos_final = Vector2.Transform(placement.offset, mat);

					bb = placement.type switch
					{
						Placement.Type.Rectangle => AABB.Centered(pos, placement.size),
						Placement.Type.Circle => AABB.Circle(pos, placement.radius),
						Placement.Type.Line => AABB.Line(pos_a, pos_b, placement.size.X),
						Placement.Type.Simple => AABB.Centered(pos_final, placement.size),
						_ => AABB.Centered(pos, placement.size)
					};
				}

				public partial struct PlaceRPC: Net.IRPC<Build.Data>
				{
					public Build.Flags flags;
					public Vector2 pos_origin;
					public Vector2 pos_raw;
					public Vector2? pos_a_raw;
					public Vector2? pos_b_raw;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Build.Data build)
					{
						ref var region = ref entity.GetRegion();

						var errors = Build.Errors.None;

						var success = false;
						var skip_support = false;
						var support = 0.00f;
						var amount_multiplier = 1.00f;

						var placement_range = 4.00f;
						var placement_range_sq = placement_range * placement_range;

						//ref var player = ref connection.GetPlayerData();
						ref var transform_entity = ref entity.GetComponent<Transform.Data>();
						ref var recipe = ref build.recipe.GetData();
						ref var character_data = ref connection.GetCharacter(out var h_character);

						if (recipe.IsNotNull() && transform_entity.IsNotNull() && character_data.IsNotNull())
						{
							//Crafting.Context.NewFromPlayer(ref region, ref player, entity, out var context);
							Crafting.Context.NewFromCharacter(ref region.AsCommon(), h_character, entity, out var context);
							//var ent_character = connection.enti

							if (recipe.type == Crafting.Recipe.Type.Build)
							{
								ref var product = ref recipe.products[0];
								if (recipe.placement.TryGetValue(out var placement))
								{
									var scale = new Vector2(1, 1);
									if (placement.flags.HasAny(Placement.Flags.Allow_Mirror_X)) scale.X.ToggleSign(this.pos_raw.X < this.pos_origin.X);

									var random = XorRandom.New(true);

									Build.GetPlacementInfo(placement: ref placement,
										flags: this.flags,
										pos_raw: this.pos_raw,
										pos_a_raw: this.pos_a_raw,
										pos_b_raw: this.pos_b_raw,
										scale: scale,
										pos: out var pos,
										pos_a: out var pos_a,
										pos_b: out var pos_b,
										pos_final: out var pos_final,
										rot_final: out var rot_final,
										bb: out var bb);

									var transform = new Transform.Data(pos_final, rot_final, scale);
									var ent_parent = entity.GetParent(Relation.Type.Child);
									//var inventory = player.GetInventory();
									var h_faction = character_data.faction;

									if (product.type == Crafting.Product.Type.Block)
									{
										ref var block = ref product.block.GetData();
										if (block.IsNotNull())
										{
											var tile_flags = block.tile_flags | product.tile_flags;

											ref var terrain = ref region.GetTerrain();

											errors |= Build.EvaluateBlock(region: ref region, placement: in placement, skip_support: ref skip_support, placed_block_count: out var placed_block_count, bb: bb, block: product.block, tile_flags: tile_flags, pos: pos, pos_a: pos_a, pos_b: pos_b);
											amount_multiplier = placed_block_count;
											errors |= Build.Evaluate(entity: entity, placement: in placement, skip_support: ref skip_support, support: out support, bb: bb, transform: in transform, pos: pos, pos_a: pos_a, pos_b: pos_b, faction_id: h_faction);

											//if (!Crafting.Evaluate(entity, ent_parent, transform.position, ref recipe.requirements, inventory: inventory, amount_multiplier: amount_multiplier, h_faction: h_faction)) errors |= Errors.RequirementsNotMet;
											if (!context.Evaluate(requirements: recipe.requirements.AsSpan(), amount_multiplier: amount_multiplier)) errors |= Errors.RequirementsNotMet;

											if (errors == Build.Errors.None)
											{
												//Crafting.Consume(ent_parent, transform.position, ref recipe.requirements, inventory: inventory, amount_multiplier: amount_multiplier, sync: true);
												context.Consume(requirements: recipe.requirements.AsSpan(), amount_multiplier: amount_multiplier);

												var args = new SetTileFuncArgs(block: product.block, tile_flags: tile_flags, count: 0, max_health: block.max_health, mappings_replace: placement.mappings_replace);
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
										var prefab_handle = default(Prefab.Handle);
										if (product.type == Crafting.Product.Type.Prefab) prefab_handle = product.prefab;
										else if (product.type == Crafting.Product.Type.Resource)
										{
											ref var material = ref product.material.GetData();
											if (material.IsNotNull())
											{
												prefab_handle = material.prefab;
											}
										}

										if (prefab_handle.TryGetPrefab(out var prefab))
										{
											//var prefab_handle = product.prefab;

											//var scale = new Vector2(1, 1);

											//if (placement.flags.HasAny(Placement.Flags.Allow_Mirror_X) && this.pos_raw.X < transform.position.X)
											//{
											//	scale.X *= -1.00f;
											//}

											errors |= Build.EvaluatePrefab(region: ref region, placement: in placement, skip_support: ref skip_support, bb: bb, pos: pos_final, pos_a: pos_a, pos_b: pos_b);
											amount_multiplier = 1.00f;
											errors |= Build.Evaluate(entity: entity, placement: in placement, skip_support: ref skip_support, support: out support, bb: bb, transform: in transform, pos: pos_final, pos_a: pos_a, pos_b: pos_b, faction_id: h_faction);

											if (placement.type == Placement.Type.Line)
											{
												amount_multiplier += Vector2.Distance(pos_a, pos_b);
											}

											if (recipe.construction.HasValue)
											{
												var construction = recipe.construction.Value;

												//if (!Crafting.Evaluate(entity, ent_parent, transform.position, ref construction.requirements, inventory: inventory, amount_multiplier: amount_multiplier, h_faction: h_faction)) errors |= Errors.RequirementsNotMet;
												if (!context.Evaluate(requirements: construction.requirements.AsSpan(), amount_multiplier: amount_multiplier)) errors |= Errors.RequirementsNotMet;

												if (errors == Build.Errors.None)
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

													var order = new Workshop.Order();
													order.amount_multiplier = 1.00f;
													order.flags |= Workshop.Order.Flags.InProgress;
													order.recipe = build.recipe;

													var shipment = new Shipment.Data();
													shipment.flags |= Shipment.Flags.Keep_Items | Shipment.Flags.No_GUI;

													var work_count = 0;
													var item_count = 0;

													var quantity = product.amount;

													var requirements = recipe.requirements;
													for (var i = 0; i < requirements.Length; i++)
													{
														ref var requirement = ref requirements[i];
														switch (requirement.type)
														{
															case Crafting.Requirement.Type.Work:
															{
																order.work[work_count++] = new(requirement.work, Work.Amount.Flags.Pending, requirement.difficulty, requirement.amount * Constants.Construction.work_requirement_multiplier, 0.00f);
															}
															break;

															case Crafting.Requirement.Type.Resource:
															{
																shipment.items[item_count++] = Shipment.Item.Resource(requirement.material, 0.00f, requirement.amount * Constants.Construction.resource_requirement_multiplier);
															}
															break;
														}
													}

													region.SpawnPrefab(prefab: construction.prefab, position: pos_final, rotation: rot_final, scale: scale, faction_id: h_faction).ContinueWith((ent) =>
													{
														ref var dismantlable = ref ent.GetOrAddComponent<Dismantlable.Data>(sync: true, ignore_mask: true);
														if (!dismantlable.IsNull())
														{
															dismantlable = dismantlable_tmp;
														}

														ref var construction = ref ent.GetComponent<Construction.Data>();
														if (!construction.IsNull())
														{
															construction.prefab = prefab_handle;
															construction.order = order;
															construction.stage = Construction.Stage.Materials;
															construction.quantity = quantity;
														}

														ref var shipment_new = ref ent.GetOrAddComponent<Shipment.Data>(sync: true, ignore_mask: true);
														if (!shipment_new.IsNull())
														{
															shipment_new = shipment;
														}
													});

													success = true;
												}
											}
											else
											{
												//if (!Crafting.Evaluate(entity, ent_parent, transform.position, ref recipe.requirements, inventory: inventory, amount_multiplier: amount_multiplier, h_faction: h_faction)) errors |= Errors.RequirementsNotMet;
												if (!context.Evaluate(requirements: recipe.requirements.AsSpan(), amount_multiplier: amount_multiplier)) errors |= Errors.RequirementsNotMet;

												if (errors == Build.Errors.None)
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

													region.SpawnPrefab(prefab: prefab_handle, position: pos_final, rotation: rot_final, scale: scale, faction_id: h_faction).ContinueWith((ent) =>
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
										Notification.Push(ref connection, $"Cannot place: {errors.ToFormattedString()}", Color32BGRA.Red, sound: "error", volume: 0.60f);
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
				private record struct SetTileFuncArgs(IBlock.Handle block, TileFlags tile_flags, int count, float max_health, Dictionary<IBlock.Handle, Block.Mapping> mappings_replace = null);
				static void SetTileFunc(ref Tile tile, int x, int y, byte mask, ref SetTileFuncArgs args)
				{
					if ((tile.BlockID == 0 || args.tile_flags.HasAny(TileFlags.Solid)) && tile.Flags.HasNone(TileFlags.Solid))
					{
						tile.Reset();

						tile.Health = 255;
						tile.BlockID = (byte)args.block.id;
						tile.Flags |= args.tile_flags;

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

					//if ((tile.BlockID != 0 || tile.Flags.HasAny(TileFlags.Ground))) // && !tile.Flags.HasAll(TileFlags.Solid))
					//{
					//	args.support_count++;
					//}

					//if (tile.Flags.HasAny(TileFlags.Solid))
					//{
					//	args.blocked_count++;
					//}

					//args.total_count++;
				}

				public static void CalculateSupport(ref Region.Data region, in Placement placement, in Transform.Data transform, out int support_count, out int blocked_count, out int total_count, Vector2 pos, Vector2? pos_a = default, Vector2? pos_b = default)
				{
					ref var terrain = ref region.GetTerrain();

					//var args = new CalculateSupportArgs(valid_count: 0, total_count: 0);
					//terrain.IterateRect(pos, (placement.size * App.pixels_per_unit) + new Vector2(2, 2), ref args, CalculateSupportFunc);

					if (placement.rect_foundation.TryGetValue(out var rect_foundation))
					{
						var tile_flags = placement.tileflags_foundation;

						var quad_world = transform.LocalToWorld(rect_foundation - placement.offset).Snap(0.125f);
						var quad_world_aabb = quad_world.GetAABB();

						var args = new CalculateFoundationSupportArgs(support_count: 0, blocked_count: 0, total_count: 0, tile_flags: placement.tileflags_foundation, max_health: 10000.00f);

						//terrain.IterateRect(quad_world_aabb, argument: ref args,
						//	func: DrawFoundationFunc,
						//	iteration_flags: Terrain.IterationFlags.Iterate_Empty);

						terrain.IterateRectRotated(quad_world, argument: ref args,
							func: CalculateFoundationSupportFunc,
							iteration_flags: Terrain.IterationFlags.Iterate_Empty);

						support_count = args.support_count;
						blocked_count = args.blocked_count;
						total_count = args.total_count;
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
						total_count = args.total_count;
					}

					//support_ratio = Maths.NormalizeClamp(args.support_count, args.total_count);
					//blocked_ratio = Maths.NormalizeClamp(args.blocked_count, args.total_count);
				}
				#endregion

				public static Vector2 ConstrainPosition(in Placement placement, Vector2 pos, Vector2? pos_a = null, Vector2? pos_b = null, Vector2? snap = null)
				{
					if (pos_a.HasValue)
					{
						var pivot = Build.GetSnappedPosition(pos_a.Value, null, new Vector2(0.125f));
						pos = Build.GetSnappedPosition(pos, pivot, snap ?? new Vector2(0.125f));
					}
					else
					{
						pos = Build.GetSnappedPosition(pos, null, new Vector2(0.125f));
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
							var pivot = Build.GetSnappedPosition(pos_a.Value, null, new Vector2(0.125f));
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
