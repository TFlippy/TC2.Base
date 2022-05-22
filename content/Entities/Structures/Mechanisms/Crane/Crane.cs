
namespace TC2.Base.Components
{
	public static partial class Crane
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Inverted = 1 << 0
		}

		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Crane.State>]
		public partial struct Data: IComponent
		{
			public float length_a = 8.00f;
			public float length_b = 8.00f;

			public Crane.Flags flags = Flags.None;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable | Net.SendType.NoDelay)]
		public partial struct State: IComponent
		{
			public float angle_a;
			public float angle_b;

			[Save.Ignore, Net.Ignore] public float next_sync;
		}

		public partial struct ConfigureRPC: Net.IRPC<Crane.Data>
		{
			public float length_a;
			public float length_b;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Crane.Data crane)
			{
				crane.length_a = this.length_a;
				crane.length_b = this.length_b;

				entity.SyncComponent(ref crane);
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
				using (var window = GUI.Window.Interaction("Crane", this.ent_crane))
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

						if (dirty)
						{
							var rpc = new Crane.ConfigureRPC
							{
								length_a = this.crane.length_a,
								length_b = this.crane.length_b,
							};
							rpc.Send(this.ent_crane);
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
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

		[ISystem.PreUpdate.Reset(ISystem.Mode.Single), Exclude<Joint.Base>(Source.Modifier.Parent)]
		public static void UpdateControl(ISystem.Info info, Entity entity,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Crane.Data crane, [Source.Owned] ref Control.Data control, [Source.Owned] ref Interactable.Data interactable)
		{
			if (interactable.ref_interactor.TryGetHandle(out var handle))
			{
				control = handle.Value.control;
			}
		}

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity, [Source.Owned] in Control.Data control,
		[Source.Owned] in Transform.Data transform, [Source.Parent] in Transform.Data transform_parent,
		[Source.Parent] ref Body.Data body_parent, [Source.Owned] ref Body.Data body,
		[Source.Parent] ref Joint.Base joint_base, [Source.Owned, Override] ref Joint.Gear gear, [Source.Parent, Override] ref Joint.Gear gear_parent, [Source.Owned] ref Crane.Data crane, [Source.Owned] ref Crane.State crane_state)
		{
			if (!joint_base.flags.HasAll(Joint.Flags.No_Aiming))
			{
				var dirty = control.mouse.GetKeyDown(Mouse.Key.Right) || control.mouse.GetKeyUp(Mouse.Key.Right);
				var invert = float.IsNegative(transform.scale.GetParity()); // != crane.flags.HasAny(Crane.Flags.Inverted);

				//if (crane.flags.HasAny(Crane.Flags.Inverted))
				//{
				//	invert = !invert;
				//}

				var transform_tmp = transform;
				var transform_parent_tmp = transform_parent;

				//if (invert)
				//{
				//	transform_tmp.scale.X *= -1.00f;
				//	transform_parent_tmp.scale.X *= -1.00f;
				//}

				if (control.mouse.GetKey(Mouse.Key.Right))
				{
					IK.Resolve2x(new Vector2(crane.length_a, crane.length_b), transform_tmp.LocalToWorld(joint_base.offset_b), control.mouse.position, new(crane_state.angle_a, crane_state.angle_b), out var angles, invert: invert != crane.flags.HasAny(Crane.Flags.Inverted));
					crane_state.angle_a = angles.X;
					crane_state.angle_b = angles.Y;

					if (info.WorldTime >= crane_state.next_sync)
					{
						crane_state.next_sync = info.WorldTime + 0.20f;
						dirty |= true;
					}
				}

				gear_parent.rotation = transform_parent_tmp.WorldToLocalRotationUnscaled(crane_state.angle_a);
				gear.rotation = crane_state.angle_b;

				if (invert)
				{
					gear_parent.rotation = -gear_parent.rotation;
					gear.rotation = Maths.OppositeAngle(gear.rotation);
				}

				if (dirty)
				{
					body.Activate();

#if SERVER
					crane.Sync(entity);
#endif
				}
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateSound(ISystem.Info info, Entity entity,
		[Source.Owned] in Crane.Data crane, [Source.Owned] ref Sound.Mixer sound_mix,
		[Source.Owned, Override] in Joint.Gear gear)
		{
			var max = 0.50f;
			sound_mix.modifier = Maths.Clamp(MathF.Abs(gear.GetCurrentSpeed()) - 0.10f, 0.00f, max) / max;
		}
	}
}
