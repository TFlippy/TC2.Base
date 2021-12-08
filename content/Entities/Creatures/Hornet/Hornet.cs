namespace TC2.Base.Components
{
	public static class Hornet
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data: IComponent
		{
			public float aim_rotation_ratio = 1.00f;
			public int fps = 30;
		}

		[ISystem.Update(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateAlive(ISystem.Info info, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] ref Hornet.Data hornet, [Source.Owned] ref NoRotate.Data no_rotate)
		{
			var dir = (control.mouse.position - transform.position).GetNormalized(out var len);
			var rot = -dir.GetAngleRadians();
			rot += (transform.scale.X > 0.00f ? 0.00f : MathF.PI);
			rot = Maths.NormalizeAngle(rot % (MathF.PI * 2.00f));

			no_rotate.rotation = rot;
		}

		[ISystem.Update(ISystem.Mode.Single), HasTag("dead", true, Source.Modifier.Owned)]
		public static void UpdateDead(ISystem.Info info, [Source.Owned] ref Hornet.Data hornet, [Source.Owned] ref NoRotate.Data no_rotate)
		{
			no_rotate.rotation = MathF.PI;
		}

#if CLIENT
		[ISystem.Update(ISystem.Mode.Single)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void OnUpdateAnimation(ISystem.Info info, [Source.Owned] in Organic.Data organic, [Source.Owned] ref Hornet.Data hornet, [Source.Owned] ref Flyer.Data flyer, [Source.Owned] ref Sprite.Renderer.Data renderer)
		{
			renderer.fps = (byte)Math.Round(hornet.fps * flyer.lift_modifier);
		}
#endif
	}
}
