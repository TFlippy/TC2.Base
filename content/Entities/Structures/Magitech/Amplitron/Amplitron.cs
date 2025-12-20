
namespace TC2.Base.Components
{
	public static partial class Amplitron
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

		}

		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			public float charge;

			public Amplitron.Flags flags;
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Amplitron.Data amplitron,
		[Source.Owned] in Control.Data control)
		{

		}

#if CLIENT
		public struct AmplitronGUI: IGUICommand
		{
			public Entity ent_amplitron;
			public Transform.Data transform;
			public Amplitron.Data amplitron;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Amplitron"u8, this.ent_amplitron))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
						{
							//GUI.TextShaded("Derpo"u8);
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Amplitron.Data amplitron)
		{
			if (interactable.IsActive())
			{
				var gui = new AmplitronGUI()
				{
					ent_amplitron = entity,
					transform = transform,
					amplitron = amplitron,
				};
				gui.Submit();
			}
		}
#endif
	}
}
