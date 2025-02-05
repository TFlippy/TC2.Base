namespace TC2.Base.Components
{
	public static class Hornet
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			public float fps = 30.00f;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned), HasTag("hornet", true, Source.Modifier.Owned)]
		public static void UpdateAlive(ISystem.Info info, 
		[Source.Owned] ref Hornet.Data hornet, [Source.Owned, Override] ref NoRotate.Data no_rotate,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control)
		{
			var rot = (control.mouse.position - transform.position).GetAngleRadiansFast(); //.GetNormalizedFast();
			//var rot = -dir.GetAngleRadiansFast();
			rot += (transform.scale.X > 0.00f ? 0.00f : float.Pi);
			//rot = Maths.NormalizeAngle(rot % float.Tau);
			rot = Maths.NormalizeAngle(rot); // % float.Tau);

			no_rotate.rotation = rot;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", true, Source.Modifier.Owned), HasTag("hornet", true, Source.Modifier.Owned)]
		public static void UpdateDead(ISystem.Info info, 
		[Source.Owned] ref Hornet.Data hornet, [Source.Owned, Override] ref NoRotate.Data no_rotate, 
		[Source.Owned] ref Animated.Renderer.Data renderer)
		{
			no_rotate.rotation = float.Pi;
			renderer.sprite.fps = 0.00f;
		}

#if CLIENT
		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned), HasTag("hornet", true, Source.Modifier.Owned)]
		public static void OnUpdateAnimation(ISystem.Info info, ref XorRandom random,
		[Source.Owned] ref Hornet.Data hornet, [Source.Owned] ref Flyer.Data flyer, [Source.Owned] ref Animated.Renderer.Data renderer)
		{
			renderer.sprite.fps = Maths.Round(hornet.fps * flyer.lift_modifier);
			renderer.offset = Maths.LerpFMA(renderer.offset, random.NextUnitVector2(0.25f), 0.10f);
			renderer.rotation = Maths.Lerp(renderer.rotation, random.NextFloat(0.40f), 0.10f);
		}
#endif
	}
}
