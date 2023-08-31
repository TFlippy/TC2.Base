
namespace TC2.Base.Components
{
	public static partial class Tool
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{
	
		}

#if SERVER
		[ISystem.Event<EssenceNode.FailureEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnFailure(ISystem.Info info, Entity entity, Entity ent_attachment_slot, ref XorRandom random, ref EssenceNode.FailureEvent data, [Source.Owned] ref Control.Data control)
		{
			control.mouse.SetKeyPressed(Mouse.Key.Left, random.NextBool(0.50f));
			control.mouse.SetKeyPressed(Mouse.Key.Right, random.NextBool(0.50f));

			control.keyboard.SetKeyPressed(Keyboard.Key.Spacebar, random.NextBool(0.50f));
			control.keyboard.SetKeyPressed(Keyboard.Key.Reload, random.NextBool(0.50f));
			control.keyboard.SetKeyPressed(Keyboard.Key.C, random.NextBool(0.50f));
			control.keyboard.SetKeyPressed(Keyboard.Key.MoveLeft, random.NextBool(0.50f));
			control.keyboard.SetKeyPressed(Keyboard.Key.MoveRight, random.NextBool(0.50f));
			control.keyboard.SetKeyPressed(Keyboard.Key.MoveDown, random.NextBool(0.50f));
			control.keyboard.SetKeyPressed(Keyboard.Key.MoveUp, random.NextBool(0.50f));

			control.Sync(entity);
		}
#endif
	}
}
