﻿namespace TC2.Base.Components
{
	public static class Telescope
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public struct Data(): IComponent
		{
			[Statistics.Info("Adjustment Speed", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Low)]
			public float speed = 2.00f;

			public float deadzone = 5.00f;

			[Statistics.Info("Zoom", description: "TODO: Desc", format: "{0:0.##}x", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			public float zoom_modifier = 0.50f;

			[Statistics.Info("Stability", description: "TODO: Desc", format: "{0:0.##}x", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Low)]
			public float shake_modifier = 1.00f;

			[Statistics.Info("Zoom (Min)", description: "TODO: Desc", format: "{0:0.##}x", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Medium)]
			public float zoom_min = 0.50f;

			[Statistics.Info("Zoom (Max)", description: "TODO: Desc", format: "{0:0.##}x", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Medium)]
			public float zoom_max = 1.00f;

			[Statistics.Info("Distance (Min)", description: "TODO: Desc", format: "{0:0} meters", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float min_distance = 12.00f;

			[Statistics.Info("Distance (Max)", description: "TODO: Desc", format: "{0:0} meters", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float max_distance = 80.00f;

			[Save.Ignore, Net.Ignore] public float current_modifier;
			public Vector2 offset;
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

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent)]
		public static void OnGUI(ISystem.Info info, Entity entity,
		[Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Telescope.Data telescope, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control,
		[Source.Parent] in Player.Data player)
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
#endif

		//#if CLIENT
		//		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region)]
		//		public static void OnUpdate(ISystem.Info info, Entity entity,
		//		[Source.Owned] ref Telescope.Data telescope, [Source.Owned] in Control.Data control)
		//		{
		//			//App.WriteLine("test");

		//			//if (control.keyboard.GetKey(Keyboard.Key.LeftShift) && control.keyboard.GetKeyDown(Keyboard.Key.Reload))
		//			if (control.keyboard.GetKey(Keyboard.Key.Reload))
		//			{
		//				telescope.offset = Vector2.Lerp(telescope.offset, Vector2.Zero, 0.10f);
		//				//telescope.Sync(entity, true);
		//			}
		//		}
		//#endif

#if CLIENT
		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent)]
		public static void OnUpdate(ISystem.Info info, Entity entity,
		[Source.Owned] ref Telescope.Data telescope, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Parent] in Interactor.Data interactor,
		[Source.Parent] in Player.Data player, [Source.Singleton] ref Camera.Singleton camera)
		{
			var pos = transform.position + telescope.offset;
			var dir = (control.mouse.position - pos).GetNormalized(out var len);
			var dist = (pos - transform.position).Length();
			var dist_clipped = Maths.Max(dist - telescope.deadzone, 0.00f);

			camera.override_target_position = pos + (new Vector2(Maths.Perlin(0.00f, info.WorldTime, 1.00f) - 0.50f, -Maths.Perlin(info.WorldTime, 0.00f, 1.00f) - 0.50f) * (dist / 100.00f) * telescope.shake_modifier * 0.40f);

			Maths.MinMax(telescope.min_distance, telescope.max_distance, out var min_distance, out var max_distance);
			var t0 = Maths.Normalize01(dist_clipped, min_distance);
			var t1 = Maths.Clamp01(dist_clipped / (max_distance - min_distance));

			camera.distance_modifier = Maths.Lerp(camera.distance_modifier, 0.00f, t0);
			camera.zoom_min = Maths.Lerp(camera.zoom_min, telescope.zoom_min, t1);
			camera.zoom_max = Maths.Lerp(camera.zoom_max, telescope.zoom_max, t1);

			len = Maths.Max(len - telescope.deadzone, 0.00f);

			if (len > telescope.deadzone && control.mouse.GetKey(Mouse.Key.Right))
			{
				telescope.offset += dir * (telescope.speed * len * App.fixed_update_interval_s);
			}
			else if (control.keyboard.GetKey(Keyboard.Key.Reload))
			{
				telescope.offset = Vector2.Lerp(telescope.offset, Vector2.Zero, 0.02f);
				//telescope.Sync(entity, true);
			}

			telescope.offset += (new Vector2(Maths.Perlin(info.WorldTime, 0.00f, 1.00f) - 0.50f, Maths.Perlin(0.00f, info.WorldTime, 1.00f) - 0.50f) * (dist / 100.00f) * telescope.shake_modifier) * 0.07f;
			telescope.offset = telescope.offset.ClampRadius(Vector2.Zero, max_distance);
		}

		[ISystem.Remove(ISystem.Mode.Single, ISystem.Scope.Region)]
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
