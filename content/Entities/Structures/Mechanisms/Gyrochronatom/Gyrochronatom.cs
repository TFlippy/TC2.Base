
namespace TC2.Base.Components
{
	public static partial class Gyrochronatom
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

		}

		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			public Gyrochronatom.Flags flags;

		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Gyrochronatom.Data gyrochronatom,
		[Source.Owned] in Control.Data control)
		{

		}

#if CLIENT
		public struct GyrochronatomGUI: IGUICommand
		{
			public Entity ent_gyrochronatom;
			public Transform.Data transform;
			public Gyrochronatom.Data gyrochronatom;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Gyrochronatom"u8, this.ent_gyrochronatom))
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
		[Source.Owned] in Gyrochronatom.Data gyrochronatom)
		{
			if (interactable.IsActive())
			{
				var gui = new GyrochronatomGUI()
				{
					ent_gyrochronatom = entity,
					transform = transform,
					gyrochronatom = gyrochronatom,
				};
				gui.Submit();
			}
		}
#endif
	}
}
