namespace TC2.Base.Components
{
	public static partial class Skyhook
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			public float speed = 0.50f;
			public float length_max = 32.00f;
			[Asset.Ignore] public float length_current;
			[Asset.Ignore] public float length_target;
		}

		public struct DEV_EditRPC: Net.IRPC<Skyhook.Data>
		{
			public float? edit_length_target;
#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Skyhook.Data data)
			{
				var sync = false;
				sync |= this.edit_length_target.TryCopyToClamped(ref data.length_target, min: 1.00f, max: data.length_max);

				if (sync)
				{
					rpc.Sync(ref data);
				}
			}
#endif
		}

#if CLIENT
		public partial struct SkyhookGUI: IGUICommand
		{
			public Entity ent_skyhook;
			public Skyhook.Data skyhook;
			public Transform.Data transform;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Skyhook"u8, this.ent_skyhook))
				{
					this.StoreCurrentWindowTypeID(order: 7);
					if (window.show)
					{

					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI([Source.Owned] in Interactable.Data interactable,
		Entity ent_skyhook, [Source.Owned] in Skyhook.Data skyhook, [Source.Owned] in Transform.Data transform)
		{
			if (interactable.IsActive())
			{
				var gui = new SkyhookGUI()
				{
					ent_skyhook = ent_skyhook,
					skyhook = skyhook,
					transform = transform,
				};
				gui.Submit();
			}
		}
#endif

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Skyhook.Data skyhook, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body)
		{

		}

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Effects(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Skyhook.Data skyhook, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned, Pair.Component<Skyhook.Data>, Optional(true)] ref Sound.Emitter sound_emitter)
		{
			if (sound_emitter.IsNotNull())
			{
				//sound_emitter.
			}
		}

		[ISystem.Update.F(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Joint(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Skyhook.Data skyhook, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Parent, Original] ref Joint.Rope joint_rope, [Source.Parent] ref Joint.Base joint_base)
		{
			//joint_rope.off

			Maths.MoveTowards(ref skyhook.length_current, skyhook.length_target, skyhook.speed * App.fixed_update_interval_s, out var delta);
			joint_rope.distance = skyhook.length_current;

//			var vel_add = body.Down * skyhook.speed * App.fixed_update_interval_s * 100 * delta;
//			body.AddForce(vel_add * body.GetMass());

//#if SERVER
//			region.DrawDebugDir(body.GetPosition(), vel_add * 10, color: Color32BGRA.Red);
//#endif
		}
	}
}
