
namespace TC2.Base.Components
{
	public static partial class ToggleActivations
	{

		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public ToggleActivations.Flags flags;
		}

		[Flags]
		public enum Flags: uint
		{
			Inactive = 0,

			Active = 1 << 0,

			BlockMouse= 2 << 0
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void PreUpdate(ISystem.Info info, Entity entity,
		[Source.Owned] ref Control.Data control, 
		[Source.Owned] ref ToggleActivations.Data toggle)
		{
			if (control.mouse.GetKey(Mouse.Key.Left))
			{
				if (!toggle.flags.HasAll(ToggleActivations.Flags.BlockMouse))
				if (toggle.flags.HasAll(ToggleActivations.Flags.Active))
				{
					toggle.flags &= ~ToggleActivations.Flags.Active;
				}
				else
				{
					toggle.flags |= ToggleActivations.Flags.Active;
				}
				toggle.flags |= ToggleActivations.Flags.BlockMouse;
			}
			else
			{
				if (toggle.flags.HasAll(ToggleActivations.Flags.BlockMouse)) //Only toggle with every click AND release
				{
					toggle.flags &= ~ToggleActivations.Flags.BlockMouse;
				}
			}

			if (toggle.flags.HasAll(ToggleActivations.Flags.Active))
			{
				control.mouse.SetKeyPressed(Mouse.Key.Left, true);
				control.keyboard.SetKeyPressed(Keyboard.Key.Spacebar, true);
			}
		}
	}
}
