namespace TC2.Base.Components
{
	public static class Telescope
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data: IComponent
		{
			[Statistics.Info("Adjustment Speed", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Low)]
			public float speed = 2.00f;

			public float deadzone = 5.00f;

			[Statistics.Info("Zoom Modifier", description: "TODO: Desc", format: "{0:0.##}x", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			public float zoom_modifier = 0.50f;

			[Statistics.Info("Zoom (Min)", description: "TODO: Desc", format: "{0:0.##}x", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Medium)]
			public float zoom_min = 0.10f;

			[Statistics.Info("Zoom (Max)", description: "TODO: Desc", format: "{0:0.##}x", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Medium)]
			public float zoom_max = 1.00f;

			[Statistics.Info("Maximum Distance", description: "TODO: Desc", format: "{0:0} meters", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float max_distance = 50.00f;

			[Save.Ignore, Net.Ignore] public float current_modifier = default;
			public Vector2 offset = default;

			public Data()
			{

			}
		}

#if CLIENT
		public struct TelescopeGUI: IGUICommand
		{
			public Transform.Data transform;
			public Control.Data control;
			public Telescope.Data telescope;
			public Entity entity;

			public void Draw()
			{
				ref var region = ref Client.GetRegion();
			}
		}

		[ISystem.GUI(ISystem.Mode.Single)]
		public static void OnGUI(ISystem.Info info, Entity entity,
		[Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Telescope.Data telescope, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control,
		[Source.Parent] in Player.Data player)
		{
			if (player.IsLocal())
			{
				var gui = new TelescopeGUI()
				{
					entity = entity,
					transform = transform,
					telescope = telescope,
					control = control
				};
				gui.Submit();
			}
		}
#endif

#if SERVER
		[ISystem.Update(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, Entity entity,
		[Source.Owned] ref Telescope.Data telescope, [Source.Owned] in Control.Data control)
		{
			//App.WriteLine("test");

			if (control.keyboard.GetKeyDown(Keyboard.Key.Reload))
			{
				telescope.offset = default;
				telescope.Sync(entity);
			}
		}
#endif

#if CLIENT
		[ISystem.Update(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, Entity entity,
		[Source.Owned] ref Telescope.Data telescope, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Parent] in Interactor.Data interactor,
		[Source.Parent] in Player.Data player, [Source.Global] ref Camera.Global camera)
		{
			if (player.IsLocal())
			{
				var pos = transform.position + telescope.offset;
				var dir = (control.mouse.position - pos).GetNormalized(out var len);
				var dist = (pos - transform.position).Length();

				var modifier = Maths.Lerp(telescope.zoom_min, telescope.zoom_max, MathF.Pow(Maths.Normalize(dist, telescope.max_distance), telescope.zoom_modifier));

				telescope.current_modifier = Maths.Lerp(telescope.current_modifier, modifier, 0.10f);

				camera.override_target_position = pos;
				camera.distance_modifier = 0.00f;
				camera.zoom_modifier *= telescope.current_modifier;

				var deadzone = telescope.deadzone * telescope.current_modifier;
				len = MathF.Max(len - deadzone, 0.00f);

				if (len > deadzone && control.mouse.GetKey(Mouse.Key.Right))
				{
					telescope.offset += dir * (telescope.speed * len * App.fixed_update_interval_s);
				}

				telescope.offset = telescope.offset.ClampRadius(Vector2.Zero, telescope.max_distance);
			}
		}

		[ISystem.Remove(ISystem.Mode.Single)]
		public static void OnRemove(ISystem.Info info, Entity entity,
		[Source.Owned] ref Telescope.Data telescope, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Parent] in Interactor.Data interactor,
		[Source.Parent] in Player.Data player)
		{
			telescope.current_modifier = 1.00f;
			//telescope.offset = default;
		}
#endif
	}
}
