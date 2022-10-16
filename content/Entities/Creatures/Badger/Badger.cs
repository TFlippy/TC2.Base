namespace TC2.Base.Components
{
	public static partial class Badger
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public float test;
		}

		// Hack, used to register "badger" tag for now
		[ISystem.EarlyUpdate(ISystem.Mode.Single), HasTag("badger", true, Source.Modifier.Owned)]
		public static void UpdateBadger(ISystem.Info info, [Source.Owned] ref Badger.Data badger)
		{

		}
	}
}
