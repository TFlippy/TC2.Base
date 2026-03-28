using TC2.Base.Components;
using System.Runtime.InteropServices;

namespace TC2.Conquest
{
	public static partial class NEI
	{
		public struct Entry
		{
			public Shipment.Item2.Header h_item;
			//public 
		}

#if CLIENT

		public partial struct TestGUI: IGUICommand
		{
			public static Shipment.Item2.Header h_item_selected;

			public void Draw()
			{
				var rect_canvas = GUI.GetCanvasRect();


				//using (var window = GUI.Window.Standalone(identifier: "hud.nei"u8,
				//pivot: new(0.50f, 0.50f),
				//size: new(48.00f * 7, 48.00f * 9),
				//flags: GUI.Window.Flags.None))
				//{
				//	if (window.show)
				//	{
				using (var widget = Sidebar.Widget.New(identifier: "nei", name: "ISO Handbook",
				icon: new Sprite(GUI.tex_icons_widget, 16, 16, 1, 2),
				size: new(48.00f * 7, 48.00f * 9),
				size_min: new(48.00f * 7, 48.00f * 6),
				size_max: new(48.00f * 7, 48.00f * 12),
				order: 150,
				flags: Sidebar.Widget.Flags.Align_Right | Sidebar.Widget.Flags.Enabled | Sidebar.Widget.Flags.Has_Window | Sidebar.Widget.Flags.Play_Sound | Sidebar.Widget.Flags.Resizable))
				{
					if (widget.state_flags.HasAny(Sidebar.Widget.StateFlags.Show))
					{
						GUI.DrawWindowBackground();

						using (var group_main = GUI.Group.New(GUI.Av, padding: new(6)))
						{
							var hovered_item_type = GUI.GetHoveredItemType();
							if (hovered_item_type.type == Shipment.Item.Type.Resource)
							{
								var h_material_hovered = (IMaterial.Handle)hovered_item_type.id;
								if (GUI.IsMouseClicked(button: GUI.MouseButton.Right))
								{
									h_item_selected.Toggle(h_material_hovered);
									Sound.PlayGUI(GUI.sound_select, volume: 0.07f);
									//Sound.PlayGUI(GUI.sound_select, volume: 0.18f, pitch: 0.85f);
								}
							}

							var h_material_selected = (IMaterial.Handle)h_item_selected.id;
							ref var material_data = ref h_material_selected.GetData();
							if (material_data.IsNotNull())
							{
								using (var group_title = GUI.Group.New(size: new(GUI.RmX, 40)))
								{
									using (var group_icon = GUI.Group.New(size: new(GUI.RmY)))
									{
										GUI.Draw9Slice(GUI.tex_slot, padding: new(4), rect: group_icon.GetOuterRect());
										GUI.DrawMaterialSmall(h_material_selected, size: GUI.Rm);
									}
									GUI.FocusableAsset(h_material_selected, rect: GUI.GetLastItemRect());

									GUI.SameLine();

									GUI.TitleCentered(material_data.GetName(), size: 24, pivot: new(0, 0.00f), offset: new(6, 0));
								}

								GUI.SeparatorThick(l: 4, r: 0);

								using (var scroll_recipes = GUI.Scrollbox.New("scroll.recipes"u8, size: material_data.related.IsNotNullOrEmpty() ? GUI.Rm.MulY(0.50f) : GUI.Rm))
								{
									var recipes = IRecipe.Database.GetAssetsSpan();
									foreach (var recipe_asset in recipes)
									{
										ref var recipe_data = ref recipe_asset.GetData();
										if (recipe_data.IsNotNull())
										{
											if (recipe_data.flags.HasAny(Recipe.Flags.Hidden | Recipe.Flags.Custom)) continue;
											if ((!App.IsModEditing || App.hide_wip_recipes) && recipe_data.flags.HasAny(Recipe.Flags.Debug | Recipe.Flags.Disabled)) continue;

											var related = recipe_data.related_materials;
											if (related.IsNotNullOrEmpty() && related.TryGetValue(h_material_selected, out var tags_related))
											{
												using (var id = GUI.ID<NEI.Entry, Recipe.Type>.Push(recipe_asset.GetHandle()))
												using (var group_row = GUI.Group.New(size: new(GUI.RmX, 32)))
												{
													if (group_row.IsVisible())
													{
														using (var group_icon = GUI.Group.New(size: new(GUI.RmY)))
														{
															GUI.DrawRecipeIcon(recipe_asset, rect: group_icon.GetOuterRect());
														}
														//GUI.DrawMaterialSmall(h_material_related, size: new(GUI.RmY));
														GUI.Draw9Slice(GUI.tex_frame, padding: new(4), rect: GUI.GetLastItemRect());

														GUI.SameLine();

														using (var group_info = GUI.Group.New(size: new(GUI.RmX, 32)))
														{
															var color = Color32BGRA.White;
															if (tags_related.HasAny(IMaterial.RelatedTags.Product | IMaterial.RelatedTags.Output | IMaterial.RelatedTags.Result)) color = GUI.col_new;
															if (tags_related.HasAny(IMaterial.RelatedTags.Ingredient | IMaterial.RelatedTags.Input | IMaterial.RelatedTags.Source)) color = GUI.col_remove;
															group_info.DrawBackground(GUI.tex_slot_white, color: color.WithAlpha(192).WithColorMult(0.50f));

															GUI.TitleCentered(recipe_data.GetShortName(), size: 16, pivot: new(0, 0.50f), offset: new(8, 0));
															GUI.TextShadedCentered(tags_related.WithMasked(
																IMaterial.RelatedTags.Scrap | IMaterial.RelatedTags.Component | IMaterial.RelatedTags.Manufactured | IMaterial.RelatedTags.Ingredient |
																IMaterial.RelatedTags.Product | IMaterial.RelatedTags.Waste | IMaterial.RelatedTags.Byproduct | IMaterial.RelatedTags.Residue | IMaterial.RelatedTags.Ore |
																IMaterial.RelatedTags.Casing | IMaterial.RelatedTags.Impurity | IMaterial.RelatedTags.Extracted | IMaterial.RelatedTags.Propellant | IMaterial.RelatedTags.Vapor |
																IMaterial.RelatedTags.Catalyst | IMaterial.RelatedTags.Reaction | IMaterial.RelatedTags.Melted | IMaterial.RelatedTags.Mixed | IMaterial.RelatedTags.Refined).GetLowestSetBit().ToStringUtf8(),
																size: 14, pivot: new(1, 0.50f), offset: new(-8, 0));
														}

														GUI.FocusableAsset(recipe_asset, rect: group_row.GetOuterRect());

														if (GUI.Selectable3(id: id.hash, rect: group_row.GetOuterRect(), selected: false))
														{

														}
													}
												}
											}
										}
									}
								}

								if (material_data.related.IsNotNullOrEmpty())
								{
									GUI.SeparatorThick(l: 4, r: 0, spacing: 16);

									using (var scroll_related = GUI.Scrollbox.New("scroll.related"u8, size: GUI.Rm))
									{
										var related = material_data.related;
										if (related.IsNotNullOrEmpty())
										{
											foreach (var (h_material_related, tags_related) in related)
											{
												if (h_material_related == h_material_selected) continue;

												ref var material_related_data = ref h_material_related.GetData();
												if (material_related_data.IsNotNull())
												{
													using (var id = GUI.ID<NEI.Entry, Material.Type>.Push(h_material_related))
													using (var group_row = GUI.Group.New(size: new(GUI.RmX, 32)))
													{
														if (group_row.IsVisible())
														{
															GUI.DrawMaterialSmall(h_material_related, size: new(GUI.RmY));
															GUI.Draw9Slice(GUI.tex_frame, padding: new(4), rect: GUI.GetLastItemRect());

															GUI.SameLine();

															using (var group_info = GUI.Group.New(size: new(GUI.RmX, 32)))
															{
																var color = Color32BGRA.White;
																if (tags_related.HasAny(IMaterial.RelatedTags.Product | IMaterial.RelatedTags.Output | IMaterial.RelatedTags.Result)) color = GUI.col_new;
																if (tags_related.HasAny(IMaterial.RelatedTags.Ingredient | IMaterial.RelatedTags.Input | IMaterial.RelatedTags.Source)) color = GUI.col_remove;
																group_info.DrawBackground(GUI.tex_slot_white, color: color.WithAlpha(192).WithColorMult(0.50f));

																GUI.TitleCentered(material_related_data.GetName(), size: 16, pivot: new(0, 0.50f), offset: new(8, 0));
																GUI.TextShadedCentered(tags_related.WithMasked(
																	IMaterial.RelatedTags.Scrap | IMaterial.RelatedTags.Component | IMaterial.RelatedTags.Manufactured | IMaterial.RelatedTags.Ingredient |
																	IMaterial.RelatedTags.Product | IMaterial.RelatedTags.Waste | IMaterial.RelatedTags.Byproduct | IMaterial.RelatedTags.Residue | IMaterial.RelatedTags.Catalyst |
																	IMaterial.RelatedTags.Crushed | IMaterial.RelatedTags.Reaction | IMaterial.RelatedTags.Extracted | IMaterial.RelatedTags.Powdered | IMaterial.RelatedTags.Filler |
																	IMaterial.RelatedTags.Ore | IMaterial.RelatedTags.Raw | IMaterial.RelatedTags.Vapor | IMaterial.RelatedTags.Rotten | IMaterial.RelatedTags.Impurity).GetLowestSetBit().ToStringUtf8(),
																		size: 14, pivot: new(1, 0.50f), offset: new(-8, 0));
															}

															GUI.FocusableAsset(h_material_related, rect: group_row.GetOuterRect());

															if (GUI.Selectable3(id: id.hash, rect: group_row.GetOuterRect(), selected: false))
															{
																h_item_selected.Toggle(h_material_related);
															}
														}
													}
												}
											}
										}
									}
								}
							}
							else
							{
								//GUI.SeparatorThick(l: 4, r: 0, spacing: 16);

								using (var scroll_related = GUI.Scrollbox.New("scroll.materials"u8, size: GUI.Rm))
								{
									var materials = IMaterial.Database.GetAssetsSpan();
									foreach (var material_asset in materials)
									{
										ref var material_related_data = ref material_asset.GetData();
										if (material_related_data.IsNotNull())
										{
											if (material_related_data.display_flags.HasAny(Material.DisplayFlags.Unlisted)) continue;

											var h_material = material_asset.GetHandle();

											using (var id = GUI.ID<NEI.Entry, Material.Type>.Push(h_material))
											using (var group_row = GUI.Group.New(size: new(GUI.RmX, 32)))
											{
												if (group_row.IsVisible())
												{
													GUI.DrawMaterialSmall(h_material, size: new(GUI.RmY));
													GUI.Draw9Slice(GUI.tex_frame, padding: new(4), rect: GUI.GetLastItemRect());

													GUI.SameLine();

													using (var group_info = GUI.Group.New(size: new(GUI.RmX, 32)))
													{
														var color = Color32BGRA.White;
														//if (tags_related.HasAny(IMaterial.RelatedTags.Product | IMaterial.RelatedTags.Output | IMaterial.RelatedTags.Result)) color = GUI.col_new;
														//if (tags_related.HasAny(IMaterial.RelatedTags.Ingredient | IMaterial.RelatedTags.Input | IMaterial.RelatedTags.Source)) color = GUI.col_remove;
														group_info.DrawBackground(GUI.tex_slot_white, color: color.WithAlpha(192).WithColorMult(0.50f));

														GUI.TitleCentered(material_related_data.GetName(), size: 16, pivot: new(0, 0.50f), offset: new(8, 0));
														//GUI.TextShadedCentered(tags_related.WithMasked(
														//	IMaterial.RelatedTags.Scrap | IMaterial.RelatedTags.Component | IMaterial.RelatedTags.Manufactured | IMaterial.RelatedTags.Ingredient |
														//	IMaterial.RelatedTags.Product | IMaterial.RelatedTags.Waste | IMaterial.RelatedTags.Byproduct | IMaterial.RelatedTags.Residue | IMaterial.RelatedTags.Catalyst |
														//	IMaterial.RelatedTags.Crushed | IMaterial.RelatedTags.Reaction | IMaterial.RelatedTags.Extracted | IMaterial.RelatedTags.Powdered | IMaterial.RelatedTags.Filler |
														//	IMaterial.RelatedTags.Ore | IMaterial.RelatedTags.Raw | IMaterial.RelatedTags.Vapor | IMaterial.RelatedTags.Rotten | IMaterial.RelatedTags.Impurity).GetLowestSetBit().ToStringUtf8(),
														//		size: 14, pivot: new(1, 0.50f), offset: new(-8, 0));
													}

													GUI.FocusableAsset(h_material, rect: group_row.GetOuterRect());

													if (GUI.Selectable3(id: id.hash, rect: group_row.GetOuterRect(), selected: false))
													{
														h_item_selected.Toggle(h_material);
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
			}
		}

		[ISystem.LateGUI(ISystem.Mode.Single, ISystem.Scope.Global)]
		public static void OnGUI_Test(ISystem.Info.Global info, ref Region.Data.Global region, [Source.Owned] ref World.Global world)
		{
			if (true)
			{
				var gui = new NEI.TestGUI()
				{

				};
				gui.Submit();
			}
		}
#endif
	}
}

