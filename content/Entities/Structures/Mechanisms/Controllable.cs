namespace TC2.Base.Components
{
	public static partial class Controllable
	{
		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region | IComponent.Scope.Global)]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0u,

				No_GUI = 1u << 0,
				Show_Misc = 1u << 1,

				Disable_Mouse_Properties = 1u << 3,
				Disable_Mouse_LMB = 1u << 4,
				Disable_Mouse_RMB = 1u << 5,
				Disable_Mouse_Pos = 1u << 6,
				Disable_Keyboard = 1u << 7,

				Hide_Cursor = 1u << 8,

				Mouse_Orthogonal = 1u << 15,
			}

			[Asset.Ignore] public Vector2 target_pos;
			public Vec2f gui_size = new Vec2f(48 * 3, 0);

			public float speed = 5.00f;
			public float speed_max = 15.00f;

			public float radius = 16.00f;
			public float radius_max = 32.00f;

			public Controllable.Data.Flags flags;

			[Net.Ignore, Save.Ignore] public float t_last_sync;
			[Net.Ignore, Save.Ignore] public float t_last_flip;
			[Net.Ignore, Save.Ignore] private float unused_00;
		}

		//#if SERVER
		//		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//		public static void UpdateJoint(ISystem.Info info, Entity entity, Entity ent_joint_base, 
		//		[Source.Shared] in Control.Data control, [Source.Shared] in Controllable.Data controllable, [Source.Shared] in Transform.Data transform,
		//		[Source.Owned] ref Joint.Base joint_base)
		//		{

		//		}
		//#endif

#if SERVER
		// ensures target position isn't [0, 0] when spawned
		[ISystem.AddFirst(ISystem.Mode.Single, ISystem.Scope.Region, order: 1000)]
		public static void OnAdd(ISystem.Info info, Entity entity,
		[Source.Owned] ref Control.Data control, [Source.Owned] ref Controllable.Data controllable, [Source.Owned] in Transform.Data transform)
		{
			App.WriteLine($"{controllable.target_pos}; {transform.position}");
			if (controllable.target_pos == Vector2.Zero) controllable.target_pos = transform.LocalToWorld(new Vector2(0, -3));
		}
#endif

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateControls(ISystem.Info info, Entity entity,
		[Source.Owned] ref Control.Data control, [Source.Owned] ref Controllable.Data controllable, [Source.Owned] in Transform.Data transform)
		{
			if (control.mouse.position != controllable.target_pos)
			{
				controllable.target_pos = controllable.target_pos.ClampRadius(transform.position, controllable.radius);

				if (controllable.flags.HasAny(Controllable.Data.Flags.Mouse_Orthogonal))
				{
					control.mouse.position = Maths.MoveTowards(control.mouse.position, controllable.target_pos, new Vector2(controllable.speed * info.DeltaTime));
				}
				else
				{
					control.mouse.position = Maths.MoveTowards(control.mouse.position, controllable.target_pos, controllable.speed * info.DeltaTime);
				}
			}
		}

		//[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateControls(ISystem.Info info, Entity entity, Entity ent_joint_base,
		//[Source.Shared] ref Control.Data control, [Source.Shared] ref Controllable.Data controllable, [Source.Shared] in Transform.Data transform,
		//[Source.Owned] ref Joint.Base joint_base)
		//{
		//	if (controllable.flags.HasAny(Controllable.Data.Flags.Mouse_Orthogonal))
		//	{
		//		control.mouse.position = Maths.MoveTowards(control.mouse.position, controllable.target_pos, new Vector2(controllable.speed * info.DeltaTime));
		//	}
		//	else
		//	{
		//		control.mouse.position = Maths.MoveTowards(control.mouse.position, controllable.target_pos, controllable.speed * info.DeltaTime);
		//	}
		//}

		public struct ConfigureRPC: Net.IRPC<Controllable.Data>
		{
			public float? radius;
			public float? speed;

			public Vector2? mouse_position;
			public Keyboard.Key? keyboard;
			public Mouse.Key? mouse;
			public Controllable.Data.Flags? flags;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Controllable.Data data)
			{
				ref var control = ref rpc.entity.GetComponent<Control.Data>();
				if (control.IsNotNull())
				{
					var sync = false;

					sync |= this.radius.TryCopyToClamped(ref data.radius, 0.00f, data.radius_max);
					sync |= this.speed.TryCopyToClamped(ref data.speed, 0.00f, data.speed_max);

					sync |= this.mouse_position.TryCopyTo(ref data.target_pos);
					sync |= this.keyboard.TryCopyTo(ref control.keyboard.GetMask());
					sync |= this.mouse.TryCopyTo(ref control.mouse.GetMask());
					sync |= this.flags.TryCopyTo(ref data.flags, Data.Flags.Mouse_Orthogonal);

					if (sync)
					{
						data.Sync(rpc.entity, true);
						control.Sync(rpc.entity, true);
					}
				}
			}
#endif
		}

#if CLIENT
		public struct ControllableGUI: IGUICommand
		{
			public Entity ent_controllable;

			public Transform.Data transform;

			public Control.Data control;
			public Controllable.Data controllable;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Control"u8, entity: this.ent_controllable, size: this.controllable.gui_size, no_mouse_close: true, show_misc: this.controllable.flags.HasAny(Data.Flags.Show_Misc)))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						ref var player = ref Client.GetPlayer();
						ref var region = ref this.ent_controllable.GetRegion();

						using (var group = GUI.Group.New(size: GUI.Rm)) // new Vector2(GUI.RmX, GUI.RmY)))
						{
							//using (var group = GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY), padding: new Vector2(8, 8)))
							{
								//GUI.DrawBackground(GUI.tex_frame, group.GetOuterRect(), new(8));

								var dirty = false;

								var w_pos_mouse = this.control.mouse.position;
								var w_pos_target = this.controllable.target_pos;

								w_pos_target = w_pos_target.ClampRadius(this.transform.position, this.controllable.radius);

								var c_pos_mouse = region.WorldToCanvas(w_pos_mouse);
								var c_pos_target = region.WorldToCanvas(w_pos_target);

								var rpc = new Controllable.ConfigureRPC
								{

								};

								//if (GUI.SliderFloat("Ratio", ref this.controllable_state.slider_ratio, 0.00f, 1.00f, size: new Vector2(GUI.RmX, 32)))
								//{
								//	dirty = true;
								//}

								//if (GUI.Checkbox("Reverse Direction", ref this.controllable.flags, Controllable.Data.Flags.Invert, size: new Vector2(GUI.RmX, 32)))
								//{
								//	dirty = true;
								//}
								//GUI.DrawHoverTooltip("Reverse direction of the controllable.");

								var picker_size = new Vector2(64, 64);

								if (!this.controllable.flags.HasAll(Data.Flags.Disable_Mouse_Pos | Data.Flags.Disable_Mouse_LMB | Data.Flags.Disable_Mouse_RMB))
								{
									using (var group_row = GUI.Group.New(size: new Vector2(GUI.RmX, 40)))
									{
										var sameline = false;

										if (this.controllable.flags.HasNone(Data.Flags.Disable_Mouse_LMB))
										{
											//new Vector2((GUI.RmX * 0.50f) - (picker_size.X * 0.50f), 48)
											var button_size = GUI.Rm.ToVec2f();
											if (this.controllable.flags.HasNone(Data.Flags.Disable_Mouse_RMB)) button_size.x *= 0.50f;
											if (this.controllable.flags.HasNone(Data.Flags.Disable_Mouse_Pos)) button_size.x -= picker_size.X;

											if (GUI.DrawButton("LMB"u8, size: button_size, color: GUI.col_button_yellow, keys: GUI.ButtonKeys.Left | GUI.ButtonKeys.Right))
											{
												rpc.mouse.GetRefOrDefault().AddFlag(Mouse.Key.Left);
												dirty = true;
											}

											sameline = true;
										}


										if (this.controllable.flags.HasNone(Data.Flags.Disable_Mouse_Pos))
										{
											if (sameline.TryReset()) GUI.SameLine();

											var w_pos_target_tmp = w_pos_target;
											if (GUI.Picker("mouse_pos"u8, "Position"u8, picker_size, ref w_pos_target_tmp, new Vector2(-4000, -4000), new Vector2(4000, 4000), sensitivity: 0.10f, absolute: true))
											{
												rpc.mouse_position = w_pos_target_tmp.SnapFloor(0.125f);
												dirty = true;
											}

											sameline = true;
										}

										if (this.controllable.flags.HasNone(Data.Flags.Disable_Mouse_RMB))
										{
											if (sameline.TryReset()) GUI.SameLine();

											if (GUI.DrawButton("RMB"u8, size: new Vector2(GUI.RmX, 40), color: GUI.col_button_yellow, keys: GUI.ButtonKeys.Left | GUI.ButtonKeys.Right))
											{
												rpc.mouse.GetRefOrDefault().AddFlag(Mouse.Key.Right);
												dirty = true;
											}
										}
									}

									//GUI.SeparatorThick();
								}

								if (this.controllable.flags.HasNone(Data.Flags.Disable_Mouse_Properties))
								{
									using (var group_row = GUI.Group.New(size: new Vector2(GUI.RmX, 48)))
									{
										if (GUI.Checkbox("Orthogonal"u8, this.controllable.flags, ref rpc.flags, Controllable.Data.Flags.Mouse_Orthogonal, size: new(0, 24)))
										{
											dirty = true;
										}

										if (GUI.SliderFloat("Radius"u8, in this.controllable.radius, ref rpc.radius, 0.00f, this.controllable.radius_max, size: new(GUI.RmX, 24), snap: 0.500f))
										{
											rpc.mouse_position = w_pos_target.ClampRadius(this.transform.position, rpc.radius.Value);
											dirty = true;
										}

										if (GUI.SliderFloat("Speed"u8, in this.controllable.speed, ref rpc.speed, 0.00f, this.controllable.speed_max, size: new(GUI.RmX, 24), snap: 0.100f))
										{
											dirty = true;
										}
									}
								}

								//if 

								if (dirty)
								{
									rpc.Send(this.ent_controllable);
								}

								if (!this.controllable.flags.HasAll(Data.Flags.Hide_Cursor | Data.Flags.Disable_Mouse_Pos))
								{
									GUI.DrawCircle(region.WorldToCanvas(this.transform.GetInterpolatedPosition()), this.controllable.radius * region.GetWorldToCanvasScale(), thickness: 2.00f, segments: 64, color: Color32BGRA.Orange.WithAlpha(50));

									var delta = w_pos_mouse - w_pos_target;
									if (this.controllable.flags.HasAny(Data.Flags.Mouse_Orthogonal))
									{
										var dir = delta.GetNormalized(out var dist);
										GUI.DrawLine(c_pos_mouse, c_pos_target, color: GUI.col_button_yellow.WithAlpha(100), thickness: 4.00f);
									}
									else
									{
										var dir = delta.GetNormalized(out var dist);
										GUI.DrawLine(c_pos_mouse, c_pos_target, color: GUI.col_button_yellow.WithAlpha(100), thickness: 4.00f);
									}

									//GUI.DrawLine()
									GUI.DrawRectFilled(region.WorldToCanvas(AABB.Centered(this.controllable.target_pos, new Vector2(0.250f))), Color32BGRA.Orange);
									GUI.DrawRectFilled(region.WorldToCanvas(AABB.Centered(this.control.mouse.position, new Vector2(0.125f))), Color32BGRA.Green);
								}
							}
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Control.Data control, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Controllable.Data controllable, [Source.Owned] in Interactable.Data interactable)
		{
			if (controllable.flags.HasNone(Data.Flags.No_GUI) && interactable.IsActive())
			{
				var gui = new ControllableGUI()
				{
					ent_controllable = entity,

					transform = transform,

					control = control,
					controllable = controllable,
				};
				gui.Submit();
			}
		}
#endif
	}
}
