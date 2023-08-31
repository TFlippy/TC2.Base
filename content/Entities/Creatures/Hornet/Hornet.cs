namespace TC2.Base.Components
{
	public static class Hornet
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public struct Data: IComponent
		{
			public int fps = 30;

			public Data()
			{

			}
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned), HasTag("hornet", true, Source.Modifier.Owned)]
		public static void UpdateAlive(ISystem.Info info, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] ref Hornet.Data hornet, [Source.Owned, Override] ref NoRotate.Data no_rotate)
		{
			var dir = (control.mouse.position - transform.position).GetNormalized(out var len);
			var rot = -dir.GetAngleRadians();
			rot += (transform.scale.X > 0.00f ? 0.00f : MathF.PI);
			rot = Maths.NormalizeAngle(rot % (MathF.PI * 2.00f));

			no_rotate.rotation = rot;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single), HasTag("dead", true, Source.Modifier.Owned), HasTag("hornet", true, Source.Modifier.Owned)]
		public static void UpdateDead(ISystem.Info info, [Source.Owned] ref Hornet.Data hornet, [Source.Owned, Override] ref NoRotate.Data no_rotate)
		{
			no_rotate.rotation = MathF.PI;
		}

#if CLIENT
		[ISystem.Update(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned), HasTag("hornet", true, Source.Modifier.Owned)]
		public static void OnUpdateAnimation(ISystem.Info info, ref XorRandom random, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Hornet.Data hornet, [Source.Owned] ref Flyer.Data flyer, [Source.Owned] ref Animated.Renderer.Data renderer)
		{
			renderer.sprite.fps = (byte)Math.Round(hornet.fps * flyer.lift_modifier);
			renderer.offset = Vector2.Lerp(renderer.offset, random.NextUnitVector2Range(0.00f, 0.25f), 0.10f);
			renderer.rotation = Maths.Lerp(renderer.rotation, random.NextFloatRange(-0.20f, 0.20f), 0.10f);
		}
#endif
	}
}
