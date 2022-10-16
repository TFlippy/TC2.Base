using System.Numerics;

namespace TC2.Base.Components
{
	public static class Radioactive
	{
		[IComponent.Data(Net.SendType.Unreliable)]
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
						entity.Hit(entity, result.entity, result.world_position, result.gradient, -result.gradient, 25.00f, result.material_type, Damage.Type.Radiation, speed: 0.50f);
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
		[IComponent.Data(Net.SendType.Unreliable)]
		public struct Data: IComponent
		{
			[Editor.Picker.Position(true, true)]
			public Vector2 offset;

			public float modifier = 1.00f;
			[Net.Ignore, Save.Ignore] public float next_smoke;

			public Data()
			{

			}
		}

		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[ISystem.Modified(ISystem.Mode.Single)]
		public static void OnModifiedResource(ISystem.Info info, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Mithril.Data mithril, [Source.Owned] in Resource.Data resource)
		{
			ref var material = ref resource.material.GetDefinition();
			mithril.modifier = 0.50f + (MathF.Log2(1.00f + (resource.quantity / MathF.Max(material.quantity_max, 1.00f))) * 0.50f);
		}

#if CLIENT
		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateFX(ISystem.Info info, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Mithril.Data mithril)
		{
			if (info.WorldTime >= mithril.next_smoke && mithril.modifier > 0.01f)
			{
				var random = XorRandom.New();

				ref var region = ref info.GetRegion();
				mithril.next_smoke = info.WorldTime + random.NextFloatRange(0.50f, 0.70f);

				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = texture_smoke,
					lifetime = random.NextFloatRange(2.00f, 4.00f) * mithril.modifier,
					pos = transform.LocalToWorld(mithril.offset),
					vel = new Vector2(0.10f, -0.10f) * 1.10f,
					force = new Vector2(0.14f, -0.50f) * random.NextFloatRange(0.90f, 1.10f),
					fps = random.NextByteRange(14, 16),
					frame_count = 64,
					frame_count_total = 64,
					frame_offset = random.NextByteRange(0, 64),
					scale = random.NextFloatRange(0.40f, 0.50f) * mithril.modifier,
					rotation = random.NextFloat(10.00f),
					angular_velocity = random.NextFloat(1.20f),
					growth = 0.20f,
					color_a = random.NextColor32Range(new Color32BGRA(100, 75, 215, 75), new Color32BGRA(150, 30, 180, 100)),
					color_b = new Color32BGRA(0, 2, 20, 5)
				});
			}
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateLight(ISystem.Info info, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Mithril.Data mithril, [Source.Owned, Pair.Of<Mithril.Data>] ref Light.Data light)
		{
			light.intensity = mithril.modifier;
			light.offset = mithril.offset;
		}
#endif
	}
}
