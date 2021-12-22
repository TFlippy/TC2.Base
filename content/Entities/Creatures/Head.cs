
namespace TC2.Base.Components
{
	public static partial class Head
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateNoRotate(ISystem.Info info, [Source.Owned] in Organic.Data organic, [Source.Owned] in Organic.State organic_state, [Source.Owned] ref NoRotate.Data no_rotate, [Source.Owned] in Head.Data head)
		{
			no_rotate.multiplier = MathF.Round(organic_state.consciousness_shared) * organic_state.efficiency;
		}

		[ISystem.Remove(ISystem.Mode.Single)]
		public static void OnRemoveHead([Source.Parent] ref Organic.Data organic, [Source.Parent] ref Organic.State organic_state, [Source.Owned] in Head.Data head, [Source.Parent] in Joint.Base joint)
		{
			if (joint.flags.HasAll(Joint.Flags.Organic))
			{
				organic_state.consciousness_shared = 0.00f;
				organic_state.motorics_shared = 0.00f;
			}
		}

#if CLIENT
		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateHeadOffset(ISystem.Info info, [Source.Parent] in Torso.Data torso, [Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned] in Head.Data head)
		{
			renderer.offset = torso.head_offset;
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateHeadOffsetTrait(ISystem.Info info, [Source.Parent] in Torso.Data torso, [Source.Owned, Trait.Any] ref Animated.Renderer.Data renderer, [Source.Owned] in Head.Data head)
		{
			renderer.offset = torso.head_offset;
		}
#endif
	}
}