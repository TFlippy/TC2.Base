
namespace TC2.Base.Components
{
	public static partial class RandomActivations
	{

		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public float duration = 0.20f;
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public struct State: IComponent
		{
			[Save.Ignore, Net.Ignore] public float stops_at;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void PreUpdate(ISystem.Info info, Entity entity,
		[Source.Owned] ref Control.Data control, 
		[Source.Owned] in RandomActivations.Data randomActivations, [Source.Owned] ref RandomActivations.State state)
		{
			var random = XorRandom.New();
			if (random.NextFloatRange(0.00f, 1.00f) > 0.995f)
			{
				state.stops_at = info.WorldTime + randomActivations.duration;
				entity.SyncComponent(ref state);
			}

			if (info.WorldTime <= state.stops_at)
			{
				control.mouse.SetKeyPressed(Mouse.Key.Left, true);
				control.keyboard.SetKeyPressed(Keyboard.Key.Spacebar, true);
				entity.SyncComponent(ref control);
				
			}
		}
	}
}
