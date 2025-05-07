namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		public static partial class Mode
		{
			public static partial class Belts
			{
				[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region, name: "Wrench (Belts)")]
				public partial struct Data: IComponent, Wrench.IMode, Wrench.ILinkerMode<Belts.TargetInfo, Belt.Data>
				{
					[Save.Ignore] public Entity ent_src;
					[Save.Ignore] public Entity ent_dst;

					[Save.Ignore] public IComponent.Handle h_component_src;
					[Save.Ignore] public IComponent.Handle h_component_dst;

					[Asset.Ignore] public IRecipe.Handle selected_recipe;
					[Asset.Ignore] public Belt.Flags flags;

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 24, 24, 0, 0);
					public static string Name { get; } = "Belts";

					public readonly Recipe.Type RecipeType => Recipe.Type.Belt;
					public readonly Physics.Layer LayerMask => Physics.Layer.Belt;
					public readonly Color32BGRA ColorOk => Color32BGRA.Green;
					public readonly Color32BGRA ColorError => Color32BGRA.Red;
					public readonly Color32BGRA ColorNew => Color32BGRA.Yellow;

					public readonly Entity EntitySrc => this.ent_src;
					public readonly Entity EntityDst => this.ent_dst;

					public readonly IComponent.Handle ComponentSrc => this.h_component_src;
					public readonly IComponent.Handle ComponentDst => this.h_component_dst;

					public readonly IRecipe.Handle SelectedRecipe => this.selected_recipe;

					public readonly TargetInfo CreateTargetInfo(ref Region.Data.Common region, Entity entity, IComponent.Handle h_component, Vector2 pos, bool is_src)
					{
						return new TargetInfo(entity, is_src);
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
						var rpc = new Wrench.Mode.Belts.EditRPC
						{
							recipe = recipe
						};
						rpc.Send(ent_wrench);
					}

					public readonly void DrawInfo(Entity ent_wrench, ref TargetInfo info_src, ref TargetInfo info_dst, Build.Errors errors_src, Build.Errors errors_dst, float distance)
					{
						ref var recipe = ref this.selected_recipe.GetData();
						if (recipe.IsNotNull() && recipe.placement.HasValue)
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

					public readonly void DrawNode(Entity ent_wrench, ref TargetInfo info, Color32BGRA color)
					{
						ref var axle_state = ref info.entity.GetComponent<Axle.State>();
						if (axle_state.IsNotNull())
						{
							GUI.DrawTextCentered($"{axle_state.angular_velocity:0.00} rad/s\n{axle_state.net_torque:0.00} Nm/s", info.Position.WorldToCanvas());
						}
					}

					public void DrawHUD(Entity ent_wrench, ref TargetInfo info_src, ref TargetInfo info_dst, Build.Errors errors_src, Build.Errors errors_dst, float distance)
					{
						ref var recipe = ref this.selected_recipe.GetData();
						if (!recipe.IsNull() && recipe.placement.HasValue)
						{
							var placement = recipe.placement.Value;

							//using (var hud = GUI.Window.Standalone("Wrench.HUD", position: (info_src.pos - new Vector2(0.00f, info_src.radius + 0.25f)).WorldToCanvas(), size: new(168, 100), pivot: new(0.50f, 1.00f)))
							//using (var hud = GUI.Window.Standalone("Wrench.HUD", position: (info_dst.pos - new Vector2(0.00f, info_dst.radius + 0.25f)).WorldToCanvas(), size: new(168, 100), pivot: new(0.50f, 1.00f)))
							using (var hud = GUI.Window.Standalone("Wrench.HUD"u8, position: ((info_src.pos + info_dst.pos) * 0.50f).WorldToCanvas(), size: new(168, 100), pivot: new(0.50f, 0.50f)))
							{
								if (hud.show)
								{
									GUI.DrawBackground(GUI.tex_panel, hud.group.GetOuterRect(), padding: new(4));

									using (GUI.Group.New(size: GUI.Rm - new Vector2(0, 48), padding: new(4)))
									{
										GUI.LabelShaded("Distance:"u8, distance, $"{{0:0.00}}/{placement.length_max:0.00} m");

										if (GUI.Checkbox("Reversed"u8, ref this.flags, Belt.Flags.Crossed, size: new Vector2(GUI.RmX, 32)))
										{
											var rpc = new Wrench.Mode.Belts.EditRPC
											{
												flags = this.flags
											};
											rpc.Send(ent_wrench);
										}
									}

									using (GUI.Group.Centered(outer_size: GUI.Rm, inner_size: new(100, 40)))
									{
										if (GUI.DrawButton("Create"u8, new Vector2(100, 40), enabled: info_src.valid && info_dst.valid, error: (errors_src | errors_dst) != Build.Errors.None, color: GUI.col_button_ok))
										{
											var rpc = new Wrench.Mode.Belts.ConfirmRPC()
											{

											};
											rpc.Send(ent_wrench);
										}
										GUI.DrawHoverTooltip("Create a belt connection."u8);
									}
								}
							}
						}
					}
#endif

					//#if SERVER
					//					[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region)]
					//					public static void OnAdd(ISystem.Info info, Entity entity, [Source.Owned] ref Wrench.Mode.Belts.Data mode, [Source.Owned] ref Wrench.Data wrench) 
					//					{
					//						if (mode.selected_recipe.id == 0)
					//						{
					//							mode.selected_recipe = 
					//							mode.Sync(entity, true);
					//						}
					//					}
					//#endif
				}

				public struct TargetInfo: ITargetInfo
				{
					public Entity entity;

					public Axle.Data axle;
					public Transform.Data transform;

					public float radius;
					public Vector2 pos;

					public bool is_src;
					public bool alive;
					public bool valid;
					public bool selectable;

					public readonly Entity Entity => this.entity;
					public readonly IComponent.Handle ComponentID => ECS.GetID<Axle.Data>();
					public readonly Vector2 Position => this.pos;
					public readonly float Radius => this.radius;
					public readonly bool IsSource => this.is_src;
					public readonly bool IsSelectable => this.valid;
					public readonly bool IsAlive => this.alive;
					public readonly bool IsValid => this.valid;

					public TargetInfo(Entity entity, bool is_src)
					{
						this.entity = entity;
						this.is_src = is_src;
						this.alive = this.entity.IsAlive();

						if (this.alive)
						{
							this.valid = true;

							this.valid &= this.entity.GetComponent<Axle.Data>().TryGetValueFromRef(out this.axle);
							this.valid &= this.entity.GetComponent<Transform.Data>().TryGetValueFromRef(out this.transform);

							if (this.valid)
							{
								this.radius = this.is_src ? this.axle.radius_a : this.axle.radius_b;
								this.pos = this.transform.LocalToWorld(this.axle.offset + (this.is_src ? this.axle.offset_inner : default));
							}
						}
					}
				}

				public struct SetTargetRPC: Net.IRPC<Wrench.Mode.Belts.Data>
				{
					public Entity ent_src;
					public Entity ent_dst;

					public IComponent.Handle h_component_src;
					public IComponent.Handle h_component_dst;

#if SERVER
					public void Invoke(Net.IRPC.Context rpc, ref Wrench.Mode.Belts.Data data)
					{
						//App.WriteLine($"{this.ent_src} == {data.ent_src}; {this.ent_dst} == {data.ent_dst}");

						ref var region = ref rpc.entity.GetRegion();

						data.ent_src = this.ent_src;
						data.ent_dst = this.ent_dst;
						data.Sync(rpc.entity);
					}
#endif
				}

				public struct EditRPC: Net.IRPC<Wrench.Mode.Belts.Data>
				{
					public IRecipe.Handle? recipe;
					public Belt.Flags? flags;

#if SERVER
					public void Invoke(Net.IRPC.Context rpc, ref Wrench.Mode.Belts.Data data)
					{
						if (this.recipe.HasValue)
						{
							data.selected_recipe = this.recipe.Value;
						}

						if (this.flags.HasValue)
						{
							data.flags = this.flags.Value;
						}

						data.Sync(rpc.entity);
					}
#endif
				}

				public struct ConfirmRPC: Net.IRPC<Wrench.Mode.Belts.Data>
				{
#if SERVER
					public void Invoke(Net.IRPC.Context rpc, ref Wrench.Mode.Belts.Data data)
					{
						ref var region = ref rpc.entity.GetRegion();
						ref var player = ref rpc.connection.GetPlayerData();
						ref var recipe = ref data.selected_recipe.GetData();

						if (!region.IsNull() && !player.IsNull() && !recipe.IsNull())
						{
							var errors = Build.Errors.None;

							var info_src = new TargetInfo(data.ent_src, true);
							var info_dst = new TargetInfo(data.ent_dst, false);

							if (info_src.valid && info_dst.valid)
							{
								var pos_mid = (info_src.pos + info_dst.pos) * 0.50f;

								errors |= data.EvaluateNodePair<Wrench.Mode.Belts.Data, Wrench.Mode.Belts.TargetInfo, Belt.Data>(ref region, ref info_src, ref info_dst, ref recipe, out _, player.faction_id);
								if (errors == Build.Errors.None)
								{
									var arg = (data.ent_src, data.ent_dst, data.flags);

									region.SpawnPrefab(recipe.products[0].prefab, pos_mid).ContinueWith(ent =>
									{
										ref var belt = ref ent.GetComponent<Belt.Data>();
										if (!belt.IsNull())
										{
											belt.a.Set(arg.ent_src);
											belt.b.Set(arg.ent_dst);

											belt.a_state.Set(arg.ent_src);
											belt.b_state.Set(arg.ent_dst);

											belt.flags = arg.flags;

											ent.MarkModified<Belt.Data>(sync: true);
										}
									});

									data.ent_src = default;
									data.ent_dst = default;
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
