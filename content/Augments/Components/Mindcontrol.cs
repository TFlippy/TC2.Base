
namespace TC2.Base.Components
{
	public static partial class Mindcontrol
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Active = 1 << 0
		}

		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public Mindcontrol.Flags flags;
		}


		[ISystem.EarlyUpdate(ISystem.Mode.Single), Exclude<Toolbelt.Data>(Source.Modifier.Parent)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] ref Control.Data control, [Source.Owned] ref Mindcontrol.Data mindcontrol)
		{
			if (!mindcontrol.flags.HasFlag(Flags.Active))
			{
				Entity parent = entity.GetParent(Relation.Type.Child);
				if (!parent.IsNull() && !(parent == 0))
				{
					//App.WriteLine(parent+ "A");
					Entity next = parent.GetParent(Relation.Type.Instance);
					bool stop = false;
					float counter = 0;
					while (!next.IsNull() && !(next == 0) && !stop)
					{
						parent = next;
						//App.WriteLine(parent + "B" + counter);
						counter++;
						next = parent.GetParent(Relation.Type.Instance);

						ref var character = ref parent.GetComponent<Character.Data>();
						if (!character.IsNull()) 
						{
							stop = true;
#if SERVER
							//App.WriteLine(parent + "C");
							entity.SetAssociatedCharacter(parent);
							mindcontrol.flags |= Flags.Active;
							entity.SyncComponent(ref mindcontrol);
#endif
						}
					}
				}
			}
		}
	}
}
