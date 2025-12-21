
namespace TC2.Base.Components
{
	public static partial class Essence
	{
		public static partial class Director
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,


			}

			[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Global | IComponent.Scope.Region)]
			public partial struct Data(): IComponent
			{
				[Save.Force, Editor.Picker.Position(relative: true)] public required Vec2f offset;
				[Save.Force, Editor.Picker.Direction(normalize: true)] public required Vec2f direction = Vec2f.Up;

				[Editor.Slider.Clamped(min: 0.00f, max: 16.00f, snap: 0.001f)]
				[Save.Force] public required float radius;
				[Save.Force] public required float capacity;
				[Save.Force] public float efficiency = 1.00f;

				[Save.Force] public Essence.Director.Flags flags;
				public ushort reserved_00;
				public uint reserved_01;
			}

			[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdate_A(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity entity,
			[Source.Owned] in Transform.Data transform, [Source.Owned] ref Essence.Director.Data director,
			[Source.Owned] in Control.Data control)
			{

			}

			[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdate_B(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity entity,
			[Source.Owned] ref Transform.Data transform,
			[Source.Owned] ref Essence.Director.Data director)
			{

			}

			[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdate_C(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity entity,
			[Source.Owned] ref Transform.Data transform,
			[Source.Owned] ref Essence.Director.Data director,
			[Source.Owned, Pair.Component<Essence.Director.Data>] ref Essence.Charge.Data charge)
			{

			}

#if CLIENT
			[ISystem.Render(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnRender_A(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity entity,
			[Source.Owned] in Transform.Data transform,
			[Source.Owned] ref Essence.Director.Data director,
			[Source.Owned, Pair.Component<Essence.Director.Data>] ref Essence.Charge.Data charge)
			{

			}
#endif

#if CLIENT
			public struct DirectorGUI: IGUICommand
			{
				public Entity ent_director;
				public Transform.Data transform;
				public Essence.Director.Data director;

				public void Draw()
				{
					using (var window = GUI.Window.Interaction("Director"u8, this.ent_director))
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
			[Source.Owned] in Essence.Director.Data director)
			{
				if (interactable.IsActive())
				{
					var gui = new DirectorGUI()
					{
						ent_director = entity,
						transform = transform,
						director = director,
					};
					gui.Submit();
				}
			}
#endif
		}
	}
}