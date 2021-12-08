namespace TC2.Base.Components
{
	public static class Sludge
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data: IComponent
		{
			[Net.Ignore, Save.Ignore] public float next_smoke;
		}

		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

#if CLIENT
		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void OnUpdateAnimation(ISystem.Info info, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Sludge.Data sludge, [Source.Owned] in Resource.Data resource)
		{
			ref var material = ref resource.material.GetDefinition();
			var modifier = resource.quantity / material.quantity_max;

			if (info.WorldTime >= sludge.next_smoke)
			{
				var random = XorRandom.New();

				sludge.next_smoke = info.WorldTime + random.NextFloatRange(0.70f, 1.30f);

				Particle.Spawn(ref info.GetRegion(), new Particle.Data()
				{
					texture = texture_smoke,
					lifetime = random.NextFloatRange(2.00f, 6.00f),
					pos = transform.position + random.NextVector2(0.50f),
					vel = new Vector2(0.10f, -0.10f) * 1.50f,
					force = new Vector2(0.14f, -0.50f) * random.NextFloatRange(0.90f, 1.10f),
					fps = (byte)random.NextFloatRange(12, 14),
					frame_count = 64,
					frame_count_total = 64,
					frame_offset = (byte)random.NextFloatRange(0, 64),
					scale = random.NextFloatRange(0.40f, 0.50f),
					rotation = random.NextFloat(10.00f),
					angular_velocity = random.NextFloat(1.20f),
					growth = 0.20f,
					color_a = random.NextColor32Range(new Color32BGRA(130, 197, 217, 100), new Color32BGRA(160, 197, 217, 100)),
					color_b = new Color32BGRA(0, 240, 240, 50)
				});
			}
		}
#endif
	}
}
