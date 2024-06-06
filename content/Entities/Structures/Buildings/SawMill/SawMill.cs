namespace TC2.Base.Components
{
	public static partial class SawMill
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true), IComponent.With<SawMill.State>]
		public struct Data: IComponent
		{
			public float slider_distance = 5.00f;
			public Vector2 saw_offset;
			public float saw_radius = 1.00f;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public struct State: IComponent
		{
			public float gear_ratio = 1.00f;
			public float slider_ratio;

			[Net.Ignore, Save.Ignore] public float next_update;
			[Net.Ignore, Save.Ignore] public float last_hit;

			public State()
			{

			}
		}

		public struct ConfigureRPC: Net.IRPC<SawMill.State>
		{
			public float gear_ratio;
			public float slider_ratio;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref SawMill.State data)
			{
				data.gear_ratio = this.gear_ratio;
				data.slider_ratio = this.slider_ratio;

				ref var body = ref entity.GetComponent<Body.Data>();
				if (body.IsNotNull())
				{
					body.Activate();
				}

				data.Sync(entity, true);
			}
#endif
		}

		//public const float update_interval = 0.12f;

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSlider(ISystem.Info info, Entity entity,
		[Source.Shared] in SawMill.Data sawmill, [Source.Shared] ref SawMill.State sawmill_state,
		[Source.Owned, Original] ref Joint.Distance joint_distance)
		{
			joint_distance.distance = sawmill.slider_distance * sawmill_state.slider_ratio;
		}

		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.12f)]
		public static void UpdateDamage(ISystem.Info info, Entity entity, Entity ent_health,
		[Source.Parent] ref Axle.Data axle, [Source.Parent] ref Axle.State axle_state, [Source.Parent] in SawMill.Data sawmill, [Source.Parent] ref SawMill.State sawmill_state, [Source.Parent] in Transform.Data transform_parent,
		[Source.Owned] ref Health.Data health, [Source.Owned] in Body.Data body_child)
		{
			var axle_speed = Maths.Max(MathF.Abs(axle_state.angular_velocity) - 2.00f, 0.00f);
			if (axle_speed > 2.00f)
			{
				var wpos_saw = transform_parent.LocalToWorld(sawmill.saw_offset);

				var overlap = body_child.GetClosestPoint(wpos_saw);
				var dir = (-transform_parent.GetDirection()).RotateByDeg(30.00f);

				//App.WriteLine()

				if (overlap.distance < sawmill.saw_radius)
				{
#if SERVER


					var damage = Maths.Clamp(axle_state.net_torque * 0.15f, 0.00f, axle_speed * 35.00f);
					//App.WriteLine(damage);

					if (damage > 25.00f)
					{
						//entity.Hit(entity, ent_health, overlap.world_position, dir, -dir, damage, overlap.material_type, Damage.Type.Saw, yield: 1.00f, speed: axle_speed);
						Damage.Hit(ent_attacker: entity, ent_owner: entity, ent_target: ent_health,
							position: overlap.world_position - overlap.gradient, velocity: dir * axle_speed, normal: -dir,
							damage_integrity: damage, damage_durability: damage, damage_terrain: damage,
							target_material_type: overlap.material_type, damage_type: Damage.Type.Saw,
							yield: 1.00f, size: 0.50f, impulse: 0.00f);
					}
#endif

					sawmill_state.last_hit = info.WorldTime;
				}
			}
		}

#if CLIENT
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateRenderer(ISystem.Info info, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] in SawMill.State sawmill_state,
		[Source.Owned] in Axle.Data axle, [Source.Owned] ref Axle.State axle_state, [Source.Owned, Pair.Of<SawMill.Data>] ref Animated.Renderer.Data renderer_saw)
		{
			renderer_saw.rotation = (renderer_saw.rotation - (axle_state.angular_velocity * info.DeltaTime * sawmill_state.gear_ratio)) % MathF.Tau;
			renderer_saw.offset = sawmill.saw_offset;
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSoundIdle(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] ref SawMill.State sawmill_state,
		[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state, [Source.Owned, Pair.Of<SawMill.Data>] ref Sound.Emitter sound_emitter)
		{
			var axle_speed = Maths.Max(MathF.Abs(axle_state.angular_velocity) - 2.00f, 0.00f);

			sound_emitter.volume_mult = Maths.Lerp2(sound_emitter.volume_mult, Maths.Clamp(axle_speed * 0.05f, 0.00f, 0.50f) * random.NextFloatRange(0.90f, 1.00f), 0.10f, 0.02f);
			sound_emitter.pitch_mult = Maths.Lerp2(sound_emitter.pitch_mult, 0.60f + (Maths.Clamp(axle_speed * 0.11f, 0.00f, 0.50f)) * random.NextFloatRange(0.80f, 1.00f), 0.02f, 0.01f);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSoundCutting(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] ref SawMill.State sawmill_state,
		[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state, [Source.Owned, Pair.Of<SawMill.State>] ref Sound.Emitter sound_emitter)
		{
			var axle_speed = Maths.Max(MathF.Abs(axle_state.angular_velocity) - 2.00f, 0.00f);
			var modifier = (info.WorldTime - sawmill_state.last_hit) < 0.25 ? Maths.Min(axle_speed * 0.08f, 1.00f) : 0.00f;

			sound_emitter.volume_mult = Maths.Lerp2(sound_emitter.volume_mult, modifier * 0.60f, 0.05f, 0.30f);
			sound_emitter.pitch_mult = Maths.Lerp2(sound_emitter.pitch_mult, 0.45f + (Maths.Min(modifier, 0.25f) * random.NextFloatRange(0.50f, 1.20f)), 0.02f, 0.08f);
		}
#endif

#if CLIENT
		public struct SawMillGUI: IGUICommand
		{
			public Entity ent_sawmill;

			public SawMill.Data sawmill;
			public SawMill.State sawmill_state;

			public Axle.Data axle;
			public Axle.State axle_state;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Sawmill"u8, this.ent_sawmill))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						ref var player = ref Client.GetPlayer();
						ref var region = ref Client.GetRegion();

						const float slider_h = 32;

						var frame_size = Inventory.GetFrameSize(4, 2);

						var context = GUI.ItemContext.Begin();
						{
							using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
							{
								using (var group = GUI.Group.New(size: new Vector2(GUI.RmX - frame_size.X - 32, GUI.RmY), padding: new Vector2(8, 8)))
								{
									GUI.DrawBackground(GUI.tex_frame, group.GetOuterRect(), new(8));

									using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY - slider_h - 64)))
									{
										GUI.LabelShaded("Angular Velocity:", this.axle_state.angular_velocity, "{0:0.00} rad/s");
										GUI.LabelShaded("Torque:", this.axle_state.net_torque, "{0:0.00} Nm/s");
									}

									var ent_child = default(Entity);
									var ent_joint = default(Entity);

									ent_joint = this.ent_sawmill.GetChild(Relation.Type.Instance);
									if (ent_joint.IsAlive())
									{
										ent_child = ent_joint.GetChild(Relation.Type.Child);
									}

									using (var slot = GUI.EntitySlot.New(ref context, "slot.sawmill", "Item", ent_child, new Vector2(GUI.RmX, 64)))
									{
										if (slot.pressed)
										{
											switch (slot.action)
											{
												case GUI.ItemContext.Action.Add:
												{
													var rpc = new Holdable.AttachRPC
													{
														ent_joint = ent_joint
													};
													rpc.Send(context.ent_pickup_target);
												}
												break;

												case GUI.ItemContext.Action.Remove:
												{
													var rpc = new Holdable.DetachRPC
													{
														
													};
													rpc.Send(ent_child);
												}
												break;

												case GUI.ItemContext.Action.Swap:
												{
													var rpc = new Holdable.AttachRPC
													{
														ent_joint = ent_joint
													};
													rpc.Send(context.ent_pickup_target);
												}
												break;
											}
										}
									}

									var dirty = false;
									if (GUI.SliderFloat("Slider", ref this.sawmill_state.slider_ratio, 1.00f, 0.00f, size: new Vector2(GUI.RmX, slider_h)))
									{
										dirty = true;
									}

									if (dirty)
									{
										var rpc = new SawMill.ConfigureRPC
										{
											gear_ratio = this.sawmill_state.gear_ratio,
											slider_ratio = this.sawmill_state.slider_ratio
										};
										rpc.Send(this.ent_sawmill);
									}
								}

								GUI.SameLine();

								using (var group = GUI.Group.Centered(GUI.Rm, frame_size))
								{
									GUI.DrawBackground(GUI.tex_frame, group.GetOuterRect(), new(8));

									GUI.DrawInventoryDock(Inventory.Type.Output, size: frame_size);
								}
							}
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity, [Source.Owned] in SawMill.Data sawmill, [Source.Owned] in SawMill.State sawmill_state, [Source.Owned] in Axle.Data axle, [Source.Owned] ref Axle.State axle_state, [Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.IsActive())
			{
				var gui = new SawMillGUI()
				{
					ent_sawmill = entity,

					sawmill = sawmill,
					sawmill_state = sawmill_state,

					axle = axle,
					axle_state = axle_state
				};
				gui.Submit();
			}
		}
#endif
	}
}
