
using System.Diagnostics.CodeAnalysis;

namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		public static partial class Mode
		{
			public static partial class Deconstruct
			{
				[Flags]
				public enum Flags: uint
				{
					None = 0,

					Active = 1 << 0,
				}

				[IComponent.Data(Net.SendType.Reliable, name: "Wrench (Demolish)")]
				public partial struct Data: IComponent, Wrench.IMode, Wrench.ITargeterMode<TargetInfo>
				{
					public static readonly Sound.Handle sound_dismantle_default = "wrench.dismantle.00";

					public EntRef<Dismantlable.Data> ref_dismantlable;
					public Deconstruct.Flags flags;

					public Sound.Handle sound_dismantle = sound_dismantle_default;

					[UnscopedRef]
					public ref Entity EntTarget => ref this.ref_dismantlable.entity;

					public TargetInfo CreateTargetInfo(Entity entity)
					{
						return new TargetInfo(entity);
					}

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 0, 1, 24, 24, 2, 0);
					public static string Name { get; } = "Demolish";

					public Crafting.Recipe.Tags RecipeTags => Crafting.Recipe.Tags.Duct;
					public Physics.Layer LayerMask => Physics.Layer.Duct;
					public Color32BGRA ColorOk => Color32BGRA.Red;
					public Color32BGRA ColorError => Color32BGRA.Grey;
					public Color32BGRA ColorNew => Color32BGRA.Yellow;

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

					}

					public void DrawHUD(Entity ent_wrench, ref TargetInfo info_target)
					{
						using (var hud = GUI.Window.Standalone("Wrench.HUD", position: info_target.pos.WorldToCanvas(), size: new(168, 200), pivot: new(0.50f, 0.50f), force_position: false))
						{
							if (hud.show)
							{
								GUI.DrawBackground(GUI.tex_panel, hud.group.GetOuterRect(), padding: new(4));

								//using (GUI.Group.New(size: GUI.GetRemainingSpace() - new Vector2(0, 48), padding: new(4)))
								//{

								//}

								using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight() - 40), padding: new(8, 8)))
								{
									GUI.Title("Produces:");
									GUI.SeparatorThick();

									var items_span = info_target.dismantlable.items.AsSpan();

									for (var i = 0; i < items_span.Length; i++)
									{
										ref var item = ref items_span[i];
										switch (item.type)
										{
											case Shipment.Item.Type.Resource:
											{
												var resource = new Resource.Data(item.material, item.quantity);
												GUI.DrawResource(ref resource, new Vector2(GUI.GetRemainingWidth(), 32), GUI.font_color_green);
											}
											break;
										}
									}

									GUI.TitleCentered($"{info_target.dismantlable.current_work:0.00}/{info_target.dismantlable.required_work:0.00}", pivot: new(0.50f, 1.00f));
								}

								using (GUI.Group.Centered(outer_size: GUI.GetRemainingSpace(), inner_size: new(100, 40)))
								{
									var active = !this.flags.HasAny(Deconstruct.Flags.Active);
									if (GUI.DrawButton("Dismantle", size: GUI.GetRemainingSpace(), color: active ? GUI.col_button_error : GUI.col_button_error.WithColorMult(0.50f)))
									{
										var rpc = new ConfigureRPC
										{
											active = active
										};
										rpc.Send(ent_wrench);
									}
								}
							}
						}
					}
#endif
				}

				public struct SetTargetRPC: Net.IRPC<Wrench.Mode.Deconstruct.Data>
				{
					public Entity ent_target;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Deconstruct.Data data)
					{
						ref var region = ref entity.GetRegion();

						data.EntTarget = this.ent_target;
						data.Sync(entity);
					}
#endif
				}

				public struct ConfigureRPC: Net.IRPC<Wrench.Mode.Deconstruct.Data>
				{
					public bool? active;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Deconstruct.Data data)
					{
						ref var region = ref entity.GetRegion();

						var sync = false;

						if (this.active.TryGetValue(out var v_active) && data.flags.TrySetFlag(Deconstruct.Flags.Active, v_active))
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
				[ISystem.Update(ISystem.Mode.Single, interval: 0.60f)]
				public static void Update(ISystem.Info info, Entity entity, ref XorRandom random, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Wrench.Data wrench, [Source.Owned] ref Wrench.Mode.Deconstruct.Data deconstruct)
				{
					ref var region = ref info.GetRegion();

					if (deconstruct.flags.HasAny(Deconstruct.Flags.Active) && deconstruct.ref_dismantlable.TryGetHandle(out var h_dismantlable))
					{
						ref var target_transform = ref deconstruct.ref_dismantlable.entity.GetComponent<Transform.Data>();
						if (target_transform.IsNotNull())
						{
							var dir = (target_transform.position - transform.position).GetNormalized(out var dist);
							if (dist <= 4.00f && region.IsInLineOfSight(transform.position, target_transform.position, 0.125f, mask: Physics.Layer.World, exclude: Physics.Layer.Essence))
							{
								h_dismantlable.data.current_work += 1.00f * info.DeltaTime;
								if (h_dismantlable.data.current_work >= h_dismantlable.data.required_work)
								{
									h_dismantlable.data.flags.SetFlag(Dismantlable.Flags.Active, true);
									h_dismantlable.data.yield = Maths.Clamp(h_dismantlable.data.yield, 0.00f, 1.00f);
									h_dismantlable.Sync();
									h_dismantlable.entity.Delete();

									deconstruct.ref_dismantlable.Reset();
									deconstruct.flags.SetFlag(Deconstruct.Flags.Active, false);
									deconstruct.Sync(entity);
								}
								else
								{
									Sound.Play(ref region, deconstruct.sound_dismantle, transform.position, volume: random.NextFloatRange(0.70f, 0.80f), pitch: random.NextFloatRange(0.95f, 1.05f));

									h_dismantlable.Sync();
								}
							}
						}
					}
				}
#endif

				public struct TargetInfo: ITargetInfo
				{
					public Entity entity;

					public Dismantlable.Data dismantlable;
					public Transform.Data transform;

					public float radius;
					public Vector2 pos;

					public bool alive;
					public bool valid;

					public Entity Entity => this.entity;
					public ulong ComponentID => ECS.GetID<Dismantlable.Data>();
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

							this.valid &= this.entity.GetComponent<Dismantlable.Data>().TryGetValue(out this.dismantlable);
							this.valid &= this.entity.GetComponent<Transform.Data>().TryGetValue(out this.transform);

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
