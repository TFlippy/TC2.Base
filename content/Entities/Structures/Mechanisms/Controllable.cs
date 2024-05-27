namespace TC2.Base.Components
{
	public static partial class Controllable
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0u,

				Mouse_Orthogonal = 1u << 0
			}

			[Asset.Ignore] public Vector2 target_pos;
			public Controllable.Data.Flags flags;

			[Net.Ignore, Save.Ignore] public float t_last_sync;
			[Net.Ignore, Save.Ignore] public float t_last_flip;

			public Data()
			{

			}
		}

//#if SERVER
//		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
//		public static void UpdateJoint(ISystem.Info info, Entity entity, Entity ent_joint_base, 
//		[Source.Shared] in Control.Data control, [Source.Shared] in Controllable.Data controllable, [Source.Shared] in Transform.Data transform,
//		[Source.Owned] ref Joint.Base joint_base)
//		{
			
//		}
//#endif

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateControls(ISystem.Info info, Entity entity, Entity ent_joint_base,
		[Source.Shared] ref Control.Data control, [Source.Shared] in Controllable.Data controllable, [Source.Shared] in Transform.Data transform,
		[Source.Owned] ref Joint.Base joint_base)
		{
			if (controllable.flags.HasAny(Controllable.Data.Flags.Mouse_Orthogonal))
			{
				control.mouse.position = Maths.MoveTowards(control.mouse.position, controllable.target_pos, new Vector2(0.125f * 8));
			}
			else
			{

			}
		}

		public struct ConfigureRPC: Net.IRPC<Controllable.Data>
		{
			public Vector2? mouse_position;
			public Keyboard.Key? keyboard;
			public Mouse.Key? mouse;
			public Controllable.Data.Flags? flags;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Controllable.Data data)
			{
				ref var control = ref entity.GetComponent<Control.Data>();
				if (control.IsNotNull())
				{
					var sync = false;

					sync |= this.mouse_position.TryCopyTo(ref data.target_pos);
					sync |= this.keyboard.TryCopyTo(ref control.keyboard.GetMask());
					sync |= this.mouse.TryCopyTo(ref control.mouse.GetMask());

					if (sync)
					{
						data.Sync(entity, true);
						control.Sync(entity, true);
					}
				}
			}
#endif
		}

#if CLIENT
		public struct ControllableGUI: IGUICommand
		{
			public Entity ent_controllable;

			public Control.Data control;
			public Controllable.Data controllable;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Control"u8, this.ent_controllable, no_mouse_close: true))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						ref var player = ref Client.GetPlayer();
						ref var region = ref Client.GetRegion();

						using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
						{
							using (var group = GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY), padding: new Vector2(8, 8)))
							{
								GUI.DrawBackground(GUI.tex_frame, group.GetOuterRect(), new(8));

								var dirty = false;

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

								if (GUI.DrawButton("LMB", size: new Vector2((GUI.RmX * 0.50f) - (picker_size.X * 0.50f), 40), color: GUI.col_button_yellow))
								{
									rpc.mouse.GetRefOrDefault().SetFlag(Mouse.Key.Left, true);
									dirty = true;
								}

								GUI.SameLine();

								var mouse_pos_tmp = this.control.mouse.position;
								if (GUI.Picker("mouse_pos", "Position", picker_size, ref mouse_pos_tmp, new Vector2(-4000, -4000), new Vector2(4000, 4000), sensitivity: 0.10f, absolute: true))
								{
									rpc.mouse_position = mouse_pos_tmp.SnapFloor(0.125f);
									dirty = true;
								}

								GUI.SameLine();

								if (GUI.DrawButton("RMB", size: new Vector2(GUI.RmX, 40), color: GUI.col_button_yellow))
								{
									rpc.mouse.GetRefOrDefault().SetFlag(Mouse.Key.Right, true);
									dirty = true;
								}


								//if 

								if (dirty)
								{
									rpc.Send(this.ent_controllable);
								}

								//GUI.DrawLine()
								GUI.DrawRectFilled(region.WorldToCanvas(AABB.Centered(this.controllable.target_pos.SnapFloor(0.125f), new Vector2(0.250f))), Color32BGRA.Orange);
								GUI.DrawRectFilled(region.WorldToCanvas(AABB.Centered(this.control.mouse.position.SnapFloor(0.125f), new Vector2(0.250f))), Color32BGRA.Orange);
							}
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity, [Source.Owned] in Control.Data control, [Source.Owned] in Controllable.Data controllable, [Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.IsActive())
			{
				var gui = new ControllableGUI()
				{
					ent_controllable = entity,

					control = control,
					controllable = controllable,
				};
				gui.Submit();
			}
		}
#endif
	}
}
