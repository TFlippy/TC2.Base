namespace TC2.Base.Components
{
	public static partial class LumberMill
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<LumberMill.State>]
		public struct Data: IComponent
		{
			public float test = 1.00f;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public struct State: IComponent
		{
			[Net.Ignore, Save.Ignore] public float next_update;
		}

		public const float update_interval = 0.20f;

#if SERVER
		[ISystem.Update(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] in LumberMill.Data lumbermill, [Source.Owned] ref LumberMill.State lumbermill_state,
		[Source.Owned] ref Wheel.Data wheel)
		{
			if (info.WorldTime >= lumbermill_state.next_update)
			{
				lumbermill_state.next_update = info.WorldTime + update_interval;
			}
		}
#endif

#if CLIENT
		public struct LumberMillGUI: IGUICommand
		{
			public Entity ent_lumbermill;
			public LumberMill.Data lumbermill;
			public LumberMill.State lumbermill_state;

			public void Draw()
			{

			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUI(Entity entity, [Source.Owned] in LumberMill.Data lumbermill, [Source.Owned] in LumberMill.State lumbermill_state, [Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
			{
				var gui = new LumberMillGUI()
				{
					ent_lumbermill = entity,

					lumbermill = lumbermill,
					lumbermill_state = lumbermill_state,
				};
				gui.Submit();
			}
		}
#endif
	}
}
