
namespace TC2.Base.Components
{
	public static partial class Firework
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Armed = 1 << 0,
			Faction = 1 << 1
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			public Firework.Flags flags;
			public uint unused_00;
			public float unused_01;
			public float unused_02;

			[Save.NewLine]
			public FixedArray4<Color32BGRA> colors;
		}

#if CLIENT
		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSprite(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] ref Firework.Data firework,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Transform.Data transform)
		{

		}
#endif

#if SERVER
		[ISystem.Event<Interactable.InteractEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnInteract(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random, [Source.Owned] ref Interactable.InteractEvent data,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Interactable.Data interactable, [Source.Owned] ref Firework.Data firework)
		{
			//Sound.Play(ref region, firework.sound_arm, transform.position, volume: 0.60f, pitch: 1.00f, priority: 0.60f, size: 0.90f);
			//firework.flags.SetFlag(Firework.Flags.Armed, !firework.flags.HasAny(Firework.Flags.Armed));

			////App.WriteLine("arm");

			//firework.Sync(entity);
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] ref Firework.Data firework, [Source.Owned] ref Explosive.Data explosive,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Transform.Data transform)
		{
			
		}
#endif
	}
}
