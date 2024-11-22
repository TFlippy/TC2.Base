﻿namespace TC2.Base.Components
{
	public static partial class Track
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true), IComponent.With<Track.State>]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Invert = 1 << 0
			}

			public float speed = 0.01f;
			public Track.Data.Flags flags;
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State(): IComponent
		{
			public float slider_ratio;
		}

#if CLIENT
		[ISystem.Render(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSprite(ISystem.Info info, Entity entity,
		[Source.Shared] in Transform.Data transform,
		[Source.Shared] in Track.Data track, [Source.Shared] ref Track.State track_state,
		[Source.Shared] in Resizable.Data resizable, [Source.Owned, Original] ref Joint.Distance joint_distance,
		[Source.Shared, Pair.Component<Track.Data>] ref Animated.Renderer.Data renderer)
		{
			renderer.rotation = -(resizable.b - resizable.a).GetAngleRadiansFast();
			//renderer.offset = Vector2.Lerp(renderer.offset, joint_distance.GetDelta().RotateByRad(-transform.GetInterpolatedRotation()), 0.40f);
			//renderer.offset = Vector2.Lerp(renderer.offset, joint_distance.GetDelta().RotateByRad(-transform.GetInterpolatedRotation()), 0.50f);
			renderer.offset = Vector2.Lerp(joint_distance.GetDelta(), (joint_distance.GetDelta() + (joint_distance.GetVelocity() * App.fixed_update_interval_s)), Vulkan.GetCurrentLerp()).RotateByRad(-transform.GetInterpolatedRotation());
		}
#endif

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSlider(ISystem.Info info, Entity entity,
		[Source.Shared] in Track.Data track, [Source.Shared] ref Track.State track_state,
		[Source.Owned, Original] ref Joint.Distance joint_distance, [Source.Shared] in Resizable.Data resizable)
		{
			var ratio = Maths.Clamp(track_state.slider_ratio, 0.0005f, 0.9995f); // TODO: hack to prevent max impulse when at 0.00f or 1.00f

			//var ratio = Maths.Clamp(track_state.slider_ratio, 0.000f, 1.000f);
			if (track.flags.HasAny(Track.Data.Flags.Invert))
			{
				ratio = 1.00f - ratio;
			}

			joint_distance.distance = Vector2.Distance(resizable.a, resizable.b) * ratio;
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateAxle(ISystem.Info info, Entity entity, [Source.Shared] in Control.Data control,
		[Source.Shared] in Track.Data track, [Source.Shared] ref Track.State track_state,
		[Source.Shared] ref Axle.Data axle, [Source.Shared] ref Axle.State axle_state,
		[Source.Owned] ref Joint.Base joint_base, [Source.Owned, Override] ref Joint.Distance joint_distance, [Source.Shared] in Resizable.Data resizable)
		{
			//axle_state.new_tmp_load += MathF.Abs(joint_distance.GetImpulseRaw()) + joint_distance.GetMass();
			axle_state.ApplyTorque(MathF.Abs(joint_distance.GetImpulseRaw()) + joint_distance.GetMass(), -joint_distance.GetBias());
			joint_distance.step = MathF.Abs(axle_state.angular_velocity) * 0.50f;
			joint_distance.max_bias = MathF.Abs(axle_state.angular_velocity) * 0.50f;
			joint_distance.max_force = MathF.Abs(axle_state.angular_momentum * 2000.00f);

			//axle_state.new_tmp_impulse -= joint_distance.GetImpulse() * info.DeltaTime;

			//if (axle_state.old_tmp_torque <= axle.mass * 2.00f) joint_distance.max_force = 0.00f;
			//joint_distance.modifier = 1.00f - Maths.NormalizeClamp(axle_state.old_tmp_load, axle_state.old_tmp_torque, fallback: 1.00f);

			//var eps = Unsafe.BitCast<float, uint>(float.Epsilon);
			//var eps2 = Unsafe.BitCast<float, uint>(Maths.epsilon);
			//App.WriteLine($"{eps.ToFormattedBinary()} vs {eps2.ToFormattedBinary()}");
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateControls(ISystem.Info info, Entity entity, [Source.Owned] in Control.Data control,
		[Source.Owned] in Track.Data track, [Source.Owned] ref Track.State track_state)
		{
			var vel = 0.00f;

			if (control.keyboard.GetKey(Keyboard.Key.MoveLeft)) vel -= 1.00f;
			if (control.keyboard.GetKey(Keyboard.Key.MoveRight)) vel += 1.00f;

			vel *= track.speed;

			track_state.slider_ratio = Maths.Clamp01(track_state.slider_ratio + vel);

#if SERVER
			if (vel != 0.00f)
			{
				track_state.Sync(entity);
			}
#endif
		}

		[ISystem.Modified.Component<Resizable.Data>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnModified(ISystem.Info info, Entity entity, [Source.Shared] in Transform.Data transform, [Source.Shared] ref Resizable.Data resizable,
		[Source.Owned] ref Joint.Base joint_base, [Source.Owned, Original] ref Joint.Slider joint_slider)
		{
			joint_slider.a = resizable.a;
			joint_slider.b = resizable.b;
			joint_slider.min = 0.00f;
			joint_slider.max = Vector2.Distance(resizable.a, resizable.b);
		}

		public struct ConfigureRPC: Net.IRPC<Track.Data>
		{
			public float? slider_ratio;
			public Track.Data.Flags? flags;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Track.Data data)
			{
				if (this.slider_ratio.HasValue)
				{
					ref var state = ref rpc.entity.GetComponent<Track.State>();
					if (!state.IsNull())
					{
						state.slider_ratio = Maths.Clamp01(this.slider_ratio.Value);
						state.Sync(rpc.entity);
					}
				}

				if (this.flags.HasValue)
				{
					data.flags = this.flags.Value;
				}

				data.Sync(rpc.entity);

				ref var body = ref rpc.entity.GetComponent<Body.Data>();
				if (body.IsNotNull())
				{
					body.Activate();

					//if (body.type == Body.Type.Static)
					//{
					//	body.
					//}

				}
			}
#endif
		}

#if CLIENT
		public struct TrackGUI: IGUICommand
		{
			public Entity ent_track;

			public Track.Data track;
			public Track.State track_state;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Track"u8, this.ent_track, no_mouse_close: true))
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
								
								if (GUI.SliderFloat("Ratio", ref this.track_state.slider_ratio, 0.00f, 1.00f, size: new Vector2(GUI.RmX, 32)))
								{
									dirty = true;
								}

								if (GUI.Checkbox("Reverse Direction", ref this.track.flags, Track.Data.Flags.Invert, size: new Vector2(GUI.RmX, 32)))
								{
									dirty = true;
								}
								GUI.DrawHoverTooltip("Reverse direction of the track.");

								if (dirty)
								{
									var rpc = new Track.ConfigureRPC
									{
										slider_ratio = this.track_state.slider_ratio,
										flags = this.track.flags
									};
									rpc.Send(this.ent_track);
								}
							}
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity, [Source.Owned] in Track.Data track, [Source.Owned] in Track.State track_state, [Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.IsActive())
			{
				var gui = new TrackGUI()
				{
					ent_track = entity,

					track = track,
					track_state = track_state,
				};
				gui.Submit();
			}
		}
#endif
	}
}
