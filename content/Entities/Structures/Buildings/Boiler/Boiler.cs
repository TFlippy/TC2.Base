namespace TC2.Base.Components
{
	public static partial class Boiler
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Boiler.State>]
		public struct Data: IComponent
		{
			public float capacity;
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public struct State: IComponent
		{
			public float pressure;
		}

#if CLIENT
		public struct BoilerGUI: IGUICommand
		{
			public Entity ent_boiler;
			public Boiler.Data boiler;
			public Boiler.State boiler_state;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Boiler", this.ent_boiler))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{

					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUI(Entity entity, [Source.Owned] ref Boiler.Data boiler, [Source.Owned] ref Boiler.State boiler_state, [Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
			{
				var gui = new BoilerGUI()
				{
					ent_boiler = entity,
					boiler = boiler,
					boiler_state = boiler_state
				};
				gui.Submit();
			}
		}
#endif
	}
}
