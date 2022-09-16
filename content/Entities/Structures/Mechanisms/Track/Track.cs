namespace TC2.Base.Components
{
	public static partial class Track
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Invert = 1 << 0
			}

			public float speed = 0.01f;
			public Track.Data.Flags flags;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct State: IComponent
		{
			public float slider_ratio = default;

			public State()
			{

			}
		}

#if CLIENT
		[ISystem.Render(ISystem.Mode.Single)]
		public static void UpdateSprite(ISystem.Info info, Entity entity,
		[Source.Shared] in Transform.Data transform,
		[Source.Shared] in Track.Data track, [Source.Shared] ref Track.State track_state,
		[Source.Shared] in Resizable.Data resizable, [Source.Owned, Original] ref Joint.Distance joint_distance,
		[Source.Shared, Pair.Of<Track.Data>] ref Animated.Renderer.Data renderer)
		{
			renderer.rotation = -(resizable.b - resizable.a).GetAngleRadians();
			renderer.offset = Vector2.Lerp(renderer.offset, joint_distance.GetDelta().RotateByRad(-transform.rotation), 0.40f);
		}
#endif

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateSlider(ISystem.Info info, Entity entity,
		[Source.Shared] in Track.Data track, [Source.Shared] ref Track.State track_state,
		[Source.Owned, Original] ref Joint.Distance joint_distance, [Source.Shared] in Resizable.Data resizable)
		{
			var ratio = track_state.slider_ratio.Clamp01();
			if (track.flags.HasAny(Track.Data.Flags.Invert))
			{
				ratio = 1.00f - ratio;
			}

			joint_distance.distance = Vector2.Distance(resizable.a, resizable.b) * ratio;
		}

		[ISystem.Update(ISystem.Mode.Single)]
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

		[ISystem.Modified<Resizable.Data>(ISystem.Mode.Single)]
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
			public void Invoke(ref NetConnection connection, Entity entity, ref Track.Data data)
			{
				if (this.slider_ratio.HasValue)
				{
					ref var state = ref entity.GetComponent<Track.State>();
					if (!state.IsNull())
					{
						state.slider_ratio = Maths.Clamp01(this.slider_ratio.Value);
						state.Sync(entity);
					}
				}

				if (this.flags.HasValue)
				{
					data.flags = this.flags.Value;
				}

				data.Sync(entity);
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
				using (var window = GUI.Window.Interaction("Track", this.ent_track))
				{
					this.StoreCurrentWindowTypeID(order: -100);

					if (window.show)
					{
						ref var player = ref Client.GetPlayer();
						ref var region = ref Client.GetRegion();

						var frame_size = Inventory.GetFrameSize(4, 2);

						using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight())))
						{
							using (var group = GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth() - frame_size.X - 32, GUI.GetRemainingHeight()), padding: new Vector2(8, 8)))
							{
								GUI.DrawBackground(GUI.tex_frame, group.GetOuterRect(), new(8));

								var dirty = false;
								
								if (GUI.SliderFloat("Slider", ref this.track_state.slider_ratio, 0.00f, 1.00f, "%.2f", size: new Vector2(GUI.GetRemainingWidth(), 32)))
								{
									dirty = true;
								}

								if (GUI.Checkbox("Invert", ref this.track.flags, Track.Data.Flags.Invert, size: new Vector2(GUI.GetRemainingWidth() * 0.50f, 32)))
								{
									dirty = true;
								}

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

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUI(Entity entity, [Source.Owned] in Track.Data track, [Source.Owned] in Track.State track_state, [Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
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
