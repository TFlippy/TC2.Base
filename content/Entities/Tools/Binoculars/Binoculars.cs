namespace TC2.Base.Components
{
	public static class Binoculars
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data: IComponent
		{
			public float speed = 2.00f;
			public float deadzone = 5.00f;
			public float zoom_modifier = 0.01f;
			public float max_distance = 50.00f;

			[Save.Ignore, Net.Ignore] public Vector2 offset;
		}

#if CLIENT
		public struct BinocularsGUI: IGUICommand
		{
			public Transform.Data transform;
			public Control.Data control;
			public Binoculars.Data binoculars;
			public Entity entity;

			public void Draw()
			{
				ref var region = ref Client.GetRegion();
			}
		}

		[ISystem.GUI(ISystem.Mode.Single)]
		public static void OnGUI(ISystem.Info info, Entity entity,
		[Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Binoculars.Data binoculars, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control,
		[Source.Parent] in Player.Data player)
		{
			if (player.IsLocal())
			{
				var gui = new BinocularsGUI()
				{
					entity = entity,
					transform = transform,
					binoculars = binoculars,
					control = control
				};
				gui.Submit();
			}
		}
#endif

#if CLIENT
		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, Entity entity,
		[Source.Owned] ref Binoculars.Data binoculars, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Parent] in Interactor.Data interactor,
		[Source.Parent] in Player.Data player, [Source.Global] ref Camera.Global camera)
		{
			if (player.IsLocal())
			{
				var pos = transform.position + binoculars.offset;
				var dir = (control.mouse.position - pos).GetNormalized(out var len);
				var dist = (pos - transform.position).Length();

				len = MathF.Max(len - binoculars.deadzone, 0.00f);

				camera.override_target_position = pos;

				camera.distance_modifier = 0.00f;
				camera.zoom_modifier = 1.00f / Maths.Clamp(1.00f + MathF.Pow(dist * binoculars.zoom_modifier, 2), 1.00f, 2.00f);

				if (len > binoculars.deadzone * camera.zoom_modifier)
				{
					binoculars.offset += dir * (binoculars.speed * len * App.fixed_update_interval_s);
				}

				binoculars.offset = binoculars.offset.ClampRadius(Vector2.Zero, binoculars.max_distance);
			}
		}

		[ISystem.Remove2(ISystem.Mode.Single)]
		public static void OnRemove(ISystem.Info info, Entity entity,
		[Source.Owned] ref Binoculars.Data binoculars, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Parent] in Interactor.Data interactor,
		[Source.Parent] in Player.Data player)
		{
			binoculars.offset = default;
		}
#endif
	}
}
