
using System.Diagnostics.CodeAnalysis;

namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		public static partial class Mode
		{
			public static partial class Construct
			{
				[IComponent.Data(Net.SendType.Reliable, name: "Wrench (Construct)")]
				public partial struct Data: IComponent, Wrench.IMode
				{
					public IRecipe.Handle selected_recipe;

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 0, 1, 24, 24, 0, 0);
					public static string Name { get; } = "Construct";

					public Crafting.Recipe.Tags RecipeTags => Crafting.Recipe.Tags.Construction;
					public Color32BGRA ColorOk => Color32BGRA.Green;
					public Color32BGRA ColorError => Color32BGRA.Red;
					public Color32BGRA ColorNew => Color32BGRA.Yellow;

					[UnscopedRef] public ref IRecipe.Handle SelectedRecipe => ref this.selected_recipe;

#if CLIENT
					public void SendSetRecipeRPC(Entity ent_wrench, IRecipe.Handle recipe)
					{
						var rpc = new Wrench.Mode.Construct.EditRPC
						{
							recipe = recipe
						};
						rpc.Send(ent_wrench);
					}

					public void Draw(Entity ent_wrench, ref Wrench.Data wrench)
					{
						using (GUI.Group.New(size: new Vector2(48 + 32 + 2, GUI.GetRemainingHeight())))
						{
							using (var scrollbox = GUI.Scrollbox.New("wrench.recipes", GUI.GetRemainingSpace(), padding: new Vector2(4, 4)))
							{
								GUI.DrawBackground(GUI.tex_window, scrollbox.group_frame.GetInnerRect(), padding: new(8));

								var recipes = IRecipe.Database.GetAssets();
								foreach (var d_recipe in recipes)
								{
									ref var recipe = ref d_recipe.GetData();
									if (recipe.IsNotNull())
									{
										if (recipe.type == Crafting.Recipe.Type.Wrench && recipe.tags.HasAll(this.RecipeTags))
										{
											using (GUI.ID.Push(d_recipe.id))
											{
												using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), 48)))
												{
													var frame_size = new Vector2(48, 48);
													var selected = this.SelectedRecipe.id == d_recipe.id;
													using (var button = GUI.CustomButton.New(recipe.name, frame_size, sound: GUI.sound_select, sound_volume: 0.10f))
													{
														GUI.Draw9Slice((selected || button.hovered) ? GUI.tex_slot_simple_hover : GUI.tex_slot_simple, new Vector4(4), button.bb);
														GUI.DrawSpriteCentered(recipe.icon, button.bb, layer: GUI.Layer.Window, scale: 2.00f);

														if (button.pressed)
														{
															this.SendSetRecipeRPC(ent_wrench, d_recipe);
														}
													}
													if (GUI.IsItemHovered())
													{
														using (GUI.Tooltip.New())
														{
															using (GUI.Wrap.Push(256))
															{
																GUI.Title(recipe.name);
																GUI.Text(recipe.desc, color: GUI.font_color_default);
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
#endif
				}

				public struct EditRPC: Net.IRPC<Wrench.Mode.Construct.Data>
				{
					public IRecipe.Handle? recipe;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Construct.Data data)
					{
						if (this.recipe.HasValue)
						{
							data.selected_recipe = this.recipe.Value;
						}

						data.Sync(entity);
					}
#endif
				}
			}
		}
	}
}
