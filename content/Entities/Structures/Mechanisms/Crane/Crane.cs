﻿namespace TC2.Base.Components
{
	public static partial class Crane
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Inverted = 1 << 0,
			//Hold = 1 << 1,
			Relative = 1 << 2,
			Maintain_Position = 1 << 3
		}

		[IComponent.Data(Net.SendType.Reliable, region_only: true), IComponent.With<Crane.State>]
		public partial struct Data(): IComponent
		{
			public float length_a = 8.00f;
			public float length_b = 8.00f;

			public Crane.Flags flags = Crane.Flags.None;
			public Crane.Flags flags_editable = Crane.Flags.None;
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State(): IComponent
		{
			[Asset.Ignore] public float angle_a;
			[Asset.Ignore] public float angle_b;

			[Asset.Ignore] public float rotation_a;
			[Asset.Ignore] public float rotation_b;

			[Asset.Ignore] public Vector2 pos_target;

			[Save.Ignore, Net.Ignore] public float last_rotation;
			[Save.Ignore, Net.Ignore] public float next_sync;
		}

		public partial struct ConfigureRPC: Net.IRPC<Crane.Data>
		{
			public float length_a;
			public float length_b;
			public Crane.Flags flags;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Crane.Data crane)
			{
				crane.length_a = this.length_a;
				crane.length_b = this.length_b;
				crane.flags = (crane.flags & ~crane.flags_editable) | (this.flags & crane.flags_editable);
				crane.Sync(rpc.entity, true);
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
				using (var window = GUI.Window.Interaction("Crane"u8, this.ent_crane, no_mouse_close: true))
				{
					this.StoreCurrentWindowTypeID(-200);
					if (window.show)
					{
						var dirty = false;
						ref var region = ref this.ent_crane.GetRegion();

						var a = this.transform.LocalToWorldInterpolated(new Vector2(-this.crane.length_a * 0.50f, 0.00f));
						//var b = this.transform.LocalToWorldInterpolated(new Vector2(this.crane.length_a * 0.50f, 0.00f));
						var b = a + new Vector2(this.crane.length_a, 0).RotateByRad(this.crane_state.angle_a);
						var c = b + new Vector2(this.crane.length_b, 0).RotateByRad(this.crane_state.angle_a + this.crane_state.angle_b);

						var color = GUI.font_color_yellow;

						var scale = region.GetWorldToCanvasScale();

						var cpos_a = region.WorldToCanvas(a);
						var cpos_b = region.WorldToCanvas(b);
						var cpos_c = region.WorldToCanvas(c);

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

						//if (this.crane.flags_editable.HasAny(Crane.Flags.Hold) && GUI.Checkbox("Hold", ref flags, Crane.Flags.Hold, new Vector2(96, 24)))
						//{
						//	dirty = true;
						//	edit_rpc.flags = flags;
						//}

						if (this.crane.flags_editable.HasAny(Crane.Flags.Inverted) && GUI.Checkbox("Invert", ref flags, Crane.Flags.Inverted, new Vector2(96, 24)))
						{
							dirty = true;
							edit_rpc.flags = flags;
						}

						if (this.crane.flags_editable.HasAny(Crane.Flags.Relative) && GUI.Checkbox("Relative", ref flags, Crane.Flags.Relative, new Vector2(96, 24)))
						{
							dirty = true;
							edit_rpc.flags = flags;
						}

						if (this.crane.flags_editable.HasAny(Crane.Flags.Maintain_Position) && GUI.Checkbox("Maintain Position", ref flags, Crane.Flags.Maintain_Position, new Vector2(96, 24)))
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
			if (interactable.IsActive())
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

		[Shitcode]
		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateA(ISystem.Info info, Entity entity, [Source.Owned] in Control.Data control,
		[Source.Owned] in Transform.Data transform, [Source.Parent] in Transform.Data transform_parent,
		[Source.Parent] ref Body.Data body_parent, [Source.Owned] ref Body.Data body, [Source.Parent] in Attachment.Slot attachment_slot,
		[Source.Parent] ref Joint.Base joint_base_parent, [Source.Parent, Override] ref Joint.Gear gear_parent, [Source.Owned] ref Crane.Data crane, [Source.Owned] ref Crane.State crane_state)
		{
			if (joint_base_parent.flags.HasNone(Joint.Flags.No_Aiming))
			{
				var invert = float.IsNegative(transform.scale.GetParity()); // != crane.flags.HasAny(Crane.Flags.Inverted);
				var is_relative = crane.flags.HasAny(Crane.Flags.Relative);
				var dirty = control.mouse.GetKeyDown(Mouse.Key.Right) || control.mouse.GetKeyUp(Mouse.Key.Right);

				//var transform_tmp = transform;
				//var transform_parent_tmp = transform_parent;

				var pos_new = control.mouse.position;

//				if (true)
//				{
//					ref var region = ref info.GetRegion();

//					var a = transform_tmp.LocalToWorld(new Vector2(-crane.length_a * 0.50f, 0.00f));
//					var b = a + new Vector2(crane.length_a, 0).RotateByRad(crane_state.angle_a);
//					var c = b + new Vector2(crane.length_b, 0).RotateByRad(crane_state.angle_a + crane_state.angle_b);
//					var d = c; // region.GetNearestPathPos(c, Terrain.PathFlags.Ground);
//					var e = d;

//					//if (region.TryOverlapPoint(d, d + new Vector2(0.00f, 2), 1.00f, out var result, mask: Physics.Layer.Solid | Physics.Layer.World, exclude: Physics.Layer.Dynamic))
//					if (region.TryOverlapPoint(d, 4.00f, out var result, mask: Physics.Layer.Solid | Physics.Layer.World, exclude: Physics.Layer.Dynamic))
//					{
//						e = result.world_position;
						
//						if (!dirty)
//						{
//							crane_state.pos_target = is_relative ? transform_parent_tmp.WorldToLocal(e, scale: false, rotation: false) : e;
//							dirty = true;
//						}
//					}

//#if CLIENT
//					region.DrawDebugCircle(a, 0.25f, Color32BGRA.Yellow, 1, true);
//					region.DrawDebugCircle(b, 0.25f, Color32BGRA.Orange, 1, true);
//					region.DrawDebugCircle(c, 0.25f, Color32BGRA.Green, 1, true);
//					region.DrawDebugCircle(d, 0.25f, Color32BGRA.Magenta, 1, false);
//					region.DrawDebugCircle(e, 0.25f, Color32BGRA.Magenta, 1, true);
//#endif

//					//region.GetNearestPathPos()
//				}

				if (!attachment_slot.flags.HasAny(Attachment.Slot.Flags.Hold) || control.mouse.GetKey(Mouse.Key.Right))
				{
					crane_state.pos_target = is_relative ? transform_parent.WorldToLocal(pos_new, scale: false, rotation: false) : pos_new;
					dirty = true;
				}

				dirty |= crane.flags.HasAny(Crane.Flags.Maintain_Position);
				var pos = is_relative ? transform_parent.LocalToWorld(crane_state.pos_target, rotation: false, scale: false) : crane_state.pos_target;

				if (dirty)
				{
					IK.Resolve2x(new Vector2(crane.length_a, crane.length_b), transform.LocalToWorld(joint_base_parent.offset_b), pos, new(crane_state.angle_a, crane_state.angle_b), out var angles, invert: invert != crane.flags.HasAny(Crane.Flags.Inverted));
					crane_state.angle_a = angles.X;
					crane_state.angle_b = angles.Y;
					//dirty = true;
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

				//var parity = transform.scale.GetParity();

				crane_state.rotation_a = transform_parent.WorldToLocalRotation(crane_state.angle_a, scale: true, normalize: false); // transform_parent.WorldToLocalRotation(crane_state.angle_a, rotation: false, scale: true, normalize: false);
				//crane_state.rotation_b = transform.WorldToLocalRotation(crane_state.angle_b, rotation: false, scale: true, normalize: false);
				crane_state.rotation_b = transform.TransformAngle(crane_state.angle_b, flip: invert);

				//if (joint_base_parent.flags.HasAny(Joint.Flags.Invert_Facing))
				//{
				//	crane_state.rotation_b = Maths.OppositeAngle(crane_state.rotation_b);
				//}

				if (invert)
				{
					//gear_parent.rotation = -gear_parent.rotation;
					//crane_state.rotation_b = Maths.OppositeAngle(crane_state.rotation_b);
				}

				gear_parent.rotation = Maths.NormalizeAngle(crane_state.rotation_a - joint_base_parent.rotation_origin);
				//crane_state.rotation_b = crane_state.rotation_b;

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
			gear.rotation = Maths.NormalizeAngle(crane_state.rotation_b - joint_base.rotation_origin);
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
