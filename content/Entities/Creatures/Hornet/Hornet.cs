namespace TC2.Base.Components
{
	public static class Hornet
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data: IComponent
		{
			public int fps = 30;

			public Data()
			{

			}
		}

		// Hack, used to register "hornet" tag for now
		[ISystem.EarlyUpdate(ISystem.Mode.Single), HasTag("hornet", true, Source.Modifier.Owned)]
		public static void UpdateHornet(ISystem.Info info, [Source.Owned] ref Hornet.Data hornet)
		{

		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateAlive(ISystem.Info info, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] ref Hornet.Data hornet, [Source.Owned, Override] ref NoRotate.Data no_rotate)
		{
			var dir = (control.mouse.position - transform.position).GetNormalized(out var len);
			var rot = -dir.GetAngleRadians();
			rot += (transform.scale.X > 0.00f ? 0.00f : MathF.PI);
			rot = Maths.NormalizeAngle(rot % (MathF.PI * 2.00f));

			no_rotate.rotation = rot;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single), HasTag("dead", true, Source.Modifier.Owned)]
		public static void UpdateDead(ISystem.Info info, [Source.Owned] ref Hornet.Data hornet, [Source.Owned, Override] ref NoRotate.Data no_rotate)
		{
			no_rotate.rotation = MathF.PI;
		}

#if CLIENT
		[ISystem.Update(ISystem.Mode.Single)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void OnUpdateAnimation(ISystem.Info info, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Hornet.Data hornet, [Source.Owned] ref Flyer.Data flyer, [Source.Owned] ref Animated.Renderer.Data renderer)
		{
			renderer.sprite.fps = (byte)Math.Round(hornet.fps * flyer.lift_modifier);
		}
#endif
	}
}
