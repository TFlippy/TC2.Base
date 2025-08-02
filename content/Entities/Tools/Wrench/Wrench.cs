
namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data: IComponent
		{
			public IComponent.Handle selected_component_id;
		}

		public interface IMode: IComponent
		{
			public static abstract Sprite Icon { get; }
			public static abstract string Name { get; }
			public Recipe.Type RecipeType { get; }

			public Color32BGRA ColorOk { get; }
			public Color32BGRA ColorError { get; }
			public Color32BGRA ColorNew { get; }

			public bool IsRecipeValid(ref Region.Data region, ref IRecipe.Data recipe)
			{
				return recipe.IsNotNull() && recipe.type == this.RecipeType && recipe.flags.HasNone(Recipe.Flags.Disabled) && recipe.placement.HasValue;
			}

#if CLIENT
			public void Draw(GUI.Window window, Entity ent_wrench, ref Wrench.Data wrench);
#endif
		}

		public interface ITargetInfo
		{
			public Entity Entity { get; }
			public IComponent.Handle ComponentID { get; }

			public Vector2 Position { get; }
			public float Radius { get; }

			public bool IsSelectable { get; }
			public bool IsSource { get; }
			public bool IsAlive { get; }
			public bool IsValid { get; }
		}

		// TODO: shithack, can't access default interface methods on structs without boxing
		public static Wrench.Mode.Build.Errors EvaluateNode<T, TInfo, TLink>(ref this T self, ref Region.Data region, ref TInfo info, ref IRecipe.Data recipe, IFaction.Handle faction_id = default) where T : unmanaged, ILinkerMode<TInfo, TLink> where TInfo : unmanaged, ITargetInfo where TLink : unmanaged, IComponent, ILink
		{
			return self.EvaluateNode(ref region, ref info, ref recipe, faction_id: faction_id);
		}

		// TODO: shithack, can't access default interface methods on structs without boxing
		public static Wrench.Mode.Build.Errors EvaluateNodePair<T, TInfo, TLink>(ref this T self, ref Region.Data region, ref TInfo info_src, ref TInfo info_dst, ref IRecipe.Data recipe, out float distance, IFaction.Handle faction_id = default) where T : unmanaged, ILinkerMode<TInfo, TLink> where TInfo : unmanaged, ITargetInfo where TLink : unmanaged, IComponent, ILink
		{
			return self.EvaluateNodePair(ref region, ref info_src, ref info_dst, ref recipe, out distance, faction_id: faction_id);
		}

		public interface ITargeterMode<TInfo>: IMode where TInfo : unmanaged, ITargetInfo
		{
			public Entity EntTarget { get; }

			public Physics.Layer LayerMask { get; }
			public Physics.Layer LayerRequire { get; }
			public Physics.Layer LayerExclude { get; }
			public TInfo CreateTargetInfo(Entity entity);

#if CLIENT
			public void SendSetTargetRPC(Entity ent_wrench, Entity ent_target);

			public void DrawInfo(Entity ent_wrench, ref TInfo info);
			public void DrawHUD(Entity ent_wrench, ref TInfo info);
#endif

#if CLIENT
			void IMode.Draw(GUI.Window window, Entity ent_wrench, ref Wrench.Data wrench)
			{
				ref var player = ref Client.GetPlayer();
				ref var region = ref Client.GetRegion();

				var faction_id = player.faction_id;

				ref readonly var kb = ref Control.GetKeyboard();
				ref readonly var mouse = ref Control.GetMouse();

				var wpos_mouse = mouse.GetInterpolatedPosition();
				var cpos_mouse = region.WorldToCanvas(wpos_mouse);

				var scale = region.GetWorldToCanvasScale();

				var info_target = this.CreateTargetInfo(this.EntTarget);
				var info_new = default(TInfo);

				var errors_target = Wrench.Mode.Build.Errors.None;
				var errors_new = Wrench.Mode.Build.Errors.None;

				//if (!info_src.valid || !info_dst.valid)
				{
					Span<OverlapResult> results = stackalloc OverlapResult[16];
					if (region.TryOverlapPointAll(wpos_mouse, 0.125f, ref results, mask: this.LayerMask, require: this.LayerRequire, exclude: this.LayerExclude))
					{
						foreach (ref var result in results)
						{
							if (result.entity == info_target.Entity) continue;

							info_new = this.CreateTargetInfo(result.entity);
							if (info_new.IsSelectable)
							{
								break;
							}
						}
					}
				}

				if (info_new.IsSelectable)
				{
					GUI.SetCursor(App.CursorType.Hand, 10);
				}

				var color_target = this.ColorOk;
				var color_new = this.ColorNew;

				//{
				//	ref var recipe = ref this.SelectedRecipe.GetRecipe();
				//	if (!recipe.IsNull())
				//	{
				//		if (info_target.IsValid)
				//		{
				//			errors_src |= this.EvaluateNode(ref region, ref info_target, ref recipe, faction_id: faction_id);
				//			if (!info_new.IsValid)
				//			{
				//				errors_new.SetFlag(Build.Errors.MaxLength | Build.Errors.OutOfRange, Vector2.Distance(info_target.Position, wpos_mouse) > recipe.placement.Value.length_max);
				//			}
				//		}

				//		if (info_dst.IsValid)
				//		{
				//			errors_target |= this.EvaluateNode(ref region, ref info_dst, ref recipe, faction_id: faction_id);
				//			if (info_target.IsValid)
				//			{
				//				errors_target |= this.EvaluateNodePair(ref region, ref info_target, ref info_dst, ref recipe, out _, player.faction_id);
				//			}
				//		}

				//		if (info_new.IsValid)
				//		{
				//			errors_new |= this.EvaluateNode(ref region, ref info_new, ref recipe, faction_id: faction_id);
				//			if (info_target.IsValid)
				//			{
				//				errors_new |= this.EvaluateNodePair(ref region, ref info_target, ref info_new, ref recipe, out _, player.faction_id);
				//			}
				//		}
				//	}
				//}

				if (errors_target != Wrench.Mode.Build.Errors.None)
				{
					color_target = this.ColorError;
				}

				if (errors_new != Wrench.Mode.Build.Errors.None)
				{
					color_new = this.ColorError;
				}

				if (info_target.IsSelectable)
				{
					//GUI.DrawCircle(info_target.Position.WorldToCanvas(), info_target.Radius * scale, color_target, 2.00f);
					GUI.DrawEntity(info_target.Entity, color_target.WithAlphaMult(0.50f));
				}

				if (info_new.IsSelectable)
				{
					//GUI.DrawCircle(info_new.Position.WorldToCanvas(), info_new.Radius * scale, color_new.WithAlphaMult(0.50f), 2.00f);
					GUI.DrawEntity(info_new.Entity, color_new.WithAlphaMult(0.50f));
				}

				if (mouse.GetKeyDown(Mouse.Key.Left))
				{
					if (info_new.IsSelectable)
					{
						if (errors_new == Wrench.Mode.Build.Errors.None)
						{
							this.SendSetTargetRPC(ent_wrench, ent_target: info_new.Entity);
							Sound.PlayGUI(GUI.sound_select, volume: 0.07f, pitch: info_new.IsSource ? 0.80f : 0.95f);
						}
						else
						{
							Sound.PlayGUI(GUI.sound_error, 0.50f);
						}
					}
				}
				else if (mouse.GetKeyDown(Mouse.Key.Right))
				{
					if (info_target.IsSelectable)
					{
						this.SendSetTargetRPC(ent_wrench, ent_target: default);
						Sound.PlayGUI(GUI.sound_select, volume: 0.07f, pitch: 0.80f);
					}
				}

				using (var group = GUI.Group.New(size: GUI.GetAvailableSize(), padding: new(4)))
				{
					GUI.DrawBackground(GUI.tex_panel, group.GetOuterRect(), padding: new(8));
					this.DrawInfo(ent_wrench, ref info_target);
				}

				if (info_target.IsSelectable)
				{
					this.DrawHUD(ent_wrench, ref info_target);
				}
			}
#endif
		}


		public interface ILinkerMode<TInfo, TLink>: IMode where TInfo : unmanaged, ITargetInfo where TLink : unmanaged, IComponent, ILink
		{
			public Entity EntitySrc { get; }
			public Entity EntityDst { get; }

			public IComponent.Handle ComponentSrc { get; }
			public IComponent.Handle ComponentDst { get; }

			public IRecipe.Handle SelectedRecipe { get; }

			public Physics.Layer LayerMask { get; }
			public TInfo CreateTargetInfo(ref Region.Data.Common region, Entity entity, IComponent.Handle h_component, Vector2 pos, bool is_src);

#if CLIENT
			public void SendSetTargetRPC(Entity ent_wrench, Entity ent_src, IComponent.Handle h_component_src, Entity ent_dst, IComponent.Handle h_component_dst);
			public void SendSetRecipeRPC(Entity ent_wrench, IRecipe.Handle recipe);

			public void DrawInfo(Entity ent_wrench, ref TInfo info_src, ref TInfo info_dst, Wrench.Mode.Build.Errors errors_src, Wrench.Mode.Build.Errors errors_dst, float distance);
			public void DrawHUD(Entity ent_wrench, ref TInfo info_src, ref TInfo info_dst, Wrench.Mode.Build.Errors errors_src, Wrench.Mode.Build.Errors errors_dst, float distance);
			public void DrawNode(Entity ent_wrench, ref TInfo info, Color32BGRA color)
			{

			}
#endif
			public Wrench.Mode.Build.Errors EvaluateNode(ref Region.Data region, ref TInfo info, ref IRecipe.Data recipe, IFaction.Handle faction_id = default)
			{
				var errors = Wrench.Mode.Build.Errors.None;

				if (this.IsRecipeValid(ref region, ref recipe) && info.IsSelectable)
				{
					var placement = recipe.placement.Value;

					var claim_ratio = Claim.GetOverlapRatio(ref region, AABB.Circle(info.Position, 1.00f), faction_id: faction_id);
					errors.SetFlag(Wrench.Mode.Build.Errors.Claimed, claim_ratio < placement.min_claim);
				}
				else
				{
					errors |= Wrench.Mode.Build.Errors.Invalid;
				}

				return errors;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public Wrench.Mode.Build.Errors EvaluateNodePair(ref Region.Data region, ref TInfo info_src, ref TInfo info_dst, ref IRecipe.Data recipe, out float distance, IFaction.Handle faction_id = default)
			{
				var errors = Wrench.Mode.Build.Errors.None;
				distance = 0.00f;

				if (this.IsRecipeValid(ref region, ref recipe) && info_src.IsSelectable && info_dst.IsSelectable && (info_src.Entity != info_dst.Entity || info_src.ComponentID != info_dst.ComponentID))
				{
					var placement = recipe.placement.Value;

					var dir = (info_src.Position - info_dst.Position).GetNormalized(out distance);
					errors.SetFlag(Wrench.Mode.Build.Errors.OutOfRange | Wrench.Mode.Build.Errors.MaxLength, distance > placement.length_max);

					var claim_ratio = Maths.Min(Claim.GetOverlapRatio(ref region, AABB.Circle(info_src.Position, 1.00f), faction_id: faction_id), Claim.GetOverlapRatio(ref region, AABB.Circle(info_dst.Position, 1.00f), faction_id: faction_id));
					errors.SetFlag(Wrench.Mode.Build.Errors.Claimed, claim_ratio < placement.min_claim);

					var pos_mid = (info_src.Position + info_dst.Position) * 0.50f;

					Span<OverlapResult> results = stackalloc OverlapResult[32];
					if (region.TryOverlapPointAll(pos_mid, 1.00f, ref results, mask: this.LayerMask))
					{
						foreach (ref var result in results)
						{
							ref var link = ref result.entity.GetComponent<TLink>();
							if (!link.IsNull())
							{
								var ent_link_src = link.EntityA;
								var ent_link_dst = link.EntityB;

								//App.WriteLine($"{ent_link_src} == {info_src.Entity}; {ent_link_dst} == {info_dst.Entity}; {link.ComponentA} == {info_src.ComponentID}; {link.ComponentB} == {info_dst.ComponentID}");

								if ((ent_link_src == info_src.Entity && ent_link_dst == info_dst.Entity && link.ComponentA == info_src.ComponentID && link.ComponentB == info_dst.ComponentID) || (ent_link_src == info_dst.Entity && ent_link_dst == info_src.Entity && link.ComponentA == info_dst.ComponentID && link.ComponentB == info_src.ComponentID))
								{
									errors.SetFlag(Wrench.Mode.Build.Errors.Obstructed, true);
									break;
								}
							}
						}
					}
				}
				else
				{
					errors |= Wrench.Mode.Build.Errors.Invalid;
				}

				return errors;
			}

#if CLIENT
			void IMode.Draw(GUI.Window window, Entity ent_wrench, ref Wrench.Data wrench)
			{
				//App.WriteLine("test");

				ref var character = ref Client.GetCharacter();
				ref var region = ref Client.GetRegion();
				ref var region_common = ref region.AsCommon();

				var h_faction = character.faction;

				ref readonly var kb = ref Control.GetKeyboard();
				ref readonly var mouse = ref Control.GetMouse();

				var wpos_mouse = mouse.GetInterpolatedPosition();
				var cpos_mouse = region.WorldToCanvas(wpos_mouse);

				var scale = region.GetWorldToCanvasScale();

				var info_src = this.CreateTargetInfo(ref region_common, this.EntitySrc, this.ComponentSrc, wpos_mouse, true);
				var info_dst = this.CreateTargetInfo(ref region_common, this.EntityDst, this.ComponentDst, wpos_mouse, false);
				var info_new = default(TInfo);

				var errors_src = Wrench.Mode.Build.Errors.None;
				var errors_dst = Wrench.Mode.Build.Errors.None;
				var errors_new = Wrench.Mode.Build.Errors.None;

				//if (!info_src.valid || !info_dst.valid)
				{
					Span<OverlapResult> results = stackalloc OverlapResult[16];
					if (region.TryOverlapPointAll(wpos_mouse, 0.125f, ref results, mask: Physics.Layer.Entity))
					{
						foreach (ref var result in results)
						{
							if (result.entity == info_src.Entity || result.entity == info_dst.Entity) continue;

							info_new = this.CreateTargetInfo(ref region_common, result.entity, default, wpos_mouse, !info_src.IsSelectable);
							if (info_new.IsSelectable)
							{
								break;
							}
						}
					}
				}

				if (info_new.IsSelectable)
				{
					GUI.SetCursor(App.CursorType.Hand, 10);
				}

				var distance = 0.00f;
				if (info_src.IsSelectable)
				{
					if (info_dst.IsSelectable)
					{
						distance = Vector2.Distance(info_src.Position, info_dst.Position);
					}
					else if (info_new.IsSelectable)
					{
						distance = Vector2.Distance(info_src.Position, info_new.Position);
					}
					else
					{
						distance = Vector2.Distance(info_src.Position, mouse.position);
					}
				}

				var color_src = this.ColorOk;
				var color_dst = this.ColorOk;
				var color_new = this.ColorNew;

				{
					ref var recipe = ref this.SelectedRecipe.GetData();
					if (!recipe.IsNull())
					{
						if (info_src.IsSelectable)
						{
							errors_src |= this.EvaluateNode(ref region, ref info_src, ref recipe, faction_id: h_faction);
							if (!info_new.IsSelectable)
							{
								errors_new.SetFlag(Wrench.Mode.Build.Errors.MaxLength | Wrench.Mode.Build.Errors.OutOfRange, Vector2.Distance(info_src.Position, wpos_mouse) > recipe.placement.Value.length_max);
							}
						}

						if (info_dst.IsSelectable)
						{
							errors_dst |= this.EvaluateNode(ref region, ref info_dst, ref recipe, faction_id: h_faction);
							if (info_src.IsSelectable)
							{
								errors_dst |= this.EvaluateNodePair(ref region, ref info_src, ref info_dst, ref recipe, out _, h_faction);
							}
						}

						if (info_new.IsSelectable)
						{
							errors_new |= this.EvaluateNode(ref region, ref info_new, ref recipe, faction_id: h_faction);
							if (info_src.IsSelectable)
							{
								//errors_new |= this.EvaluateNodePair(ref region, ref info_src, ref info_new, ref recipe, out _, player.faction_id);
							}
						}
					}
				}

				if (errors_src != Wrench.Mode.Build.Errors.None)
				{
					color_src = this.ColorError;
				}

				if (errors_dst != Wrench.Mode.Build.Errors.None)
				{
					color_dst = this.ColorError;
				}

				if (errors_new != Wrench.Mode.Build.Errors.None)
				{
					color_new = this.ColorError;
				}

				this.DrawGizmos(ent_wrench, ref wpos_mouse, ref info_src, ref info_dst, ref info_new, ref color_src, ref color_dst, ref color_new);

				if (mouse.GetKeyDown(Mouse.Key.Left))
				{
					if (info_new.IsSelectable)
					{
						if (errors_new == Wrench.Mode.Build.Errors.None)
						{
							if (info_new.IsSource)
							{
								this.SendSetTargetRPC(ent_wrench,
									ent_src: info_new.Entity, h_component_src: info_new.ComponentID,
									ent_dst: this.EntityDst, h_component_dst: this.ComponentDst);
							}
							else
							{
								this.SendSetTargetRPC(ent_wrench,
									ent_src: this.EntitySrc, h_component_src: this.ComponentSrc,
									ent_dst: info_new.Entity, h_component_dst: info_new.ComponentID);

								//this.SendSetTargetRPC(ent_wrench, ent_src: info_new.IsSource ? info_new.Entity : this.EntitySrc, ent_dst: !info_new.IsSource ? info_new.Entity : this.EntityDst);
							}

							//this.SendSetTargetRPC(ent_wrench, ent_src: info_new.IsSource ? info_new.Entity : this.EntitySrc, ent_dst: !info_new.IsSource ? info_new.Entity : this.EntityDst);
							Sound.PlayGUI(GUI.sound_select, volume: 0.07f, pitch: info_new.IsSource ? 0.80f : 0.95f);
						}
						else
						{
							Sound.PlayGUI(GUI.sound_error, 0.50f);
						}
					}
				}
				else if (mouse.GetKeyDown(Mouse.Key.Right))
				{
					if (info_src.IsSelectable || info_dst.IsSelectable)
					{
						if (info_dst.IsSelectable)
						{
							this.SendSetTargetRPC(ent_wrench,
								ent_src: this.EntitySrc, h_component_src: this.ComponentSrc,
								ent_dst: default, h_component_dst: default);
						}
						else
						{
							this.SendSetTargetRPC(ent_wrench,
								ent_src: default, h_component_src: default,
								ent_dst: default, h_component_dst: default);
						}

						//this.SendSetTargetRPC(ent_wrench, ent_src: info_dst.IsSelectable ? info_src.Entity : default, ent_dst: default);
						Sound.PlayGUI(GUI.sound_select, volume: 0.07f, pitch: 0.80f);
					}
				}

				//using (GUI.Group.New(size: new Vector2(48 + 32 + 2, GUI.RmY)))
				using (GUI.Group.New(size: new Vector2(48 + 30, GUI.RmY)))
				{
					using (var scrollbox = GUI.Scrollbox.New("wrench.recipes"u8, GUI.Rm, padding: new Vector2(4, 4)))
					{
						GUI.DrawBackground(GUI.tex_window, scrollbox.group_frame.GetInnerRect(), padding: new(8));

						var recipes = IRecipe.Database.GetAssetsSpan();
						foreach (var d_recipe in recipes)
						{
							ref var recipe = ref d_recipe.GetData();
							if (recipe.IsNotNull())
							{
								if (recipe.type == this.RecipeType)
								{
									using (GUI.ID.Push(d_recipe.id))
									using (GUI.Group.New(size: new Vector2(GUI.RmX, 48)))
									{
										var frame_size = new Vector2(48, 48);
										var selected = this.SelectedRecipe.id == d_recipe.id;
										using (var button = GUI.CustomButton.New(recipe.name, frame_size, sound: GUI.sound_select, sound_volume: 0.10f, enabled: recipe.flags.HasNone(Recipe.Flags.Disabled)))
										{
											GUI.Draw9Slice((selected || button.hovered) ? GUI.tex_slot_simple_hover : GUI.tex_slot_simple, new Vector4(4), button.bb);

											using (var clip = GUI.Clip.Push(button.group.GetInnerRect().Shrink(6), cull: true))
											{
												if (clip.visible)
												{
													GUI.DrawSpriteCentered(recipe.icon, button.bb, layer: GUI.Layer.Window, scale: 2.00f);
												}
											}

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
													GUI.Title(recipe.GetName());
													GUI.TextShaded(recipe.GetDescription().OrDefault(recipe.GetDescriptionFallback()), color: GUI.font_color_default);
												}
											}
										}
										GUI.FocusableAsset(d_recipe.GetHandle());
									}
								}
							}
						}
					}
				}

				GUI.SameLine();

				using (var group = GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY), padding: new(4)))
				{
					GUI.DrawBackground(GUI.tex_panel, group.GetOuterRect(), padding: new(8));
					this.DrawInfo(ent_wrench, ref info_src, ref info_dst, errors_src, errors_dst, distance);
				}

				if (info_src.IsSelectable && info_dst.IsSelectable)
				{
					this.DrawHUD(ent_wrench, ref info_src, ref info_dst, errors_src, errors_dst, distance);
				}
			}

			public void DrawGizmos(Entity ent_wrench, ref Vector2 wpos_mouse, ref TInfo info_src, ref TInfo info_dst, ref TInfo info_new, ref Color32BGRA color_src, ref Color32BGRA color_dst, ref Color32BGRA color_new)
			{
				ref var region = ref ent_wrench.GetRegionCommon();

				if (info_src.IsSelectable)
				{
					if (!info_new.IsSelectable && !info_dst.IsSelectable)
					{
						var dir = (info_src.Position - wpos_mouse).GetNormalizedFast();
						var n = new Vector2(-dir.Y, dir.X);
						var offset_src = n * info_src.Radius;
						var offset_mouse = n * info_src.Radius * 0.50f;

						GUI.DrawLine2(region.WorldToCanvas(info_src.Position + offset_src), region.WorldToCanvas(wpos_mouse + offset_mouse), color_src, color_new.WithAlphaMult(0.00f), 2.00f, 2.00f);
						GUI.DrawLine2(region.WorldToCanvas(info_src.Position - offset_src), region.WorldToCanvas(wpos_mouse - offset_mouse), color_src, color_new.WithAlphaMult(0.00f), 2.00f, 2.00f);
					}

					if (info_new.IsSelectable)
					{
						var dir = (info_src.Position - info_new.Position).GetNormalizedFast();
						var n = new Vector2(-dir.Y, dir.X);
						var offset_src = n * info_src.Radius;
						var offset_new = n * info_new.Radius;

						GUI.DrawLine2(region.WorldToCanvas(info_src.Position + offset_src), region.WorldToCanvas(info_new.Position + offset_new), color_src, color_new.WithAlphaMult(0.50f), 2.00f, 2.00f);
						GUI.DrawLine2(region.WorldToCanvas(info_src.Position - offset_src), region.WorldToCanvas(info_new.Position - offset_new), color_src, color_new.WithAlphaMult(0.50f), 2.00f, 2.00f);
					}

					if (info_dst.IsSelectable)
					{
						var dir = (info_src.Position - info_dst.Position).GetNormalizedFast();
						var n = new Vector2(-dir.Y, dir.X);
						var offset_src = n * info_src.Radius;
						var offset_dst = n * info_dst.Radius;

						GUI.DrawLine2(region.WorldToCanvas(info_src.Position + offset_src), region.WorldToCanvas(info_dst.Position + offset_dst), color_src, color_dst, 2.00f, 2.00f);
						GUI.DrawLine2(region.WorldToCanvas(info_src.Position - offset_src), region.WorldToCanvas(info_dst.Position - offset_dst), color_src, color_dst, 2.00f, 2.00f);
					}
				}

				if (info_src.IsSelectable)
				{
					GUI.DrawCircle(region.WorldToCanvas(info_src.Position), info_src.Radius * region.GetWorldToCanvasScale(), color_src, 2.00f);
					this.DrawNode(ent_wrench, ref info_src, color_src);
				}

				if (info_dst.IsSelectable)
				{
					GUI.DrawCircle(region.WorldToCanvas(info_dst.Position), info_dst.Radius * region.GetWorldToCanvasScale(), color_dst, 2.00f);
					this.DrawNode(ent_wrench, ref info_dst, color_dst);
				}

				if (info_new.IsSelectable)
				{
					GUI.DrawCircle(region.WorldToCanvas(info_new.Position), info_new.Radius * region.GetWorldToCanvasScale(), color_new.WithAlphaMult(0.50f), 2.00f);
					this.DrawNode(ent_wrench, ref info_new, color_new);
				}
			}
#endif
		}

		public struct SelectModeRPC: Net.IRPC<Wrench.Data>
		{
			public IComponent.Handle selected_component_id;

#if SERVER
			public readonly void Invoke(Net.IRPC.Context rpc, ref Wrench.Data data)
			{
				if (rpc.entity.HasComponent(this.selected_component_id))
				{
					data.selected_component_id = this.selected_component_id;
					data.Sync(rpc.entity);
				}
			}
#endif
		}

#if CLIENT
		public static void DrawModeButton<T>(Entity ent_wrench, ref Wrench.Data wrench) where T : unmanaged, Wrench.IMode
		{
			ref readonly var info = ref ECS.GetInfo<T>();

			using (GUI.ID<Wrench.Data>.Push(info.id))
			{
				if (GUI.DrawIconButton($"wrench.mode.{info.identifier}", T.Icon, new(64, 64), color: info.id == wrench.selected_component_id ? GUI.col_button_highlight : GUI.col_button))
				{
					var rpc = new Wrench.SelectModeRPC()
					{
						selected_component_id = info.id
					};
					rpc.Send(ent_wrench);
				}

				if (GUI.IsItemHovered())
				{
					using (GUI.Tooltip.New())
					{
						GUI.Title(T.Name);
					}
				}
			}
		}

		public const string dock_identifier = "wrench.mode.dock";
		public static Sprite sprite_wrench = new Sprite("wrench", 8, 16, 0, 0);

		public struct WrenchGUI: IGUICommand
		{
			public Entity ent_wrench;

			public Transform.Data transform;
			public Wrench.Data wrench;

			[Shitcode]
			public void Draw()
			{
				//var window_size = new Vector2(422, 500);

				//using (var window = GUI.Window.Standalone("Wrench", size: window_size, padding: new(8, 8), pivot: new(0.50f, 0.50f)))
				//using (var window = GUI.Window.Interaction("Wrench"u8, this.ent_wrench, padding: new(0, 0), no_mouse_close: true))
				//{

				using (var widget = Sidebar.Widget.New(identifier: "wrench", name: "Wrench", icon: sprite_wrench,
				size: new Vector2(454, 442), //size_min: new Vector2(454 - 96, 256),
				lockable: false, order: 2.00f,
				flags: Sidebar.Widget.Flags.Force_Open | Sidebar.Widget.Flags.Has_Window | Sidebar.Widget.Flags.Show_As_Selected | Sidebar.Widget.Flags.Align_Right | Sidebar.Widget.Flags.Fade | Sidebar.Widget.Flags.Enabled/* | Sidebar.Widget.Flags.Resizable*/))
				{
					if (widget.state_flags.HasAny(Sidebar.Widget.StateFlags.Show))
					{
						Editor.SkipGUI();

						//this.StoreCurrentWindowTypeID();
						//if (window.show)
						//{
						//GUI.DrawWindowBackground(GUI.tex_window_menu, new Vector4(8, 8, 8, 8));

						ref var region = ref Client.GetRegion();

						using (GUI.Group.New(size: new(GUI.AvX, 64)))
						{
							if (this.ent_wrench.HasComponent<Wrench.Mode.Build.Data>())
							{
								Wrench.DrawModeButton<Wrench.Mode.Build.Data>(this.ent_wrench, ref this.wrench);
								GUI.SameLine();
							}

							if (this.ent_wrench.HasComponent<Wrench.Mode.Belts.Data>())
							{
								Wrench.DrawModeButton<Wrench.Mode.Belts.Data>(this.ent_wrench, ref this.wrench);
								GUI.SameLine();
							}

							if (this.ent_wrench.HasComponent<Wrench.Mode.Conveyors.Data>())
							{
								Wrench.DrawModeButton<Wrench.Mode.Conveyors.Data>(this.ent_wrench, ref this.wrench);
								GUI.SameLine();
							}

							if (this.ent_wrench.HasComponent<Wrench.Mode.Pipes.Data>())
							{
								Wrench.DrawModeButton<Wrench.Mode.Pipes.Data>(this.ent_wrench, ref this.wrench);
								GUI.SameLine();
							}

							//if (this.ent_wrench.HasComponent<Wrench.Mode.Deconstruct.Data>())
							//{
							//	Wrench.DrawModeButton<Wrench.Mode.Deconstruct.Data>(this.ent_wrench, ref this.wrench);
							//	GUI.SameLine();
							//}

							//if (this.ent_wrench.HasComponent<Wrench.Mode.Repair.Data>())
							//{
							//	Wrench.DrawModeButton<Wrench.Mode.Repair.Data>(this.ent_wrench, ref this.wrench);
							//	GUI.SameLine();
							//}
						}

						GUI.SeparatorThick();

						using (GUI.Group.New(size: GUI.Av))
						{
							using (var dock = GUI.Dock.New(Wrench.dock_identifier))
							{
								dock.SetSpace(size: GUI.Rm);
							}
						}
					}
				}
			}
		}

		public struct WrenchModeGUI<T>: IGUICommand where T : unmanaged, Wrench.IMode
		{
			public Entity ent_wrench;

			public Transform.Data transform;
			public Wrench.Data wrench;
			public T mode;

			public void Draw()
			{
				ref readonly var info = ref ECS.GetInfo<T>();

				using (var window = GUI.Window.Docked(info.identifier, dock_identifier: Wrench.dock_identifier))
				{
					if (window.show)
					{
						this.mode.Draw(window, this.ent_wrench, ref this.wrench);
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent), HasTag("dead", false, Source.Modifier.Parent), HasComponent<Interactor.Data>(Source.Modifier.Parent, true)]
		public static void OnGUI(ISystem.Info info, Entity entity,
		[Source.Owned] ref Wrench.Data wrench, [Source.Owned] in Transform.Data transform)
		{
			//if (interactable.IsActive())
			{
				var gui = new WrenchGUI()
				{
					ent_wrench = entity,
					transform = transform,
					wrench = wrench
				};
				gui.Submit();
			}
		}

		[ISystem.LateGUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent), HasTag("dead", false, Source.Modifier.Parent), HasComponent<Interactor.Data>(Source.Modifier.Parent, true)]
		public static void OnGUIMode<T>(ISystem.Info info, Entity entity,
		[Source.Owned] in T mode, [Source.Owned] ref Wrench.Data wrench, [Source.Owned] in Transform.Data transform) where T : unmanaged, Wrench.IMode
		{
			//if (interactable.IsActive() && wrench.selected_component_id == ECS.GetID<T>())
			if (wrench.selected_component_id == ECS.GetID<T>())
			{
				var gui = new WrenchModeGUI<T>()
				{
					ent_wrench = entity,
					transform = transform,
					wrench = wrench,
					mode = mode
				};
				gui.Submit();
			}
		}
#endif

#if SERVER
		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("initialized", true, Source.Modifier.Owned)]
		public static void OnAdd<T>(ISystem.Info info, Entity entity, [Source.Owned] ref T mode, [Source.Owned] ref Wrench.Data wrench) where T : unmanaged, Wrench.IMode
		{
			if (!wrench.selected_component_id)
			{
				wrench.selected_component_id.SetComponent<T>();
				wrench.Sync(entity);
			}
		}
#endif

		//[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void Update(ISystem.Info info, Entity entity,
		//[Source.Owned] ref Wrench.Data wrench, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] in Body.Data body)
		//{

		//}
	}
}
