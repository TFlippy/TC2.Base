namespace TC2.Base.Components
{
	public static class Radioactive
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data: IComponent
		{
			[Net.Ignore, Save.Ignore] public float next_update;
		}

#if SERVER
		[ISystem.Update(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Radioactive.Data radioactive)
		{
			if (info.WorldTime >= radioactive.next_update)
			{
				ref var region = ref info.GetRegion();
				var random = XorRandom.New();

				//var ts = Timestamp.Now();
				Span<OverlapResult> results = stackalloc OverlapResult[8];
				if (region.TryOverlapPointAll(transform.position, 4.00f, ref results, mask: Physics.Layer.Destructible))
				{
					foreach (ref var result in results)
					{
						entity.Hit(entity, result.entity, result.world_position, result.gradient, -result.gradient, 25.00f, result.material_type, Damage.Type.Radiation);
					}
				}
				//App.WriteLine($"{ts.GetMilliseconds():0.0000} ms");

				radioactive.next_update = info.WorldTime + random.NextFloatRange(0.10f, 0.20f);
			}
		}
#endif
	}

	public static class Mithril
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data: IComponent
		{
			[Net.Ignore, Save.Ignore] public float next_smoke;
		}

		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

#if CLIENT
		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateFX(ISystem.Info info, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Mithril.Data mithril, [Source.Owned] in Resource.Data resource)
		{
			if (info.WorldTime >= mithril.next_smoke)
			{
				var random = XorRandom.New();

				ref var region = ref info.GetRegion();
				ref var material = ref resource.material.GetDefinition();

				var modifier = MathF.Log2(1.00f + (resource.quantity / MathF.Max(material.quantity_max, 1.00f)));
				var modifier2 = 0.50f + (modifier * 0.50f);

				mithril.next_smoke = info.WorldTime + random.NextFloatRange(0.50f, 0.70f);

				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = texture_smoke,
					lifetime = random.NextFloatRange(2.00f, 4.00f) * modifier2,
					pos = transform.position,
					vel = new Vector2(0.10f, -0.10f) * 1.10f,
					force = new Vector2(0.14f, -0.50f) * random.NextFloatRange(0.90f, 1.10f),
					fps = (byte)random.NextFloatRange(14, 16),
					frame_count = 64,
					frame_count_total = 64,
					frame_offset = (byte)random.NextFloatRange(0, 64),
					scale = random.NextFloatRange(0.40f, 0.50f) * modifier2,
					rotation = random.NextFloat(10.00f),
					angular_velocity = random.NextFloat(1.20f),
					growth = 0.20f,
					color_a = random.NextColor32Range(new Color32BGRA(100, 75, 215, 75), new Color32BGRA(150, 30, 180, 100)),
					color_b = new Color32BGRA(0, 2, 20, 5)
				});
			}
		}
#endif
	}
}
