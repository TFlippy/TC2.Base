namespace TC2.Base.Components
{
	public static partial class Crane
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Inverted = 1 << 0,
			Hold = 1 << 1,
		}

		[IComponent.Data(Net.SendType.Reliable, region_only: true), IComponent.With<Crane.State>]
		public partial struct Data: IComponent
		{
			public float length_a = 8.00f;
			public float length_b = 8.00f;

			public Crane.Flags flags = Crane.Flags.None;
			public Crane.Flags flags_editable = Crane.Flags.None;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State: IComponent
		{
			public float angle_a;
			public float angle_b;

			public float rotation_b;

			[Save.Ignore, Net.Ignore] public float last_rotation;
			[Save.Ignore, Net.Ignore] public float next_sync;
		}

		public partial struct ConfigureRPC: Net.IRPC<Crane.Data>
		{
			public float length_a;
			public float length_b;
			public Crane.Flags flags;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Crane.Data crane)
			{
				crane.length_a = this.length_a;
				crane.length_b = this.length_b;
				crane.flags = (crane.flags & ~crane.flags_editable) | (this.flags & crane.flags_editable);
				crane.Sync(entity, true);
			}
#endif
		}

#if CLIENT
		public partial struct CraneGUI: IGUICommand
		{
			public Entity ent_crane;
			public Transform.Data transform;
			public Crane.Data crane;
			public Crane.State crane_state;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Crane", this.ent_crane, no_mouse_close: true))
				{
					if (window.show)
					{
						var dirty = false;

						var a = this.transform.LocalToWorldInterpolated(new Vector2(-this.crane.length_a * 0.50f, 0.00f));
						//var b = this.transform.LocalToWorldInterpolated(new Vector2(this.crane.length_a * 0.50f, 0.00f));
						var b = a + new Vector2(this.crane.length_a, 0).RotateByRad(this.crane_state.angle_a);
						var c = b + new Vector2(this.crane.length_b, 0).RotateByRad(this.crane_state.angle_a + this.crane_state.angle_b);

						var color = GUI.font_color_yellow;

						var scale = GUI.GetWorldToCanvasScale();

						var cpos_a = GUI.WorldToCanvas(a);
						var cpos_b = GUI.WorldToCanvas(b);
						var cpos_c = GUI.WorldToCanvas(c);

						GUI.DrawLine(cpos_a, cpos_b, color.WithAlphaMult(0.40f), scale * 0.20f);
						GUI.DrawLine(cpos_b, cpos_c, color.WithAlphaMult(0.40f), scale * 0.20f);

						GUI.DrawCircleFilled(cpos_a, scale * 0.25f, color.WithAlphaMult(0.75f), 16);
						GUI.DrawCircleFilled(cpos_b, scale * 0.25f, color.WithAlphaMult(0.75f), 16);
						GUI.DrawCircleFilled(cpos_c, scale * 0.25f, color.WithAlphaMult(0.75f), 16);

						dirty |= GUI.SliderFloat("A", ref this.crane.length_a, 0.00f, 16.00f);
						dirty |= GUI.SliderFloat("B", ref this.crane.length_b, 0.00f, 16.00f);

						var flags = this.crane.flags;

						var edit_rpc = new Crane.ConfigureRPC
						{
							length_a = this.crane.length_a,
							length_b = this.crane.length_b,
							flags = flags
						};

						if (this.crane.flags_editable.HasAny(Crane.Flags.Hold) && GUI.Checkbox("Hold", ref flags, Crane.Flags.Hold, new Vector2(96, 24)))
						{
							dirty = true;
							edit_rpc.flags = flags;
						}

						if (this.crane.flags_editable.HasAny(Crane.Flags.Inverted) && GUI.Checkbox("Invert", ref flags, Crane.Flags.Inverted, new Vector2(96, 24)))
						{
							dirty = true;
							edit_rpc.flags = flags;
						}

						if (dirty)
						{
							edit_rpc.Send(this.ent_crane);
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Crane.Data crane, [Source.Owned] in Crane.State crane_state,
		[Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
			{
				var gui = new CraneGUI()
				{
					ent_crane = entity,
					transform = transform,
					crane = crane,
					crane_state = crane_state
				};
				gui.Submit();
			}
		}
#endif

		//[ISystem.PreUpdate.Reset(ISystem.Mode.Single, ISystem.Scope.Region), Exclude<Joint.Base>(Source.Modifier.Parent)]
		//public static void UpdateControl(ISystem.Info info, Entity entity,
		//[Source.Owned] in Transform.Data transform,
		//[Source.Owned] ref Crane.Data crane, [Source.Owned] ref Control.Data control, [Source.Owned] ref Interactable.Data interactable)
		//{
		//	if (interactable.ref_interactor.TryGetHandle(out var handle))
		//	{
		//		control = handle.Value.control;
		//	}
		//}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateA(ISystem.Info info, Entity entity, [Source.Owned] in Control.Data control,
		[Source.Owned] in Transform.Data transform, [Source.Parent] in Transform.Data transform_parent,
		[Source.Parent] ref Body.Data body_parent, [Source.Owned] ref Body.Data body,
		[Source.Parent] ref Joint.Base joint_base_parent, [Source.Parent, Override] ref Joint.Gear gear_parent, [Source.Owned] ref Crane.Data crane, [Source.Owned] ref Crane.State crane_state)
		{
			if (!joint_base_parent.flags.HasAll(Joint.Flags.No_Aiming))
			{
				var dirty = control.mouse.GetKeyDown(Mouse.Key.Right) || control.mouse.GetKeyUp(Mouse.Key.Right);
				var invert = float.IsNegative(transform.scale.GetParity()); // != crane.flags.HasAny(Crane.Flags.Inverted);

				var transform_tmp = transform;
				var transform_parent_tmp = transform_parent;

				if (!crane.flags.HasAny(Crane.Flags.Hold) || control.mouse.GetKey(Mouse.Key.Right))
				{
					IK.Resolve2x(new Vector2(crane.length_a, crane.length_b), transform_tmp.LocalToWorld(joint_base_parent.offset_b), control.mouse.position, new(crane_state.angle_a, crane_state.angle_b), out var angles, invert: invert != crane.flags.HasAny(Crane.Flags.Inverted));
					crane_state.angle_a = angles.X;
					crane_state.angle_b = angles.Y;
					dirty = true;
					//if (info.WorldTime >= crane_state.next_sync)
					//{
					//	crane_state.next_sync = info.WorldTime + 0.20f;
					//	dirty = true;
					//}
				}

				//#if SERVER
				//				if (info.WorldTime >= crane_state.next_sync && dirty)
				//				{
				//					crane_state.last_rotation = crane_state.rotation_b;
				//					dirty = true;
				//				}
				//#endif

				var parity = transform_tmp.scale.GetParity();

				gear_parent.rotation = transform_parent_tmp.WorldToLocalRotation(crane_state.angle_a) * parity;
				crane_state.rotation_b = transform_parent_tmp.WorldToLocalRotation(crane_state.angle_b, rotation: false) * parity;

				if (joint_base_parent.flags.HasAny(Joint.Flags.Invert_Facing))
				{
					crane_state.rotation_b = Maths.OppositeAngle(crane_state.rotation_b);
				}

				if (invert)
				{
					gear_parent.rotation = -gear_parent.rotation;
					crane_state.rotation_b = Maths.OppositeAngle(crane_state.rotation_b);
				}

				gear_parent.rotation = Maths.NormalizeAngle(gear_parent.rotation);
				crane_state.rotation_b = Maths.NormalizeAngle(crane_state.rotation_b);

				if (info.WorldTime >= crane_state.next_sync && crane_state.last_rotation != crane_state.rotation_b)
				{
					body.Activate();
					crane_state.next_sync = info.WorldTime + 0.43f;
					crane_state.last_rotation = crane_state.rotation_b;

					//App.WriteLine("sync");


#if SERVER
					crane_state.Sync(entity, true);
#endif
				}
			}
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateB(ISystem.Info info, Entity entity, [Source.Owned] ref Joint.Base joint_base,
		[Source.Owned, Override] ref Joint.Gear gear, [Source.Shared] ref Crane.State crane_state)
		{
			gear.rotation = crane_state.rotation_b;
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSound(ISystem.Info info, Entity entity,
		[Source.Owned] in Crane.Data crane, [Source.Owned] ref Sound.Mixer sound_mix,
		[Source.Owned, Override] in Joint.Gear gear)
		{
			var max = 0.50f;
			sound_mix.modifier = Maths.Clamp(MathF.Abs(gear.GetCurrentSpeed()) - 0.10f, 0.00f, max) / max;
		}
	}
}
