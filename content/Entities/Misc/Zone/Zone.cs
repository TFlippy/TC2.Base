using TC2.Base.Components;
using System.Runtime.InteropServices;

namespace TC2.Conquest
{
	public static partial class Zone
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0u,
			}

			public Zone.Data.Flags flags;
			public uint unused_00;
			public uint unused_01;
			public uint unused_02;

			[Editor.Picker.Box(mark_modified: true, snap: 0.25f)]
			public AABB rect;

		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info.Common info, ref Region.Data.Common region, Entity entity,
		[Source.Owned] ref Zone.Data zone, [Source.Owned] ref Transform.Data transform)
		{

		}

#if CLIENT
		public partial struct ZoneGUI: IGUICommand
		{
			public Entity ent_zone;
			public Zone.Data zone;
			public Transform.Data transform;

			public void Draw()
			{
			
			}
		}

		[ISystem.LateGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(ISystem.Info info, ref Region.Data region, Entity ent_zone,
		[Source.Owned] in Zone.Data zone, [Source.Owned] in Transform.Data transform)
		{
			var gui = new Zone.ZoneGUI()
			{
				ent_zone = ent_zone,
				zone = zone,
				transform = transform
			};
			gui.Submit();
		}
#endif
	}
}

