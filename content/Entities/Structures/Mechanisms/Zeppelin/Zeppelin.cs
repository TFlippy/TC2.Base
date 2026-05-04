namespace TC2.Base.Components
{
	//public static partial class Airdrop
	//{
	//	[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region | IComponent.Scope.Global)]
	//	public partial struct Data(): IComponent
	//	{
	//		[Editor.Picker.Position(relative: false)] public Vec2f pos_target;
	//	}
	//}

	public static partial class Zeppelin
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0u,

			[Asset.Ignore] Sync_Pending = 1u << 31
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region | IComponent.Scope.Global)]
		public partial struct Data(): IComponent
		{
			public float unused_00;
			public float unused_01;
			public float unused_02;

			public Zeppelin.Flags flags;

			public Vec2f speed_step = new(1.00f, 0.25f);
			public Vec2f speed_max = new(10, 2);

			//[Asset.Ignore] public Vec2f vel_current;
			[Asset.Ignore] public Vec2f vel_target;
		}

#if CLIENT
		public partial struct ZeppelinGUI: IGUICommand
		{
			public Entity ent_zeppelin;
			public Zeppelin.Data zeppelin;
			public Transform.Data transform;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Zeppelin"u8, this.ent_zeppelin))
				{
					this.StoreCurrentWindowTypeID(order: 6);
					if (window.show)
					{

					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI([Source.Owned] in Interactable.Data interactable,
		Entity ent_zeppelin, [Source.Owned] in Zeppelin.Data zeppelin, [Source.Owned] in Transform.Data transform)
		{
			if (interactable.IsActive())
			{
				var gui = new ZeppelinGUI()
				{
					ent_zeppelin = ent_zeppelin,
					zeppelin = zeppelin,
					transform = transform,
				};
				gui.Submit();
			}
		}
#endif

		// TODO: this could be entirely serverside actually, since the body's velocity gets automatically synced by the physics component anyway
		[Shitcode]
		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Zeppelin.Data zeppelin, [Source.Owned] ref Control.Data control,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body)
		{
			// TODO: optimize with SIMD instructions for adjacent add/sub operations
			var vel_x = control.keyboard.GetKeyAxis(Keyboard.Key.MoveRight, Keyboard.Key.MoveLeft, zeppelin.speed_step.x);
			var vel_y = control.keyboard.GetKeyAxis(Keyboard.Key.MoveDown, Keyboard.Key.MoveUp, zeppelin.speed_step.y);

			vel_x = Maths.FMA(vel_x, App.fixed_update_interval_s_f32, zeppelin.vel_target.x);
			vel_y = Maths.FMA(vel_y, App.fixed_update_interval_s_f32, zeppelin.vel_target.y);

			vel_x.ClampMagnitude(zeppelin.speed_max.x);
			vel_y.ClampMagnitude(zeppelin.speed_max.y);

			if (zeppelin.vel_target.TrySet(new(vel_x, vel_y)))
			{
#if SERVER
				zeppelin.flags.AddFlag(Zeppelin.Flags.Sync_Pending);
#endif
			}

			//var acc = new Vec2f(acc_x, acc_y);
			//if (acc.IsNotNil())
			//{
			//	// TODO: use FMA
			//	var vel_target_new = zeppelin.vel_target + (acc * App.fixed_update_interval_s_f32);

			//	// TODO: bleh
			//	var vel_target_new_clamped = new Vec2f(Maths.ClampMagnitude(vel_target_new.x, zeppelin.speed_max.x), Maths.ClampMagnitude(vel_target_new.y, zeppelin.speed_max.y));

			//}

			body.SetVelocity(zeppelin.vel_target);

#if SERVER
			if (info.Tickstamp.CheckInterval(Tickstamp.Interval.T008))
			{
				if (zeppelin.flags.TryRemoveFlag(Zeppelin.Flags.Sync_Pending))
				{
					zeppelin.Sync(entity);
				}
			}
#endif
		}
	}
}
