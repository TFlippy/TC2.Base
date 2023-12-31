
using System.Diagnostics.CodeAnalysis;

namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		public static partial class Mode
		{
			public static partial class Repair
			{
				[Flags]
				public enum Flags: uint
				{
					None = 0,

					Active = 1 << 0,
				}

				[Flags]
				public enum Filter: uint
				{
					None = 0,

					[Name("Buildings")]
					Buildings = 1u << 0,

					[Name("Belts")]
					Belts = 1u << 1,

					[Name("Conveyors")]
					Conveyors = 1u << 2,

					[Name("Ladders")]
					Ladders = 1u << 3,

					[Name("Doors")]
					Doors = 1u << 4,
				}

				[IComponent.Data(Net.SendType.Reliable, name: "Wrench (Repair)", region_only: true)]
				public partial struct Data: IComponent, Wrench.IMode, Wrench.ITargeterMode<TargetInfo>
				{
					public static readonly Sound.Handle sound_repair_default = "wrench.repair.00";

					public EntRef<Repairable.Data> ref_repairable;
					public EntRef<Health.Data> ref_health;
					public Repair.Flags flags;
					public Repair.Filter filter = Filter.Buildings | Filter.Belts | Filter.Conveyors | Filter.Ladders | Filter.Doors;
					public Sound.Handle sound_repair = sound_repair_default;

					//[Save.Ignore, Net.Ignore]
					//public Vector2 target_pos;

					public Entity EntTarget => this.ref_repairable.entity;

					public TargetInfo CreateTargetInfo(Entity entity)
					{
						return new TargetInfo(entity);
					}

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 24, 24, 5, 0);
					public static string Name { get; } = "Repair";

					public Crafting.Recipe.Tags RecipeTags => Crafting.Recipe.Tags.None;
					public Physics.Layer LayerMask
					{
						get
						{
							var mask = Physics.Layer.None;

							if (this.filter.HasAny(Repair.Filter.Buildings))
							{
								mask |= Physics.Layer.Building;
							}

							if (this.filter.HasAny(Repair.Filter.Belts))
							{
								mask |= Physics.Layer.Belt;
							}

							if (this.filter.HasAny(Repair.Filter.Conveyors))
							{
								mask |= Physics.Layer.Conveyor;
							}

							if (this.filter.HasAny(Repair.Filter.Ladders))
							{
								mask |= Physics.Layer.Climbable;
							}

							if (this.filter.HasAny(Repair.Filter.Doors))
							{
								mask |= Physics.Layer.Door;
							}

							return mask;
						}
					}
					public Physics.Layer LayerRequire => Physics.Layer.Repairable | Physics.Layer.Destructible;
					public Physics.Layer LayerExclude => Physics.Layer.Harvestable;

					public Color32BGRA ColorOk => Color32BGRA.Orange;
					public Color32BGRA ColorError => Color32BGRA.Grey;
					public Color32BGRA ColorNew => Color32BGRA.White;

					public Data()
					{

					}

#if CLIENT
					public void SendSetTargetRPC(Entity ent_wrench, Entity ent_target)
					{
						var rpc = new SetTargetRPC
						{
							ent_target = ent_target
						};
						rpc.Send(ent_wrench);
					}

					public void DrawInfo(Entity ent_wrench, ref TargetInfo info)
					{
						var dirty = false;
						var rpc = new ConfigureRPC
						{

						};

						var filter_tmp = this.filter;
						using (GUI.Group.New(size: new Vector2(GUI.RmX, 40), padding: new(0, 0)))
						{
							if (GUI.EnumInput("filter", ref filter_tmp, new Vector2(GUI.RmX - 80, 40), show_label: false))
							{
								rpc.filter = filter_tmp;
								dirty = true;
							}

							GUI.SameLine();

							GUI.TitleCentered("Filter", size: 24, pivot: new(0.50f, 0.50f));
						}

						GUI.SeparatorThick();

						using (GUI.Group.New(size: new Vector2(GUI.RmX, 32), padding: new(8, 4)))
						{
							GUI.TitleCentered(info.alive ? info.entity.GetPrefabName() : "<no target selected>", size: 32, pivot: new(0.00f, 0.50f));
						}

						GUI.SeparatorThick();

						using (var group = GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY - 40), padding: new(8, 8)))
						{
							GUI.DrawBackground(GUI.tex_panel, group.GetOuterRect(), new(4));

							//GUI.Title("Retrievable resources:", size: 16);
							////GUI.SeparatorThick();

							//var items_span = info.health.items.AsSpan();

							//for (var i = 0; i < items_span.Length; i++)
							//{
							//	ref var item = ref items_span[i];
							//	switch (item.type)
							//	{
							//		case Shipment.Item.Type.Resource:
							//		{
							//			var resource = new Resource.Data(item.material, item.quantity);
							//			GUI.DrawResource(ref resource, new Vector2(GUI.RmX, 32), GUI.font_color_green);
							//		}
							//		break;
							//	}
							//}
						}

						using (GUI.Group.New(size: GUI.Rm, padding: new(0, 0)))
						{
							using (var group = GUI.Group.New(size: new Vector2(GUI.RmX - 120, GUI.RmY), padding: new(8, 8)))
							{
								GUI.TitleCentered($"{info.health.GetIntegrity():0.00}/{info.health.max:0.00}", size: 16, pivot: new(0.50f, 1.00f));
							}

							GUI.SameLine();

							var active = !this.flags.HasAny(Repair.Flags.Active);
							if (GUI.DrawButton("Repair", size: GUI.Rm, enabled: info.IsAlive, color: active ? GUI.col_button_error : GUI.col_button_error.WithColorMult(0.50f)))
							{
								rpc.active = active;
								dirty = true;
							}
						}

						//GUI.TitleCentered("Repair", size: 24, pivot: new Vector2(0.50f, 0.50f));




						//using (GUI.Group.New(size: new(GUI.RmX, 28), padding: new(4, 2)))
						//{
						//	GUI.TitleCentered("Repair", size: 24, pivot: new Vector2(0.00f, 0.50f));
						//}

						//GUI.SeparatorThick();

						//using (GUI.Group.New(size: GUI.Rm, padding: new(4, 6)))
						//{
						//	using (GUI.Wrap.Push(GUI.RmX))
						//	{
						//		//GUI.TextShaded(recipe.desc, color: GUI.font_color_desc);

						//		//GUI.NewLine();

						//		//if (recipe.products[0].type == Crafting.Product.Type.Prefab && recipe.products[0].prefab.TryGetPrefab(out var prefab))
						//		//{
						//		//	var root = prefab.Root;
						//		//	if (root != null)
						//		//	{
						//		//		GUI.DrawStats(root, priority_min: Statistics.Priority.Low);
						//		//	}
						//		//}
						//	}
						//}

						if (dirty)
						{
							rpc.Send(ent_wrench);
						}
					}

					public void DrawHUD(Entity ent_wrench, ref TargetInfo info_target)
					{
						if (info_target.IsAlive)
						{
							ref var transform_wrench = ref ent_wrench.GetComponent<Transform.Data>();
							if (transform_wrench.IsNotNull())
							{
								var target_pos = info_target.Position;
								ref var target_body = ref info_target.entity.GetComponent<Body.Data>();
								if (target_body.IsNotNull())
								{
									target_pos = target_body.GetClosestPoint(transform_wrench.GetInterpolatedPosition(), true).world_position;
								}
								else
								{
									ref var target_transform = ref info_target.transform;
									if (target_transform.IsNotNull())
									{
										target_pos = target_transform.GetInterpolatedPosition();
									}
								}

								//var text = "Dismantling...";

								var dist_sq = Vector2.DistanceSquared(transform_wrench.GetInterpolatedPosition(), target_pos);
								var color = dist_sq <= (2.00f * 2.00f) ? Color32BGRA.Green : Color32BGRA.Red;
								var target_pos_c = target_pos.WorldToCanvas();

								GUI.DrawLine(transform_wrench.GetInterpolatedPosition().WorldToCanvas(), target_pos_c, color.WithAlphaMult(0.25f), 4);
								GUI.DrawCircleFilled(target_pos_c, 0.20f * GUI.GetWorldToCanvasScale(), color.WithAlphaMult(0.50f));
								//GUI.DrawTextCentered(text, target_pos_c + new Vector2(0, 20), font: GUI.Font.Superstar, size: 16, color: GUI.font_color_title);
							}
						}

						//using (var hud = GUI.Window.Standalone("Wrench.HUD", position: info_target.pos.WorldToCanvas(), size: new(168, 200), pivot: new(0.50f, 0.50f), force_position: false))
						//{
						//	if (hud.show)
						//	{
						//		GUI.DrawBackground(GUI.tex_panel, hud.group.GetOuterRect(), padding: new(4));

						//		//using (GUI.Group.New(size: GUI.Rm - new Vector2(0, 48), padding: new(4)))
						//		//{

						//		//}

						//		using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY - 40), padding: new(8, 8)))
						//		{
						//			GUI.Title("Produces:");
						//			GUI.SeparatorThick();

						//			var items_span = info_target.health.items.AsSpan();

						//			for (var i = 0; i < items_span.Length; i++)
						//			{
						//				ref var item = ref items_span[i];
						//				switch (item.type)
						//				{
						//					case Shipment.Item.Type.Resource:
						//					{
						//						var resource = new Resource.Data(item.material, item.quantity);
						//						GUI.DrawResource(ref resource, new Vector2(GUI.RmX, 32), GUI.font_color_green);
						//					}
						//					break;
						//				}
						//			}

						//			GUI.TitleCentered($"{info_target.health.current_work:0.00}/{info_target.health.required_work:0.00}", pivot: new(0.50f, 1.00f));
						//		}

						//		using (GUI.Group.Centered(outer_size: GUI.Rm, inner_size: new(100, 40)))
						//		{
						//			var active = !this.flags.HasAny(Repair.Flags.Active);
						//			if (GUI.DrawButton("Dismantle", size: GUI.Rm, color: active ? GUI.col_button_error : GUI.col_button_error.WithColorMult(0.50f)))
						//			{
						//				var rpc = new ConfigureRPC
						//				{
						//					active = active
						//				};
						//				rpc.Send(ent_wrench);
						//			}
						//		}
						//	}
						//}

					}
#endif
				}

				public struct SetTargetRPC: Net.IRPC<Wrench.Mode.Repair.Data>
				{
					public Entity ent_target;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Repair.Data data)
					{
						ref var region = ref entity.GetRegion();

						data.flags.SetFlag(Flags.Active, false);
						data.ref_repairable.Set(this.ent_target);
						data.ref_health.Set(this.ent_target);
						data.Sync(entity);
					}
#endif
				}

				public struct ConfigureRPC: Net.IRPC<Wrench.Mode.Repair.Data>
				{
					public Repair.Filter? filter;
					public bool? active;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Repair.Data data)
					{
						ref var region = ref entity.GetRegion();

						var sync = false;

						if (this.filter.TryGetValue(out var v_filter))
						{
							data.filter = v_filter;
							sync = true;
						}

						if (this.active.TryGetValue(out var v_active) && data.flags.TrySetFlag(Repair.Flags.Active, v_active))
						{
							sync = true;
						}

						if (sync)
						{
							data.Sync(entity);
						}
					}
#endif
				}

#if SERVER
				[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.60f), HasTag("dead", false, Source.Modifier.Parent), HasRelation(Source.Modifier.Owned, Relation.Type.Stored, false)]
				public static void Update(ref Region.Data region, ISystem.Info info, Entity entity, ref XorRandom random, 
				[Source.Owned] in Transform.Data transform, [Source.Parent] in Interactor.Data interactor,
				[Source.Owned] ref Wrench.Data wrench, [Source.Owned] ref Wrench.Mode.Repair.Data repair)
				{
					if (repair.flags.HasAny(Repair.Flags.Active) && repair.ref_repairable.TryGetHandle(out var h_repairable) && repair.ref_health.TryGetHandle(out var h_health))
					{
						var target_pos = transform.position;
						ref var target_body = ref repair.ref_repairable.entity.GetComponent<Body.Data>();
						if (target_body.IsNotNull())
						{
							target_pos = target_body.GetClosestPoint(transform.position, true).world_position;
						}
						else
						{
							ref var target_transform = ref repair.ref_repairable.entity.GetComponent<Transform.Data>();
							if (target_transform.IsNotNull())
							{
								target_pos = target_transform.position;
							}
						}

						//ref var target_transform = ref repair.ref_repairable.entity.GetComponent<Transform.Data>();
						//if (target_transform.IsNotNull())
						{
							var dir = (target_pos - transform.position).GetNormalized(out var dist);
							if (dist <= 2.00f && region.IsInLineOfSight(transform.position, target_pos, 0.125f, mask: Physics.Layer.World, exclude: Physics.Layer.Essence))
							{
								//h_repairable.data.current_work += 1.00f * info.DeltaTime;
								if (false) //h_repairable.data.current_work >= h_repairable.data.required_work)
								{

									//repair.ref_repairable.Reset();
									//repair.flags.SetFlag(Repair.Flags.Active, false);
									//repair.Sync(entity);
								}
								else
								{
									Sound.Play(ref region, repair.sound_repair, transform.position, volume: random.NextFloatRange(0.70f, 0.80f), pitch: random.NextFloatRange(0.95f, 1.05f));

									//h_repairable.Sync();
								}
							}
						}
					}
				}
#endif

				public struct TargetInfo: ITargetInfo
				{
					public Entity entity;

					public Repairable.Data repairable;
					public Health.Data health;
					public Transform.Data transform;

					public float radius;
					public Vector2 pos;

					public bool alive;
					public bool valid;

					public Entity Entity => this.entity;
					public IComponent.Handle ComponentID => ECS.GetID<Repairable.Data>();
					public Vector2 Position => this.pos;
					public float Radius => this.radius;
					public bool IsSource => true;
					public bool IsAlive => this.alive;
					public bool IsValid => this.valid;

					public TargetInfo(Entity entity)
					{
						this.entity = entity;
						this.alive = this.entity.IsAlive();

						if (this.alive)
						{
							this.valid = true;

							this.valid &= this.entity.GetComponent<Repairable.Data>().TryGetRefValue(out this.repairable);
							this.valid &= this.entity.GetComponent<Health.Data>().TryGetRefValue(out this.health);
							this.valid &= this.entity.GetComponent<Transform.Data>().TryGetRefValue(out this.transform);

							if (this.valid)
							{
								this.radius = 1.00f;
								this.pos = this.transform.position;
							}
						}
					}
				}
			}
		}
	}
}
